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
    ./build-vision-strategy-enhanced.ps1 -VisionEpicId 10087 -Preview
    ./build-vision-strategy-enhanced.ps1 -VisionEpicId 10087 -AutoApprove
    ./build-vision-strategy-enhanced.ps1 -VisionEpicId 10087 -Timeframe 24
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
$organization = $config.azureDevOps.organization
$project = $config.azureDevOps.project
$pat = $config.azureDevOps.personalAccessToken
$apiVersion = $config.azureDevOps.apiVersion

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

# Function to extract vision information
function Get-VisionInfo {
    param($VisionWorkItem)
    
    $title = $VisionWorkItem.fields.'System.Title'
    $description = $VisionWorkItem.fields.'System.Description'
    
    # Extract product name from title
    $productName = if ($title -match "Product Vision - (.+)") { $Matches[1] } else { "Unknown Product" }
    
    # Extract strategic themes
    $themes = @()
    if ($description -match '<h3>Strategic Themes</h3>(.+?)<h3>') {
        $themeSection = $Matches[1]
        $themeMatches = [regex]::Matches($themeSection, '<strong>(.+?):</strong>\s*(.+?)(?=<li>|</ol>)')
        foreach ($match in $themeMatches) {
            $themes += @{
                Name = $match.Groups[1].Value -replace '\[|\]', ''
                Description = $match.Groups[2].Value -replace '<[^>]+>', ''
            }
        }
    }
    
    # Extract success metrics
    $metrics = @()
    if ($description -match '<h3>Success Metrics.*?</h3>(.+?)<h3>') {
        $metricsSection = $Matches[1]
        $metricMatches = [regex]::Matches($metricsSection, '<strong>(.+?):</strong>\s*(.+?)(?=<li>|</ul>)')
        foreach ($match in $metricMatches) {
            $metrics += @{
                Name = $match.Groups[1].Value
                Target = $match.Groups[2].Value
            }
        }
    }
    
    return @{
        ProductName = $productName
        Themes = $themes
        Metrics = $metrics
        FullDescription = $description
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
    
    $currentDate = Get-Date -Format "yyyy-MM-dd"
    $currentYear = (Get-Date).Year
    $endDate = (Get-Date).AddMonths($TimeframeMonths)
    $quarters = [Math]::Ceiling($TimeframeMonths / 3)
    
    $strategyHtml = @"
<h1>Vision Strategy - $ProductName</h1>

<h2>Strategy Overview</h2>

<h3>Strategic Context</h3>
<p><strong>Product Vision Reference:</strong> Epic #$VisionEpicId</p>
<p><strong>Strategy Timeframe:</strong> $TimeframeMonths months ($(Get-Date -Format "MMM yyyy") - $($endDate.ToString("MMM yyyy")))</p>
<p><strong>Last Updated:</strong> $currentDate</p>

<h3>Executive Summary</h3>
<p>This strategy outlines our $TimeframeMonths-month roadmap to deliver on the Product Vision for $ProductName. 
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
<li>Start of Quarter: [Initial milestone]</li>
<li>Mid-Quarter: [Progress milestone]</li>
<li>End of Quarter: [Completion milestone]</li>
</ul>
"@
    }
    
    # Add success metrics cascade
    $strategyHtml += @"

<h2>Metrics Cascade</h2>
<p>How Vision metrics translate to Strategy targets:</p>
<table>
<tr><th>Vision Metric</th><th>Strategy Target (End of Period)</th><th>Quarterly Breakdown</th></tr>
"@

    foreach ($metric in $Metrics) {
        $strategyHtml += @"
<tr>
<td>$($metric.Name): $($metric.Target)</td>
<td>[Specific target for strategy period]</td>
<td>[Q1: X%, Q2: Y%, Q3: Z%, Q4: 100%]</td>
</tr>
"@
    }
    
    $strategyHtml += @"
</table>

<h2>Resource Requirements</h2>
<ul>
<li><strong>Team Size:</strong> [Number of people needed]</li>
<li><strong>Key Roles:</strong> [Critical roles to fill]</li>
<li><strong>Budget Estimate:</strong> [Total investment required]</li>
<li><strong>Technology Stack:</strong> [Key technologies needed]</li>
</ul>

<h2>Risk Mitigation</h2>
<ul>
<li><strong>Technical Risk:</strong> [Risk and mitigation strategy]</li>
<li><strong>Market Risk:</strong> [Risk and mitigation strategy]</li>
<li><strong>Resource Risk:</strong> [Risk and mitigation strategy]</li>
<li><strong>Timeline Risk:</strong> [Risk and mitigation strategy]</li>
</ul>

<h2>Success Criteria</h2>
<p>This strategy will be considered successful when:</p>
<ul>
<li>All quarterly milestones are achieved on schedule</li>
<li>Key results for each initiative meet or exceed targets</li>
<li>Vision metrics show positive trajectory</li>
<li>Customer feedback validates strategic direction</li>
<li>Team morale and productivity remain high</li>
</ul>

<hr>
<p><em>Strategy generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm")</em></p>
<p><em>Next Review: End of Q1</em></p>
"@

    return $strategyHtml
}

