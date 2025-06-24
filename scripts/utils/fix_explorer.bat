@echo off
echo ================================================
echo         Explorer.exe Recovery Tool
echo ================================================
echo.
echo If your taskbar and desktop icons are missing,
echo this script will restart Windows Explorer.
echo.

echo Checking if explorer.exe is running...
tasklist /fi "imagename eq explorer.exe" | find "explorer.exe" >nul
if %errorlevel% == 0 (
    echo Explorer.exe is already running!
    echo.
    echo If you're still having issues, you can:
    echo   1. Press Ctrl+Shift+Esc to open Task Manager
    echo   2. Go to File -^> Run new task
    echo   3. Type "explorer.exe" and press Enter
) else (
    echo Explorer.exe is NOT running!
    echo Starting explorer.exe...
    start explorer.exe
    echo.
    echo ✅ Explorer.exe has been restarted!
    echo Your taskbar and desktop should appear shortly.
)

echo.
echo ================================================
echo Prevention Tips:
echo ================================================
echo.
echo 1. Always use 'stop_dev.bat' (now updated to be safe)
echo 2. Never use "taskkill /f /im explorer.exe"
echo 3. The updated stop_dev.bat only kills project-specific processes
echo.
pause