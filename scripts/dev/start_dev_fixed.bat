@echo off
REM ========================================
REM Ideas Matter Development Startup Script (Fixed)
REM Addresses WSL/Windows path confusion
REM ========================================

echo Starting Ideas Matter Development Environment...
echo.

REM Set explicit Windows paths to avoid WSL confusion
set "CURRENT_DIR=%CD%"
set "SCRIPT_DIR=%~dp0"
set "ROOT_DIR=%SCRIPT_DIR%..\.."
set "BACKEND_DIR=%ROOT_DIR%\backend"
set "FRONTEND_DIR=%ROOT_DIR%\frontend"

echo Script directory: %SCRIPT_DIR%
echo Root directory: %ROOT_DIR%
echo Backend directory: %BACKEND_DIR%
echo Frontend directory: %FRONTEND_DIR%
echo.

REM Check if Python is installed and accessible
echo Checking Python installation...
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8+ and ensure it's in your PATH
    pause
    exit /b 1
) else (
    echo Python found: 
    python --version
)

REM Check if Node.js is installed
echo Checking Node.js installation...
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js is not installed or not in PATH
    echo Please install Node.js 16+ and ensure it's in your PATH
    pause
    exit /b 1
) else (
    echo Node.js found:
    node --version
)

echo.

REM Start Backend
echo ========================================
echo Starting Backend Server...
echo ========================================
cd /d "%BACKEND_DIR%"
echo Current directory: %CD%

if exist venv\Scripts\activate.bat (
    echo Using existing virtual environment...
) else (
    echo Creating virtual environment...
    python -m venv venv
    if errorlevel 1 (
        echo ERROR: Failed to create virtual environment
        pause
        exit /b 1
    )
)

REM Create a batch file to run the backend to avoid command line issues
echo @echo off > start_backend_temp.bat
echo echo Starting backend server... >> start_backend_temp.bat
echo call venv\Scripts\activate.bat >> start_backend_temp.bat
echo if errorlevel 1 ( >> start_backend_temp.bat
echo     echo Failed to activate virtual environment >> start_backend_temp.bat
echo     pause >> start_backend_temp.bat
echo     exit /b 1 >> start_backend_temp.bat
echo ^) >> start_backend_temp.bat
echo pip install -r requirements.txt >> start_backend_temp.bat
echo if errorlevel 1 ( >> start_backend_temp.bat
echo     echo Failed to install requirements >> start_backend_temp.bat
echo     pause >> start_backend_temp.bat
echo     exit /b 1 >> start_backend_temp.bat
echo ^) >> start_backend_temp.bat
echo python main.py >> start_backend_temp.bat

start "Ideas Matter Backend" cmd /k "start_backend_temp.bat"

REM Wait for backend to start
echo Waiting for backend to start...
timeout /t 8 /nobreak >nul

REM Start Frontend
echo.
echo ========================================
echo Starting Frontend Server...
echo ========================================
cd /d "%FRONTEND_DIR%"
echo Current directory: %CD%

if not exist node_modules (
    echo Installing frontend dependencies...
    call npm install
    if errorlevel 1 (
        echo ERROR: Failed to install frontend dependencies
        pause
        exit /b 1
    )
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
echo Check the opened command windows for any errors.
echo Press any key to exit (servers will continue running)...
pause >nul

REM Cleanup
if exist "%BACKEND_DIR%\start_backend_temp.bat" (
    del "%BACKEND_DIR%\start_backend_temp.bat"
)