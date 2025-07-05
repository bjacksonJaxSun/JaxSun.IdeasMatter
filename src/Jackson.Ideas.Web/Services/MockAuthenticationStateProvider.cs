using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Jackson.Ideas.Web.Services;

/// <summary>
/// Mock authentication state provider that always returns an authenticated user
/// </summary>
public class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILogger<MockAuthenticationStateProvider> _logger;
    private readonly ClaimsPrincipal _mockUser;

    public MockAuthenticationStateProvider(ILogger<MockAuthenticationStateProvider> logger)
    {
        _logger = logger;
        
        // Create a mock authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "demo-user-123"),
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Email, "demo@ideasmatter.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("email_verified", "true")
        };

        var identity = new ClaimsIdentity(claims, "mock");
        _mockUser = new ClaimsPrincipal(identity);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _logger.LogDebug("Mock authentication state requested - returning authenticated user");
        
        // Always return an authenticated state
        var authState = new AuthenticationState(_mockUser);
        return Task.FromResult(authState);
    }

    // Method to notify authentication state changed (for compatibility)
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}