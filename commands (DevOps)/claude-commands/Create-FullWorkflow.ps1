#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create FullWorkflow - [Product Name] [Limits]

.DESCRIPTION
    Natural language Claude command to create complete workflow.
    Usage: Create-FullWorkflow.ps1 "HR Portal"
           Create-FullWorkflow.ps1 "HR Portal" 2 3 2

.EXAMPLE
    ./Create-FullWorkflow.ps1 "Employee Portal"
    ./Create-FullWorkflow.ps1 "Employee Portal" 2 3 3
    # Creates: 2 epics, 3 features per epic, 3 stories per feature
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$ProductName,
    
    [Parameter(Mandatory=$false, Position=1)]
    [int]$MaxEpics = 5,
    
    [Parameter(Mandatory=$false, Position=2)]
    [int]$MaxFeatures = 5,
    
    [Parameter(Mandatory=$false, Position=3)]
    [int]$MaxStories = 5,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTestCases = 10,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTasks = 10,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "`n🚀 CREATE FULL WORKFLOW: $ProductName" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host ""
Write-Host "📊 Hierarchy to Create:" -ForegroundColor Yellow
Write-Host "   • 1 Product Vision" -ForegroundColor White
Write-Host "   • 1 Vision Strategy" -ForegroundColor White
Write-Host "   • $MaxEpics Epics" -ForegroundColor White
Write-Host "   • $MaxFeatures Features per Epic" -ForegroundColor White
Write-Host "   • $MaxStories Stories per Feature" -ForegroundColor White
Write-Host "   • $MaxTestCases Test Cases per Story" -ForegroundColor White
Write-Host "   • $MaxTasks Tasks per Story" -ForegroundColor White
Write-Host ""

# Calculate totals
$totalFeatures = $MaxEpics * $MaxFeatures
$totalStories = $totalFeatures * $MaxStories
$totalTestCases = $totalStories * $MaxTestCases
$totalTasks = $totalStories * $MaxTasks
$totalItems = 2 + $MaxEpics + $totalFeatures + $totalStories + $totalTestCases + $totalTasks

Write-Host "📈 Total Items: ~$totalItems work items" -ForegroundColor Magenta
Write-Host ""

if ($Preview) {
    Write-Host "📋 Preview Mode - No items will be created" -ForegroundColor Yellow
    $scriptPath = Join-Path $PSScriptRoot "../workflow/execute-full-workflow.ps1"
    & $scriptPath -VisionName $ProductName -MaxEpics $MaxEpics -MaxFeatures $MaxFeatures -MaxStories $MaxStories -MaxTestCases $MaxTestCases -MaxTasks $MaxTasks -Preview
}
else {
    Write-Host "✅ Starting automated workflow creation..." -ForegroundColor Green
    Write-Host "⏱️  This may take several minutes..." -ForegroundColor Gray
    Write-Host ""
    
    $scriptPath = Join-Path $PSScriptRoot "../workflow/execute-full-workflow.ps1"
    & $scriptPath -VisionName $ProductName -MaxEpics $MaxEpics -MaxFeatures $MaxFeatures -MaxStories $MaxStories -MaxTestCases $MaxTestCases -MaxTasks $MaxTasks -AutoApprove
    
    Write-Host "`n✨ Workflow creation complete!" -ForegroundColor Green
}