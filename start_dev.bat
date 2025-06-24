@echo off
REM ========================================
REM Ideas Matter - Smart Development Script
REM Stops existing services, starts fresh, opens browser
REM ========================================

echo.
echo ========================================
echo    Ideas Matter Development Environment
echo ========================================
echo.

REM Check prerequisites
echo [1/6] Checking prerequisites...
python --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Python is not installed or not in PATH
    echo    Please install Python 3.8+ from https://python.org
    pause
    exit /b 1
)

node --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Node.js is not installed or not in PATH  
    echo    Please install Node.js 16+ from https://nodejs.org
    pause
    exit /b 1
)

echo [OK] Prerequisites found
echo.

REM Stop any existing services
echo [2/6] Stopping existing services...
echo    Checking for services on ports 8000 and 4000...

REM Stop backend on port 8000
set backend_stopped=false
netstat -aon | findstr :8000 | findstr LISTENING >nul 2>&1
if not errorlevel 1 (
    echo    Stopping processes on port 8000...
    for /f "tokens=5" %%a in ('netstat -aon ^| findstr :8000 ^| findstr LISTENING') do (
        taskkill /F /PID %%a >nul 2>&1
    )
    set backend_stopped=true
)

REM Stop frontend on port 4000
set frontend_stopped=false
netstat -aon | findstr :4000 | findstr LISTENING >nul 2>&1
if not errorlevel 1 (
    echo    Stopping processes on port 4000...
    for /f "tokens=5" %%a in ('netstat -aon ^| findstr :4000 ^| findstr LISTENING') do (
        taskkill /F /PID %%a >nul 2>&1
    )
    set frontend_stopped=true
)

REM Also stop by window title
taskkill /FI "WindowTitle eq Ideas Matter Backend*" >nul 2>&1
taskkill /FI "WindowTitle eq Ideas Matter Frontend*" >nul 2>&1

if "%backend_stopped%"=="true" echo    [OK] Stopped existing backend
if "%frontend_stopped%"=="true" echo    [OK] Stopped existing frontend
if "%backend_stopped%"=="false" if "%frontend_stopped%"=="false" echo    [INFO] No existing services found

REM Give processes time to stop
timeout /t 2 /nobreak >nul
echo.

REM Setup backend
echo [3/6] Setting up backend...
cd /d "%~dp0backend"

if exist venv\Scripts\activate.bat (
    echo    [OK] Using existing virtual environment
) else (
    echo    Creating virtual environment...
    python -m venv venv
    if errorlevel 1 (
        echo    [ERROR] Failed to create virtual environment
        pause
        exit /b 1
    )
    echo    [OK] Virtual environment created
)

echo    Installing/updating Python dependencies...
call venv\Scripts\activate.bat
pip install -q -r requirements.txt
if errorlevel 1 (
    echo    [ERROR] Failed to install Python dependencies
    echo    Try running: backend\venv\Scripts\activate.bat then pip install -r requirements.txt
    pause
    exit /b 1
)
echo    [OK] Backend dependencies ready
echo.

REM Setup frontend  
echo [4/6] Setting up frontend...
cd /d "%~dp0frontend"

if exist node_modules (
    echo    [OK] Using existing node_modules
) else (
    echo    Installing Node.js dependencies...
    call npm install
    if errorlevel 1 (
        echo    [ERROR] Failed to install Node.js dependencies
        pause
        exit /b 1
    )
    echo    [OK] Frontend dependencies installed
)
echo.

REM Start services
echo [5/6] Starting services...
echo    Starting backend server...
cd /d "%~dp0backend"
start "Ideas Matter Backend" /min cmd /k "call venv\Scripts\activate.bat && echo Backend starting on http://localhost:8000 && echo API Docs: http://localhost:8000/docs && echo Press Ctrl+C to stop && python main.py"

echo    [WAIT] Waiting for backend to initialize...
timeout /t 5 /nobreak >nul

echo    Starting frontend server...
cd /d "%~dp0frontend"
start "Ideas Matter Frontend" /min cmd /k "echo Frontend starting on http://localhost:4000 && echo Press Ctrl+C to stop && npm run dev"

echo    [WAIT] Waiting for frontend to initialize...
timeout /t 8 /nobreak >nul
echo.

REM Open browser
echo [6/6] Opening application...
echo    [BROWSER] Opening Ideas Matter in your default browser...
start "" "http://localhost:4000"

echo.
echo ========================================
echo [SUCCESS] Ideas Matter is now running!
echo ========================================
echo.
echo [WEB] Application:  http://localhost:4000
echo [API] Backend API:  http://localhost:8000  
echo [DOCS] API Docs:     http://localhost:8000/docs
echo.
echo [INFO] Both servers are running in separate windows
echo [STOP] To stop: Close the server windows or run this script again
echo.
echo Press any key to exit this window...
pause >nul