@echo off
REM Google Cloud Setup Checker for Jackson Ideas

echo ================================================
echo Google Cloud Setup Checker
echo ================================================
echo.

REM Check if Google Cloud SDK is installed
IF EXIST "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" (
    set "GCLOUD_PATH=C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin"
    echo [OK] Google Cloud SDK found at: C:\Program Files (x86)\Google\Cloud SDK
) ELSE IF EXIST "C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" (
    set "GCLOUD_PATH=C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin"
    echo [OK] Google Cloud SDK found at: C:\Program Files\Google\Cloud SDK
) ELSE (
    echo [ERROR] Google Cloud SDK not found!
    echo.
    echo Please install Google Cloud SDK:
    echo 1. Download from: https://cloud.google.com/sdk/docs/install
    echo 2. Run the installer
    echo 3. Follow the setup wizard
    echo 4. Run this script again after installation
    echo.
    pause
    exit /b 1
)

REM Add gcloud to PATH
set PATH=%GCLOUD_PATH%;%PATH%

REM Check gcloud version
echo.
echo Checking Google Cloud CLI version...
call gcloud --version
echo.

REM Check authentication
echo Checking authentication status...
call gcloud auth list
echo.

REM Check current project
echo Current project configuration:
call gcloud config get-value project
echo.

REM Check if Docker is installed
echo Checking Docker installation...
docker --version >nul 2>&1
if errorlevel 1 (
    echo [WARNING] Docker is not installed or not in PATH
    echo Cloud Build will handle containerization, but local testing requires Docker
) else (
    echo [OK] Docker is installed
    docker --version
)

echo.
echo ================================================
echo Setup Check Complete
echo ================================================
echo.
echo If everything looks good above, you can run:
echo   deploy-to-gcp.bat
echo.
echo If you need to authenticate first, run:
echo   gcloud auth login
echo.
echo To set the project, run:
echo   gcloud config set project ideas-matter-1749958193112
echo.
pause