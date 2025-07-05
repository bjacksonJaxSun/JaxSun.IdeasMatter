using Jackson.Ideas.Api.Controllers;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Jackson.Ideas.Api.Tests.Controllers;

public class ResearchSessionControllerTests
{
    private readonly Mock<IResearchSessionService> _mockResearchSessionService;
    private readonly Mock<ILogger<ResearchSessionController>> _mockLogger;
    private readonly ResearchSessionController _controller;
    private readonly string _testUserId = "123";

    public ResearchSessionControllerTests()
    {
        _mockResearchSessionService = new Mock<IResearchSessionService>();
        _mockLogger = new Mock<ILogger<ResearchSessionController>>();
        
        _controller = new ResearchSessionController(
            _mockResearchSessionService.Object,
            _mockLogger.Object);

        // Setup user context for authorization
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId)
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
    public async Task CreateSessionAsync_WithValidRequest_ShouldReturnCreatedSession()
    {
        // Arrange
        var request = new CreateSessionRequest
        {
            IdeaDescription = "AI-powered task management app",
            Goals = "Validate market opportunity and build MVP"
        };

        var expectedSession = new ResearchSession
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = request.IdeaDescription,
            Description = request.Goals,
            Status = ResearchStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _mockResearchSessionService
            .Setup(x => x.CreateSessionAsync(_testUserId, request.IdeaDescription, request.Goals))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _controller.CreateSessionAsync(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var session = Assert.IsType<ResearchSession>(createdResult.Value);
        
        Assert.Equal(expectedSession.Id, session.Id);
        Assert.Equal(expectedSession.Title, session.Title);
        Assert.Equal(expectedSession.Description, session.Description);
        Assert.Equal(expectedSession.Status, session.Status);
        
        // Verify service was called with correct parameters
        _mockResearchSessionService.Verify(
            x => x.CreateSessionAsync(_testUserId, request.IdeaDescription, request.Goals),
            Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_WithEmptyIdeaDescription_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateSessionRequest
        {
            IdeaDescription = "",
            Goals = "Valid goals"
        };

        _mockResearchSessionService
            .Setup(x => x.CreateSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Idea description is required"));

        // Act
        var result = await _controller.CreateSessionAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var errorResponse = badRequestResult.Value;
        
        Assert.NotNull(errorResponse);
        // Check that error message is included
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.NotNull(errorProperty);
        Assert.Contains("Idea description is required", errorProperty.GetValue(errorResponse)?.ToString());
    }

    [Fact]
    public async Task GetSessionAsync_WithValidSessionId_ShouldReturnSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Idea",
            Description = "Test Goals",
            Status = ResearchStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(expectedSession);

        // Act
        var result = await _controller.GetSessionAsync(sessionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<ResearchSession>(okResult.Value);
        
        Assert.Equal(expectedSession.Id, session.Id);
        Assert.Equal(expectedSession.Title, session.Title);
        Assert.Equal(expectedSession.UserId, session.UserId);
        
        // Verify service was called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetSessionAsync_WithNonExistentSessionId_ShouldReturnNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync((ResearchSession?)null);

        // Act
        var result = await _controller.GetSessionAsync(sessionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var errorResponse = notFoundResult.Value;
        
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Equal("Session not found", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Fact]
    public async Task GetSessionAsync_WithSessionBelongingToDifferentUser_ShouldReturnForbid()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var sessionWithDifferentUser = new ResearchSession
        {
            Id = sessionId,
            UserId = "999", // Different user ID
            Title = "Test Idea",
            Status = ResearchStatus.InProgress
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(sessionWithDifferentUser);

        // Act
        var result = await _controller.GetSessionAsync(sessionId);

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
        
        // Verify service was called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ShouldReturnUserSessions()
    {
        // Arrange
        var expectedSessions = new List<ResearchSession>
        {
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Title = "Session 1",
                Status = ResearchStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                Title = "Session 2",
                Status = ResearchStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockResearchSessionService
            .Setup(x => x.GetUserSessionsAsync(_testUserId))
            .ReturnsAsync(expectedSessions);

        // Act
        var result = await _controller.GetUserSessionsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessions = Assert.IsType<List<ResearchSession>>(okResult.Value);
        
        Assert.Equal(2, sessions.Count);
        Assert.All(sessions, session => Assert.Equal(_testUserId, session.UserId));
        
        // Verify service was called
        _mockResearchSessionService.Verify(x => x.GetUserSessionsAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionAsync_WithValidRequest_ShouldReturnUpdatedSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Original Title",
            Description = "Original Description",
            Status = ResearchStatus.InProgress
        };

        var updateRequest = new UpdateSessionRequest
        {
            IdeaDescription = "Updated Title",
            Goals = "Updated Goals"
        };

        var updatedSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = updateRequest.IdeaDescription,
            Description = updateRequest.Goals,
            Status = ResearchStatus.InProgress,
            UpdatedAt = DateTime.UtcNow
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(existingSession);

        _mockResearchSessionService
            .Setup(x => x.UpdateSessionAsync(It.IsAny<ResearchSession>()))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _controller.UpdateSessionAsync(sessionId, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<ResearchSession>(okResult.Value);
        
        Assert.Equal(updatedSession.Title, session.Title);
        Assert.Equal(updatedSession.Description, session.Description);
        
        // Verify service methods were called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
        _mockResearchSessionService.Verify(x => x.UpdateSessionAsync(It.IsAny<ResearchSession>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSessionAsync_WithValidSessionId_ShouldReturnNoContent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.InProgress
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(existingSession);

        _mockResearchSessionService
            .Setup(x => x.DeleteSessionAsync(sessionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteSessionAsync(sessionId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        // Verify service methods were called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
        _mockResearchSessionService.Verify(x => x.DeleteSessionAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task AddInsightAsync_WithValidRequest_ShouldReturnUpdatedSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.InProgress
        };

        var insightRequest = new AddInsightRequest
        {
            Title = "Market Insight",
            Content = "Market shows strong demand for productivity tools",
            Category = "Market Analysis",
            Priority = 1
        };

        var updatedSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.InProgress,
            ResearchInsights = new List<ResearchInsight>
            {
                new ResearchInsight
                {
                    Id = Guid.NewGuid(),
                    ResearchSessionId = sessionId,
                    Content = insightRequest.Content,
                    Phase = insightRequest.Category
                }
            }
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(existingSession);

        _mockResearchSessionService
            .Setup(x => x.AddInsightToSessionAsync(sessionId, It.IsAny<ResearchInsight>()))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _controller.AddInsightAsync(sessionId, insightRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<ResearchSession>(okResult.Value);
        
        Assert.Equal(sessionId, session.Id);
        Assert.NotEmpty(session.ResearchInsights);
        
        // Verify service methods were called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
        _mockResearchSessionService.Verify(x => x.AddInsightToSessionAsync(sessionId, It.IsAny<ResearchInsight>()), Times.Once);
    }

    [Fact]
    public async Task AddOptionAsync_WithValidRequest_ShouldReturnUpdatedSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.InProgress
        };

        var optionRequest = new AddOptionRequest
        {
            Title = "Freemium Model",
            Description = "Free tier with premium features",
            FeasibilityScore = 8.5,
            ImpactScore = 7.2,
            Notes = "High conversion potential"
        };

        var updatedSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.InProgress,
            ResearchOptions = new List<ResearchOption>
            {
                new ResearchOption
                {
                    Id = Guid.NewGuid(),
                    ResearchSessionId = sessionId,
                    Title = optionRequest.Title,
                    Description = optionRequest.Description
                }
            }
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(existingSession);

        _mockResearchSessionService
            .Setup(x => x.AddOptionToSessionAsync(sessionId, It.IsAny<ResearchOption>()))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _controller.AddOptionAsync(sessionId, optionRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<ResearchSession>(okResult.Value);
        
        Assert.Equal(sessionId, session.Id);
        Assert.NotEmpty(session.ResearchOptions);
        Assert.Equal(optionRequest.Title, session.ResearchOptions.First().Title);
        
        // Verify service methods were called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
        _mockResearchSessionService.Verify(x => x.AddOptionToSessionAsync(sessionId, It.IsAny<ResearchOption>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionStatusAsync_WithValidRequest_ShouldReturnUpdatedSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.InProgress
        };

        var statusRequest = new UpdateStatusRequest
        {
            Status = "Completed"
        };

        var updatedSession = new ResearchSession
        {
            Id = sessionId,
            UserId = _testUserId,
            Title = "Test Session",
            Status = ResearchStatus.Completed,
            UpdatedAt = DateTime.UtcNow
        };

        _mockResearchSessionService
            .Setup(x => x.GetSessionAsync(sessionId))
            .ReturnsAsync(existingSession);

        _mockResearchSessionService
            .Setup(x => x.UpdateSessionStatusAsync(sessionId, statusRequest.Status))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _controller.UpdateSessionStatusAsync(sessionId, statusRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var session = Assert.IsType<ResearchSession>(okResult.Value);
        
        Assert.Equal(ResearchStatus.Completed, session.Status);
        Assert.NotNull(session.UpdatedAt);
        
        // Verify service methods were called
        _mockResearchSessionService.Verify(x => x.GetSessionAsync(sessionId), Times.Once);
        _mockResearchSessionService.Verify(x => x.UpdateSessionStatusAsync(sessionId, statusRequest.Status), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new CreateSessionRequest
        {
            IdeaDescription = "Valid idea",
            Goals = "Valid goals"
        };

        _mockResearchSessionService
            .Setup(x => x.CreateSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _controller.CreateSessionAsync(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        
        var errorResponse = statusCodeResult.Value;
        Assert.NotNull(errorResponse);
        var errorProperty = errorResponse.GetType().GetProperty("error");
        Assert.Equal("An error occurred while creating the session", errorProperty?.GetValue(errorResponse)?.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateSessionAsync_WithInvalidIdeaDescription_ShouldReturnBadRequest(string invalidDescription)
    {
        // Arrange
        var request = new CreateSessionRequest
        {
            IdeaDescription = invalidDescription,
            Goals = "Valid goals"
        };

        _mockResearchSessionService
            .Setup(x => x.CreateSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Idea description is required"));

        // Act
        var result = await _controller.CreateSessionAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }
}