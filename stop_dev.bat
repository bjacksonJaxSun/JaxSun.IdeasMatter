@echo off
echo ================================================
echo          Stopping Ideas Matter Development
echo ================================================
echo.

echo Stopping all Node.js processes (Frontend)...
taskkill /f /im node.exe >nul 2>&1

echo Stopping all Python processes (Backend)...
taskkill /f /im python.exe >nul 2>&1

echo Stopping all Command Prompt windows with "Ideas Matter" in title...
taskkill /f /fi "WINDOWTITLE eq Ideas Matter*" >nul 2>&1

echo.
echo ✅ All development services stopped!
echo.
pause