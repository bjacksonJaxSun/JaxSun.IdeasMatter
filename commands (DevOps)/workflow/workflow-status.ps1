#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Shows the status and progress of product development workflows.

.DESCRIPTION
    This script analyzes the Azure DevOps backlog to show the current status of
    product development workflows, including completion percentages and bottlenecks.

.PARAMETER ProductName
    Optional. Filter by specific product name.

.PARAMETER ShowDetails
    Show detailed breakdown of all work items.

.PARAMETER ExportReport
    Export detailed report to HTML file.

.EXAMPLE
    ./workflow-status.ps1
    ./workflow-status.ps1 -ProductName "Bermuda Payroll" -ShowDetails
    ./workflow-status.ps1 -ExportReport
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$ProductName,
    
    [Parameter(Mandatory=$false)]
    [switch]$ShowDetails,
    
    [Parameter(Mandatory=$false)]
    [switch]$ExportReport
)

# Read configuration
$configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
if (-not (Test-Path $configPath)) {
    Write-Host "Error: Configuration file not found at $configPath" -ForegroundColor Red
    exit 1
}

$config = Get-Content $configPath -Raw | ConvertFrom-Json
$organization = $config.azureDevOps.organization
$project = $config.azureDevOps.project
$pat = $config.azureDevOps.personalAccessToken
$apiVersion = $config.azureDevOps.apiVersion

# Create authorization header
$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
$headers = @{
    Authorization = "Basic $base64AuthInfo"
    "Content-Type" = "application/json"
}

# Function to query work items
function Get-WorkItems {
    param(
        [string]$WorkItemType,
        [string]$TagFilter = ""
    )
    
    $wiql = @"
SELECT [System.Id], [System.Title], [System.State], [System.Tags], [System.WorkItemType]
FROM WorkItems
WHERE [System.TeamProject] = '$project'
AND [System.WorkItemType] = '$WorkItemType'
$(if ($TagFilter) { "AND [System.Tags] Contains '$TagFilter'" })
ORDER BY [System.CreatedDate] DESC
"@
    
    $body = @{
        query = $wiql
    } | ConvertTo-Json
    
    $url = "https://$organization.visualstudio.com/$project/_apis/wit/wiql?api-version=$apiVersion"
    
    try {
        $queryResult = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body
        
        if ($queryResult.workItems.Count -eq 0) {
            return @()
        }
        
        # Get work item details
        $ids = ($queryResult.workItems | ForEach-Object { $_.id }) -join ','
        $fieldsUrl = "https://$organization.visualstudio.com/$project/_apis/wit/workitems?ids=$ids&api-version=$apiVersion"
        
        $workItems = Invoke-RestMethod -Uri $fieldsUrl -Method Get -Headers $headers
        return $workItems.value
    }
    catch {
        Write-Host "Failed to query $WorkItemType work items: $_" -ForegroundColor Red
        return @()
    }
}

# Function to get work item relationships
function Get-WorkItemRelations {
    param([int]$WorkItemId)
    
    $url = "https://$organization.visualstudio.com/$project/_apis/wit/workitems/$WorkItemId`?`$expand=relations&api-version=$apiVersion"
    
    try {
        $workItem = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
        return $workItem.relations | Where-Object { $_.rel -eq "System.LinkTypes.Hierarchy-Forward" }
    }
    catch {
        return @()
    }
}

# Function to calculate completion percentage
function Get-CompletionPercentage {
    param([array]$WorkItems)
    
    if ($WorkItems.Count -eq 0) {
        return 0
    }
    
    $completed = @($WorkItems | Where-Object { 
        $_.fields.'System.State' -in @('Done', 'Closed', 'Resolved', 'Completed') 
    }).Count
    
    return [Math]::Round(($completed / $WorkItems.Count) * 100, 1)
}

# Function to display progress bar
function Show-ProgressBar {
    param(
        [int]$Percentage,
        [int]$Width = 30
    )
    
    $filled = [Math]::Floor($Width * $Percentage / 100)
    $empty = $Width - $filled
    
    $bar = "[" + ("█" * $filled) + ("░" * $empty) + "]"
    
    $color = if ($Percentage -ge 80) { "Green" } 
             elseif ($Percentage -ge 50) { "Yellow" } 
             else { "Red" }
    
    Write-Host "$bar $Percentage%" -ForegroundColor $color -NoNewline
}

