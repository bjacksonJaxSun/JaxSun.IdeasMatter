#!/usr/bin/env pwsh
# PowerShell script to run GitHub workflow

Write-Host "==========================================" -ForegroundColor Blue
Write-Host "Running Ideas Matter Vision Creation" -ForegroundColor Blue
Write-Host "==========================================" -ForegroundColor Blue
Write-Host ""

# Change to repository directory
Set-Location "C:\Development\Jackson.Ideas"

# Check if gh is installed
$ghPath = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghPath) {
    Write-Host "ERROR: GitHub CLI not found in PATH" -ForegroundColor Red
    Write-Host "Please install from: https://cli.github.com/" -ForegroundColor Yellow
    Write-Host "Or add it to your PATH if already installed" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check authentication
Write-Host "Checking GitHub authentication..." -ForegroundColor Cyan
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Not authenticated with GitHub" -ForegroundColor Red
    Write-Host "Please run: gh auth login" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Authenticated successfully!" -ForegroundColor Green
Write-Host ""

# Run the workflow
Write-Host "Triggering GitHub workflow..." -ForegroundColor Cyan
$result = gh workflow run create-vision.yml `
    -f product_name="Ideas Matter" `
    -f vision_file_path="Commands/docs/visions/ideas-matter/vision.md" `
    -f preview=false 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "SUCCESS! Workflow triggered." -ForegroundColor Green
    Write-Host ""
    
    # Wait a moment for the workflow to register
    Start-Sleep -Seconds 3
    
    # Get the latest run URL
    Write-Host "Getting workflow URL..." -ForegroundColor Cyan
    $runInfo = gh run list --workflow=create-vision.yml --limit=1 --json url,status,databaseId | ConvertFrom-Json
    
    if ($runInfo -and $runInfo.Count -gt 0) {
        $latestRun = $runInfo[0]
        Write-Host ""
        Write-Host "Workflow Information:" -ForegroundColor Yellow
        Write-Host "  Run ID: $($latestRun.databaseId)" -ForegroundColor White
        Write-Host "  Status: $($latestRun.status)" -ForegroundColor White
        Write-Host "  URL: $($latestRun.url)" -ForegroundColor White
        Write-Host ""
        
        # Open in browser
        Write-Host "Opening workflow in browser..." -ForegroundColor Cyan
        Start-Process $latestRun.url
        
        Write-Host ""
        Write-Host "The workflow will create:" -ForegroundColor Green
        Write-Host "  - Vision Issue (pinned to repository)" -ForegroundColor White
        Write-Host "  - GitHub Project for Ideas Matter" -ForegroundColor White
        Write-Host "  - All necessary labels" -ForegroundColor White
        Write-Host ""
        Write-Host "Watch the progress in your browser!" -ForegroundColor Yellow
    }
} else {
    Write-Host "ERROR: Failed to trigger workflow" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Check authentication: gh auth status" -ForegroundColor White
    Write-Host "2. Verify repository: gh repo view" -ForegroundColor White
    Write-Host "3. List workflows: gh workflow list" -ForegroundColor White
}

Write-Host ""
Read-Host "Press Enter to exit"