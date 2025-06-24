# ========================================
# Ideas Matter Development Shutdown Script (PowerShell)
# Stops backend and frontend servers safely
# ========================================

Write-Host "Stopping Ideas Matter Development Servers..." -ForegroundColor Yellow
Write-Host ""

# Function to stop process on a specific port
function Stop-ProcessOnPort {
    param($Port, $ServiceName)
    
    $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    
    if ($connections) {
        foreach ($conn in $connections) {
            $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Stopping $ServiceName server (PID: $($process.Id), Name: $($process.Name))..." -ForegroundColor Cyan
                Stop-Process -Id $process.Id -Force
            }
        }
        Write-Host "$ServiceName server stopped" -ForegroundColor Green
    } else {
        Write-Host "$ServiceName server not found on port $Port" -ForegroundColor Gray
    }
}

# Stop backend on port 8000
Stop-ProcessOnPort -Port 8000 -ServiceName "Backend"

# Stop frontend on port 4000  
Stop-ProcessOnPort -Port 4000 -ServiceName "Frontend"

# Also try to stop by window title as backup
$backendWindows = Get-Process | Where-Object { $_.MainWindowTitle -like "Ideas Matter Backend*" }
foreach ($window in $backendWindows) {
    Write-Host "Stopping backend window (PID: $($window.Id))..." -ForegroundColor Cyan
    Stop-Process -Id $window.Id -Force -ErrorAction SilentlyContinue
}

$frontendWindows = Get-Process | Where-Object { $_.MainWindowTitle -like "Ideas Matter Frontend*" }
foreach ($window in $frontendWindows) {
    Write-Host "Stopping frontend window (PID: $($window.Id))..." -ForegroundColor Cyan
    Stop-Process -Id $window.Id -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "========================================"  -ForegroundColor Green
Write-Host "Development servers stopped successfully" -ForegroundColor Green
Write-Host "========================================"  -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"