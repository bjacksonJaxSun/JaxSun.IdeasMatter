#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create a Product Vision with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create a Product Vision by passing
    the approval response automatically.

.EXAMPLE
    ./create-vision-approved.ps1 -VisionName "HR Platform"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$VisionName,
    
    [Parameter(Mandatory=$false)]
    [string]$TemplateFile,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

# Build the command
$scriptPath = Join-Path $PSScriptRoot "../vision/create-product-vision.ps1"
$params = @{
    VisionName = $VisionName
}

if ($TemplateFile) {
    $params.TemplateFile = $TemplateFile
}

if ($Preview) {
    # Preview doesn't need approval
    & $scriptPath @params
}
else {
    # Auto-respond with Y to approval prompt
    Write-Host "Creating Product Vision: $VisionName" -ForegroundColor Cyan
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    # Use PowerShell to pipe the approval
    "Y" | & $scriptPath @params
}