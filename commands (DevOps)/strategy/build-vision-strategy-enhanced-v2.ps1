#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates a Vision Strategy Epic in Azure DevOps based on Product Vision content.

.DESCRIPTION
    This script analyzes a Product Vision Epic and generates a comprehensive Vision Strategy
    with approval workflow, preview mode, and intelligent content generation.

.PARAMETER VisionEpicId
    The ID of the Product Vision Epic to base the strategy on.

.PARAMETER Preview
    Shows what would be created without actually creating it.

.PARAMETER AutoApprove
    Skip approval prompt and create immediately (use with caution).

.PARAMETER Timeframe
    Strategy timeframe in months (default: 18)

.EXAMPLE
    ./build-vision-strategy-enhanced-v2.ps1 -VisionEpicId 10087 -Preview
    ./build-vision-strategy-enhanced-v2.ps1 -VisionEpicId 10087 -AutoApprove
    ./build-vision-strategy-enhanced-v2.ps1 -VisionEpicId 10087 -Timeframe 24
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$VisionEpicId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [int]$Timeframe = 18
)

# Read configuration
$configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
if (-not (Test-Path $configPath)) {
    Write-Host "Error: Configuration file not found at $configPath" -ForegroundColor Red
    exit 1
}

$config = Get-Content $configPath -Raw | ConvertFrom-Json
$script:organization = $config.azureDevOps.organization
$script:project = $config.azureDevOps.project
$script:pat = $config.azureDevOps.personalAccessToken
$script:apiVersion = $config.azureDevOps.apiVersion
$script:config = $config

