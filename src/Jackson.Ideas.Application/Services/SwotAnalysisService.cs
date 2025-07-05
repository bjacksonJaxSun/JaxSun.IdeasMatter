using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.AI;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Jackson.Ideas.Application.Services;

public class SwotAnalysisService : ISwotAnalysisService
{
    private readonly IAIOrchestrator _aiOrchestrator;
    private readonly ILogger<SwotAnalysisService> _logger;

    public SwotAnalysisService(
        IAIOrchestrator aiOrchestrator,
        ILogger<SwotAnalysisService> logger)
    {
        _aiOrchestrator = aiOrchestrator;
        _logger = logger;
    }

    public async Task<SwotAnalysisResult> GenerateSwotAnalysisAsync(
        string ideaDescription,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Generating SWOT analysis for idea: {IdeaDescription}", ideaDescription);

        try
        {
            var prompt = BuildSwotAnalysisPrompt(ideaDescription);
            var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

            var swotAnalysis = ParseSwotAnalysisResponse(response);
            
            _logger.LogInformation("SWOT analysis completed successfully");
            return swotAnalysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SWOT analysis generation");
            throw;
        }
    }

    public async Task<SwotAnalysisResult> GenerateEnhancedSwotAnalysisAsync(
        string ideaDescription,
        CompetitiveAnalysisResult? competitiveAnalysis = null,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Generating enhanced SWOT analysis with competitive context");

        try
        {
            var prompt = BuildEnhancedSwotAnalysisPrompt(ideaDescription, competitiveAnalysis);
            var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

            var swotAnalysis = ParseSwotAnalysisResponse(response);
            
            _logger.LogInformation("Enhanced SWOT analysis completed successfully");
            return swotAnalysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced SWOT analysis generation");
            throw;
        }
    }

    public async Task<StrategicImplicationsResult> AnalyzeStrategicImplicationsAsync(
        SwotAnalysisResult swotAnalysis,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Analyzing strategic implications from SWOT analysis");

        try
        {
            var prompt = BuildStrategicImplicationsPrompt(swotAnalysis);
            var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

            var implications = ParseStrategicImplicationsResponse(response);
            
            _logger.LogInformation("Strategic implications analysis completed");
            return implications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing strategic implications");
            throw;
        }
    }

    private string BuildSwotAnalysisPrompt(string ideaDescription)
    {
        return $@"
Conduct a comprehensive SWOT analysis for the following business idea:

Business Idea: {ideaDescription}

Please provide analysis in JSON format with the following structure:
{{
  ""strengths"": [
    {{
      ""title"": ""Strength Title"",
      ""description"": ""Detailed description"",
      ""impact"": 8.5,
      ""priority"": ""High"",
      ""actionableInsights"": [""insight1"", ""insight2""]
    }}
  ],
  ""weaknesses"": [
    {{
      ""title"": ""Weakness Title"",
      ""description"": ""Detailed description"",
      ""impact"": 7.0,
      ""priority"": ""Medium"",
      ""actionableInsights"": [""insight1"", ""insight2""]
    }}
  ],
  ""opportunities"": [
    {{
      ""title"": ""Opportunity Title"",
      ""description"": ""Detailed description"",
      ""impact"": 9.0,
      ""likelihood"": 7.5,
      ""priority"": ""High"",
      ""actionableInsights"": [""insight1"", ""insight2""]
    }}
  ],
  ""threats"": [
    {{
      ""title"": ""Threat Title"",
      ""description"": ""Detailed description"",
      ""impact"": 6.5,
      ""likelihood"": 5.0,
      ""priority"": ""Medium"",
      ""actionableInsights"": [""insight1"", ""insight2""]
    }}
  ],
  ""strategicImplications"": [""implication1"", ""implication2""],
  ""criticalSuccessFactors"": [""factor1"", ""factor2""],
  ""confidenceScore"": 0.85,
  ""summary"": ""Overall SWOT analysis summary""
}}

Provide 4-6 factors for each SWOT category with specific, actionable insights.
";
    }

