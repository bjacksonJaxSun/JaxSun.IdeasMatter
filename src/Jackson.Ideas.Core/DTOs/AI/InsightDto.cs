using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.AI;

public class InsightDto
{
    [Required]
    public string Category { get; set; } = string.Empty; // target_market, customer_profile, problem_solution, growth_targets, cost_model, revenue_model
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, 1)]
    public double ConfidenceScore { get; set; }
    
    public string? Subcategory { get; set; }
}