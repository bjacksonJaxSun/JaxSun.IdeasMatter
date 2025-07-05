using Jackson.Ideas.Core.Entities;
using Xunit;

namespace Jackson.Ideas.Core.Tests.Entities;

public class ResearchInsightTests
{
    [Fact]
    public void ResearchInsight_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var insight = new ResearchInsight();

        // Assert
        Assert.Equal(Guid.Empty, insight.Id); // New entities have Id = Empty until saved
        Assert.Empty(insight.Phase);
        Assert.Empty(insight.Content);
        Assert.Equal(0.0, insight.ConfidenceScore);
        Assert.True(insight.CreatedAt <= DateTime.UtcNow);
        Assert.Null(insight.UpdatedAt); // UpdatedAt is null for new entities
    }

    [Fact]
    public void ResearchInsight_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var phase = "market_context";
        var content = "Market analysis insights";
        var confidence = 0.85;

        // Act
        var insight = new ResearchInsight
        {
            ResearchSessionId = sessionId,
            Phase = phase,
            Content = content,
            ConfidenceScore = confidence
        };

        // Assert
        Assert.Equal(sessionId, insight.ResearchSessionId);
        Assert.Equal(phase, insight.Phase);
        Assert.Equal(content, insight.Content);
        Assert.Equal(confidence, insight.ConfidenceScore);
    }

    [Theory]
    [InlineData("market_context")]
    [InlineData("competitive_intelligence")]
    [InlineData("customer_understanding")]
    [InlineData("strategic_assessment")]
    public void ResearchInsight_ValidPhases_ShouldAccept(string phase)
    {
        // Arrange
        var insight = new ResearchInsight();

        // Act
        insight.Phase = phase;

        // Assert
        Assert.Equal(phase, insight.Phase);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void ResearchInsight_ValidConfidenceScore_ShouldAccept(double confidence)
    {
        // Arrange
        var insight = new ResearchInsight();

        // Act
        insight.ConfidenceScore = confidence;

        // Assert
        Assert.Equal(confidence, insight.ConfidenceScore);
    }

    [Fact]
    public void ResearchInsight_WithMetadata_ShouldSerializeCorrectly()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["market_size"] = 5000000000,
            ["growth_rate"] = 12.5,
            ["trends"] = new[] { "AI", "Cloud", "Mobile" }
        };

        var insight = new ResearchInsight
        {
            Phase = "market_context",
            Content = "Market analysis",
            Metadata = System.Text.Json.JsonSerializer.Serialize(metadata)
        };

        // Act
        var deserializedMetadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(insight.Metadata);

        // Assert
        Assert.NotNull(deserializedMetadata);
        Assert.Equal(3, deserializedMetadata.Count);
        Assert.True(deserializedMetadata.ContainsKey("market_size"));
    }
}