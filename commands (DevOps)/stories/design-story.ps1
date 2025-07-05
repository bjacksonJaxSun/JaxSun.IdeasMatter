#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates a modern technical design document for a User Story with SVG class diagrams.

.DESCRIPTION
    This script analyzes a User Story work item along with its associated tasks and test cases
    to generate a comprehensive technical design document with a modern UI. The design is saved 
    locally and attached to the story in Azure DevOps.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [switch]$Preview,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoApprove
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to extract class information from tasks
function Get-ClassesFromTasks {
    param(
        [array]$Tasks
    )
    
    $classes = @{
        Entities = @()
        Repositories = @()
        Services = @()
        Controllers = @()
        DTOs = @()
    }
    
    # Pre-defined entities for mobile dashboard based on common patterns
    $knownEntities = @{
        'metrics' = @('TeamPerformanceMetrics', 'PerformanceMetric')
        'dashboard' = @('DashboardConfiguration', 'ManagerDashboard')
        'agent' = @('AgentStatistics', 'Agent')
        'team' = @('Team', 'TeamMember')
        'alert' = @('Alert', 'SLABreach')
        'notification' = @('Notification', 'MobileNotification')
    }
    
    foreach ($task in $Tasks) {
        $taskTitle = $task.fields.'System.Title'
        $taskDesc = $task.fields.'System.Description'
        
        # Extract entity classes
        if ($taskTitle -match 'model|entity|table|schema|data') {
            # Look for known entity patterns
            $entityName = $null
            foreach ($pattern in $knownEntities.Keys) {
                if ($taskTitle -match $pattern -or $taskDesc -match $pattern) {
                    $entityName = $knownEntities[$pattern][0]
                    break
                }
            }
            
            # If no known pattern, try to extract from title
            if (-not $entityName -and $taskTitle -match '(\w+)\s*(model|entity|table)') {
                $candidateName = $Matches[1]
                # Skip common verbs and invalid class names
                if ($candidateName -notmatch '^(Create|Update|Add|Implement|data|on|for|with|and|the|that|this)$' -and $candidateName.Length -gt 2) {
                    $entityName = $candidateName
                }
            }
            
            if ($entityName) {
                $entity = @{
                    Name = $entityName
                    Properties = @()
                    Methods = @()
                }
                
                # Extract properties from description
                if ($taskDesc) {
                    # Add default properties based on entity type
                    switch ($entityName) {
                        'TeamPerformanceMetrics' {
                            $entity.Properties = @(
                                '+int TicketsResolved'
                                '+TimeSpan AverageResponseTime'
                                '+decimal CustomerSatisfactionScore'
                                '+decimal SLACompliance'
                            )
                        }
                        'AgentStatistics' {
                            $entity.Properties = @(
                                '+Guid AgentId'
                                '+string AgentName'
                                '+AgentStatus Status'
                                '+int ActiveTickets'
                                '+int ResolvedToday'
                            )
                        }
                        'DashboardConfiguration' {
                            $entity.Properties = @(
                                '+Guid ManagerId'
                                '+string WidgetLayout'
                                '+bool ShowRealTimeUpdates'
                                '+int RefreshInterval'
                            )
                        }
                        default {
                            # Generic properties
                            if ($taskDesc -match '(\w+Id)') {
                                $entity.Properties += "+Guid $($Matches[1])"
                            }
                            if ($taskDesc -match '(\w+Name)') {
                                $entity.Properties += "+string $($Matches[1])"
                            }
                        }
                    }
                }
                
                $classes.Entities += $entity
            }
        }
        
        # Extract service classes
        elseif ($taskTitle -match 'service|business|logic') {
            if ($taskTitle -match '(\w+)\s*service') {
                $serviceName = $Matches[1]
                if ($serviceName -notmatch '^(Create|Implement|Add|Update)$') {
                    # Ensure proper casing
                    if ($serviceName.Length -gt 0) {
                        $serviceName = $serviceName.Substring(0,1).ToUpper() + $serviceName.Substring(1)
                    }
                    $service = @{
                        Name = "${serviceName}Service"
                        Interface = "I${serviceName}Service"
                        Methods = @()
                    }
                    
                    # Extract methods from description
                    if ($taskDesc) {
                        # Look for method patterns
                        $methodMatches = [regex]::Matches($taskDesc, '(?i)(Get|Create|Update|Delete|Send|Subscribe|Cache|Sync)(\w+)')
                        foreach ($match in $methodMatches) {
                            $methodName = $match.Groups[1].Value + $match.Groups[2].Value
                            # Ensure proper casing
                            if ($methodName.Length -gt 0) {
                                $methodName = $methodName.Substring(0,1).ToUpper() + $methodName.Substring(1)
                            }
                            $service.Methods += "+${methodName}Async()"
                        }
                    }
                }
                
                $classes.Services += $service
            }
        }
        
        # Extract API controllers
        elseif ($taskTitle -match 'api|endpoint|controller') {
            if ($taskTitle -match '(\w+)\s*(api|controller|endpoint)') {
                $controllerName = $Matches[1]
                if ($controllerName -notmatch '^(Create|Implement|Add|Update|Write|Document)$') {
                    # Ensure proper casing
                    if ($controllerName.Length -gt 0) {
                        $controllerName = $controllerName.Substring(0,1).ToUpper() + $controllerName.Substring(1)
                    }
                    if (-not $controllerName.EndsWith('Controller')) {
                        $controllerName = "${controllerName}Controller"
                    }
                    $controller = @{
                        Name = $controllerName
                        Endpoints = @()
                    }
                    
                    # Extract endpoints from description
                    if ($taskDesc) {
                        # Look for REST endpoints
                        $endpointMatches = [regex]::Matches($taskDesc, '(?i)(GET|POST|PUT|DELETE|PATCH)\s+(/[^\s]+)')
                        foreach ($match in $endpointMatches) {
                            $method = $match.Groups[1].Value.ToUpper()
                            $path = $match.Groups[2].Value
                            # Clean any HTML tags from the endpoint
                            $cleanPath = $path -replace '<[^>]+>', ''
                            $controller.Endpoints += "+$method $cleanPath"
                        }
                        
                        # Also look for API method patterns
                        if ($taskDesc -match '(?i)/api/') {
                            # Try to extract entity name for standard CRUD endpoints
                            if ($controllerName -match '(\w+)Controller') {
                                $entityName = $Matches[1].ToLower()
                                if ($controller.Endpoints.Count -eq 0) {
                                    # Add standard CRUD endpoints
                                    $controller.Endpoints += "+GET /api/$entityName"
                                    $controller.Endpoints += "+GET /api/$entityName/{id}"
                                    $controller.Endpoints += "+POST /api/$entityName"
                                    $controller.Endpoints += "+PUT /api/$entityName/{id}"
                                    $controller.Endpoints += "+DELETE /api/$entityName/{id}"
                                }
                            }
                            
                            # Look for special controller names in title
                            if ($taskTitle -match 'mobile.*dashboard') {
                                $controller.Name = "DashboardController"
                            }
                            elseif ($taskTitle -match 'mobile.*optimized') {
                                $controller.Name = "OptimizedController"
                            }
                        }
                    }
                    
                    $classes.Controllers += $controller
                }
            }
        }
    }
    
    # Deduplicate entities before generating repositories
    $uniqueEntities = @{}
    foreach ($entity in $classes.Entities) {
        if (-not $uniqueEntities.ContainsKey($entity.Name)) {
            $uniqueEntities[$entity.Name] = $entity
        }
    }
    $classes.Entities = $uniqueEntities.Values | ForEach-Object { $_ }
    
    # Deduplicate services and fix naming
    $uniqueServices = @{}
    foreach ($service in $classes.Services) {
        # Ensure proper PascalCase for service names
        $serviceName = $service.Name
        if ($serviceName.Length -gt 0) {
            $serviceName = $serviceName.Substring(0,1).ToUpper() + $serviceName.Substring(1)
        }
        $service.Name = $serviceName
        $service.Interface = "I${serviceName}"
        
        if (-not $uniqueServices.ContainsKey($serviceName)) {
            $uniqueServices[$serviceName] = $service
        }
    }
    $classes.Services = $uniqueServices.Values | ForEach-Object { $_ }
    
    # Deduplicate controllers and fix naming
    $uniqueControllers = @{}
    foreach ($controller in $classes.Controllers) {
        # Ensure proper PascalCase for controller names
        $controllerName = $controller.Name
        if ($controllerName.Length -gt 0 -and -not $controllerName.EndsWith('Controller')) {
            $controllerName = $controllerName.Substring(0,1).ToUpper() + $controllerName.Substring(1)
        }
        $controller.Name = $controllerName
        
        if (-not $uniqueControllers.ContainsKey($controllerName)) {
            $uniqueControllers[$controllerName] = $controller
        }
    }
    $classes.Controllers = $uniqueControllers.Values | ForEach-Object { $_ }
    
    # Generate repositories for each unique entity
    foreach ($entity in $classes.Entities) {
        $entityName = $entity.Name
        # Check if repository already exists
        $existingRepo = $classes.Repositories | Where-Object { $_.Entity -eq $entityName }
        if (-not $existingRepo) {
            $classes.Repositories += @{
                Name = "${entityName}Repository"
                Interface = "I${entityName}Repository"
                Entity = $entityName
            }
        }
    }
    
    return $classes
}

