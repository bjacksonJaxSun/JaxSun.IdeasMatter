using Jackson.Ideas.Core.Configuration;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Jackson.Ideas.Application.Services;

/// <summary>
/// Demo-mode aware market analysis service implementing UX Blueprint principles.
/// Provides realistic market analysis data with confidence visualization and 
/// multi-tier information architecture.
/// </summary>
public class DemoModeMarketAnalysisService : IMarketAnalysisService
{
    private readonly IMarketAnalysisService _realService;
    private readonly MockDataService _mockDataService;
    private readonly DemoModeOptions _demoOptions;

    public DemoModeMarketAnalysisService(
        IMarketAnalysisService realService,
        MockDataService mockDataService,
        IOptions<DemoModeOptions> demoOptions)
    {
        _realService = realService;
        _mockDataService = mockDataService;
        _demoOptions = demoOptions.Value;
    }

    public async Task<ComprehensiveMarketAnalysisDto> GenerateComprehensiveMarketAnalysisAsync(
        int sessionId, 
        string ideaTitle, 
        string ideaDescription, 
        CancellationToken cancellationToken = default)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Simulate realistic processing time per UX Blueprint
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }

            // Return comprehensive mock analysis with confidence indicators
            var industry = DetectIndustryFromDescription(ideaDescription);
            return _mockDataService.GetMockMarketAnalysis(industry);
        }

        return await _realService.GenerateComprehensiveMarketAnalysisAsync(sessionId, ideaTitle, ideaDescription, cancellationToken);
    }

    public async Task<MarketSizingAnalysisDto> CalculateMarketSizingAsync(
        int sessionId, 
        string geographicScope = "global", 
        List<string>? targetSegments = null, 
        CancellationToken cancellationToken = default)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            var industry = "technology"; // Default for demo
            var analysis = _mockDataService.GetMockMarketAnalysis(industry);
            
            return new MarketSizingAnalysisDto
            {
                Industry = industry,
                TAM = analysis.MarketSizing.TAM,
                SAM = analysis.MarketSizing.SAM,
                SOM = analysis.MarketSizing.SOM,
                Currency = "USD",
                Year = DateTime.UtcNow.Year,
                GrowthProjections = GenerateGrowthProjections(analysis.MarketSizing),
                ConfidenceLevel = analysis.ConfidenceScore,
                Methodology = "Bottom-up analysis combining industry reports, competitor data, and market surveys"
            };
        }

        return await _realService.CalculateMarketSizingAsync(sessionId, geographicScope, targetSegments, cancellationToken);
    }

    public async Task<CompetitiveLandscapeDto> AnalyzeCompetitiveLandscapeAsync(
        int sessionId, 
        CancellationToken cancellationToken = default)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            var industry = "technology";
            var analysis = _mockDataService.GetMockMarketAnalysis(industry);
            return analysis.CompetitiveLandscape;
        }

        return await _realService.AnalyzeCompetitiveLandscapeAsync(sessionId, cancellationToken);
    }

    public async Task<CompetitorAnalysisDto> ResearchCompetitorAsync(
        int marketAnalysisId, 
        string competitorName, 
        string researchDepth = "standard", 
        CancellationToken cancellationToken = default)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            var industry = "technology";
            var analysis = _mockDataService.GetMockMarketAnalysis(industry);
            return analysis.CompetitiveLandscape.DirectCompetitors.FirstOrDefault() 
                ?? new CompetitorAnalysisDto { Name = competitorName };
        }

        return await _realService.ResearchCompetitorAsync(marketAnalysisId, competitorName, researchDepth, cancellationToken);
    }

    public async Task<MarketAnalysisDto> ConductMarketAnalysisAsync(
        Guid researchId, 
        string ideaTitle, 
        string ideaDescription, 
        CancellationToken cancellationToken = default)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            var industry = DetectIndustryFromDescription(ideaDescription);
            var comprehensive = _mockDataService.GetMockMarketAnalysis(industry);
            
            // Convert to basic analysis for simplified view
            return new MarketAnalysisDto
            {
                Industry = industry,
                MarketSize = comprehensive.MarketSizing.TAM.ToString("C0"),
                GrowthRate = "12.5% CAGR",
                KeyTrends = comprehensive.MarketTrends.Take(3).Select(t => t.Description).ToList(),
                MainCompetitors = comprehensive.CompetitiveLandscape.DirectCompetitors.Take(5).Select(c => c.Name).ToList(),
                Opportunities = comprehensive.MarketOpportunities.Take(3).Select(o => o.Description).ToList(),
                ConfidenceScore = comprehensive.ConfidenceScore
            };
        }

        return await _realService.ConductMarketAnalysisAsync(researchId, ideaTitle, ideaDescription, cancellationToken);
    }

    public async Task<MarketAnalysisDto?> GetMarketAnalysisAsync(Guid analysisId, CancellationToken cancellationToken = default)
    {
        return await _realService.GetMarketAnalysisAsync(analysisId, cancellationToken);
    }

    public async Task<IEnumerable<MarketAnalysisDto>> GetMarketAnalysesByResearchAsync(Guid researchId, CancellationToken cancellationToken = default)
    {
        return await _realService.GetMarketAnalysesByResearchAsync(researchId, cancellationToken);
    }

    public async Task<MarketAnalysisDto> UpdateMarketAnalysisAsync(Guid analysisId, MarketAnalysisDto updateDto, CancellationToken cancellationToken = default)
    {
        return await _realService.UpdateMarketAnalysisAsync(analysisId, updateDto, cancellationToken);
    }

    public async Task DeleteMarketAnalysisAsync(Guid analysisId, CancellationToken cancellationToken = default)
    {
        await _realService.DeleteMarketAnalysisAsync(analysisId, cancellationToken);
    }

    // Helper methods
    private string DetectIndustryFromDescription(string description)
    {
        var industries = new Dictionary<string, string[]>
        {
            ["fintech"] = ["payment", "banking", "finance", "crypto", "investment"],
            ["ecommerce"] = ["shopping", "marketplace", "retail", "store", "commerce"],
            ["saas"] = ["software", "platform", "tool", "service", "automation"],
            ["healthtech"] = ["health", "medical", "wellness", "fitness", "therapy"]
        };

        var lowerDescription = description.ToLowerInvariant();
        
        foreach (var industry in industries)
        {
            if (industry.Value.Any(keyword => lowerDescription.Contains(keyword)))
            {
                return industry.Key;
            }
        }
        
        return "technology";
    }

    private Dictionary<int, decimal> GenerateGrowthProjections(dynamic marketSizing)
    {
        var projections = new Dictionary<int, decimal>();
        var currentYear = DateTime.UtcNow.Year;
        var baseValue = (decimal)marketSizing.SOM;
        var growthRate = 0.15m; // 15% annual growth for demo

        for (int i = 0; i < 5; i++)
        {
            projections[currentYear + i] = baseValue * (decimal)Math.Pow(1 + (double)growthRate, i);
        }

        return projections;
    }
}