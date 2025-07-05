#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates or updates a Product Vision Epic in Azure DevOps.

.DESCRIPTION
    This script creates a new Product Vision Epic or updates an existing one based on a template or custom content.

.PARAMETER VisionName
    The name of the product/system for the vision (e.g., "Bermuda Payroll", "AI Analytics")

.PARAMETER EpicId
    Optional. The ID of an existing epic to update. If not provided, creates a new epic.

.PARAMETER TemplateFile
    Optional. Path to a markdown file containing the vision content. If not provided, uses default template.

.PARAMETER Preview
    Shows what would be created without actually creating it.

.PARAMETER AutoApprove
    Skip approval prompt and create immediately (use with caution).

.EXAMPLE
    ./create-product-vision.ps1 -VisionName "Bermuda Payroll" -Preview
    ./create-product-vision.ps1 -VisionName "AI Analytics" -TemplateFile "templates/ai-vision.md"
    ./create-product-vision.ps1 -EpicId 10087 -VisionName "Bermuda Payroll System" -AutoApprove
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$VisionName,
    
    [Parameter(Mandatory=$false)]
    [string]$EpicId,
    
    [Parameter(Mandatory=$false)]
    [string]$TemplateFile,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove
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

# Function to validate vision content
function Test-VisionCompleteness {
    param([string]$Content)
    
    $required = @(
        'Vision Statement',
        'Target Market',
        'Key Problems',
        'Strategic Themes',
        'Success Metrics',
        'Key Differentiators'
    )
    
    $missing = @()
    foreach ($section in $required) {
        if ($Content -notmatch $section) {
            $missing += $section
        }
    }
    
    $completeness = (($required.Count - $missing.Count) / $required.Count) * 100
    
    return @{
        Completeness = $completeness
        Missing = $missing
        IsComplete = $missing.Count -eq 0
    }
}

# Function to extract strategic themes
function Get-StrategicThemes {
    param([string]$Content)
    
    $themes = @()
    if ($Content -match '<h3>Strategic Themes</h3>(.+?)<h3>') {
        $themeSection = $Matches[1]
        $themeMatches = [regex]::Matches($themeSection, '<strong>\[(.+?)\]:</strong>')
        foreach ($match in $themeMatches) {
            $themes += $match.Groups[1].Value
        }
    }
    
    return $themes
}

# Function to display analysis report
function Show-VisionAnalysis {
    param(
        [string]$Content,
        [string]$ProductName
    )
    
    Write-Host "`n=== VISION ANALYSIS REPORT ===" -ForegroundColor Cyan
    Write-Host "Product: $ProductName" -ForegroundColor White
    Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm')" -ForegroundColor White
    
    # Check completeness
    $validation = Test-VisionCompleteness -Content $Content
    Write-Host "`nCompleteness Score: $($validation.Completeness)%" -ForegroundColor $(if ($validation.Completeness -ge 80) { "Green" } elseif ($validation.Completeness -ge 60) { "Yellow" } else { "Red" })
    
    if ($validation.Missing.Count -gt 0) {
        Write-Host "Missing Sections:" -ForegroundColor Yellow
        $validation.Missing | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
    }
    
    # Extract themes
    $themes = Get-StrategicThemes -Content $Content
    if ($themes.Count -gt 0) {
        Write-Host "`nIdentified Strategic Themes:" -ForegroundColor Cyan
        $themes | ForEach-Object { Write-Host "  - $_" -ForegroundColor White }
    }
    
    # Suggestions
    Write-Host "`nSuggestions for Improvement:" -ForegroundColor Cyan
    if ($Content -match '\[.*?\]') {
        Write-Host "  - Fill in placeholder values marked with [brackets]" -ForegroundColor Yellow
    }
    if ($themes.Count -lt 3) {
        Write-Host "  - Define at least 3 strategic themes" -ForegroundColor Yellow
    }
    if ($Content -notmatch 'TAM|SAM') {
        Write-Host "  - Add market size data (TAM/SAM)" -ForegroundColor Yellow
    }
    
    Write-Host "`n================================" -ForegroundColor Cyan
}

# Function to prompt for approval
function Get-UserApproval {
    param(
        [string]$Message = "Do you want to proceed with creating/updating the vision?"
    )
    
    Write-Host "`n$Message" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No  [E] Edit template first" -ForegroundColor White
    
    $response = Read-Host "Choice"
    return $response -match '^[Yy]'
}

