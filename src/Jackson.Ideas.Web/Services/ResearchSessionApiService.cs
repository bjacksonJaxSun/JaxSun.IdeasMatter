using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Jackson.Ideas.Web.Services;

public interface IResearchSessionApiService
{
    Task<ResearchSession?> CreateSessionAsync(CreateSessionRequest request);
    Task<ResearchSession?> GetSessionAsync(Guid sessionId);
    Task<List<ResearchSession>> GetUserSessionsAsync();
    Task<ResearchSession?> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<ResearchSession?> AddInsightAsync(Guid sessionId, AddInsightRequest request);
    Task<ResearchSession?> AddOptionAsync(Guid sessionId, AddOptionRequest request);
    Task<ResearchSession?> UpdateSessionStatusAsync(Guid sessionId, UpdateStatusRequest request);
    Task<bool> StartResearchExecutionAsync(Guid sessionId);
    Task<object?> GetResearchProgressAsync(Guid sessionId);
}

public class ResearchSessionApiService : IResearchSessionApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResearchSessionApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ResearchSessionApiService(
        HttpClient httpClient,
        ILogger<ResearchSessionApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ResearchSession?> CreateSessionAsync(CreateSessionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1/researchsession", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ResearchSession>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create session: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating research session");
            return null;
        }
    }

    public async Task<ResearchSession?> GetSessionAsync(Guid sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/researchsession/{sessionId}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ResearchSession>(responseContent, _jsonOptions);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get session: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting research session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<List<ResearchSession>> GetUserSessionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/researchsession");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ResearchSession>>(responseContent, _jsonOptions) ?? new List<ResearchSession>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get user sessions: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new List<ResearchSession>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sessions");
            return new List<ResearchSession>();
        }
    }

    public async Task<ResearchSession?> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/v1/researchsession/{sessionId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ResearchSession>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update session: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating research session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/researchsession/{sessionId}");
            
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete session: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting research session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<ResearchSession?> AddInsightAsync(Guid sessionId, AddInsightRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/v1/researchsession/{sessionId}/insights", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ResearchSession>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to add insight: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding insight to session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<ResearchSession?> AddOptionAsync(Guid sessionId, AddOptionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/v1/researchsession/{sessionId}/options", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ResearchSession>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to add option: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding option to session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<ResearchSession?> UpdateSessionStatusAsync(Guid sessionId, UpdateStatusRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/v1/researchsession/{sessionId}/status", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ResearchSession>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update session status: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session status {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<bool> StartResearchExecutionAsync(Guid sessionId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/researchsession/{sessionId}/execute", null);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to start research execution: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting research execution for session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<object?> GetResearchProgressAsync(Guid sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/researchsession/{sessionId}/progress");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get research progress: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting research progress for session {SessionId}", sessionId);
            return null;
        }
    }
}