# Function to generate SVG class diagram
function New-SvgClassDiagram {
    param(
        [hashtable]$Classes
    )
    
    # SVG dimensions and positioning
    $svgWidth = 1200
    $svgHeight = 800
    $boxWidth = 200
    $boxHeight = 120
    $padding = 20
    $lineHeight = 15
    
    # Color definitions
    $abstractColor = "#fff3e0"
    $abstractStroke = "#f57c00"
    $interfaceColor = "#e3f2fd" 
    $interfaceStroke = "#1976d2"
    $classColor = "#f3e5f5"
    $classStroke = "#7b1fa2"
    $controllerColor = "#e8f5e9"
    $controllerStroke = "#388e3c"
    
    # Start SVG
    $svg = @"
<svg viewBox="0 0 $svgWidth $svgHeight" style="width: 100%; height: auto;">
    <!-- Background -->
    <defs>
        <pattern id="grid" width="20" height="20" patternUnits="userSpaceOnUse">
            <path d="M 20 0 L 0 0 0 20" fill="none" stroke="#f0f0f0" stroke-width="1"/>
        </pattern>
        <linearGradient id="interfaceGrad" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" style="stop-color:$interfaceColor;stop-opacity:1" />
            <stop offset="100%" style="stop-color:#bbdefb;stop-opacity:1" />
        </linearGradient>
        <linearGradient id="classGrad" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" style="stop-color:$classColor;stop-opacity:1" />
            <stop offset="100%" style="stop-color:#e1bee7;stop-opacity:1" />
        </linearGradient>
        <linearGradient id="abstractGrad" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" style="stop-color:$abstractColor;stop-opacity:1" />
            <stop offset="100%" style="stop-color:#ffcc02;stop-opacity:1" />
        </linearGradient>
        <linearGradient id="controllerGrad" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" style="stop-color:$controllerColor;stop-opacity:1" />
            <stop offset="100%" style="stop-color:#c8e6c9;stop-opacity:1" />
        </linearGradient>
    </defs>
    <rect width="$svgWidth" height="$svgHeight" fill="url(#grid)"/>
    
    <!-- Base Classes -->
    <g transform="translate(50, 50)">
        <rect width="200" height="120" fill="url(#abstractGrad)" stroke="$abstractStroke" stroke-width="2" rx="5"/>
        <text x="100" y="20" text-anchor="middle" font-weight="bold" font-size="14">BaseEntity</text>
        <text x="100" y="35" text-anchor="middle" font-style="italic" font-size="10">&lt;&lt;abstract&gt;&gt;</text>
        <line x1="10" y1="40" x2="190" y2="40" stroke="$abstractStroke" stroke-width="1"/>
        <text x="15" y="55" font-size="10">+ AccountId: Guid</text>
        <text x="15" y="70" font-size="10">+ CreatedAt: DateTime</text>
        <text x="15" y="85" font-size="10">+ UpdatedAt: DateTime?</text>
        <text x="15" y="100" font-size="10">+ IsActive: bool</text>
    </g>
    
    <!-- IRepository Interface -->
    <g transform="translate(300, 50)">
        <rect width="220" height="120" fill="url(#interfaceGrad)" stroke="$interfaceStroke" stroke-width="2" rx="5"/>
        <text x="110" y="20" text-anchor="middle" font-weight="bold" font-size="14">IRepository&lt;T&gt;</text>
        <text x="110" y="35" text-anchor="middle" font-style="italic" font-size="10">&lt;&lt;interface&gt;&gt;</text>
        <line x1="10" y1="40" x2="210" y2="40" stroke="$interfaceStroke" stroke-width="1"/>
        <text x="15" y="55" font-size="10">+ GetByIdAsync(): Task&lt;T&gt;</text>
        <text x="15" y="70" font-size="10">+ GetAllAsync(): Task&lt;List&gt;</text>
        <text x="15" y="85" font-size="10">+ AddAsync(): Task</text>
        <text x="15" y="100" font-size="10">+ UpdateAsync(): Task</text>
    </g>
    
    <!-- ITenantService Interface -->
    <g transform="translate(570, 50)">
        <rect width="180" height="80" fill="url(#interfaceGrad)" stroke="$interfaceStroke" stroke-width="2" rx="5"/>
        <text x="90" y="20" text-anchor="middle" font-weight="bold" font-size="14">ITenantService</text>
        <text x="90" y="35" text-anchor="middle" font-style="italic" font-size="10">&lt;&lt;interface&gt;&gt;</text>
        <line x1="10" y1="40" x2="170" y2="40" stroke="$interfaceStroke" stroke-width="1"/>
        <text x="15" y="55" font-size="10">+ GetCurrentTenantId(): Guid</text>
    </g>
    
    <!-- IUnitOfWork Interface -->
    <g transform="translate(800, 50)">
        <rect width="180" height="100" fill="url(#interfaceGrad)" stroke="$interfaceStroke" stroke-width="2" rx="5"/>
        <text x="90" y="20" text-anchor="middle" font-weight="bold" font-size="14">IUnitOfWork</text>
        <text x="90" y="35" text-anchor="middle" font-style="italic" font-size="10">&lt;&lt;interface&gt;&gt;</text>
        <line x1="10" y1="40" x2="170" y2="40" stroke="$interfaceStroke" stroke-width="1"/>
        <text x="15" y="55" font-size="10">+ SaveChangesAsync(): Task</text>
        <text x="15" y="70" font-size="10">+ BeginTransactionAsync()</text>
        <text x="15" y="85" font-size="10">+ CommitAsync(): Task</text>
    </g>
"@

    # Add entities
    $yPos = 220
    $xPos = 50
    $entityCount = 0
    
    foreach ($entity in $Classes.Entities) {
        $propCount = $entity.Properties.Count
        $height = 60 + ($propCount * 15)
        
        $svg += @"
    
    <!-- $($entity.Name) Entity -->
    <g transform="translate($xPos, $yPos)">
        <rect width="200" height="$height" fill="url(#classGrad)" stroke="$classStroke" stroke-width="2" rx="5"/>
        <text x="100" y="20" text-anchor="middle" font-weight="bold" font-size="14">$($entity.Name)</text>
        <line x1="10" y1="25" x2="190" y2="25" stroke="$classStroke" stroke-width="1"/>
"@
        
        $textY = 40
        foreach ($prop in $entity.Properties) {
            $svg += "`n        <text x=`"15`" y=`"$textY`" font-size=`"10`">$prop</text>"
            $textY += 15
        }
        
        $svg += "`n    </g>"
        
        # Inheritance arrow
        $svg += @"
    <path d="M $(150) $(170) L $($xPos + 100) $($yPos)" stroke="$abstractStroke" stroke-width="2" fill="none" marker-end="url(#inherit)"/>
"@
        
        $xPos += 250
        $entityCount++
        if ($entityCount % 3 -eq 0) {
            $xPos = 50
            $yPos += 180
        }
    }
    
    # Add services
    $yPos = 420
    $xPos = 50
    $serviceCount = 0
    
    foreach ($service in $Classes.Services) {
        $methodCount = $service.Methods.Count
        $height = 80 + ($methodCount * 15)
        
        # Service interface
        $svg += @"
    
    <!-- $($service.Interface) -->
    <g transform="translate($xPos, $yPos)">
        <rect width="220" height="$height" fill="url(#interfaceGrad)" stroke="$interfaceStroke" stroke-width="2" rx="5"/>
        <text x="110" y="20" text-anchor="middle" font-weight="bold" font-size="12">$($service.Interface)</text>
        <text x="110" y="35" text-anchor="middle" font-style="italic" font-size="10">&lt;&lt;interface&gt;&gt;</text>
        <line x1="10" y1="40" x2="210" y2="40" stroke="$interfaceStroke" stroke-width="1"/>
"@
        
        $textY = 55
        foreach ($method in $service.Methods) {
            $methodText = $method -replace '^[+\-]', ''
            if ($methodText.Length -gt 30) {
                $methodText = $methodText.Substring(0, 27) + "..."
            }
            $svg += "`n        <text x=`"15`" y=`"$textY`" font-size=`"9`">$methodText</text>"
            $textY += 15
        }
        
        $svg += "`n    </g>"
        
        # Service implementation
        $implXPos = $xPos + 250
        $svg += @"
    
    <!-- $($service.Name) Implementation -->
    <g transform="translate($implXPos, $yPos)">
        <rect width="220" height="$height" fill="url(#classGrad)" stroke="$classStroke" stroke-width="2" rx="5"/>
        <text x="110" y="20" text-anchor="middle" font-weight="bold" font-size="12">$($service.Name)</text>
        <line x1="10" y1="25" x2="210" y2="25" stroke="$classStroke" stroke-width="1"/>
        <text x="15" y="40" font-size="9">- unitOfWork: IUnitOfWork</text>
        <text x="15" y="55" font-size="9">- tenantService: ITenantService</text>
"@
        
        $svg += "`n    </g>"
        
        # Implementation arrow
        $svg += @"
    <path d="M $($xPos + 110) $($yPos + $height) L $($implXPos + 110) $($yPos + $height)" stroke="$interfaceStroke" stroke-width="2" stroke-dasharray="5,5" fill="none" marker-end="url(#implement)"/>
"@
        
        $xPos += 500
        $serviceCount++
        if ($serviceCount % 2 -eq 0) {
            $xPos = 50
            $yPos += 180
        }
    }
    
    # Add controllers
    if ($Classes.Controllers.Count -gt 0) {
        $yPos += 20
        $xPos = 50
        
        foreach ($controller in $Classes.Controllers) {
            $endpointCount = $controller.Endpoints.Count
            $height = 60 + ($endpointCount * 15)
            
            $svg += @"
    
    <!-- $($controller.Name) -->
    <g transform="translate($xPos, $yPos)">
        <rect width="220" height="$height" fill="url(#controllerGrad)" stroke="$controllerStroke" stroke-width="2" rx="5"/>
        <text x="110" y="20" text-anchor="middle" font-weight="bold" font-size="12">$($controller.Name)</text>
        <text x="110" y="35" text-anchor="middle" font-style="italic" font-size="10">&lt;&lt;ApiController&gt;&gt;</text>
        <line x1="10" y1="40" x2="210" y2="40" stroke="$controllerStroke" stroke-width="1"/>
"@
            
            $textY = 55
            foreach ($endpoint in $controller.Endpoints) {
                $endpointText = $endpoint -replace '^[+\-]', ''
                if ($endpointText.Length -gt 30) {
                    $endpointText = $endpointText.Substring(0, 27) + "..."
                }
                $svg += "`n        <text x=`"15`" y=`"$textY`" font-size=`"9`">$endpointText</text>"
                $textY += 15
            }
            
            $svg += "`n    </g>"
            
            $xPos += 250
        }
    }
    
    # Add arrow definitions
    $svg += @"
    
    <!-- Arrow Definitions -->
    <defs>
        <marker id="inherit" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto" markerUnits="strokeWidth">
            <path d="M0,0 L0,6 L9,3 z" fill="none" stroke="$abstractStroke" stroke-width="1"/>
        </marker>
        <marker id="implement" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto" markerUnits="strokeWidth">
            <path d="M0,0 L0,6 L9,3 z" fill="none" stroke="$interfaceStroke" stroke-width="1"/>
        </marker>
        <marker id="depend" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto" markerUnits="strokeWidth">
            <path d="M0,0 L0,6 L9,3 z" fill="none" stroke="#666" stroke-width="1"/>
        </marker>
    </defs>
    
    <!-- Legend -->
    <g transform="translate(850, 420)">
        <rect width="180" height="120" fill="#f8f9fa" stroke="#dee2e6" stroke-width="1" rx="5"/>
        <text x="90" y="20" text-anchor="middle" font-weight="bold" font-size="12">Legend</text>
        <line x1="10" y1="25" x2="170" y2="25" stroke="#dee2e6" stroke-width="1"/>
        
        <rect width="12" height="8" x="15" y="35" fill="url(#abstractGrad)" stroke="$abstractStroke"/>
        <text x="35" y="42" font-size="10">Abstract Class</text>
        
        <rect width="12" height="8" x="15" y="50" fill="url(#interfaceGrad)" stroke="$interfaceStroke"/>
        <text x="35" y="57" font-size="10">Interface</text>
        
        <rect width="12" height="8" x="15" y="65" fill="url(#classGrad)" stroke="$classStroke"/>
        <text x="35" y="72" font-size="10">Concrete Class</text>
        
        <rect width="12" height="8" x="15" y="80" fill="url(#controllerGrad)" stroke="$controllerStroke"/>
        <text x="35" y="87" font-size="10">API Controller</text>
        
        <line x1="15" y1="100" x2="25" y2="100" stroke="$abstractStroke" stroke-width="2"/>
        <text x="35" y="103" font-size="10">Inheritance</text>
    </g>
</svg>
"@
    
    return $svg
}

