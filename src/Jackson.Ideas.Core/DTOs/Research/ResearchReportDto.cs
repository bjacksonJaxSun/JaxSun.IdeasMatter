using Jackson.Ideas.Core.DTOs.BusinessPlan;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;

namespace Jackson.Ideas.Core.DTOs.Research;

public class ResearchReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public MarketAnalysisDto? MarketAnalysis { get; set; }
    public SwotAnalysisResult? SwotAnalysis { get; set; }
    public CompetitiveAnalysisResult? CompetitiveAnalysis { get; set; }
    public CustomerSegmentationResult? CustomerSegmentation { get; set; }
    public BusinessPlanDto? BusinessPlan { get; set; }
    public List<CompetitorAnalysisDto> Competitors { get; set; } = new();
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}