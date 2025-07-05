using Jackson.Ideas.Core.DTOs.Research;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface ISwotAnalysisService
{
    Task<SwotAnalysisResult> GenerateSwotAnalysisAsync(
        string ideaDescription,
        Dictionary<string, object>? parameters = null);
    
    Task<SwotAnalysisResult> GenerateEnhancedSwotAnalysisAsync(
        string ideaDescription,
        CompetitiveAnalysisResult? competitiveAnalysis = null,
        Dictionary<string, object>? parameters = null);
    
    Task<StrategicImplicationsResult> AnalyzeStrategicImplicationsAsync(
        SwotAnalysisResult swotAnalysis,
        Dictionary<string, object>? parameters = null);
}