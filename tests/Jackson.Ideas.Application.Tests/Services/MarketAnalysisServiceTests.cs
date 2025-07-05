using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jackson.Ideas.Application.Tests.Services;

public class MarketAnalysisServiceTests
{
    private readonly Mock<IAIOrchestrator> _mockAIOrchestrator;
    private readonly Mock<IRepository<MarketAnalysis>> _mockRepository;
    private readonly Mock<IRepository<CompetitorAnalysis>> _mockCompetitorRepository;
    private readonly Mock<IRepository<Research>> _mockResearchRepository;
    private readonly Mock<ILogger<MarketAnalysisService>> _mockLogger;
    private readonly MarketAnalysisService _service;

    public MarketAnalysisServiceTests()
    {
        _mockAIOrchestrator = new Mock<IAIOrchestrator>();
        _mockRepository = new Mock<IRepository<MarketAnalysis>>();
        _mockCompetitorRepository = new Mock<IRepository<CompetitorAnalysis>>();
        _mockResearchRepository = new Mock<IRepository<Research>>();
        _mockLogger = new Mock<ILogger<MarketAnalysisService>>();
        
        _service = new MarketAnalysisService(
            _mockAIOrchestrator.Object,
            _mockRepository.Object,
            _mockCompetitorRepository.Object,
            _mockResearchRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithValidInput_ShouldReturnAnalysis()
    {
        // Arrange
        var researchId = Guid.NewGuid();
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app using AI to prioritize tasks";
        
        var expectedMarketAnalysis = new MarketAnalysisDto
        {
            MarketSize = "Global task management software market size was $2.8 billion in 2022",
            GrowthRate = "13.7% CAGR from 2023 to 2030",
            TargetAudience = "Busy professionals, students, and small business owners",
            GeographicScope = "Global, focusing initially on North America and Europe",
            Industry = "Productivity Software",
            CompetitiveLandscape = new string[] { "Todoist", "Asana", "Monday.com", "Notion", "ClickUp" },
            KeyTrends = new string[] 
            { 
                "AI integration in productivity tools",
                "Remote work driving demand",
                "Mobile-first design preferences",
                "Integration with collaboration platforms"
            },
            CustomerSegments = new[]
            {
                new { Name = "Busy Professionals", Size = "45M", Willingness = "High" },
                new { Name = "Students", Size = "12M", Willingness = "Medium" },
                new { Name = "Small Business Owners", Size = "8M", Willingness = "High" }
            },
            RevenueModels = new[]
            {
                new { Type = "Freemium", Description = "Basic features free, premium AI features paid" },
                new { Type = "Subscription", Description = "Monthly/annual subscription for full features" },
                new { Type = "Enterprise", Description = "Custom pricing for large organizations" }
            },
            EntryBarriers = new List<string>
            {
                "High customer acquisition costs in saturated market",
                "Need for significant AI development expertise",
                "User behavior change requirements",
                "Competition from established players with large user bases"
            }
        };

        _mockAIOrchestrator.Setup(x => x.ConductMarketAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMarketAnalysis);

        var savedAnalysis = new MarketAnalysis();
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<MarketAnalysis>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedAnalysis);

        // Act
        var result = await _service.ConductMarketAnalysisAsync(researchId, ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMarketAnalysis.MarketSize, result.MarketSize);
        Assert.Equal(expectedMarketAnalysis.GrowthRate, result.GrowthRate);
        Assert.Equal(expectedMarketAnalysis.TargetAudience, result.TargetAudience);
        Assert.Equal(expectedMarketAnalysis.Industry, result.Industry);
        Assert.NotEmpty(result.CompetitiveLandscape);
        Assert.NotEmpty(result.KeyTrends);
        
        // Verify AI orchestrator was called
        _mockAIOrchestrator.Verify(x => x.ConductMarketAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()), 
            Times.Once);
        
        // Verify repository save was called
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<MarketAnalysis>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var researchId = Guid.NewGuid();
        var emptyTitle = "";
        var ideaDescription = "Valid description";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.ConductMarketAnalysisAsync(researchId, emptyTitle, ideaDescription));
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithNullDescription_ShouldThrowArgumentException()
    {
        // Arrange
        var researchId = Guid.NewGuid();
        var ideaTitle = "Valid Title";
        string? nullDescription = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.ConductMarketAnalysisAsync(researchId, ideaTitle, nullDescription!));
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyResearchId = Guid.Empty;
        var ideaTitle = "Valid Title";
        var ideaDescription = "Valid description";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.ConductMarketAnalysisAsync(emptyResearchId, ideaTitle, ideaDescription));
    }

    [Fact]
    public async Task GetMarketAnalysisAsync_WithValidId_ShouldReturnAnalysis()
    {
        // Arrange
        var analysisId = 123;
        var expectedAnalysis = new MarketAnalysis
        {
            Id = analysisId,
            MarketSize = "Test market size",
            GrowthRate = "10% CAGR",
            TargetAudience = "Test audience",
            GeographicScope = "Global",
            Industry = "Technology",
            CompetitiveLandscapeJson = "[\"Competitor1\", \"Competitor2\"]",
            KeyTrendsJson = "[\"Trend1\", \"Trend2\"]",
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.GetByIdAsync(analysisId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalysis);

        // Act
        var result = await _service.GetMarketAnalysisAsync(analysisId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(analysisId, result.Id);
        Assert.Equal(expectedAnalysis.MarketSize, result.MarketSize);
        Assert.Equal(expectedAnalysis.GrowthRate, result.GrowthRate);
        Assert.Equal(expectedAnalysis.TargetAudience, result.TargetAudience);
        
        // Verify repository was called
        _mockRepository.Verify(x => x.GetByIdAsync(analysisId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetMarketAnalysisAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = 999;

        _mockRepository.Setup(x => x.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketAnalysis?)null);

        // Act
        var result = await _service.GetMarketAnalysisAsync(invalidId);

        // Assert
        Assert.Null(result);
        
        // Verify repository was called
        _mockRepository.Verify(x => x.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetMarketAnalysisAsync_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetMarketAnalysisAsync(emptyId));
    }

    [Fact]
    public async Task GetMarketAnalysesByResearchAsync_WithValidResearchId_ShouldReturnAnalyses()
    {
        // Arrange
        var researchId = Guid.NewGuid();
        var expectedAnalyses = new List<MarketAnalysis>
        {
            new MarketAnalysis
            {
                Id = Guid.NewGuid(),
                ResearchId = researchId,
                MarketSize = "Analysis 1",
                GrowthRate = "10% CAGR",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new MarketAnalysis
            {
                Id = Guid.NewGuid(),
                ResearchId = researchId,
                MarketSize = "Analysis 2",
                GrowthRate = "15% CAGR",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalyses);

        // Act
        var result = await _service.GetMarketAnalysesByResearchAsync(researchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, analysis => Assert.Equal(researchId, analysis.ResearchId));
        
        // Verify results are ordered by creation date (newest first)
        var orderedResults = result.ToList();
        Assert.True(orderedResults[0].CreatedAt >= orderedResults[1].CreatedAt);
    }

    [Fact]
    public async Task GetMarketAnalysesByResearchAsync_WithNoAnalyses_ShouldReturnEmptyList()
    {
        // Arrange
        var researchId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MarketAnalysis>());

        // Act
        var result = await _service.GetMarketAnalysesByResearchAsync(researchId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WhenAIOrchestratorFails_ShouldThrowException()
    {
        // Arrange
        var researchId = Guid.NewGuid();
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";

        _mockAIOrchestrator.Setup(x => x.ConductMarketAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ConductMarketAnalysisAsync(researchId, ideaTitle, ideaDescription));
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WhenRepositoryFails_ShouldThrowException()
    {
        // Arrange
        var researchId = Guid.NewGuid();
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";

        var marketAnalysisDto = new MarketAnalysisDto
        {
            MarketSize = "Test market",
            GrowthRate = "10%",
            CompetitiveLandscape = new string[] { "Competitor" },
            KeyTrends = new string[] { "Trend" }
        };

        _mockAIOrchestrator.Setup(x => x.ConductMarketAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(marketAnalysisDto);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<MarketAnalysis>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database save failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ConductMarketAnalysisAsync(researchId, ideaTitle, ideaDescription));
    }

    [Fact]
    public async Task UpdateMarketAnalysisAsync_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var analysisId = 123;
        var existingAnalysis = new MarketAnalysis
        {
            Id = analysisId,
            MarketSize = "Old market size",
            GrowthRate = "5% CAGR",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new MarketAnalysisDto
        {
            MarketSize = "Updated market size",
            GrowthRate = "12% CAGR",
            TargetAudience = "Updated audience",
            CompetitiveLandscape = new string[] { "New Competitor" },
            KeyTrends = new string[] { "New Trend" }
        };

        _mockRepository.Setup(x => x.GetByIdAsync(analysisId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnalysis);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<MarketAnalysis>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateMarketAnalysisAsync(analysisId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.MarketSize, result.MarketSize);
        Assert.Equal(updateDto.GrowthRate, result.GrowthRate);
        Assert.Equal(updateDto.TargetAudience, result.TargetAudience);
        Assert.NotNull(result.UpdatedAt);
        
        // Verify repository methods were called
        _mockRepository.Verify(x => x.GetByIdAsync(analysisId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MarketAnalysis>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMarketAnalysisAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidId = 999;
        var updateDto = new MarketAnalysisDto
        {
            MarketSize = "Updated market size"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketAnalysis?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateMarketAnalysisAsync(invalidId, updateDto));
    }

    [Fact]
    public async Task DeleteMarketAnalysisAsync_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var analysisId = 123;
        var existingAnalysis = new MarketAnalysis
        {
            Id = analysisId,
            MarketSize = "Test market size"
        };

        _mockRepository.Setup(x => x.GetByIdAsync(analysisId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAnalysis);

        _mockRepository.Setup(x => x.DeleteAsync(analysisId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteMarketAnalysisAsync(analysisId);

        // Assert
        // Verify repository methods were called
        _mockRepository.Verify(x => x.GetByIdAsync(analysisId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(analysisId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMarketAnalysisAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidId = 999;

        _mockRepository.Setup(x => x.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MarketAnalysis?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.DeleteMarketAnalysisAsync(invalidId));
    }
}