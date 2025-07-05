#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Update TestCases - [ID1,ID2,ID3]

.DESCRIPTION
    Natural language Claude command to update existing test cases with proper steps.
    Usage: Update-TestCases.ps1 10103,10104,10105

.EXAMPLE
    ./Update-TestCases.ps1 10103
    ./Update-TestCases.ps1 10103,10104,10105,10106
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$TestCaseIds,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

# Convert comma-separated string to array
$ids = $TestCaseIds -split ',' | ForEach-Object { $_.Trim() }

Write-Host "`n🔄 UPDATE TEST CASES with Steps" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "📋 Test Cases to Update: $($ids -join ', ')" -ForegroundColor White
Write-Host "📝 Will add: Actions and Expected Results" -ForegroundColor White

$scriptPath = Join-Path $PSScriptRoot "../test-cases/update-test-case-steps.ps1"

if ($Preview) {
    Write-Host "`n📋 Preview Mode - No updates will be made" -ForegroundColor Yellow
    & $scriptPath -TestCaseIds $ids -Preview
}
else {
    Write-Host "`n✅ Updating test cases with proper steps..." -ForegroundColor Green
    & $scriptPath -TestCaseIds $ids
    
    Write-Host "`n✨ Test cases updated with detailed steps!" -ForegroundColor Green
}