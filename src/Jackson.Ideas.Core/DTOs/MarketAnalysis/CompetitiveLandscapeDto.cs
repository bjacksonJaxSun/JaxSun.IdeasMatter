using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class CompetitiveLandscapeDto
{
    [Required]
    public string MarketName { get; set; } = string.Empty;
    
    public List<CompetitorAnalysisDto> DirectCompetitors { get; set; } = new();
    
    public List<CompetitorAnalysisDto> IndirectCompetitors { get; set; } = new();
    
    public List<CompetitorAnalysisDto> SubstituteThreats { get; set; } = new();
    
    public List<CompetitorAnalysisDto> SubstituteProducts { get; set; } = new();
    
    public CompetitorAnalysisDto? MarketLeader { get; set; }
    
    public List<string> CompetitiveAdvantages { get; set; } = new();
    
    public List<string> CompetitiveDisadvantages { get; set; } = new();
    
    public string MarketConcentration { get; set; } = string.Empty;
    
    public List<string> BarriersToEntry { get; set; } = new();
    
    public List<string> MarketTrends { get; set; } = new();
    
    public string CompetitiveIntensity { get; set; } = string.Empty;
    
    public double CompetitiveIntensityScore { get; set; }
    
    // Additional property for demo mode compatibility
    public int CompetitiveIntensityValue { get; set; }
    
    public Dictionary<string, string> KeySuccessFactors { get; set; } = new();
    
    public List<string> ThreatsOfSubstitutes { get; set; } = new();
    
    public string BuyerPower { get; set; } = string.Empty;
    
    public string SupplierPower { get; set; } = string.Empty;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public List<string> Sources { get; set; } = new();
}