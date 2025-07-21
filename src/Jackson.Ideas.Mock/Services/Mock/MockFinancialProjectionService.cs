using Jackson.Ideas.Mock.Models;
using Jackson.Ideas.Mock.Services.Interfaces;

namespace Jackson.Ideas.Mock.Services.Mock;

public class MockFinancialProjectionService : IFinancialProjectionService
{
    private readonly List<FinancialProjections> _financialProjections;

    public MockFinancialProjectionService()
    {
        _financialProjections = GenerateFinancialProjections();
    }

    public async Task<FinancialProjections?> GetFinancialProjectionsAsync(string scenarioId)
    {
        await Task.Delay(150);
        return _financialProjections.FirstOrDefault(f => f.ScenarioId == scenarioId);
    }

    public async Task<List<FinancialProjections>> GetAllFinancialProjectionsAsync()
    {
        await Task.Delay(200);
        return _financialProjections;
    }

    public async Task<FinancialProjections?> GetFinancialProjectionsByIndustryAsync(string industry)
    {
        await Task.Delay(100);
        return _financialProjections.FirstOrDefault();
    }

    public async Task<List<YearlyFinancials>> GetYearlyBreakdownAsync(string scenarioId)
    {
        await Task.Delay(100);
        var projections = _financialProjections.FirstOrDefault(f => f.ScenarioId == scenarioId);
        return projections?.YearlyBreakdown ?? new List<YearlyFinancials>();
    }

    public async Task<CashFlowProjections> GetCashFlowProjectionsAsync(string scenarioId)
    {
        await Task.Delay(100);
        var projections = _financialProjections.FirstOrDefault(f => f.ScenarioId == scenarioId);
        return projections?.CashFlow ?? new CashFlowProjections();
    }

    public async Task<FinancialMetrics> GetFinancialMetricsAsync(string scenarioId)
    {
        await Task.Delay(100);
        var projections = _financialProjections.FirstOrDefault(f => f.ScenarioId == scenarioId);
        return projections?.Metrics ?? new FinancialMetrics();
    }

