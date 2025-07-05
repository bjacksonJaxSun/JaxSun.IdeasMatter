#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Claude command to execute the full workflow with automatic approvals.

.DESCRIPTION
    This wrapper script allows Claude to execute the complete workflow
    by passing approval responses automatically at each step.

.EXAMPLE
    ./execute-full-workflow-approved.ps1 -VisionName "New Product" -MaxEpics 2 -MaxFeatures 3 -MaxStories 2
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$VisionName,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxEpics = 5,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxFeatures = 5,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxStories = 5,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTestCases = 10,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTasks = 10,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

Write-Host "=== CLAUDE WORKFLOW EXECUTION ===" -ForegroundColor Cyan
Write-Host "Product: $VisionName" -ForegroundColor Yellow
Write-Host "This will create:" -ForegroundColor Yellow
Write-Host "  - 1 Product Vision" -ForegroundColor White
Write-Host "  - 1 Vision Strategy" -ForegroundColor White
Write-Host "  - Up to $MaxEpics Epics" -ForegroundColor White
Write-Host "  - Up to $MaxFeatures Features per Epic" -ForegroundColor White
Write-Host "  - Up to $MaxStories Stories per Feature" -ForegroundColor White
Write-Host "  - Up to $MaxTestCases Test Cases per Story" -ForegroundColor White
Write-Host "  - Up to $MaxTasks Tasks per Story" -ForegroundColor White
Write-Host ""

if ($Preview) {
    Write-Host "PREVIEW MODE - No items will be created" -ForegroundColor Yellow
    & (Join-Path $PSScriptRoot "../workflow/execute-full-workflow.ps1") @PSBoundParameters
}
else {
    Write-Host "Starting workflow with automatic approvals..." -ForegroundColor Green
    Write-Host ""
    
    # Create a temporary script that will handle all the approvals
    $tempScript = @"
# Auto-approve all prompts
while (`$true) {
    Start-Sleep -Milliseconds 100
    Write-Output "Y"
}
"@
    
    $tempFile = New-TemporaryFile
    $tempScript | Out-File -FilePath $tempFile.FullName -Encoding UTF8
    
    try {
        # Run the workflow with auto-approvals
        $workflowScript = Join-Path $PSScriptRoot "../workflow/execute-full-workflow.ps1"
        
        # Use a different approach - call with AutoApprove for each step
        & $workflowScript -VisionName $VisionName `
            -MaxEpics $MaxEpics `
            -MaxFeatures $MaxFeatures `
            -MaxStories $MaxStories `
            -MaxTestCases $MaxTestCases `
            -MaxTasks $MaxTasks `
            -AutoApprove
    }
    finally {
        Remove-Item -Path $tempFile.FullName -Force -ErrorAction SilentlyContinue
    }
}