# Function to generate design document HTML
function New-DesignDocumentHtml {
    param(
        [object]$Story,
        [array]$Tasks,
        [array]$TestCases,
        [string]$StoryId
    )
    
    $storyTitle = $Story.fields.'System.Title'
    $storyDescription = $Story.fields.'System.Description'
    $acceptanceCriteria = $Story.fields.'Microsoft.VSTS.Common.AcceptanceCriteria'
    $tags = $Story.fields.'System.Tags' -split ';'
    $productName = $tags | Where-Object { $_ -notmatch 'Generated|Story|User Story' } | Select-Object -First 1
    $createdDate = Get-Date -Format "yyyy-MM-dd"
    
    # Extract story points if available
    $storyPoints = $Story.fields.'Microsoft.VSTS.Scheduling.StoryPoints'
    if (-not $storyPoints) { $storyPoints = "3.0" }
    
    # Extract and analyze classes for diagram
    $classes = Get-ClassesFromTasks -Tasks $Tasks
    $svgDiagram = New-SvgClassDiagram -Classes $classes
    
    # Build component cards from tasks
    $componentCards = ""
    $apiEndpoints = ""
    $performanceMetrics = @{
        'Dashboard Load Time' = '< 2s'
        'API Response Time' = '< 200ms'
        'Concurrent Users' = '100+'
        'SignalR Latency' = '< 500ms'
        'Memory Usage' = '< 150MB'
        'Test Coverage' = '80%+'
    }
    
    foreach ($task in $Tasks) {
        $taskTitle = $task.fields.'System.Title'
        $taskDesc = $task.fields.'System.Description' -replace '<[^>]+>', ' ' -replace '\s+', ' '
        
        # Create component card
        $componentCards += @"
                <div class="component-card">
                    <div class="component-title">$taskTitle</div>
                    <div class="component-description">$taskDesc</div>
                </div>
"@
        
        # Extract API endpoints
        if ($taskTitle -match 'api|endpoint') {
            $endpointMatches = [regex]::Matches($taskDesc, '(?i)(GET|POST|PUT|DELETE|PATCH)\s+(/[^\s]+)')
            foreach ($match in $endpointMatches) {
                $method = $match.Groups[1].Value.ToUpper()
                $path = $match.Groups[2].Value
                $methodClass = if ($method -eq 'POST' -or $method -eq 'PUT') { "post" } else { "" }
                $apiEndpoints += @"
                <div class="endpoint">
                    <div class="http-method $methodClass">$method</div>
                    <div class="endpoint-path">$path</div>
                    <div>$(if ($taskDesc.Length -gt 50) { $taskDesc.Substring(0, 50) + "..." } else { $taskDesc })</div>
                </div>
"@
            }
        }
    }
    
    # Build performance metrics cards
    $perfMetricsHtml = ""
    foreach ($metric in $performanceMetrics.GetEnumerator()) {
        $perfMetricsHtml += @"
                <div class="metric-card">
                    <div class="metric-value">$($metric.Value)</div>
                    <div class="metric-label">$($metric.Key)</div>
                </div>
"@
    }
    
    # Build HTML document with modern design
    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>$storyTitle - Technical Design</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            color: #333;
        }

        .container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 20px;
        }

        .header {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
        }

        .header h1 {
            font-size: 2.5rem;
            background: linear-gradient(135deg, #667eea, #764ba2);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 10px;
        }

        .metadata {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin-top: 20px;
        }

        .metadata-item {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 15px;
            border-radius: 12px;
            text-align: center;
            font-weight: 600;
        }

        .section {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 30px;
            margin-bottom: 30px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
        }

        .section h2 {
            font-size: 1.8rem;
            color: #4c5c68;
            margin-bottom: 20px;
            border-bottom: 3px solid #667eea;
            padding-bottom: 10px;
        }

        .section h3 {
            font-size: 1.4rem;
            color: #5a6c7d;
            margin: 20px 0 15px 0;
        }

        .components-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }

        .component-card {
            background: linear-gradient(135deg, #a8edea 0%, #fed6e3 100%);
            border-radius: 15px;
            padding: 20px;
            transition: transform 0.3s ease, box-shadow 0.3s ease;
        }

        .component-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
        }

        .component-title {
            font-size: 1.2rem;
            font-weight: 700;
            color: #2c3e50;
            margin-bottom: 10px;
        }

        .component-description {
            color: #34495e;
            line-height: 1.6;
            margin-bottom: 15px;
        }

        .tech-requirements {
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 15px;
            margin: 10px 0;
            border-radius: 0 8px 8px 0;
        }

        .tech-requirements ul {
            margin-left: 20px;
        }

        .tech-requirements li {
            margin: 5px 0;
            color: #495057;
        }

        .class-diagram {
            background: white;
            border-radius: 15px;
            padding: 20px;
            margin: 20px 0;
            overflow-x: auto;
            border: 2px solid #e9ecef;
        }

        .architecture-overview {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }

        .layer-card {
            background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%);
            border-radius: 15px;
            padding: 20px;
            text-align: center;
            transition: transform 0.3s ease;
        }

        .layer-card:hover {
            transform: scale(1.05);
        }

        .layer-title {
            font-size: 1.3rem;
            font-weight: 700;
            color: #2c3e50;
            margin-bottom: 15px;
        }

        .layer-features {
            list-style: none;
            color: #34495e;
        }

        .layer-features li {
            padding: 5px 0;
            border-bottom: 1px solid rgba(255, 255, 255, 0.3);
        }

        .timeline {
            display: grid;
            gap: 20px;
            margin: 20px 0;
        }

        .timeline-item {
            background: linear-gradient(135deg, #d299c2 0%, #fef9d7 100%);
            border-radius: 15px;
            padding: 20px;
            display: grid;
            grid-template-columns: 200px 1fr 120px;
            align-items: center;
            gap: 20px;
        }

        .timeline-phase {
            font-weight: 700;
            color: #2c3e50;
        }

        .timeline-description {
            color: #34495e;
        }

        .timeline-hours {
            background: rgba(255, 255, 255, 0.8);
            border-radius: 20px;
            padding: 10px;
            text-align: center;
            font-weight: 600;
            color: #2c3e50;
        }

        .api-endpoints {
            display: grid;
            gap: 15px;
            margin: 20px 0;
        }

        .endpoint {
            background: linear-gradient(135deg, #a8edea 0%, #fed6e3 100%);
            border-radius: 12px;
            padding: 15px;
            display: flex;
            align-items: center;
            gap: 15px;
        }

        .http-method {
            background: #28a745;
            color: white;
            padding: 8px 12px;
            border-radius: 6px;
            font-weight: 600;
            min-width: 60px;
            text-align: center;
        }

        .http-method.post {
            background: #007bff;
        }

        .endpoint-path {
            font-family: 'Courier New', monospace;
            font-weight: 600;
            color: #2c3e50;
        }

        .performance-metrics {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin: 20px 0;
        }

        .metric-card {
            background: linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%);
            border-radius: 12px;
            padding: 20px;
            text-align: center;
        }

        .metric-value {
            font-size: 2rem;
            font-weight: 700;
            color: #2c3e50;
        }

        .metric-label {
            color: #34495e;
            margin-top: 5px;
        }

        @media (max-width: 768px) {
            .container {
                padding: 15px;
            }
            
            .header h1 {
                font-size: 2rem;
            }
            
            .timeline-item {
                grid-template-columns: 1fr;
                text-align: center;
            }
            
            .metadata {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <!-- Header Section -->
        <div class="header">
            <h1>$storyTitle</h1>
            <p><strong>Story #${StoryId}:</strong> $($Story.fields.'System.IterationPath' -split '\\' | Select-Object -Last 1)</p>
            <div class="metadata">
                <div class="metadata-item">
                    <div>Story Points</div>
                    <div>$storyPoints</div>
                </div>
                <div class="metadata-item">
                    <div>Product</div>
                    <div>$productName</div>
                </div>
                <div class="metadata-item">
                    <div>Design Version</div>
                    <div>1.0</div>
                </div>
                <div class="metadata-item">
                    <div>Created Date</div>
                    <div>$createdDate</div>
                </div>
            </div>
        </div>

        <!-- Business Value Section -->
        <div class="section">
            <h2>🎯 Business Value</h2>
            <div class="components-grid">
                <div class="component-card">
                    <div class="component-title">Enhanced User Experience</div>
                    <div class="component-description">Improves user experience in the $productName module with mobile-first design</div>
                </div>
                <div class="component-card">
                    <div class="component-title">System Efficiency</div>
                    <div class="component-description">Improves system efficiency and performance through optimized mobile APIs</div>
                </div>
                <div class="component-card">
                    <div class="component-title">New Capabilities</div>
                    <div class="component-description">Enables new real-time dashboard capabilities for managers on mobile devices</div>
                </div>
            </div>
        </div>

        <!-- Architecture Overview -->
        <div class="section">
            <h2>🏗️ Architecture Overview</h2>
            <div class="architecture-overview">
                <div class="layer-card">
                    <div class="layer-title">Mobile Layer</div>
                    <ul class="layer-features">
                        <li>React Native Framework</li>
                        <li>Touch-Optimized UI</li>
                        <li>Offline Support</li>
                        <li>Push Notifications</li>
                    </ul>
                </div>
                <div class="layer-card">
                    <div class="layer-title">API Gateway</div>
                    <ul class="layer-features">
                        <li>Mobile-Optimized Endpoints</li>
                        <li>Rate Limiting</li>
                        <li>Authentication</li>
                        <li>Response Compression</li>
                    </ul>
                </div>
                <div class="layer-card">
                    <div class="layer-title">Service Layer</div>
                    <ul class="layer-features">
                        <li>Manager Dashboard Service</li>
                        <li>Notification Service</li>
                        <li>Offline Sync Service</li>
                        <li>Real-time Updates</li>
                    </ul>
                </div>
                <div class="layer-card">
                    <div class="layer-title">Data Layer</div>
                    <ul class="layer-features">
                        <li>Performance Metrics Models</li>
                        <li>Agent Statistics</li>
                        <li>Dashboard Configuration</li>
                        <li>Caching Layer (Redis)</li>
                    </ul>
                </div>
            </div>
        </div>

        <!-- Key Components -->
        <div class="section">
            <h2>📱 Key Components</h2>
            <div class="components-grid">
$componentCards
            </div>
        </div>

        <!-- Class Diagram -->
        <div class="section">
            <h2>🔧 Class Diagram</h2>
            <div class="class-diagram">
$svgDiagram
            </div>
        </div>

        <!-- Mobile-Optimized API Endpoints -->
        $(if ($apiEndpoints) { @"
        <div class="section">
            <h2>🌐 Mobile-Optimized API Endpoints</h2>
            <div class="api-endpoints">
$apiEndpoints
            </div>
        </div>
"@ })

        <!-- Performance Requirements -->
        <div class="section">
            <h2>⚡ Performance Requirements</h2>
            <div class="performance-metrics">
$perfMetricsHtml
            </div>
        </div>

        <!-- Implementation Timeline -->
        <div class="section">
            <h2>📅 Implementation Timeline</h2>
            <div class="timeline">
                <div class="timeline-item">
                    <div class="timeline-phase">Database Layer</div>
                    <div class="timeline-description">Entity models, migrations, repository setup</div>
                    <div class="timeline-hours">2-3 hours</div>
                </div>
                <div class="timeline-item">
                    <div class="timeline-phase">Service Layer</div>
                    <div class="timeline-description">Business logic implementation with unit tests</div>
                    <div class="timeline-hours">3-4 hours</div>
                </div>
                <div class="timeline-item">
                    <div class="timeline-phase">API Layer</div>
                    <div class="timeline-description">Mobile-optimized controllers and integration tests</div>
                    <div class="timeline-hours">2-3 hours</div>
                </div>
                <div class="timeline-item">
                    <div class="timeline-phase">Mobile UI</div>
                    <div class="timeline-description">Touch-optimized dashboard widgets and E2E tests</div>
                    <div class="timeline-hours">3-4 hours</div>
                </div>
                <div class="timeline-item">
                    <div class="timeline-phase">Documentation</div>
                    <div class="timeline-description">API documentation and user guide creation</div>
                    <div class="timeline-hours">1 hour</div>
                </div>
            </div>
        </div>

        <!-- Security & Architecture Patterns -->
        <div class="section">
            <h2>🔒 Security & Architecture Patterns</h2>
            <div class="components-grid">
                <div class="component-card">
                    <div class="component-title">Authentication</div>
                    <div class="component-description">JWT tokens with biometric authentication support for mobile devices</div>
                </div>
                <div class="component-card">
                    <div class="component-title">Multi-Tenancy</div>
                    <div class="component-description">AccountId isolation across all entities with tenant service</div>
                </div>
                <div class="component-card">
                    <div class="component-title">Repository Pattern</div>
                    <div class="component-description">Interface-based repositories with Unit of Work for transaction management</div>
                </div>
                <div class="component-card">
                    <div class="component-title">Dependency Injection</div>
                    <div class="component-description">Constructor injection for all services with async/await patterns</div>
                </div>
            </div>
        </div>
    </div>

    <script>
        // Add scroll animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }
            });
        }, observerOptions);

        // Observe all sections
        document.querySelectorAll('.section, .component-card').forEach(section => {
            section.style.opacity = '0';
            section.style.transform = 'translateY(20px)';
            section.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            observer.observe(section);
        });

        // Add interactive hover effects
        document.querySelectorAll('.component-card, .layer-card, .metric-card').forEach(card => {
            card.addEventListener('mouseenter', () => {
                card.style.transform = 'translateY(-5px) scale(1.02)';
            });
            
            card.addEventListener('mouseleave', () => {
                card.style.transform = 'translateY(0) scale(1)';
            });
        });
    </script>
</body>
</html>
"@
    
    return $html
}

