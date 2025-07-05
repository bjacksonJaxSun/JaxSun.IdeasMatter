using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class MarketSizingAnalysisDto
{
    [Required]
    public string MarketName { get; set; } = string.Empty;
    
    [Required]
    public decimal TotalAddressableMarket { get; set; }
    
    public decimal Tam { get; set; }
    
    [Required]
    public decimal ServiceableAddressableMarket { get; set; }
    
    public decimal Sam { get; set; }
    
    [Required]
    public decimal ServiceableObtainableMarket { get; set; }
    
    public decimal Som { get; set; }
    
    [Required]
    public string Currency { get; set; } = "USD";
    
    public string MarketSizeMetric { get; set; } = string.Empty;
    
    public List<MarketSegmentDto> Segments { get; set; } = new();
    
    public List<string> KeyAssumptions { get; set; } = new();
    
    public Dictionary<string, object> MarketSizeBreakdown { get; set; } = new();
    
    public List<string> Assumptions { get; set; } = new();
    
    public Dictionary<string, object> GrowthProjections { get; set; } = new();
    
    public double ConfidenceLevel { get; set; }
    
    public Dictionary<string, decimal> MarketSizeByRegion { get; set; } = new();
    
    public string GrowthRate { get; set; } = string.Empty;
    
    public string MarketMaturity { get; set; } = string.Empty;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public List<string> Sources { get; set; } = new();
    
    public string Industry { get; set; } = string.Empty;
    public decimal TAM { get; set; }
    public decimal SAM { get; set; }
    public decimal SOM { get; set; }
    public int Year { get; set; }
    public string Methodology { get; set; } = string.Empty;
}