# Function to fetch vision details
function Get-VisionDetails {
    param([string]$EpicId)
    
    $url = "https://$script:organization.visualstudio.com/$script:project/_apis/wit/workitems/$EpicId`?api-version=$script:apiVersion"
    
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$script:pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json"
    }
    
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
        return $response
    }
    catch {
        Write-Host "Failed to fetch Vision Epic details" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# Function to sync and get vision information
function Get-VisionInfo {
    param(
        $VisionWorkItem,
        [string]$VisionEpicId
    )
    
    $title = $VisionWorkItem.fields.'System.Title' -replace '\*\*', ''
    
    # Extract product name from title
    $productName = if ($title -match "Product Vision - (.+)") { $Matches[1] } else { "Unknown Product" }
    
    # Check if we have a JSON file with synchronized data
    $visionDir = Join-Path $PSScriptRoot "../../docs/visions"
    $jsonFileName = "$($productName -replace ' ', '-')-vision.json"
    $jsonPath = Join-Path $visionDir $jsonFileName
    
    # Sync vision data if JSON doesn't exist or is outdated
    if (-not (Test-Path $jsonPath)) {
        Write-Host "Vision data not found locally. Synchronizing from Azure DevOps..." -ForegroundColor Yellow
        $syncScript = Join-Path $PSScriptRoot "../../scripts/sync-vision-data.ps1"
        if (Test-Path $syncScript) {
            & $syncScript -VisionEpicId $VisionEpicId
        }
    }
    
    # Load data from JSON if available
    if (Test-Path $jsonPath) {
        Write-Host "Loading vision data from: $jsonFileName" -ForegroundColor DarkGray
        $visionData = Get-Content $jsonPath | ConvertFrom-Json
        
        # Convert themes hashtable to array format expected by strategy
        $themes = @()
        if ($visionData.themes) {
            foreach ($theme in $visionData.themes.PSObject.Properties) {
                $themes += @{
                    Name = $theme.Name
                    Description = $theme.Value
                }
            }
        }
        
        # Convert metrics to expected format
        $metrics = @()
        if ($visionData.metrics) {
            foreach ($metric in $visionData.metrics) {
                $metrics += @{
                    Name = $metric.Metric
                    Current = $metric.Current
                    Target = $metric.Target
                    Timeline = $metric.Timeline
                }
            }
        }
        
        return @{
            ProductName = $visionData.productName
            Themes = $themes
            Metrics = $metrics
            VisionStatement = $visionData.visionStatement
            TargetAudience = $visionData.targetAudience
        }
    }
    else {
        # Fallback to HTML parsing if JSON not available
        Write-Host "Warning: Using fallback HTML parsing. Run sync-vision-data.ps1 for better results." -ForegroundColor Yellow
        return @{
            ProductName = $productName
            Themes = @()
            Metrics = @()
        }
    }
}

# Function to generate strategy content
function New-StrategyContent {
    param(
        [string]$ProductName,
        [array]$Themes,
        [array]$Metrics,
        [int]$TimeframeMonths,
        [string]$VisionEpicId
    )
    
    $quarters = [Math]::Ceiling($TimeframeMonths / 3)
    $currentDate = Get-Date
    
    $strategyHtml = @"
<h1>Vision Strategy - $ProductName</h1>

<h2>Strategy Overview</h2>
<h3>Strategic Context</h3>
<p>This vision strategy operationalizes the Product Vision for $ProductName over the next $TimeframeMonths months.
It translates the vision's strategic themes into actionable initiatives with clear milestones and success metrics.</p>

<h2>Strategic Initiatives</h2>
"@

    # Generate initiatives from themes
    $initiativeCount = 1
    foreach ($theme in $Themes) {
        $strategyHtml += @"

<h3>Initiative $($initiativeCount): $($theme.Name)</h3>
<p><strong>Vision Theme:</strong> $($theme.Description)</p>
<p><strong>Strategic Objective:</strong> [TO BE DEFINED: Specific objective for this initiative]</p>
<p><strong>Key Results:</strong></p>
<ul>
<li>[KR1: Measurable outcome aligned with theme]</li>
<li>[KR2: Measurable outcome aligned with theme]</li>
<li>[KR3: Measurable outcome aligned with theme]</li>
</ul>
<p><strong>Success Metrics:</strong></p>
<ul>
<li>Leading Indicator: [Metric that predicts success]</li>
<li>Lagging Indicator: [Metric that confirms success]</li>
<li>Quality Metric: [Metric that ensures quality]</li>
</ul>
"@
        $initiativeCount++
    }
    
    # Generate quarterly roadmap
    $strategyHtml += @"

<h2>Quarterly Roadmap</h2>
"@

    for ($q = 1; $q -le $quarters; $q++) {
        $quarterStart = (Get-Date).AddMonths(($q - 1) * 3)
        $quarterName = "Q$((($quarterStart.Month - 1) / 3) + 1) $($quarterStart.Year)"
        
        $strategyHtml += @"

<h3>$quarterName - [Theme Focus]</h3>
<p><strong>Primary Focus:</strong> [Main deliverable theme for this quarter]</p>
<p><strong>Key Deliverables:</strong></p>
<ul>
<li>[Deliverable 1 aligned with initiatives]</li>
<li>[Deliverable 2 aligned with initiatives]</li>
<li>[Deliverable 3 aligned with initiatives]</li>
</ul>
<p><strong>Milestones:</strong></p>
<ul>
<li>[Milestone 1: Date and deliverable]</li>
<li>[Milestone 2: Date and deliverable]</li>
</ul>
"@
    }
    
    # Add metrics cascade
    $strategyHtml += @"

<h2>Metrics Cascade</h2>
<p>The following metrics will be tracked to measure strategy success:</p>
<table>
<tr><th>Vision Metric</th><th>Strategy Target</th><th>Q1 Target</th><th>Q2 Target</th><th>Q3 Target</th><th>Q4 Target</th></tr>
"@

    foreach ($metric in $Metrics) {
        # Extract target value from metric
        $targetValue = if ($metric.Target) { $metric.Target } elseif ($metric.Year1) { $metric.Year1 } else { "[TBD]" }
        $currentValue = if ($metric.Current) { " (from $($metric.Current))" } else { "" }
        
        $strategyHtml += @"
<tr>
<td>$($metric.Name)</td>
<td>$targetValue$currentValue</td>
<td>[Q1 Target]</td>
<td>[Q2 Target]</td>
<td>[Q3 Target]</td>
<td>[Q4 Target]</td>
</tr>
"@
    }
    
    $strategyHtml += @"
</table>

<h2>Resource Requirements</h2>
<ul>
<li><strong>Team Size:</strong> [Number of FTEs required]</li>
<li><strong>Budget:</strong> [Total budget required]</li>
<li><strong>Key Skills:</strong> [Critical skills needed]</li>
<li><strong>Dependencies:</strong> [External dependencies]</li>
</ul>

<h2>Risk Mitigation</h2>
<table>
<tr><th>Risk</th><th>Impact</th><th>Probability</th><th>Mitigation</th></tr>
<tr><td>[Risk 1]</td><td>High/Medium/Low</td><td>High/Medium/Low</td><td>[Mitigation strategy]</td></tr>
<tr><td>[Risk 2]</td><td>High/Medium/Low</td><td>High/Medium/Low</td><td>[Mitigation strategy]</td></tr>
<tr><td>[Risk 3]</td><td>High/Medium/Low</td><td>High/Medium/Low</td><td>[Mitigation strategy]</td></tr>
</table>

<h2>Success Criteria</h2>
<ul>
<li>[Success criterion 1: Measurable outcome]</li>
<li>[Success criterion 2: Measurable outcome]</li>
<li>[Success criterion 3: Measurable outcome]</li>
</ul>

<hr>
<p><em>Strategy created: $(Get-Date -Format 'yyyy-MM-dd')</em></p>
<p><em>Vision Epic: #$VisionEpicId</em></p>
<p><em>Timeframe: $TimeframeMonths months</em></p>
"@

    return $strategyHtml
}

# Function to analyze strategy completeness
function Test-StrategyCompleteness {
    param([string]$Content)
    
    $placeholders = ([regex]::Matches($Content, '\[.+?\]')).Count
    $completeness = if ($placeholders -eq 0) { 100 } else { [Math]::Round((1 - ($placeholders / 100)) * 100) }
    
    return @{
        PlaceholderCount = $placeholders
        Completeness = $completeness
        EstimatedEffort = $placeholders * 5  # 5 minutes per placeholder
    }
}

# Function to show strategy preview
function Show-StrategyPreview {
    param(
        [string]$Content,
        [string]$ProductName,
        [hashtable]$Analysis
    )
    
    Write-Host "`n=== STRATEGY PREVIEW ===" -ForegroundColor Yellow
    Write-Host "Product: $ProductName" -ForegroundColor White
    Write-Host "Completeness: $($Analysis.Completeness)%" -ForegroundColor $(if ($Analysis.Completeness -gt 80) { 'Green' } elseif ($Analysis.Completeness -gt 50) { 'Yellow' } else { 'Red' })
    Write-Host "Placeholders to fill: $($Analysis.PlaceholderCount)" -ForegroundColor White
    Write-Host "Estimated effort: $($Analysis.EstimatedEffort) minutes to complete" -ForegroundColor White
    
    # Show structure
    Write-Host "`nStrategy Structure:" -ForegroundColor Cyan
    $sections = [regex]::Matches($Content, '<h[23]>(.+?)</h[23]>')
    foreach ($section in $sections) {
        $level = if ($section.Value.StartsWith('<h2>')) { "  " } else { "    - " }
        Write-Host "$level$($section.Groups[1].Value)" -ForegroundColor Gray
    }
    
    Write-Host "`n================================" -ForegroundColor Cyan
}

# Enhanced approval function with View option
function Get-UserApproval {
    param(
        [string]$Message = "Do you want to proceed with creating the strategy?",
        [string]$Content = ""
    )
    
    while ($true) {
        Write-Host "`n$Message" -ForegroundColor Yellow
        Write-Host "[Y] Yes  [N] No  [V] View full content" -ForegroundColor White
        
        $response = Read-Host "Choice"
        
        switch -Regex ($response) {
            '^[Yy]' {
                return $true
            }
            '^[Nn]' {
                return $false
            }
            '^[Vv]' {
                if ($Content) {
                    Write-Host "`n=== FULL STRATEGY CONTENT ===" -ForegroundColor Cyan
                    Write-Host "============================================================" -ForegroundColor DarkGray
                    
                    # Display content without HTML tags for readability
                    $cleanContent = $Content -replace '<[^>]+>', ''
                    $cleanContent = $cleanContent -replace '&lt;', '<'
                    $cleanContent = $cleanContent -replace '&gt;', '>'
                    $cleanContent = $cleanContent -replace '&amp;', '&'
                    
                    Write-Host $cleanContent -ForegroundColor White
                    
                    Write-Host "`n============================================================" -ForegroundColor DarkGray
                    Write-Host "=== END OF CONTENT ===" -ForegroundColor Cyan
                }
                else {
                    Write-Host "No content available to view." -ForegroundColor Yellow
                }
                # Continue the loop to ask again
            }
            default {
                Write-Host "Invalid choice. Please enter Y, N, or V." -ForegroundColor Red
            }
        }
    }
}

# Function to save work item
function Save-StrategyWorkItem {
    param(
        [string]$Title,
        [string]$Description,
        [string]$ParentId,
        [string]$Tags,
        [bool]$DryRun = $false
    )
    
    if ($DryRun) {
        Write-Host "`n=== PREVIEW MODE ===" -ForegroundColor Yellow
        Write-Host "Would create Strategy Epic with:" -ForegroundColor Cyan
        Write-Host "  Title: $Title" -ForegroundColor White
        Write-Host "  Parent: Epic #$ParentId" -ForegroundColor White
        Write-Host "  Tags: $Tags" -ForegroundColor White
        return "PREVIEW"
    }
    
    $operations = @(
        @{
            op = "add"
            path = "/fields/System.Title"
            value = $Title
        },
        @{
            op = "add"
            path = "/fields/System.Description"
            value = $Description
        },
        @{
            op = "add"
            path = "/fields/Microsoft.VSTS.Common.Priority"
            value = 1
        },
        @{
            op = "add"
            path = "/fields/System.AreaPath"
            value = $script:config.azureDevOps.workItemSettings.defaultAreaPath
        },
        @{
            op = "add"
            path = "/fields/System.IterationPath"
            value = $script:config.azureDevOps.workItemSettings.defaultIterationPath
        },
        @{
            op = "add"
            path = "/fields/System.Tags"
            value = $Tags
        },
        @{
            op = "add"
            path = "/relations/-"
            value = @{
                rel = "System.LinkTypes.Hierarchy-Reverse"
                url = "https://$script:organization.visualstudio.com/$script:project/_apis/wit/workitems/$ParentId"
            }
        }
    )
    
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$script:pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json-patch+json"
    }
    
    $url = "https://$script:organization.visualstudio.com/$script:project/_apis/wit/workitems/`$Epic?api-version=$script:apiVersion"
    
    try {
        $body = $operations | ConvertTo-Json -Depth 10
        $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body
        return $response.id
    }
    catch {
        Write-Host "Failed to create Strategy Epic" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# Main execution
Write-Host "`nVision Strategy Builder for Azure DevOps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Workflow Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })

# Fetch vision details
Write-Host "`nFetching Product Vision details (Epic #$VisionEpicId)..." -ForegroundColor Yellow
$visionWorkItem = Get-VisionDetails -EpicId $VisionEpicId

if (-not $visionWorkItem) {
    Write-Host "Failed to fetch Vision Epic. Please check the Epic ID." -ForegroundColor Red
    exit 1
}

# Extract vision information
$visionInfo = Get-VisionInfo -VisionWorkItem $visionWorkItem -VisionEpicId $VisionEpicId
Write-Host "Product Vision: $($visionInfo.ProductName)" -ForegroundColor Green
Write-Host "Strategic Themes Found: $($visionInfo.Themes.Count)" -ForegroundColor Green
Write-Host "Success Metrics Found: $($visionInfo.Metrics.Count)" -ForegroundColor Green

# Generate strategy content
Write-Host "`nGenerating Vision Strategy content..." -ForegroundColor Yellow
$strategyTitle = "**Vision Strategy - $($visionInfo.ProductName)"
$strategyContent = New-StrategyContent `
    -ProductName $visionInfo.ProductName `
    -Themes $visionInfo.Themes `
    -Metrics $visionInfo.Metrics `
    -TimeframeMonths $Timeframe `
    -VisionEpicId $VisionEpicId

# Analyze strategy
$analysis = Test-StrategyCompleteness -Content $strategyContent
Show-StrategyPreview -Content $strategyContent -ProductName $visionInfo.ProductName -Analysis $analysis

# Check if auto-approve is set
$shouldProceed = $AutoApprove -or $Preview

# If not auto-approved and not preview, ask for approval
if (-not $shouldProceed) {
    $shouldProceed = Get-UserApproval -Content $strategyContent
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create the strategy
Write-Host "`nCreating Vision Strategy Epic..." -ForegroundColor Yellow
$tags = "Vision Strategy; $($visionInfo.ProductName); Strategic Planning; Generated"
$strategyId = Save-StrategyWorkItem `
    -Title $strategyTitle `
    -Description $strategyContent `
    -ParentId $VisionEpicId `
    -Tags $tags `
    -DryRun:$Preview

if ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the strategy." -ForegroundColor Gray
}
elseif ($strategyId) {
    Write-Host "`n[SUCCESS] Vision Strategy created!" -ForegroundColor Green
    Write-Host "Strategy Epic ID: $strategyId" -ForegroundColor White
    Write-Host "View in Azure DevOps: https://$script:organization.visualstudio.com/$script:project/_workitems/edit/$strategyId" -ForegroundColor Cyan
    
    Write-Host "`n[NEXT] Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Review and fill in placeholders in the strategy" -ForegroundColor White
    Write-Host "2. Create epics: ./Create-Epics.ps1 $strategyId" -ForegroundColor White
    Write-Host "3. Build quarterly roadmap details" -ForegroundColor White
}
else {
    Write-Host "`n[ERROR] Failed to create Vision Strategy" -ForegroundColor Red
}