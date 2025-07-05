#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Vision - [Product Name]

.DESCRIPTION
    Natural language Claude command to create a Product Vision.
    Usage: Create-Vision.ps1 "HR Platform"

.EXAMPLE
    ./Create-Vision.ps1 "Employee Portal"
    ./Create-Vision.ps1 "Customer Support System"
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$ProductName,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n🎯 CREATE VISION: $ProductName" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

$scriptPath = Join-Path $PSScriptRoot "../vision/create-product-vision.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -VisionName $ProductName -Preview
}
else {
    Write-Host "✅ Auto-approving vision creation..." -ForegroundColor Green
    "Y" | & $scriptPath -VisionName $ProductName
    
    Write-Host "`n💡 Next Command:" -ForegroundColor Yellow
    Write-Host "   ./Create-Strategy.ps1 [VISION_ID]" -ForegroundColor White
}