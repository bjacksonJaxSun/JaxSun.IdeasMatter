using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Text.Json;

namespace Jackson.Ideas.Application.Tests.Services;

public class SwotAnalysisServiceTests
{
    private readonly Mock<IAIOrchestrator> _mockAIOrchestrator;
    private readonly Mock<ILogger<SwotAnalysisService>> _mockLogger;
    private readonly SwotAnalysisService _service;

    public SwotAnalysisServiceTests()
    {
        _mockAIOrchestrator = new Mock<IAIOrchestrator>();
        _mockLogger = new Mock<ILogger<SwotAnalysisService>>();
        
        _service = new SwotAnalysisService(
            _mockAIOrchestrator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateSwotAnalysisAsync_WithValidInput_ShouldReturnSwotAnalysis()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app using AI to prioritize tasks";
        var marketContext = "Competitive task management market with growing demand for AI-enhanced productivity tools";
        
        var expectedSwotAnalysis = new SwotAnalysisResult
        {
            Strengths = new string[]
            {
                "AI-powered task prioritization provides unique value proposition",
                "Growing market demand for productivity automation",
                "Potential for high user engagement through intelligent features",
                "Scalable software business model"
            },
            Weaknesses = new string[]
            {
                "High development costs for AI capabilities",
                "Requires significant data for effective AI training",
                "Complex user onboarding for AI features",
                "Dependency on AI technology accuracy"
            },
            Opportunities = new string[]
            {
                "Remote work trend increasing demand for productivity tools",
                "Growing acceptance of AI in workplace applications",
                "Integration opportunities with existing business tools",
                "Potential for enterprise market expansion"
            },
            Threats = new string[]
            {
                "Intense competition from established players like Todoist and Asana",
                "Risk of major tech companies entering the market",
                "Economic downturn could reduce spending on productivity tools",
                "AI technology could become commoditized"
            },
            StrategicInsights = new string[]
            {
                "Focus on niche markets where AI provides clear advantage",
                "Build strong data moat through user engagement",
                "Consider partnership strategies with established platforms",
                "Develop defensible AI algorithms and user experience"
            },
            OverallAssessment = "Moderate to high potential with strong AI differentiation, but requires significant investment and faces intense competition.",
            RiskLevel = "Medium-High",
            OpportunityScore = 7.5,
            ThreatLevel = 6.8,
            StrengthRating = 7.2,
            WeaknessImpact = 6.5
        };

        _mockAIOrchestrator.Setup(x => x.ProcessRequestAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonSerializer.Serialize(expectedSwotAnalysis));

        // Act
        var result = await _service.GenerateSwotAnalysisAsync(ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty((string[])result.Strengths);
        Assert.NotEmpty((string[])result.Weaknesses);
        Assert.NotEmpty((string[])result.Opportunities);
        Assert.NotEmpty((string[])result.Threats);
        Assert.NotEmpty(result.StrategicInsights);
        Assert.NotEmpty(result.OverallAssessment);
        Assert.NotEmpty(result.RiskLevel);
        
        // Verify scores are within valid ranges
        Assert.True(result.OpportunityScore >= 0 && result.OpportunityScore <= 10);
        Assert.True(result.ThreatLevel >= 0 && result.ThreatLevel <= 10);
        Assert.True(result.StrengthRating >= 0 && result.StrengthRating <= 10);
        Assert.True(result.WeaknessImpact >= 0 && result.WeaknessImpact <= 10);
        
        // Verify specific content
        Assert.Contains("AI-powered", result.Strengths[0]);
        Assert.Contains("competition", result.Threats[0]);
        Assert.Contains("Remote work", result.Opportunities[0]);
        
        // Verify AI orchestrator was called
        _mockAIOrchestrator.Verify(x => x.GenerateSwotAnalysisAsync(ideaTitle, ideaDescription, marketContext, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GenerateSwotAnalysisAsync_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTitle = "";
        var ideaDescription = "Valid description";
        var marketContext = "Valid market context";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GenerateSwotAnalysisAsync(emptyTitle, ideaDescription, marketContext));
    }

    [Fact]
    public async Task GenerateSwotAnalysisAsync_WithNullDescription_ShouldThrowArgumentException()
    {
        // Arrange
        var ideaTitle = "Valid Title";
        string? nullDescription = null;
        var marketContext = "Valid market context";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GenerateSwotAnalysisAsync(ideaTitle, nullDescription!, marketContext));
    }

    [Fact]
    public async Task GenerateSwotAnalysisAsync_WithEmptyMarketContext_ShouldThrowArgumentException()
    {
        // Arrange
        var ideaTitle = "Valid Title";
        var ideaDescription = "Valid description";
        var emptyMarketContext = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GenerateSwotAnalysisAsync(ideaTitle, ideaDescription, emptyMarketContext));
    }

    [Fact]
    public async Task GenerateSwotAnalysisAsync_WhenAIOrchestratorFails_ShouldThrowException()
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var marketContext = "Test market context";

        _mockAIOrchestrator.Setup(x => x.GenerateSwotAnalysisAsync(ideaTitle, ideaDescription, marketContext, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.GenerateSwotAnalysisAsync(ideaTitle, ideaDescription, marketContext));
    }

    [Fact]
    public async Task GenerateComprehensiveSwotAsync_WithValidInput_ShouldReturnDetailedAnalysis()
    {
        // Arrange
        var ideaTitle = "SaaS Platform for Small Businesses";
        var ideaDescription = "Cloud-based business management platform";
        var marketContext = "Growing SaaS market";
        var competitiveAnalysis = "Several established competitors";
        var customerInsights = "Small businesses need affordable solutions";
        
        var expectedSwotAnalysis = new SwotAnalysisResult
        {
            Strengths = new string[]
            {
                "Focused on underserved small business market",
                "Cloud-based architecture for scalability",
                "Lower cost structure than enterprise solutions",
                "Agile development and faster feature delivery"
            },
            Weaknesses = new string[]
            {
                "Limited brand recognition in established market",
                "Smaller development team compared to competitors",
                "Less capital for marketing and customer acquisition",
                "Need to prove reliability and security to businesses"
            },
            Opportunities = new string[]
            {
                "Small business digitization accelerating post-pandemic",
                "Gaps in current solutions for specific verticals",
                "Integration opportunities with popular small business tools",
                "Potential for international expansion in emerging markets"
            },
            Threats = new string[]
            {
                "Large competitors could lower prices or add features",
                "Economic recession could reduce small business spending",
                "New entrants with better funding and technology",
                "Changing regulations affecting small business operations"
            },
            StrategicInsights = new string[]
            {
                "Focus on specific vertical markets for initial penetration",
                "Build strong customer success and support capabilities",
                "Develop partnerships with small business service providers",
                "Create network effects through integrations and marketplace"
            },
            OverallAssessment = "Strong opportunity in underserved market segment with clear path to differentiation through focus and customer service.",
            RiskLevel = "Medium",
            OpportunityScore = 8.2,
            ThreatLevel = 5.8,
            StrengthRating = 6.9,
            WeaknessImpact = 7.1
        };

        _mockAIOrchestrator.Setup(x => x.GenerateSwotAnalysisAsync(
                $"{ideaTitle}\n\nMarket Context: {marketContext}\nCompetitive Analysis: {competitiveAnalysis}\nCustomer Insights: {customerInsights}", 
                ideaDescription, 
                marketContext, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSwotAnalysis);

        // Act
        var result = await _service.GenerateComprehensiveSwotAsync(
            ideaTitle, ideaDescription, marketContext, competitiveAnalysis, customerInsights);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty((string[])result.Strengths);
        Assert.NotEmpty((string[])result.Weaknesses);
        Assert.NotEmpty((string[])result.Opportunities);
        Assert.NotEmpty((string[])result.Threats);
        Assert.NotEmpty(result.StrategicInsights);
        
        // Verify comprehensive analysis has more detailed insights
        Assert.True(result.Strengths.Length >= 3);
        Assert.True(result.Weaknesses.Length >= 3);
        Assert.True(result.Opportunities.Length >= 3);
        Assert.True(result.Threats.Length >= 3);
        Assert.True(result.StrategicInsights.Length >= 3);
        
        // Verify scores reflect the analysis
        Assert.True(result.OpportunityScore > result.ThreatLevel); // This idea shows more opportunity than threat
        
        // Verify AI orchestrator was called with enhanced prompt
        _mockAIOrchestrator.Verify(x => x.GenerateSwotAnalysisAsync(
            It.Is<string>(s => s.Contains("Market Context") && s.Contains("Competitive Analysis") && s.Contains("Customer Insights")), 
            ideaDescription, 
            marketContext, 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeStrategicOptionsAsync_WithValidInput_ShouldReturnOptionsAnalysis()
    {
        // Arrange
        var baseSwotAnalysis = new SwotAnalysisResult
        {
            Strengths = new string[] { "Strong technical team", "Innovative AI technology" },
            Weaknesses = new string[] { "Limited market presence", "High development costs" },
            Opportunities = new string[] { "Growing AI market", "Enterprise adoption increasing" },
            Threats = new string[] { "Intense competition", "Regulatory uncertainty" }
        };

        var strategicOptions = new List<string>
        {
            "Focus on niche vertical markets",
            "Partner with established platforms",
            "Build comprehensive enterprise solution"
        };

        var expectedAnalysis = new Dictionary<string, SwotAnalysisResult>
        {
            ["Focus on niche vertical markets"] = new SwotAnalysisResult
            {
                Strengths = new string[] { "Can leverage technical expertise in specific domain", "Lower competition in niches" },
                Weaknesses = new string[] { "Limited market size", "Requires domain expertise" },
                Opportunities = new string[] { "Higher margins in specialized markets", "Potential for market leadership" },
                Threats = new string[] { "Risk of market being too small", "Vertical-specific regulations" },
                OverallAssessment = "Moderate risk, moderate reward strategy with clear path to market leadership",
                OpportunityScore = 7.5,
                RiskLevel = "Medium"
            },
            ["Partner with established platforms"] = new SwotAnalysisResult
            {
                Strengths = new string[] { "Leverage partner's market presence", "Reduced customer acquisition costs" },
                Weaknesses = new string[] { "Dependency on partner strategy", "Limited control over customer relationship" },
                Opportunities = new string[] { "Rapid market penetration", "Access to enterprise customers" },
                Threats = new string[] { "Partner could become competitor", "Revenue sharing reduces margins" },
                OverallAssessment = "Lower risk strategy with faster time to market but limited control",
                OpportunityScore = 6.8,
                RiskLevel = "Low-Medium"
            }
        };

        // Mock AI orchestrator to return different analysis for each option
        _mockAIOrchestrator.SetupSequence(x => x.GenerateSwotAnalysisAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalysis["Focus on niche vertical markets"])
            .ReturnsAsync(expectedAnalysis["Partner with established platforms"]);

        // Act
        var result = await _service.AnalyzeStrategicOptionsAsync(baseSwotAnalysis, strategicOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only testing first 2 options
        Assert.Contains("Focus on niche vertical markets", result.Keys);
        Assert.Contains("Partner with established platforms", result.Keys);
        
        // Verify each option has detailed analysis
        foreach (var option in result)
        {
            Assert.NotEmpty(option.Value.Strengths);
            Assert.NotEmpty(option.Value.Weaknesses);
            Assert.NotEmpty(option.Value.Opportunities);
            Assert.NotEmpty(option.Value.Threats);
            Assert.NotEmpty(option.Value.OverallAssessment);
            Assert.True(option.Value.OpportunityScore > 0);
        }
        
        // Verify AI orchestrator was called for each option
        _mockAIOrchestrator.Verify(x => x.GenerateSwotAnalysisAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task AnalyzeStrategicOptionsAsync_WithEmptyOptions_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var baseSwotAnalysis = new SwotAnalysisResult
        {
            Strengths = new string[] { "Test strength" },
            Weaknesses = new string[] { "Test weakness" },
            Opportunities = new string[] { "Test opportunity" },
            Threats = new string[] { "Test threat" }
        };

        var emptyOptions = Array.Empty<string>();

        // Act
        var result = await _service.AnalyzeStrategicOptionsAsync(baseSwotAnalysis, emptyOptions);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify AI orchestrator was not called
        _mockAIOrchestrator.Verify(x => x.GenerateSwotAnalysisAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Theory]
    [InlineData("High", 8.5)]
    [InlineData("Medium-High", 7.2)]
    [InlineData("Medium", 5.5)]
    [InlineData("Low-Medium", 3.8)]
    [InlineData("Low", 2.1)]
    public async Task GenerateSwotAnalysisAsync_ShouldReturnConsistentRiskScoring(string expectedRiskLevel, double expectedMaxOpportunityScore)
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var marketContext = "Test market context";
        
        var swotAnalysis = new SwotAnalysisResult
        {
            Strengths = new string[] { "Test strength" },
            Weaknesses = new string[] { "Test weakness" },
            Opportunities = new string[] { "Test opportunity" },
            Threats = new string[] { "Test threat" },
            RiskLevel = expectedRiskLevel,
            OpportunityScore = expectedMaxOpportunityScore - 0.5 // Just below max for the risk level
        };

        _mockAIOrchestrator.Setup(x => x.GenerateSwotAnalysisAsync(ideaTitle, ideaDescription, marketContext, It.IsAny<CancellationToken>()))
            .ReturnsAsync(swotAnalysis);

        // Act
        var result = await _service.GenerateSwotAnalysisAsync(ideaDescription);

        // Assert
        Assert.Equal(expectedRiskLevel, result.RiskLevel);
        Assert.True(result.OpportunityScore <= expectedMaxOpportunityScore);
    }

    [Fact]
    public async Task CompareSwotAnalysesAsync_WithMultipleAnalyses_ShouldReturnComparison()
    {
        // Arrange
        var analyses = new Dictionary<string, SwotAnalysisResult>
        {
            ["Option A"] = new SwotAnalysisResult
            {
                OpportunityScore = 8.5,
                ThreatLevel = 4.2,
                StrengthRating = 7.8,
                WeaknessImpact = 5.1,
                RiskLevel = "Medium"
            },
            ["Option B"] = new SwotAnalysisResult
            {
                OpportunityScore = 6.8,
                ThreatLevel = 6.5,
                StrengthRating = 6.2,
                WeaknessImpact = 7.3,
                RiskLevel = "Medium-High"
            }
        };

        // Act
        var comparison = await _service.CompareSwotAnalysesAsync(analyses);

        // Assert
        Assert.NotNull(comparison);
        Assert.Equal(2, comparison.Count);
        
        // Verify Option A scores higher due to better opportunity/risk ratio
        var optionA = comparison["Option A"];
        var optionB = comparison["Option B"];
        
        Assert.True(optionA.OpportunityScore > optionB.OpportunityScore);
        Assert.True(optionA.ThreatLevel < optionB.ThreatLevel);
        
        // Verify risk levels are preserved
        Assert.Equal("Medium", optionA.RiskLevel);
        Assert.Equal("Medium-High", optionB.RiskLevel);
    }
}