using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Xunit;

namespace Jackson.Ideas.Core.Tests.Entities;

public class ResearchSessionTests
{
    [Fact]
    public void ResearchSession_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var session = new ResearchSession();

        // Assert
        Assert.Equal(Guid.Empty, session.Id); // New entities have Id = Empty until saved
        Assert.Equal(ResearchStatus.Pending, session.Status);
        Assert.Empty(session.Title);
        Assert.Empty(session.Description);
        Assert.True(session.CreatedAt <= DateTime.UtcNow);
        Assert.Null(session.UpdatedAt); // UpdatedAt is null for new entities
        Assert.Empty(session.ResearchInsights);
        Assert.Empty(session.ResearchOptions);
    }

    [Fact]
    public void ResearchSession_WithUserAndIdea_ShouldSetProperties()
    {
        // Arrange
        var userId = "user123";
        var title = "Test Idea";
        var description = "Test Description";

        // Act
        var session = new ResearchSession
        {
            UserId = userId,
            Title = title,
            Description = description,
            Status = ResearchStatus.InProgress
        };

        // Assert
        Assert.Equal(userId, session.UserId);
        Assert.Equal(title, session.Title);
        Assert.Equal(description, session.Description);
        Assert.Equal(ResearchStatus.InProgress, session.Status);
    }

    [Fact]
    public void ResearchSession_SetStrategy_ShouldUpdateApproachAndDuration()
    {
        // Arrange
        var session = new ResearchSession();
        var approach = "quick_validation";
        var duration = 15;

        // Act
        session.ResearchApproach = approach;
        session.EstimatedDurationMinutes = duration;

        // Assert
        Assert.Equal(approach, session.ResearchApproach);
        Assert.Equal(duration, session.EstimatedDurationMinutes);
    }

    [Theory]
    [InlineData(ResearchStatus.Pending)]
    [InlineData(ResearchStatus.InProgress)]
    [InlineData(ResearchStatus.Completed)]
    [InlineData(ResearchStatus.Failed)]
    public void ResearchSession_StatusTransitions_ShouldBeValid(ResearchStatus status)
    {
        // Arrange
        var session = new ResearchSession();

        // Act
        session.Status = status;

        // Assert
        Assert.Equal(status, session.Status);
    }

    [Fact]
    public void ResearchSession_AddInsight_ShouldMaintainRelationship()
    {
        // Arrange
        var session = new ResearchSession { Id = Guid.NewGuid() };
        var insight = new ResearchInsight
        {
            ResearchSessionId = session.Id,
            Phase = "market_context",
            Content = "Test insight"
        };

        // Act
        session.ResearchInsights.Add(insight);

        // Assert
        Assert.Single(session.ResearchInsights);
        Assert.Equal(session.Id, insight.ResearchSessionId);
    }

    [Fact]
    public void ResearchSession_AddOption_ShouldMaintainRelationship()
    {
        // Arrange
        var session = new ResearchSession { Id = Guid.NewGuid() };
        var option = new ResearchOption
        {
            ResearchSessionId = session.Id,
            Title = "Test Option",
            Approach = "niche_domination"
        };

        // Act
        session.ResearchOptions.Add(option);

        // Assert
        Assert.Single(session.ResearchOptions);
        Assert.Equal(session.Id, option.ResearchSessionId);
    }
}