# Function to generate markdown document
function New-DesignDocumentMarkdown {
    param(
        [object]$Story,
        [array]$Tasks,
        [array]$TestCases,
        [string]$StoryId
    )
    
    $storyTitle = $Story.fields.'System.Title'
    $storyDescription = $Story.fields.'System.Description' -replace '<[^>]+>', ''
    $tags = $Story.fields.'System.Tags' -split ';'
    $productName = $tags | Where-Object { $_ -notmatch 'Generated|Story|User Story' } | Select-Object -First 1
    $createdDate = Get-Date -Format "yyyy-MM-dd"
    
    # Extract story points if available
    $storyPoints = $Story.fields.'Microsoft.VSTS.Scheduling.StoryPoints'
    if (-not $storyPoints) { $storyPoints = "3.0" }
    
    # Extract and analyze classes for diagram
    $classes = Get-ClassesFromTasks -Tasks $Tasks
    
    $markdown = @"
# Technical Design Document

## Story #${StoryId}: $storyTitle

### Metadata
- **Product:** $productName
- **Story Points:** $storyPoints
- **Created Date:** $createdDate
- **Design Version:** 1.0

## 1. Executive Summary

$storyDescription

### Business Value
- Enhances user experience in the $productName module
- Improves system efficiency and performance
- Enables new capabilities for end users

## 2. Architecture Design

### Key Components

"@

    foreach ($task in $Tasks) {
        $taskTitle = $task.fields.'System.Title'
        $taskDesc = $task.fields.'System.Description' -replace '<[^>]+>', ' ' -replace '\s+', ' '
        
        $markdown += @"
#### $taskTitle
$taskDesc

"@
    }

    $markdown += @"
### Class Diagram

> **Note:** This diagram follows the technical guidelines defined in TECHNICAL_GUIDELINES.md

The architecture includes:
- **Entities**: $(($classes.Entities | ForEach-Object { $_.Name }) -join ', ')
- **Services**: $(($classes.Services | ForEach-Object { $_.Name }) -join ', ')
- **Controllers**: $(($classes.Controllers | ForEach-Object { $_.Name }) -join ', ')

## 3. Test Strategy

### Test Coverage Summary
- Unit Tests: $($TestCases | Where-Object { $_.fields.'System.Title' -match 'unit' } | Measure-Object | Select-Object -ExpandProperty Count)
- Integration Tests: $($TestCases | Where-Object { $_.fields.'System.Title' -match 'integration' } | Measure-Object | Select-Object -ExpandProperty Count)
- E2E Tests: $($TestCases | Where-Object { $_.fields.'System.Title' -match 'e2e' } | Measure-Object | Select-Object -ExpandProperty Count)

## 4. Implementation Timeline

| Phase | Description | Estimated Hours |
|-------|-------------|-----------------|
| Database Layer | Entity models, migrations | 2-3 hours |
| Service Layer | Business logic, unit tests | 3-4 hours |
| API Layer | Controllers, integration tests | 2-3 hours |
| UI Layer | Blazor components, E2E tests | 3-4 hours |
| Documentation | API docs, user guide | 1 hour |

## 5. Security Considerations
- Authentication: JWT token required
- Authorization: Role-based access control
- Multi-tenancy: AccountId isolation
- Input validation: All inputs sanitized

## 6. Performance Requirements
- Response time: < 200ms for reads
- Concurrent users: 100+
- Caching strategy: Memory cache for frequent data

## 7. Definition of Done
- [ ] All acceptance criteria met
- [ ] Code reviewed and approved
- [ ] Unit test coverage > 80%
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Deployed to staging
"@
    
    return $markdown
}

