#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create TestCases - StoryId [ID] [MaxCount]

.DESCRIPTION
    Natural language Claude command to create Test Cases from Story.
    Creates test cases with proper Azure DevOps test steps.
    Usage: Create-TestCases.ps1 10131
           Create-TestCases.ps1 10131 6

.EXAMPLE
    ./Create-TestCases.ps1 10131
    ./Create-TestCases.ps1 10131 10
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false, Position=1)]
    [int]$MaxTestCases = 40,
    
    [Parameter(Mandatory=$false)]
    [string]$TestTypes = "Unit,Integration,E2E",
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n🧪 CREATE TEST CASES for Story: $StoryId" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "📊 Maximum Test Cases: $MaxTestCases" -ForegroundColor White
Write-Host "🔧 Test Types: $TestTypes" -ForegroundColor White

$scriptPath = Join-Path $PSScriptRoot "../test-cases/build-test-cases.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -StoryId $StoryId -MaxTestCases $MaxTestCases -TestTypes $TestTypes -Preview
}
else {
    Write-Host "✅ Auto-approving test case creation..." -ForegroundColor Green
    Write-Host "📝 Test cases will include proper test steps (Actions & Expected Results)" -ForegroundColor Gray
    "Y" | & $scriptPath -StoryId $StoryId -MaxTestCases $MaxTestCases -TestTypes $TestTypes
    
    Write-Host "`n💡 Also create tasks:" -ForegroundColor Yellow
    Write-Host "   ./Create-Tasks.ps1 $StoryId" -ForegroundColor White
}