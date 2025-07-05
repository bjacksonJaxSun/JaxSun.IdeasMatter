using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Jackson.Ideas.Web.Services;

public class MockAuthenticationSchemeOptions : AuthenticationSchemeOptions { }

public class MockAuthenticationHandler : AuthenticationHandler<MockAuthenticationSchemeOptions>
{
    private readonly ILogger<MockAuthenticationHandler> _logger;

    public MockAuthenticationHandler(
        IOptionsMonitor<MockAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _logger = logger.CreateLogger<MockAuthenticationHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _logger.LogDebug("Mock authentication handler invoked");

        // Create mock claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "demo-user-123"),
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Email, "demo@ideasmatter.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("email_verified", "true")
        };

        var identity = new ClaimsIdentity(claims, "MockScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "MockScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // In mock mode, we don't challenge - just return success
        Context.Response.StatusCode = 200;
        return Task.CompletedTask;
    }
}