#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create Epics with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create Epics by passing
    the approval response automatically.

.EXAMPLE
    ./create-epics-approved.ps1 -StrategyEpicId 10124 -MaxEpics 3
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StrategyEpicId,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxEpics = 20,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

$scriptPath = Join-Path $PSScriptRoot "../epics/build-epics.ps1"

if ($Preview) {
    & $scriptPath -StrategyEpicId $StrategyEpicId -MaxEpics $MaxEpics -Preview
}
else {
    Write-Host "Creating Epics for Strategy: $StrategyEpicId" -ForegroundColor Cyan
    Write-Host "Maximum Epics: $MaxEpics" -ForegroundColor Yellow
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    "Y" | & $scriptPath -StrategyEpicId $StrategyEpicId -MaxEpics $MaxEpics
}