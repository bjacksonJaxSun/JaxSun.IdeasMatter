using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Interfaces;

public interface IResearchRepository : IRepository<Research>
{
    Task<IEnumerable<Research>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Research>> GetByStatusAsync(ResearchStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Research>> GetByUserIdAndStatusAsync(string userId, ResearchStatus status, CancellationToken cancellationToken = default);
    Task<Research?> GetWithMarketAnalysisAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Research>> GetRecentResearchesAsync(string userId, int take = 10, CancellationToken cancellationToken = default);
    Task<int> GetActiveResearchCountAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCostForUserAsync(string userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    
    // Research Session methods
    Task<ResearchSession> CreateSessionAsync(ResearchSession session);
    Task<ResearchSession?> GetSessionAsync(Guid sessionId);
    Task<List<ResearchSession>> GetUserSessionsAsync(string userId);
    Task<ResearchSession> UpdateSessionAsync(ResearchSession session);
    Task DeleteSessionAsync(Guid sessionId);
}