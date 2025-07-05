using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.MarketAnalysis;

public class MarketOpportunityDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public int Id { get; set; }
    
    public int SessionId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal PotentialRevenue { get; set; }
    
    public decimal PotentialValue { get; set; }
    
    [Required]
    public string OpportunityType { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty;
    
    public string Timeline { get; set; } = string.Empty;
    
    public double Feasibility { get; set; }
    
    public string RequiredResources { get; set; } = string.Empty;
    
    public string RiskFactors { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public decimal InvestmentRequired { get; set; }
    
    public decimal ExpectedROI { get; set; }
    
    public string RiskLevel { get; set; } = string.Empty;
    
    public List<string> KeySuccessFactors { get; set; } = new();
    
    public List<string> Challenges { get; set; } = new();
    
    public List<string> CompetitiveAdvantages { get; set; } = new();
    
    public List<string> TargetSegments { get; set; } = new();
    
    public List<string> RequiredCapabilities { get; set; } = new();
    
    public string MarketEntry { get; set; } = string.Empty;
    
    public Dictionary<string, string> AdditionalMetrics { get; set; } = new();
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public double Probability { get; set; }
}