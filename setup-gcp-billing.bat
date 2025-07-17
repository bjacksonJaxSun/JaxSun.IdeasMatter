@echo off
echo ================================================
echo Google Cloud Billing Setup Guide
echo ================================================
echo.
echo Project: ideas-matter-1749958193112
echo Account: bjackson071968@gmail.com
echo.
echo This script will help you set up billing for your Google Cloud project.
echo.
echo IMPORTANT: You need to complete these steps in your web browser.
echo.
pause

echo.
echo Step 1: Opening Google Cloud Billing page...
start https://console.cloud.google.com/billing/linkedaccount?project=ideas-matter-1749958193112

echo.
echo ================================================
echo Follow these steps in your browser:
echo ================================================
echo.
echo 1. Click "CREATE BILLING ACCOUNT" or "LINK BILLING ACCOUNT"
echo.
echo 2. If creating a new billing account:
echo    - Enter your payment information (credit/debit card)
echo    - Accept the terms of service
echo    - Click "START MY FREE TRIAL" to get $300 in credits
echo.
echo 3. If linking an existing billing account:
echo    - Select your billing account from the list
echo    - Click "SET ACCOUNT"
echo.
echo 4. Wait for confirmation that billing is enabled
echo.
echo ================================================
echo.
echo Press any key when you've completed the billing setup...
pause >nul

echo.
echo Verifying billing status...
echo.

REM Check if billing is enabled by trying to enable a service
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' services enable cloudresourcemanager.googleapis.com" >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Billing appears to be enabled!
    echo.
    echo You can now run: deploy-to-gcp.bat
) else (
    echo [WARNING] Billing may not be enabled yet.
    echo.
    echo If you just enabled billing, it may take a few minutes to activate.
    echo Try running deploy-to-gcp.bat in a few minutes.
)

echo.
echo ================================================
echo Additional Information:
echo ================================================
echo.
echo - New accounts get $300 in free credits (90 days)
echo - You won't be charged unless you upgrade to a paid account
echo - This app will cost approximately $5-10/month after free credits
echo - You can set up budget alerts to monitor spending
echo.
echo To check your billing status:
powershell -Command "& 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd' beta billing accounts list"
echo.
pause