#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates Test Case work items in Azure DevOps with proper test steps.

.DESCRIPTION
    This script analyzes a User Story work item and generates Test Cases with detailed
    test steps including Actions and Expected Results.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxTestCases = 40,
    
    [Parameter(Mandatory=$false)]
    [string]$TestTypes = "Unit,Integration,E2E"
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to create test case with steps
function New-TestCaseWithSteps {
    param(
        [string]$Title,
        [string]$Description,
        [string]$ParentId,
        [string]$Tags,
        [int]$Priority,
        [array]$TestSteps
    )
    
    # Read configuration
    $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $org = $config.azureDevOps.organization
    $proj = $config.azureDevOps.project
    $pat = $config.azureDevOps.personalAccessToken
    $apiVer = $config.azureDevOps.apiVersion
    $areaPath = $config.azureDevOps.workItemSettings.defaultAreaPath
    $iterationPath = $config.azureDevOps.workItemSettings.defaultIterationPath
    
    # Build test steps XML
    $stepsXml = '<steps id="0" last="' + $TestSteps.Count + '">'
    $stepId = 1
    foreach ($step in $TestSteps) {
        $action = [System.Security.SecurityElement]::Escape($step.Action)
        $expectedResult = [System.Security.SecurityElement]::Escape($step.ExpectedResult)
        $attachments = if ($step.Attachments) { $step.Attachments } else { "" }
        
        $stepsXml += '<step id="' + $stepId + '" type="ValidateStep">'
        $stepsXml += '<parameterizedString isformatted="true">&lt;DIV&gt;' + $action + '&lt;/DIV&gt;</parameterizedString>'
        $stepsXml += '<parameterizedString isformatted="true">&lt;DIV&gt;' + $expectedResult + '&lt;/DIV&gt;</parameterizedString>'
        $stepsXml += '<description/>'
        $stepsXml += '</step>'
        $stepId++
    }
    $stepsXml += '</steps>'
    
    # Build operations array
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
            value = $Priority
        },
        @{
            op = "add"
            path = "/fields/System.AreaPath"
            value = $areaPath
        },
        @{
            op = "add"
            path = "/fields/System.IterationPath"
            value = $iterationPath
        },
        @{
            op = "add"
            path = "/fields/System.Tags"
            value = $Tags
        },
        @{
            op = "add"
            path = "/fields/Microsoft.VSTS.TCM.Steps"
            value = $stepsXml
        }
    )
    
    # Add parent link if provided
    if ($ParentId) {
        $operations += @{
            op = "add"
            path = "/relations/-"
            value = @{
                rel = "System.LinkTypes.Hierarchy-Reverse"
                url = "https://$org.visualstudio.com/$proj/_apis/wit/workitems/$ParentId"
            }
        }
    }
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json-patch+json"
    }
    
    $url = "https://$org.visualstudio.com/$proj/_apis/wit/workitems/`$Test%20Case?api-version=$apiVer"
    
    try {
        $body = $operations | ConvertTo-Json -Depth 10
        $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body
        
        return @{
            Success = $true
            Id = $response.id
            Url = $response._links.html.href
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.ToString()
        }
    }
}

