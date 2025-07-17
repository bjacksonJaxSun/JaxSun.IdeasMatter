@echo off
echo ================================================
echo Google Cloud Setup Checker
echo ================================================
echo.

REM Try to run gcloud directly first
gcloud --version >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Google Cloud CLI is available in PATH
    echo.
    gcloud --version
    goto :check_auth
)

REM Check standard installation paths
if exist "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" (
    echo [OK] Google Cloud SDK found at: C:\Program Files (x86)
    "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" --version
    set GCLOUD="C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    goto :check_auth
)

if exist "C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" (
    echo [OK] Google Cloud SDK found at: C:\Program Files
    "C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd" --version
    set GCLOUD="C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    goto :check_auth
)

echo [ERROR] Google Cloud SDK not found!
echo.
echo Please install from: https://cloud.google.com/sdk/docs/install
echo.
pause
exit /b 1

:check_auth
echo.
echo ================================================
echo Checking Authentication and Project
echo ================================================
echo.

if defined GCLOUD (
    echo Active account:
    %GCLOUD% auth list --filter=status:ACTIVE --format="value(account)"
    echo.
    echo Current project:
    %GCLOUD% config get-value project
) else (
    echo Active account:
    gcloud auth list --filter=status:ACTIVE --format="value(account)"
    echo.
    echo Current project:
    gcloud config get-value project
)

echo.
echo ================================================
echo Setup Status
echo ================================================
echo.
echo Project ID: ideas-matter-1749958193112
echo.
echo If you need to authenticate, run:
echo   gcloud auth login
echo.
echo To set the project, run:
echo   gcloud config set project ideas-matter-1749958193112
echo.
echo When ready, run:
echo   deploy-to-gcp.bat
echo.
pause