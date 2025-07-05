using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Jackson.Ideas.Infrastructure.Repositories;

public class ResearchRepository : Repository<Research>, IResearchRepository
{
    public ResearchRepository(JacksonIdeasDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Research>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Research>> GetByStatusAsync(ResearchStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Status == status && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Research>> GetByUserIdAndStatusAsync(string userId, ResearchStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.UserId == userId && r.Status == status && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Research?> GetWithMarketAnalysisAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.MarketAnalyses)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Research>> GetRecentResearchesAsync(string userId, int take = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveResearchCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .CountAsync(r => r.UserId == userId && 
                           (r.Status == ResearchStatus.Pending || 
                            r.Status == ResearchStatus.InProgress || 
                            r.Status == ResearchStatus.Analyzing) && 
                           !r.IsDeleted, 
                       cancellationToken);
    }

    public async Task<decimal> GetTotalCostForUserAsync(string userId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.UserId == userId && !r.IsDeleted);
        
        if (from.HasValue)
            query = query.Where(r => r.CreatedAt >= from.Value);
            
        if (to.HasValue)
            query = query.Where(r => r.CreatedAt <= to.Value);
        
        return await query.SumAsync(r => r.EstimatedCost, cancellationToken);
    }

    public async Task<ResearchSession> CreateSessionAsync(ResearchSession session)
    {
        _context.ResearchSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<ResearchSession?> GetSessionAsync(Guid sessionId)
    {
        return await _context.ResearchSessions
            .Include(s => s.ResearchInsights)
            .Include(s => s.ResearchOptions)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<List<ResearchSession>> GetUserSessionsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new List<ResearchSession>();
        }

        return await _context.ResearchSessions
            .Where(s => s.UserId == userId)
            .Include(s => s.ResearchInsights)
            .Include(s => s.ResearchOptions)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ResearchSession> UpdateSessionAsync(ResearchSession session)
    {
        _context.ResearchSessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        if (session != null)
        {
            _context.ResearchSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }
}