using Jackson.Ideas.Core.DTOs.AI;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;

namespace Jackson.Ideas.Core.Interfaces.AI;

public interface IAIOrchestrator
{
    Task<BrainstormResponseDto> BrainstormAsync(string message, Dictionary<string, object>? context = null, List<Dictionary<string, object>>? insights = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, List<string>>> CategorizeInsightsAsync(string idea, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> PerformAnalysisAsync(string analysisType, string idea, List<Dictionary<string, object>>? insights = null, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    Task<FactCheckResponseDto> FactCheckAsync(string claim, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default);
    Task<List<OptionDto>> GenerateOptionsAsync(string category, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default);
    Task<List<string>> RecommendNextStepsAsync(Dictionary<string, object> sessionData, CancellationToken cancellationToken = default);
    Task<string> ProcessRequestAsync(string prompt, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    
    // Additional methods for test compatibility
    Task<SwotAnalysisResult> GenerateSwotAnalysisAsync(string ideaTitle, string ideaDescription, string marketContext, CancellationToken cancellationToken = default);
    Task<CompetitiveAnalysisResult> GenerateCompetitiveAnalysisAsync(string ideaTitle, string ideaDescription, CancellationToken cancellationToken = default);
    Task<CustomerSegmentationResult> GenerateCustomerSegmentationAsync(string ideaTitle, string ideaDescription, string marketAnalysis, CancellationToken cancellationToken = default);
    Task<BrainstormResponseDto> BrainstormStrategicOptionsAsync(string ideaTitle, string ideaDescription, string marketInsights, string swotAnalysis, string customerSegments, int optionsCount, CancellationToken cancellationToken = default);
    Task<FactCheckResponseDto> ValidateIdeaFeasibilityAsync(string ideaTitle, string ideaDescription, string marketAnalysis, CancellationToken cancellationToken = default);
    Task<MarketAnalysisDto> ConductMarketAnalysisAsync(string ideaTitle, string ideaDescription, CancellationToken cancellationToken = default);
}