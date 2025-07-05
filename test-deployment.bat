@echo off
echo ================================================
echo Testing Jackson Ideas Platform Deployment
echo ================================================
echo.

set WEB_URL=https://jackson-ideas-web-480585172218.us-central1.run.app

echo 1. Testing Web Application...
echo    URL: %WEB_URL%
curl -s -o nul -w "   Status: %%{http_code}\n" %WEB_URL%
echo.

set /p API_URL="2. Enter your Render API URL (or press Enter to skip): "

if not "%API_URL%"=="" (
    echo.
    echo 3. Testing API Health Endpoint...
    echo    URL: %API_URL%/health
    curl -s %API_URL%/health
    echo.
    echo.
    echo 4. Testing API Swagger Documentation...
    echo    URL: %API_URL%/swagger
    curl -s -o nul -w "   Status: %%{http_code}\n" %API_URL%/swagger
) else (
    echo.
    echo API testing skipped - deploy to Render first!
)

echo.
echo ================================================
echo Test Summary
echo ================================================
echo.
echo Web App URL: %WEB_URL%
if not "%API_URL%"=="" (
    echo API URL: %API_URL%
    echo.
    echo If both show Status: 200, your deployment is successful!
) else (
    echo API URL: Not yet deployed
    echo.
    echo Next step: Deploy API to Render.com
)
echo.
echo Manual Testing Steps:
echo 1. Open %WEB_URL% in your browser
echo 2. Click "Register" to create an account
echo 3. Login with your credentials
echo 4. Submit an idea for analysis
echo.
echo ================================================
pause