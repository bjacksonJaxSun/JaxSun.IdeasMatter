using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.Entities;

public class ResearchOption
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }

    [Required]
    public Guid ResearchSessionId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Approach { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string TargetCustomerSegment { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string ValueProposition { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string GoToMarketStrategy { get; set; } = string.Empty;
    
    [Range(0.0, 10.0)]
    public double OverallScore { get; set; } = 0.0;
    
    [Range(0.0, 10.0)]
    public double FeasibilityScore { get; set; } = 0.0;
    
    [Range(0.0, 10.0)]
    public double ImpactScore { get; set; } = 0.0;
    
    public int TimelineToMarketMonths { get; set; }
    
    public int TimelineToProfitabilityMonths { get; set; }
    
    [Range(0, 100)]
    public int SuccessProbabilityPercent { get; set; }
    
    public decimal EstimatedInvestmentUsd { get; set; }
    
    public bool IsRecommended { get; set; } = false;
    
    // JSON serialized business model data
    public string BusinessModel { get; set; } = "{}";
    
    // JSON serialized risk factors array
    public string RiskFactors { get; set; } = "[]";
    
    // JSON serialized mitigation strategies array
    public string MitigationStrategies { get; set; } = "[]";
    
    // JSON serialized success metrics array
    public string SuccessMetrics { get; set; } = "[]";
    
    // JSON serialized SWOT analysis
    public string SwotAnalysis { get; set; } = "{}";
    
    [StringLength(1000)]
    public string? CompetitivePositioning { get; set; }
    
    [StringLength(2000)]
    public string Notes { get; set; } = string.Empty;
    
    public int? SortOrder { get; set; }
    
    // Navigation properties
    public virtual ResearchSession? ResearchSession { get; set; }
}