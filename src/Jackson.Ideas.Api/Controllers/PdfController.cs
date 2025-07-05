using Jackson.Ideas.Core.DTOs.BusinessPlan;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<PdfController> _logger;

    public PdfController(IPdfService pdfService, ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a comprehensive research report PDF
    /// </summary>
    [HttpPost("research-report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateResearchReportAsync(
        [FromBody] GenerateResearchReportRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pdfBytes = await _pdfService.GenerateResearchReportAsync(
                request.ReportData,
                request.Options,
                cancellationToken);

            var fileName = $"{SanitizeFileName(request.ReportData.Title)}_Research_Report_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate research report PDF");
            return StatusCode(500, "An error occurred while generating the PDF report");
        }
    }

    /// <summary>
    /// Generate a SWOT analysis PDF
    /// </summary>
    [HttpPost("swot-analysis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateSwotAnalysisPdfAsync(
        [FromBody] GenerateSwotAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pdfBytes = await _pdfService.GenerateSwotAnalysisPdfAsync(
                request.SwotAnalysis,
                request.IdeaTitle,
                request.IdeaDescription,
                request.Options,
                cancellationToken);

            var fileName = $"{SanitizeFileName(request.IdeaTitle)}_SWOT_Analysis_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SWOT analysis PDF");
            return StatusCode(500, "An error occurred while generating the SWOT analysis PDF");
        }
    }

    /// <summary>
    /// Generate a market analysis PDF
    /// </summary>
    [HttpPost("market-analysis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateMarketAnalysisPdfAsync(
        [FromBody] GenerateMarketAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pdfBytes = await _pdfService.GenerateMarketAnalysisPdfAsync(
                request.MarketAnalysis,
                request.CompetitiveLandscape,
                request.Options,
                cancellationToken);

            var fileName = $"Market_Analysis_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate market analysis PDF");
            return StatusCode(500, "An error occurred while generating the market analysis PDF");
        }
    }

    /// <summary>
    /// Generate a business plan PDF
    /// </summary>
    [HttpPost("business-plan")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateBusinessPlanPdfAsync(
        [FromBody] GenerateBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pdfBytes = await _pdfService.GenerateBusinessPlanPdfAsync(
                request.BusinessPlan,
                request.Options,
                cancellationToken);

            var fileName = $"Business_Plan_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate business plan PDF");
            return StatusCode(500, "An error occurred while generating the business plan PDF");
        }
    }

    /// <summary>
    /// Generate a competitive analysis PDF
    /// </summary>
    [HttpPost("competitive-analysis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateCompetitiveAnalysisPdfAsync(
        [FromBody] GenerateCompetitiveAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pdfBytes = await _pdfService.GenerateCompetitiveAnalysisPdfAsync(
                request.CompetitiveAnalysis,
                request.IdeaTitle,
                request.IdeaDescription,
                request.Options,
                cancellationToken);

            var fileName = $"{SanitizeFileName(request.IdeaTitle)}_Competitive_Analysis_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate competitive analysis PDF");
            return StatusCode(500, "An error occurred while generating the competitive analysis PDF");
        }
    }

    /// <summary>
    /// Generate a customer segmentation PDF
    /// </summary>
    [HttpPost("customer-segmentation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateCustomerSegmentationPdfAsync(
        [FromBody] GenerateCustomerSegmentationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pdfBytes = await _pdfService.GenerateCustomerSegmentationPdfAsync(
                request.Segmentation,
                request.IdeaTitle,
                request.IdeaDescription,
                request.Options,
                cancellationToken);

            var fileName = $"{SanitizeFileName(request.IdeaTitle)}_Customer_Segmentation_{DateTime.UtcNow:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate customer segmentation PDF");
            return StatusCode(500, "An error occurred while generating the customer segmentation PDF");
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries))
            .TrimEnd('.');
    }
}

public class GenerateResearchReportRequest
{
    [Required]
    public ResearchReportDto ReportData { get; set; } = new();
    
    public PdfGenerationOptions? Options { get; set; }
}

public class GenerateSwotAnalysisRequest
{
    [Required]
    public SwotAnalysisResult SwotAnalysis { get; set; } = new();
    
    [Required]
    public string IdeaTitle { get; set; } = string.Empty;
    
    public string IdeaDescription { get; set; } = string.Empty;
    
    public PdfGenerationOptions? Options { get; set; }
}

public class GenerateMarketAnalysisRequest
{
    [Required]
    public MarketAnalysisDto MarketAnalysis { get; set; } = new();
    
    public CompetitiveLandscapeDto? CompetitiveLandscape { get; set; }
    
    public PdfGenerationOptions? Options { get; set; }
}

public class GenerateBusinessPlanRequest
{
    [Required]
    public BusinessPlanDto BusinessPlan { get; set; } = new();
    
    public PdfGenerationOptions? Options { get; set; }
}

public class GenerateCompetitiveAnalysisRequest
{
    [Required]
    public CompetitiveAnalysisResult CompetitiveAnalysis { get; set; } = new();
    
    [Required]
    public string IdeaTitle { get; set; } = string.Empty;
    
    public string IdeaDescription { get; set; } = string.Empty;
    
    public PdfGenerationOptions? Options { get; set; }
}

public class GenerateCustomerSegmentationRequest
{
    [Required]
    public CustomerSegmentationResult Segmentation { get; set; } = new();
    
    [Required]
    public string IdeaTitle { get; set; } = string.Empty;
    
    public string IdeaDescription { get; set; } = string.Empty;
    
    public PdfGenerationOptions? Options { get; set; }
}