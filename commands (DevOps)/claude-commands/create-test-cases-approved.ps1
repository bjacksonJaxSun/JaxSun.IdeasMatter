#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create Test Cases with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create Test Cases with proper steps
    by passing the approval response automatically.

.EXAMPLE
    ./create-test-cases-approved.ps1 -StoryId 10130 -MaxTestCases 6
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTestCases = 40,
    
    [Parameter(Mandatory=$false)]
    [string]$TestTypes = "Unit,Integration,E2E",
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

$scriptPath = Join-Path $PSScriptRoot "../test-cases/build-test-cases.ps1"

if ($Preview) {
    & $scriptPath -StoryId $StoryId -MaxTestCases $MaxTestCases -TestTypes $TestTypes -Preview
}
else {
    Write-Host "Creating Test Cases for Story: $StoryId" -ForegroundColor Cyan
    Write-Host "Maximum Test Cases: $MaxTestCases" -ForegroundColor Yellow
    Write-Host "Test Types: $TestTypes" -ForegroundColor Yellow
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    "Y" | & $scriptPath -StoryId $StoryId -MaxTestCases $MaxTestCases -TestTypes $TestTypes
}