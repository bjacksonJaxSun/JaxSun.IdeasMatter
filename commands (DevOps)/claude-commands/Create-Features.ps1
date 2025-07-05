#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Features - EpicId [ID] [MaxCount]

.DESCRIPTION
    Natural language Claude command to create Features from Epic.
    Usage: Create-Features.ps1 10125
           Create-Features.ps1 10125 5

.EXAMPLE
    ./Create-Features.ps1 10125
    ./Create-Features.ps1 10125 3
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$EpicId,
    
    [Parameter(Mandatory=$false, Position=1)]
    [int]$MaxFeatures = 40,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n✨ CREATE FEATURES for Epic: $EpicId" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "📊 Maximum Features: $MaxFeatures" -ForegroundColor White

$scriptPath = Join-Path $PSScriptRoot "../features/build-features.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -EpicId $EpicId -MaxFeatures $MaxFeatures -Preview
}
else {
    Write-Host "✅ Auto-approving feature creation..." -ForegroundColor Green
    "Y" | & $scriptPath -EpicId $EpicId -MaxFeatures $MaxFeatures
    
    Write-Host "`n💡 Next Command:" -ForegroundColor Yellow
    Write-Host "   ./Create-Stories.ps1 [FEATURE_ID]" -ForegroundColor White
    Write-Host "   ./Create-Stories.ps1 [FEATURE_ID] 3     (limit to 3 stories)" -ForegroundColor Gray
}