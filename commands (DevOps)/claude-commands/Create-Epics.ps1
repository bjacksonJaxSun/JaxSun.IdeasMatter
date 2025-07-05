#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Epics - StrategyEpicId [ID] [MaxCount]

.DESCRIPTION
    Natural language Claude command to create Epics from Strategy.
    Usage: Create-Epics.ps1 10124
           Create-Epics.ps1 10124 3

.EXAMPLE
    ./Create-Epics.ps1 10124
    ./Create-Epics.ps1 10124 5
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$StrategyEpicId,
    
    [Parameter(Mandatory=$false, Position=1)]
    [int]$MaxEpics = 20,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n📚 CREATE EPICS for Strategy: $StrategyEpicId" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "📊 Maximum Epics: $MaxEpics" -ForegroundColor White

$scriptPath = Join-Path $PSScriptRoot "../epics/build-epics.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -StrategyEpicId $StrategyEpicId -MaxEpics $MaxEpics -Preview
}
else {
    Write-Host "✅ Auto-approving epic creation..." -ForegroundColor Green
    "Y" | & $scriptPath -StrategyEpicId $StrategyEpicId -MaxEpics $MaxEpics
    
    Write-Host "`n💡 Next Command:" -ForegroundColor Yellow
    Write-Host "   ./Create-Features.ps1 [EPIC_ID]" -ForegroundColor White
    Write-Host "   ./Create-Features.ps1 [EPIC_ID] 5     (limit to 5 features)" -ForegroundColor Gray
}