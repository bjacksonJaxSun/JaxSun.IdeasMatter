using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Entities;

public class CompetitorAnalysis : BaseEntity
{
    [Required]
    public int MarketAnalysisId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Website { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public CompetitorTier Tier { get; set; } = CompetitorTier.Direct;
    
    public double? MarketShare { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Revenue { get; set; }
    
    public int? Employees { get; set; }
    
    public string ProductOfferings { get; set; } = string.Empty;
    
    public string PricingStrategy { get; set; } = string.Empty;
    
    public string TargetCustomers { get; set; } = string.Empty;
    
    public string Strengths { get; set; } = string.Empty;
    
    public string Weaknesses { get; set; } = string.Empty;
    
    public string CompetitiveAdvantages { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? FundingTotal { get; set; }
    
    public double? GrowthRate { get; set; }
    
    public double? ThreatLevel { get; set; }
    
    public int? WebsiteTraffic { get; set; }
    
    public double? CustomerRating { get; set; }
    
    public string SocialMediaPresence { get; set; } = string.Empty;
    
    public double DataCompleteness { get; set; } = 1.0;
    
    // Navigation properties
    public virtual MarketAnalysis MarketAnalysis { get; set; } = null!;
}