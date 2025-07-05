namespace Jackson.Ideas.Core.DTOs.Research;

public class DifferentiationOpportunity
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public double ImpactScore { get; set; }
    
    public double ImplementationDifficulty { get; set; }
    
    public string CompetitiveAdvantage { get; set; } = string.Empty;
    
    public List<string> RequiredCapabilities { get; set; } = new();
    
    public List<string> RiskFactors { get; set; } = new();
    
    public int EstimatedTimeToImplement { get; set; }
    
    public decimal EstimatedCost { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}