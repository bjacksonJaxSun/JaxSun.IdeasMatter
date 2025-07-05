using System.ComponentModel.DataAnnotations;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class CompetitorAnalysisDto
{
    public int Id { get; set; }
    public int MarketAnalysisId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string Description { get; set; } = string.Empty;
    public CompetitorTier Tier { get; set; }
    public double? MarketShare { get; set; }
    public decimal? Revenue { get; set; }
    public int? Employees { get; set; }
    public string ProductOfferings { get; set; } = string.Empty;
    public string PricingStrategy { get; set; } = string.Empty;
    public string TargetCustomers { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string CompetitiveAdvantages { get; set; } = string.Empty;
    public decimal? FundingTotal { get; set; }
    public double? GrowthRate { get; set; }
    public double? ThreatLevel { get; set; }
    public int? WebsiteTraffic { get; set; }
    public double? CustomerRating { get; set; }
    public string SocialMediaPresence { get; set; } = string.Empty;
    public double DataCompleteness { get; set; } = 1.0;
    public int? EmployeeCount { get; set; }
    public decimal? FundingAmount { get; set; }
    public string CompetitiveTier { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CompetitorAnalysisCreateDto
{
    [Required]
    public int MarketAnalysisId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Website { get; set; }
    public string Description { get; set; } = string.Empty;
    public CompetitorTier Tier { get; set; } = CompetitorTier.Direct;
    public double? MarketShare { get; set; }
    public decimal? Revenue { get; set; }
    public int? Employees { get; set; }
    public string ProductOfferings { get; set; } = string.Empty;
    public string PricingStrategy { get; set; } = string.Empty;
    public string TargetCustomers { get; set; } = string.Empty;
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string CompetitiveAdvantages { get; set; } = string.Empty;
    public decimal? FundingTotal { get; set; }
    public double? GrowthRate { get; set; }
    public double? ThreatLevel { get; set; }
    public int? WebsiteTraffic { get; set; }
    public double? CustomerRating { get; set; }
    public string SocialMediaPresence { get; set; } = string.Empty;
    public double DataCompleteness { get; set; } = 1.0;
}