# Function to generate test steps based on test type
function Get-TestStepsByType {
    param(
        [string]$TestType,
        [string]$Scenario,
        [string]$StoryTitle
    )
    
    $steps = @()
    
    switch ($TestType) {
        "Unit" {
            switch -Wildcard ($Scenario) {
                "*Positive flow*" {
                    $steps = @(
                        @{ Action = "Arrange: Create test instance with valid parameters"; ExpectedResult = "Instance created successfully without errors" },
                        @{ Action = "Arrange: Set up mock dependencies with expected behavior"; ExpectedResult = "All mocks configured and ready" },
                        @{ Action = "Act: Call the method with valid input data"; ExpectedResult = "Method executes without throwing exceptions" },
                        @{ Action = "Assert: Verify the return value"; ExpectedResult = "Return value matches expected result" },
                        @{ Action = "Assert: Verify mock interactions"; ExpectedResult = "All expected mock calls were made with correct parameters" }
                    )
                }
                "*Negative flow*" {
                    $steps = @(
                        @{ Action = "Arrange: Create test instance"; ExpectedResult = "Instance created successfully" },
                        @{ Action = "Arrange: Prepare invalid input data (null, empty, malformed)"; ExpectedResult = "Invalid test data prepared" },
                        @{ Action = "Act: Call the method with invalid input"; ExpectedResult = "Method handles invalid input gracefully" },
                        @{ Action = "Assert: Verify appropriate exception is thrown"; ExpectedResult = "Expected exception type thrown with correct message" },
                        @{ Action = "Assert: Verify system state unchanged"; ExpectedResult = "No side effects from failed operation" }
                    )
                }
                "*Boundary*" {
                    $steps = @(
                        @{ Action = "Arrange: Prepare minimum boundary value"; ExpectedResult = "Minimum value test data ready" },
                        @{ Action = "Act: Test with minimum value"; ExpectedResult = "Operation succeeds with minimum value" },
                        @{ Action = "Arrange: Prepare maximum boundary value"; ExpectedResult = "Maximum value test data ready" },
                        @{ Action = "Act: Test with maximum value"; ExpectedResult = "Operation succeeds with maximum value" },
                        @{ Action = "Assert: Verify both results are within acceptable range"; ExpectedResult = "Results are valid and within specifications" }
                    )
                }
                default {
                    $steps = @(
                        @{ Action = "Arrange: Set up test environment"; ExpectedResult = "Test environment ready" },
                        @{ Action = "Act: Execute test scenario"; ExpectedResult = "Scenario executes successfully" },
                        @{ Action = "Assert: Verify results"; ExpectedResult = "Results match expectations" }
                    )
                }
            }
        }
        "Integration" {
            switch -Wildcard ($Scenario) {
                "*API endpoint*" {
                    $steps = @(
                        @{ Action = "Setup: Start API service with test configuration"; ExpectedResult = "API service running on test port" },
                        @{ Action = "Prepare: Create valid authentication token"; ExpectedResult = "Valid JWT token generated" },
                        @{ Action = "Execute: Send GET request to endpoint"; ExpectedResult = "Response received with 200 OK status" },
                        @{ Action = "Verify: Check response headers"; ExpectedResult = "Content-Type is application/json, CORS headers present" },
                        @{ Action = "Verify: Validate response body structure"; ExpectedResult = "JSON structure matches API documentation" },
                        @{ Action = "Execute: Send POST request with valid data"; ExpectedResult = "Response received with 201 Created status" },
                        @{ Action = "Verify: Check created resource"; ExpectedResult = "Resource created with correct values" }
                    )
                }
                "*Database*" {
                    $steps = @(
                        @{ Action = "Setup: Initialize test database with clean schema"; ExpectedResult = "Database initialized, all tables created" },
                        @{ Action = "Prepare: Insert test data using repository"; ExpectedResult = "Test data inserted successfully" },
                        @{ Action = "Execute: Perform read operation"; ExpectedResult = "Data retrieved matches inserted data" },
                        @{ Action = "Execute: Perform update operation"; ExpectedResult = "Update completes without errors" },
                        @{ Action = "Verify: Check data consistency"; ExpectedResult = "All foreign keys valid, no orphaned records" },
                        @{ Action = "Cleanup: Rollback test transaction"; ExpectedResult = "Database restored to clean state" }
                    )
                }
                default {
                    $steps = @(
                        @{ Action = "Setup: Initialize integration test environment"; ExpectedResult = "All services connected" },
                        @{ Action = "Execute: Perform integration scenario"; ExpectedResult = "Scenario completes successfully" },
                        @{ Action = "Verify: Check integration points"; ExpectedResult = "All systems communicated correctly" }
                    )
                }
            }
        }
        "E2E" {
            switch -Wildcard ($Scenario) {
                "*Complete user workflow*" {
                    $steps = @(
                        @{ Action = "Open browser and navigate to application URL"; ExpectedResult = "Application loads, login page displayed" },
                        @{ Action = "Enter valid credentials and click Login"; ExpectedResult = "User authenticated, redirected to dashboard" },
                        @{ Action = "Navigate to $StoryTitle feature"; ExpectedResult = "Feature page loads with all UI elements visible" },
                        @{ Action = "Fill in required form fields with valid data"; ExpectedResult = "Form accepts input, validation indicators show green" },
                        @{ Action = "Click Submit/Save button"; ExpectedResult = "Loading indicator appears, then success message" },
                        @{ Action = "Verify data persistence by refreshing page"; ExpectedResult = "Data still present after page refresh" },
                        @{ Action = "Logout and login as different user"; ExpectedResult = "Different user sees appropriate view of data" }
                    )
                }
                "*UI interaction*" {
                    $steps = @(
                        @{ Action = "Check responsive design at 1920x1080 resolution"; ExpectedResult = "All elements properly aligned, no overflow" },
                        @{ Action = "Resize browser to mobile viewport (375x667)"; ExpectedResult = "Layout adjusts, mobile menu appears" },
                        @{ Action = "Test keyboard navigation with Tab key"; ExpectedResult = "Focus moves logically through all interactive elements" },
                        @{ Action = "Test form validation with invalid data"; ExpectedResult = "Inline error messages appear immediately" },
                        @{ Action = "Test loading states during async operations"; ExpectedResult = "Appropriate loading indicators and disabled states" },
                        @{ Action = "Verify accessibility with screen reader"; ExpectedResult = "All elements have proper ARIA labels" }
                    )
                }
                default {
                    $steps = @(
                        @{ Action = "Navigate to test feature"; ExpectedResult = "Feature loads successfully" },
                        @{ Action = "Perform user actions"; ExpectedResult = "Actions complete without errors" },
                        @{ Action = "Verify results in UI"; ExpectedResult = "Expected results displayed" }
                    )
                }
            }
        }
        default {
            $steps = @(
                @{ Action = "Execute test scenario"; ExpectedResult = "Scenario runs successfully" },
                @{ Action = "Verify results"; ExpectedResult = "Results match expectations" }
            )
        }
    }
    
    return $steps
}