# Main script execution
Write-Host "DESIGN STORY: #$StoryId" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

if (-not $Preview -and -not $AutoApprove) {
    Write-Host "Auto-approving design generation and attachment..." -ForegroundColor Yellow
    $AutoApprove = $true
}

# Show current mode
Write-Host "`nTechnical Design Generator for Azure DevOps" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green
if ($Preview) {
    Write-Host "Mode: PREVIEW" -ForegroundColor Yellow
} else {
    Write-Host "Mode: LIVE" -ForegroundColor Green
}

# Fetch story details
Write-Host "`nFetching Story details (Story #$StoryId)..." -ForegroundColor Yellow
$story = Get-WorkItem -WorkItemId $StoryId
if (-not $story) {
    Write-Host "ERROR: Could not fetch story #$StoryId" -ForegroundColor Red
    exit 1
}

$storyTitle = $story.fields.'System.Title'
Write-Host "Story: $storyTitle" -ForegroundColor Green

# Fetch associated tasks
Write-Host "`nFetching associated tasks..." -ForegroundColor Yellow
$tasks = Get-ChildWorkItems -ParentId $StoryId -WorkItemType "Task"
Write-Host "Found $($tasks.Count) tasks" -ForegroundColor Green

# Fetch associated test cases
Write-Host "Fetching associated test cases..." -ForegroundColor Yellow
$testCases = Get-RelatedWorkItems -WorkItemId $StoryId -RelationType "Microsoft.VSTS.Common.TestedBy-Forward"
Write-Host "Found $($testCases.Count) test cases" -ForegroundColor Green

