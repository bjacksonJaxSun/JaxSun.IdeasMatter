@echo off
REM Simple script to trigger workflow using Python API method
REM No Git or GitHub CLI required - just needs a token

echo ==========================================
echo Ideas Matter Vision Creation (API Method)
echo ==========================================
echo.

cd /d C:\Development\Jackson.Ideas

REM Check if Python is available
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python from: https://www.python.org/
    pause
    exit /b 1
)

REM Check for GitHub token
if "%GH_TOKEN%"=="" (
    if "%GITHUB_TOKEN%"=="" (
        echo ERROR: No GitHub token found!
        echo.
        echo To create a token:
        echo 1. Go to: https://github.com/settings/tokens
        echo 2. Click "Generate new token (classic)"
        echo 3. Select scopes: 'repo' and 'workflow'
        echo 4. Copy the token
        echo.
        echo Then set it:
        echo   set GH_TOKEN=your-token-here
        echo.
        echo And run this script again.
        pause
        exit /b 1
    )
)

echo Running workflow trigger...
python Commands\claude-commands\trigger-workflow-api.py

pause