#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create a Vision Strategy with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create a Vision Strategy by passing
    the approval response automatically.

.EXAMPLE
    ./create-strategy-approved.ps1 -VisionEpicId 10123
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$VisionEpicId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

$scriptPath = Join-Path $PSScriptRoot "../strategy/build-vision-strategy-enhanced.ps1"

if ($Preview) {
    & $scriptPath -VisionEpicId $VisionEpicId -Preview
}
else {
    Write-Host "Creating Vision Strategy for Epic: $VisionEpicId" -ForegroundColor Cyan
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    "Y" | & $scriptPath -VisionEpicId $VisionEpicId
}