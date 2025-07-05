#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Implement Story - [Story Id]

.DESCRIPTION
    Natural language Claude command to implement a story based on its technical design.
    Ensures technical design exists, implements using TDD approach, and updates Azure DevOps.
    Usage: Implement-Story.ps1 10024

.EXAMPLE
    ./Implement-Story.ps1 10024
    ./Implement-Story.ps1 10024 -SkipTests
    ./Implement-Story.ps1 10024 -DryRun
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

Write-Host "`nIMPLEMENT STORY: #$StoryId" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor DarkGray

$scriptPath = Join-Path $PSScriptRoot "../stories/implement-story.ps1"

if ($DryRun) {
    Write-Host "Dry Run Mode - No changes will be made" -ForegroundColor Yellow
    & $scriptPath -StoryId $StoryId -DryRun
}
elseif ($SkipTests) {
    Write-Host "Skip Tests Mode - Will implement without running tests" -ForegroundColor Yellow
    & $scriptPath -StoryId $StoryId -SkipTests
}
else {
    Write-Host "Full Implementation Mode - Following TDD approach..." -ForegroundColor Green
    & $scriptPath -StoryId $StoryId
    
    Write-Host "`nNext Steps:" -ForegroundColor Yellow
    Write-Host "   - Review implementation in your IDE" -ForegroundColor White
    Write-Host "   - Check test results in Azure DevOps" -ForegroundColor White
    Write-Host "   - Create PR: gh pr create" -ForegroundColor White
}