#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates Task work items in Azure DevOps based on User Story technical requirements.

.DESCRIPTION
    This script analyzes a User Story work item and generates 1-40 Task work items
    with detailed technical implementation specifications.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTasks = 40,
    
    [Parameter(Mandatory=$false)]
    [string]$TaskTypes = "Development,Testing,Documentation"
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to generate task content
function New-TaskContent {
    param(
        [string]$TaskTitle,
        [string]$TaskType,
        [string]$StoryTitle,
        [string]$ProductName,
        [int]$TaskNumber,
        [int]$TotalTasks,
        [int]$EstimatedHours
    )
    
    $taskHtml = @"
<h2>Task: $TaskTitle</h2>

<h3>Task Information</h3>
<p><strong>Type:</strong> $TaskType</p>
<p><strong>Story:</strong> $StoryTitle</p>
<p><strong>Task Number:</strong> $TaskNumber of $TotalTasks</p>
<p><strong>Estimated Hours:</strong> $EstimatedHours</p>

<h3>Task Description</h3>
<p>Implement $TaskTitle as part of the $StoryTitle story.</p>

<h3>Technical Requirements</h3>
<ul>
<li>Primary: [Main technical requirement]</li>
<li>Secondary: [Additional requirements]</li>
<li>Constraints: [Technical constraints]</li>
</ul>

<h3>Implementation Approach</h3>
<ol>
<li>Setup: [Initial setup steps]</li>
<li>Core Implementation: [Main implementation steps]</li>
<li>Testing: [Testing approach]</li>
<li>Integration: [Integration steps]</li>
<li>Cleanup: [Finalization steps]</li>
</ol>

<h3>Code Components</h3>
<ul>
<li><strong>Files to Create/Modify:</strong>
  <ul>
    <li>[File path 1]</li>
    <li>[File path 2]</li>
    <li>[File path 3]</li>
  </ul>
</li>
<li><strong>Methods/Functions:</strong>
  <ul>
    <li>[Method signature 1]</li>
    <li>[Method signature 2]</li>
  </ul>
</li>
<li><strong>Dependencies:</strong>
  <ul>
    <li>[Dependency 1]</li>
    <li>[Dependency 2]</li>
  </ul>
</li>
</ul>

<h3>Acceptance Criteria</h3>
<ul>
<li>Code compiles without errors or warnings</li>
<li>Unit tests pass with 80%+ coverage</li>
<li>Integration points tested</li>
<li>Code review completed</li>
<li>Documentation updated</li>
</ul>

<h3>Architecture Considerations</h3>
<ul>
<li><strong>Design Pattern:</strong> [Applicable pattern]</li>
<li><strong>Performance:</strong> [Performance considerations]</li>
<li><strong>Security:</strong> [Security considerations]</li>
<li><strong>Scalability:</strong> [Scalability considerations]</li>
</ul>

<h3>Definition of Done</h3>
<ul>
<li>Implementation complete</li>
<li>Tests written and passing</li>
<li>Code reviewed and approved</li>
<li>Documentation updated</li>
<li>Deployed to development environment</li>
</ul>

<hr>
<p><em>Task generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm")</em></p>
"@

    return $taskHtml
}

# Function to generate tasks by type
function Get-TasksByType {
    param(
        [string]$TaskType,
        [string]$StoryTitle,
        [int]$MaxCount
    )
    
    $tasks = @()
    
    switch ($TaskType) {
        "Development" {
            $taskList = @(
                @{ Title = "Create data models"; Hours = 4 },
                @{ Title = "Implement service layer"; Hours = 8 },
                @{ Title = "Create API endpoints"; Hours = 6 },
                @{ Title = "Implement business logic"; Hours = 8 },
                @{ Title = "Add validation rules"; Hours = 4 },
                @{ Title = "Implement error handling"; Hours = 4 }
            )
        }
        "Testing" {
            $taskList = @(
                @{ Title = "Write unit tests"; Hours = 6 },
                @{ Title = "Create integration tests"; Hours = 4 },
                @{ Title = "Perform manual testing"; Hours = 4 },
                @{ Title = "Test edge cases"; Hours = 3 },
                @{ Title = "Performance testing"; Hours = 4 }
            )
        }
        "Documentation" {
            $taskList = @(
                @{ Title = "Update API documentation"; Hours = 2 },
                @{ Title = "Write user guide"; Hours = 3 },
                @{ Title = "Update technical specs"; Hours = 2 },
                @{ Title = "Create deployment guide"; Hours = 2 }
            )
        }
        "DevOps" {
            $taskList = @(
                @{ Title = "Setup CI/CD pipeline"; Hours = 4 },
                @{ Title = "Configure environments"; Hours = 3 },
                @{ Title = "Setup monitoring"; Hours = 3 },
                @{ Title = "Create deployment scripts"; Hours = 2 }
            )
        }
        "UI/UX" {
            $taskList = @(
                @{ Title = "Create UI components"; Hours = 6 },
                @{ Title = "Implement responsive design"; Hours = 4 },
                @{ Title = "Add client-side validation"; Hours = 3 },
                @{ Title = "Implement accessibility"; Hours = 3 }
            )
        }
        default {
            $taskList = @(
                @{ Title = "Generic implementation task"; Hours = 4 }
            )
        }
    }
    
    $count = [Math]::Min($taskList.Count, $MaxCount)
    for ($i = 0; $i -lt $count; $i++) {
        $tasks += @{
            Title = $taskList[$i].Title
            Type = $TaskType
            EstimatedHours = $taskList[$i].Hours
        }
    }
    
    return $tasks
}