# Function to create or update work item
function Save-WorkItem {
    param(
        [string]$WorkItemType,
        [string]$Title,
        [string]$Description,
        [int]$Priority = 1,
        [string]$Id = "",
        [string]$Tags = "",
        [bool]$DryRun = $false
    )

    if ($DryRun) {
        Write-Host "`n=== PREVIEW MODE ===" -ForegroundColor Yellow
        Write-Host "Would create/update $WorkItemType with:" -ForegroundColor Cyan
        Write-Host "  Title: $Title" -ForegroundColor White
        Write-Host "  Priority: $Priority" -ForegroundColor White
        Write-Host "  Tags: $Tags" -ForegroundColor White
        Write-Host "`nDescription Preview (first 500 chars):" -ForegroundColor Cyan
        Write-Host ($Description.Substring(0, [Math]::Min(500, $Description.Length)) + "...") -ForegroundColor Gray
        return "PREVIEW"
    }

    # Create base operations
    $operations = @()
    
    if ($Id) {
        # Update operations
        $operations += @{
            op = "replace"
            path = "/fields/System.Title"
            value = $Title
        }
        $operations += @{
            op = "replace"
            path = "/fields/System.Description"
            value = $Description
        }
    }
    else {
        # Create operations
        $operations += @{
            op = "add"
            path = "/fields/System.Title"
            value = $Title
        }
        $operations += @{
            op = "add"
            path = "/fields/System.Description"
            value = $Description
        }
        $operations += @{
            op = "add"
            path = "/fields/System.AreaPath"
            value = $config.azureDevOps.workItemSettings.defaultAreaPath
        }
        $operations += @{
            op = "add"
            path = "/fields/System.IterationPath"
            value = $config.azureDevOps.workItemSettings.defaultIterationPath
        }
    }
    
    $operations += @{
        op = $(if ($Id) { "replace" } else { "add" })
        path = "/fields/Microsoft.VSTS.Common.Priority"
        value = $Priority
    }
    
    if ($Tags) {
        $operations += @{
            op = $(if ($Id) { "replace" } else { "add" })
            path = "/fields/System.Tags"
            value = $Tags
        }
    }

    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json-patch+json"
    }

    # Create or update work item
    if ($Id) {
        $url = "https://$organization.visualstudio.com/$project/_apis/wit/workitems/$Id`?api-version=$apiVersion"
        $method = "Patch"
        $action = "Updated"
    }
    else {
        $url = "https://$organization.visualstudio.com/$project/_apis/wit/workitems/`$$WorkItemType`?api-version=$apiVersion"
        $method = "Post"
        $action = "Created"
    }
    
    try {
        $body = $operations | ConvertTo-Json -Depth 10
        $response = Invoke-RestMethod -Uri $url -Method $method -Headers $headers -Body $body
        
        Write-Host "$action ${WorkItemType}: $Title (ID: $($response.id))" -ForegroundColor Green
        return $response.id
    }
    catch {
        Write-Host "Failed to $action ${WorkItemType}: $Title" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# Function to generate vision content
function Get-VisionContent {
    param(
        [string]$ProductName,
        [string]$TemplatePath
    )

    if ($TemplatePath -and (Test-Path $TemplatePath)) {
        Write-Host "Using template from: $TemplatePath" -ForegroundColor Cyan
        $content = Get-Content $TemplatePath -Raw
        # Replace placeholders
        $content = $content -replace '\[PRODUCT_NAME\]', $ProductName
        $content = $content -replace '\[DATE\]', (Get-Date -Format "yyyy-MM-dd")
        return $content
    }

    # Default template if no file provided
    return @"
<h2>Product Vision: $ProductName</h2>

<h3>Vision Statement</h3>
<p><strong>[TO BE DEFINED: One-sentence vision that captures the essence and aspiration of the product]</strong></p>

<h3>Target Market</h3>
<ul>
<li><strong>Primary Market:</strong> [Define primary customer segment]</li>
<li><strong>Secondary Market:</strong> [Define secondary customer segment]</li>
<li><strong>Tertiary Market:</strong> [Define tertiary customer segment]</li>
</ul>

<h3>Key Problems We Solve</h3>
<ol>
<li><strong>[Problem 1]:</strong> [Description of the problem and its impact]</li>
<li><strong>[Problem 2]:</strong> [Description of the problem and its impact]</li>
<li><strong>[Problem 3]:</strong> [Description of the problem and its impact]</li>
</ol>

<h3>Our Solution</h3>
<p>A [type of solution] that:</p>
<ul>
<li>[Key capability 1]</li>
<li>[Key capability 2]</li>
<li>[Key capability 3]</li>
<li>[Key capability 4]</li>
<li>[Key capability 5]</li>
</ul>

<h3>Strategic Themes</h3>
<ol>
<li><strong>[Theme 1]:</strong> [Description of strategic focus area]</li>
<li><strong>[Theme 2]:</strong> [Description of strategic focus area]</li>
<li><strong>[Theme 3]:</strong> [Description of strategic focus area]</li>
<li><strong>[Theme 4]:</strong> [Description of strategic focus area]</li>
</ol>

<h3>Success Metrics (3-Year Horizon)</h3>
<ul>
<li><strong>Market Share:</strong> [Target percentage]</li>
<li><strong>Customer Base:</strong> [Target number of customers]</li>
<li><strong>Revenue Target:</strong> [Target ARR/MRR]</li>
<li><strong>Customer Satisfaction:</strong> [Target NPS/CSAT score]</li>
<li><strong>[Custom Metric]:</strong> [Target value]</li>
</ul>

<h3>Key Differentiators</h3>
<ol>
<li><strong>[Differentiator 1]:</strong> [How we're different/better]</li>
<li><strong>[Differentiator 2]:</strong> [How we're different/better]</li>
<li><strong>[Differentiator 3]:</strong> [How we're different/better]</li>
<li><strong>[Differentiator 4]:</strong> [How we're different/better]</li>
</ol>

<h3>Product Principles</h3>
<ul>
<li><strong>[Principle 1]:</strong> [Guiding principle for all decisions]</li>
<li><strong>[Principle 2]:</strong> [Guiding principle for all decisions]</li>
<li><strong>[Principle 3]:</strong> [Guiding principle for all decisions]</li>
<li><strong>[Principle 4]:</strong> [Guiding principle for all decisions]</li>
</ul>

<h3>5-Year Vision</h3>
<p>By [Year], the $ProductName will be:</p>
<ul>
<li>[Future state achievement 1]</li>
<li>[Future state achievement 2]</li>
<li>[Future state achievement 3]</li>
<li>[Future state achievement 4]</li>
</ul>

<h3>Investment Required</h3>
<ul>
<li><strong>Year 1:</strong> $[Amount] ([Key focus areas])</li>
<li><strong>Year 2:</strong> $[Amount] ([Key focus areas])</li>
<li><strong>Year 3:</strong> $[Amount] ([Key focus areas])</li>
</ul>

<h3>Risk Mitigation</h3>
<ul>
<li><strong>[Risk 1]:</strong> [Mitigation strategy]</li>
<li><strong>[Risk 2]:</strong> [Mitigation strategy]</li>
<li><strong>[Risk 3]:</strong> [Mitigation strategy]</li>
<li><strong>[Risk 4]:</strong> [Mitigation strategy]</li>
</ul>

<hr>
<p><em>Vision created/updated on: $(Get-Date -Format "yyyy-MM-dd HH:mm")</em></p>
"@
}

# Main execution
Write-Host "`nProduct Vision Creator for Azure DevOps" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "Workflow Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })

# Prepare vision content
$visionTitle = "**Product Vision - $VisionName"
$visionDescription = Get-VisionContent -ProductName $VisionName -TemplatePath $TemplateFile
$visionTags = "Product Vision; $VisionName; Strategic Planning"

# Analyze vision content
Show-VisionAnalysis -Content $visionDescription -ProductName $VisionName

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

# Create or update the epic
if ($EpicId) {
    Write-Host "`nUpdating existing Product Vision Epic (ID: $EpicId)..." -ForegroundColor Yellow
    $resultId = Save-WorkItem -WorkItemType "Epic" `
        -Title $visionTitle `
        -Description $visionDescription `
        -Priority 1 `
        -Id $EpicId `
        -Tags $visionTags `
        -DryRun:$Preview
}
else {
    Write-Host "`nCreating new Product Vision Epic..." -ForegroundColor Yellow
    $resultId = Save-WorkItem -WorkItemType "Epic" `
        -Title $visionTitle `
        -Description $visionDescription `
        -Priority 1 `
        -Tags $visionTags `
        -DryRun:$Preview
}

if ($resultId -and $resultId -ne "PREVIEW") {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Product Vision Epic Operation Successful!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Epic ID: $resultId" -ForegroundColor Cyan
    Write-Host "Title: $visionTitle" -ForegroundColor Cyan
    Write-Host "`nView the Epic in Azure DevOps:" -ForegroundColor Yellow
    Write-Host "https://$organization.visualstudio.com/$project/_workitems/edit/$resultId" -ForegroundColor White
    
    if (-not $EpicId) {
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "1. Update the vision content with specific details for $VisionName" -ForegroundColor White
        Write-Host "2. Run: build-vision-strategy.ps1 -VisionEpicId $resultId" -ForegroundColor White
        Write-Host "3. Review and approve the generated strategy" -ForegroundColor White
        Write-Host "4. Continue with epic and feature generation" -ForegroundColor White
    }
    
    # Save the epic ID for future reference
    $visionFile = Join-Path -Path $PSScriptRoot -ChildPath "../../docs/visions/$($VisionName -replace ' ', '-')-vision.json"
    $visionDir = Split-Path $visionFile -Parent
    if (-not (Test-Path $visionDir)) {
        New-Item -ItemType Directory -Path $visionDir -Force | Out-Null
    }
    
    # Create audit trail
    $auditData = @{
        epicId = $resultId
        visionName = $VisionName
        title = $visionTitle
        createdDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        createdBy = $env:USERNAME
        url = "https://$organization.visualstudio.com/$project/_workitems/edit/$resultId"
        completenessScore = (Test-VisionCompleteness -Content $visionDescription).Completeness
        themes = Get-StrategicThemes -Content $visionDescription
    }
    
    $auditData | ConvertTo-Json | Set-Content -Path $visionFile
    
    Write-Host "`nVision reference saved to: $visionFile" -ForegroundColor Gray
}
elseif ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the vision." -ForegroundColor Cyan
}
else {
    Write-Host "`nFailed to create/update Product Vision Epic" -ForegroundColor Red
}