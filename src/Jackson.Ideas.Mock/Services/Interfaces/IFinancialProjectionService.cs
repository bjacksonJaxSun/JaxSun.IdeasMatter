using Jackson.Ideas.Mock.Models;

namespace Jackson.Ideas.Mock.Services.Interfaces;

public interface IFinancialProjectionService
{
    Task<FinancialProjections?> GetFinancialProjectionsAsync(string scenarioId);
    Task<List<FinancialProjections>> GetAllFinancialProjectionsAsync();
    Task<FinancialProjections?> GetFinancialProjectionsByIndustryAsync(string industry);
    Task<List<YearlyFinancials>> GetYearlyBreakdownAsync(string scenarioId);
    Task<CashFlowProjections> GetCashFlowProjectionsAsync(string scenarioId);
    Task<FinancialMetrics> GetFinancialMetricsAsync(string scenarioId);
}