using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.Services;
using Jackson.Ideas.Core.DTOs.Research;
using Microsoft.Extensions.Logging;

namespace Jackson.Ideas.Application.Services;

public class ResearchSessionService : IResearchSessionService
{
    private readonly IResearchRepository _researchRepository;
    private readonly IResearchBackgroundService _backgroundService;
    private readonly ILogger<ResearchSessionService> _logger;

    public ResearchSessionService(
        IResearchRepository researchRepository,
        IResearchBackgroundService backgroundService,
        ILogger<ResearchSessionService> logger)
    {
        _researchRepository = researchRepository;
        _backgroundService = backgroundService;
        _logger = logger;
    }

    public async Task<ResearchSession> CreateSessionAsync(CreateSessionRequest request)
    {
        _logger.LogInformation("Creating new research session for user {UserId}", request.UserId);

        if (string.IsNullOrEmpty(request.UserId))
            throw new ArgumentException("User ID is required", nameof(request.UserId));

        if (string.IsNullOrEmpty(request.Description))
            throw new ArgumentException("Idea description is required", nameof(request.Description));

        var session = new ResearchSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title ?? request.Description,
            Description = request.Description,
            Status = ResearchStatus.Pending,
            ResearchType = request.ResearchType,
            Goals = request.Goals,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ResearchInsights = new List<ResearchInsight>(),
            ResearchOptions = new List<ResearchOption>()
        };

        var createdSession = await _researchRepository.CreateSessionAsync(session);
        
        _logger.LogInformation("Research session {SessionId} created successfully for user {UserId}", 
            createdSession.Id, request.UserId);

        // Trigger background research workflow
        try
        {
            var researchType = request.ResearchType ?? "Market Deep-Dive";
            var userGoals = request.Goals ?? "Comprehensive market analysis";
            
            var taskIds = await _backgroundService.EnqueueResearchWorkflowAsync(
                createdSession.Id.ToString(),
                request.Description,
                researchType,
                userGoals);
                
            _logger.LogInformation("Triggered research workflow for session {SessionId} with {TaskCount} tasks", 
                createdSession.Id, taskIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger research workflow for session {SessionId}", createdSession.Id);
            // Don't fail the session creation, just log the error
        }

        return createdSession;
    }

    public async Task<ResearchSession?> GetSessionAsync(Guid sessionId)
    {
        _logger.LogDebug("Retrieving research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);

        if (session == null)
        {
            _logger.LogWarning("Research session {SessionId} not found", sessionId);
        }

        return session;
    }

    public async Task<IEnumerable<ResearchSession>> GetUserSessionsAsync(string userId)
    {
        _logger.LogDebug("Retrieving research sessions for user {UserId}", userId);

        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        var sessions = await _researchRepository.GetUserSessionsAsync(userId);

        _logger.LogDebug("Found {SessionCount} research sessions for user {UserId}", 
            sessions.Count, userId);

        return sessions;
    }

    public async Task<ResearchSession> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request)
    {
        _logger.LogInformation("Updating research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found", nameof(sessionId));
        }

        // Update properties from request
        if (!string.IsNullOrEmpty(request.Title))
            session.Title = request.Title;
        
        if (!string.IsNullOrEmpty(request.Description))
            session.Description = request.Description;

        session.UpdatedAt = DateTime.UtcNow;

        var updatedSession = await _researchRepository.UpdateSessionAsync(session);

        _logger.LogInformation("Research session {SessionId} updated successfully", sessionId);

        return updatedSession;
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        _logger.LogInformation("Deleting research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Attempted to delete non-existent session {SessionId}", sessionId);
            return false;
        }

        await _researchRepository.DeleteSessionAsync(sessionId);

        _logger.LogInformation("Research session {SessionId} deleted successfully", sessionId);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid sessionId, UpdateStatusRequest request)
    {
        _logger.LogInformation("Updating status of research session {SessionId} to {Status}", 
            sessionId, request.Status);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found for status update", sessionId);
            return false;
        }

        if (Enum.TryParse<ResearchStatus>(request.Status, true, out var researchStatus))
        {
            session.Status = researchStatus;
        }
        else
        {
            _logger.LogError("Invalid status: {Status} for session {SessionId}", request.Status, sessionId);
            return false;
        }

        session.UpdatedAt = DateTime.UtcNow;

        await _researchRepository.UpdateSessionAsync(session);

        _logger.LogInformation("Research session {SessionId} status updated to {Status} successfully", 
            sessionId, request.Status);

        return true;
    }

    // Backward compatibility methods
    public async Task<ResearchSession> AddInsightToSessionAsync(Guid sessionId, ResearchInsight insight)
    {
        _logger.LogInformation("Adding insight to research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found", nameof(sessionId));
        }

        insight.Id = Guid.NewGuid();
        insight.ResearchSessionId = sessionId;
        insight.CreatedAt = DateTime.UtcNow;

        session.ResearchInsights.Add(insight);
        session.UpdatedAt = DateTime.UtcNow;

        var updatedSession = await _researchRepository.UpdateSessionAsync(session);

        _logger.LogInformation("Insight added to research session {SessionId} successfully", sessionId);

        return updatedSession;
    }

    public async Task<ResearchSession> AddOptionToSessionAsync(Guid sessionId, ResearchOption option)
    {
        _logger.LogInformation("Adding option to research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found", nameof(sessionId));
        }

        option.Id = Guid.NewGuid();
        option.ResearchSessionId = sessionId;
        option.CreatedAt = DateTime.UtcNow;

        session.ResearchOptions.Add(option);
        session.UpdatedAt = DateTime.UtcNow;

        var updatedSession = await _researchRepository.UpdateSessionAsync(session);

        _logger.LogInformation("Option added to research session {SessionId} successfully", sessionId);

        return updatedSession;
    }

    public async Task<List<ResearchInsight>> GetSessionInsightsAsync(Guid sessionId)
    {
        _logger.LogDebug("Retrieving insights for research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found", nameof(sessionId));
        }

        return session.ResearchInsights.ToList();
    }

    public async Task<List<ResearchOption>> GetSessionOptionsAsync(Guid sessionId)
    {
        _logger.LogDebug("Retrieving options for research session {SessionId}", sessionId);

        var session = await _researchRepository.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Session not found", nameof(sessionId));
        }

        return session.ResearchOptions.ToList();
    }
}