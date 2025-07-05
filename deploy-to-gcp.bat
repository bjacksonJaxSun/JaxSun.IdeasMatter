@echo off
REM Google Cloud Deployment Script for Jackson Ideas Platform
REM This script sets up the environment and deploys to Google Cloud

echo ================================================
echo Jackson Ideas - Google Cloud Deployment
echo ================================================
echo.

REM Check if Google Cloud SDK is installed
IF EXIST "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" (
    set "GCLOUD_PATH=C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin"
) ELSE IF EXIST "C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" (
    set "GCLOUD_PATH=C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin"
) ELSE (
    echo ERROR: Google Cloud SDK not found!
    echo Please install it from: https://cloud.google.com/sdk/docs/install
    echo.
    echo After installation, run this script again.
    pause
    exit /b 1
)

REM Add gcloud to PATH for this session
set PATH=%GCLOUD_PATH%;%PATH%

REM Configuration
set PROJECT_ID=ideas-matter-1749958193112
set REGION=us-central1
set API_SERVICE_NAME=jackson-ideas-api
set WEB_SERVICE_NAME=jackson-ideas-web

echo Using Project ID: %PROJECT_ID%
echo Using Region: %REGION%
echo.

REM Check if user is authenticated
echo Checking authentication status...
call gcloud auth list --filter=status:ACTIVE --format="value(account)" > temp_auth.txt
set /p ACTIVE_ACCOUNT=<temp_auth.txt
del temp_auth.txt

if "%ACTIVE_ACCOUNT%"=="" (
    echo You are not authenticated with Google Cloud.
    echo Opening browser for authentication...
    call gcloud auth login
    if errorlevel 1 (
        echo Authentication failed!
        pause
        exit /b 1
    )
)

echo Authenticated as: %ACTIVE_ACCOUNT%
echo.

REM Set the project
echo Setting Google Cloud project...
call gcloud config set project %PROJECT_ID%
if errorlevel 1 (
    echo Failed to set project. Make sure you have access to project: %PROJECT_ID%
    pause
    exit /b 1
)

REM Enable required APIs
echo.
echo Enabling required Google Cloud APIs...
call gcloud services enable run.googleapis.com cloudbuild.googleapis.com secretmanager.googleapis.com containerregistry.googleapis.com
if errorlevel 1 (
    echo Failed to enable APIs. Check your permissions.
    pause
    exit /b 1
)

REM Check if JWT secret exists
echo.
echo Checking secrets...
call gcloud secrets describe jackson-ideas-jwt-secret >nul 2>&1
if errorlevel 1 (
    echo Creating JWT secret...
    powershell -Command "$jwt = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_}); $jwt | Out-File -FilePath jwt_secret.txt -NoNewline"
    call gcloud secrets create jackson-ideas-jwt-secret --data-file=jwt_secret.txt
    del jwt_secret.txt
    echo JWT secret created!
) else (
    echo JWT secret already exists
)

REM For simplicity, we'll use SQLite for this deployment
echo.
echo Setting up SQLite database connection...
echo Data Source=/app/jackson_ideas.db > db_connection.txt
call gcloud secrets create jackson-ideas-db-connection --data-file=db_connection.txt >nul 2>&1
if errorlevel 1 (
    call gcloud secrets versions add jackson-ideas-db-connection --data-file=db_connection.txt
)
del db_connection.txt

REM Build and deploy
echo.
echo ================================================
echo Starting Cloud Build deployment...
echo This will build and deploy your application.
echo ================================================
echo.

call gcloud builds submit --config cloudbuild.yaml .
if errorlevel 1 (
    echo.
    echo Cloud Build failed! Check the error messages above.
    echo.
    echo Common issues:
    echo - Make sure billing is enabled for your project
    echo - Check that all APIs are enabled
    echo - Verify you have the necessary permissions
    pause
    exit /b 1
)

REM Wait for deployment
echo.
echo Waiting for services to be ready...
timeout /t 30 /nobreak >nul

REM Get service URLs
echo.
echo Getting service URLs...
for /f "tokens=*" %%i in ('gcloud run services describe %API_SERVICE_NAME% --region=%REGION% --format="value(status.url)"') do set API_URL=%%i
for /f "tokens=*" %%i in ('gcloud run services describe %WEB_SERVICE_NAME% --region=%REGION% --format="value(status.url)"') do set WEB_URL=%%i

REM Update Web service with API URL
echo Updating Web service configuration...
call gcloud run services update %WEB_SERVICE_NAME% --region=%REGION% --update-env-vars="ApiSettings__BaseUrl=%API_URL%"

echo.
echo ================================================
echo Deployment Complete!
echo ================================================
echo API URL: %API_URL%
echo Web URL: %WEB_URL%
echo.
echo Your application is now live on Google Cloud!
echo.
echo Next steps:
echo 1. Visit %WEB_URL% to access your application
echo 2. Configure AI provider API keys (optional)
echo 3. Set up a custom domain (optional)
echo.
echo To add AI provider API keys, run:
echo gcloud run services update %API_SERVICE_NAME% --region=%REGION% --update-env-vars="DemoMode__ApiKeys__OpenAI=your-key"
echo ================================================
echo.
pause