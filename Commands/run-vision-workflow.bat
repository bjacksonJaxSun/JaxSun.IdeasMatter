@echo off
REM Script to run GitHub workflow from Windows
echo ==========================================
echo Running Ideas Matter Vision Creation
echo ==========================================
echo.

cd /d C:\Development\Jackson.Ideas

REM Check if gh is installed
where gh >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: GitHub CLI not found in PATH
    echo Please install from: https://cli.github.com/
    echo Or add it to your PATH if already installed
    pause
    exit /b 1
)

echo Triggering GitHub workflow...
gh workflow run create-vision.yml -f product_name="Ideas Matter" -f vision_file_path="Commands/docs/visions/ideas-matter/vision.md" -f preview=false

if %errorlevel% equ 0 (
    echo.
    echo SUCCESS! Workflow triggered.
    echo.
    echo Checking workflow status...
    timeout /t 3 /nobreak >nul
    
    REM Get the latest run
    for /f "tokens=*" %%i in ('gh run list --workflow=create-vision.yml --limit=1 --json url --jq ".[0].url"') do set RUN_URL=%%i
    
    if defined RUN_URL (
        echo.
        echo View workflow progress at:
        echo %RUN_URL%
        echo.
        echo Opening in browser...
        start "" "%RUN_URL%"
    )
) else (
    echo.
    echo ERROR: Failed to trigger workflow
    echo Please check your GitHub authentication with: gh auth status
)

echo.
pause