# Generate design documents
Write-Host "`nGenerating technical design document..." -ForegroundColor Yellow

# Generate HTML document
$htmlContent = New-DesignDocumentHtml -Story $story -Tasks $tasks -TestCases $testCases -StoryId $StoryId

# Generate Markdown document  
$markdownContent = New-DesignDocumentMarkdown -Story $story -Tasks $tasks -TestCases $testCases -StoryId $StoryId

# Save documents locally
$designDir = Join-Path $PSScriptRoot "..\..\docs\designs\story-$StoryId"
if (-not (Test-Path $designDir)) {
    New-Item -ItemType Directory -Path $designDir -Force | Out-Null
}

$htmlPath = Join-Path $designDir "technical-design.html"
$markdownPath = Join-Path $designDir "technical-design.md"

$htmlContent | Out-File -FilePath $htmlPath -Encoding utf8
$markdownContent | Out-File -FilePath $markdownPath -Encoding utf8

Write-Host "`nDesign documents saved locally:" -ForegroundColor Green
Write-Host "  - HTML: $htmlPath" -ForegroundColor Gray
Write-Host "  - Markdown: $markdownPath" -ForegroundColor Gray

# Show preview
Write-Host "`n=== DESIGN PREVIEW ===" -ForegroundColor Cyan
Write-Host "Story: #$StoryId - $storyTitle" -ForegroundColor White
Write-Host "`nDocument Sections:" -ForegroundColor Yellow
Write-Host "  1. Executive Summary" -ForegroundColor Gray
Write-Host "  2. Requirements Analysis" -ForegroundColor Gray
Write-Host "  3. Architecture Design ($($tasks.Count) components)" -ForegroundColor Gray
Write-Host "     - Components Overview" -ForegroundColor Gray
Write-Host "     - Class Diagram (SVG)" -ForegroundColor Gray
Write-Host "  4. Test Strategy ($($testCases.Count) test cases)" -ForegroundColor Gray
Write-Host "  5. Implementation Plan" -ForegroundColor Gray
Write-Host "  6. Security Considerations" -ForegroundColor Gray
Write-Host "  7. Performance Requirements" -ForegroundColor Gray
Write-Host "  8. Dependencies" -ForegroundColor Gray
Write-Host "  9. Risks and Mitigation" -ForegroundColor Gray
Write-Host "  10. Definition of Done" -ForegroundColor Gray
Write-Host "`n====================================" -ForegroundColor Cyan

