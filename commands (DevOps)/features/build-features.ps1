#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates Feature work items in Azure DevOps based on Epic scope.

.DESCRIPTION
    This script analyzes an Epic work item and generates 1-40 Feature work items
    covering different aspects of the epic implementation.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$EpicId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxFeatures = 40
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to generate feature content
function New-FeatureContent {
    param(
        [string]$FeatureTitle,
        [string]$Category,
        [string]$EpicTitle,
        [string]$ProductName,
        [int]$FeatureNumber,
        [int]$TotalFeatures
    )
    
    $featureHtml = @"
<h2>Feature: $FeatureTitle</h2>

<h3>Overview</h3>
<p><strong>Category:</strong> $Category</p>
<p><strong>Parent Epic:</strong> $EpicTitle</p>
<p><strong>Priority:</strong> $FeatureNumber of $TotalFeatures features in this epic</p>

<h3>Business Value</h3>
<p>This feature contributes to $EpicTitle by providing $Category capabilities for $ProductName.</p>

<h3>Functional Requirements</h3>
<ul>
<li><strong>[REQ-001]:</strong> [Primary requirement description]</li>
<li><strong>[REQ-002]:</strong> [Secondary requirement description]</li>
<li><strong>[REQ-003]:</strong> [Additional requirement description]</li>
</ul>

<h3>Acceptance Criteria</h3>
<ul>
<li>[Criteria 1 - specific measurable outcome]</li>
<li>[Criteria 2 - specific measurable outcome]</li>
<li>[Criteria 3 - specific measurable outcome]</li>
</ul>

<h3>User Story Themes</h3>
<ul>
<li><strong>Admin Users:</strong> Configuration and management capabilities</li>
<li><strong>End Users:</strong> Core functionality and user experience</li>
<li><strong>System:</strong> Backend processing and automation</li>
</ul>

<h3>Technical Considerations</h3>
<ul>
<li><strong>Architecture:</strong> [Key architectural decisions]</li>
<li><strong>Integration:</strong> [Integration points with other systems]</li>
<li><strong>Performance:</strong> [Performance requirements]</li>
<li><strong>Security:</strong> [Security considerations]</li>
</ul>

<h3>Definition of Done</h3>
<ul>
<li>All acceptance criteria met</li>
<li>Unit test coverage > 80%</li>
<li>Integration tests passing</li>
<li>Documentation updated</li>
<li>Code reviewed and approved</li>
</ul>

<hr>
<p><em>Feature generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm")</em></p>
"@

    return $featureHtml
}

# Function to determine feature categories based on epic
function Get-FeatureCategories {
    param([string]$EpicTitle)
    
    $commonCategories = @(
        "Core Functionality",
        "User Interface",
        "API & Integration", 
        "Data Management",
        "Security & Access Control",
        "Performance & Optimization",
        "Monitoring & Analytics",
        "Documentation & Help"
    )
    
    # Add specific categories based on epic type
    if ($EpicTitle -match "Platform|Architecture|Infrastructure") {
        return $commonCategories + @(
            "Service Architecture",
            "Deployment Pipeline",
            "Environment Management",
            "Configuration Management"
        )
    }
    elseif ($EpicTitle -match "Security|Compliance") {
        return $commonCategories + @(
            "Authentication System",
            "Authorization Framework",
            "Audit Logging",
            "Compliance Reporting"
        )
    }
    else {
        return $commonCategories
    }
}

# Main execution
Write-Host "`nFeature Builder for Azure DevOps" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Workflow Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })

# Fetch epic details
Write-Host "`nFetching Epic details (Epic #$EpicId)..." -ForegroundColor Yellow
$epicWorkItem = Get-AzureDevOpsWorkItem -WorkItemId $EpicId

if (-not $epicWorkItem) {
    Write-Host "Failed to fetch Epic. Please check the Epic ID." -ForegroundColor Red
    exit 1
}

