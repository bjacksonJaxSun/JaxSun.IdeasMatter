#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create Tasks with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create Tasks by passing
    the approval response automatically.

.EXAMPLE
    ./create-tasks-approved.ps1 -StoryId 10130 -MaxTasks 6
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTasks = 40,
    
    [Parameter(Mandatory=$false)]
    [string]$TaskTypes = "Development,Testing,Documentation",
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

$scriptPath = Join-Path $PSScriptRoot "../tasks/build-tasks.ps1"

if ($Preview) {
    & $scriptPath -StoryId $StoryId -MaxTasks $MaxTasks -TaskTypes $TaskTypes -Preview
}
else {
    Write-Host "Creating Tasks for Story: $StoryId" -ForegroundColor Cyan
    Write-Host "Maximum Tasks: $MaxTasks" -ForegroundColor Yellow
    Write-Host "Task Types: $TaskTypes" -ForegroundColor Yellow
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    "Y" | & $scriptPath -StoryId $StoryId -MaxTasks $MaxTasks -TaskTypes $TaskTypes
}