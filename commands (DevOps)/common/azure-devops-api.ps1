# Common Azure DevOps API functions
# This file provides properly scoped API functions for all commands

# Function to create a work item
function New-AzureDevOpsWorkItem {
    param(
        [Parameter(Mandatory=$true)]
        [string]$WorkItemType,
        
        [Parameter(Mandatory=$true)]
        [string]$Title,
        
        [Parameter(Mandatory=$true)]
        [string]$Description,
        
        [Parameter(Mandatory=$false)]
        [string]$ParentId,
        
        [Parameter(Mandatory=$false)]
        [string]$Tags,
        
        [Parameter(Mandatory=$false)]
        [int]$Priority = 2,
        
        [Parameter(Mandatory=$false)]
        [string]$AreaPath,
        
        [Parameter(Mandatory=$false)]
        [string]$IterationPath
    )
    
    # Read configuration if not already loaded
    if (-not $script:apiConfig) {
        $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
        if (-not (Test-Path $configPath)) {
            throw "Configuration file not found at $configPath"
        }
        $script:apiConfig = Get-Content $configPath -Raw | ConvertFrom-Json
    }
    
    # Extract config values
    $org = $script:apiConfig.azureDevOps.organization
    $proj = $script:apiConfig.azureDevOps.project
    $pat = $script:apiConfig.azureDevOps.personalAccessToken
    $apiVer = $script:apiConfig.azureDevOps.apiVersion
    
    # Use defaults if not provided
    if (-not $AreaPath) {
        $AreaPath = $script:apiConfig.azureDevOps.workItemSettings.defaultAreaPath
    }
    if (-not $IterationPath) {
        $IterationPath = $script:apiConfig.azureDevOps.workItemSettings.defaultIterationPath
    }
    
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
            value = $AreaPath
        },
        @{
            op = "add"
            path = "/fields/System.IterationPath"
            value = $IterationPath
        }
    )
    
    # Add tags if provided
    if ($Tags) {
        $operations += @{
            op = "add"
            path = "/fields/System.Tags"
            value = $Tags
        }
    }
    
    # Add parent link if provided
    if ($ParentId) {
        $operations += @{
            op = "add"
            path = "/relations/-"
            value = @{
                rel = "System.LinkTypes.Hierarchy-Reverse"
                url = "https://dev.azure.com/$org/$proj/_apis/wit/workitems/$ParentId"
            }
        }
    }
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json-patch+json"
    }
    
    # Build URL - encode spaces in work item type
    $encodedType = $WorkItemType -replace ' ', '%20'
    $url = "https://dev.azure.com/$org/$proj/_apis/wit/workitems/`$$encodedType`?api-version=$apiVer"
    
    try {
        $body = $operations | ConvertTo-Json -Depth 10
        
        # Debug output
        if ($env:DEBUG_API -eq "true") {
            Write-Host "`nDEBUG - API Request:" -ForegroundColor Magenta
            Write-Host "URL: $url" -ForegroundColor Gray
            Write-Host "Body:" -ForegroundColor Gray
            Write-Host $body -ForegroundColor Gray
        }
        
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

# Function to update a work item
function Update-AzureDevOpsWorkItem {
    param(
        [Parameter(Mandatory=$true)]
        [string]$WorkItemId,
        
        [Parameter(Mandatory=$true)]
        [array]$UpdateOperations
    )
    
    # Read configuration if not already loaded
    if (-not $script:apiConfig) {
        $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
        if (-not (Test-Path $configPath)) {
            throw "Configuration file not found at $configPath"
        }
        $script:apiConfig = Get-Content $configPath -Raw | ConvertFrom-Json
    }
    
    # Extract config values
    $org = $script:apiConfig.azureDevOps.organization
    $proj = $script:apiConfig.azureDevOps.project
    $pat = $script:apiConfig.azureDevOps.personalAccessToken
    $apiVer = $script:apiConfig.azureDevOps.apiVersion
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json-patch+json"
    }
    
    $url = "https://dev.azure.com/$org/$proj/_apis/wit/workitems/$WorkItemId`?api-version=$apiVer"
    
    try {
        $body = $UpdateOperations | ConvertTo-Json -Depth 10
        $response = Invoke-RestMethod -Uri $url -Method Patch -Headers $headers -Body $body
        return $response
    }
    catch {
        Write-Host "Failed to update work item $WorkItemId" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# Function to get a work item
function Get-AzureDevOpsWorkItem {
    param(
        [Parameter(Mandatory=$true)]
        [string]$WorkItemId
    )
    
    # Read configuration if not already loaded
    if (-not $script:apiConfig) {
        $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
        if (-not (Test-Path $configPath)) {
            throw "Configuration file not found at $configPath"
        }
        $script:apiConfig = Get-Content $configPath -Raw | ConvertFrom-Json
    }
    
    # Extract config values
    $org = $script:apiConfig.azureDevOps.organization
    $proj = $script:apiConfig.azureDevOps.project
    $pat = $script:apiConfig.azureDevOps.personalAccessToken
    $apiVer = $script:apiConfig.azureDevOps.apiVersion
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json"
    }
    
    $url = "https://dev.azure.com/$org/$proj/_apis/wit/workitems/$WorkItemId`?api-version=$apiVer"
    
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
        return $response
    }
    catch {
        Write-Host "Failed to fetch work item $WorkItemId" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# Function to get related work items
function Get-RelatedWorkItems {
    param(
        [Parameter(Mandatory=$true)]
        [string]$WorkItemId,
        
        [Parameter(Mandatory=$false)]
        [string]$RelationType = "System.LinkTypes.Hierarchy-Forward"
    )
    
    # Read configuration if not already loaded
    if (-not $script:apiConfig) {
        $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
        if (-not (Test-Path $configPath)) {
            throw "Configuration file not found at $configPath"
        }
        $script:apiConfig = Get-Content $configPath -Raw | ConvertFrom-Json
    }
    
    # Extract config values
    $org = $script:apiConfig.azureDevOps.organization
    $proj = $script:apiConfig.azureDevOps.project
    $pat = $script:apiConfig.azureDevOps.personalAccessToken
    $apiVer = $script:apiConfig.azureDevOps.apiVersion
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        Authorization = "Basic $base64AuthInfo"
        "Content-Type" = "application/json"
    }
    
    # Get the work item with relations expanded
    $url = "https://dev.azure.com/$org/$proj/_apis/wit/workitems/$WorkItemId`?`$expand=relations&api-version=$apiVer"
    
    try {
        $response = Invoke-RestMethod -Uri $url -Method Get -Headers $headers
        
        if (-not $response.relations) {
            return @()
        }
        
        # Filter relations by type and get related work item IDs
        $relatedIds = $response.relations | 
            Where-Object { $_.rel -eq $RelationType } | 
            ForEach-Object { 
                if ($_.url -match '/workitems/(\d+)$') { 
                    $matches[1] 
                } 
            }
        
        # Fetch details for each related work item
        $relatedItems = @()
        foreach ($id in $relatedIds) {
            $item = Get-AzureDevOpsWorkItem -WorkItemId $id
            if ($item) {
                $relatedItems += $item
            }
        }
        
        return $relatedItems
    }
    catch {
        Write-Host "Failed to fetch related work items for $WorkItemId" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        return @()
    }
}

# Function to add attachment to work item
function Add-WorkItemAttachment {
    param(
        [Parameter(Mandatory=$true)]
        [string]$WorkItemId,
        
        [Parameter(Mandatory=$true)]
        [string]$FilePath,
        
        [Parameter(Mandatory=$false)]
        [string]$FileName,
        
        [Parameter(Mandatory=$false)]
        [string]$Comment = ""
    )
    
    # Read configuration if not already loaded
    if (-not $script:apiConfig) {
        $configPath = Join-Path -Path $PSScriptRoot -ChildPath "../../azuredevops.config.json"
        if (-not (Test-Path $configPath)) {
            throw "Configuration file not found at $configPath"
        }
        $script:apiConfig = Get-Content $configPath -Raw | ConvertFrom-Json
    }
    
    # Extract config values
    $org = $script:apiConfig.azureDevOps.organization
    $proj = $script:apiConfig.azureDevOps.project
    $pat = $script:apiConfig.azureDevOps.personalAccessToken
    $apiVer = $script:apiConfig.azureDevOps.apiVersion
    
    # Validate file exists
    if (-not (Test-Path $FilePath)) {
        throw "File not found: $FilePath"
    }
    
    # Get file info
    $fileInfo = Get-Item $FilePath
    if (-not $FileName) {
        $FileName = $fileInfo.Name
    }
    
    # Create authorization header
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    
    try {
        # Step 1: Upload the attachment
        $uploadUrl = "https://dev.azure.com/$org/$proj/_apis/wit/attachments?fileName=$FileName&api-version=$apiVer"
        
        $uploadHeaders = @{
            Authorization = "Basic $base64AuthInfo"
            "Content-Type" = "application/octet-stream"
        }
        
        $fileBytes = [System.IO.File]::ReadAllBytes($FilePath)
        $uploadResponse = Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $uploadHeaders -Body $fileBytes
        
        # Step 2: Link the attachment to the work item
        $attachmentUrl = $uploadResponse.url
        
        $linkOperations = @()
        $linkOperations += [ordered]@{
            op = "add"
            path = "/relations/-"
            value = [ordered]@{
                rel = "AttachedFile"
                url = $attachmentUrl
                attributes = @{
                    comment = $Comment
                }
            }
        }
        
        $linkHeaders = @{
            Authorization = "Basic $base64AuthInfo"
            "Content-Type" = "application/json-patch+json"
        }
        
        $linkUrl = "https://dev.azure.com/$org/$proj/_apis/wit/workitems/$WorkItemId`?api-version=$apiVer"
        $linkBody = ConvertTo-Json -InputObject $linkOperations -Depth 10 -Compress
        
        $linkResponse = Invoke-RestMethod -Uri $linkUrl -Method Patch -Headers $linkHeaders -Body $linkBody -ContentType "application/json-patch+json"
        
        Write-Host "Successfully attached $FileName to work item $WorkItemId" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed to attach file to work item $WorkItemId" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        
        # Try to get more error details
        if ($_.Exception.Response) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                Write-Host "Response details: $responseBody" -ForegroundColor Red
            } catch {}
        }
        
        return $false
    }
}