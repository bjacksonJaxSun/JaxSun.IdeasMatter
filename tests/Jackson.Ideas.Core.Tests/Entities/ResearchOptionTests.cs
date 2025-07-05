using Jackson.Ideas.Core.Entities;
using Xunit;

namespace Jackson.Ideas.Core.Tests.Entities;

public class ResearchOptionTests
{
    [Fact]
    public void ResearchOption_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var option = new ResearchOption();

        // Assert
        Assert.Equal(Guid.Empty, option.Id); // New entities have Id = Empty until saved
        Assert.Empty(option.Title);
        Assert.Empty(option.Description);
        Assert.Empty(option.Approach);
        Assert.Equal(0.0, option.OverallScore);
        Assert.Equal(0, option.TimelineToMarketMonths);
        Assert.Equal(0, option.SuccessProbabilityPercent);
        Assert.False(option.IsRecommended);
        Assert.True(option.CreatedAt <= DateTime.UtcNow);
        Assert.Null(option.UpdatedAt); // UpdatedAt is null for new entities
    }

    [Fact]
    public void ResearchOption_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var title = "Niche Market Leader";
        var approach = "niche_domination";
        var score = 8.5;

        // Act
        var option = new ResearchOption
        {
            ResearchSessionId = sessionId,
            Title = title,
            Approach = approach,
            OverallScore = score,
            IsRecommended = true
        };

        // Assert
        Assert.Equal(sessionId, option.ResearchSessionId);
        Assert.Equal(title, option.Title);
        Assert.Equal(approach, option.Approach);
        Assert.Equal(score, option.OverallScore);
        Assert.True(option.IsRecommended);
    }

    [Theory]
    [InlineData("niche_domination")]
    [InlineData("market_leader_challenge")]
    [InlineData("innovation_leadership")]
    [InlineData("cost_leadership")]
    [InlineData("differentiation")]
    public void ResearchOption_ValidApproaches_ShouldAccept(string approach)
    {
        // Arrange
        var option = new ResearchOption();

        // Act
        option.Approach = approach;

        // Assert
        Assert.Equal(approach, option.Approach);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(5.5)]
    [InlineData(10.0)]
    public void ResearchOption_ValidOverallScore_ShouldAccept(double score)
    {
        // Arrange
        var option = new ResearchOption();

        // Act
        option.OverallScore = score;

        // Assert
        Assert.Equal(score, option.OverallScore);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(36)]
    public void ResearchOption_ValidTimelines_ShouldAccept(int months)
    {
        // Arrange
        var option = new ResearchOption();

        // Act
        option.TimelineToMarketMonths = months;

        // Assert
        Assert.Equal(months, option.TimelineToMarketMonths);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void ResearchOption_ValidSuccessProbability_ShouldAccept(int percentage)
    {
        // Arrange
        var option = new ResearchOption();

        // Act
        option.SuccessProbabilityPercent = percentage;

        // Assert
        Assert.Equal(percentage, option.SuccessProbabilityPercent);
    }

    [Fact]
    public void ResearchOption_WithBusinessModelData_ShouldSerializeCorrectly()
    {
        // Arrange
        var businessModel = new
        {
            revenue_streams = new[] { "subscription", "licensing" },
            cost_structure = new[] { "development", "marketing", "operations" },
            key_partners = new[] { "technology_providers", "distributors" }
        };

        var option = new ResearchOption
        {
            Title = "SaaS Model",
            BusinessModel = System.Text.Json.JsonSerializer.Serialize(businessModel)
        };

        // Act
        var deserializedModel = System.Text.Json.JsonSerializer.Deserialize<dynamic>(option.BusinessModel);

        // Assert
        Assert.NotNull(deserializedModel);
        Assert.NotEmpty(option.BusinessModel);
    }

    [Fact]
    public void ResearchOption_WithRiskFactors_ShouldSerializeCorrectly()
    {
        // Arrange
        var riskFactors = new[]
        {
            "Market competition",
            "Technical challenges",
            "Regulatory changes"
        };

        var option = new ResearchOption
        {
            Title = "High Risk Option",
            RiskFactors = System.Text.Json.JsonSerializer.Serialize(riskFactors)
        };

        // Act
        var deserializedRisks = System.Text.Json.JsonSerializer.Deserialize<string[]>(option.RiskFactors);

        // Assert
        Assert.NotNull(deserializedRisks);
        Assert.Equal(3, deserializedRisks.Length);
        Assert.Contains("Market competition", deserializedRisks);
    }
}