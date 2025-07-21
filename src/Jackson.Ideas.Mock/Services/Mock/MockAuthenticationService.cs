using Jackson.Ideas.Mock.Models;
using Jackson.Ideas.Mock.Services.Interfaces;

namespace Jackson.Ideas.Mock.Services.Mock;

public class MockAuthenticationService : IMockAuthenticationService
{
    private readonly List<MockUser> _users;
    private readonly Dictionary<string, UserSession> _sessions;
    private readonly Dictionary<string, string> _passwordResetTokens;

    public MockAuthenticationService()
    {
        _users = GenerateMockUsers();
        _sessions = new Dictionary<string, UserSession>();
        _passwordResetTokens = new Dictionary<string, string>();
    }

    public async Task<MockUser?> LoginAsync(string email, string password)
    {
        await Task.Delay(100);
        
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        if (user != null)
        {
            // In a real app, you'd validate the password hash
            // For mock purposes, accept any password
            user.LastLoginAt = DateTime.UtcNow;
            user.LastActivityAt = DateTime.UtcNow;
            
            return user;
        }
        
        return null;
    }

    public async Task<MockUser?> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        await Task.Delay(150);
        
        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return null; // User already exists
        }
        
        var newUser = new MockUser
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = HashPassword(password),
            EmailVerified = true, // Auto-verify for mock
            AchievementBadges = new List<string> { "Welcome Aboard" }
        };
        
        _users.Add(newUser);
        return newUser;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        await Task.Delay(50);
        
        if (_sessions.TryGetValue(token, out var session))
        {
            if (session.IsActive && session.ExpiresAt > DateTime.UtcNow)
            {
                session.LastActivity = DateTime.UtcNow;
                return true;
            }
            else
            {
                session.IsActive = false;
            }
        }
        
        return false;
    }

    public async Task LogoutAsync(string userId)
    {
        await Task.Delay(50);
        
        var sessionsToInvalidate = _sessions.Where(kvp => kvp.Value.UserId == userId).ToList();
        foreach (var session in sessionsToInvalidate)
        {
            session.Value.IsActive = false;
        }
    }

    public async Task<MockUser?> GetUserByIdAsync(string userId)
    {
        await Task.Delay(50);
        return _users.FirstOrDefault(u => u.Id == userId && u.IsActive);
    }

    public async Task<MockUser?> GetUserByEmailAsync(string email)
    {
        await Task.Delay(50);
        return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
    }

    public async Task<MockUser?> GetUserByTokenAsync(string token)
    {
        await Task.Delay(50);
        
        if (_sessions.TryGetValue(token, out var session) && session.IsActive && session.ExpiresAt > DateTime.UtcNow)
        {
            return await GetUserByIdAsync(session.UserId);
        }
        
        return null;
    }

    public async Task<string> GenerateTokenAsync(string userId)
    {
        await Task.Delay(50);
        
        var token = Guid.NewGuid().ToString();
        var session = new UserSession
        {
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            LastActivity = DateTime.UtcNow,
            IsActive = true,
            DeviceInfo = "Mock Device",
            SessionType = "Standard"
        };
        
        _sessions[token] = session;
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email)
    {
        await Task.Delay(100);
        
        var user = await GetUserByEmailAsync(email);
        if (user != null)
        {
            var resetToken = Guid.NewGuid().ToString();
            _passwordResetTokens[resetToken] = user.Id;
            
            // In a real app, you'd send an email with the reset token
            return true;
        }
        
        return false;
    }

    public async Task<bool> VerifyPasswordResetTokenAsync(string token)
    {
        await Task.Delay(50);
        return _passwordResetTokens.ContainsKey(token);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        await Task.Delay(100);
        
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            // In a real app, you'd validate the old password
            user.PasswordHash = HashPassword(newPassword);
            return true;
        }
        
        return false;
    }

    public async Task<List<MockUser>> GetAllUsersAsync()
    {
        await Task.Delay(100);
        return _users.Where(u => u.IsActive).ToList();
    }

    public async Task<bool> UpdateUserAsync(MockUser user)
    {
        await Task.Delay(100);
        
        var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser != null)
        {
            var index = _users.IndexOf(existingUser);
            _users[index] = user;
            user.LastActivityAt = DateTime.UtcNow;
            return true;
        }
        
        return false;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        await Task.Delay(100);
        
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.IsActive = false;
            await InvalidateAllUserTokensAsync(userId);
            return true;
        }
        
        return false;
    }

    public async Task<UserSession?> GetUserSessionAsync(string token)
    {
        await Task.Delay(50);
        
        if (_sessions.TryGetValue(token, out var session))
        {
            return session;
        }
        
        return null;
    }

    public async Task InvalidateAllUserTokensAsync(string userId)
    {
        await Task.Delay(50);
        
        var userSessions = _sessions.Where(kvp => kvp.Value.UserId == userId).ToList();
        foreach (var session in userSessions)
        {
            session.Value.IsActive = false;
        }
    }

    private static List<MockUser> GenerateMockUsers()
    {
        return new List<MockUser>
        {
            new MockUser
            {
                Email = "demo@ideasmatter.com",
                FirstName = "Alex",
                LastName = "Demo",
                PasswordHash = HashPassword("demo123"),
                Skills = new List<string> { "Marketing", "Business Development", "Strategic Planning" },
                Interests = new List<string> { "Technology", "Sustainability", "Social Impact" },
                AvailableFunding = 50000,
                TimeCommitmentHours = 20,
                CompletedOnboardingSteps = new List<string> { "Profile", "Preferences", "Skills Assessment" },
                Role = "User",
                EmailVerified = true,
                Bio = "Passionate entrepreneur focused on sustainable technology solutions.",
                Company = "GreenTech Innovations",
                JobTitle = "Founder & CEO",
                Location = "San Francisco, CA",
                TimeZone = "PST",
                AchievementBadges = new List<string> { "Welcome Aboard", "Profile Complete", "First Idea" },
                TotalIdeasSubmitted = 3,
                TotalIdeasCompleted = 1,
                TotalInvestmentAmount = 25000,
                Preferences = new UserPreferences
                {
                    EmailNotifications = true,
                    PushNotifications = true,
                    MarketingEmails = true,
                    PreferredCommunicationStyle = "Professional",
                    IndustryPreferences = new List<string> { "Technology", "Sustainability", "Food & Beverage" },
                    RiskTolerance = "Medium",
                    ShareProgressPublicly = true,
                    DashboardLayout = "Default"
                }
            },
            new MockUser
            {
                Email = "entrepreneur@example.com",
                FirstName = "Jordan",
                LastName = "Smith",
                PasswordHash = HashPassword("password123"),
                Skills = new List<string> { "Software Development", "Product Management", "UI/UX Design" },
                Interests = new List<string> { "AI", "EdTech", "FinTech" },
                AvailableFunding = 100000,
                TimeCommitmentHours = 40,
                CompletedOnboardingSteps = new List<string> { "Profile", "Preferences", "Goals", "Skills Assessment", "Investment Profile" },
                Role = "User",
                EmailVerified = true,
                Bio = "Full-stack developer turned entrepreneur with a passion for AI-powered education.",
                Company = "EduTech Solutions",
                JobTitle = "CTO & Co-Founder",
                Location = "Austin, TX",
                TimeZone = "CST",
                LinkedInProfile = "https://linkedin.com/in/jordansmith",
                GitHubProfile = "https://github.com/jordansmith",
                AchievementBadges = new List<string> { "Welcome Aboard", "Profile Complete", "Power User", "Mentor", "Investor" },
                TotalIdeasSubmitted = 8,
                TotalIdeasCompleted = 3,
                TotalInvestmentAmount = 75000,
                Preferences = new UserPreferences
                {
                    EmailNotifications = true,
                    PushNotifications = false,
                    MarketingEmails = false,
                    PreferredCommunicationStyle = "Technical",
                    IndustryPreferences = new List<string> { "Education Technology", "Artificial Intelligence", "Software as a Service" },
                    RiskTolerance = "High",
                    ShareProgressPublicly = false,
                    DashboardLayout = "Detailed"
                }
            },
            new MockUser
            {
                Email = "admin@ideasmatter.com",
                FirstName = "Sarah",
                LastName = "Chen",
                PasswordHash = HashPassword("admin123"),
                Skills = new List<string> { "Business Strategy", "Operations Management", "Data Analysis" },
                Interests = new List<string> { "Platform Growth", "User Experience", "Market Research" },
                AvailableFunding = 0,
                TimeCommitmentHours = 40,
                CompletedOnboardingSteps = new List<string> { "Profile", "Admin Setup" },
                Role = "Admin",
                EmailVerified = true,
                Bio = "Platform administrator focused on creating the best experience for Ideas Matter users.",
                Company = "Ideas Matter Inc.",
                JobTitle = "Platform Operations Manager",
                Location = "Remote",
                TimeZone = "EST",
                AchievementBadges = new List<string> { "Platform Administrator", "Community Builder" },
                TotalIdeasSubmitted = 0,
                TotalIdeasCompleted = 0,
                TotalInvestmentAmount = 0,
                Preferences = new UserPreferences
                {
                    EmailNotifications = true,
                    PushNotifications = true,
                    MarketingEmails = true,
                    PreferredCommunicationStyle = "Professional",
                    IndustryPreferences = new List<string> { "Platform Management", "User Experience" },
                    RiskTolerance = "Low",
                    ShareProgressPublicly = false,
                    DashboardLayout = "Compact"
                }
            }
        };
    }

    private static string HashPassword(string password)
    {
        // In a real application, use a proper password hashing library like BCrypt
        // For mock purposes, we'll use a simple hash
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"hashed_{password}"));
    }
}