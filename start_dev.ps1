# ========================================
# Ideas Matter - Smart Development Script (PowerShell)
# Stops existing services, starts fresh, opens browser
# ========================================

Write-Host ""
Write-Host "========================================"  -ForegroundColor Green
Write-Host "   Ideas Matter Development Environment"   -ForegroundColor Green
Write-Host "========================================"  -ForegroundColor Green
Write-Host ""

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Check prerequisites
Write-Host "[1/6] Checking prerequisites..." -ForegroundColor Yellow

try {
    $pythonVersion = python --version 2>&1
    Write-Host "    ✅ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "    ❌ Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "       Please install Python 3.8+ from https://python.org" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

try {
    $nodeVersion = node --version 2>&1
    Write-Host "    ✅ Node.js found: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "    ❌ Node.js is not installed or not in PATH" -ForegroundColor Red
    Write-Host "       Please install Node.js 16+ from https://nodejs.org" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""

# Stop existing services
Write-Host "[2/6] Stopping existing services..." -ForegroundColor Yellow
Write-Host "    Checking for services on ports 8000 and 4000..."

$backendStopped = $false
$frontendStopped = $false

# Stop backend on port 8000
$backendConnections = Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue
if ($backendConnections) {
    foreach ($conn in $backendConnections) {
        $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "    Stopping backend server (PID: $($process.Id))..." -ForegroundColor Cyan
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            $backendStopped = $true
        }
    }
}

# Stop frontend on port 4000
$frontendConnections = Get-NetTCPConnection -LocalPort 4000 -State Listen -ErrorAction SilentlyContinue
if ($frontendConnections) {
    foreach ($conn in $frontendConnections) {
        $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "    Stopping frontend server (PID: $($process.Id))..." -ForegroundColor Cyan
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            $frontendStopped = $true
        }
    }
}

# Stop by window title as backup
$backendWindows = Get-Process | Where-Object { $_.MainWindowTitle -like "Ideas Matter Backend*" }
foreach ($window in $backendWindows) {
    Stop-Process -Id $window.Id -Force -ErrorAction SilentlyContinue
    $backendStopped = $true
}

$frontendWindows = Get-Process | Where-Object { $_.MainWindowTitle -like "Ideas Matter Frontend*" }
foreach ($window in $frontendWindows) {
    Stop-Process -Id $window.Id -Force -ErrorAction SilentlyContinue
    $frontendStopped = $true
}

if ($backendStopped) { Write-Host "    ✅ Stopped existing backend" -ForegroundColor Green }
if ($frontendStopped) { Write-Host "    ✅ Stopped existing frontend" -ForegroundColor Green }
if (-not $backendStopped -and -not $frontendStopped) { 
    Write-Host "    ℹ️  No existing services found" -ForegroundColor Gray 
}

# Give processes time to stop
Start-Sleep -Seconds 2
Write-Host ""

# Setup backend
Write-Host "[3/6] Setting up backend..." -ForegroundColor Yellow
$backendPath = Join-Path $scriptDir "backend"
Set-Location $backendPath

$venvPath = Join-Path $backendPath "venv"
if (Test-Path (Join-Path $venvPath "Scripts\activate.ps1")) {
    Write-Host "    ✅ Using existing virtual environment" -ForegroundColor Green
} else {
    Write-Host "    Creating virtual environment..." -ForegroundColor Cyan
    try {
        python -m venv venv
        Write-Host "    ✅ Virtual environment created" -ForegroundColor Green
    } catch {
        Write-Host "    ❌ Failed to create virtual environment" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
}

Write-Host "    Installing/updating Python dependencies..." -ForegroundColor Cyan
try {
    & "$venvPath\Scripts\Activate.ps1"
    pip install -q -r requirements.txt
    if ($LASTEXITCODE -ne 0) { throw }
    Write-Host "    ✅ Backend dependencies ready" -ForegroundColor Green
} catch {
    Write-Host "    ❌ Failed to install Python dependencies" -ForegroundColor Red
    Write-Host "       Try running: backend\venv\Scripts\Activate.ps1 then pip install -r requirements.txt" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""

# Setup frontend
Write-Host "[4/6] Setting up frontend..." -ForegroundColor Yellow
$frontendPath = Join-Path $scriptDir "frontend"
Set-Location $frontendPath

if (Test-Path "node_modules") {
    Write-Host "    ✅ Using existing node_modules" -ForegroundColor Green
} else {
    Write-Host "    Installing Node.js dependencies..." -ForegroundColor Cyan
    try {
        npm install
        if ($LASTEXITCODE -ne 0) { throw }
        Write-Host "    ✅ Frontend dependencies installed" -ForegroundColor Green
    } catch {
        Write-Host "    ❌ Failed to install Node.js dependencies" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
}

Write-Host ""

# Start services
Write-Host "[5/6] Starting services..." -ForegroundColor Yellow

Write-Host "    Starting backend server..." -ForegroundColor Cyan
$backendScript = @"
cd '$backendPath'
& '$venvPath\Scripts\Activate.ps1'
Write-Host 'Backend starting on http://localhost:8000' -ForegroundColor Green
Write-Host 'API Docs: http://localhost:8000/docs' -ForegroundColor Cyan
Write-Host 'Press Ctrl+C to stop' -ForegroundColor Yellow
python main.py
"@

Start-Process powershell -ArgumentList "-NoExit", "-Command", $backendScript -WindowStyle Minimized

Write-Host "    ⏳ Waiting for backend to initialize..." -ForegroundColor Gray
Start-Sleep -Seconds 5

Write-Host "    Starting frontend server..." -ForegroundColor Cyan
$frontendScript = @"
cd '$frontendPath'
Write-Host 'Frontend starting on http://localhost:4000' -ForegroundColor Green
Write-Host 'Press Ctrl+C to stop' -ForegroundColor Yellow
npm run dev
"@

Start-Process powershell -ArgumentList "-NoExit", "-Command", $frontendScript -WindowStyle Minimized

Write-Host "    ⏳ Waiting for frontend to initialize..." -ForegroundColor Gray
Start-Sleep -Seconds 8
Write-Host ""

# Open browser
Write-Host "[6/6] Opening application..." -ForegroundColor Yellow
Write-Host "    🌐 Opening Ideas Matter in your default browser..." -ForegroundColor Cyan
Start-Process "http://localhost:4000"

Write-Host ""
Write-Host "========================================"  -ForegroundColor Green
Write-Host "✅ Ideas Matter is now running!"           -ForegroundColor Green
Write-Host "========================================"  -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Application:  http://localhost:4000"    -ForegroundColor Cyan
Write-Host "🔧 Backend API:  http://localhost:8000"    -ForegroundColor Cyan
Write-Host "📚 API Docs:     http://localhost:8000/docs" -ForegroundColor Cyan
Write-Host ""
Write-Host "ℹ️  Both servers are running in separate windows" -ForegroundColor Gray
Write-Host "⏹️  To stop: Close the server windows or run this script again" -ForegroundColor Gray
Write-Host ""
Read-Host "Press Enter to exit this window"