#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Implements a User Story based on its technical design document.

.DESCRIPTION
    This script validates that a technical design exists, then implements the story
    following TDD principles, updates test cases, and ensures all tests pass.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StoryId,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

# Load common API functions
. "$PSScriptRoot/../common/azure-devops-api.ps1"

# Function to check if technical design exists
function Test-TechnicalDesignExists {
    param(
        [string]$StoryId
    )
    
    Write-Host "`nChecking for technical design document..." -ForegroundColor Yellow
    
    # Get story details
    $story = Get-WorkItem -WorkItemId $StoryId
    if (-not $story) {
        throw "Story #$StoryId not found"
    }
    
    # Check for design-completed tag
    $tags = $story.fields.'System.Tags' -split ';'
    if ($tags -notcontains 'design-completed') {
        return $false
    }
    
    # Check for technical-design.md attachment
    $attachments = Get-WorkItemAttachments -WorkItemId $StoryId
    $designDoc = $attachments | Where-Object { $_.attributes.name -eq 'technical-design.md' }
    
    return $null -ne $designDoc
}

# Function to download and parse technical design
function Get-TechnicalDesign {
    param(
        [string]$StoryId
    )
    
    Write-Host "Downloading technical design document..." -ForegroundColor Yellow
    
    $attachments = Get-WorkItemAttachments -WorkItemId $StoryId
    $designDoc = $attachments | Where-Object { $_.attributes.name -eq 'technical-design.md' }
    
    if (-not $designDoc) {
        throw "Technical design document not found"
    }
    
    # Download the design document
    $tempFile = [System.IO.Path]::GetTempFileName()
    $downloadUrl = $designDoc.url
    
    # Create headers for authentication
    $pat = $script:apiConfig.azureDevOps.personalAccessToken
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat"))
    $headers = @{
        "Authorization" = "Basic $base64AuthInfo"
    }
    
    try {
        Invoke-WebRequest -Uri $downloadUrl -Headers $headers -OutFile $tempFile
        $designContent = Get-Content $tempFile -Raw
        return $designContent
    }
    finally {
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
    }
}

# Function to extract implementation details from design
function Get-ImplementationPlan {
    param(
        [string]$DesignContent
    )
    
    $plan = @{
        Entities = @()
        Repositories = @()
        Services = @()
        Controllers = @()
        Components = @()
        Tests = @{
            Unit = @()
            Integration = @()
            E2E = @()
        }
    }
    
    # Parse entities from design
    if ($DesignContent -match '(?ms)Entities[:\s]+(.+?)(?=Services:|Controllers:|$)') {
        $entitiesText = $Matches[1]
        $plan.Entities = [regex]::Matches($entitiesText, '\b(\w+Entity|\w+Model|[A-Z]\w+)\b') | 
            ForEach-Object { $_.Value } | 
            Where-Object { $_ -notmatch '^(The|And|For|With)' } |
            Select-Object -Unique
    }
    
    # Parse services from design
    if ($DesignContent -match '(?ms)Services[:\s]+(.+?)(?=Controllers:|Components:|$)') {
        $servicesText = $Matches[1]
        $plan.Services = [regex]::Matches($servicesText, '\b(\w+Service)\b') | 
            ForEach-Object { $_.Value } |
            Select-Object -Unique
    }
    
    # Parse controllers from design
    if ($DesignContent -match '(?ms)Controllers[:\s]+(.+?)(?=Components:|Test|$)') {
        $controllersText = $Matches[1]
        $plan.Controllers = [regex]::Matches($controllersText, '\b(\w+Controller)\b') | 
            ForEach-Object { $_.Value } |
            Select-Object -Unique
    }
    
    return $plan
}

