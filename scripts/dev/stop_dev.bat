@echo off
REM ========================================
REM Ideas Matter Development Shutdown Script
REM Stops backend and frontend servers safely
REM ========================================

echo Stopping Ideas Matter Development Servers...
echo.

REM Stop services on ports 8000 and 4000
echo Stopping services on ports 8000 and 4000...

REM Stop backend on port 8000
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :8000 ^| findstr LISTENING') do (
    echo Stopping backend server (PID: %%a)...
    taskkill /F /PID %%a >nul 2>&1
)

REM Stop frontend on port 4000
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :4000 ^| findstr LISTENING') do (
    echo Stopping frontend server (PID: %%a)...
    taskkill /F /PID %%a >nul 2>&1
)

REM Also stop by window title as backup
taskkill /FI "WindowTitle eq Ideas Matter Backend*" >nul 2>&1
taskkill /FI "WindowTitle eq Ideas Matter Frontend*" >nul 2>&1

REM Stop any Python/Node processes that might be hanging
echo Cleaning up any remaining Python/Node processes...
tasklist | findstr "python.exe" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Found Python processes, checking if they're ours...
    REM Only kill Python processes on our ports
    for /f "tokens=5" %%a in ('netstat -aon ^| findstr :8000') do taskkill /F /PID %%a >nul 2>&1
)

tasklist | findstr "node.exe" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Found Node processes, checking if they're ours...
    REM Only kill Node processes on our ports
    for /f "tokens=5" %%a in ('netstat -aon ^| findstr :4000') do taskkill /F /PID %%a >nul 2>&1
)

echo.
echo ========================================
echo Development servers stopped successfully
echo ========================================
echo.
pause