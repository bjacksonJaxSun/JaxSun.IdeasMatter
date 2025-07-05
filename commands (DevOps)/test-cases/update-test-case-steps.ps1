#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates existing Test Case work items in Azure DevOps to add proper test steps.
#>

param(
    [Parameter(Mandatory=$true)]
    [string[]]$TestCaseIds,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to generate test steps based on test case title
function Get-TestStepsFromTitle {
    param([string]$Title)
    
    $steps = @()
    
    if ($Title -match "Unit Test.*Positive flow") {
        $steps = @(
            @{ Action = "Arrange: Create test instance with valid parameters"; ExpectedResult = "Instance created successfully without errors" },
            @{ Action = "Arrange: Set up mock dependencies with expected behavior"; ExpectedResult = "All mocks configured and ready" },
            @{ Action = "Act: Call the method with valid input data"; ExpectedResult = "Method executes without throwing exceptions" },
            @{ Action = "Assert: Verify the return value"; ExpectedResult = "Return value matches expected result" },
            @{ Action = "Assert: Verify mock interactions"; ExpectedResult = "All expected mock calls were made with correct parameters" }
        )
    }
    elseif ($Title -match "Unit Test.*Negative flow") {
        $steps = @(
            @{ Action = "Arrange: Create test instance"; ExpectedResult = "Instance created successfully" },
            @{ Action = "Arrange: Prepare invalid input data (null, empty, malformed)"; ExpectedResult = "Invalid test data prepared" },
            @{ Action = "Act: Call the method with invalid input"; ExpectedResult = "Method handles invalid input gracefully" },
            @{ Action = "Assert: Verify appropriate exception is thrown"; ExpectedResult = "Expected exception type thrown with correct message" },
            @{ Action = "Assert: Verify system state unchanged"; ExpectedResult = "No side effects from failed operation" }
        )
    }
    elseif ($Title -match "Integration Test.*API endpoint") {
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
    elseif ($Title -match "Integration Test.*Database") {
        $steps = @(
            @{ Action = "Setup: Initialize test database with clean schema"; ExpectedResult = "Database initialized, all tables created" },
            @{ Action = "Prepare: Insert test data using repository"; ExpectedResult = "Test data inserted successfully" },
            @{ Action = "Execute: Perform read operation"; ExpectedResult = "Data retrieved matches inserted data" },
            @{ Action = "Execute: Perform update operation"; ExpectedResult = "Update completes without errors" },
            @{ Action = "Verify: Check data consistency"; ExpectedResult = "All foreign keys valid, no orphaned records" },
            @{ Action = "Cleanup: Rollback test transaction"; ExpectedResult = "Database restored to clean state" }
        )
    }
    elseif ($Title -match "E2E Test.*Complete user workflow") {
        $steps = @(
            @{ Action = "Open browser and navigate to application URL"; ExpectedResult = "Application loads, login page displayed" },
            @{ Action = "Enter valid credentials and click Login"; ExpectedResult = "User authenticated, redirected to dashboard" },
            @{ Action = "Navigate to feature under test"; ExpectedResult = "Feature page loads with all UI elements visible" },
            @{ Action = "Fill in required form fields with valid data"; ExpectedResult = "Form accepts input, validation indicators show green" },
            @{ Action = "Click Submit/Save button"; ExpectedResult = "Loading indicator appears, then success message" },
            @{ Action = "Verify data persistence by refreshing page"; ExpectedResult = "Data still present after page refresh" },
            @{ Action = "Logout and login as different user"; ExpectedResult = "Different user sees appropriate view of data" }
        )
    }
    elseif ($Title -match "E2E Test.*UI interaction") {
        $steps = @(
            @{ Action = "Check responsive design at 1920x1080 resolution"; ExpectedResult = "All elements properly aligned, no overflow" },
            @{ Action = "Resize browser to mobile viewport (375x667)"; ExpectedResult = "Layout adjusts, mobile menu appears" },
            @{ Action = "Test keyboard navigation with Tab key"; ExpectedResult = "Focus moves logically through all interactive elements" },
            @{ Action = "Test form validation with invalid data"; ExpectedResult = "Inline error messages appear immediately" },
            @{ Action = "Test loading states during async operations"; ExpectedResult = "Appropriate loading indicators and disabled states" },
            @{ Action = "Verify accessibility with screen reader"; ExpectedResult = "All elements have proper ARIA labels" }
        )
    }
    else {
        $steps = @(
            @{ Action = "Setup test environment"; ExpectedResult = "Test environment ready" },
            @{ Action = "Execute test scenario"; ExpectedResult = "Scenario runs successfully" },
            @{ Action = "Verify results"; ExpectedResult = "Results match expectations" }
        )
    }
    
    return $steps
}

# Function to build test steps XML
function Build-TestStepsXml {
    param([array]$TestSteps)
    
    $stepsXml = '<steps id="0" last="' + $TestSteps.Count + '">'
    $stepId = 1
    foreach ($step in $TestSteps) {
        $action = [System.Security.SecurityElement]::Escape($step.Action)
        $expectedResult = [System.Security.SecurityElement]::Escape($step.ExpectedResult)
        
        $stepsXml += '<step id="' + $stepId + '" type="ValidateStep">'
        $stepsXml += '<parameterizedString isformatted="true">&lt;DIV&gt;' + $action + '&lt;/DIV&gt;</parameterizedString>'
        $stepsXml += '<parameterizedString isformatted="true">&lt;DIV&gt;' + $expectedResult + '&lt;/DIV&gt;</parameterizedString>'
        $stepsXml += '<description/>'
        $stepsXml += '</step>'
        $stepId++
    }
    $stepsXml += '</steps>'
    
    return $stepsXml
}

# Function to update test case with steps - Fixed version
function Update-TestCaseSteps {
    param(
        [string]$TestCaseId,
        [string]$StepsXml
    )
    
    # Read configuration
    $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
    $config = Get-Content $configPath -Raw | ConvertFrom-Json
    $org = $config.azureDevOps.organization
    $proj = $config.azureDevOps.project
    $pat = $config.azureDevOps.personalAccessToken
    $apiVer = $config.azureDevOps.apiVersion
    
    # Build operations array with replace operation
    $operations = @(
        @{
            op = "replace"
            path = "/fields/Microsoft.VSTS.TCM.Steps"
            value = $StepsXml
        }
    )
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json-patch+json"
    }
    
    $url = "https://$org.visualstudio.com/$proj/_apis/wit/workitems/$TestCaseId`?api-version=$apiVer"
    
    try {
        $body = ConvertTo-Json @($operations) -Depth 10
        
        # Debug output
        if ($env:DEBUG_API -eq "true") {
            Write-Host "`nDEBUG - Update Request:" -ForegroundColor Magenta
            Write-Host "URL: $url" -ForegroundColor Gray
            Write-Host "Body:" -ForegroundColor Gray
            Write-Host $body -ForegroundColor Gray
        }
        
        $response = Invoke-RestMethod -Uri $url -Method Patch -Headers $headers -Body $body
        
        return @{
            Success = $true
            Id = $response.id
            Url = $response._links.html.href
        }
    }
    catch {
        # Try with "add" if field doesn't exist
        if ($_.ToString() -match "does not exist") {
            $operations[0].op = "add"
            try {
                $body = ConvertTo-Json @($operations) -Depth 10
                $response = Invoke-RestMethod -Uri $url -Method Patch -Headers $headers -Body $body
                
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
        else {
            return @{
                Success = $false
                Error = $_.ToString()
            }
        }
    }
}

# Main execution
Write-Host "`nTest Case Step Updater for Azure DevOps" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Mode: $(if ($Preview) { 'PREVIEW' } else { 'LIVE UPDATE' })" -ForegroundColor $(if ($Preview) { 'Yellow' } else { 'Green' })
Write-Host "Test Cases to Update: $($TestCaseIds -join ', ')" -ForegroundColor White

$updatedCount = 0
$failedCount = 0

foreach ($testCaseId in $TestCaseIds) {
    Write-Host "`nProcessing Test Case #$testCaseId..." -ForegroundColor Yellow
    
    # Get test case details
    $testCase = Get-AzureDevOpsWorkItem -WorkItemId $testCaseId
    
    if (-not $testCase) {
        Write-Host "  X Failed to fetch test case" -ForegroundColor Red
        $failedCount++
        continue
    }
    
    $title = $testCase.fields.'System.Title'
    Write-Host "  Title: $title" -ForegroundColor Gray
    
    # Check if steps already exist
    if ($testCase.fields.'Microsoft.VSTS.TCM.Steps') {
        Write-Host "  ! Test steps already exist, skipping" -ForegroundColor Yellow
        continue
    }
    
    # Generate steps based on title
    $steps = Get-TestStepsFromTitle -Title $title
    Write-Host "  Generated $($steps.Count) test steps" -ForegroundColor Gray
    
    if ($Preview) {
        Write-Host "  PREVIEW: Would add the following steps:" -ForegroundColor Cyan
        $stepNum = 1
        foreach ($step in $steps) {
            Write-Host "    Step ${stepNum}: $($step.Action)" -ForegroundColor White
            Write-Host "    Expected: $($step.ExpectedResult)" -ForegroundColor Gray
            $stepNum++
        }
    }
    else {
        # Build XML and update
        $stepsXml = Build-TestStepsXml -TestSteps $steps
        $result = Update-TestCaseSteps -TestCaseId $testCaseId -StepsXml $stepsXml
        
        if ($result.Success) {
            Write-Host "  V Successfully updated with $($steps.Count) steps" -ForegroundColor Green
            $updatedCount++
        }
        else {
            Write-Host "  X Failed to update: $($result.Error)" -ForegroundColor Red
            $failedCount++
        }
    }
}

# Summary
Write-Host "`n=============================================" -ForegroundColor Cyan
if ($Preview) {
    Write-Host "PREVIEW COMPLETE - No changes made" -ForegroundColor Yellow
    Write-Host "Remove -Preview flag to apply updates" -ForegroundColor White
}
else {
    Write-Host "Update Summary:" -ForegroundColor Green
    Write-Host "  Successfully Updated: $updatedCount" -ForegroundColor Green
    if ($failedCount -gt 0) {
        Write-Host "  Failed: $failedCount" -ForegroundColor Red
    }
}