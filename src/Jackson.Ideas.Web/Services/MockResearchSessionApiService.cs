using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Web.Services;

public class MockResearchSessionApiService : IResearchSessionApiService
{
    private readonly ILogger<MockResearchSessionApiService> _logger;
    private readonly List<ResearchSession> _mockSessions;

    public MockResearchSessionApiService(ILogger<MockResearchSessionApiService> logger)
    {
        _logger = logger;
        
        // Create mock research sessions for demo
        _mockSessions = new List<ResearchSession>
        {
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                Title = "AI-Powered Healthcare Assistant",
                Description = "Developing an AI assistant for healthcare professionals to streamline patient care",
                Status = ResearchStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-2),
                UserId = "demo-user-123",
                ResearchType = "Market Analysis",
                Goals = "Analyze market demand, identify competitors, assess technical feasibility"
            },
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                Title = "Smart Home Security System",
                Description = "IoT-based security system with facial recognition and automated alerts",
                Status = ResearchStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                UserId = "demo-user-123",
                ResearchType = "Technical Research",
                Goals = "Evaluate hardware requirements, security protocols, and user experience"
            },
            new ResearchSession
            {
                Id = Guid.NewGuid(),
                Title = "Sustainable Food Delivery",
                Description = "Eco-friendly food delivery service using electric vehicles and biodegradable packaging",
                Status = ResearchStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                UserId = "demo-user-123",
                ResearchType = "Business Strategy",
                Goals = "Research sustainability impact, cost analysis, and market positioning"
            }
        };
    }

    public Task<ResearchSession?> CreateSessionAsync(CreateSessionRequest request)
    {
        _logger.LogInformation("Mock research service: Creating new session '{Title}'", request.Title);
        
        var session = new ResearchSession
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = ResearchStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = "demo-user-123",
            ResearchType = request.ResearchType ?? "General Research",
            Goals = request.Goals ?? "General research goals"
        };
        
        _mockSessions.Add(session);
        return Task.FromResult<ResearchSession?>(session);
    }

    public Task<ResearchSession?> GetSessionAsync(Guid sessionId)
    {
        _logger.LogInformation("Mock research service: Getting session {SessionId}", sessionId);
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        return Task.FromResult(session);
    }

    public Task<List<ResearchSession>> GetUserSessionsAsync()
    {
        _logger.LogInformation("Mock research service: Getting all user sessions");
        return Task.FromResult(_mockSessions);
    }

    public Task<ResearchSession?> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request)
    {
        _logger.LogInformation("Mock research service: Updating session {SessionId}", sessionId);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            session.Title = request.Title ?? session.Title;
            session.Description = request.Description ?? session.Description;
            session.UpdatedAt = DateTime.UtcNow;
        }
        
        return Task.FromResult(session);
    }

    public Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        _logger.LogInformation("Mock research service: Deleting session {SessionId}", sessionId);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            _mockSessions.Remove(session);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<ResearchSession?> AddInsightAsync(Guid sessionId, AddInsightRequest request)
    {
        _logger.LogInformation("Mock research service: Adding insight to session {SessionId}", sessionId);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
            // In a real implementation, this would add the insight to the session
        }
        
        return Task.FromResult(session);
    }

    public Task<ResearchSession?> AddOptionAsync(Guid sessionId, AddOptionRequest request)
    {
        _logger.LogInformation("Mock research service: Adding option to session {SessionId}", sessionId);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
            // In a real implementation, this would add the option to the session
        }
        
        return Task.FromResult(session);
    }

    public Task<ResearchSession?> UpdateSessionStatusAsync(Guid sessionId, UpdateStatusRequest request)
    {
        _logger.LogInformation("Mock research service: Updating session {SessionId} status to {Status}", sessionId, request.Status);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            if (Enum.TryParse<ResearchStatus>(request.Status, out var status))
            {
                session.Status = status;
            }
            session.UpdatedAt = DateTime.UtcNow;
        }
        
        return Task.FromResult(session);
    }

    public Task<bool> StartResearchExecutionAsync(Guid sessionId)
    {
        _logger.LogInformation("Mock research service: Starting research execution for session {SessionId}", sessionId);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            session.Status = ResearchStatus.InProgress;
            session.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<object?> GetResearchProgressAsync(Guid sessionId)
    {
        _logger.LogInformation("Mock research service: Getting research progress for session {SessionId}", sessionId);
        
        var session = _mockSessions.FirstOrDefault(s => s.Id == sessionId);
        if (session != null)
        {
            var progress = new
            {
                SessionId = sessionId,
                Status = session.Status.ToString(),
                Progress = session.Status switch
                {
                    ResearchStatus.Pending => 0,
                    ResearchStatus.InProgress => 65,
                    ResearchStatus.Completed => 100,
                    _ => 0
                },
                CurrentStep = session.Status switch
                {
                    ResearchStatus.Pending => "Waiting to start",
                    ResearchStatus.InProgress => "Analyzing market data",
                    ResearchStatus.Completed => "Research complete",
                    _ => "Unknown"
                },
                EstimatedTimeRemaining = session.Status switch
                {
                    ResearchStatus.Pending => "15 minutes",
                    ResearchStatus.InProgress => "5 minutes",
                    ResearchStatus.Completed => "Complete",
                    _ => "Unknown"
                }
            };
            
            return Task.FromResult<object?>(progress);
        }
        
        return Task.FromResult<object?>(null);
    }
}