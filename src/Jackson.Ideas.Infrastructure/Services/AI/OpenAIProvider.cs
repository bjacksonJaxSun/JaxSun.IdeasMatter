using Jackson.Ideas.Core.Interfaces.AI;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Jackson.Ideas.Infrastructure.Services.AI;

public class OpenAIProvider : IBaseAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly double _temperature;
    private readonly int _maxTokens;

    public OpenAIProvider(HttpClient httpClient, string apiKey, string model = "gpt-4-turbo-preview", double temperature = 0.7, int maxTokens = 2000)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _model = model;
        _temperature = temperature;
        _maxTokens = maxTokens;

        _httpClient.BaseAddress ??= new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public string ProviderType => "openai";
    public string Model => _model;

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default, params object[] args)
    {
        var temperature = args.Length > 0 && args[0] is double temp ? temp : _temperature;
        var maxTokens = args.Length > 1 && args[1] is int tokens ? tokens : _maxTokens;

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature,
            max_tokens = maxTokens
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return jsonResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new Exception($"OpenAI API error: {ex.Message}", ex);
        }
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return await GenerateAsync(prompt, cancellationToken);
    }

    public async Task<bool> ValidateApiKeyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = "Hi" }
                },
                max_tokens = 5
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}