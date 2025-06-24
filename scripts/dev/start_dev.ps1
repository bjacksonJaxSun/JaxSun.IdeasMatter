# ========================================
# Ideas Matter Development Startup Script (PowerShell)
# Starts both backend and frontend servers
# ========================================

Write-Host "Starting Ideas Matter Development Environment..." -ForegroundColor Green
Write-Host ""

# Check if Python is installed
try {
    $pythonVersion = python --version 2>&1
    Write-Host "Found Python: $pythonVersion" -ForegroundColor Cyan
} catch {
    Write-Host "ERROR: Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Python 3.8+ and ensure it's in your PATH"
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if Node.js is installed
try {
    $nodeVersion = node --version 2>&1
    Write-Host "Found Node.js: $nodeVersion" -ForegroundColor Cyan
} catch {
    Write-Host "ERROR: Node.js is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install Node.js 16+ and ensure it's in your PATH"
    Read-Host "Press Enter to exit"
    exit 1
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent (Split-Path -Parent $scriptDir)

# Start Backend
Write-Host "`nStarting Backend Server..." -ForegroundColor Yellow
$backendPath = Join-Path $rootDir "backend"
Set-Location $backendPath

$venvPath = Join-Path $backendPath "venv"
if (Test-Path (Join-Path $venvPath "Scripts\activate.ps1")) {
    Write-Host "Using existing virtual environment..."
} else {
    Write-Host "Creating virtual environment..."
    python -m venv venv
}

# Start backend in new window
$backendScript = @"
cd '$backendPath'
& '$venvPath\Scripts\Activate.ps1'
pip install -r requirements.txt
python main.py
"@

Start-Process powershell -ArgumentList "-NoExit", "-Command", $backendScript -WindowStyle Normal

# Wait for backend to start
Start-Sleep -Seconds 5

# Start Frontend
Write-Host "Starting Frontend Server..." -ForegroundColor Yellow
$frontendPath = Join-Path $rootDir "frontend"
Set-Location $frontendPath

if (-not (Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..."
    npm install
}

# Start frontend in new window
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$frontendPath'; npm run dev" -WindowStyle Normal

Write-Host ""
Write-Host "========================================"  -ForegroundColor Green
Write-Host "Development servers starting:"
Write-Host "Backend:  http://localhost:8000"         -ForegroundColor Cyan
Write-Host "Frontend: http://localhost:4000"         -ForegroundColor Cyan
Write-Host "API Docs: http://localhost:8000/docs"    -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Press any key to exit (servers will continue running)..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")