using Jackson.Ideas.Core.DTOs.MarketAnalysis;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface IMarketAnalysisService
{
    Task<ComprehensiveMarketAnalysisDto> GenerateComprehensiveMarketAnalysisAsync(
        int sessionId, 
        string ideaTitle, 
        string ideaDescription, 
        CancellationToken cancellationToken = default);
        
    Task<MarketSizingAnalysisDto> CalculateMarketSizingAsync(
        int sessionId, 
        string geographicScope = "global",
        List<string>? targetSegments = null, 
        CancellationToken cancellationToken = default);
        
    Task<CompetitiveLandscapeDto> AnalyzeCompetitiveLandscapeAsync(
        int sessionId, 
        CancellationToken cancellationToken = default);
        
    Task<CompetitorAnalysisDto> ResearchCompetitorAsync(
        int marketAnalysisId, 
        string competitorName, 
        string researchDepth = "standard", 
        CancellationToken cancellationToken = default);
        
    // Additional methods needed by tests (using Guid for compatibility with test expectations)
    Task<MarketAnalysisDto> ConductMarketAnalysisAsync(Guid researchId, string ideaTitle, string ideaDescription, CancellationToken cancellationToken = default);
    Task<MarketAnalysisDto?> GetMarketAnalysisAsync(Guid analysisId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MarketAnalysisDto>> GetMarketAnalysesByResearchAsync(Guid researchId, CancellationToken cancellationToken = default);
    Task<MarketAnalysisDto> UpdateMarketAnalysisAsync(Guid analysisId, MarketAnalysisDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteMarketAnalysisAsync(Guid analysisId, CancellationToken cancellationToken = default);
}