using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.DTOs.Research;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface IResearchSessionService
{
    Task<ResearchSession> CreateSessionAsync(CreateSessionRequest request);
    Task<ResearchSession?> GetSessionAsync(Guid sessionId);
    Task<IEnumerable<ResearchSession>> GetUserSessionsAsync(string userId);
    Task<ResearchSession> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> UpdateStatusAsync(Guid sessionId, UpdateStatusRequest request);
    
    // Additional methods for backward compatibility
    Task<ResearchSession> AddInsightToSessionAsync(Guid sessionId, ResearchInsight insight);
    Task<ResearchSession> AddOptionToSessionAsync(Guid sessionId, ResearchOption option);
    Task<List<ResearchInsight>> GetSessionInsightsAsync(Guid sessionId);
    Task<List<ResearchOption>> GetSessionOptionsAsync(Guid sessionId);
}