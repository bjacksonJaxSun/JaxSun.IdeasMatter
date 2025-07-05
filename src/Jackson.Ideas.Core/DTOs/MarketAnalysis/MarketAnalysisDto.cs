using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class MarketAnalysisDto
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string Industry { get; set; } = string.Empty;
    public string MarketCategory { get; set; } = string.Empty;
    public string GeographicScope { get; set; } = string.Empty;
    public string TargetDemographics { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public decimal? TamValue { get; set; }
    public decimal? SamValue { get; set; }
    public decimal? SomValue { get; set; }
    public double? CagrPercentage { get; set; }
    public string MarketMaturityStage { get; set; } = string.Empty;
    public string MarketDrivers { get; set; } = string.Empty;
    public string MarketBarriers { get; set; } = string.Empty;
    public string RegulatoryFactors { get; set; } = string.Empty;
    public string TechnologyTrends { get; set; } = string.Empty;
    public string CustomerSegments { get; set; } = string.Empty;
    public string KeyPainPoints { get; set; } = string.Empty;
    public string BuyingBehaviorPatterns { get; set; } = string.Empty;
    public double? PriceSensitivity { get; set; }
    public double? ConfidenceScore { get; set; }
    public string DataSources { get; set; } = string.Empty;
    public int? MarketSizeYear { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Additional properties for demo mode compatibility
    public string MarketSize { get; set; } = string.Empty;
    
    public string GrowthRate { get; set; } = string.Empty;
    
    public List<string> KeyTrends { get; set; } = new();
    
    public List<string> MainCompetitors { get; set; } = new();
    
    public List<string> Opportunities { get; set; } = new();
    
    // Legacy support for arrays
    public string[] CompetitiveLandscape { get; set; } = Array.Empty<string>();
}

public class MarketAnalysisCreateDto
{
    [Required]
    public int SessionId { get; set; }
    
    [Required]
    public string Industry { get; set; } = string.Empty;
    
    [Required]
    public string MarketCategory { get; set; } = string.Empty;
    
    public string GeographicScope { get; set; } = "global";
    public string TargetDemographics { get; set; } = string.Empty;
    public decimal? TamValue { get; set; }
    public decimal? SamValue { get; set; }
    public decimal? SomValue { get; set; }
    public double? CagrPercentage { get; set; }
    public string MarketMaturityStage { get; set; } = string.Empty;
    public string MarketDrivers { get; set; } = string.Empty;
    public string MarketBarriers { get; set; } = string.Empty;
    public string RegulatoryFactors { get; set; } = string.Empty;
    public string TechnologyTrends { get; set; } = string.Empty;
    public string CustomerSegments { get; set; } = string.Empty;
    public string KeyPainPoints { get; set; } = string.Empty;
    public string BuyingBehaviorPatterns { get; set; } = string.Empty;
    public double? PriceSensitivity { get; set; }
    public double? ConfidenceScore { get; set; }
    public string DataSources { get; set; } = string.Empty;
    public int? MarketSizeYear { get; set; }
}