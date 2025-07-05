using Jackson.Ideas.Application.Services.AI;
using Jackson.Ideas.Core.DTOs.AI;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace Jackson.Ideas.Application.Tests.Services.AI;

public class AIOrchestratorTests
{
    private readonly Mock<IAIProviderManager> _mockProviderManager;
    private readonly Mock<IRepository<AIProviderConfig>> _mockProviderRepository;
    private readonly Mock<ILogger<AIOrchestrator>> _mockLogger;
    private readonly Mock<IBaseAIProvider> _mockAIProvider;
    private readonly AIOrchestrator _orchestrator;

    public AIOrchestratorTests()
    {
        _mockProviderManager = new Mock<IAIProviderManager>();
        _mockProviderRepository = new Mock<IRepository<AIProviderConfig>>();
        _mockLogger = new Mock<ILogger<AIOrchestrator>>();
        _mockAIProvider = new Mock<IBaseAIProvider>();
        
        _orchestrator = new AIOrchestrator(
            _mockProviderManager.Object,
            _mockProviderRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithValidInput_ShouldReturnAnalysis()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app using AI to prioritize tasks";
        var expectedAnalysis = new MarketAnalysisDto
        {
            MarketSize = "Global task management software market size was $2.8 billion in 2022",
            GrowthRate = "13.7% CAGR from 2023 to 2030",
            CompetitiveLandscape = new[] { "Todoist", "Asana", "Monday.com", "Notion" },
            KeyTrends = new[] { "AI integration", "Remote work growth", "Mobile-first design" }
        };

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedAnalysis));

