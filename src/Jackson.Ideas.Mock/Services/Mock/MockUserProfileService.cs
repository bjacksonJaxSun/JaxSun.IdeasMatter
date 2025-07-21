using Jackson.Ideas.Mock.Models;
using Jackson.Ideas.Mock.Services.Interfaces;

namespace Jackson.Ideas.Mock.Services.Mock;

public class MockUserProfileService : IUserProfileService
{
    private readonly IMockAuthenticationService _authService;
    private readonly IMockDataService _dataService;
    private readonly Dictionary<string, List<UserActivity>> _userActivities;

    public MockUserProfileService(IMockAuthenticationService authService, IMockDataService dataService)
    {
        _authService = authService;
        _dataService = dataService;
        _userActivities = new Dictionary<string, List<UserActivity>>();
        InitializeUserActivities();
    }

    public async Task<MockUser?> GetUserProfileAsync(string userId)
    {
        return await _authService.GetUserByIdAsync(userId);
    }

    public async Task<bool> UpdateUserProfileAsync(MockUser user)
    {
        var success = await _authService.UpdateUserAsync(user);
        if (success)
        {
            await LogUserActivityAsync(user.Id, "ProfileUpdated", "User profile information updated");
        }
        return success;
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.Preferences = preferences;
            var success = await _authService.UpdateUserAsync(user);
            if (success)
            {
                await LogUserActivityAsync(userId, "PreferencesUpdated", "User preferences updated");
            }
            return success;
        }
        return false;
    }

    public async Task<bool> UpdateUserSkillsAsync(string userId, List<string> skills)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.Skills = skills;
            var success = await _authService.UpdateUserAsync(user);
            if (success)
            {
                await LogUserActivityAsync(userId, "SkillsUpdated", $"Skills updated: {string.Join(", ", skills)}");
                
                // Award skill badge if they have 5+ skills
                if (skills.Count >= 5 && !user.AchievementBadges.Contains("Skill Expert"))
                {
                    await AddAchievementBadgeAsync(userId, "Skill Expert");
                }
            }
            return success;
        }
        return false;
    }

    public async Task<bool> UpdateUserInterestsAsync(string userId, List<string> interests)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.Interests = interests;
            var success = await _authService.UpdateUserAsync(user);
            if (success)
            {
                await LogUserActivityAsync(userId, "InterestsUpdated", $"Interests updated: {string.Join(", ", interests)}");
            }
            return success;
        }
        return false;
    }

    public async Task<bool> AddAchievementBadgeAsync(string userId, string badge)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null && !user.AchievementBadges.Contains(badge))
        {
            user.AchievementBadges.Add(badge);
            var success = await _authService.UpdateUserAsync(user);
            if (success)
            {
                await LogUserActivityAsync(userId, "BadgeEarned", $"Earned achievement badge: {badge}");
            }
            return success;
        }
        return false;
    }

    public async Task<List<string>> GetUserAchievementsAsync(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        return user?.AchievementBadges ?? new List<string>();
    }

    public async Task<bool> UpdateOnboardingProgressAsync(string userId, string step)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null && !user.CompletedOnboardingSteps.Contains(step))
        {
            user.CompletedOnboardingSteps.Add(step);
            var success = await _authService.UpdateUserAsync(user);
            if (success)
            {
                await LogUserActivityAsync(userId, "OnboardingProgress", $"Completed onboarding step: {step}");
                
                // Award onboarding completion badge
                var allSteps = new[] { "Profile", "Preferences", "Skills Assessment", "Goals", "Investment Profile" };
                if (allSteps.All(s => user.CompletedOnboardingSteps.Contains(s)) && 
                    !user.AchievementBadges.Contains("Onboarding Complete"))
                {
                    await AddAchievementBadgeAsync(userId, "Onboarding Complete");
                }
            }
            return success;
        }
        return false;
    }

    public async Task<List<string>> GetIncompleteOnboardingStepsAsync(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) return new List<string>();

        var allSteps = new[] { "Profile", "Preferences", "Skills Assessment", "Goals", "Investment Profile" };
        return allSteps.Where(step => !user.CompletedOnboardingSteps.Contains(step)).ToList();
    }

    public async Task<bool> SetPreferredScenarioAsync(string userId, string scenarioId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.PreferredScenarioId = scenarioId;
            var success = await _authService.UpdateUserAsync(user);
            if (success)
            {
                await LogUserActivityAsync(userId, "PreferredScenarioSet", $"Set preferred scenario: {scenarioId}");
            }
            return success;
        }
        return false;
    }

    public async Task<Dictionary<string, object>> GetUserAnalyticsAsync(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) return new Dictionary<string, object>();

        var activities = _userActivities.GetValueOrDefault(userId, new List<UserActivity>());
        
        return new Dictionary<string, object>
        {
            ["TotalSessions"] = activities.Count(a => a.Type == "SessionStarted"),
            ["TotalTimeSpent"] = TimeSpan.FromHours(user.TimeCommitmentHours * 4), // Mock calculation
            ["LastActiveDate"] = user.LastActivityAt ?? user.LastLoginAt,
            ["IdeasViewed"] = activities.Count(a => a.Type == "ScenarioViewed"),
            ["ProfileUpdates"] = activities.Count(a => a.Type == "ProfileUpdated"),
            ["BadgesEarned"] = user.AchievementBadges.Count,
            ["EngagementScore"] = CalculateEngagementScore(user, activities),
            ["TopActivities"] = activities.GroupBy(a => a.Type).OrderByDescending(g => g.Count()).Take(3).Select(g => g.Key).ToList()
        };
    }

    public async Task<List<MockUser>> GetUsersBySkillAsync(string skill)
    {
        var allUsers = await _authService.GetAllUsersAsync();
        return allUsers.Where(u => u.Skills.Contains(skill, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<MockUser>> GetUsersByInterestAsync(string interest)
    {
        var allUsers = await _authService.GetAllUsersAsync();
        return allUsers.Where(u => u.Interests.Contains(interest, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    public async Task<bool> UpdateUserActivityAsync(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user != null)
        {
            user.LastActivityAt = DateTime.UtcNow;
            return await _authService.UpdateUserAsync(user);
        }
        return false;
    }

    public async Task<UserProfile> GetCompleteUserProfileAsync(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null) throw new ArgumentException("User not found");

        var activities = _userActivities.GetValueOrDefault(userId, new List<UserActivity>());
        var analytics = await GetUserAnalyticsAsync(userId);
        var scenarios = await _dataService.GetAllScenariosAsync();
        
        return new UserProfile
        {
            User = user,
            Analytics = new UserAnalytics
            {
                TotalSessions = (int)analytics.GetValueOrDefault("TotalSessions", 0),
                TotalTimeSpent = (TimeSpan)analytics.GetValueOrDefault("TotalTimeSpent", TimeSpan.Zero),
                LastActiveDate = (DateTime)analytics.GetValueOrDefault("LastActiveDate", DateTime.UtcNow),
                IdeasViewed = (int)analytics.GetValueOrDefault("IdeasViewed", 0),
                EngagementScore = (decimal)analytics.GetValueOrDefault("EngagementScore", 0),
                TopInteractionTypes = (List<string>)analytics.GetValueOrDefault("TopActivities", new List<string>())
            },
            RecentActivity = activities.OrderByDescending(a => a.Timestamp).Take(10).ToList(),
            SuggestedScenarios = GetSuggestedScenarios(user, scenarios),
            RecommendedConnections = await GetRecommendedConnections(user),
            Insights = GenerateUserInsights(user, activities)
        };
    }

    private async Task LogUserActivityAsync(string userId, string type, string description)
    {
        if (!_userActivities.ContainsKey(userId))
        {
            _userActivities[userId] = new List<UserActivity>();
        }

        _userActivities[userId].Add(new UserActivity
        {
            Type = type,
            Description = description,
            Timestamp = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }

    private static decimal CalculateEngagementScore(MockUser user, List<UserActivity> activities)
    {
        var baseScore = user.CompletedOnboardingSteps.Count * 10;
        var activityScore = activities.Count * 2;
        var badgeScore = user.AchievementBadges.Count * 5;
        var timeScore = Math.Min(user.TimeCommitmentHours, 40);
        
        return Math.Min(100, baseScore + activityScore + badgeScore + timeScore);
    }

    private static List<string> GetSuggestedScenarios(MockUser user, List<BusinessIdeaScenario> scenarios)
    {
        return scenarios
            .Where(s => user.Interests.Any(interest => 
                s.Industry.Contains(interest, StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains(interest, StringComparison.OrdinalIgnoreCase)))
            .Take(3)
            .Select(s => s.Id)
            .ToList();
    }

    private async Task<List<string>> GetRecommendedConnections(MockUser user)
    {
        var allUsers = await _authService.GetAllUsersAsync();
        
        return allUsers
            .Where(u => u.Id != user.Id && 
                       (u.Skills.Intersect(user.Skills).Any() || 
                        u.Interests.Intersect(user.Interests).Any()))
            .Take(3)
            .Select(u => $"{u.FirstName} {u.LastName}")
            .ToList();
    }

    private static UserInsights GenerateUserInsights(MockUser user, List<UserActivity> activities)
    {
        var completionPercentage = CalculateProfileCompletion(user);
        var missingElements = GetMissingProfileElements(user);
        
        return new UserInsights
        {
            StrengthAreas = user.Skills.Take(3).ToList(),
            GrowthOpportunities = new List<string> { "Network Building", "Skill Development", "Market Research" },
            PersonalizedTips = GeneratePersonalizedTips(user, completionPercentage),
            NextRecommendedAction = GetNextRecommendedAction(user, missingElements),
            ProfileCompletionPercentage = completionPercentage,
            MissingProfileElements = missingElements
        };
    }

    private static decimal CalculateProfileCompletion(MockUser user)
    {
        var completedFields = 0;
        var totalFields = 15;

        if (!string.IsNullOrEmpty(user.FirstName)) completedFields++;
        if (!string.IsNullOrEmpty(user.LastName)) completedFields++;
        if (!string.IsNullOrEmpty(user.Email)) completedFields++;
        if (!string.IsNullOrEmpty(user.Bio)) completedFields++;
        if (!string.IsNullOrEmpty(user.Company)) completedFields++;
        if (!string.IsNullOrEmpty(user.JobTitle)) completedFields++;
        if (!string.IsNullOrEmpty(user.Location)) completedFields++;
        if (user.Skills.Any()) completedFields++;
        if (user.Interests.Any()) completedFields++;
        if (user.AvailableFunding > 0) completedFields++;
        if (user.TimeCommitmentHours > 0) completedFields++;
        if (!string.IsNullOrEmpty(user.LinkedInProfile)) completedFields++;
        if (!string.IsNullOrEmpty(user.GitHubProfile)) completedFields++;
        if (user.Preferences.IndustryPreferences.Any()) completedFields++;
        if (user.CompletedOnboardingSteps.Count >= 3) completedFields++;

        return Math.Round((decimal)completedFields / totalFields * 100, 1);
    }

    private static List<string> GetMissingProfileElements(MockUser user)
    {
        var missing = new List<string>();

        if (string.IsNullOrEmpty(user.Bio)) missing.Add("Bio");
        if (string.IsNullOrEmpty(user.Company)) missing.Add("Company");
        if (string.IsNullOrEmpty(user.JobTitle)) missing.Add("Job Title");
        if (string.IsNullOrEmpty(user.Location)) missing.Add("Location");
        if (!user.Skills.Any()) missing.Add("Skills");
        if (!user.Interests.Any()) missing.Add("Interests");
        if (user.AvailableFunding == 0) missing.Add("Available Funding");
        if (string.IsNullOrEmpty(user.LinkedInProfile)) missing.Add("LinkedIn Profile");

        return missing;
    }

    private static List<string> GeneratePersonalizedTips(MockUser user, decimal completionPercentage)
    {
        var tips = new List<string>();

        if (completionPercentage < 80)
        {
            tips.Add("Complete your profile to get better idea recommendations");
        }

        if (!user.Skills.Any())
        {
            tips.Add("Add your skills to connect with like-minded entrepreneurs");
        }

        if (user.TotalIdeasSubmitted == 0)
        {
            tips.Add("Submit your first idea to start your entrepreneurial journey");
        }

        if (user.CompletedOnboardingSteps.Count < 3)
        {
            tips.Add("Complete the onboarding process to unlock all features");
        }

        return tips.Take(3).ToList();
    }

    private static string GetNextRecommendedAction(MockUser user, List<string> missingElements)
    {
        if (user.CompletedOnboardingSteps.Count < 3)
        {
            return "Complete your onboarding process";
        }

        if (missingElements.Any())
        {
            return $"Add your {missingElements.First().ToLower()} to improve your profile";
        }

        if (user.TotalIdeasSubmitted == 0)
        {
            return "Explore business scenarios and submit your first idea";
        }

        return "Connect with other entrepreneurs in your industry";
    }

    private void InitializeUserActivities()
    {
        // Initialize with some sample activities for demo users
        var demoUserId = "demo-user-id"; // This would be the actual user ID
        
        _userActivities[demoUserId] = new List<UserActivity>
        {
            new UserActivity { Type = "ProfileUpdated", Description = "Updated profile information", Timestamp = DateTime.UtcNow.AddDays(-2) },
            new UserActivity { Type = "ScenarioViewed", Description = "Viewed EcoEats Delivery scenario", Timestamp = DateTime.UtcNow.AddDays(-1) },
            new UserActivity { Type = "BadgeEarned", Description = "Earned Welcome Aboard badge", Timestamp = DateTime.UtcNow.AddDays(-3) }
        };
    }
}