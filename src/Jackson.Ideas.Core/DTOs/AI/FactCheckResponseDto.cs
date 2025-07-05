using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.AI;

public class FactCheckResponseDto
{
    [Required]
    public string VerificationStatus { get; set; } = string.Empty; // verified, disputed, unverified
    
    [Required]
    public string ConfidenceLevel { get; set; } = string.Empty; // high, medium, low
    
    public List<string> Sources { get; set; } = new();
    
    [Required]
    public string Notes { get; set; } = string.Empty;
    
    // Additional properties for feasibility validation compatibility
    public double OverallFeasibilityScore { get; set; }
    
    public double TechnicalFeasibility { get; set; }
    
    public double MarketFeasibility { get; set; }
    
    public double FinancialFeasibility { get; set; }
    
    public string[] KeyAssumptions { get; set; } = Array.Empty<string>();
    
    public string[] CriticalRisks { get; set; } = Array.Empty<string>();
    
    public string[] RecommendedNextSteps { get; set; } = Array.Empty<string>();
}