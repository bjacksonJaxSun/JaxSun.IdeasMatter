namespace Jackson.Ideas.Core.DTOs.Research;

public class ResearchStrategyResponse
{
    public Guid StrategyId { get; set; }
    
    public ResearchApproach Approach { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int EstimatedDurationMinutes { get; set; }
    
    public string ComplexityLevel { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;
    
    public double ProgressPercentage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string EstimatedCompletionTime { get; set; } = string.Empty;
    
    public List<string> IncludedAnalyses { get; set; } = new();
    
    public List<string> NextSteps { get; set; } = new();
}