using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Entities;

public class Research : BaseEntity
{
    [Required]
    [StringLength(500)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public ResearchStatus Status { get; set; } = ResearchStatus.Pending;
    
    [StringLength(50)]
    public string ResearchType { get; set; } = "comprehensive";
    
    [StringLength(50)]
    public string? AIProvider { get; set; }
    
    public bool IncludeMarketAnalysis { get; set; } = true;
    
    public bool IncludeSwot { get; set; } = true;
    
    public bool IncludeBusinessPlan { get; set; } = true;
    
    // JSON fields stored as strings and converted using JsonDocument
    public string? MarketAnalysisJson { get; set; }
    
    public string? SwotAnalysisJson { get; set; }
    
    public string? BusinessPlanJson { get; set; }
    
    public string? CompetitorsJson { get; set; }
    
    public string? ProviderInsightsJson { get; set; }
    
    public int ProgressPercentage { get; set; } = 0;
    
    [StringLength(500)]
    public string? CurrentStep { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public int TokensUsed { get; set; } = 0;
    
    public decimal EstimatedCost { get; set; } = 0;
    
    // Foreign key
    [Required]
    [StringLength(450)]  // Standard length for ASP.NET Identity user IDs
    public string UserId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    
    public virtual ICollection<MarketAnalysis> MarketAnalyses { get; set; } = new List<MarketAnalysis>();
    
    // Helper properties for JSON conversion
    [NotMapped]
    public JsonDocument? MarketAnalysis
    {
        get => string.IsNullOrEmpty(MarketAnalysisJson) ? null : JsonDocument.Parse(MarketAnalysisJson);
        set => MarketAnalysisJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? SwotAnalysis
    {
        get => string.IsNullOrEmpty(SwotAnalysisJson) ? null : JsonDocument.Parse(SwotAnalysisJson);
        set => SwotAnalysisJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? BusinessPlan
    {
        get => string.IsNullOrEmpty(BusinessPlanJson) ? null : JsonDocument.Parse(BusinessPlanJson);
        set => BusinessPlanJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? Competitors
    {
        get => string.IsNullOrEmpty(CompetitorsJson) ? null : JsonDocument.Parse(CompetitorsJson);
        set => CompetitorsJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? ProviderInsights
    {
        get => string.IsNullOrEmpty(ProviderInsightsJson) ? null : JsonDocument.Parse(ProviderInsightsJson);
        set => ProviderInsightsJson = value?.RootElement.GetRawText();
    }
}