# Function to create test files following TDD
function New-TestFiles {
    param(
        [hashtable]$ImplementationPlan,
        [string]$StoryId
    )
    
    Write-Host "`n[TDD - RED PHASE] Creating failing tests..." -ForegroundColor Red
    
    $testResults = @{
        Created = @()
        Failed = @()
    }
    
    # Create unit tests for each service
    foreach ($service in $ImplementationPlan.Services) {
        $testFileName = "$($service)Tests.cs"
        $testPath = "src/Integrity.HRP.WebApp.Tests/Services/$testFileName"
        
        Write-Host "  Creating unit test: $testFileName" -ForegroundColor Gray
        
        # Generate test class content
        $testContent = @"
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using Integrity.HRP.WebApp.Services;
using Integrity.HRP.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Integrity.HRP.WebApp.Tests.Services
{
    public class $($service)Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<$service>> _mockLogger;
        private readonly $service _service;

        public $($service)Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<$service>>();
            _service = new $service(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        [Trait("TestCase", "TC-$StoryId-U001")]
        [Trait("Category", "Unit")]
        [Trait("Story", "$StoryId")]
        public async Task GetAllAsync_ShouldReturnItems_WhenDataExists()
        {
            // Arrange
            // TODO: Setup mock data

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        [Trait("TestCase", "TC-$StoryId-U002")]
        [Trait("Category", "Unit")]
        [Trait("Story", "$StoryId")]
        public async Task CreateAsync_ShouldAddItem_WhenValidDataProvided()
        {
            // Arrange
            // TODO: Setup test data

            // Act
            // TODO: Call create method

            // Assert
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("TestCase", "TC-$StoryId-U003")]
        [Trait("Category", "Unit")]
        [Trait("Story", "$StoryId")]
        public async Task UpdateAsync_ShouldModifyItem_WhenItemExists()
        {
            // Arrange
            // TODO: Setup existing item

            // Act
            // TODO: Call update method

            // Assert
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
"@
        
        if (-not $DryRun) {
            # Ensure directory exists
            $testDir = Split-Path $testPath -Parent
            if (-not (Test-Path $testDir)) {
                New-Item -ItemType Directory -Path $testDir -Force | Out-Null
            }
            
            # Write test file
            $testContent | Out-File -FilePath $testPath -Encoding utf8
            $testResults.Created += $testPath
        }
    }
    
    # Create integration tests for repositories
    foreach ($entity in $ImplementationPlan.Entities) {
        $testFileName = "$($entity)RepositoryTests.cs"
        $testPath = "src/Integrity.HRP.WebApp.Tests/Integration/$testFileName"
        
        Write-Host "  Creating integration test: $testFileName" -ForegroundColor Gray
        
        # Generate integration test content
        $integrationTestContent = @"
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Integrity.HRP.Data.Entities;
using Integrity.HRP.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Integrity.HRP.WebApp.Tests.Integration
{
    public class $($entity)RepositoryTests : IntegrationTestBase
    {
        [Fact]
        [Trait("TestCase", "TC-$StoryId-I001")]
        [Trait("Category", "Integration")]
        [Trait("Story", "$StoryId")]
        public async Task Repository_ShouldRespectMultiTenancy()
        {
            // Arrange
            var accountId1 = Guid.NewGuid();
            var accountId2 = Guid.NewGuid();

            // Act
            // TODO: Create entities for different accounts

            // Assert
            // TODO: Verify tenant isolation
        }

        [Fact]
        [Trait("TestCase", "TC-$StoryId-I002")]
        [Trait("Category", "Integration")]
        [Trait("Story", "$StoryId")]
        public async Task Repository_ShouldSupportCRUDOperations()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            // Act & Assert
            // TODO: Test Create, Read, Update, Delete operations
        }
    }
}
"@
        
        if (-not $DryRun) {
            # Ensure directory exists
            $testDir = Split-Path $testPath -Parent
            if (-not (Test-Path $testDir)) {
                New-Item -ItemType Directory -Path $testDir -Force | Out-Null
            }
            
            # Write test file
            $integrationTestContent | Out-File -FilePath $testPath -Encoding utf8
            $testResults.Created += $testPath
        }
    }
    
    return $testResults
}

# Function to implement entities
function New-Entities {
    param(
        [string[]]$Entities,
        [string]$StoryId
    )
    
    Write-Host "`n[TDD - GREEN PHASE] Implementing entities..." -ForegroundColor Green
    
    foreach ($entity in $Entities) {
        $entityPath = "src/Integrity.HRP.Data/Entities/$entity.cs"
        Write-Host "  Creating entity: $entity" -ForegroundColor Gray
        
        $entityContent = @"
using System;
using System.ComponentModel.DataAnnotations;

namespace Integrity.HRP.Data.Entities
{
    /// <summary>
    /// $entity entity for Story #$StoryId
    /// </summary>
    public class $entity : BaseEntity
    {
        // TODO: Add properties based on technical design
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        // Navigation properties
        // TODO: Add relationships as defined in design
    }
}
"@
        
        if (-not $DryRun) {
            # Ensure directory exists
            $entityDir = Split-Path $entityPath -Parent
            if (-not (Test-Path $entityDir)) {
                New-Item -ItemType Directory -Path $entityDir -Force | Out-Null
            }
            
            # Write entity file
            $entityContent | Out-File -FilePath $entityPath -Encoding utf8
        }
    }
}

# Function to implement repositories
function New-Repositories {
    param(
        [string[]]$Entities,
        [string]$StoryId
    )
    
    Write-Host "`nImplementing repositories..." -ForegroundColor Green
    
    foreach ($entity in $Entities) {
        $repoInterface = "I$($entity)Repository"
        $repoClass = "$($entity)Repository"
        
        # Create interface
        $interfacePath = "src/Integrity.HRP.Data/Repositories/Interfaces/$repoInterface.cs"
        Write-Host "  Creating repository interface: $repoInterface" -ForegroundColor Gray
        
        $interfaceContent = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Integrity.HRP.Data.Entities;

namespace Integrity.HRP.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for $entity (Story #$StoryId)
    /// </summary>
    public interface $repoInterface : IRepository<$entity>
    {
        // Add custom methods specific to $entity
        Task<IEnumerable<$entity>> GetActiveAsync(Guid accountId);
        Task<$entity> GetByNameAsync(Guid accountId, string name);
    }
}
"@
        
        # Create implementation
        $repoPath = "src/Integrity.HRP.Data/Repositories/$repoClass.cs"
        Write-Host "  Creating repository implementation: $repoClass" -ForegroundColor Gray
        
        $repoContent = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Integrity.HRP.Data.Context;
using Integrity.HRP.Data.Entities;
using Integrity.HRP.Data.Repositories.Interfaces;

namespace Integrity.HRP.Data.Repositories
{
    /// <summary>
    /// Repository implementation for $entity (Story #$StoryId)
    /// </summary>
    public class $repoClass : Repository<$entity>, $repoInterface
    {
        public $repoClass(HRPDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<$entity>> GetActiveAsync(Guid accountId)
        {
            return await _context.Set<$entity>()
                .Where(e => e.AccountId == accountId && e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<$entity> GetByNameAsync(Guid accountId, string name)
        {
            return await _context.Set<$entity>()
                .FirstOrDefaultAsync(e => e.AccountId == accountId && e.Name == name);
        }
    }
}
"@
        
        if (-not $DryRun) {
            # Ensure directories exist
            $interfaceDir = Split-Path $interfacePath -Parent
            if (-not (Test-Path $interfaceDir)) {
                New-Item -ItemType Directory -Path $interfaceDir -Force | Out-Null
            }
            
            $repoDir = Split-Path $repoPath -Parent
            if (-not (Test-Path $repoDir)) {
                New-Item -ItemType Directory -Path $repoDir -Force | Out-Null
            }
            
            # Write files
            $interfaceContent | Out-File -FilePath $interfacePath -Encoding utf8
            $repoContent | Out-File -FilePath $repoPath -Encoding utf8
        }
    }
}

# Function to implement services
function New-Services {
    param(
        [string[]]$Services,
        [string]$StoryId
    )
    
    Write-Host "`nImplementing services..." -ForegroundColor Green
    
    foreach ($service in $Services) {
        $serviceInterface = "I$service"
        
        # Create interface
        $interfacePath = "src/Integrity.HRP.WebApp/Services/Interfaces/$serviceInterface.cs"
        Write-Host "  Creating service interface: $serviceInterface" -ForegroundColor Gray
        
        $interfaceContent = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrity.HRP.WebApp.Services.Interfaces
{
    /// <summary>
    /// Service interface for $service (Story #$StoryId)
    /// </summary>
    public interface $serviceInterface
    {
        // TODO: Add methods based on technical design
        Task<IEnumerable<object>> GetAllAsync();
        Task<object> GetByIdAsync(Guid id);
        Task<object> CreateAsync(object model);
        Task UpdateAsync(Guid id, object model);
        Task DeleteAsync(Guid id);
    }
}
"@
        
        # Create implementation
        $servicePath = "src/Integrity.HRP.WebApp/Services/$service.cs"
        Write-Host "  Creating service implementation: $service" -ForegroundColor Gray
        
        $serviceContent = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Integrity.HRP.WebApp.Services.Interfaces;
using Integrity.HRP.Data.Repositories;

namespace Integrity.HRP.WebApp.Services
{
    /// <summary>
    /// Service implementation for $service (Story #$StoryId)
    /// </summary>
    public class $service : $serviceInterface
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<$service> _logger;

        public $service(IUnitOfWork unitOfWork, ILogger<$service> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all items");
                // TODO: Implement based on technical design
                return new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all items");
                throw;
            }
        }

        public async Task<object> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting item by id: {Id}", id);
                // TODO: Implement based on technical design
                return new object();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item by id: {Id}", id);
                throw;
            }
        }

        public async Task<object> CreateAsync(object model)
        {
            try
            {
                _logger.LogInformation("Creating new item");
                // TODO: Implement based on technical design
                await _unitOfWork.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, object model)
        {
            try
            {
                _logger.LogInformation("Updating item: {Id}", id);
                // TODO: Implement based on technical design
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item: {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting item: {Id}", id);
                // TODO: Implement based on technical design
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {Id}", id);
                throw;
            }
        }
    }
}
"@
        
        if (-not $DryRun) {
            # Ensure directories exist
            $interfaceDir = Split-Path $interfacePath -Parent
            if (-not (Test-Path $interfaceDir)) {
                New-Item -ItemType Directory -Path $interfaceDir -Force | Out-Null
            }
            
            $serviceDir = Split-Path $servicePath -Parent
            if (-not (Test-Path $serviceDir)) {
                New-Item -ItemType Directory -Path $serviceDir -Force | Out-Null
            }
            
            # Write files
            $interfaceContent | Out-File -FilePath $interfacePath -Encoding utf8
            $serviceContent | Out-File -FilePath $servicePath -Encoding utf8
        }
    }
}

# Function to implement controllers
function New-Controllers {
    param(
        [string[]]$Controllers,
        [string]$StoryId
    )
    
    Write-Host "`nImplementing API controllers..." -ForegroundColor Green
    
    foreach ($controller in $Controllers) {
        $controllerPath = "src/Integrity.HRP.WebApp/Controllers/$controller.cs"
        Write-Host "  Creating controller: $controller" -ForegroundColor Gray
        
        $controllerContent = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Integrity.HRP.WebApp.Services.Interfaces;

namespace Integrity.HRP.WebApp.Controllers
{
    /// <summary>
    /// API Controller for $controller (Story #$StoryId)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class $controller : ControllerBase
    {
        private readonly ILogger<$controller> _logger;
        // TODO: Inject required services based on technical design

        public $controller(ILogger<$controller> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all items
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all items");
                // TODO: Implement based on technical design
                return Ok(new List<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all items");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting item by id: {Id}", id);
                // TODO: Implement based on technical design
                return Ok(new object());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item by id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create new item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> Create([FromBody] object model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new item");
                // TODO: Implement based on technical design
                return CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update existing item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] object model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating item: {Id}", id);
                // TODO: Implement based on technical design
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting item: {Id}", id);
                // TODO: Implement based on technical design
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
"@
        
        if (-not $DryRun) {
            # Ensure directory exists
            $controllerDir = Split-Path $controllerPath -Parent
            if (-not (Test-Path $controllerDir)) {
                New-Item -ItemType Directory -Path $controllerDir -Force | Out-Null
            }
            
            # Write file
            $controllerContent | Out-File -FilePath $controllerPath -Encoding utf8
        }
    }
}

# Function to run build verification
function Test-BuildSuccess {
    Write-Host "`n[BUILD VERIFICATION] Running build..." -ForegroundColor Yellow
    
    $buildScript = Join-Path $PSScriptRoot "../../scripts/build/build-wsl-interop.sh"
    
    if (Test-Path $buildScript) {
        & bash $buildScript
        return $LASTEXITCODE -eq 0
    }
    else {
        Write-Host "Build script not found, trying dotnet build..." -ForegroundColor Yellow
        & dotnet build
        return $LASTEXITCODE -eq 0
    }
}

# Function to run tests
function Test-AllTestsPass {
    Write-Host "`n[TEST EXECUTION] Running all tests..." -ForegroundColor Yellow
    
    $testScript = Join-Path $PSScriptRoot "../../scripts/build/test-and-fix.sh"
    
    if (Test-Path $testScript) {
        & bash $testScript
        return $LASTEXITCODE -eq 0
    }
    else {
        Write-Host "Test script not found, trying dotnet test..." -ForegroundColor Yellow
        & dotnet test
        return $LASTEXITCODE -eq 0
    }
}

# Function to update test cases in Azure DevOps
function Update-TestCases {
    param(
        [string]$StoryId,
        [hashtable]$TestResults
    )
    
    Write-Host "`nUpdating test cases in Azure DevOps..." -ForegroundColor Yellow
    
    # Get all test cases for the story
    $testCases = Get-RelatedWorkItems -WorkItemId $StoryId -RelationType "Microsoft.VSTS.Common.TestedBy-Forward"
    
    foreach ($testCase in $testCases) {
        $testCaseId = $testCase.id
        $testCaseTitle = $testCase.fields.'System.Title'
        
        # Check if this is an automated test
        if ($testCaseTitle -match 'unit|integration|e2e|automated') {
            Write-Host "  Updating test case #$testCaseId: $testCaseTitle" -ForegroundColor Gray
            
            # Update test case to Ready state
            $updatePayload = @(
                @{
                    op = "add"
                    path = "/fields/System.State"
                    value = "Ready"
                },
                @{
                    op = "add"
                    path = "/fields/Microsoft.VSTS.TCM.AutomatedTestName"
                    value = "TC-$StoryId-$(New-Guid)"
                },
                @{
                    op = "add"
                    path = "/fields/Microsoft.VSTS.TCM.AutomatedTestStorage"
                    value = "Integrity.HRP.WebApp.Tests.dll"
                }
            )
            
            if (-not $DryRun) {
                Update-WorkItem -WorkItemId $testCaseId -UpdatePayload $updatePayload | Out-Null
            }
        }
    }
}

# Function to update story status
function Update-StoryStatus {
    param(
        [string]$StoryId,
        [bool]$Success
    )
    
    Write-Host "`nUpdating story status..." -ForegroundColor Yellow
    
    if ($Success) {
        # Add implementation-completed tag
        $story = Get-WorkItem -WorkItemId $StoryId
        $existingTags = $story.fields.'System.Tags' -split ';' | Where-Object { $_ -ne '' }
        
        if ($existingTags -notcontains 'implementation-completed') {
            $existingTags += 'implementation-completed'
            $newTags = $existingTags -join ';'
            
            $updatePayload = @(
                @{
                    op = "add"
                    path = "/fields/System.Tags"
                    value = $newTags
                },
                @{
                    op = "add"
                    path = "/fields/System.State"
                    value = "Resolved"
                }
            )
            
            if (-not $DryRun) {
                Update-WorkItem -WorkItemId $StoryId -UpdatePayload $updatePayload | Out-Null
                Write-Host "  Story marked as implementation-completed and Resolved" -ForegroundColor Green
            }
        }
    }
    else {
        Write-Host "  Story remains in Active state due to failures" -ForegroundColor Yellow
    }
}

# Main script execution
Write-Host "IMPLEMENT STORY: #$StoryId" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "`nDRY RUN MODE - No changes will be made" -ForegroundColor Yellow
}

try {
    # Step 1: Verify technical design exists
    if (-not (Test-TechnicalDesignExists -StoryId $StoryId)) {
        throw @"
Technical design document not found for Story #$StoryId.

Please ensure:
1. The story has been designed using 'Design-Story.ps1 $StoryId'
2. The story has the 'design-completed' tag
3. The technical-design.md file is attached to the story

To create a design:
  Design-Story.ps1 $StoryId

"@
    }
    
    Write-Host "✓ Technical design document found" -ForegroundColor Green
    
    # Step 2: Download and parse technical design
    $designContent = Get-TechnicalDesign -StoryId $StoryId
    $implementationPlan = Get-ImplementationPlan -DesignContent $designContent
    
    Write-Host "`nImplementation Plan:" -ForegroundColor Cyan
    Write-Host "  Entities: $($implementationPlan.Entities.Count)" -ForegroundColor Gray
    Write-Host "  Services: $($implementationPlan.Services.Count)" -ForegroundColor Gray
    Write-Host "  Controllers: $($implementationPlan.Controllers.Count)" -ForegroundColor Gray
    
    # Step 3: TDD - RED Phase (Create failing tests)
    $testResults = New-TestFiles -ImplementationPlan $implementationPlan -StoryId $StoryId
    
    # Step 4: TDD - GREEN Phase (Implement functionality)
    New-Entities -Entities $implementationPlan.Entities -StoryId $StoryId
    New-Repositories -Entities $implementationPlan.Entities -StoryId $StoryId
    New-Services -Services $implementationPlan.Services -StoryId $StoryId
    New-Controllers -Controllers $implementationPlan.Controllers -StoryId $StoryId
    
    # Step 5: Build verification
    if (-not $SkipTests) {
        $buildSuccess = Test-BuildSuccess
        if (-not $buildSuccess) {
            throw "Build failed. Please fix compilation errors before proceeding."
        }
        Write-Host "✓ Build successful" -ForegroundColor Green
        
        # Step 6: Run tests
        $testSuccess = Test-AllTestsPass
        if (-not $testSuccess) {
            Write-Host "⚠ Some tests are failing. This is expected in TDD." -ForegroundColor Yellow
            Write-Host "  Please complete the implementation to make all tests pass." -ForegroundColor Yellow
        }
        else {
            Write-Host "✓ All tests passing" -ForegroundColor Green
        }
    }
    else {
        Write-Host "⚠ Tests skipped as requested" -ForegroundColor Yellow
        $testSuccess = $false
    }
    
    # Step 7: Update Azure DevOps
    Update-TestCases -StoryId $StoryId -TestResults $testResults
    Update-StoryStatus -StoryId $StoryId -Success $testSuccess
    
    # Step 8: Summary
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "IMPLEMENTATION SUMMARY" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Story #$StoryId implementation " -NoNewline
    
    if ($testSuccess) {
        Write-Host "COMPLETED" -ForegroundColor Green
    }
    else {
        Write-Host "IN PROGRESS" -ForegroundColor Yellow
    }
    
    Write-Host "`nCreated:" -ForegroundColor Yellow
    Write-Host "  - Entities: $($implementationPlan.Entities.Count)" -ForegroundColor Gray
    Write-Host "  - Repositories: $($implementationPlan.Entities.Count)" -ForegroundColor Gray
    Write-Host "  - Services: $($implementationPlan.Services.Count)" -ForegroundColor Gray
    Write-Host "  - Controllers: $($implementationPlan.Controllers.Count)" -ForegroundColor Gray
    Write-Host "  - Test Files: $($testResults.Created.Count)" -ForegroundColor Gray
    
    if (-not $testSuccess) {
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "  1. Complete TODO sections in generated files" -ForegroundColor Gray
        Write-Host "  2. Run 'dotnet build' to fix any compilation errors" -ForegroundColor Gray
        Write-Host "  3. Run 'dotnet test' to see failing tests" -ForegroundColor Gray
        Write-Host "  4. Implement functionality to make tests pass" -ForegroundColor Gray
        Write-Host "  5. Run 'Implement-Story.ps1 $StoryId' again to verify" -ForegroundColor Gray
    }
    else {
        Write-Host "`nNext Steps:" -ForegroundColor Yellow
        Write-Host "  1. Review the implementation in your IDE" -ForegroundColor Gray
        Write-Host "  2. Add any additional tests if needed" -ForegroundColor Gray
        Write-Host "  3. Create a pull request: gh pr create" -ForegroundColor Gray
    }
}
catch {
    Write-Host "`nERROR: $_" -ForegroundColor Red
    exit 1
}