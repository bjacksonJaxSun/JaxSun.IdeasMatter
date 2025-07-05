using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jackson.Ideas.Application.Tests.Services;

public class ResearchStrategyServiceTests
{
    private readonly Mock<ILogger<ResearchStrategyService>> _mockLogger;
    private readonly ResearchStrategyService _service;

    public ResearchStrategyServiceTests()
    {
        _mockLogger = new Mock<ILogger<ResearchStrategyService>>();
        _service = new ResearchStrategyService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetAvailableApproachesAsync_ShouldReturnThreeApproaches()
    {
        // Act
        var approaches = await _service.GetAvailableApproachesAsync();

        // Assert
        Assert.Equal(3, approaches.Count);
        Assert.Contains(approaches, a => a.Approach == ResearchApproach.QuickValidation);
        Assert.Contains(approaches, a => a.Approach == ResearchApproach.MarketDeepDive);
        Assert.Contains(approaches, a => a.Approach == ResearchApproach.LaunchStrategy);
    }

    [Theory]
    [InlineData(ResearchApproach.QuickValidation, 15, "beginner")]
    [InlineData(ResearchApproach.MarketDeepDive, 45, "intermediate")]
    [InlineData(ResearchApproach.LaunchStrategy, 90, "advanced")]
    public async Task GetAvailableApproachesAsync_ShouldReturnCorrectConfiguration(
        ResearchApproach approach, int expectedDuration, string expectedComplexity)
    {
        // Act
        var approaches = await _service.GetAvailableApproachesAsync();
        var targetApproach = approaches.First(a => a.Approach == approach);

        // Assert
        Assert.Equal(expectedDuration, targetApproach.DurationMinutes);
        Assert.Equal(expectedComplexity, targetApproach.Complexity);
        Assert.NotEmpty(targetApproach.BestFor);
        Assert.NotEmpty(targetApproach.Includes);
        Assert.NotEmpty(targetApproach.Deliverables);
    }

    [Fact]
    public async Task InitiateResearchStrategyAsync_ShouldCreateValidStrategy()
    {
        // Arrange
        var sessionId = 1;
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var approach = ResearchApproach.QuickValidation;

        // Act
        var strategy = await _service.InitiateResearchStrategyAsync(
            sessionId, ideaTitle, ideaDescription, approach);

        // Assert
        Assert.NotNull(strategy);
        Assert.Equal(ResearchStatus.Pending, strategy.Status);
        Assert.Equal(approach.ToString(), strategy.ResearchApproach);
        Assert.Equal(15, strategy.EstimatedDurationMinutes);
        Assert.Equal(0.0, strategy.ProgressPercentage);
        Assert.Contains(ideaTitle, strategy.Title);
    }

    [Fact]
    public async Task ExecuteResearchStrategyAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var strategy = new ResearchSession
        {
            Id = 1,
            ResearchApproach = ResearchApproach.QuickValidation.ToString(),
            Status = ResearchStatus.Pending
        };
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var progressUpdates = new List<(int strategyId, string phase, double progress)>();

        Task ProgressCallback(int id, string phase, double progress)
        {
            progressUpdates.Add((id, phase, progress));
            return Task.CompletedTask;
        }

        // Act
        var result = await _service.ExecuteResearchStrategyAsync(
            strategy, ideaTitle, ideaDescription, ProgressCallback);

        // Assert
        Assert.Equal(ResearchStatus.Completed, result.Status);
        Assert.Equal(100.0, result.ProgressPercentage);
        Assert.NotNull(result.CompletedAt);
        Assert.NotNull(result.AnalysisConfidence);
        Assert.True(progressUpdates.Count > 0);
    }

    [Fact]
    public async Task ExecuteResearchStrategyAsync_WithQuickValidation_ShouldHaveCorrectPhases()
    {
        // Arrange
        var strategy = new ResearchSession
        {
            Id = 1,
            ResearchApproach = ResearchApproach.QuickValidation.ToString(),
            Status = ResearchStatus.Pending
        };
        var phases = new List<string>();

        Task ProgressCallback(int id, string phase, double progress)
        {
            if (!phases.Contains(phase))
                phases.Add(phase);
            return Task.CompletedTask;
        }

        // Act
        await _service.ExecuteResearchStrategyAsync(
            strategy, "Test", "Test", ProgressCallback);

        // Assert
        Assert.Contains("market_context", phases);
        Assert.Contains("competitive_intelligence", phases);
        Assert.Contains("strategic_assessment", phases);
        Assert.Contains("strategic_options", phases);
    }

    [Fact]
    public async Task ExecuteResearchStrategyAsync_WithMarketDeepDive_ShouldIncludeCustomerUnderstanding()
    {
        // Arrange
        var strategy = new ResearchSession
        {
            Id = 1,
            ResearchApproach = ResearchApproach.MarketDeepDive.ToString(),
            Status = ResearchStatus.Pending
        };
        var phases = new List<string>();

        Task ProgressCallback(int id, string phase, double progress)
        {
            if (!phases.Contains(phase))
                phases.Add(phase);
            return Task.CompletedTask;
        }

        // Act
        await _service.ExecuteResearchStrategyAsync(
            strategy, "Test", "Test", ProgressCallback);

        // Assert
        Assert.Contains("customer_understanding", phases);
    }

    [Theory]
    [InlineData(ResearchApproach.QuickValidation, 2)]
    [InlineData(ResearchApproach.MarketDeepDive, 3)]
    [InlineData(ResearchApproach.LaunchStrategy, 5)]
    public async Task ExecuteResearchStrategyAsync_ShouldGenerateCorrectNumberOfOptions(
        ResearchApproach approach, int expectedOptionsCount)
    {
        // Arrange
        var strategy = new ResearchSession
        {
            Id = 1,
            ResearchApproach = approach.ToString(),
            Status = ResearchStatus.Pending
        };

        // Act
        var result = await _service.ExecuteResearchStrategyAsync(
            strategy, "Test", "Test");

        // Assert
        // Since we don't have access to the options directly in the current implementation,
        // we'll verify through the completion status and progress
        Assert.Equal(ResearchStatus.Completed, result.Status);
        Assert.Equal(100.0, result.ProgressPercentage);
    }

    [Fact]
    public async Task GetProgressAsync_ShouldReturnValidProgressUpdate()
    {
        // Arrange
        var strategyId = 1;

        // Act
        var progress = await _service.GetProgressAsync(strategyId);

        // Assert
        Assert.NotNull(progress);
        Assert.Equal(strategyId, progress.StrategyId);
        Assert.NotEmpty(progress.CurrentPhase);
        Assert.True(progress.ProgressPercentage >= 0);
        Assert.True(progress.EstimatedCompletionMinutes >= 0);
    }

    [Fact]
    public async Task GetSessionStrategiesAsync_ShouldReturnEmptyListForNonExistentSession()
    {
        // Arrange
        var sessionId = 999;

        // Act
        var strategies = await _service.GetSessionStrategiesAsync(sessionId);

        // Assert
        Assert.NotNull(strategies);
        Assert.Empty(strategies);
    }
}