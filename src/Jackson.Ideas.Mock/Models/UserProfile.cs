namespace Jackson.Ideas.Mock.Models;

public class UserProfile
{
    public MockUser User { get; set; } = new();
    public UserAnalytics Analytics { get; set; } = new();
    public List<UserActivity> RecentActivity { get; set; } = new();
    public List<string> SuggestedScenarios { get; set; } = new();
    public List<string> RecommendedConnections { get; set; } = new();
    public UserInsights Insights { get; set; } = new();
}

public class UserAnalytics
{
    public int TotalSessions { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public DateTime LastActiveDate { get; set; }
    public int IdeasViewed { get; set; }
    public int IdeasBookmarked { get; set; }
    public int ForumsPostsCreated { get; set; }
    public int NetworkConnections { get; set; }
    public decimal EngagementScore { get; set; }
    public List<string> TopInteractionTypes { get; set; } = new();
    public Dictionary<string, int> MonthlyActivity { get; set; } = new();
}

public class UserActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = ""; // ScenarioViewed, ProfileUpdated, BadgeEarned, etc.
    public string Description { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class UserInsights
{
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> GrowthOpportunities { get; set; } = new();
    public List<string> PersonalizedTips { get; set; } = new();
    public string NextRecommendedAction { get; set; } = "";
    public decimal ProfileCompletionPercentage { get; set; }
    public List<string> MissingProfileElements { get; set; } = new();
}