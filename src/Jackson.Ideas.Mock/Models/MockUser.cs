namespace Jackson.Ideas.Mock.Models;

public class MockUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public List<string> Skills { get; set; } = new();
    public List<string> Interests { get; set; } = new();
    public decimal AvailableFunding { get; set; }
    public int TimeCommitmentHours { get; set; }
    public List<string> CompletedOnboardingSteps { get; set; } = new();
    public string PreferredScenarioId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public string Role { get; set; } = "User"; // User, Admin, SystemAdmin
    public string ProfilePicture { get; set; } = "";
    public string Bio { get; set; } = "";
    public string LinkedInProfile { get; set; } = "";
    public string GitHubProfile { get; set; } = "";
    public string Company { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Location { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public UserPreferences Preferences { get; set; } = new();
    public List<string> AchievementBadges { get; set; } = new();
    public int TotalIdeasSubmitted { get; set; }
    public int TotalIdeasCompleted { get; set; }
    public decimal TotalInvestmentAmount { get; set; }
}

public class UserPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool MarketingEmails { get; set; } = false;
    public string PreferredCommunicationStyle { get; set; } = "Professional"; // Casual, Professional, Technical
    public List<string> IndustryPreferences { get; set; } = new();
    public string RiskTolerance { get; set; } = "Medium"; // Low, Medium, High
    public bool ShareProgressPublicly { get; set; } = false;
    public string DashboardLayout { get; set; } = "Default"; // Default, Compact, Detailed
}