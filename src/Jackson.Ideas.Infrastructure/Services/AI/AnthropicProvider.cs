using Jackson.Ideas.Core.Interfaces.AI;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Jackson.Ideas.Infrastructure.Services.AI;

public class AnthropicProvider : IBaseAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly double _temperature;
    private readonly int _maxTokens;

    public AnthropicProvider(HttpClient httpClient, string apiKey, string model = "claude-3-opus-20240229", double temperature = 0.7, int maxTokens = 2000)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _model = model;
        _temperature = temperature;
        _maxTokens = maxTokens;

        _httpClient.BaseAddress ??= new Uri("https://api.anthropic.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public string ProviderType => "anthropic";
    public string Model => _model;

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default, params object[] args)
    {
        var temperature = args.Length > 0 && args[0] is double temp ? temp : _temperature;
        var maxTokens = args.Length > 1 && args[1] is int tokens ? tokens : _maxTokens;

        var requestBody = new
        {
            model = _model,
            max_tokens = maxTokens,
            temperature,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("messages", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return jsonResponse
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new Exception($"Anthropic API error: {ex.Message}", ex);
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
                model = "claude-3-haiku-20240307",
                max_tokens = 5,
                messages = new[]
                {
                    new { role = "user", content = "Hi" }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("messages", content, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}