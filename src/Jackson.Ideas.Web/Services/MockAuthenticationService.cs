using Jackson.Ideas.Core.DTOs.Auth;
using System.Security.Claims;

namespace Jackson.Ideas.Web.Services;

/// <summary>
/// Mock authentication service that bypasses login for development/demo purposes
/// </summary>
public class MockAuthenticationService : IAuthenticationService
{
    private readonly ILogger<MockAuthenticationService> _logger;
    private bool _isAuthenticated = true; // Always authenticated
    private readonly UserInfo _mockUser;

    public MockAuthenticationService(ILogger<MockAuthenticationService> logger)
    {
        _logger = logger;
        
        // Create a mock demo user
        _mockUser = new UserInfo
        {
            Id = "demo-user-123",
            Email = "demo@ideasmatter.com",
            Name = "Demo User",
            Role = "User",
            IsVerified = true,
            Permissions = new[] { "create_ideas", "view_research", "export_reports" }
        };
    }

    public Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Mock login requested for {Email}", request.Email);
        
        // Always return success
        return Task.FromResult(new AuthResponse
        {
            IsSuccess = true,
            Message = "Mock login successful",
            AccessToken = "mock-access-token",
            RefreshToken = "mock-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = _mockUser
        });
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Mock registration requested for {Email}", request.Email);
        
        // Always return success
        return Task.FromResult(new AuthResponse
        {
            IsSuccess = true,
            Message = "Mock registration successful",
            AccessToken = "mock-access-token",
            RefreshToken = "mock-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = _mockUser
        });
    }

    public Task<AuthResponse> RefreshTokenAsync()
    {
        return Task.FromResult(new AuthResponse
        {
            IsSuccess = true,
            Message = "Mock token refresh successful",
            AccessToken = "mock-access-token-refreshed",
            RefreshToken = "mock-refresh-token-refreshed",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = _mockUser
        });
    }

    public Task<bool> LogoutAsync()
    {
        _logger.LogInformation("Mock logout requested");
        // In a real implementation, this would clear tokens
        // For mock, we just return success
        return Task.FromResult(true);
    }

    public Task<AuthResponse> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        _logger.LogInformation("Mock password change requested");
        
        return Task.FromResult(new AuthResponse
        {
            IsSuccess = true,
            Message = "Mock password change successful"
        });
    }

    public Task<UserInfo?> GetCurrentUserAsync()
    {
        return Task.FromResult<UserInfo?>(_mockUser);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_isAuthenticated);
    }

    public ClaimsPrincipal GetClaimsPrincipal()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _mockUser.Id),
            new Claim(ClaimTypes.Name, _mockUser.Name),
            new Claim(ClaimTypes.Email, _mockUser.Email),
            new Claim(ClaimTypes.Role, _mockUser.Role),
            new Claim("email_verified", _mockUser.IsVerified.ToString())
        };

        var identity = new ClaimsIdentity(claims, "mock");
        return new ClaimsPrincipal(identity);
    }
}