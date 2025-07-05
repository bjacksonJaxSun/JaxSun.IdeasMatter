#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Strategy - VisionEpicId [ID] - With Approval Process

.DESCRIPTION
    Natural language Claude command to create a Vision Strategy with approval.
    This version shows the approval process instead of auto-approving.
    Usage: Create-Strategy-WithApproval.ps1 10123

.EXAMPLE
    ./Create-Strategy-WithApproval.ps1 10123
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$VisionEpicId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove
)

Write-Host "`n[STRATEGY] CREATE STRATEGY for Vision Epic: $VisionEpicId" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host ""
Write-Host "[INFO] This command will guide you through the approval process" -ForegroundColor Yellow
Write-Host ""

$scriptPath = Join-Path $PSScriptRoot "../strategy/build-vision-strategy-enhanced-v2.ps1"

if ($Preview) {
    Write-Host "[PREVIEW] Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -VisionEpicId $VisionEpicId -Preview
}
elseif ($AutoApprove) {
    Write-Host "[AUTO] Auto-approving strategy creation..." -ForegroundColor Green
    "Y" | & $scriptPath -VisionEpicId $VisionEpicId
}
else {
    Write-Host "[APPROVAL] Running with manual approval process..." -ForegroundColor Magenta
    Write-Host ""
    Write-Host "You will be prompted to review and approve:" -ForegroundColor White
    Write-Host "  1. Vision details and metrics" -ForegroundColor Gray
    Write-Host "  2. Generated strategy content" -ForegroundColor Gray
    Write-Host "  3. Final creation confirmation" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Press Enter to continue..." -ForegroundColor Yellow
    Read-Host
    
    # Run the script without auto-approval
    & $scriptPath -VisionEpicId $VisionEpicId
    
    Write-Host "`n[NEXT] Next Command:" -ForegroundColor Yellow
    Write-Host "   ./Create-Epics.ps1 [STRATEGY_ID]" -ForegroundColor White
    Write-Host "   ./Create-Epics.ps1 [STRATEGY_ID] 3     (limit to 3 epics)" -ForegroundColor Gray
}