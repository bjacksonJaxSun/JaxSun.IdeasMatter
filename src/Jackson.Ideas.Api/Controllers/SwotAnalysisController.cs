using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SwotAnalysisController : ControllerBase
{
    private readonly ISwotAnalysisService _swotAnalysisService;
    private readonly ILogger<SwotAnalysisController> _logger;

    public SwotAnalysisController(
        ISwotAnalysisService swotAnalysisService,
        ILogger<SwotAnalysisController> logger)
    {
        _swotAnalysisService = swotAnalysisService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<SwotAnalysisResult>> GenerateSwotAnalysisAsync(
        [FromBody] SwotAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting SWOT analysis for idea: {IdeaDescription}", 
                request.IdeaDescription);

            var result = await _swotAnalysisService.GenerateSwotAnalysisAsync(
                request.IdeaDescription,
                request.Parameters);

            _logger.LogInformation("SWOT analysis completed successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for SWOT analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SWOT analysis");
            return StatusCode(500, new { error = "An error occurred during SWOT analysis" });
        }
    }

    [HttpPost("enhanced-analyze")]
    public async Task<ActionResult<SwotAnalysisResult>> GenerateEnhancedSwotAnalysisAsync(
        [FromBody] EnhancedSwotAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting enhanced SWOT analysis with competitive context");

            var result = await _swotAnalysisService.GenerateEnhancedSwotAnalysisAsync(
                request.IdeaDescription,
                request.CompetitiveAnalysis,
                request.Parameters);

            _logger.LogInformation("Enhanced SWOT analysis completed successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for enhanced SWOT analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced SWOT analysis");
            return StatusCode(500, new { error = "An error occurred during enhanced SWOT analysis" });
        }
    }

    [HttpPost("strategic-implications")]
    public async Task<ActionResult<StrategicImplicationsResult>> AnalyzeStrategicImplicationsAsync(
        [FromBody] StrategicImplicationsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing strategic implications from SWOT analysis");

            var implications = await _swotAnalysisService.AnalyzeStrategicImplicationsAsync(
                request.SwotAnalysis,
                request.Parameters);

            _logger.LogInformation("Strategic implications analysis completed");
            return Ok(implications);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid SWOT analysis for strategic implications");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing strategic implications");
            return StatusCode(500, new { error = "An error occurred during strategic analysis" });
        }
    }

    [HttpGet("factors/{category}")]
    public async Task<ActionResult<List<SwotFactor>>> GetSwotFactorsByCategoryAsync(
        string category,
        [FromQuery] string ideaDescription,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(ideaDescription))
            {
                return BadRequest(new { error = "Idea description is required" });
            }

            var validCategories = new[] { "Strengths", "Weaknesses", "Opportunities", "Threats" };
            if (!validCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Invalid category. Must be one of: Strengths, Weaknesses, Opportunities, Threats" });
            }

            _logger.LogInformation("Getting SWOT factors for category: {Category}", category);

            var swotResult = await _swotAnalysisService.GenerateSwotAnalysisAsync(ideaDescription);
            
            var factors = category.ToLower() switch
            {
                "strengths" => swotResult.Strengths,
                "weaknesses" => swotResult.Weaknesses,
                "opportunities" => swotResult.Opportunities,
                "threats" => swotResult.Threats,
                _ => new List<SwotFactor>()
            };

            return Ok(factors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SWOT factors for category: {Category}", category);
            return StatusCode(500, new { error = "An error occurred while retrieving SWOT factors" });
        }
    }
}

public class SwotAnalysisRequest
{
    public string IdeaDescription { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class EnhancedSwotAnalysisRequest
{
    public string IdeaDescription { get; set; } = string.Empty;
    public CompetitiveAnalysisResult? CompetitiveAnalysis { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

public class StrategicImplicationsRequest
{
    public SwotAnalysisResult SwotAnalysis { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
}