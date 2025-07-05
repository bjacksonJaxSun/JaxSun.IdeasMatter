#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates User Story work items in Azure DevOps based on Feature requirements.

.DESCRIPTION
    This script analyzes a Feature work item and generates 1-40 User Story work items
    using the standard story template format.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$FeatureId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxStories = 40
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to generate story content
function New-StoryContent {
    param(
        [string]$StoryTitle,
        [string]$AsA,
        [string]$IWant,
        [string]$SoThat,
        [string]$FeatureTitle,
        [string]$StoryType,
        [int]$StoryNumber,
        [int]$TotalStories
    )
    
    $storyHtml = @"
<h2>User Story: $StoryTitle</h2>

<h3>Story</h3>
<p><strong>As a</strong> $AsA<br/>
<strong>I want</strong> $IWant<br/>
<strong>So that</strong> $SoThat</p>

<h3>Story Details</h3>

<h4>Overview</h4>
<p>This story implements $StoryType functionality for the $FeatureTitle feature. Priority $StoryNumber of $TotalStories stories in this feature.</p>

<h4>User Scenario</h4>
<ol>
<li>User navigates to relevant section</li>
<li>User initiates the action</li>
<li>System validates input/permissions</li>
<li>System processes the request</li>
<li>User receives feedback/confirmation</li>
<li>User can proceed with next action</li>
</ol>

<h4>Acceptance Criteria</h4>
<p><strong>Given</strong> a valid user session<br/>
   <strong>When</strong> the user performs the action<br/>
   <strong>Then</strong> the system should respond appropriately</p>

<p><strong>Given</strong> invalid input data<br/>
   <strong>When</strong> the user submits the form<br/>
   <strong>Then</strong> appropriate validation messages should appear</p>

<p><strong>Given</strong> a system error occurs<br/>
   <strong>When</strong> processing the request<br/>
   <strong>Then</strong> user should see a friendly error message</p>

<h4>Business Rules</h4>
<ul>
<li>Business rule 1 specific to this story</li>
<li>Business rule 2 specific to this story</li>
<li>Business rule 3 specific to this story</li>
</ul>

<h4>Technical Considerations</h4>
<ul>
<li><strong>API Changes:</strong> Required endpoints or modifications</li>
<li><strong>Database Changes:</strong> Schema updates if needed</li>
<li><strong>Security:</strong> Authentication/authorization requirements</li>
<li><strong>Performance:</strong> Expected load and response time</li>
</ul>

<h4>Definition of Done</h4>
<ul>
<li>Code complete and follows standards</li>
<li>Unit tests written and passing (>80% coverage)</li>
<li>Integration tests written and passing</li>
<li>Code reviewed and approved</li>
<li>Documentation updated</li>
<li>Deployed to staging</li>
</ul>

<h4>Estimation</h4>
<ul>
<li><strong>Story Points:</strong> 3</li>
<li><strong>T-Shirt Size:</strong> M</li>
<li><strong>Effort Hours:</strong> 16-24</li>
</ul>

<hr>
<p><em>Story generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm")</em></p>
"@

    return $storyHtml
}

# Function to generate story personas and types
function Get-StoryVariants {
    param([string]$FeatureTitle)
    
    $personas = @(
        @{ Role = "Administrator"; Goal = "manage and configure the system" },
        @{ Role = "End User"; Goal = "complete daily tasks efficiently" },
        @{ Role = "Manager"; Goal = "oversee team operations" },
        @{ Role = "System"; Goal = "automate processes" }
    )
    
    $storyTypes = @(
        @{ Type = "Setup"; Description = "Initial configuration" },
        @{ Type = "Core"; Description = "Primary functionality" },
        @{ Type = "Advanced"; Description = "Advanced features" },
        @{ Type = "Reporting"; Description = "Analytics and reports" }
    )
    
    $stories = @()
    $count = 0
    
    foreach ($storyType in $storyTypes) {
        foreach ($persona in $personas) {
            if ($count -ge $MaxStories) { break }
            
            $stories += @{
                Title = "$($storyType.Type) - $($persona.Role)"
                AsA = $persona.Role
                IWant = "to access $($storyType.Description) capabilities"
                SoThat = "I can $($persona.Goal)"
                Type = $storyType.Type
                Priority = $count + 1
            }
            $count++
        }
        if ($count -ge $MaxStories) { break }
    }
    
    return $stories
}

