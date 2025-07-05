namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class ComprehensiveMarketAnalysisDto
{
    // Strategic Level (High-level overview)
    public string ExecutiveSummary { get; set; } = "";
    public dynamic MarketSizing { get; set; } = new { TAM = 0M, SAM = 0M, SOM = 0M };
    public int ConfidenceScore { get; set; }
    
    // Operational Level (Detailed metrics)
    public CompetitiveLandscapeDto CompetitiveLandscape { get; set; } = new();
    public List<MarketTrendDto> MarketTrends { get; set; } = new();
    public List<MarketSegmentDto> CustomerSegments { get; set; } = new();
    
    // Analytical Level (Deep-dive data)
    public List<string> RegulatoryAnalysis { get; set; } = new();
    public List<MarketOpportunityDto> MarketOpportunities { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
    
    // Metadata for confidence visualization
    public AnalysisMetadata AnalysisMetadata { get; set; } = new();

    public string MarketAnalysis { get; set; } = string.Empty;
    public List<CompetitorAnalysisDto> Competitors { get; set; } = new();
    public List<string> Segments { get; set; } = new();
    public List<string> Trends { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
}

public class MarketTrendDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int ImpactLevel { get; set; }
    public string Timeline { get; set; } = "";
    public int Confidence { get; set; }
}

public class AnalysisMetadata
{
    public TimeSpan ProcessingTime { get; set; }
    public List<string> DataSources { get; set; } = new();
    public Dictionary<string, int> ConfidenceFactors { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}