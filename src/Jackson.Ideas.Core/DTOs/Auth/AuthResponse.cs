namespace Jackson.Ideas.Core.DTOs.Auth;

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public string? AccessToken { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string? Picture { get; set; }
    
    public string Role { get; set; } = string.Empty;
    
    public bool IsVerified { get; set; }
    
    public string[] Permissions { get; set; } = Array.Empty<string>();
}

public class AdminStatsResponse
{
    public int TotalUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int UnverifiedUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public int RecentRegistrations { get; set; }
}