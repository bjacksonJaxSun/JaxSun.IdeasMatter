#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Design Story - [Story Id]

.DESCRIPTION
    Natural language Claude command to create a technical design for a story.
    Analyzes the story, tasks, and test cases to generate comprehensive design documentation.
    Usage: Design-Story.ps1 10024

.EXAMPLE
    ./Design-Story.ps1 10024
    ./Design-Story.ps1 10024 -Preview
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`nDESIGN STORY: #$StoryId" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor DarkGray

$scriptPath = Join-Path $PSScriptRoot "../stories/design-story.ps1"

if ($Preview) {
    Write-Host "Preview Mode - Design will be generated but not attached" -ForegroundColor Yellow
    & $scriptPath -StoryId $StoryId -Preview
}
else {
    Write-Host "Auto-approving design generation and attachment..." -ForegroundColor Green
    & $scriptPath -StoryId $StoryId -AutoApprove
    
    Write-Host "`nNext Steps:" -ForegroundColor Yellow
    Write-Host "   - Review the attached design in Azure DevOps" -ForegroundColor White
    Write-Host "   - Execute story implementation: execute story $StoryId" -ForegroundColor White
    Write-Host "   - Or modify design: Design-Story.ps1 $StoryId -Preview" -ForegroundColor White
}