using Jackson.Ideas.Core.DTOs.Research;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface ICompetitiveAnalysisService
{
    Task<CompetitiveAnalysisResult> AnalyzeCompetitorsAsync(
        string ideaDescription,
        string targetMarket,
        Dictionary<string, object>? parameters = null);
    
    Task<CompetitorProfile> AnalyzeSpecificCompetitorAsync(
        string competitorName,
        string ideaDescription,
        Dictionary<string, object>? parameters = null);
    
    Task<MarketPositioningAnalysis> GeneratePositioningAnalysisAsync(
        string ideaDescription,
        List<CompetitorProfile> competitors,
        Dictionary<string, object>? parameters = null);
    
    Task<MarketPositioningAnalysis> AnalyzeMarketPositioningAsync(
        string ideaDescription,
        List<CompetitorProfile> competitorProfiles,
        Dictionary<string, object>? parameters = null);
    
    Task<List<DifferentiationOpportunity>> IdentifyDifferentiationOpportunitiesAsync(
        CompetitiveAnalysisResult competitiveAnalysis,
        Dictionary<string, object>? parameters = null);
    
    Task<CompetitiveInsightResult> GenerateCompetitiveInsightsAsync(
        CompetitiveAnalysisResult competitiveAnalysis,
        Dictionary<string, object>? parameters = null);
    
    // Additional methods for test compatibility
    Task<CompetitiveAnalysisResult> AnalyzeCompetitiveLandscapeAsync(string ideaTitle, string ideaDescription);
    Task<object> AnalyzeMarketPositioningAsync(string ideaTitle, string ideaDescription, CompetitiveAnalysisResult competitiveAnalysis);
    Task<object> IdentifyCompetitiveThreatsAsync(string ideaTitle, string ideaDescription, string timeframe);
    Task<object> CompareWithCompetitorsAsync(string ideaTitle, string ideaDescription, string[] competitors);
    Task<object> AnalyzeBarriersToEntryAsync(string ideaTitle, string ideaDescription, string marketContext);
}