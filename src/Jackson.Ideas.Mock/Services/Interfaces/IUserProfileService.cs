using Jackson.Ideas.Mock.Models;

namespace Jackson.Ideas.Mock.Services.Interfaces;

public interface IUserProfileService
{
    Task<MockUser?> GetUserProfileAsync(string userId);
    Task<bool> UpdateUserProfileAsync(MockUser user);
    Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences);
    Task<bool> UpdateUserSkillsAsync(string userId, List<string> skills);
    Task<bool> UpdateUserInterestsAsync(string userId, List<string> interests);
    Task<bool> AddAchievementBadgeAsync(string userId, string badge);
    Task<List<string>> GetUserAchievementsAsync(string userId);
    Task<bool> UpdateOnboardingProgressAsync(string userId, string step);
    Task<List<string>> GetIncompleteOnboardingStepsAsync(string userId);
    Task<bool> SetPreferredScenarioAsync(string userId, string scenarioId);
    Task<Dictionary<string, object>> GetUserAnalyticsAsync(string userId);
    Task<List<MockUser>> GetUsersBySkillAsync(string skill);
    Task<List<MockUser>> GetUsersByInterestAsync(string interest);
    Task<bool> UpdateUserActivityAsync(string userId);
    Task<UserProfile> GetCompleteUserProfileAsync(string userId);
}