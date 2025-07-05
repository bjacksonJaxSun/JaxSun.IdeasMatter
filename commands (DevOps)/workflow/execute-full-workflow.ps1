#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Executes the complete product development workflow from Vision to Tasks.

.DESCRIPTION
    This script orchestrates the entire product development workflow, executing each step
    in sequence with approval gates. It can start from any point in the hierarchy.

.PARAMETER StartFrom
    Where to start the workflow: Vision, Strategy, Epic, Feature, Story
    Default: Vision

.PARAMETER VisionName
    Required if StartFrom is Vision. The name of the product/system.

.PARAMETER ParentId
    Required if StartFrom is not Vision. The ID of the parent work item.

.PARAMETER StopAt
    Where to stop the workflow: Vision, Strategy, Epics, Features, Stories, TestCases, Tasks
    Default: Tasks (complete workflow)

.PARAMETER AutoApprove
    Skip all approval prompts (use with extreme caution).

.PARAMETER DryRun
    Execute entire workflow in preview mode without creating anything.

.PARAMETER MaxItemsPerLevel
    Maximum items to create at each level (default: 10)

.EXAMPLE
    ./execute-full-workflow.ps1 -VisionName "Bermuda Payroll" -DryRun
    ./execute-full-workflow.ps1 -StartFrom Epic -ParentId 10088 -StopAt Stories
    ./execute-full-workflow.ps1 -VisionName "AI Analytics" -AutoApprove -MaxItemsPerLevel 5
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Vision", "Strategy", "Epic", "Feature", "Story")]
    [string]$StartFrom = "Vision",
    
    [Parameter(Mandatory=$false)]
    [string]$VisionName,
    
    [Parameter(Mandatory=$false)]
    [string]$ParentId,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Vision", "Strategy", "Epics", "Features", "Stories", "TestCases", "Tasks")]
    [string]$StopAt = "Tasks",
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxItemsPerLevel = 10
)

# Validate parameters
if ($StartFrom -eq "Vision" -and -not $VisionName) {
    Write-Host "Error: VisionName is required when starting from Vision" -ForegroundColor Red
    exit 1
}
if ($StartFrom -ne "Vision" -and -not $ParentId) {
    Write-Host "Error: ParentId is required when not starting from Vision" -ForegroundColor Red
    exit 1
}

# Get script directory (commands folder)
$scriptDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

# Workflow state tracking
$workflowState = @{
    StartTime = Get-Date
    CurrentPhase = $StartFrom
    CreatedItems = @{
        Vision = @()
        Strategy = @()
        Epics = @()
        Features = @()
        Stories = @()
        TestCases = @()
        Tasks = @()
    }
    Errors = @()
    Warnings = @()
}

# Function to log workflow event
function Write-WorkflowLog {
    param(
        [string]$Phase,
        [string]$Message,
        [string]$Type = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Info" { "Cyan" }
        default { "White" }
    }
    
    Write-Host "[$timestamp] [$Phase] $Message" -ForegroundColor $color
    
    # Add to workflow state
    if ($Type -eq "Error") {
        $workflowState.Errors += "${Phase}: ${Message}"
    }
    elseif ($Type -eq "Warning") {
        $workflowState.Warnings += "${Phase}: ${Message}"
    }
}

