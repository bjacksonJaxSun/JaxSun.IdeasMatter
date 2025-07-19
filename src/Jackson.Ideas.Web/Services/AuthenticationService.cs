using Jackson.Ideas.Core.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Jackson.Ideas.Web.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<bool> LogoutAsync();
    Task<AuthResponse> RefreshTokenAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<AuthResponse> ChangePasswordAsync(string currentPassword, string newPassword);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        HttpClient httpClient,
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _logger = logger;
        _configuration = configuration;

        // Configure HttpClient base address
        var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (authResponse?.IsSuccess == true && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                await SetAuthenticationAsync(authResponse);
            }

            return authResponse ?? new AuthResponse { IsSuccess = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthResponse { IsSuccess = false, Message = "An error occurred during login" };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/auth/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (authResponse?.IsSuccess == true && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                await SetAuthenticationAsync(authResponse);
            }

            return authResponse ?? new AuthResponse { IsSuccess = false, Message = "Registration failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new AuthResponse { IsSuccess = false, Message = "An error occurred during registration" };
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var refreshToken = _authStateProvider.GetRefreshTokenFromCookie();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var logoutRequest = new { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(logoutRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync("/api/v1/auth/logout", content);
            }

            _authStateProvider.RemoveTokenFromCookie();
            _authStateProvider.MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = _authStateProvider.GetRefreshTokenFromCookie();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return new AuthResponse { IsSuccess = false, Message = "No refresh token available" };
            }

            var refreshRequest = new { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(refreshRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/auth/refresh", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (authResponse?.IsSuccess == true && !string.IsNullOrEmpty(authResponse.AccessToken))
            {
                await SetAuthenticationAsync(authResponse);
            }

            return authResponse ?? new AuthResponse { IsSuccess = false, Message = "Token refresh failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResponse { IsSuccess = false, Message = "An error occurred during token refresh" };
        }
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/auth/me");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserInfo>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
        }

        return null;
    }

    public async Task<AuthResponse> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var request = new { CurrentPassword = currentPassword, NewPassword = newPassword };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/auth/change-password", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new AuthResponse { IsSuccess = false, Message = "Password change failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return new AuthResponse { IsSuccess = false, Message = "An error occurred while changing password" };
        }
    }

    private async Task SetAuthenticationAsync(AuthResponse authResponse)
    {
        // Set tokens in cookies
        _authStateProvider.SetTokenInCookie(authResponse.AccessToken!, authResponse.RefreshToken!);

        // Set authorization header for HttpClient
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

        // Create claims from user info
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authResponse.User!.Id),
            new(ClaimTypes.Email, authResponse.User.Email),
            new(ClaimTypes.Name, authResponse.User.Name),
            new(ClaimTypes.Role, authResponse.User.Role),
            new("is_verified", authResponse.User.IsVerified.ToString())
        };

        // Add permission claims
        foreach (var permission in authResponse.User.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        _authStateProvider.MarkUserAsAuthenticated(user);
    }
}