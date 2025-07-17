@echo off
REM Final script to create Ideas Matter vision in GitHub

cd /d C:\Development\Jackson.Ideas

echo ==========================================
echo Ideas Matter Vision Creation - FINAL
echo ==========================================
echo.

REM Check if gh is available
where gh >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: GitHub CLI not found
    echo Please install from: https://cli.github.com/
    pause
    exit /b 1
)

REM Check authentication
echo Checking GitHub authentication...
gh auth status >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Not authenticated with GitHub
    echo Please run: gh auth login
    echo.
    echo When prompted:
    echo - Choose: GitHub.com
    echo - Choose: HTTPS
    echo - Authenticate with: a web browser
    pause
    exit /b 1
)

echo Authentication OK!
echo.
echo Triggering workflow...
gh workflow run create-vision.yml -f product_name="Ideas Matter" -f vision_file_path="Commands/docs/visions/ideas-matter/vision.md" -f preview=false

if %errorlevel% equ 0 (
    echo.
    echo ==========================================
    echo SUCCESS! Workflow triggered.
    echo ==========================================
    echo.
    timeout /t 3 /nobreak >nul
    
    echo Opening GitHub Actions page...
    start https://github.com/bjackson071968/Jackson.Ideas/actions
    
    echo.
    echo The workflow will create:
    echo - Vision Issue titled "Vision: Ideas Matter" (pinned)
    echo - All necessary labels for the workflow
    echo.
    echo NOTE: GitHub Projects must be created manually due to permissions
    echo.
    echo After the workflow completes:
    echo 1. Check the Issues tab for the Vision issue
    echo 2. Note the issue number for creating strategy
    echo 3. Optionally create a project at:
    echo    https://github.com/bjackson071968/Jackson.Ideas/projects
) else (
    echo.
    echo ERROR: Failed to trigger workflow
    echo Please check: gh auth status
)

echo.
pause