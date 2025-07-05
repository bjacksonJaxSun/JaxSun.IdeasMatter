using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CompetitiveAnalysisController : ControllerBase
{
    private readonly ICompetitiveAnalysisService _competitiveAnalysisService;
    private readonly ILogger<CompetitiveAnalysisController> _logger;

    public CompetitiveAnalysisController(
        ICompetitiveAnalysisService competitiveAnalysisService,
        ILogger<CompetitiveAnalysisController> logger)
    {
        _competitiveAnalysisService = competitiveAnalysisService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<CompetitiveAnalysisResult>> AnalyzeCompetitorsAsync(
        [FromBody] CompetitiveAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting competitive analysis for idea: {IdeaDescription}", 
                request.IdeaDescription);

            var result = await _competitiveAnalysisService.AnalyzeCompetitorsAsync(
                request.IdeaDescription,
                request.TargetMarket,
                request.Parameters);

            _logger.LogInformation("Competitive analysis completed successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for competitive analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during competitive analysis");
            return StatusCode(500, new { error = "An error occurred during competitive analysis" });
        }
    }

    [HttpPost("market-positioning")]
    public async Task<ActionResult<MarketPositioningAnalysis>> AnalyzeMarketPositioningAsync(
        [FromBody] MarketPositioningRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing market positioning for: {IdeaDescription}", 
                request.IdeaDescription);

            var positioning = await _competitiveAnalysisService.AnalyzeMarketPositioningAsync(
                request.IdeaDescription,
                request.CompetitorProfiles,
                request.Parameters);

            return Ok(positioning);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for market positioning analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during market positioning analysis");
            return StatusCode(500, new { error = "An error occurred during positioning analysis" });
        }
    }

    [HttpPost("differentiation-opportunities")]
    public async Task<ActionResult<List<DifferentiationOpportunity>>> IdentifyDifferentiationOpportunitiesAsync(
        [FromBody] DifferentiationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Identifying differentiation opportunities");

            var opportunities = await _competitiveAnalysisService.IdentifyDifferentiationOpportunitiesAsync(
                request.CompetitiveAnalysis,
                request.Parameters);

            return Ok(opportunities);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid competitive analysis data for differentiation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying differentiation opportunities");
            return StatusCode(500, new { error = "An error occurred while identifying opportunities" });
        }
    }

    [HttpPost("competitive-insights")]
    public async Task<ActionResult<CompetitiveInsightResult>> GenerateCompetitiveInsightsAsync(
        [FromBody] CompetitiveInsightRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating competitive insights");

            var insights = await _competitiveAnalysisService.GenerateCompetitiveInsightsAsync(
                request.CompetitiveAnalysis,
                request.Parameters);

            return Ok(insights);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid competitive analysis for insights generation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating competitive insights");
            return StatusCode(500, new { error = "An error occurred while generating insights" });
        }
    }
}

public class CompetitiveAnalysisRequest
{
    public string IdeaDescription { get; set; } = string.Empty;
    public string TargetMarket { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class MarketPositioningRequest
{
    public string IdeaDescription { get; set; } = string.Empty;
    public List<CompetitorProfile> CompetitorProfiles { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
}

public class DifferentiationRequest
{
    public CompetitiveAnalysisResult CompetitiveAnalysis { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CompetitiveInsightRequest
{
    public CompetitiveAnalysisResult CompetitiveAnalysis { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
}