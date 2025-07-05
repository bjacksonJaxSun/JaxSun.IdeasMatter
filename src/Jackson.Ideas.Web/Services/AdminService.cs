using Jackson.Ideas.Core.DTOs.Auth;
using System.Text;
using System.Text.Json;

namespace Jackson.Ideas.Web.Services;

public interface IAdminService
{
    Task<List<UserInfo>> GetAllUsersAsync();
    Task<UserInfo?> GetUserByIdAsync(string id);
    Task<AuthResponse> UpdateUserRoleAsync(string id, string role);
    Task<AuthResponse> ToggleUserVerificationAsync(string id);
    Task<AdminStatsResponse?> GetStatsAsync();
}

public class AdminService : IAdminService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminService> _logger;

    public AdminService(HttpClient httpClient, ILogger<AdminService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<UserInfo>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/users");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<UserInfo>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new List<UserInfo>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
        }

        return new List<UserInfo>();
    }

    public async Task<UserInfo?> GetUserByIdAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/admin/users/{id}");
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
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
        }

        return null;
    }

    public async Task<AuthResponse> UpdateUserRoleAsync(string id, string role)
    {
        try
        {
            var request = new { Role = role };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/v1/admin/users/{id}/role", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new AuthResponse { IsSuccess = false, Message = "Role update failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for ID: {UserId}", id);
            return new AuthResponse { IsSuccess = false, Message = "An error occurred while updating user role" };
        }
    }

    public async Task<AuthResponse> ToggleUserVerificationAsync(string id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/admin/users/{id}/toggle-verification", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new AuthResponse { IsSuccess = false, Message = "Verification toggle failed" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user verification for ID: {UserId}", id);
            return new AuthResponse { IsSuccess = false, Message = "An error occurred while toggling user verification" };
        }
    }

    public async Task<AdminStatsResponse?> GetStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/admin/stats");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AdminStatsResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin stats");
        }

        return null;
    }
}