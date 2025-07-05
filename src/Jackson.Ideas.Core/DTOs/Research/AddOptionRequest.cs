using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.Research;

public class AddOptionRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public double FeasibilityScore { get; set; } = 0.0;
    
    public double ImpactScore { get; set; } = 0.0;
    
    public double RiskScore { get; set; } = 0.0;
    
    public string? Approach { get; set; }
    
    public int TimelineToMarketMonths { get; set; } = 0;
    
    public int SuccessProbabilityPercent { get; set; } = 0;
    
    public bool IsRecommended { get; set; } = false;
    
    public string? Notes { get; set; }
    
    public List<string>? Pros { get; set; }
    
    public List<string>? Cons { get; set; }
}