    private static List<FinancialProjections> GenerateFinancialProjections()
    {
        return new List<FinancialProjections>
        {
            new FinancialProjections
            {
                ScenarioId = "eco-eats-001",
                Revenue = new RevenueProjections
                {
                    RevenueModel = "Commission-based (15-20% per order) + Subscription fees",
                    Year1Total = 500000,
                    Year3Total = 2800000,
                    Year5Total = 8500000,
                    Streams = new List<RevenueStream>
                    {
                        new RevenueStream
                        {
                            Name = "Delivery Commission",
                            Description = "Commission from restaurant orders",
                            Year1Amount = 350000,
                            Year3Amount = 2100000,
                            Year5Amount = 6800000,
                            PricingModel = "18% commission per order",
                            CustomerCount = 25000,
                            AverageRevenuePerCustomer = 14.00m
                        },
                        new RevenueStream
                        {
                            Name = "Subscription Fees",
                            Description = "Premium membership for customers",
                            Year1Amount = 120000,
                            Year3Amount = 550000,
                            Year5Amount = 1200000,
                            PricingModel = "$9.99/month premium membership",
                            CustomerCount = 1000,
                            AverageRevenuePerCustomer = 120.00m
                        },
                        new RevenueStream
                        {
                            Name = "Partner Services",
                            Description = "Marketing and consulting for restaurants",
                            Year1Amount = 30000,
                            Year3Amount = 150000,
                            Year5Amount = 500000,
                            PricingModel = "Monthly service packages $250-1000",
                            CustomerCount = 150,
                            AverageRevenuePerCustomer = 200.00m
                        }
                    },
                    MonthlyGrowth = new MonthlyProjection
                    {
                        Month = 12,
                        Revenue = 65000,
                        Expenses = 45000,
                        NetCashFlow = 20000,
                        CumulativeCashFlow = 85000
                    }
                },
                Expenses = new ExpenseProjections
                {
                    Year1Total = 415000,
                    Year3Total = 1960000,
                    Year5Total = 5950000,
                    CostStructure = "Variable costs scale with order volume; fixed costs for platform and overhead",
                    Operating = new OperatingExpenses
                    {
                        PersonnelCosts = 180000,
                        MarketingExpenses = 85000,
                        TechnologyCosts = 45000,
                        OperationalExpenses = 75000,
                        AdministrativeExpenses = 30000,
                        Categories = new List<ExpenseCategory>
                        {
                            new ExpenseCategory { Name = "Delivery Costs", MonthlyAmount = 12000, AnnualAmount = 144000, Type = "Variable" },
                            new ExpenseCategory { Name = "Platform Development", MonthlyAmount = 8000, AnnualAmount = 96000, Type = "Fixed" },
                            new ExpenseCategory { Name = "Customer Acquisition", MonthlyAmount = 15000, AnnualAmount = 180000, Type = "Variable" }
                        }
                    },
                    Startup = new StartupCosts
                    {
                        InitialCapital = 150000,
                        EquipmentCosts = 25000,
                        LegalAndRegulatory = 15000,
                        MarketingLaunch = 40000,
                        WorkingCapital = 50000,
                        ContingencyReserve = 20000,
                        Breakdown = new List<StartupExpense>
                        {
                            new StartupExpense { Category = "Technology", Description = "Platform development and infrastructure", Amount = 60000, Timing = "Months 1-3" },
                            new StartupExpense { Category = "Marketing", Description = "Launch campaign and brand development", Amount = 40000, Timing = "Months 2-4" },
                            new StartupExpense { Category = "Operations", Description = "Initial inventory and delivery setup", Amount = 30000, Timing = "Month 2" }
                        }
                    }
                },
                CashFlow = new CashFlowProjections
                {
                    BreakEvenMonth = 14,
                    MaxCashDeficit = -85000,
                    CashFlowPositiveMonth = 16,
                    SeasonalityFactors = "Higher demand during holidays and winter months; 20% variance",
                    Monthly = GenerateMonthlyCashFlow(12, 25000, 35000)
                },
                YearlyBreakdown = new List<YearlyFinancials>
                {
                    new YearlyFinancials
                    {
                        Year = 1,
                        Revenue = 500000,
                        Expenses = 415000,
                        NetIncome = 85000,
                        GrossMargin = 0.72m,
                        OperatingMargin = 0.17m,
                        NetMargin = 0.17m,
                        CustomerCount = 25000,
                        RevenuePerCustomer = 20.00m
                    },
                    new YearlyFinancials
                    {
                        Year = 3,
                        Revenue = 2800000,
                        Expenses = 1960000,
                        NetIncome = 840000,
                        GrossMargin = 0.75m,
                        OperatingMargin = 0.30m,
                        NetMargin = 0.30m,
                        CustomerCount = 140000,
                        RevenuePerCustomer = 20.00m
                    },
                    new YearlyFinancials
                    {
                        Year = 5,
                        Revenue = 8500000,
                        Expenses = 5950000,
                        NetIncome = 2550000,
                        GrossMargin = 0.78m,
                        OperatingMargin = 0.30m,
                        NetMargin = 0.30m,
                        CustomerCount = 425000,
                        RevenuePerCustomer = 20.00m
                    }
                },
                Metrics = new FinancialMetrics
                {
                    GrossMarginPercent = 72.0m,
                    OperatingMarginPercent = 17.0m,
                    NetMarginPercent = 17.0m,
                    CustomerAcquisitionCost = 45.00m,
                    CustomerLifetimeValue = 240.00m,
                    ReturnOnInvestment = 56.7m,
                    PaybackPeriodMonths = 18,
                    BurnRate = 25000.00m,
                    RunwayMonths = 6
                },
                Funding = new FundingRequirements
                {
                    TotalFundingNeeded = 750000,
                    FundingStrategy = "Seed round followed by Series A for expansion",
                    UseOfFunds = "40% technology development, 35% marketing, 15% operations, 10% working capital",
                    ExitStrategy = "Strategic acquisition by major food delivery company or IPO after 7-10 years",
                    PotentialInvestors = new List<string> { "Sustainability-focused VCs", "Food tech investors", "Impact investors" },
                    Rounds = new List<FundingRound>
                    {
                        new FundingRound
                        {
                            Name = "Seed Round",
                            Amount = 500000,
                            Stage = "Seed",
                            Timing = "Month 6",
                            Purpose = "Platform development and initial market validation",
                            ValuationEstimate = 2500000
                        },
                        new FundingRound
                        {
                            Name = "Series A",
                            Amount = 2500000,
                            Stage = "Series A",
                            Timing = "Month 18",
                            Purpose = "Market expansion and team growth",
                            ValuationEstimate = 12500000
                        }
                    }
                }
            },
            new FinancialProjections
            {
                ScenarioId = "ai-study-buddy-002",
                Revenue = new RevenueProjections
                {
                    RevenueModel = "Freemium SaaS with premium subscriptions and institutional licenses",
                    Year1Total = 800000,
                    Year3Total = 4200000,
                    Year5Total = 12800000,
                    Streams = new List<RevenueStream>
                    {
                        new RevenueStream
                        {
                            Name = "Individual Subscriptions",
                            Description = "Monthly premium subscriptions for individuals",
                            Year1Amount = 480000,
                            Year3Amount = 2520000,
                            Year5Amount = 7680000,
                            PricingModel = "$19.99/month premium subscription",
                            CustomerCount = 2000,
                            AverageRevenuePerCustomer = 240.00m
                        },
                        new RevenueStream
                        {
                            Name = "Institutional Licenses",
                            Description = "Enterprise licenses for schools and organizations",
                            Year1Amount = 240000,
                            Year3Amount = 1260000,
                            Year5Amount = 3840000,
                            PricingModel = "$5-15 per student per month",
                            CustomerCount = 50,
                            AverageRevenuePerCustomer = 4800.00m
                        },
                        new RevenueStream
                        {
                            Name = "Tutoring Marketplace",
                            Description = "Commission from human tutor connections",
                            Year1Amount = 80000,
                            Year3Amount = 420000,
                            Year5Amount = 1280000,
                            PricingModel = "20% commission on tutor sessions",
                            CustomerCount = 800,
                            AverageRevenuePerCustomer = 100.00m
                        }
                    }
                },
                Expenses = new ExpenseProjections
                {
                    Year1Total = 580000,
                    Year3Total = 2940000,
                    Year5Total = 8960000,
                    CostStructure = "High upfront AI development costs; scalable cloud infrastructure",
                    Operating = new OperatingExpenses
                    {
                        PersonnelCosts = 320000,
                        MarketingExpenses = 120000,
                        TechnologyCosts = 80000,
                        OperationalExpenses = 45000,
                        AdministrativeExpenses = 15000,
                        Categories = new List<ExpenseCategory>
                        {
                            new ExpenseCategory { Name = "AI Development", MonthlyAmount = 15000, AnnualAmount = 180000, Type = "Fixed" },
                            new ExpenseCategory { Name = "Cloud Infrastructure", MonthlyAmount = 8000, AnnualAmount = 96000, Type = "Variable" },
                            new ExpenseCategory { Name = "Content Creation", MonthlyAmount = 12000, AnnualAmount = 144000, Type = "Fixed" }
                        }
                    },
                    Startup = new StartupCosts
                    {
                        InitialCapital = 200000,
                        EquipmentCosts = 30000,
                        LegalAndRegulatory = 20000,
                        MarketingLaunch = 50000,
                        WorkingCapital = 75000,
                        ContingencyReserve = 25000
                    }
                },
                YearlyBreakdown = new List<YearlyFinancials>
                {
                    new YearlyFinancials
                    {
                        Year = 1,
                        Revenue = 800000,
                        Expenses = 580000,
                        NetIncome = 220000,
                        GrossMargin = 0.80m,
                        OperatingMargin = 0.28m,
                        NetMargin = 0.28m,
                        CustomerCount = 2850,
                        RevenuePerCustomer = 280.70m
                    }
                },
                Metrics = new FinancialMetrics
                {
                    GrossMarginPercent = 80.0m,
                    OperatingMarginPercent = 28.0m,
                    NetMarginPercent = 28.0m,
                    CustomerAcquisitionCost = 35.00m,
                    CustomerLifetimeValue = 1800.00m,
                    ReturnOnInvestment = 110.0m,
                    PaybackPeriodMonths = 12,
                    BurnRate = 15000.00m,
                    RunwayMonths = 18
                },
                Funding = new FundingRequirements
                {
                    TotalFundingNeeded = 1200000,
                    FundingStrategy = "Angel round followed by VC funding for growth",
                    Rounds = new List<FundingRound>
                    {
                        new FundingRound
                        {
                            Name = "Angel Round",
                            Amount = 500000,
                            Stage = "Angel",
                            Timing = "Month 3",
                            Purpose = "AI development and initial product launch"
                        }
                    }
                }
            }
        };
    }

    private static List<MonthlyProjection> GenerateMonthlyCashFlow(int months, decimal startingRevenue, decimal startingExpenses)
    {
        var projections = new List<MonthlyProjection>();
        var cumulativeCashFlow = 0m;
        
        for (int i = 1; i <= months; i++)
        {
            var revenue = startingRevenue * (1 + (i * 0.15m));
            var expenses = startingExpenses * (1 + (i * 0.08m));
            var netCashFlow = revenue - expenses;
            cumulativeCashFlow += netCashFlow;
            
            projections.Add(new MonthlyProjection
            {
                Month = i,
                Revenue = revenue,
                Expenses = expenses,
                NetCashFlow = netCashFlow,
                CumulativeCashFlow = cumulativeCashFlow
            });
        }
        
        return projections;
    }
}