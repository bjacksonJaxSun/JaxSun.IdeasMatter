#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Stories - FeatureId [ID] [MaxCount]

.DESCRIPTION
    Natural language Claude command to create User Stories from Feature.
    Usage: Create-Stories.ps1 10128
           Create-Stories.ps1 10128 3

.EXAMPLE
    ./Create-Stories.ps1 10128
    ./Create-Stories.ps1 10128 5
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$FeatureId,
    
    [Parameter(Mandatory=$false, Position=1)]
    [int]$MaxStories = 40,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n📝 CREATE USER STORIES for Feature: $FeatureId" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "📊 Maximum Stories: $MaxStories" -ForegroundColor White

$scriptPath = Join-Path $PSScriptRoot "../stories/build-stories.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -FeatureId $FeatureId -MaxStories $MaxStories -Preview
}
else {
    Write-Host "✅ Auto-approving story creation..." -ForegroundColor Green
    "Y" | & $scriptPath -FeatureId $FeatureId -MaxStories $MaxStories
    
    Write-Host "`n💡 Next Commands:" -ForegroundColor Yellow
    Write-Host "   ./Create-TestCases.ps1 [STORY_ID]" -ForegroundColor White
    Write-Host "   ./Create-Tasks.ps1 [STORY_ID]" -ForegroundColor White
    Write-Host "   ./Create-TestCases.ps1 [STORY_ID] 6     (limit to 6 test cases)" -ForegroundColor Gray
}