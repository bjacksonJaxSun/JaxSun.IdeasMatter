using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class MarketSegmentDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Size { get; set; }
    
    public int Id { get; set; }
    
    public int MarketAnalysisId { get; set; }
    
    [Required]
    public string SizeUnit { get; set; } = string.Empty;
    
    public decimal MarketShare { get; set; }
    
    public decimal GrowthRate { get; set; }
    
    public decimal Value { get; set; }
    
    public long Size2 { get; set; }
    
    public double? PenetrationRate { get; set; }
    
    public string Characteristics { get; set; } = string.Empty;
    
    public string BuyingBehavior { get; set; } = string.Empty;
    
    public double Attractiveness { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public string PrimaryDemographics { get; set; } = string.Empty;
    
    public List<string> KeyCharacteristics { get; set; } = new();
    
    public List<string> PainPoints { get; set; } = new();
    
    public List<string> PreferredChannels { get; set; } = new();
    
    public decimal Revenue { get; set; }
    
    public string Profitability { get; set; } = string.Empty;
    
    public string CompetitivePosition { get; set; } = string.Empty;
    
    public List<string> OpportunityAreas { get; set; } = new();
    
    public Dictionary<string, string> AdditionalMetrics { get; set; } = new();
}