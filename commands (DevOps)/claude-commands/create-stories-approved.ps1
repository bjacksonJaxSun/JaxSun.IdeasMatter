#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to create User Stories with automatic approval.

.DESCRIPTION
    This wrapper script allows Claude to create User Stories by passing
    the approval response automatically.

.EXAMPLE
    ./create-stories-approved.ps1 -FeatureId 10127 -MaxStories 3
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$FeatureId,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxStories = 40,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

$scriptPath = Join-Path $PSScriptRoot "../stories/build-stories.ps1"

if ($Preview) {
    & $scriptPath -FeatureId $FeatureId -MaxStories $MaxStories -Preview
}
else {
    Write-Host "Creating User Stories for Feature: $FeatureId" -ForegroundColor Cyan
    Write-Host "Maximum Stories: $MaxStories" -ForegroundColor Yellow
    Write-Host "Auto-approving creation..." -ForegroundColor Yellow
    
    "Y" | & $scriptPath -FeatureId $FeatureId -MaxStories $MaxStories
}