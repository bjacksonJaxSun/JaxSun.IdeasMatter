using Jackson.Ideas.Core.DTOs.Research;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface ICustomerSegmentationService
{
    Task<CustomerSegmentationResult> AnalyzeCustomerSegmentsAsync(
        string ideaDescription,
        Dictionary<string, object>? parameters = null);
    
    Task<CustomerPersona> CreateCustomerPersonaAsync(
        string segmentName,
        string ideaDescription,
        Dictionary<string, object>? parameters = null);
    
    Task<List<CustomerJourney>> AnalyzeCustomerJourneysAsync(
        List<CustomerSegment> segments,
        string ideaDescription,
        Dictionary<string, object>? parameters = null);
    
    Task<CustomerInsightResult> GenerateCustomerInsightsAsync(
        CustomerSegmentationResult segmentation,
        Dictionary<string, object>? parameters = null);
}