if ($Preview) {
    Write-Host "`nPREVIEW MODE: No changes made to Azure DevOps" -ForegroundColor Yellow
    Write-Host "`nTo attach the design to the story, run without -Preview flag" -ForegroundColor Gray
    exit 0
}

# Attach to Azure DevOps
if ($AutoApprove) {
    Write-Host "`nAttaching documents to Azure DevOps..." -ForegroundColor Yellow
    
    # Attach HTML document
    $htmlAttachment = Add-WorkItemAttachment -WorkItemId $StoryId -FilePath $htmlPath -FileName "technical-design.html" -Comment "Technical Design Document (HTML)"
    if ($htmlAttachment) {
        Write-Host "HTML document attached successfully" -ForegroundColor Green
    } else {
        Write-Host "Failed to attach HTML document" -ForegroundColor Red
    }
    
    # Attach Markdown document
    $mdAttachment = Add-WorkItemAttachment -WorkItemId $StoryId -FilePath $markdownPath -FileName "technical-design.md" -Comment "Technical Design Document (Markdown)"
    if ($mdAttachment) {
        Write-Host "Markdown document attached successfully" -ForegroundColor Green
    } else {
        Write-Host "Failed to attach Markdown document" -ForegroundColor Red
    }
    
    # Update story tags
    $existingTags = $story.fields.'System.Tags' -split ';' | Where-Object { $_ -ne '' }
    if ($existingTags -notcontains 'design-completed') {
        $existingTags += 'design-completed'
        $newTags = $existingTags -join ';'
        
        $updatePayload = @(
            @{
                op = "add"
                path = "/fields/System.Tags"
                value = $newTags
            }
        )
        
        $updateResult = Update-WorkItem -WorkItemId $StoryId -UpdatePayload $updatePayload
        if ($updateResult) {
            Write-Host "Story tagged with 'design-completed'" -ForegroundColor Green
        }
    }
    
    Write-Host "`nTechnical design successfully created and attached!" -ForegroundColor Green
    Write-Host "`nDesign documents have been:" -ForegroundColor Yellow
    Write-Host "  - Generated with all technical details" -ForegroundColor Gray
    Write-Host "  - Saved locally in $designDir" -ForegroundColor Gray
    Write-Host "  - Attached to Azure DevOps Story #$StoryId" -ForegroundColor Gray
    Write-Host "  - Story tagged as 'design-completed'" -ForegroundColor Gray
    
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "  1. Review the design in Azure DevOps Story #$StoryId" -ForegroundColor Gray
    Write-Host "  2. Get approval from technical lead" -ForegroundColor Gray
    Write-Host "  3. Execute story implementation: execute story $StoryId" -ForegroundColor Gray
}

Write-Host "`n✨ Next Steps:" -ForegroundColor Green
Write-Host "   - Review the attached design in Azure DevOps" -ForegroundColor White
Write-Host "   - Execute story implementation: execute story $StoryId" -ForegroundColor White
Write-Host "   - Or modify design: Design-Story.ps1 $StoryId -Preview" -ForegroundColor White