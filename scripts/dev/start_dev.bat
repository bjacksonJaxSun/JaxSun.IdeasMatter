@echo off
REM ========================================
REM Ideas Matter Development Startup Script
REM Starts both backend and frontend servers
REM ========================================

echo Starting Ideas Matter Development Environment...
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8+ and ensure it's in your PATH
    pause
    exit /b 1
)

REM Check if Node.js is installed
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js is not installed or not in PATH
    echo Please install Node.js 16+ and ensure it's in your PATH
    pause
    exit /b 1
)

REM Start Backend
echo Starting Backend Server...
cd /d "%~dp0..\..\backend"

if exist venv\Scripts\activate.bat (
    echo Using existing virtual environment...
) else (
    echo Creating virtual environment...
    python -m venv venv
)

REM Use main.py for more reliable startup
start "Ideas Matter Backend" cmd /k "call venv\Scripts\activate.bat && pip install -r requirements.txt && python main.py"

REM Wait for backend to start
timeout /t 5 /nobreak >nul

REM Start Frontend
echo Starting Frontend Server...
cd /d "%~dp0..\..\frontend"

if not exist node_modules (
    echo Installing frontend dependencies...
    call npm install
)

start "Ideas Matter Frontend" cmd /k "npm run dev"

echo.
echo ========================================
echo Development servers starting:
echo Backend: http://localhost:8000
echo Frontend: http://localhost:4000
echo API Docs: http://localhost:8000/docs
echo ========================================
echo.
echo Press any key to exit (servers will continue running)...
pause >nul