#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create Features with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create Features by passing
    the approval response automatically.

.EXAMPLE
    ./create-features-approved.ps1 -EpicId 10125 -MaxFeatures 5
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$EpicId,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxFeatures = 40,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

$scriptPath = Join-Path $PSScriptRoot "../features/build-features.ps1"

if ($Preview) {
    & $scriptPath -EpicId $EpicId -MaxFeatures $MaxFeatures -Preview
}
else {
    Write-Host "Creating Features for Epic: $EpicId" -ForegroundColor Cyan
    Write-Host "Maximum Features: $MaxFeatures" -ForegroundColor Yellow
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    "Y" | & $scriptPath -EpicId $EpicId -MaxFeatures $MaxFeatures
}