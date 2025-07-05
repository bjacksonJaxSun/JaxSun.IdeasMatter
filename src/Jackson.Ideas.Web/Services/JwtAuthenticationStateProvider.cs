using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Jackson.Ideas.Web.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<JwtAuthenticationStateProvider> _logger;
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public JwtAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<JwtAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var user = await GetUserFromTokenAsync();
            var result = new AuthenticationState(user);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private async Task<ClaimsPrincipal> GetUserFromTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        // Try to get token from cookie first
        var token = httpContext.Request.Cookies["jwt"];
        
        if (string.IsNullOrEmpty(token))
        {
            // Try to get from Authorization header as fallback
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        return GetClaimsFromToken(token);
    }

    private ClaimsPrincipal GetClaimsFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            // Check if token is expired
            if (jsonToken.ValidTo <= DateTime.UtcNow)
            {
                _logger.LogInformation("JWT token is expired");
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = new ClaimsIdentity(jsonToken.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JWT token");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

    public void MarkUserAsAuthenticated(ClaimsPrincipal user)
    {
        _cachedUser = user;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    public void SetTokenInCookie(string token, string refreshToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60) // Match token expiration
            };

            httpContext.Response.Cookies.Append("jwt", token, cookieOptions);
            
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7) // Match refresh token expiration
            };
            
            httpContext.Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
        }
    }

    public void RemoveTokenFromCookie()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Delete("jwt");
            httpContext.Response.Cookies.Delete("refresh_token");
        }
    }

    public string? GetRefreshTokenFromCookie()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Cookies["refresh_token"];
    }
}