# Function to execute command with error handling
function Invoke-WorkflowCommand {
    param(
        [string]$Command,
        [hashtable]$Parameters,
        [string]$Phase
    )
    
    try {
        Write-WorkflowLog -Phase $Phase -Message "Executing: $Command" -Type "Info"
        
        # Build parameter string
        $paramString = ""
        foreach ($key in $Parameters.Keys) {
            if ($Parameters[$key] -is [switch]) {
                if ($Parameters[$key]) {
                    $paramString += " -$key"
                }
            }
            else {
                $paramString += " -$key `"$($Parameters[$key])`""
            }
        }
        
        # Execute command
        $result = Invoke-Expression "$Command $paramString"
        
        Write-WorkflowLog -Phase $Phase -Message "Command completed successfully" -Type "Success"
        return $result
    }
    catch {
        Write-WorkflowLog -Phase $Phase -Message "Command failed: $_" -Type "Error"
        throw
    }
}

# Function to parse created items from command output
function Get-CreatedItems {
    param([string]$Output, [string]$ItemType)
    
    $items = @()
    $pattern = switch ($ItemType) {
        "Epic" { "Epic #(\d+)" }
        "Feature" { "Feature #(\d+)" }
        "Story" { "Story #(\d+)" }
        "TestCase" { "Test Case #(\d+)" }
        "Task" { "Task #(\d+)" }
        default { "#(\d+)" }
    }
    
    $matches = [regex]::Matches($Output, $pattern)
    foreach ($match in $matches) {
        $items += $match.Groups[1].Value
    }
    
    return $items
}

# Function to show workflow summary
function Show-WorkflowSummary {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "WORKFLOW EXECUTION SUMMARY" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    $duration = (Get-Date) - $workflowState.StartTime
    Write-Host "Total Duration: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor White
    Write-Host "Mode: $(if ($DryRun) { 'DRY RUN' } else { 'LIVE' })" -ForegroundColor $(if ($DryRun) { 'Yellow' } else { 'Green' })
    
    Write-Host "`nItems Created:" -ForegroundColor Cyan
    foreach ($level in $workflowState.CreatedItems.Keys) {
        $count = $workflowState.CreatedItems[$level].Count
        if ($count -gt 0) {
            Write-Host "  ${level}: $count items" -ForegroundColor White
        }
    }
    
    if ($workflowState.Warnings.Count -gt 0) {
        Write-Host "`nWarnings ($($workflowState.Warnings.Count)):" -ForegroundColor Yellow
        foreach ($warning in $workflowState.Warnings) {
            Write-Host "  - $warning" -ForegroundColor Yellow
        }
    }
    
    if ($workflowState.Errors.Count -gt 0) {
        Write-Host "`nErrors ($($workflowState.Errors.Count)):" -ForegroundColor Red
        foreach ($error in $workflowState.Errors) {
            Write-Host "  - $error" -ForegroundColor Red
        }
    }
    
    # Save workflow report
    $reportFile = Join-Path -Path $scriptDir -ChildPath "docs/workflow-reports/workflow-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $reportDir = Split-Path $reportFile -Parent
    if (-not (Test-Path $reportDir)) {
        New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    }
    
    $workflowState | ConvertTo-Json -Depth 10 | Set-Content -Path $reportFile
    Write-Host "`nWorkflow report saved to: $reportFile" -ForegroundColor Gray
}

# Function to prompt for continuation
function Get-ContinueApproval {
    param([string]$NextPhase)
    
    if ($AutoApprove) {
        return $true
    }
    
    Write-Host "`nReady to proceed to: $NextPhase" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No  [S] Skip this phase" -ForegroundColor White
    
    $response = Read-Host "Choice"
    return $response -match '^[Yy]'
}

# Main workflow execution
Write-Host "`n=== PRODUCT DEVELOPMENT WORKFLOW ORCHESTRATOR ===" -ForegroundColor Cyan
Write-Host "Start From: $StartFrom" -ForegroundColor White
Write-Host "Stop At: $StopAt" -ForegroundColor White
Write-Host "Max Items Per Level: $MaxItemsPerLevel" -ForegroundColor White
Write-Host "Auto-Approve: $(if ($AutoApprove) { 'YES' } else { 'NO' })" -ForegroundColor $(if ($AutoApprove) { 'Yellow' } else { 'White' })
Write-Host "Mode: $(if ($DryRun) { 'DRY RUN' } else { 'LIVE' })" -ForegroundColor $(if ($DryRun) { 'Yellow' } else { 'Green' })
Write-Host "=================================================" -ForegroundColor Cyan

try {
    # Phase 1: Product Vision
    if ($StartFrom -eq "Vision") {
        if ($StopAt -eq "Vision") {
            $stopAfterVision = $true
        }
        
        Write-WorkflowLog -Phase "Vision" -Message "Creating Product Vision for $VisionName" -Type "Info"
        
        $visionParams = @{
            VisionName = $VisionName
        }
        if ($DryRun) { $visionParams.Preview = $true }
        if ($AutoApprove) { $visionParams.AutoApprove = $true }
        
        $visionCmd = Join-Path -Path $scriptDir -ChildPath "commands/vision/create-product-vision.ps1"
        $visionResult = Invoke-WorkflowCommand -Command $visionCmd -Parameters $visionParams -Phase "Vision"
        
        # Extract Vision ID from output (this is simplified, real parsing would be more robust)
        if ($visionResult -match "Epic ID: (\d+)") {
            $visionId = $Matches[1]
            $workflowState.CreatedItems.Vision += $visionId
            $ParentId = $visionId
            Write-WorkflowLog -Phase "Vision" -Message "Created Vision Epic #$visionId" -Type "Success"
        }
        
        if ($stopAfterVision) {
            Write-WorkflowLog -Phase "Vision" -Message "Stopping at Vision as requested" -Type "Info"
            Show-WorkflowSummary
            exit 0
        }
    }
    
    # Phase 2: Vision Strategy
    if ($StartFrom -in @("Vision", "Strategy") -and $StopAt -ne "Vision") {
        if (-not (Get-ContinueApproval -NextPhase "Vision Strategy")) {
            Write-WorkflowLog -Phase "Strategy" -Message "Skipped by user" -Type "Warning"
        }
        else {
            Write-WorkflowLog -Phase "Strategy" -Message "Creating Vision Strategy" -Type "Info"
            
            $strategyParams = @{
                VisionEpicId = $ParentId
            }
            if ($DryRun) { $strategyParams.Preview = $true }
            if ($AutoApprove) { $strategyParams.AutoApprove = $true }
            
            $strategyCmd = Join-Path -Path $scriptDir -ChildPath "commands/strategy/build-vision-strategy-enhanced.ps1"
            $strategyResult = Invoke-WorkflowCommand -Command $strategyCmd -Parameters $strategyParams -Phase "Strategy"
            
            if ($strategyResult -match "Strategy Epic ID: (\d+)") {
                $strategyId = $Matches[1]
                $workflowState.CreatedItems.Strategy += $strategyId
                $ParentId = $strategyId
                Write-WorkflowLog -Phase "Strategy" -Message "Created Strategy Epic #$strategyId" -Type "Success"
            }
        }
        
        if ($StopAt -eq "Strategy") {
            Write-WorkflowLog -Phase "Strategy" -Message "Stopping at Strategy as requested" -Type "Info"
            Show-WorkflowSummary
            exit 0
        }
    }
    
    # Phase 3: Epics
    if ($StartFrom -in @("Vision", "Strategy", "Epic") -and $StopAt -notin @("Vision", "Strategy")) {
        if (-not (Get-ContinueApproval -NextPhase "Epics")) {
            Write-WorkflowLog -Phase "Epics" -Message "Skipped by user" -Type "Warning"
        }
        else {
            Write-WorkflowLog -Phase "Epics" -Message "Creating Epics from Strategy" -Type "Info"
            
            $epicsParams = @{
                StrategyEpicId = $ParentId
                MaxEpics = $MaxItemsPerLevel
            }
            if ($DryRun) { $epicsParams.Preview = $true }
            if ($AutoApprove) { $epicsParams.AutoApprove = $true }
            
            $epicsCmd = Join-Path -Path $scriptDir -ChildPath "commands/epics/build-epics.ps1"
            $epicsResult = Invoke-WorkflowCommand -Command $epicsCmd -Parameters $epicsParams -Phase "Epics"
            
            # Parse created epic IDs
            $epicIds = Get-CreatedItems -Output $epicsResult -ItemType "Epic"
            $workflowState.CreatedItems.Epics += $epicIds
            Write-WorkflowLog -Phase "Epics" -Message "Created $($epicIds.Count) Epics" -Type "Success"
        }
        
        if ($StopAt -eq "Epics") {
            Write-WorkflowLog -Phase "Epics" -Message "Stopping at Epics as requested" -Type "Info"
            Show-WorkflowSummary
            exit 0
        }
    }
    
    # Phase 4: Features
    if ($StartFrom -in @("Vision", "Strategy", "Epic", "Feature") -and $StopAt -notin @("Vision", "Strategy", "Epics")) {
        if ($workflowState.CreatedItems.Epics.Count -eq 0 -and $StartFrom -ne "Feature") {
            Write-WorkflowLog -Phase "Features" -Message "No epics to create features from" -Type "Warning"
        }
        else {
            $epicsList = if ($StartFrom -eq "Feature") { @($ParentId) } else { $workflowState.CreatedItems.Epics }
            
            foreach ($epicId in $epicsList[0..([Math]::Min(3, $epicsList.Count - 1))]) {  # Process first 3 epics
                if (-not (Get-ContinueApproval -NextPhase "Features for Epic #$epicId")) {
                    Write-WorkflowLog -Phase "Features" -Message "Skipped features for Epic #$epicId" -Type "Warning"
                    continue
                }
                
                Write-WorkflowLog -Phase "Features" -Message "Creating Features for Epic #$epicId" -Type "Info"
                
                $featuresParams = @{
                    EpicId = $epicId
                    MaxFeatures = $MaxItemsPerLevel
                }
                if ($DryRun) { $featuresParams.Preview = $true }
                if ($AutoApprove) { $featuresParams.AutoApprove = $true }
                
                $featuresCmd = Join-Path -Path $scriptDir -ChildPath "commands/features/build-features.ps1"
                $featuresResult = Invoke-WorkflowCommand -Command $featuresCmd -Parameters $featuresParams -Phase "Features"
                
                $featureIds = Get-CreatedItems -Output $featuresResult -ItemType "Feature"
                $workflowState.CreatedItems.Features += $featureIds
            }
            
            Write-WorkflowLog -Phase "Features" -Message "Created $($workflowState.CreatedItems.Features.Count) Features total" -Type "Success"
        }
        
        if ($StopAt -eq "Features") {
            Write-WorkflowLog -Phase "Features" -Message "Stopping at Features as requested" -Type "Info"
            Show-WorkflowSummary
            exit 0
        }
    }
    
    # Phase 5: Stories
    if ($StartFrom -in @("Vision", "Strategy", "Epic", "Feature", "Story") -and $StopAt -notin @("Vision", "Strategy", "Epics", "Features")) {
        if ($workflowState.CreatedItems.Features.Count -eq 0 -and $StartFrom -ne "Story") {
            Write-WorkflowLog -Phase "Stories" -Message "No features to create stories from" -Type "Warning"
        }
        else {
            $featuresList = if ($StartFrom -eq "Story") { @($ParentId) } else { $workflowState.CreatedItems.Features }
            
            foreach ($featureId in $featuresList[0..([Math]::Min(2, $featuresList.Count - 1))]) {  # Process first 3 features
                if (-not (Get-ContinueApproval -NextPhase "Stories for Feature #$featureId")) {
                    Write-WorkflowLog -Phase "Stories" -Message "Skipped stories for Feature #$featureId" -Type "Warning"
                    continue
                }
                
                Write-WorkflowLog -Phase "Stories" -Message "Creating Stories for Feature #$featureId" -Type "Info"
                
                $storiesParams = @{
                    FeatureId = $featureId
                    MaxStories = $MaxItemsPerLevel
                }
                if ($DryRun) { $storiesParams.Preview = $true }
                if ($AutoApprove) { $storiesParams.AutoApprove = $true }
                
                $storiesCmd = Join-Path -Path $scriptDir -ChildPath "commands/stories/build-stories.ps1"
                $storiesResult = Invoke-WorkflowCommand -Command $storiesCmd -Parameters $storiesParams -Phase "Stories"
                
                $storyIds = Get-CreatedItems -Output $storiesResult -ItemType "Story"
                $workflowState.CreatedItems.Stories += $storyIds
            }
            
            Write-WorkflowLog -Phase "Stories" -Message "Created $($workflowState.CreatedItems.Stories.Count) Stories total" -Type "Success"
        }
        
        if ($StopAt -eq "Stories") {
            Write-WorkflowLog -Phase "Stories" -Message "Stopping at Stories as requested" -Type "Info"
            Show-WorkflowSummary
            exit 0
        }
    }
    
    # Phase 6: Test Cases
    if ($StopAt -notin @("Vision", "Strategy", "Epics", "Features", "Stories")) {
        if ($workflowState.CreatedItems.Stories.Count -eq 0) {
            Write-WorkflowLog -Phase "TestCases" -Message "No stories to create test cases from" -Type "Warning"
        }
        else {
            # Process only first story for demo
            $storyId = $workflowState.CreatedItems.Stories[0]
            
            if (-not (Get-ContinueApproval -NextPhase "Test Cases for Story #$storyId")) {
                Write-WorkflowLog -Phase "TestCases" -Message "Skipped test cases" -Type "Warning"
            }
            else {
                Write-WorkflowLog -Phase "TestCases" -Message "Creating Test Cases for Story #$storyId" -Type "Info"
                
                $testParams = @{
                    StoryId = $storyId
                    MaxTestCases = [Math]::Min(20, $MaxItemsPerLevel)
                    TestTypes = "Unit,Integration,E2E"
                }
                if ($DryRun) { $testParams.Preview = $true }
                if ($AutoApprove) { $testParams.AutoApprove = $true }
                
                $testCmd = Join-Path -Path $scriptDir -ChildPath "commands/test-cases/build-test-cases.ps1"
                $testResult = Invoke-WorkflowCommand -Command $testCmd -Parameters $testParams -Phase "TestCases"
                
                $testIds = Get-CreatedItems -Output $testResult -ItemType "TestCase"
                $workflowState.CreatedItems.TestCases += $testIds
                
                Write-WorkflowLog -Phase "TestCases" -Message "Created $($testIds.Count) Test Cases" -Type "Success"
            }
        }
        
        if ($StopAt -eq "TestCases") {
            Write-WorkflowLog -Phase "TestCases" -Message "Stopping at Test Cases as requested" -Type "Info"
            Show-WorkflowSummary
            exit 0
        }
    }
    
    # Phase 7: Tasks
    if ($StopAt -eq "Tasks") {
        if ($workflowState.CreatedItems.Stories.Count -eq 0) {
            Write-WorkflowLog -Phase "Tasks" -Message "No stories to create tasks from" -Type "Warning"
        }
        else {
            # Process only first story for demo
            $storyId = $workflowState.CreatedItems.Stories[0]
            
            if (-not (Get-ContinueApproval -NextPhase "Tasks for Story #$storyId")) {
                Write-WorkflowLog -Phase "Tasks" -Message "Skipped tasks" -Type "Warning"
            }
            else {
                Write-WorkflowLog -Phase "Tasks" -Message "Creating Tasks for Story #$storyId" -Type "Info"
                
                $tasksParams = @{
                    StoryId = $storyId
                    MaxTasks = [Math]::Min(20, $MaxItemsPerLevel)
                    TaskTypes = "Development,Testing,Documentation"
                }
                if ($DryRun) { $tasksParams.Preview = $true }
                if ($AutoApprove) { $tasksParams.AutoApprove = $true }
                
                $tasksCmd = Join-Path -Path $scriptDir -ChildPath "commands/tasks/build-tasks.ps1"
                $tasksResult = Invoke-WorkflowCommand -Command $tasksCmd -Parameters $tasksParams -Phase "Tasks"
                
                $taskIds = Get-CreatedItems -Output $tasksResult -ItemType "Task"
                $workflowState.CreatedItems.Tasks += $taskIds
                
                Write-WorkflowLog -Phase "Tasks" -Message "Created $($taskIds.Count) Tasks" -Type "Success"
            }
        }
    }
    
    # Workflow completed
    Write-WorkflowLog -Phase "Complete" -Message "Workflow execution completed successfully!" -Type "Success"
}
catch {
    Write-WorkflowLog -Phase "Error" -Message "Workflow failed: $_" -Type "Error"
}
finally {
    # Always show summary
    Show-WorkflowSummary
}

# Return success/failure code
exit $(if ($workflowState.Errors.Count -eq 0) { 0 } else { 1 })