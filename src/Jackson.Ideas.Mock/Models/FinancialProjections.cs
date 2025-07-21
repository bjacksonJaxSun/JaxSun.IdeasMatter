namespace Jackson.Ideas.Mock.Models;

public class FinancialProjections
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ScenarioId { get; set; } = "";
    public RevenueProjections Revenue { get; set; } = new();
    public ExpenseProjections Expenses { get; set; } = new();
    public CashFlowProjections CashFlow { get; set; } = new();
    public List<YearlyFinancials> YearlyBreakdown { get; set; } = new();
    public FinancialMetrics Metrics { get; set; } = new();
    public FundingRequirements Funding { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RevenueProjections
{
    public List<RevenueStream> Streams { get; set; } = new();
    public MonthlyProjection MonthlyGrowth { get; set; } = new();
    public string RevenueModel { get; set; } = "";
    public decimal Year1Total { get; set; }
    public decimal Year3Total { get; set; }
    public decimal Year5Total { get; set; }
}

public class RevenueStream
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Year1Amount { get; set; }
    public decimal Year3Amount { get; set; }
    public decimal Year5Amount { get; set; }
    public string PricingModel { get; set; } = "";
    public int CustomerCount { get; set; }
    public decimal AverageRevenuePerCustomer { get; set; }
}

public class ExpenseProjections
{
    public OperatingExpenses Operating { get; set; } = new();
    public StartupCosts Startup { get; set; } = new();
    public decimal Year1Total { get; set; }
    public decimal Year3Total { get; set; }
    public decimal Year5Total { get; set; }
    public string CostStructure { get; set; } = "";
}

public class OperatingExpenses
{
    public decimal PersonnelCosts { get; set; }
    public decimal MarketingExpenses { get; set; }
    public decimal TechnologyCosts { get; set; }
    public decimal OperationalExpenses { get; set; }
    public decimal AdministrativeExpenses { get; set; }
    public List<ExpenseCategory> Categories { get; set; } = new();
}

public class ExpenseCategory
{
    public string Name { get; set; } = "";
    public decimal MonthlyAmount { get; set; }
    public decimal AnnualAmount { get; set; }
    public string Type { get; set; } = ""; // Fixed, Variable, Semi-Variable
}

public class StartupCosts
{
    public decimal InitialCapital { get; set; }
    public decimal EquipmentCosts { get; set; }
    public decimal LegalAndRegulatory { get; set; }
    public decimal MarketingLaunch { get; set; }
    public decimal WorkingCapital { get; set; }
    public decimal ContingencyReserve { get; set; }
    public List<StartupExpense> Breakdown { get; set; } = new();
}

public class StartupExpense
{
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Timing { get; set; } = "";
}

public class CashFlowProjections
{
    public List<MonthlyProjection> Monthly { get; set; } = new();
    public decimal BreakEvenMonth { get; set; }
    public decimal MaxCashDeficit { get; set; }
    public decimal CashFlowPositiveMonth { get; set; }
    public string SeasonalityFactors { get; set; } = "";
}

public class MonthlyProjection
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal CumulativeCashFlow { get; set; }
}

public class YearlyFinancials
{
    public int Year { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal GrossMargin { get; set; }
    public decimal OperatingMargin { get; set; }
    public decimal NetMargin { get; set; }
    public int CustomerCount { get; set; }
    public decimal RevenuePerCustomer { get; set; }
}

public class FinancialMetrics
{
    public decimal GrossMarginPercent { get; set; }
    public decimal OperatingMarginPercent { get; set; }
    public decimal NetMarginPercent { get; set; }
    public decimal CustomerAcquisitionCost { get; set; }
    public decimal CustomerLifetimeValue { get; set; }
    public decimal ReturnOnInvestment { get; set; }
    public decimal PaybackPeriodMonths { get; set; }
    public decimal BurnRate { get; set; }
    public decimal RunwayMonths { get; set; }
}

public class FundingRequirements
{
    public decimal TotalFundingNeeded { get; set; }
    public List<FundingRound> Rounds { get; set; } = new();
    public string FundingStrategy { get; set; } = "";
    public List<string> PotentialInvestors { get; set; } = new();
    public string UseOfFunds { get; set; } = "";
    public string ExitStrategy { get; set; } = "";
}

public class FundingRound
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public string Stage { get; set; } = "";
    public string Timing { get; set; } = "";
    public string Purpose { get; set; } = "";
    public decimal ValuationEstimate { get; set; }
}