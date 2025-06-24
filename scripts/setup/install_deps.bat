@echo off
REM ========================================
REM Dependency Installation Script
REM Installs all required dependencies
REM ========================================

echo Installing Ideas Matter Dependencies...
echo.

REM Check Python
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8+ first
    pause
    exit /b 1
)

REM Check Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js is not installed or not in PATH
    echo Please install Node.js 16+ first
    pause
    exit /b 1
)

REM Backend Dependencies
echo.
echo ========================================
echo Installing Backend Dependencies...
echo ========================================
cd /d "%~dp0..\..\backend"

REM Create virtual environment if it doesn't exist
if not exist venv (
    echo Creating Python virtual environment...
    python -m venv venv
)

REM Activate and install
echo Installing Python packages...
call venv\Scripts\activate.bat
python -m pip install --upgrade pip
pip install -r requirements.txt

echo.
echo Backend dependencies installed!

REM Frontend Dependencies
echo.
echo ========================================
echo Installing Frontend Dependencies...
echo ========================================
cd /d "%~dp0..\..\frontend"

echo Installing Node packages...
call npm install

echo.
echo Frontend dependencies installed!

REM Summary
echo.
echo ========================================
echo ✅ All dependencies installed successfully!
echo ========================================
echo.
echo Next steps:
echo 1. Copy backend\.env.example to backend\.env
echo 2. Configure your environment variables
echo 3. Run: scripts\dev\start_dev.bat
echo.
pause