@echo off
echo ================================================
echo Redeploying Web Application to Google Cloud
echo ================================================
echo.

set PROJECT_ID=ideas-matter-1749958193112
set REGION=us-central1

echo Building and pushing Web Docker image...
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' builds submit --tag gcr.io/%PROJECT_ID%/jackson-ideas-web:latest -f Dockerfile.web ."

echo.
echo Deploying to Cloud Run...
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' run deploy jackson-ideas-web --image=gcr.io/%PROJECT_ID%/jackson-ideas-web:latest --region=%REGION% --platform=managed --allow-unauthenticated --port=8080 --memory=1Gi --cpu=2 --set-env-vars='ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080,ApiSettings__BaseUrl=https://jackson-ideas.onrender.com' --timeout=300"

echo.
echo ================================================
echo Deployment Complete!
echo ================================================
echo.
echo Your updated web application is live at:
echo https://jackson-ideas-web-480585172218.us-central1.run.app
echo.
echo The navigation changes are now live:
echo - User menu in top-right corner
echo - Profile and logout in dropdown
echo - Cleaner sidebar navigation
echo ================================================
pause