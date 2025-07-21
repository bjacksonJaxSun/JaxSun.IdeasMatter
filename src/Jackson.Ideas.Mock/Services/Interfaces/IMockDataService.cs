using Jackson.Ideas.Mock.Models;

namespace Jackson.Ideas.Mock.Services.Interfaces;

public interface IMockDataService
{
    Task<List<BusinessIdeaScenario>> GetAllScenariosAsync();
    Task<BusinessIdeaScenario?> GetScenarioByIdAsync(string id);
    Task<List<BusinessIdeaScenario>> GetScenariosByIndustryAsync(string industry);
}