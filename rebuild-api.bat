@echo off
echo Rebuilding and deploying API only...

set PROJECT_ID=ideas-matter-1749958193112
set REGION=us-central1

echo.
echo Building API Docker image locally...
docker build -t gcr.io/%PROJECT_ID%/jackson-ideas-api:latest -f Dockerfile.api .
if errorlevel 1 (
    echo Failed to build Docker image locally. Using Cloud Build instead...
    echo.
    echo Submitting to Cloud Build...
    powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' builds submit --tag gcr.io/%PROJECT_ID%/jackson-ideas-api:latest -f Dockerfile.api ."
) else (
    echo.
    echo Pushing to Container Registry...
    docker push gcr.io/%PROJECT_ID%/jackson-ideas-api:latest
)

echo.
echo Deploying API to Cloud Run...
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run deploy jackson-ideas-api --image=gcr.io/%PROJECT_ID%/jackson-ideas-api:latest --region=%REGION% --platform=managed --allow-unauthenticated --port=8080 --memory=1Gi --cpu=2 --timeout=300 --max-instances=10"

echo.
echo Getting API URL...
for /f "tokens=*" %%i in ('powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run services describe jackson-ideas-api --region=%REGION% --format='value(status.url)'"') do set API_URL=%%i

echo.
echo ================================================
echo API Deployment Complete!
echo ================================================
echo API URL: %API_URL%
echo.
echo Test the API health endpoint:
echo curl %API_URL%/health
echo.
echo Update Web app with API URL:
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run services update jackson-ideas-web --region=%REGION% --update-env-vars=ApiSettings__BaseUrl=%API_URL%"
echo ================================================
pause