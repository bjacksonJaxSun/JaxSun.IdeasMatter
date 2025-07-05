using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.AI;

public class OptionDto
{
    [Required]
    public string Category { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public List<string> Pros { get; set; } = new();
    
    public List<string> Cons { get; set; } = new();
    
    [Range(0, 1)]
    public double FeasibilityScore { get; set; }
    
    [Range(0, 1)]
    public double ImpactScore { get; set; }
    
    [Range(0, 1)]
    public double RiskScore { get; set; }
    
    // Additional properties for strategic options compatibility
    public string Approach { get; set; } = string.Empty;
    
    public string TargetSegment { get; set; } = string.Empty;
    
    public string ValueProposition { get; set; } = string.Empty;
    
    public double OverallScore { get; set; }
    
    public int TimelineMonths { get; set; }
    
    public decimal EstimatedInvestment { get; set; }
    
    public int SuccessProbability { get; set; }
}