using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.BusinessPlan;

public class BusinessPlanDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string MarketOpportunity { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string RevenueModel { get; set; } = string.Empty;
    public string GoToMarketStrategy { get; set; } = string.Empty;
    public string FundingRequirements { get; set; } = string.Empty;
    public FinancialProjectionsDto? FinancialProjections { get; set; }
    public TeamStructureDto? Team { get; set; }
    public List<MilestoneDto> Milestones { get; set; } = new();
    public Dictionary<string, object> AdditionalSections { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BusinessPlanCreateDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string ExecutiveSummary { get; set; } = string.Empty;
    
    public string MarketOpportunity { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string RevenueModel { get; set; } = string.Empty;
    public string GoToMarketStrategy { get; set; } = string.Empty;
    public string FundingRequirements { get; set; } = string.Empty;
    public FinancialProjectionsDto? FinancialProjections { get; set; }
    public TeamStructureDto? Team { get; set; }
    public List<MilestoneDto> Milestones { get; set; } = new();
}

public class BusinessPlanUpdateDto
{
    public string? Title { get; set; }
    public string? ExecutiveSummary { get; set; }
    public string? MarketOpportunity { get; set; }
    public string? ProductDescription { get; set; }
    public string? RevenueModel { get; set; }
    public string? GoToMarketStrategy { get; set; }
    public string? FundingRequirements { get; set; }
    public FinancialProjectionsDto? FinancialProjections { get; set; }
    public TeamStructureDto? Team { get; set; }
    public List<MilestoneDto>? Milestones { get; set; }
}

public class FinancialProjectionsDto
{
    public List<string> Revenue { get; set; } = new();
    public List<string> Costs { get; set; } = new();
    public List<string> Profit { get; set; } = new();
    public Dictionary<string, string> YearlyProjections { get; set; } = new();
    public string Currency { get; set; } = "USD";
    public double? BreakEvenMonth { get; set; }
    public string? FundingNeeded { get; set; }
}

public class TeamStructureDto
{
    public List<string> Founders { get; set; } = new();
    public List<string> KeyHires { get; set; } = new();
    public List<string> Advisors { get; set; } = new();
    public Dictionary<string, string> Roles { get; set; } = new();
    public int? TotalTeamSize { get; set; }
}

public class MilestoneDto
{
    public int Id { get; set; }
    public string Quarter { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public string Status { get; set; } = "Planned";
    public int Priority { get; set; } = 1;
}