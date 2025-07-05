using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CustomerSegmentationController : ControllerBase
{
    private readonly ICustomerSegmentationService _customerSegmentationService;
    private readonly ILogger<CustomerSegmentationController> _logger;

    public CustomerSegmentationController(
        ICustomerSegmentationService customerSegmentationService,
        ILogger<CustomerSegmentationController> logger)
    {
        _customerSegmentationService = customerSegmentationService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<CustomerSegmentationResult>> AnalyzeCustomerSegmentsAsync(
        [FromBody] CustomerSegmentationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting customer segmentation analysis for idea: {IdeaDescription}", 
                request.IdeaDescription);

            var result = await _customerSegmentationService.AnalyzeCustomerSegmentsAsync(
                request.IdeaDescription,
                request.Parameters);

            _logger.LogInformation("Customer segmentation analysis completed successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for customer segmentation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer segmentation analysis");
            return StatusCode(500, new { error = "An error occurred during customer segmentation" });
        }
    }

    [HttpPost("persona")]
    public async Task<ActionResult<CustomerPersona>> CreateCustomerPersonaAsync(
        [FromBody] CustomerPersonaRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating customer persona for segment: {SegmentName}", 
                request.SegmentName);

            var persona = await _customerSegmentationService.CreateCustomerPersonaAsync(
                request.SegmentName,
                request.IdeaDescription,
                request.Parameters);

            _logger.LogInformation("Customer persona created successfully for segment: {SegmentName}", 
                request.SegmentName);
            return Ok(persona);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for persona creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer persona");
            return StatusCode(500, new { error = "An error occurred during persona creation" });
        }
    }

    [HttpPost("journey")]
    public async Task<ActionResult<List<CustomerJourney>>> AnalyzeCustomerJourneysAsync(
        [FromBody] CustomerJourneyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing customer journeys for {SegmentCount} segments", 
                request.Segments.Count);

            var journeys = await _customerSegmentationService.AnalyzeCustomerJourneysAsync(
                request.Segments,
                request.IdeaDescription,
                request.Parameters);

            _logger.LogInformation("Customer journey analysis completed successfully");
            return Ok(journeys);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for journey analysis");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing customer journeys");
            return StatusCode(500, new { error = "An error occurred during journey analysis" });
        }
    }

    [HttpPost("insights")]
    public async Task<ActionResult<CustomerInsightResult>> GenerateCustomerInsightsAsync(
        [FromBody] CustomerInsightRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating customer insights from segmentation analysis");

            var insights = await _customerSegmentationService.GenerateCustomerInsightsAsync(
                request.Segmentation,
                request.Parameters);

            _logger.LogInformation("Customer insights generation completed successfully");
            return Ok(insights);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid segmentation data for insights generation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer insights");
            return StatusCode(500, new { error = "An error occurred during insights generation" });
        }
    }

    [HttpGet("segments/{segmentName}/persona")]
    public async Task<ActionResult<CustomerPersona>> GetPersonaForSegmentAsync(
        string segmentName,
        [FromQuery] string ideaDescription,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(ideaDescription))
            {
                return BadRequest(new { error = "Idea description is required" });
            }

            _logger.LogInformation("Getting persona for segment: {SegmentName}", segmentName);

            var persona = await _customerSegmentationService.CreateCustomerPersonaAsync(
                segmentName,
                ideaDescription);

            return Ok(persona);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for persona retrieval");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving persona for segment: {SegmentName}", segmentName);
            return StatusCode(500, new { error = "An error occurred while retrieving persona" });
        }
    }
}

public class CustomerSegmentationRequest
{
    public string IdeaDescription { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CustomerPersonaRequest
{
    public string SegmentName { get; set; } = string.Empty;
    public string IdeaDescription { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CustomerJourneyRequest
{
    public List<CustomerSegment> Segments { get; set; } = new();
    public string IdeaDescription { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CustomerInsightRequest
{
    public CustomerSegmentationResult Segmentation { get; set; } = new();
    public Dictionary<string, object>? Parameters { get; set; }
}