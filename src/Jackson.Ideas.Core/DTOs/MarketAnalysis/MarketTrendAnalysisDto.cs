using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class MarketTrendAnalysisDto
{
    [Required]
    public string MarketName { get; set; } = string.Empty;
    
    public int Id { get; set; }
    
    public int SessionId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public string Impact { get; set; } = string.Empty;
    
    public double ImpactScore { get; set; }
    
    public string Timeline { get; set; } = string.Empty;
    
    public double Probability { get; set; }
    
    public string Sources { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public List<MarketTrendDto> EmergingTrends { get; set; } = new();
    
    public List<MarketTrendDto> DecliningTrends { get; set; } = new();
    
    public List<string> TechnologyTrends { get; set; } = new();
    
    public List<string> ConsumerBehaviorTrends { get; set; } = new();
    
    public List<string> RegulatoryTrends { get; set; } = new();
    
    public List<string> EconomicTrends { get; set; } = new();
    
    public string OverallMarketDirection { get; set; } = string.Empty;
    
    public List<string> OpportunityTrends { get; set; } = new();
    
    public List<string> ThreatTrends { get; set; } = new();
    
    public Dictionary<string, string> TrendImplications { get; set; } = new();
    
    public string TimeHorizon { get; set; } = string.Empty;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public List<string> SourcesList { get; set; } = new();
    
    // Additional properties for demo mode compatibility
    public string Industry { get; set; } = "";
    public List<MarketTrendDto> Trends { get; set; } = new();
    public int TrendConfidence { get; set; }
    public List<string> DataSources { get; set; } = new();
}