# Function to generate test cases by type
function Get-TestCasesByType {
    param(
        [string]$TestType,
        [string]$StoryTitle,
        [int]$MaxCount
    )
    
    $testCases = @()
    
    switch ($TestType) {
        "Unit" {
            $scenarios = @(
                @{ Name = "Positive flow with valid inputs"; Priority = "P1" },
                @{ Name = "Negative flow with invalid inputs"; Priority = "P1" },
                @{ Name = "Boundary value testing"; Priority = "P2" },
                @{ Name = "Null/empty input handling"; Priority = "P2" },
                @{ Name = "Exception handling"; Priority = "P2" }
            )
        }
        "Integration" {
            $scenarios = @(
                @{ Name = "API endpoint validation"; Priority = "P1" },
                @{ Name = "Database integration"; Priority = "P1" },
                @{ Name = "External service integration"; Priority = "P2" },
                @{ Name = "Error propagation"; Priority = "P2" },
                @{ Name = "Transaction handling"; Priority = "P2" }
            )
        }
        "E2E" {
            $scenarios = @(
                @{ Name = "Complete user workflow"; Priority = "P1" },
                @{ Name = "UI interaction flow"; Priority = "P1" },
                @{ Name = "Cross-browser compatibility"; Priority = "P2" },
                @{ Name = "Performance under load"; Priority = "P3" },
                @{ Name = "Error recovery"; Priority = "P2" }
            )
        }
        default {
            $scenarios = @(
                @{ Name = "Basic functionality test"; Priority = "P3" }
            )
        }
    }
    
    $count = [Math]::Min($scenarios.Count, $MaxCount)
    for ($i = 0; $i -lt $count; $i++) {
        $testCases += @{
            Title = "$TestType Test - $($scenarios[$i].Name)"
            Type = $TestType
            Scenario = $scenarios[$i].Name
            Priority = $scenarios[$i].Priority
            Steps = Get-TestStepsByType -TestType $TestType -Scenario $scenarios[$i].Name -StoryTitle $StoryTitle
        }
    }
    
    return $testCases
}

# Main execution
Write-Host "`nTest Case Builder for Azure DevOps" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
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

# Parse test types
$requestedTypes = $TestTypes -split ',' | ForEach-Object { $_.Trim() }

# Generate test cases
$allTestCases = @()
$testNumber = 1

foreach ($testType in $requestedTypes) {
    $casesPerType = [Math]::Floor($MaxTestCases / $requestedTypes.Count)
    $typeCases = Get-TestCasesByType -TestType $testType -StoryTitle $storyTitle -MaxCount $casesPerType
    
    foreach ($testCase in $typeCases) {
        $testCase.Number = $testNumber++
        $allTestCases += $testCase
    }
}

# Show preview
Write-Host "`n=== TEST CASE GENERATION PREVIEW ===" -ForegroundColor Cyan
Write-Host "Total Test Cases to Create: $($allTestCases.Count)" -ForegroundColor White
Write-Host "`nTest Case Breakdown:" -ForegroundColor Cyan

