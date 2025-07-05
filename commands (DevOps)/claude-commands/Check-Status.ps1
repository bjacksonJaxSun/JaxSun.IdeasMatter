#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Check Status - VisionEpicId [ID]

.DESCRIPTION
    Natural language Claude command to check workflow status.
    Usage: Check-Status.ps1
           Check-Status.ps1 10123

.EXAMPLE
    ./Check-Status.ps1
    ./Check-Status.ps1 10123
#>

param(
    [Parameter(Mandatory=$false, Position=0)]
    [string]$VisionEpicId
)

Write-Host "`n📊 WORKFLOW STATUS CHECK" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

$scriptPath = Join-Path $PSScriptRoot "../workflow/workflow-status.ps1"

if ($VisionEpicId) {
    Write-Host "🔍 Checking status for Vision Epic: $VisionEpicId" -ForegroundColor White
    & $scriptPath -VisionEpicId $VisionEpicId
}
else {
    Write-Host "🔍 Checking all workflows..." -ForegroundColor White
    & $scriptPath
}