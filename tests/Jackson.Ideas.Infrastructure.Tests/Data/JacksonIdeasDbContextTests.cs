using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Jackson.Ideas.Infrastructure.Tests.Data;

public class JacksonIdeasDbContextTests : IDisposable
{
    private readonly JacksonIdeasDbContext _context;

    public JacksonIdeasDbContextTests()
    {
        var options = new DbContextOptionsBuilder<JacksonIdeasDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new JacksonIdeasDbContext(options);
    }

    [Fact]
    public async Task CreateResearchSession_ShouldPersistToDatabase()
    {
        // Arrange
        var session = new ResearchSession
        {
            Id = Guid.NewGuid(),
            UserId = "testuser123",
            Title = "Test Research Session",
            Description = "Testing database persistence",
            Status = ResearchStatus.InProgress,
            ResearchApproach = "quick_validation",
            EstimatedDurationMinutes = 15,
            ProgressPercentage = 0.0,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ResearchSessions.Add(session);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.ResearchSessions.FindAsync(session.Id);
        Assert.NotNull(savedSession);
        Assert.Equal(session.Title, savedSession.Title);
        Assert.Equal(session.Description, savedSession.Description);
        Assert.Equal(session.Status, savedSession.Status);
        Assert.Equal(session.UserId, savedSession.UserId);
    }

    [Fact]
    public async Task CreateResearchSessionWithInsights_ShouldMaintainRelationships()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ResearchSession
        {
            Id = sessionId,
            UserId = "testuser123",
            Title = "Session with Insights",
            Description = "Testing relationship persistence",
            Status = ResearchStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        var insights = new List<ResearchInsight>
        {
            new ResearchInsight
            {
                Id = Guid.NewGuid(),
                ResearchSessionId = sessionId,
                Phase = "market_context",
                Content = "Market shows strong growth potential",
                ConfidenceScore = 0.85,
                CreatedAt = DateTime.UtcNow
            },
            new ResearchInsight
            {
                Id = Guid.NewGuid(),
                ResearchSessionId = sessionId,
                Phase = "competitive_intelligence",
                Content = "Moderate competition with opportunities for differentiation",
                ConfidenceScore = 0.78,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        _context.ResearchSessions.Add(session);
        _context.ResearchInsights.AddRange(insights);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.ResearchSessions
            .Include(s => s.ResearchInsights)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        Assert.NotNull(savedSession);
        Assert.Equal(2, savedSession.ResearchInsights.Count);
        Assert.All(savedSession.ResearchInsights, insight => 
        {
            Assert.Equal(sessionId, insight.ResearchSessionId);
            Assert.NotEmpty(insight.Phase);
            Assert.NotEmpty(insight.Content);
            Assert.True(insight.ConfidenceScore > 0);
        });
    }

    [Fact]
    public async Task CreateResearchSessionWithOptions_ShouldMaintainRelationships()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ResearchSession
        {
            Id = sessionId,
            UserId = "testuser123",
            Title = "Session with Options",
            Description = "Testing option relationships",
            Status = ResearchStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        var options = new List<ResearchOption>
        {
            new ResearchOption
            {
                Id = Guid.NewGuid(),
                ResearchSessionId = sessionId,
                Title = "Niche Market Strategy",
                Description = "Focus on specific market segment",
                Approach = "niche_domination",
                OverallScore = 8.5,
                TimelineToMarketMonths = 12,
                SuccessProbabilityPercent = 75,
                EstimatedInvestmentUsd = 500000,
                IsRecommended = true,
                CreatedAt = DateTime.UtcNow
            },
            new ResearchOption
            {
                Id = Guid.NewGuid(),
                ResearchSessionId = sessionId,
                Title = "Mass Market Strategy", 
                Description = "Compete directly with established players",
                Approach = "market_leader_challenge",
                OverallScore = 6.8,
                TimelineToMarketMonths = 18,
                SuccessProbabilityPercent = 45,
                EstimatedInvestmentUsd = 2000000,
                IsRecommended = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Act
        _context.ResearchSessions.Add(session);
        _context.ResearchOptions.AddRange(options);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.ResearchSessions
            .Include(s => s.ResearchOptions)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        Assert.NotNull(savedSession);
        Assert.Equal(2, savedSession.ResearchOptions.Count);
        
        var recommendedOption = savedSession.ResearchOptions.First(o => o.IsRecommended);
        Assert.Equal("Niche Market Strategy", recommendedOption.Title);
        Assert.Equal("niche_domination", recommendedOption.Approach);
        Assert.Equal(8.5, recommendedOption.OverallScore);
        
        var alternativeOption = savedSession.ResearchOptions.First(o => !o.IsRecommended);
        Assert.Equal("Mass Market Strategy", alternativeOption.Title);
        Assert.Equal("market_leader_challenge", alternativeOption.Approach);
    }

    [Fact]
    public async Task UpdateResearchSession_ShouldUpdateTimestamp()
    {
        // Arrange
        var session = new ResearchSession
        {
            Id = Guid.NewGuid(),
            UserId = "testuser123",
            Title = "Test Session",
            Description = "Original description",
            Status = ResearchStatus.InProgress,
            CreatedAt = DateTime.UtcNow.AddHours(-1) // Created 1 hour ago
        };

        _context.ResearchSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        session.Description = "Updated description";
        session.Status = ResearchStatus.Completed;
        await _context.SaveChangesAsync();

        // Assert
        var updatedSession = await _context.ResearchSessions.FindAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal("Updated description", updatedSession.Description);
        Assert.Equal(ResearchStatus.Completed, updatedSession.Status);
        Assert.NotNull(updatedSession.UpdatedAt);
        Assert.True(updatedSession.UpdatedAt > updatedSession.CreatedAt);
    }

    [Fact]
    public async Task QueryResearchSessionsByUserId_ShouldReturnUserSessions()
    {
        // Arrange
        var userId1 = "user1";
        var userId2 = "user2";

        var sessionsUser1 = new List<ResearchSession>
        {
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                Title = "User 1 Session 1",
                Status = ResearchStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = userId1,
                Title = "User 1 Session 2",
                Status = ResearchStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var sessionsUser2 = new List<ResearchSession>
        {
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = userId2,
                Title = "User 2 Session 1",
                Status = ResearchStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.ResearchSessions.AddRange(sessionsUser1);
        _context.ResearchSessions.AddRange(sessionsUser2);
        await _context.SaveChangesAsync();

        // Act
        var user1Sessions = await _context.ResearchSessions
            .Where(s => s.UserId == userId1)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var user2Sessions = await _context.ResearchSessions
            .Where(s => s.UserId == userId2)
            .ToListAsync();

        // Assert
        Assert.Equal(2, user1Sessions.Count);
        Assert.All(user1Sessions, session => Assert.Equal(userId1, session.UserId));
        
        // Verify ordering (newest first)
        Assert.Equal("User 1 Session 2", user1Sessions[0].Title);
        Assert.Equal("User 1 Session 1", user1Sessions[1].Title);

        Assert.Single(user2Sessions);
        Assert.Equal(userId2, user2Sessions[0].UserId);
        Assert.Equal("User 2 Session 1", user2Sessions[0].Title);
    }

    [Fact]
    public async Task DeleteResearchSession_ShouldCascadeDeleteRelatedEntities()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ResearchSession
        {
            Id = sessionId,
            UserId = "testuser123",
            Title = "Session to Delete",
            Status = ResearchStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        var insight = new ResearchInsight
        {
            Id = Guid.NewGuid(),
            ResearchSessionId = sessionId,
            Phase = "test_phase",
            Content = "Test insight",
            ConfidenceScore = 0.8,
            CreatedAt = DateTime.UtcNow
        };

        var option = new ResearchOption
        {
            Id = Guid.NewGuid(),
            ResearchSessionId = sessionId,
            Title = "Test Option",
            Approach = "test_approach",
            OverallScore = 7.5,
            CreatedAt = DateTime.UtcNow
        };

        _context.ResearchSessions.Add(session);
        _context.ResearchInsights.Add(insight);
        _context.ResearchOptions.Add(option);
        await _context.SaveChangesAsync();

        // Act
        _context.ResearchSessions.Remove(session);
        await _context.SaveChangesAsync();

        // Assert
        var deletedSession = await _context.ResearchSessions.FindAsync(sessionId);
        var deletedInsight = await _context.ResearchInsights.FindAsync(insight.Id);
        var deletedOption = await _context.ResearchOptions.FindAsync(option.Id);

        Assert.Null(deletedSession);
        Assert.Null(deletedInsight); // Should be cascade deleted
        Assert.Null(deletedOption);  // Should be cascade deleted
    }

    [Fact]
    public async Task ResearchInsight_JsonSerialization_ShouldWorkCorrectly()
    {
        // Arrange
        var metadata = new
        {
            market_size = 5000000000,
            growth_rate = 12.5,
            trends = new[] { "AI", "Cloud", "Mobile" },
            segments = new
            {
                primary = "Tech professionals",
                secondary = "Small businesses"
            }
        };

        var insight = new ResearchInsight
        {
            Id = Guid.NewGuid(),
            ResearchSessionId = Guid.NewGuid(),
            Phase = "market_context",
            Content = "Comprehensive market analysis results",
            ConfidenceScore = 0.89,
            Metadata = System.Text.Json.JsonSerializer.Serialize(metadata),
            InsightType = "quantitative_analysis",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ResearchInsights.Add(insight);
        await _context.SaveChangesAsync();

        // Assert
        var savedInsight = await _context.ResearchInsights.FindAsync(insight.Id);
        Assert.NotNull(savedInsight);
        Assert.NotEmpty(savedInsight.Metadata);
        
        // Verify JSON can be deserialized
        var deserializedMetadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(savedInsight.Metadata);
        Assert.NotNull(deserializedMetadata);
        Assert.True(deserializedMetadata.ContainsKey("market_size"));
        Assert.True(deserializedMetadata.ContainsKey("growth_rate"));
    }

    [Fact]
    public async Task ResearchOption_JsonSerialization_ShouldWorkCorrectly()
    {
        // Arrange
        var businessModel = new
        {
            revenue_streams = new[] { "subscription", "licensing", "consulting" },
            cost_structure = new[] { "development", "marketing", "operations" },
            key_resources = new[] { "technology", "talent", "partnerships" }
        };

        var riskFactors = new[] { "Market competition", "Technical challenges", "Regulatory changes" };
        var mitigationStrategies = new[] { "Focused execution", "Strong partnerships", "Agile development" };

        var option = new ResearchOption
        {
            Id = Guid.NewGuid(),
            ResearchSessionId = Guid.NewGuid(),
            Title = "SaaS Business Model",
            Description = "Subscription-based software as a service approach",
            Approach = "innovation_leadership",
            TargetCustomerSegment = "Mid-market businesses",
            ValueProposition = "50% cost reduction with 3x productivity improvement",
            GoToMarketStrategy = "Direct sales with partner channel development",
            OverallScore = 8.2,
            TimelineToMarketMonths = 15,
            TimelineToProfitabilityMonths = 24,
            SuccessProbabilityPercent = 72,
            EstimatedInvestmentUsd = 1500000,
            BusinessModel = System.Text.Json.JsonSerializer.Serialize(businessModel),
            RiskFactors = System.Text.Json.JsonSerializer.Serialize(riskFactors),
            MitigationStrategies = System.Text.Json.JsonSerializer.Serialize(mitigationStrategies),
            IsRecommended = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ResearchOptions.Add(option);
        await _context.SaveChangesAsync();

        // Assert
        var savedOption = await _context.ResearchOptions.FindAsync(option.Id);
        Assert.NotNull(savedOption);
        Assert.NotEmpty(savedOption.BusinessModel);
        Assert.NotEmpty(savedOption.RiskFactors);
        Assert.NotEmpty(savedOption.MitigationStrategies);
        
        // Verify JSON can be deserialized
        var deserializedBusinessModel = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(savedOption.BusinessModel);
        var deserializedRiskFactors = System.Text.Json.JsonSerializer.Deserialize<string[]>(savedOption.RiskFactors);
        var deserializedMitigationStrategies = System.Text.Json.JsonSerializer.Deserialize<string[]>(savedOption.MitigationStrategies);
        
        Assert.NotNull(deserializedBusinessModel);
        Assert.NotNull(deserializedRiskFactors);
        Assert.NotNull(deserializedMitigationStrategies);
        Assert.Equal(3, deserializedRiskFactors.Length);
        Assert.Equal(3, deserializedMitigationStrategies.Length);
    }

    [Fact]
    public async Task QueryResearchSessionsWithIncludes_ShouldLoadRelatedData()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ResearchSession
        {
            Id = sessionId,
            UserId = "testuser123",
            Title = "Complete Research Session",
            Status = ResearchStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        var insights = new List<ResearchInsight>
        {
            new ResearchInsight { Id = Guid.NewGuid(), ResearchSessionId = sessionId, Phase = "market_context", Content = "Market insight", CreatedAt = DateTime.UtcNow },
            new ResearchInsight { Id = Guid.NewGuid(), ResearchSessionId = sessionId, Phase = "competitive_intelligence", Content = "Competitive insight", CreatedAt = DateTime.UtcNow }
        };

        var options = new List<ResearchOption>
        {
            new ResearchOption { Id = Guid.NewGuid(), ResearchSessionId = sessionId, Title = "Option 1", Approach = "niche_domination", CreatedAt = DateTime.UtcNow },
            new ResearchOption { Id = Guid.NewGuid(), ResearchSessionId = sessionId, Title = "Option 2", Approach = "market_leader_challenge", CreatedAt = DateTime.UtcNow }
        };

        _context.ResearchSessions.Add(session);
        _context.ResearchInsights.AddRange(insights);
        _context.ResearchOptions.AddRange(options);
        await _context.SaveChangesAsync();

        // Act
        var loadedSession = await _context.ResearchSessions
            .Include(s => s.ResearchInsights)
            .Include(s => s.ResearchOptions)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        // Assert
        Assert.NotNull(loadedSession);
        Assert.Equal(2, loadedSession.ResearchInsights.Count);
        Assert.Equal(2, loadedSession.ResearchOptions.Count);
        
        // Verify insights are loaded
        var marketInsight = loadedSession.ResearchInsights.First(i => i.Phase == "market_context");
        Assert.Equal("Market insight", marketInsight.Content);
        
        // Verify options are loaded
        var nicheOption = loadedSession.ResearchOptions.First(o => o.Approach == "niche_domination");
        Assert.Equal("Option 1", nicheOption.Title);
    }

    [Theory]
    [InlineData(ResearchStatus.Pending)]
    [InlineData(ResearchStatus.InProgress)]
    [InlineData(ResearchStatus.Completed)]
    [InlineData(ResearchStatus.Failed)]
    public async Task ResearchSession_AllStatusValues_ShouldPersistCorrectly(ResearchStatus status)
    {
        // Arrange
        var session = new ResearchSession
        {
            Id = Guid.NewGuid(),
            UserId = "testuser123",
            Title = $"Session with {status} status",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ResearchSessions.Add(session);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.ResearchSessions.FindAsync(session.Id);
        Assert.NotNull(savedSession);
        Assert.Equal(status, savedSession.Status);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}