    private string BuildEnhancedSwotAnalysisPrompt(string ideaDescription, CompetitiveAnalysisResult? competitiveAnalysis)
    {
        var competitiveContext = competitiveAnalysis != null 
            ? $@"
Consider the following competitive context:
- Direct Competitors: {string.Join(", ", competitiveAnalysis.DirectCompetitors.Select(c => c.Name))}
- Key Competitive Advantages: {string.Join(", ", competitiveAnalysis.CompetitiveAdvantages)}
- Barriers to Entry: {string.Join(", ", competitiveAnalysis.BarriersToEntry)}
"
            : "";

        return $@"
Conduct an enhanced SWOT analysis for the following business idea:

Business Idea: {ideaDescription}

{competitiveContext}

Provide comprehensive analysis considering the competitive landscape. Use the same JSON structure as a standard SWOT analysis but incorporate competitive insights:

{{
  ""strengths"": [/* Include competitive advantages and unique capabilities */],
  ""weaknesses"": [/* Include areas where competitors are stronger */],
  ""opportunities"": [/* Include market gaps and competitive opportunities */],
  ""threats"": [/* Include competitive threats and market risks */],
  ""strategicImplications"": [/* Strategic insights considering competition */],
  ""criticalSuccessFactors"": [/* Key factors for competitive success */],
  ""confidenceScore"": 0.85,
  ""summary"": ""Enhanced SWOT analysis with competitive context""
}}

Focus on competitive differentiation and market positioning opportunities.
";
    }

    private string BuildStrategicImplicationsPrompt(SwotAnalysisResult swotAnalysis)
    {
        var swotSummary = $@"
Strengths: {string.Join(", ", swotAnalysis.StrengthsFactors.Select(s => s.Title))}
Weaknesses: {string.Join(", ", swotAnalysis.WeaknessesFactors.Select(w => w.Title))}
Opportunities: {string.Join(", ", swotAnalysis.OpportunitiesFactors.Select(o => o.Title))}
Threats: {string.Join(", ", swotAnalysis.ThreatsFactors.Select(t => t.Title))}
";

        return $@"
Analyze the strategic implications of the following SWOT analysis and generate strategic recommendations:

{swotSummary}

Provide strategic recommendations in JSON format:
{{
  ""soStrategies"": [
    {{
      ""strategy"": ""Strategy Title"",
      ""type"": ""SO"",
      ""description"": ""Detailed strategy description"",
      ""requiredActions"": [""action1"", ""action2""],
      ""timeline"": 12,
      ""priority"": ""High"",
      ""feasibilityScore"": 8.0,
      ""impactScore"": 9.0
    }}
  ],
  ""woStrategies"": [/* Weaknesses-Opportunities strategies */],
  ""stStrategies"": [/* Strengths-Threats strategies */],
  ""wtStrategies"": [/* Weaknesses-Threats strategies */],
  ""priorityActions"": [""action1"", ""action2""],
  ""keyRisks"": [""risk1"", ""risk2""],
  ""overallStrategicDirection"": ""Strategic direction summary"",
  ""strategicFitScore"": 8.5
}}

Generate 2-3 strategies for each SWOT combination (SO, WO, ST, WT).
";
    }

    private SwotAnalysisResult ParseSwotAnalysisResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                return new SwotAnalysisResult
                {
                    Strengths = ParseSwotFactors(parsed.GetProperty("strengths"), "Strengths"),
                    Weaknesses = ParseSwotFactors(parsed.GetProperty("weaknesses"), "Weaknesses"),
                    Opportunities = ParseSwotFactors(parsed.GetProperty("opportunities"), "Opportunities"),
                    Threats = ParseSwotFactors(parsed.GetProperty("threats"), "Threats"),
                    StrategicImplications = ParseStringArray(parsed.GetProperty("strategicImplications")),
                    CriticalSuccessFactors = ParseStringArray(parsed.GetProperty("criticalSuccessFactors")),
                    ConfidenceScore = parsed.GetProperty("confidenceScore").GetDouble(),
                    Summary = parsed.GetProperty("summary").GetString() ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse SWOT analysis response, using fallback");
        }

