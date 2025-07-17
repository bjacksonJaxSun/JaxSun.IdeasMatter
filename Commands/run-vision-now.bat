@echo off
REM Quick script to run the vision workflow now that it's pushed to GitHub

cd /d C:\Development\Jackson.Ideas

echo ==========================================
echo Running Ideas Matter Vision Creation
echo ==========================================
echo.

REM Check if gh is available
where gh >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: GitHub CLI not found
    echo Please ensure Git for Windows and GitHub CLI are installed
    pause
    exit /b 1
)

REM Check authentication
echo Checking GitHub authentication...
gh auth status >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Not authenticated with GitHub
    echo Please run: gh auth login
    pause
    exit /b 1
)

echo Triggering workflow...
gh workflow run create-vision.yml -f product_name="Ideas Matter" -f vision_file_path="Commands/docs/visions/ideas-matter/vision.md" -f preview=false

if %errorlevel% equ 0 (
    echo.
    echo SUCCESS! Workflow triggered.
    echo.
    timeout /t 3 /nobreak >nul
    
    echo Opening GitHub Actions page...
    start https://github.com/bjackson071968/Jackson.Ideas/actions
    
    echo.
    echo The workflow will create:
    echo - Vision Issue (pinned to repository)
    echo - GitHub Project for Ideas Matter
    echo - All necessary labels
) else (
    echo.
    echo ERROR: Failed to trigger workflow
    echo Try running: gh auth login
)

echo.
pause