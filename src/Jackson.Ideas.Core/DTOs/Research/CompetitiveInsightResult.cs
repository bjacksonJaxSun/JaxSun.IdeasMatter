namespace Jackson.Ideas.Core.DTOs.Research;

public class CompetitiveInsightResult
{
    public string MarketPosition { get; set; } = string.Empty;
    
    public double CompetitiveStrength { get; set; }
    
    public List<string> KeyStrengths { get; set; } = new();
    
    public List<string> Weaknesses { get; set; } = new();
    
    public List<string> Opportunities { get; set; } = new();
    
    public List<string> Threats { get; set; } = new();
    
    public List<CompetitorProfile> MainCompetitors { get; set; } = new();
    
    public List<DifferentiationOpportunity> DifferentiationOpportunities { get; set; } = new();
    
    public string RecommendedStrategy { get; set; } = string.Empty;
    
    public List<string> NextSteps { get; set; } = new();
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    public double ConfidenceScore { get; set; }
}

