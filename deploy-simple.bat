@echo off
echo Deploying simplified version to Cloud Run...

set PROJECT_ID=ideas-matter-1749958193112
set REGION=us-central1

echo.
echo Deploying API without database connection...
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run deploy jackson-ideas-api --image=gcr.io/ideas-matter-1749958193112/jackson-ideas-api:latest --region=%REGION% --platform=managed --allow-unauthenticated --port=8080 --memory=1Gi --cpu=2 --set-env-vars=ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080 --set-secrets=JwtSettings__SecretKey=jackson-ideas-jwt-secret:latest --timeout=300"

echo.
echo Waiting for API deployment...
timeout /t 30 /nobreak >nul

echo.
echo Getting API URL...
for /f "tokens=*" %%i in ('powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run services describe jackson-ideas-api --region=%REGION% --format='value(status.url)'"') do set API_URL=%%i

echo API URL: %API_URL%

echo.
echo Deploying Web application...
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run deploy jackson-ideas-web --image=gcr.io/ideas-matter-1749958193112/jackson-ideas-web:latest --region=%REGION% --platform=managed --allow-unauthenticated --port=8080 --memory=1Gi --cpu=2 --set-env-vars=ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080,ApiSettings__BaseUrl=%API_URL% --timeout=300"

echo.
echo Getting Web URL...
for /f "tokens=*" %%i in ('powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run services describe jackson-ideas-web --region=%REGION% --format='value(status.url)'"') do set WEB_URL=%%i

echo.
echo ================================================
echo Deployment Status:
echo ================================================
echo API URL: %API_URL%
echo Web URL: %WEB_URL%
echo.
echo To check logs:
echo   gcloud run services logs read jackson-ideas-api --region=%REGION%
echo   gcloud run services logs read jackson-ideas-web --region=%REGION%
echo ================================================
pause