using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Shared.DTOs;

public record CreateResearchRequest
{
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;
    
    [Required]
    [StringLength(5000, MinimumLength = 10)]
    public string Description { get; init; } = string.Empty;
    
    [StringLength(50)]
    public string ResearchType { get; init; } = "comprehensive";
    
    [StringLength(50)]
    public string? AIProvider { get; init; }
    
    public bool IncludeMarketAnalysis { get; init; } = true;
    
    public bool IncludeSwot { get; init; } = true;
    
    public bool IncludeBusinessPlan { get; init; } = true;
    
    [StringLength(100)]
    public string? GeographicScope { get; init; }
    
    [StringLength(100)]
    public string? Industry { get; init; }
    
    [StringLength(500)]
    public string? TargetMarket { get; init; }
}