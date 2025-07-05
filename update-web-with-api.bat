@echo off
echo ================================================
echo Update Web App with Render API URL
echo ================================================
echo.

set REGION=us-central1
set WEB_SERVICE_NAME=jackson-ideas-web

echo Please complete the Render deployment first!
echo.
echo 1. Go to https://dashboard.render.com
echo 2. Click "New +" then "Web Service"
echo 3. Connect your GitHub repository: bjackson071968/Jackson.Ideas
echo 4. Use these settings:
echo    - Name: jackson-ideas-api
echo    - Branch: master
echo    - Runtime: Docker
echo    - Dockerfile Path: ./Dockerfile.api.render
echo 5. Click "Create Web Service"
echo.
echo After deployment completes (5-10 minutes), you'll get a URL like:
echo https://jackson-ideas-api-xxxx.onrender.com
echo.
set /p API_URL="Enter your Render API URL (e.g., https://jackson-ideas-api-xxxx.onrender.com): "

if "%API_URL%"=="" (
    echo No URL provided. Exiting.
    pause
    exit /b 1
)

echo.
echo Updating Web application with new API URL...
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run services update %WEB_SERVICE_NAME% --region=%REGION% --update-env-vars='ApiSettings__BaseUrl=%API_URL%'"

echo.
echo ================================================
echo Deployment Complete!
echo ================================================
echo.
echo Your application is now fully deployed:
echo.
echo Web App: https://jackson-ideas-web-480585172218.us-central1.run.app
echo API: %API_URL%
echo.
echo Test your application:
echo 1. Visit the web app URL
echo 2. Register a new account
echo 3. Login and submit an idea
echo.
echo API Health Check: %API_URL%/health
echo API Documentation: %API_URL%/swagger
echo.
echo ================================================
pause