# Function to analyze strategy completeness
function Test-StrategyCompleteness {
    param([string]$Content)
    
    $placeholders = ([regex]::Matches($Content, '\[.+?\]')).Count
    $totalSections = 10  # Approximate number of sections needing completion
    $completeness = [Math]::Max(0, (1 - ($placeholders / ($totalSections * 3))) * 100)
    
    return @{
        Completeness = $completeness
        PlaceholderCount = $placeholders
        EstimatedEffort = "$([Math]::Ceiling($placeholders * 5)) minutes to complete"
    }
}

# Function to show strategy preview
function Show-StrategyPreview {
    param(
        [string]$Content,
        [string]$ProductName,
        [hashtable]$Analysis
    )
    
    Write-Host "`n=== STRATEGY PREVIEW ===" -ForegroundColor Cyan
    Write-Host "Product: $ProductName" -ForegroundColor White
    Write-Host "Completeness: $($Analysis.Completeness)%" -ForegroundColor $(if ($Analysis.Completeness -ge 70) { "Green" } else { "Yellow" })
    Write-Host "Placeholders to fill: $($Analysis.PlaceholderCount)" -ForegroundColor Yellow
    Write-Host "Estimated effort: $($Analysis.EstimatedEffort)" -ForegroundColor White
    
    Write-Host "`nStrategy Structure:" -ForegroundColor Cyan
    $sections = [regex]::Matches($Content, '<h[23]>(.+?)</h[23]>')
    foreach ($section in $sections) {
        $level = if ($section.Value -match '<h2>') { "  " } else { "    - " }
        Write-Host "$level$($section.Groups[1].Value)" -ForegroundColor White
    }
    
    Write-Host "`n================================" -ForegroundColor Cyan
}

# Function to prompt for approval
function Get-UserApproval {
    param(
        [string]$Message = "Do you want to proceed with creating the strategy?"
    )
    
    Write-Host "`n$Message" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No  [V] View full content" -ForegroundColor White
    
    $response = Read-Host "Choice"
    return $response -match '^[Yy]'
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
        
        # Debug output
        Write-Host "`nDEBUG - Request URL: $url" -ForegroundColor Magenta
        Write-Host "DEBUG - Request Body:" -ForegroundColor Magenta
        Write-Host $body -ForegroundColor Gray
        
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
$visionInfo = Get-VisionInfo -VisionWorkItem $visionWorkItem
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
    $shouldProceed = Get-UserApproval
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create the strategy
$strategyTags = "Vision Strategy; $($visionInfo.ProductName); Strategic Planning; Generated"

Write-Host "`nCreating Vision Strategy Epic..." -ForegroundColor Yellow
$resultId = Save-StrategyWorkItem `
    -Title $strategyTitle `
    -Description $strategyContent `
    -ParentId $VisionEpicId `
    -Tags $strategyTags `
    -DryRun:$Preview

if ($resultId -and $resultId -ne "PREVIEW") {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Vision Strategy Created Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Strategy Epic ID: $resultId" -ForegroundColor Cyan
    Write-Host "Title: $strategyTitle" -ForegroundColor Cyan
    Write-Host "Parent Vision Epic: $VisionEpicId" -ForegroundColor Cyan
    Write-Host "`nView the Strategy in Azure DevOps:" -ForegroundColor Yellow
    Write-Host "https://$organization.visualstudio.com/$project/_workitems/edit/$resultId" -ForegroundColor White
    
    Write-Host "`nNext Steps:" -ForegroundColor Yellow
    Write-Host "1. Fill in the $($analysis.PlaceholderCount) placeholder sections" -ForegroundColor White
    Write-Host "2. Run: build-epics.ps1 -StrategyEpicId $resultId" -ForegroundColor White
    Write-Host "3. Review generated epics for each initiative" -ForegroundColor White
    Write-Host "4. Continue with feature breakdown" -ForegroundColor White
    
    # Save strategy reference
    $strategyFile = Join-Path -Path $PSScriptRoot -ChildPath "../../docs/strategies/$($visionInfo.ProductName -replace ' ', '-')-strategy.json"
    $strategyDir = Split-Path $strategyFile -Parent
    if (-not (Test-Path $strategyDir)) {
        New-Item -ItemType Directory -Path $strategyDir -Force | Out-Null
    }
    
    @{
        strategyId = $resultId
        visionId = $VisionEpicId
        productName = $visionInfo.ProductName
        title = $strategyTitle
        createdDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        createdBy = $env:USERNAME
        timeframe = $Timeframe
        themes = $visionInfo.Themes | ForEach-Object { $_.Name }
        url = "https://$organization.visualstudio.com/$project/_workitems/edit/$resultId"
    } | ConvertTo-Json | Set-Content -Path $strategyFile
    
    Write-Host "`nStrategy reference saved to: $strategyFile" -ForegroundColor Gray
}
elseif ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the strategy." -ForegroundColor Cyan
}
else {
    Write-Host "`nFailed to create Vision Strategy Epic" -ForegroundColor Red
}