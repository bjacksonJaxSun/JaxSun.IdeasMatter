namespace Jackson.Ideas.Core.DTOs.Research;

public class CompetitiveAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    
    public List<CompetitorProfile> DirectCompetitors { get; set; } = new();
    
    public List<CompetitorProfile> IndirectCompetitors { get; set; } = new();
    
    public List<string> SubstituteSolutions { get; set; } = new();
    
    public List<string> CompetitiveAdvantages { get; set; } = new();
    
    public List<string> BarriersToEntry { get; set; } = new();
    
    public MarketPositioningAnalysis PositioningAnalysis { get; set; } = new();
    
    public double ConfidenceScore { get; set; }
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties for test compatibility
    public string[] SubstituteProducts { get; set; } = Array.Empty<string>();
    
    public string[] MarketGaps { get; set; } = Array.Empty<string>();
    
    public string[] CompetitiveThreats { get; set; } = Array.Empty<string>();
    
    public string[] StrategicRecommendations { get; set; } = Array.Empty<string>();
    
    public string MarketPositioning { get; set; } = string.Empty;
    
    public double CompetitiveRating { get; set; }
    
    public double MarketOpportunity { get; set; }
    
    public double ThreatLevel { get; set; }
    
    public double DifferentiationStrength { get; set; }
    
    public string Industry { get; set; } = string.Empty;
    
    public List<CompetitorProfile> CompetitorProfiles { get; set; } = new();
    
    public string CompetitiveMatrix { get; set; } = string.Empty;
    
    public List<string> DifferentiationOpportunities { get; set; } = new();
    
    public double CompetitiveThreatLevel { get; set; }
}

public class CompetitorProfile
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string CompetitorType { get; set; } = string.Empty; // Direct, Indirect, Substitute
    
    public List<string> Strengths { get; set; } = new();
    
    public List<string> Weaknesses { get; set; } = new();
    
    public string MarketShare { get; set; } = string.Empty;
    
    public string PricingStrategy { get; set; } = string.Empty;
    
    public List<string> KeyFeatures { get; set; } = new();
    
    public string TargetCustomers { get; set; } = string.Empty;
    
    public string Website { get; set; } = string.Empty;
    
    public decimal? EstimatedRevenue { get; set; }
    
    public int? EmployeeCount { get; set; }
    
    public string FundingStage { get; set; } = string.Empty;
    
    public double ThreatLevel { get; set; } // 0-10 scale
    
    public string DifferentiationOpportunity { get; set; } = string.Empty;
}

public class MarketPositioningAnalysis
{
    public string PositioningStrategy { get; set; } = string.Empty;
    
    public List<string> DifferentiationFactors { get; set; } = new();
    
    public string ValuePropositionGap { get; set; } = string.Empty;
    
    public List<string> UnservedMarketSegments { get; set; } = new();
    
    public string RecommendedPositioning { get; set; } = string.Empty;
    
    public List<string> CompetitiveRisks { get; set; } = new();
    
    public string MarketEntryStrategy { get; set; } = string.Empty;
}