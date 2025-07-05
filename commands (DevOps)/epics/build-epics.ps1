#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates Epic work items in Azure DevOps based on Vision Strategy initiatives.

.DESCRIPTION
    This script analyzes a Vision Strategy Epic and generates 1-20 Epic work items
    representing major implementation initiatives.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StrategyEpicId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxEpics = 20
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to generate epic content
function New-EpicContent {
    param(
        [string]$EpicTitle,
        [string]$Initiative,
        [string]$Quarter,
        [string]$ProductName,
        [int]$EpicNumber,
        [int]$TotalEpics
    )
    
    $epicHtml = @"
<h2>Epic: $EpicTitle</h2>

<h3>Overview</h3>
<p><strong>Initiative:</strong> $Initiative</p>
<p><strong>Target Quarter:</strong> $Quarter</p>
<p><strong>Priority:</strong> $EpicNumber of $TotalEpics epics in this strategy</p>

<h3>Business Context</h3>
<p>This epic delivers critical capabilities for $ProductName by implementing $Initiative.</p>

<h3>Goals & Objectives</h3>
<ul>
<li>Primary Goal: [Define primary objective]</li>
<li>Secondary Goals:
  <ul>
    <li>[Secondary goal 1]</li>
    <li>[Secondary goal 2]</li>
    <li>[Secondary goal 3]</li>
  </ul>
</li>
</ul>

<h3>Scope</h3>
<h4>In Scope</h4>
<ul>
<li>[What is included in this epic]</li>
<li>[Additional included items]</li>
<li>[More included items]</li>
</ul>

<h4>Out of Scope</h4>
<ul>
<li>[What is NOT included in this epic]</li>
<li>[Additional excluded items]</li>
</ul>

<h3>Success Criteria</h3>
<ul>
<li>Acceptance: [How we know this epic is complete]</li>
<li>Metrics: [Measurable success indicators]</li>
<li>Quality: [Quality standards to meet]</li>
</ul>

<h3>Dependencies</h3>
<ul>
<li>Upstream: [What this epic depends on]</li>
<li>Downstream: [What depends on this epic]</li>
</ul>

<h3>Risks & Mitigation</h3>
<ul>
<li>Risk: [Identified risk]
  <ul><li>Mitigation: [How to address it]</li></ul>
</li>
</ul>

<hr>
<p><em>Epic generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm")</em></p>
"@

    return $epicHtml
}

# Main execution
Write-Host "`nEpic Builder for Azure DevOps" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host "Workflow Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })

# Fetch strategy details
Write-Host "`nFetching Vision Strategy details (Epic #$StrategyEpicId)..." -ForegroundColor Yellow
$strategyWorkItem = Get-AzureDevOpsWorkItem -WorkItemId $StrategyEpicId

if (-not $strategyWorkItem) {
    Write-Host "Failed to fetch Strategy Epic. Please check the Epic ID." -ForegroundColor Red
    exit 1
}

$strategyTitle = $strategyWorkItem.fields.'System.Title'
$productName = if ($strategyTitle -match 'Vision Strategy - (.+)') { $Matches[1] } else { "Unknown Product" }

Write-Host "Product: $productName" -ForegroundColor Green

# Generate epics
$epics = @()
$initiativeTypes = @(
    "Platform Architecture & Infrastructure",
    "Security & Compliance Framework", 
    "Integration & API Platform",
    "User Experience & Interface Design",
    "Data Management & Analytics",
    "Performance & Scalability",
    "DevOps & Automation",
    "Documentation & Training"
)

$quarters = @("Q3 2025", "Q4 2025", "Q1 2026", "Q2 2026", "Q3 2026", "Q4 2026")

for ($i = 0; $i -lt [Math]::Min($MaxEpics, $initiativeTypes.Count); $i++) {
    $quarter = $quarters[$i % $quarters.Count]
    $epics += @{
        Title = $initiativeTypes[$i]
        Initiative = $initiativeTypes[$i]
        Quarter = $quarter
        Priority = $i + 1
    }
}

# Show preview
Write-Host "`n=== EPIC GENERATION PREVIEW ===" -ForegroundColor Cyan
Write-Host "Total Epics to Create: $($epics.Count)" -ForegroundColor White
Write-Host "`nEpic Breakdown:" -ForegroundColor Cyan
foreach ($epic in $epics) {
    Write-Host "  - $($epic.Title) (Q: $($epic.Quarter))" -ForegroundColor White
}
Write-Host "`n================================" -ForegroundColor Cyan

# Check approval
$shouldProceed = $AutoApprove -or $Preview
if (-not $shouldProceed) {
    Write-Host "`nDo you want to proceed with creating these epics?" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No" -ForegroundColor White
    $response = Read-Host "Choice"
    $shouldProceed = $response -match '^[Yy]'
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create epics
$createdEpics = @()
$failedEpics = @()

foreach ($epic in $epics) {
    Write-Host "`nCreating Epic: $($epic.Title)..." -ForegroundColor Yellow
    
    if ($Preview) {
        Write-Host "PREVIEW: Would create Epic '$($epic.Title)'" -ForegroundColor Cyan
        $createdEpics += @{
            Id = "PREVIEW"
            Title = $epic.Title
        }
    }
    else {
        $epicContent = New-EpicContent `
            -EpicTitle $epic.Title `
            -Initiative $epic.Initiative `
            -Quarter $epic.Quarter `
            -ProductName $productName `
            -EpicNumber $epic.Priority `
            -TotalEpics $epics.Count
            
        $result = New-AzureDevOpsWorkItem `
            -WorkItemType "Epic" `
            -Title $epic.Title `
            -Description $epicContent `
            -ParentId $StrategyEpicId `
            -Tags "$productName; $($epic.Initiative); $($epic.Quarter); Generated" `
            -Priority $epic.Priority
            
        if ($result.Success) {
            Write-Host "✓ Created Epic #$($result.Id)" -ForegroundColor Green
            $createdEpics += @{
                Id = $result.Id
                Title = $epic.Title
                Url = $result.Url
            }
        }
        else {
            Write-Host "✗ Failed to create Epic" -ForegroundColor Red
            Write-Host "  Error: $($result.Error)" -ForegroundColor Red
            $failedEpics += $epic.Title
        }
    }
}

# Summary
if ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the epics." -ForegroundColor Cyan
}
else {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Epic Creation Summary" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Successfully Created: $($createdEpics.Count) epics" -ForegroundColor Green
    if ($failedEpics.Count -gt 0) {
        Write-Host "Failed: $($failedEpics.Count) epics" -ForegroundColor Red
    }
    
    if ($createdEpics.Count -gt 0) {
        Write-Host "`nCreated Epics:" -ForegroundColor Cyan
        foreach ($created in $createdEpics) {
            Write-Host "  - Epic #$($created.Id): $($created.Title)" -ForegroundColor White
        }
        
        Write-Host "`nView in Azure DevOps:" -ForegroundColor Yellow
        Write-Host "https://integrity-data.visualstudio.com/Integrity.HRP/_workitems/recentlyupdated" -ForegroundColor White
    }
}