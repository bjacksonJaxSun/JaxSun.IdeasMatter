@echo off
echo ================================================
echo           Service Status Check
echo ================================================
echo.

echo Checking Backend (Port 8000)...
netstat -an | findstr ":8000.*LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✅ Backend is running on port 8000
    echo Testing backend health...
    curl -s http://localhost:8000/health >nul 2>&1
    if %ERRORLEVEL% EQU 0 (
        echo ✅ Backend is responding
    ) else (
        echo ❌ Backend not responding to requests
    )
) else (
    echo ❌ Backend is NOT running on port 8000
)

echo.
echo Checking Frontend (Port 4000)...
netstat -an | findstr ":4000.*LISTENING" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✅ Frontend is running on port 4000
) else (
    echo ❌ Frontend is NOT running on port 4000
)

echo.
echo Checking Python/Node processes...
tasklist | findstr "python.exe" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✅ Python processes found
) else (
    echo ❌ No Python processes running
)

tasklist | findstr "node.exe" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ✅ Node.js processes found
) else (
    echo ❌ No Node.js processes running
)

echo.
echo ================================================
echo           Quick Actions
echo ================================================
echo 1. Use start_dev.bat to start services
echo 2. Use stop_dev.bat to stop all services
echo 3. Open http://localhost:4000 in browser
echo 4. Check http://localhost:8000/health for API
echo.
pause