# Function to analyze workflow
function Get-WorkflowAnalysis {
    Write-Host "`nAnalyzing Product Development Workflows..." -ForegroundColor Cyan
    
    # Get all work items by type
    $visions = Get-WorkItems -WorkItemType "Epic" -TagFilter "Product Vision"
    $strategies = Get-WorkItems -WorkItemType "Epic" -TagFilter "Vision Strategy"
    $epics = Get-WorkItems -WorkItemType "Epic" | Where-Object { 
        $_.fields.'System.Tags' -notmatch 'Product Vision|Vision Strategy' 
    }
    $features = Get-WorkItems -WorkItemType "Feature"
    $stories = Get-WorkItems -WorkItemType "User Story"
    $testCases = Get-WorkItems -WorkItemType "Test Case"
    $tasks = Get-WorkItems -WorkItemType "Task"
    
    # Filter by product if specified
    if ($ProductName) {
        $visions = $visions | Where-Object { $_.fields.'System.Title' -match $ProductName }
        $strategies = $strategies | Where-Object { $_.fields.'System.Title' -match $ProductName }
        # For other items, we'd need to trace relationships
    }
    
    # Create analysis object
    $analysis = @{
        Visions = @{
            Items = $visions
            Total = $visions.Count
            Completed = @($visions | Where-Object { $_.fields.'System.State' -eq 'Done' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $visions
        }
        Strategies = @{
            Items = $strategies
            Total = $strategies.Count
            Completed = @($strategies | Where-Object { $_.fields.'System.State' -eq 'Done' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $strategies
        }
        Epics = @{
            Items = $epics
            Total = $epics.Count
            Completed = @($epics | Where-Object { $_.fields.'System.State' -eq 'Done' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $epics
        }
        Features = @{
            Items = $features
            Total = $features.Count
            Completed = @($features | Where-Object { $_.fields.'System.State' -eq 'Done' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $features
        }
        Stories = @{
            Items = $stories
            Total = $stories.Count
            Completed = @($stories | Where-Object { $_.fields.'System.State' -eq 'Done' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $stories
        }
        TestCases = @{
            Items = $testCases
            Total = $testCases.Count
            Completed = @($testCases | Where-Object { $_.fields.'System.State' -eq 'Closed' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $testCases
        }
        Tasks = @{
            Items = $tasks
            Total = $tasks.Count
            Completed = @($tasks | Where-Object { $_.fields.'System.State' -eq 'Done' }).Count
            Percentage = Get-CompletionPercentage -WorkItems $tasks
        }
    }
    
    return $analysis
}

# Function to display workflow status
function Show-WorkflowStatus {
    param($Analysis)
    
    Write-Host "`n=== PRODUCT DEVELOPMENT WORKFLOW STATUS ===" -ForegroundColor Cyan
    if ($ProductName) {
        Write-Host "Product Filter: $ProductName" -ForegroundColor White
    }
    Write-Host "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm')" -ForegroundColor White
    Write-Host "===========================================" -ForegroundColor Cyan
    
    # Summary table
    Write-Host "`nWORKFLOW SUMMARY:" -ForegroundColor Yellow
    Write-Host "─────────────────────────────────────────────────────────────────" -ForegroundColor Gray
    
    $levels = @("Visions", "Strategies", "Epics", "Features", "Stories", "TestCases", "Tasks")
    
    foreach ($level in $levels) {
        $data = $Analysis.$level
        $levelName = $level.PadRight(12)
        
        Write-Host "$levelName " -NoNewline
        Show-ProgressBar -Percentage $data.Percentage
        Write-Host " " -NoNewline
        Write-Host "$($data.Completed)/$($data.Total)" -ForegroundColor White
    }
    
    # Bottleneck analysis
    Write-Host "`nBOTTLENECK ANALYSIS:" -ForegroundColor Yellow
    Write-Host "─────────────────────────────────────────────────────────────────" -ForegroundColor Gray
    
    $bottlenecks = @()
    foreach ($level in $levels) {
        if ($Analysis.$level.Total -gt 0 -and $Analysis.$level.Percentage -lt 50) {
            $bottlenecks += @{
                Level = $level
                Percentage = $Analysis.$level.Percentage
                Pending = $Analysis.$level.Total - $Analysis.$level.Completed
            }
        }
    }
    
    if ($bottlenecks.Count -eq 0) {
        Write-Host "No significant bottlenecks detected!" -ForegroundColor Green
    }
    else {
        foreach ($bottleneck in ($bottlenecks | Sort-Object Percentage)) {
            Write-Host "⚠ $($bottleneck.Level): " -ForegroundColor Yellow -NoNewline
            Write-Host "$($bottleneck.Pending) items pending " -ForegroundColor White -NoNewline
            Write-Host "($($bottleneck.Percentage)% complete)" -ForegroundColor Red
        }
    }
    
    # Workflow health score
    $totalItems = 0
    $totalCompleted = 0
    foreach ($level in $levels) {
        $totalItems += $Analysis.$level.Total
        $totalCompleted += $Analysis.$level.Completed
    }
    
    $healthScore = if ($totalItems -gt 0) { 
        [Math]::Round(($totalCompleted / $totalItems) * 100, 1) 
    } else { 0 }
    
    Write-Host "`nWORKFLOW HEALTH SCORE: " -ForegroundColor Yellow -NoNewline
    $healthColor = if ($healthScore -ge 80) { "Green" }
                   elseif ($healthScore -ge 60) { "Yellow" }
                   else { "Red" }
    Write-Host "$healthScore%" -ForegroundColor $healthColor
}

# Function to show detailed breakdown
function Show-DetailedBreakdown {
    param($Analysis)
    
    if (-not $ShowDetails) {
        return
    }
    
    Write-Host "`nDETAILED BREAKDOWN:" -ForegroundColor Yellow
    Write-Host "===================" -ForegroundColor Yellow
    
    # Product Visions
    if ($Analysis.Visions.Total -gt 0) {
        Write-Host "`nProduct Visions:" -ForegroundColor Cyan
        foreach ($vision in $Analysis.Visions.Items) {
            $state = $vision.fields.'System.State'
            $stateColor = if ($state -eq 'Done') { "Green" } else { "Yellow" }
            Write-Host "  • " -NoNewline
            Write-Host "[$state]" -ForegroundColor $stateColor -NoNewline
            Write-Host " $($vision.fields.'System.Title')" -ForegroundColor White
        }
    }
    
    # Strategies
    if ($Analysis.Strategies.Total -gt 0) {
        Write-Host "`nVision Strategies:" -ForegroundColor Cyan
        foreach ($strategy in $Analysis.Strategies.Items) {
            $state = $strategy.fields.'System.State'
            $stateColor = if ($state -eq 'Done') { "Green" } else { "Yellow" }
            Write-Host "  • " -NoNewline
            Write-Host "[$state]" -ForegroundColor $stateColor -NoNewline
            Write-Host " $($strategy.fields.'System.Title')" -ForegroundColor White
        }
    }
    
    # Show counts for other levels
    Write-Host "`nWork Item Counts:" -ForegroundColor Cyan
    Write-Host "  Epics: $($Analysis.Epics.Total) ($($Analysis.Epics.Completed) completed)" -ForegroundColor White
    Write-Host "  Features: $($Analysis.Features.Total) ($($Analysis.Features.Completed) completed)" -ForegroundColor White
    Write-Host "  Stories: $($Analysis.Stories.Total) ($($Analysis.Stories.Completed) completed)" -ForegroundColor White
    Write-Host "  Test Cases: $($Analysis.TestCases.Total) ($($Analysis.TestCases.Completed) completed)" -ForegroundColor White
    Write-Host "  Tasks: $($Analysis.Tasks.Total) ($($Analysis.Tasks.Completed) completed)" -ForegroundColor White
}

# Function to export HTML report
function Export-HtmlReport {
    param($Analysis)
    
    if (-not $ExportReport) {
        return
    }
    
    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Product Development Workflow Status</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        h1 { color: #333; border-bottom: 2px solid #0066cc; padding-bottom: 10px; }
        h2 { color: #0066cc; margin-top: 30px; }
        .summary-table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        .summary-table th, .summary-table td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        .summary-table th { background-color: #0066cc; color: white; }
        .progress-bar { width: 200px; height: 20px; background-color: #e0e0e0; border-radius: 10px; overflow: hidden; display: inline-block; }
        .progress-fill { height: 100%; background-color: #4CAF50; transition: width 0.3s; }
        .bottleneck { background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 10px; margin: 10px 0; border-radius: 4px; }
        .health-score { font-size: 48px; font-weight: bold; text-align: center; margin: 30px 0; }
        .good { color: #4CAF50; }
        .warning { color: #ff9800; }
        .critical { color: #f44336; }
    </style>
</head>
<body>
    <div class="container">
        <h1>Product Development Workflow Status</h1>
        <p>Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm')</p>
        
        <h2>Workflow Summary</h2>
        <table class="summary-table">
            <tr>
                <th>Level</th>
                <th>Progress</th>
                <th>Completed</th>
                <th>Total</th>
                <th>Percentage</th>
            </tr>
"@
    
    $levels = @("Visions", "Strategies", "Epics", "Features", "Stories", "TestCases", "Tasks")
    foreach ($level in $levels) {
        $data = $Analysis.$level
        $html += @"
            <tr>
                <td>$level</td>
                <td>
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: $($data.Percentage)%"></div>
                    </div>
                </td>
                <td>$($data.Completed)</td>
                <td>$($data.Total)</td>
                <td>$($data.Percentage)%</td>
            </tr>
"@
    }
    
    $totalItems = 0
    $totalCompleted = 0
    foreach ($level in $levels) {
        $totalItems += $Analysis.$level.Total
        $totalCompleted += $Analysis.$level.Completed
    }
    
    $healthScore = if ($totalItems -gt 0) { 
        [Math]::Round(($totalCompleted / $totalItems) * 100, 1) 
    } else { 0 }
    
    $healthClass = if ($healthScore -ge 80) { "good" }
                   elseif ($healthScore -ge 60) { "warning" }
                   else { "critical" }
    
    $html += @"
        </table>
        
        <h2>Workflow Health Score</h2>
        <div class="health-score $healthClass">$healthScore%</div>
        
        <h2>Bottleneck Analysis</h2>
"@
    
    $bottlenecks = @()
    foreach ($level in $levels) {
        if ($Analysis.$level.Total -gt 0 -and $Analysis.$level.Percentage -lt 50) {
            $bottlenecks += @{
                Level = $level
                Percentage = $Analysis.$level.Percentage
                Pending = $Analysis.$level.Total - $Analysis.$level.Completed
            }
        }
    }
    
    if ($bottlenecks.Count -eq 0) {
        $html += "<p style='color: green;'>No significant bottlenecks detected!</p>"
    }
    else {
        foreach ($bottleneck in ($bottlenecks | Sort-Object Percentage)) {
            $html += @"
        <div class="bottleneck">
            <strong>$($bottleneck.Level):</strong> $($bottleneck.Pending) items pending ($($bottleneck.Percentage)% complete)
        </div>
"@
        }
    }
    
    $html += @"
    </div>
</body>
</html>
"@
    
    $reportFile = Join-Path -Path $PSScriptRoot -ChildPath "../../docs/workflow-reports/status-$(Get-Date -Format 'yyyyMMdd-HHmmss').html"
    $reportDir = Split-Path $reportFile -Parent
    if (-not (Test-Path $reportDir)) {
        New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    }
    
    $html | Set-Content -Path $reportFile
    Write-Host "`nHTML report exported to: $reportFile" -ForegroundColor Green
}

# Main execution
$analysis = Get-WorkflowAnalysis
Show-WorkflowStatus -Analysis $analysis
Show-DetailedBreakdown -Analysis $analysis
Export-HtmlReport -Analysis $analysis

# Show recommendations
Write-Host "`nRECOMMENDATIONS:" -ForegroundColor Yellow
Write-Host "────────────────" -ForegroundColor Gray

if ($analysis.Visions.Total -eq 0) {
    Write-Host "• Start by creating a Product Vision using:" -ForegroundColor White
    Write-Host "  ./commands/vision/create-product-vision.ps1 -VisionName 'Your Product'" -ForegroundColor Gray
}
elseif ($analysis.Strategies.Total -eq 0) {
    Write-Host "• Create Vision Strategies for your Product Visions using:" -ForegroundColor White
    Write-Host "  ./commands/strategy/build-vision-strategy-enhanced.ps1 -VisionEpicId <ID>" -ForegroundColor Gray
}
elseif ($analysis.Epics.Percentage -lt 30) {
    Write-Host "• Generate Epics from your Strategies using:" -ForegroundColor White
    Write-Host "  ./commands/epics/build-epics.ps1 -StrategyEpicId <ID>" -ForegroundColor Gray
}
elseif ($analysis.Features.Percentage -lt 30) {
    Write-Host "• Break down Epics into Features using:" -ForegroundColor White
    Write-Host "  ./commands/features/build-features.ps1 -EpicId <ID>" -ForegroundColor Gray
}
else {
    Write-Host "• Continue breaking down work items and implementing features" -ForegroundColor White
}

Write-Host "`nUse execute-full-workflow.ps1 to automate the entire process!" -ForegroundColor Cyan