$epicTitle = $epicWorkItem.fields.'System.Title'
$tags = $epicWorkItem.fields.'System.Tags' -split ';'
$productName = $tags | Where-Object { $_ -notmatch 'Generated|Epic' } | Select-Object -First 1

Write-Host "Epic: $epicTitle" -ForegroundColor Green
Write-Host "Product: $productName" -ForegroundColor Green

# Generate features
$categories = Get-FeatureCategories -EpicTitle $epicTitle
$features = @()

$featureCount = [Math]::Min($MaxFeatures, $categories.Count * 2)
for ($i = 0; $i -lt $featureCount; $i++) {
    $category = $categories[$i % $categories.Count]
    $variant = if ($i -ge $categories.Count) { " - Advanced" } else { "" }
    
    $features += @{
        Title = "$category$variant"
        Category = $category
        Priority = $i + 1
    }
}

# Show preview
Write-Host "`n=== FEATURE GENERATION PREVIEW ===" -ForegroundColor Cyan
Write-Host "Total Features to Create: $($features.Count)" -ForegroundColor White
Write-Host "`nFeature Breakdown:" -ForegroundColor Cyan
foreach ($feature in $features) {
    Write-Host "  - $($feature.Title) (Priority: $($feature.Priority))" -ForegroundColor White
}
Write-Host "`n==================================" -ForegroundColor Cyan

# Check approval
$shouldProceed = $AutoApprove -or $Preview
if (-not $shouldProceed) {
    Write-Host "`nDo you want to proceed with creating these features?" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No" -ForegroundColor White
    $response = Read-Host "Choice"
    $shouldProceed = $response -match '^[Yy]'
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create features
$createdFeatures = @()
$failedFeatures = @()

foreach ($feature in $features) {
    Write-Host "`nCreating Feature: $($feature.Title)..." -ForegroundColor Yellow
    
    if ($Preview) {
        Write-Host "PREVIEW: Would create Feature '$($feature.Title)'" -ForegroundColor Cyan
        $createdFeatures += @{
            Id = "PREVIEW"
            Title = $feature.Title
        }
    }
    else {
        $featureContent = New-FeatureContent `
            -FeatureTitle $feature.Title `
            -Category $feature.Category `
            -EpicTitle $epicTitle `
            -ProductName $productName `
            -FeatureNumber $feature.Priority `
            -TotalFeatures $features.Count
            
        $result = New-AzureDevOpsWorkItem `
            -WorkItemType "Feature" `
            -Title $feature.Title `
            -Description $featureContent `
            -ParentId $EpicId `
            -Tags "$productName; $($feature.Category); Generated" `
            -Priority $feature.Priority
            
        if ($result.Success) {
            Write-Host "✓ Created Feature #$($result.Id)" -ForegroundColor Green
            $createdFeatures += @{
                Id = $result.Id
                Title = $feature.Title
                Url = $result.Url
            }
        }
        else {
            Write-Host "✗ Failed to create Feature" -ForegroundColor Red
            Write-Host "  Error: $($result.Error)" -ForegroundColor Red
            $failedFeatures += $feature.Title
        }
    }
}

# Summary
if ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the features." -ForegroundColor Cyan
}
else {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Feature Creation Summary" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Successfully Created: $($createdFeatures.Count) features" -ForegroundColor Green
    if ($failedFeatures.Count -gt 0) {
        Write-Host "Failed: $($failedFeatures.Count) features" -ForegroundColor Red
    }
    
    if ($createdFeatures.Count -gt 0) {
        Write-Host "`nCreated Features:" -ForegroundColor Cyan
        foreach ($created in $createdFeatures) {
            Write-Host "  - Feature #$($created.Id): $($created.Title)" -ForegroundColor White
        }
        
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "1. Review and refine each feature's requirements" -ForegroundColor White
        Write-Host "2. Run: build-stories.ps1 -FeatureId <FEATURE_ID> for each feature" -ForegroundColor White
    }
}