# Ideas Matter Development Starter (PowerShell)
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "          Starting Ideas Matter Development" -ForegroundColor Cyan  
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Create log directory if it doesn't exist
if (!(Test-Path "logs")) {
    New-Item -ItemType Directory -Name "logs"
}

Write-Host "[1/4] Starting Backend Server..." -ForegroundColor Yellow
Write-Host "Starting backend on http://localhost:8000" -ForegroundColor Green
$backendPath = ".\backend"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$backendPath'; .\venv\Scripts\Activate.ps1; python main.py" -WindowStyle Normal

Write-Host "[2/4] Waiting for backend to initialize..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

Write-Host "[3/4] Starting Frontend Development Server..." -ForegroundColor Yellow
Write-Host "Starting frontend on http://localhost:4000" -ForegroundColor Green
$frontendPath = ".\frontend"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$frontendPath'; npm run dev" -WindowStyle Normal

Write-Host "[4/4] Services starting..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "✅ Both services are starting!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Backend API: http://localhost:8000" -ForegroundColor Blue
Write-Host "📊 API Docs: http://localhost:8000/docs" -ForegroundColor Blue
Write-Host "🌐 Frontend: http://localhost:4000" -ForegroundColor Blue
Write-Host ""
Write-Host "Press Enter to open the application in your browser..."
Read-Host

# Open the frontend in default browser
Start-Process "http://localhost:4000"

Write-Host ""
Write-Host "Services are running in separate PowerShell windows." -ForegroundColor Cyan
Write-Host "Close those windows or use stop_dev.ps1 to stop all services." -ForegroundColor Cyan
Read-Host "Press Enter to exit this script"