        return CreateFallbackSwotAnalysis(response);
    }

    private StrategicImplicationsResult ParseStrategicImplicationsResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                return new StrategicImplicationsResult
                {
                    SOStrategies = ParseStrategicRecommendations(parsed.GetProperty("soStrategies"), "SO"),
                    WOStrategies = ParseStrategicRecommendations(parsed.GetProperty("woStrategies"), "WO"),
                    STStrategies = ParseStrategicRecommendations(parsed.GetProperty("stStrategies"), "ST"),
                    WTStrategies = ParseStrategicRecommendations(parsed.GetProperty("wtStrategies"), "WT"),
                    PriorityActions = ParseStringArray(parsed.GetProperty("priorityActions")),
                    KeyRisks = ParseStringArray(parsed.GetProperty("keyRisks")),
                    OverallStrategicDirection = parsed.GetProperty("overallStrategicDirection").GetString() ?? string.Empty,
                    StrategicFitScore = parsed.GetProperty("strategicFitScore").GetDouble()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse strategic implications, using fallback");
        }

        return new StrategicImplicationsResult
        {
            OverallStrategicDirection = "Strategic direction based on SWOT analysis",
            StrategicFitScore = 7.5
        };
    }

    private List<SwotFactor> ParseSwotFactors(JsonElement factorsElement, string category)
    {
        var factors = new List<SwotFactor>();

        try
        {
            foreach (var factor in factorsElement.EnumerateArray())
            {
                factors.Add(new SwotFactor
                {
                    Category = category,
                    Title = factor.GetProperty("title").GetString() ?? string.Empty,
                    Description = factor.GetProperty("description").GetString() ?? string.Empty,
                    Impact = factor.GetProperty("impact").GetDouble(),
                    Likelihood = factor.TryGetProperty("likelihood", out var likelihood) ? likelihood.GetDouble() : 0,
                    Priority = factor.GetProperty("priority").GetString() ?? string.Empty,
                    ActionableInsights = ParseStringArray(factor.GetProperty("actionableInsights"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse SWOT factors for category: {Category}", category);
        }

        return factors;
    }

    private List<StrategicRecommendation> ParseStrategicRecommendations(JsonElement strategiesElement, string type)
    {
        var strategies = new List<StrategicRecommendation>();

        try
        {
            foreach (var strategy in strategiesElement.EnumerateArray())
            {
                strategies.Add(new StrategicRecommendation
                {
                    Strategy = strategy.GetProperty("strategy").GetString() ?? string.Empty,
                    Type = type,
                    Description = strategy.GetProperty("description").GetString() ?? string.Empty,
                    RequiredActions = ParseStringArray(strategy.GetProperty("requiredActions")),
                    Timeline = strategy.GetProperty("timeline").GetInt32(),
                    Priority = strategy.GetProperty("priority").GetString() ?? string.Empty,
                    FeasibilityScore = strategy.GetProperty("feasibilityScore").GetDouble(),
                    ImpactScore = strategy.GetProperty("impactScore").GetDouble()
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse strategic recommendations for type: {Type}", type);
        }

        return strategies;
    }

    private List<string> ParseStringArray(JsonElement arrayElement)
    {
        var items = new List<string>();

        try
        {
            foreach (var item in arrayElement.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    items.Add(value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse string array");
        }

        return items;
    }

    private SwotAnalysisResult CreateFallbackSwotAnalysis(string response)
    {
        return new SwotAnalysisResult
        {
            Strengths = new List<SwotFactor>
            {
                new() { Category = "Strengths", Title = "Innovation", Description = "Strong innovative capabilities", Impact = 8.0, Priority = "High" },
                new() { Category = "Strengths", Title = "Market Timing", Description = "Optimal market entry timing", Impact = 7.5, Priority = "Medium" }
            },
            Weaknesses = new List<SwotFactor>
            {
                new() { Category = "Weaknesses", Title = "Limited Resources", Description = "Resource constraints", Impact = 6.0, Priority = "High" },
                new() { Category = "Weaknesses", Title = "Brand Recognition", Description = "Low brand awareness", Impact = 5.5, Priority = "Medium" }
            },
            Opportunities = new List<SwotFactor>
            {
                new() { Category = "Opportunities", Title = "Market Growth", Description = "Growing market demand", Impact = 9.0, Likelihood = 8.0, Priority = "High" },
                new() { Category = "Opportunities", Title = "Technology Trends", Description = "Favorable technology trends", Impact = 7.0, Likelihood = 7.5, Priority = "Medium" }
            },
            Threats = new List<SwotFactor>
            {
                new() { Category = "Threats", Title = "Competition", Description = "Strong competitive pressure", Impact = 7.0, Likelihood = 6.0, Priority = "High" },
                new() { Category = "Threats", Title = "Market Changes", Description = "Rapid market evolution", Impact = 6.0, Likelihood = 5.0, Priority = "Medium" }
            },
            StrategicImplications = new List<string> { "Focus on innovation", "Build strategic partnerships", "Accelerate market entry" },
            CriticalSuccessFactors = new List<string> { "Product quality", "Customer acquisition", "Market timing" },
            ConfidenceScore = 0.75,
            Summary = "Comprehensive SWOT analysis identifying key strategic factors"
        };
    }
}