        // Act
        var result = await _orchestrator.ConductMarketAnalysisAsync(ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("task management", result.MarketSize);
        Assert.NotEmpty(result.CompetitiveLandscape);
        Assert.NotEmpty(result.KeyTrends);
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTitle = "";
        var ideaDescription = "Valid description";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _orchestrator.ConductMarketAnalysisAsync(emptyTitle, ideaDescription));
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithNullDescription_ShouldThrowArgumentException()
    {
        // Arrange
        var ideaTitle = "Valid Title";
        string? nullDescription = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _orchestrator.ConductMarketAnalysisAsync(ideaTitle, nullDescription!));
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WhenProviderFails_ShouldThrowException()
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No providers available"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _orchestrator.ConductMarketAnalysisAsync(ideaTitle, ideaDescription));
    }

    [Fact]
    public async Task GenerateSwotAnalysisAsync_WithValidInput_ShouldReturnSwotAnalysis()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app";
        var marketContext = "Competitive task management market";
        
        var expectedSwot = new SwotAnalysisResult
        {
            Strengths = new[] { "AI-powered prioritization", "Smart automation" },
            Weaknesses = new[] { "High development cost", "Market saturation" },
            Opportunities = new[] { "Remote work trend", "AI adoption" },
            Threats = new[] { "Established competitors", "Economic downturn" }
        };

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedSwot));

        // Act
        var result = await _orchestrator.GenerateSwotAnalysisAsync(ideaTitle, ideaDescription, marketContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Strengths);
        Assert.NotEmpty(result.Weaknesses);
        Assert.NotEmpty(result.Opportunities);
        Assert.NotEmpty(result.Threats);
    }

    [Fact]
    public async Task GenerateCompetitiveAnalysisAsync_WithValidInput_ShouldReturnCompetitiveAnalysis()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app";
        
        var expectedAnalysis = new CompetitiveAnalysisResult
        {
            DirectCompetitors = new[] { "Todoist", "Any.do" },
            IndirectCompetitors = new[] { "Google Calendar", "Apple Reminders" },
            SubstituteProducts = new[] { "Paper planners", "Spreadsheets" },
            CompetitiveAdvantages = new[] { "AI prioritization", "Smart scheduling" },
            MarketGaps = new[] { "Lack of AI integration", "Poor mobile UX" }
        };

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedAnalysis));

        // Act
        var result = await _orchestrator.GenerateCompetitiveAnalysisAsync(ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.DirectCompetitors);
        Assert.NotEmpty(result.CompetitiveAdvantages);
        Assert.NotEmpty(result.MarketGaps);
    }

    [Fact]
    public async Task GenerateCustomerSegmentationAsync_WithValidInput_ShouldReturnSegmentation()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app";
        var marketAnalysis = "Large and growing market";
        
        var expectedSegmentation = new CustomerSegmentationResult
        {
            PrimarySegments = new[]
            {
                new CustomerSegment
                {
                    Name = "Busy Professionals",
                    Size = "45M users",
                    Characteristics = new[] { "High income", "Tech-savvy", "Time-constrained" },
                    PainPoints = new[] { "Information overload", "Poor time management" },
                    ValueProposition = "AI-powered task prioritization saves 2+ hours daily"
                }
            },
            SecondarySegments = new[]
            {
                new CustomerSegment
                {
                    Name = "Students",
                    Size = "12M users",
                    Characteristics = new[] { "Budget-conscious", "Mobile-first", "Collaborative" },
                    PainPoints = new[] { "Procrastination", "Deadline management" },
                    ValueProposition = "Smart deadlines and study scheduling"
                }
            }
        };

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedSegmentation));

        // Act
        var result = await _orchestrator.GenerateCustomerSegmentationAsync(ideaTitle, ideaDescription, marketAnalysis);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.PrimarySegments);
        Assert.NotEmpty(result.SecondarySegments);
        Assert.All(result.PrimarySegments, segment => 
        {
            Assert.NotEmpty(segment.Name);
            Assert.NotEmpty(segment.Characteristics);
            Assert.NotEmpty(segment.PainPoints);
        });
    }

    [Fact]
    public async Task BrainstormStrategicOptionsAsync_WithValidInput_ShouldReturnOptions()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app";
        var marketInsights = "Large market with growth opportunities";
        var swotAnalysis = "Strong AI capabilities, competitive market";
        var customerSegments = "Professionals and students";
        var optionsCount = 3;
        
        var expectedResponse = new BrainstormResponseDto
        {
            GeneratedOptions = new[]
            {
                new OptionDto
                {
                    Title = "Niche AI Focus",
                    Approach = "niche_domination",
                    Description = "Focus on AI-powered task prioritization",
                    TargetSegment = "Busy professionals",
                    ValueProposition = "Save 2+ hours daily with smart prioritization",
                    OverallScore = 8.5,
                    TimelineMonths = 12,
                    EstimatedInvestment = 500000,
                    SuccessProbability = 75
                },
                new OptionDto
                {
                    Title = "Mass Market Appeal",
                    Approach = "market_leader_challenge",
                    Description = "Compete directly with established players",
                    TargetSegment = "General consumers",
                    ValueProposition = "Better UX than existing solutions",
                    OverallScore = 6.5,
                    TimelineMonths = 18,
                    EstimatedInvestment = 2000000,
                    SuccessProbability = 45
                }
            }
        };

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedResponse));

        // Act
        var result = await _orchestrator.BrainstormStrategicOptionsAsync(
            ideaTitle, ideaDescription, marketInsights, swotAnalysis, customerSegments, optionsCount);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.GeneratedOptions);
        Assert.Equal(2, result.GeneratedOptions.Count());
        Assert.All(result.GeneratedOptions, option =>
        {
            Assert.NotEmpty(option.Title);
            Assert.NotEmpty(option.Approach);
            Assert.True(option.OverallScore > 0);
            Assert.True(option.TimelineMonths > 0);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task BrainstormStrategicOptionsAsync_WithInvalidOptionsCount_ShouldThrowArgumentException(int invalidCount)
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var marketInsights = "Test insights";
        var swotAnalysis = "Test SWOT";
        var customerSegments = "Test segments";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _orchestrator.BrainstormStrategicOptionsAsync(
                ideaTitle, ideaDescription, marketInsights, swotAnalysis, customerSegments, invalidCount));
    }

    [Fact]
    public async Task ValidateIdeaFeasibilityAsync_WithValidInput_ShouldReturnFactCheckResponse()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app";
        var marketAnalysis = "Large and growing market";
        
        var expectedResponse = new FactCheckResponseDto
        {
            OverallFeasibilityScore = 7.8,
            TechnicalFeasibility = 8.5,
            MarketFeasibility = 7.2,
            FinancialFeasibility = 7.8,
            KeyAssumptions = new[]
            {
                "AI technology is mature enough for task prioritization",
                "Users are willing to pay for AI-enhanced productivity tools",
                "Market has room for new entrants"
            },
            CriticalRisks = new[]
            {
                "High customer acquisition costs",
                "Intense competition from established players",
                "AI accuracy requirements"
            },
            RecommendedNextSteps = new[]
            {
                "Develop MVP with core AI features",
                "Conduct user testing with target segments",
                "Analyze competitor pricing strategies"
            }
        };

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedResponse));

        // Act
        var result = await _orchestrator.ValidateIdeaFeasibilityAsync(ideaTitle, ideaDescription, marketAnalysis);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.OverallFeasibilityScore > 0 && result.OverallFeasibilityScore <= 10);
        Assert.True(result.TechnicalFeasibility > 0 && result.TechnicalFeasibility <= 10);
        Assert.True(result.MarketFeasibility > 0 && result.MarketFeasibility <= 10);
        Assert.True(result.FinancialFeasibility > 0 && result.FinancialFeasibility <= 10);
        Assert.NotEmpty(result.KeyAssumptions);
        Assert.NotEmpty(result.CriticalRisks);
        Assert.NotEmpty(result.RecommendedNextSteps);
    }

    [Fact]
    public async Task ValidateIdeaFeasibilityAsync_WhenAIResponseIsInvalid_ShouldThrowException()
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var marketAnalysis = "Test analysis";

        _mockProviderManager.Setup(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockAIProvider.Object);

        _mockAIProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Invalid JSON response");

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => 
            _orchestrator.ValidateIdeaFeasibilityAsync(ideaTitle, ideaDescription, marketAnalysis));
    }

    [Fact]
    public async Task ConductMarketAnalysisAsync_WithProviderFallback_ShouldUseSecondaryProvider()
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var expectedAnalysis = new MarketAnalysisDto
        {
            MarketSize = "Test market size",
            GrowthRate = "10% CAGR",
            CompetitiveLandscape = new[] { "Competitor 1" },
            KeyTrends = new[] { "Trend 1" }
        };

        var mockSecondaryProvider = new Mock<IBaseAIProvider>();

        // First call fails, second succeeds
        _mockProviderManager.SetupSequence(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Primary provider failed"))
            .ReturnsAsync(mockSecondaryProvider.Object);

        mockSecondaryProvider.Setup(x => x.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedAnalysis));

        // Act
        var result = await _orchestrator.ConductMarketAnalysisAsync(ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test market size", result.MarketSize);
        _mockProviderManager.Verify(x => x.GetProviderAsync(It.IsAny<AIProviderType>(), It.IsAny<CancellationToken>()), 
            Times.AtLeast(2));
    }
}