# Main execution
Write-Host "`nTask Builder for Azure DevOps" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Workflow Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })

# Fetch story details
Write-Host "`nFetching Story details (Story #$StoryId)..." -ForegroundColor Yellow
$storyWorkItem = Get-AzureDevOpsWorkItem -WorkItemId $StoryId

if (-not $storyWorkItem) {
    Write-Host "Failed to fetch Story. Please check the Story ID." -ForegroundColor Red
    exit 1
}

$storyTitle = $storyWorkItem.fields.'System.Title'
$tags = $storyWorkItem.fields.'System.Tags' -split ';'
$productName = $tags | Where-Object { $_ -notmatch 'Generated|Story|User Story' } | Select-Object -First 1

Write-Host "Story: $storyTitle" -ForegroundColor Green
Write-Host "Product: $productName" -ForegroundColor Green

# Parse task types
$requestedTypes = $TaskTypes -split ',' | ForEach-Object { $_.Trim() }

# Generate tasks
$allTasks = @()
$taskNumber = 1

foreach ($taskType in $requestedTypes) {
    $tasksPerType = [Math]::Floor($MaxTasks / $requestedTypes.Count)
    $typeTasks = Get-TasksByType -TaskType $taskType -StoryTitle $storyTitle -MaxCount $tasksPerType
    
    foreach ($task in $typeTasks) {
        $task.Number = $taskNumber++
        $allTasks += $task
    }
}

# Show preview
Write-Host "`n=== TASK GENERATION PREVIEW ===" -ForegroundColor Cyan
Write-Host "Total Tasks to Create: $($allTasks.Count)" -ForegroundColor White
Write-Host "Total Estimated Hours: $(($allTasks | Measure-Object -Property EstimatedHours -Sum).Sum)" -ForegroundColor White
Write-Host "`nTask Breakdown:" -ForegroundColor Cyan

$groupedTasks = $allTasks | Group-Object -Property Type
foreach ($group in $groupedTasks) {
    Write-Host "`n$($group.Name) Tasks:" -ForegroundColor Yellow
    foreach ($task in $group.Group) {
        Write-Host "  - $($task.Title) ($($task.EstimatedHours)h)" -ForegroundColor White
    }
}
Write-Host "`n===============================" -ForegroundColor Cyan

# Check approval
$shouldProceed = $AutoApprove -or $Preview
if (-not $shouldProceed) {
    Write-Host "`nDo you want to proceed with creating these tasks?" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No" -ForegroundColor White
    $response = Read-Host "Choice"
    $shouldProceed = $response -match '^[Yy]'
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create tasks
$createdTasks = @()
$failedTasks = @()

foreach ($task in $allTasks) {
    Write-Host "`nCreating Task: $($task.Title)..." -ForegroundColor Yellow
    
    if ($Preview) {
        Write-Host "PREVIEW: Would create Task '$($task.Title)'" -ForegroundColor Cyan
        $createdTasks += @{
            Id = "PREVIEW"
            Title = $task.Title
        }
    }
    else {
        $taskContent = New-TaskContent `
            -TaskTitle $task.Title `
            -TaskType $task.Type `
            -StoryTitle $storyTitle `
            -ProductName $productName `
            -TaskNumber $task.Number `
            -TotalTasks $allTasks.Count `
            -EstimatedHours $task.EstimatedHours
            
        # For Tasks, we need to include remaining work
        $result = New-AzureDevOpsWorkItem `
            -WorkItemType "Task" `
            -Title "TASK-$StoryId-$('{0:D3}' -f $task.Number): $($task.Title)" `
            -Description $taskContent `
            -ParentId $StoryId `
            -Tags "$productName; Task; $($task.Type); Generated" `
            -Priority 2
            
        if ($result.Success) {
            Write-Host "✓ Created Task #$($result.Id)" -ForegroundColor Green
            $createdTasks += @{
                Id = $result.Id
                Title = $task.Title
                Url = $result.Url
                Hours = $task.EstimatedHours
            }
        }
        else {
            Write-Host "✗ Failed to create Task" -ForegroundColor Red
            Write-Host "  Error: $($result.Error)" -ForegroundColor Red
            $failedTasks += $task.Title
        }
    }
}

# Summary
if ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the tasks." -ForegroundColor Cyan
}
else {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Task Creation Summary" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Successfully Created: $($createdTasks.Count) tasks" -ForegroundColor Green
    if ($failedTasks.Count -gt 0) {
        Write-Host "Failed: $($failedTasks.Count) tasks" -ForegroundColor Red
    }
    
    if ($createdTasks.Count -gt 0) {
        $totalHours = ($createdTasks | Measure-Object -Property Hours -Sum).Sum
        Write-Host "Total Estimated Hours: $totalHours" -ForegroundColor White
        
        Write-Host "`nCreated Tasks:" -ForegroundColor Cyan
        foreach ($created in $createdTasks) {
            Write-Host "  - Task #$($created.Id): $($created.Title)" -ForegroundColor White
        }
        
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "1. Assign tasks to team members" -ForegroundColor White
        Write-Host "2. Update remaining work estimates" -ForegroundColor White
        Write-Host "3. Add tasks to sprint backlog" -ForegroundColor White
    }
}