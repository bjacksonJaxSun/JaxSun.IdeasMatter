using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.Research;

public class ResearchStrategyRequest
{
    [Required]
    public Guid SessionId { get; set; }
    
    [Required]
    public string IdeaDescription { get; set; } = string.Empty;
    
    public string UserGoals { get; set; } = string.Empty;
    
    [Required]
    public ResearchApproach Approach { get; set; }
    
    public Dictionary<string, object>? Parameters { get; set; }
    
    public Dictionary<string, object>? CustomParameters { get; set; }
}

public enum ResearchApproach
{
    QuickValidation,
    MarketDeepDive,
    LaunchStrategy
}