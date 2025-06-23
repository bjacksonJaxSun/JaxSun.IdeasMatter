# Ideas Matter Development Stopper (PowerShell)
Write-Host "================================================" -ForegroundColor Red
Write-Host "          Stopping Ideas Matter Development" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Red
Write-Host ""

Write-Host "Stopping all Node.js processes (Frontend)..." -ForegroundColor Yellow
Get-Process -Name "node" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Stopping all Python processes (Backend)..." -ForegroundColor Yellow  
Get-Process -Name "python" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Stopping PowerShell windows running dev servers..." -ForegroundColor Yellow
# This will close other PowerShell windows that might be running the servers
# Be careful as it might close other PowerShell instances
$processes = Get-WmiObject Win32_Process | Where-Object { $_.CommandLine -like "*python main.py*" -or $_.CommandLine -like "*npm run dev*" }
$processes | ForEach-Object { $_.Terminate() }

Write-Host ""
Write-Host "✅ All development services stopped!" -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"