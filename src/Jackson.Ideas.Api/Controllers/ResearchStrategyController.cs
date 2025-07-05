using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ResearchStrategyController : ControllerBase
{
    private readonly IResearchStrategyService _researchStrategyService;
    private readonly ILogger<ResearchStrategyController> _logger;

    public ResearchStrategyController(
        IResearchStrategyService researchStrategyService,
        ILogger<ResearchStrategyController> logger)
    {
        _researchStrategyService = researchStrategyService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ResearchStrategyResponse>> AnalyzeIdeaAsync(
        [FromBody] ResearchStrategyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting research strategy analysis for idea: {IdeaDescription}", 
                request.IdeaDescription);

            var result = await _researchStrategyService.AnalyzeIdeaAsync(
                request.IdeaDescription,
                request.UserGoals,
                request.Parameters,
                cancellationToken);

            _logger.LogInformation("Research strategy analysis completed successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for research strategy analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during research strategy analysis");
            return StatusCode(500, new { error = "An error occurred during analysis" });
        }
    }

    [HttpPost("suggest-approaches")]
    public async Task<ActionResult<List<string>>> SuggestResearchApproachesAsync(
        [FromBody] string ideaDescription,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suggesting research approaches for idea: {IdeaDescription}", ideaDescription);

            var approaches = await _researchStrategyService.SuggestResearchApproachesAsync(
                ideaDescription, 
                cancellationToken);

            return Ok(approaches);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid idea description for research approach suggestions");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting research approaches");
            return StatusCode(500, new { error = "An error occurred while suggesting approaches" });
        }
    }

    [HttpPost("validate-approach")]
    public async Task<ActionResult<bool>> ValidateResearchApproachAsync(
        [FromBody] ValidateApproachRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating research approach: {Approach}", request.Approach);

            var isValid = await _researchStrategyService.ValidateResearchApproachAsync(
                request.IdeaDescription,
                request.Approach,
                request.Parameters,
                cancellationToken);

            return Ok(new { isValid, approach = request.Approach });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for approach validation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating research approach");
            return StatusCode(500, new { error = "An error occurred during validation" });
        }
    }

    [HttpPost("track-progress")]
    public async Task<ActionResult<AnalysisProgressUpdate>> TrackAnalysisProgressAsync(
        [FromBody] TrackProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Tracking analysis progress for session: {SessionId}", request.SessionId);

            var progress = await _researchStrategyService.TrackAnalysisProgressAsync(
                request.SessionId,
                request.Parameters,
                cancellationToken);

            return Ok(progress);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session ID for progress tracking");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking analysis progress");
            return StatusCode(500, new { error = "An error occurred while tracking progress" });
        }
    }
}

public class ValidateApproachRequest
{
    public string IdeaDescription { get; set; } = string.Empty;
    public string Approach { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class TrackProgressRequest
{
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}