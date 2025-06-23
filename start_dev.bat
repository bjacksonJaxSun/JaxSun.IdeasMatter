@echo off
echo ================================================
echo          Starting Ideas Matter Development
echo ================================================
echo.

REM Create log directory if it doesn't exist
if not exist "logs" mkdir logs

echo [1/4] Starting Backend Server...
echo Starting backend on http://localhost:8000
cd backend
start "Ideas Matter Backend" cmd /k "venv\Scripts\activate.bat && python main.py"
cd ..

echo [2/4] Waiting for backend to initialize...
timeout /t 3 /nobreak >nul

echo [3/4] Starting Frontend Development Server...
echo Starting frontend on http://localhost:4000
cd frontend
start "Ideas Matter Frontend" cmd /k "npm run dev"
cd ..

echo [4/4] Services starting...
timeout /t 2 /nobreak >nul

echo.
echo ✅ Both services are starting!
echo.
echo 📊 Backend API: http://localhost:8000
echo 📊 API Docs: http://localhost:8000/docs
echo 🌐 Frontend: http://localhost:4000
echo.
echo Press any key to open the application in your browser...
pause >nul

REM Open the frontend in default browser
start http://localhost:4000

echo.
echo Services are running in separate windows.
echo Close those windows or use stop_dev.bat to stop all services.
pause