#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create Tasks - StoryId [ID] [MaxCount]

.DESCRIPTION
    Natural language Claude command to create Tasks from Story.
    Usage: Create-Tasks.ps1 10131
           Create-Tasks.ps1 10131 6

.EXAMPLE
    ./Create-Tasks.ps1 10131
    ./Create-Tasks.ps1 10131 8
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false, Position=1)]
    [int]$MaxTasks = 40,
    
    [Parameter(Mandatory=$false)]
    [string]$TaskTypes = "Development,Testing,Documentation",
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n⚡ CREATE TASKS for Story: $StoryId" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host "📊 Maximum Tasks: $MaxTasks" -ForegroundColor White
Write-Host "🔧 Task Types: $TaskTypes" -ForegroundColor White

$scriptPath = Join-Path $PSScriptRoot "../tasks/build-tasks.ps1"

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    & $scriptPath -StoryId $StoryId -MaxTasks $MaxTasks -TaskTypes $TaskTypes -Preview
}
else {
    Write-Host "✅ Auto-approving task creation..." -ForegroundColor Green
    "Y" | & $scriptPath -StoryId $StoryId -MaxTasks $MaxTasks -TaskTypes $TaskTypes
    
    Write-Host "`n✨ Story complete with Test Cases and Tasks!" -ForegroundColor Green
}