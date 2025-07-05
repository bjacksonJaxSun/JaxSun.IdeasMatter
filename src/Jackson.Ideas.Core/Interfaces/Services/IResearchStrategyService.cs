using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface IResearchStrategyService
{
    Task<List<ResearchStrategyInfo>> GetAvailableApproachesAsync();
    
    Task<ResearchSession> InitiateResearchStrategyAsync(
        Guid sessionId, 
        string ideaTitle, 
        string ideaDescription, 
        ResearchApproach approach, 
        Dictionary<string, object>? customParameters = null);
    
    Task<ResearchSession> ExecuteResearchStrategyAsync(
        ResearchSession strategy,
        string ideaTitle,
        string ideaDescription,
        Func<Guid, string, double, Task>? progressCallback = null);
    
    Task<AnalysisProgressUpdate> GetProgressAsync(Guid strategyId);
    
    Task<List<ResearchSession>> GetSessionStrategiesAsync(Guid sessionId);
    
    Task<ResearchStrategyResponse> AnalyzeIdeaAsync(
        string ideaDescription,
        string userGoals,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);
    
    Task<List<string>> SuggestResearchApproachesAsync(
        string ideaDescription,
        CancellationToken cancellationToken = default);
    
    Task<bool> ValidateResearchApproachAsync(
        string ideaDescription,
        string approach,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);
    
    Task<AnalysisProgressUpdate> TrackAnalysisProgressAsync(
        string sessionId,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default);
}

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