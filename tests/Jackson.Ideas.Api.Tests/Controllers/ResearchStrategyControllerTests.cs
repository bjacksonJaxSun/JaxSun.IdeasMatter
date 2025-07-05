using Jackson.Ideas.Api.Controllers;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Jackson.Ideas.Api.Tests.Controllers;

public class ResearchStrategyControllerTests
{
    private readonly Mock<IResearchStrategyService> _mockResearchStrategyService;
    private readonly Mock<ILogger<ResearchStrategyController>> _mockLogger;
    private readonly ResearchStrategyController _controller;

    public ResearchStrategyControllerTests()
    {
        _mockResearchStrategyService = new Mock<IResearchStrategyService>();
        _mockLogger = new Mock<ILogger<ResearchStrategyController>>();
        
        _controller = new ResearchStrategyController(
            _mockResearchStrategyService.Object,
            _mockLogger.Object);

        // Setup user context for authorization
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task AnalyzeIdeaAsync_WithValidRequest_ShouldReturnAnalysisResult()
    {
        // Arrange
        var request = new ResearchStrategyRequest
        {
            IdeaDescription = "AI-powered task management app for busy professionals",
            UserGoals = "Build an MVP and validate market demand",
            Parameters = new Dictionary<string, object>
            {
                { "approach", "quick_validation" },
                { "target_market", "professionals" }
            }
        };

        var expectedResponse = new ResearchStrategyResponse
        {
            StrategyId = Guid.NewGuid(),
            Approach = ResearchApproach.QuickValidation,
            Title = "Quick Validation Analysis",
            Description = "Rapid validation of core business assumptions",
            EstimatedDurationMinutes = 15,
            ComplexityLevel = "beginner",
            Status = "pending",
            ProgressPercentage = 0.0,
            CreatedAt = DateTime.UtcNow,
            EstimatedCompletionTime = "15 minutes",
            IncludedAnalyses = new List<string> 
            { 
                "Market opportunity assessment", 
                "Basic competitive analysis", 
                "Strategic recommendation" 
            },
            NextSteps = new List<string> 
            { 
                "Validate key assumptions", 
                "Create MVP prototype", 
                "Test with early adopters" 
            }
        };

        _mockResearchStrategyService
            .Setup(x => x.AnalyzeIdeaAsync(
                request.IdeaDescription, 
                request.UserGoals, 
                request.Parameters, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AnalyzeIdeaAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ResearchStrategyResponse>(okResult.Value);
        
        Assert.Equal(expectedResponse.StrategyId, response.StrategyId);
        Assert.Equal(expectedResponse.Approach, response.Approach);
        Assert.Equal(expectedResponse.Title, response.Title);
        Assert.Equal(expectedResponse.EstimatedDurationMinutes, response.EstimatedDurationMinutes);
        Assert.Equal(expectedResponse.Status, response.Status);
        Assert.NotEmpty(response.IncludedAnalyses);
        Assert.NotEmpty(response.NextSteps);
        
        // Verify service was called with correct parameters
        _mockResearchStrategyService.Verify(
            x => x.AnalyzeIdeaAsync(
                request.IdeaDescription, 
                request.UserGoals, 
                request.Parameters, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeIdeaAsync_WithEmptyIdeaDescription_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ResearchStrategyRequest
        {
            IdeaDescription = "",
            UserGoals = "Valid goals",
            Parameters = new Dictionary<string, object>()
        };

        _mockResearchStrategyService
            .Setup(x => x.AnalyzeIdeaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Idea description is required"));

        // Act
        var result = await _controller.AnalyzeIdeaAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = badRequestResult.Value;
        
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Contains("Idea description is required", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Fact]
    public async Task SuggestResearchApproachesAsync_WithValidIdeaDescription_ShouldReturnApproaches()
    {
        // Arrange
        var ideaDescription = "SaaS platform for small business accounting";
        var expectedApproaches = new List<string>
        {
            "Quick Validation",
            "Market Deep-Dive", 
            "Launch Strategy"
        };

        _mockResearchStrategyService
            .Setup(x => x.SuggestResearchApproachesAsync(ideaDescription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedApproaches);

        // Act
        var result = await _controller.SuggestResearchApproachesAsync(ideaDescription);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var approaches = Assert.IsType<List<string>>(okResult.Value);
        
        Assert.Equal(3, approaches.Count);
        Assert.Contains("Quick Validation", approaches);
        Assert.Contains("Market Deep-Dive", approaches);
        Assert.Contains("Launch Strategy", approaches);
        
        // Verify service was called
        _mockResearchStrategyService.Verify(
            x => x.SuggestResearchApproachesAsync(ideaDescription, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SuggestResearchApproachesAsync_WithEmptyIdeaDescription_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyIdeaDescription = "";

        _mockResearchStrategyService
            .Setup(x => x.SuggestResearchApproachesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Idea description cannot be empty"));

        // Act
        var result = await _controller.SuggestResearchApproachesAsync(emptyIdeaDescription);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = badRequestResult.Value;
        
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Contains("Idea description cannot be empty", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Fact]
    public async Task ValidateResearchApproachAsync_WithValidRequest_ShouldReturnValidationResult()
    {
        // Arrange
        var request = new ValidateApproachRequest
        {
            IdeaDescription = "AI chatbot for customer service",
            Approach = "quick_validation",
            Parameters = new Dictionary<string, object>
            {
                { "time_constraint", "high" },
                { "budget", "limited" }
            }
        };

        _mockResearchStrategyService
            .Setup(x => x.ValidateResearchApproachAsync(
                request.IdeaDescription, 
                request.Approach, 
                request.Parameters, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateResearchApproachAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        
        Assert.NotNull(response);
        
        // Check the anonymous object properties
        var responseType = response.GetType();
        var isValidProperty = responseType.GetProperty("isValid");
        var approachProperty = responseType.GetProperty("approach");
        
        Assert.True((bool)isValidProperty.GetValue(response));
        Assert.Equal(request.Approach, approachProperty.GetValue(response));
        
        // Verify service was called
        _mockResearchStrategyService.Verify(
            x => x.ValidateResearchApproachAsync(
                request.IdeaDescription, 
                request.Approach, 
                request.Parameters, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateResearchApproachAsync_WithInvalidApproach_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new ValidateApproachRequest
        {
            IdeaDescription = "Valid idea",
            Approach = "", // Invalid empty approach
            Parameters = new Dictionary<string, object>()
        };

        _mockResearchStrategyService
            .Setup(x => x.ValidateResearchApproachAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Research approach is required"));

        // Act
        var result = await _controller.ValidateResearchApproachAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = badRequestResult.Value;
        
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Contains("Research approach is required", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Fact]
    public async Task TrackAnalysisProgressAsync_WithValidRequest_ShouldReturnProgressUpdate()
    {
        // Arrange
        var request = new TrackProgressRequest
        {
            SessionId = Guid.NewGuid().ToString(),
            Parameters = new Dictionary<string, object>
            {
                { "detailed_tracking", true }
            }
        };

        var expectedProgress = new AnalysisProgressUpdate
        {
            StrategyId = Guid.NewGuid(),
            CurrentPhase = "competitive_intelligence",
            ProgressPercentage = 65.0,
            EstimatedCompletionMinutes = 8,
            PhaseDetails = "Analyzing competitive landscape and market positioning",
            CompletedPhases = new List<string> { "market_context", "initial_analysis" },
            RemainingPhases = new List<string> { "strategic_assessment", "recommendations" }
        };

        _mockResearchStrategyService
            .Setup(x => x.TrackAnalysisProgressAsync(
                request.SessionId, 
                request.Parameters, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProgress);

        // Act
        var result = await _controller.TrackAnalysisProgressAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var progress = Assert.IsType<AnalysisProgressUpdate>(okResult.Value);
        
        Assert.Equal(expectedProgress.StrategyId, progress.StrategyId);
        Assert.Equal(expectedProgress.CurrentPhase, progress.CurrentPhase);
        Assert.Equal(expectedProgress.ProgressPercentage, progress.ProgressPercentage);
        Assert.Equal(expectedProgress.EstimatedCompletionMinutes, progress.EstimatedCompletionMinutes);
        Assert.NotEmpty(progress.CompletedPhases);
        Assert.NotEmpty(progress.RemainingPhases);
        
        // Verify service was called
        _mockResearchStrategyService.Verify(
            x => x.TrackAnalysisProgressAsync(
                request.SessionId, 
                request.Parameters, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TrackAnalysisProgressAsync_WithInvalidSessionId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new TrackProgressRequest
        {
            SessionId = "", // Invalid empty session ID
            Parameters = new Dictionary<string, object>()
        };

        _mockResearchStrategyService
            .Setup(x => x.TrackAnalysisProgressAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Session ID is required"));

        // Act
        var result = await _controller.TrackAnalysisProgressAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = badRequestResult.Value;
        
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Contains("Session ID is required", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Fact]
    public async Task AnalyzeIdeaAsync_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new ResearchStrategyRequest
        {
            IdeaDescription = "Valid idea",
            UserGoals = "Valid goals",
            Parameters = new Dictionary<string, object>()
        };

        _mockResearchStrategyService
            .Setup(x => x.AnalyzeIdeaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI service is temporarily unavailable"));

        // Act
        var result = await _controller.AnalyzeIdeaAsync(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        
        var errorResponse = statusCodeResult.Value;
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Equal("An error occurred during analysis", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SuggestResearchApproachesAsync_WithInvalidIdeaDescription_ShouldReturnBadRequest(string invalidDescription)
    {
        // Arrange
        _mockResearchStrategyService
            .Setup(x => x.SuggestResearchApproachesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Idea description cannot be empty"));

        // Act
        var result = await _controller.SuggestResearchApproachesAsync(invalidDescription);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task ValidateResearchApproachAsync_WithNullParameters_ShouldStillWork()
    {
        // Arrange
        var request = new ValidateApproachRequest
        {
            IdeaDescription = "Valid idea",
            Approach = "quick_validation",
            Parameters = null // Null parameters should be handled gracefully
        };

        _mockResearchStrategyService
            .Setup(x => x.ValidateResearchApproachAsync(
                request.IdeaDescription, 
                request.Approach, 
                request.Parameters, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateResearchApproachAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
        
        // Verify service was called with null parameters
        _mockResearchStrategyService.Verify(
            x => x.ValidateResearchApproachAsync(
                request.IdeaDescription, 
                request.Approach, 
                null, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TrackAnalysisProgressAsync_WithNullParameters_ShouldStillWork()
    {
        // Arrange
        var request = new TrackProgressRequest
        {
            SessionId = Guid.NewGuid().ToString(),
            Parameters = null // Null parameters should be handled gracefully
        };

        var expectedProgress = new AnalysisProgressUpdate
        {
            StrategyId = Guid.NewGuid(),
            CurrentPhase = "market_context",
            ProgressPercentage = 25.0,
            EstimatedCompletionMinutes = 12
        };

        _mockResearchStrategyService
            .Setup(x => x.TrackAnalysisProgressAsync(
                request.SessionId, 
                request.Parameters, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProgress);

        // Act
        var result = await _controller.TrackAnalysisProgressAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var progress = Assert.IsType<AnalysisProgressUpdate>(okResult.Value);
        Assert.Equal(expectedProgress.CurrentPhase, progress.CurrentPhase);
        
        // Verify service was called with null parameters
        _mockResearchStrategyService.Verify(
            x => x.TrackAnalysisProgressAsync(
                request.SessionId, 
                null, 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

// Supporting class for testing if not already defined
public class ResearchStrategyInfo
{
    public ResearchApproach Approach { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Complexity { get; set; } = string.Empty;
    public List<string> BestFor { get; set; } = new();
    public List<string> Includes { get; set; } = new();
    public List<string> Deliverables { get; set; } = new();
}