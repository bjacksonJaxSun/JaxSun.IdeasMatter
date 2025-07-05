using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Jackson.Ideas.Core.Entities;

public class MarketAnalysis : BaseEntity
{
    [StringLength(255)]
    public string? MarketSize { get; set; }
    
    [StringLength(100)]
    public string? GrowthRate { get; set; }
    
    [StringLength(500)]
    public string? TargetAudience { get; set; }
    
    public string? CompetitiveLandscapeJson { get; set; }
    
    public string? KeyTrendsJson { get; set; }
    
    public string? CustomerSegmentsJson { get; set; }
    
    public string? RegulatoryEnvironmentJson { get; set; }
    
    public string? RevenueModelsJson { get; set; }
    
    public string? EntryBarriersJson { get; set; }
    
    [StringLength(100)]
    public string? GeographicScope { get; set; }
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    // Foreign key
    public int ResearchId { get; set; }
    
    // Navigation properties
    public virtual Research Research { get; set; } = null!;
    
    // Helper properties for JSON conversion
    [NotMapped]
    public JsonDocument? CompetitiveLandscape
    {
        get => string.IsNullOrEmpty(CompetitiveLandscapeJson) ? null : JsonDocument.Parse(CompetitiveLandscapeJson);
        set => CompetitiveLandscapeJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? KeyTrends
    {
        get => string.IsNullOrEmpty(KeyTrendsJson) ? null : JsonDocument.Parse(KeyTrendsJson);
        set => KeyTrendsJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? CustomerSegments
    {
        get => string.IsNullOrEmpty(CustomerSegmentsJson) ? null : JsonDocument.Parse(CustomerSegmentsJson);
        set => CustomerSegmentsJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? RegulatoryEnvironment
    {
        get => string.IsNullOrEmpty(RegulatoryEnvironmentJson) ? null : JsonDocument.Parse(RegulatoryEnvironmentJson);
        set => RegulatoryEnvironmentJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? RevenueModels
    {
        get => string.IsNullOrEmpty(RevenueModelsJson) ? null : JsonDocument.Parse(RevenueModelsJson);
        set => RevenueModelsJson = value?.RootElement.GetRawText();
    }
    
    [NotMapped]
    public JsonDocument? EntryBarriers
    {
        get => string.IsNullOrEmpty(EntryBarriersJson) ? null : JsonDocument.Parse(EntryBarriersJson);
        set => EntryBarriersJson = value?.RootElement.GetRawText();
    }
}