# Main execution
Write-Host "`nUser Story Builder for Azure DevOps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Workflow Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })

# Fetch feature details
Write-Host "`nFetching Feature details (Feature #$FeatureId)..." -ForegroundColor Yellow
$featureWorkItem = Get-AzureDevOpsWorkItem -WorkItemId $FeatureId

if (-not $featureWorkItem) {
    Write-Host "Failed to fetch Feature. Please check the Feature ID." -ForegroundColor Red
    exit 1
}

$featureTitle = $featureWorkItem.fields.'System.Title'
$tags = $featureWorkItem.fields.'System.Tags' -split ';'
$productName = $tags | Where-Object { $_ -notmatch 'Generated|Feature' } | Select-Object -First 1

Write-Host "Feature: $featureTitle" -ForegroundColor Green
Write-Host "Product: $productName" -ForegroundColor Green

# Generate stories
$stories = Get-StoryVariants -FeatureTitle $featureTitle

# Show preview
Write-Host "`n=== STORY GENERATION PREVIEW ===" -ForegroundColor Cyan
Write-Host "Total Stories to Create: $($stories.Count)" -ForegroundColor White
Write-Host "`nStory Breakdown:" -ForegroundColor Cyan
foreach ($story in $stories) {
    Write-Host "  - $($story.Title) (Priority: $($story.Priority))" -ForegroundColor White
}
Write-Host "`n================================" -ForegroundColor Cyan

# Check approval
$shouldProceed = $AutoApprove -or $Preview
if (-not $shouldProceed) {
    Write-Host "`nDo you want to proceed with creating these stories?" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No" -ForegroundColor White
    $response = Read-Host "Choice"
    $shouldProceed = $response -match '^[Yy]'
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create stories
$createdStories = @()
$failedStories = @()

foreach ($story in $stories) {
    Write-Host "`nCreating Story: $($story.Title)..." -ForegroundColor Yellow
    
    if ($Preview) {
        Write-Host "PREVIEW: Would create Story '$($story.Title)'" -ForegroundColor Cyan
        $createdStories += @{
            Id = "PREVIEW"
            Title = $story.Title
        }
    }
    else {
        $storyContent = New-StoryContent `
            -StoryTitle "$featureTitle - $($story.Title)" `
            -AsA $story.AsA `
            -IWant $story.IWant `
            -SoThat $story.SoThat `
            -FeatureTitle $featureTitle `
            -StoryType $story.Type `
            -StoryNumber $story.Priority `
            -TotalStories $stories.Count
            
        $result = New-AzureDevOpsWorkItem `
            -WorkItemType "User Story" `
            -Title "$featureTitle - $($story.Title)" `
            -Description $storyContent `
            -ParentId $FeatureId `
            -Tags "$productName; User Story; $($story.Type); Generated" `
            -Priority $story.Priority
            
        if ($result.Success) {
            Write-Host "✓ Created Story #$($result.Id)" -ForegroundColor Green
            $createdStories += @{
                Id = $result.Id
                Title = $story.Title
                Url = $result.Url
            }
        }
        else {
            Write-Host "✗ Failed to create Story" -ForegroundColor Red
            Write-Host "  Error: $($result.Error)" -ForegroundColor Red
            $failedStories += $story.Title
        }
    }
}

# Summary
if ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the stories." -ForegroundColor Cyan
}
else {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Story Creation Summary" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Successfully Created: $($createdStories.Count) stories" -ForegroundColor Green
    if ($failedStories.Count -gt 0) {
        Write-Host "Failed: $($failedStories.Count) stories" -ForegroundColor Red
    }
    
    if ($createdStories.Count -gt 0) {
        Write-Host "`nCreated Stories:" -ForegroundColor Cyan
        foreach ($created in $createdStories) {
            Write-Host "  - Story #$($created.Id): $($created.Title)" -ForegroundColor White
        }
        
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "1. Review and refine each story's acceptance criteria" -ForegroundColor White
        Write-Host "2. Run: build-test-cases.ps1 -StoryId <STORY_ID> for each story" -ForegroundColor White
        Write-Host "3. Run: build-tasks.ps1 -StoryId <STORY_ID> for each story" -ForegroundColor White
    }
}