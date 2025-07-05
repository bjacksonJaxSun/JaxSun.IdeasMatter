#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Strategy - VisionEpicId [ID]

.DESCRIPTION
    Natural language Claude command to create a Vision Strategy.
    Usage: Create-Strategy.ps1 10123

.EXAMPLE
    ./Create-Strategy.ps1 10123
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$VisionEpicId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n📊 CREATE STRATEGY for Vision Epic: $VisionEpicId" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

$scriptPath = Join-Path $PSScriptRoot "../strategy/build-vision-strategy-enhanced.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -VisionEpicId $VisionEpicId -Preview
}
else {
    Write-Host "✅ Auto-approving strategy creation..." -ForegroundColor Green
    "Y" | & $scriptPath -VisionEpicId $VisionEpicId
    
    Write-Host "`n💡 Next Command:" -ForegroundColor Yellow
    Write-Host "   ./Create-Epics.ps1 [STRATEGY_ID]" -ForegroundColor White
    Write-Host "   ./Create-Epics.ps1 [STRATEGY_ID] 3     (limit to 3 epics)" -ForegroundColor Gray
}