$groupedCases = $allTestCases | Group-Object -Property Type
foreach ($group in $groupedCases) {
    Write-Host "`n$($group.Name) Tests:" -ForegroundColor Yellow
    foreach ($tc in $group.Group) {
        Write-Host "  - $($tc.Title) ($($tc.Steps.Count) steps)" -ForegroundColor White
    }
}

# Show sample test steps
if ($allTestCases.Count -gt 0) {
    $sample = $allTestCases[0]
    Write-Host "`nSample Test Steps for '$($sample.Title)':" -ForegroundColor Cyan
    $stepNum = 1
    foreach ($step in $sample.Steps) {
        Write-Host "  Step ${stepNum}:" -ForegroundColor Yellow
        Write-Host "    Action: $($step.Action)" -ForegroundColor White
        Write-Host "    Expected: $($step.ExpectedResult)" -ForegroundColor Gray
        $stepNum++
    }
}

Write-Host "`n====================================" -ForegroundColor Cyan

# Check approval
$shouldProceed = $AutoApprove -or $Preview
if (-not $shouldProceed) {
    Write-Host "`nDo you want to proceed with creating these test cases?" -ForegroundColor Yellow
    Write-Host "[Y] Yes  [N] No" -ForegroundColor White
    $response = Read-Host "Choice"
    $shouldProceed = $response -match '^[Yy]'
}

if (-not $shouldProceed) {
    Write-Host "`nOperation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Create test cases
$createdTestCases = @()
$failedTestCases = @()

foreach ($testCase in $allTestCases) {
    Write-Host "`nCreating Test Case: $($testCase.Title)..." -ForegroundColor Yellow
    
    if ($Preview) {
        Write-Host "PREVIEW: Would create Test Case '$($testCase.Title)' with $($testCase.Steps.Count) steps" -ForegroundColor Cyan
        $createdTestCases += @{
            Id = "PREVIEW"
            Title = $testCase.Title
            Steps = $testCase.Steps.Count
        }
    }
    else {
        # Create simple description
        $description = "<h3>Test Objective</h3><p>Verify $($testCase.Scenario) for $storyTitle</p>"
        
        $result = New-TestCaseWithSteps `
            -Title "TC-$StoryId-$('{0:D3}' -f $testCase.Number): $($testCase.Title)" `
            -Description $description `
            -ParentId $StoryId `
            -Tags "$productName; Test Case; $($testCase.Type); $($testCase.Priority); Generated" `
            -Priority $(switch($testCase.Priority) { "P1" {1} "P2" {2} "P3" {3} default {4} }) `
            -TestSteps $testCase.Steps
            
        if ($result.Success) {
            Write-Host "✓ Created Test Case #$($result.Id) with $($testCase.Steps.Count) steps" -ForegroundColor Green
            $createdTestCases += @{
                Id = $result.Id
                Title = $testCase.Title
                Url = $result.Url
                Steps = $testCase.Steps.Count
            }
        }
        else {
            Write-Host "✗ Failed to create Test Case" -ForegroundColor Red
            Write-Host "  Error: $($result.Error)" -ForegroundColor Red
            $failedTestCases += $testCase.Title
        }
    }
}

# Summary
if ($Preview) {
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "No changes were made. Remove -Preview flag to create the test cases." -ForegroundColor Cyan
}
else {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Test Case Creation Summary" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Successfully Created: $($createdTestCases.Count) test cases" -ForegroundColor Green
    $totalSteps = ($createdTestCases | Measure-Object -Property Steps -Sum).Sum
    Write-Host "Total Test Steps: $totalSteps" -ForegroundColor White
    
    if ($failedTestCases.Count -gt 0) {
        Write-Host "Failed: $($failedTestCases.Count) test cases" -ForegroundColor Red
    }
    
    if ($createdTestCases.Count -gt 0) {
        Write-Host "`nCreated Test Cases:" -ForegroundColor Cyan
        foreach ($created in $createdTestCases) {
            Write-Host "  - Test Case #$($created.Id): $($created.Title) ($($created.Steps) steps)" -ForegroundColor White
        }
        
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "1. Open test cases in Azure DevOps Test Plans" -ForegroundColor White
        Write-Host "2. Add attachments for test data files if needed" -ForegroundColor White
        Write-Host "3. Link test cases to test suites" -ForegroundColor White
        Write-Host "4. Execute test cases and record results" -ForegroundColor White
    }
}