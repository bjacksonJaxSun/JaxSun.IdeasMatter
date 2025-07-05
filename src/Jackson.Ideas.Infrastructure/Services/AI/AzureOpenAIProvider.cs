using Jackson.Ideas.Core.Interfaces.AI;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Jackson.Ideas.Infrastructure.Services.AI;

public class AzureOpenAIProvider : IBaseAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureOpenAIProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;

    public string Name => "Azure OpenAI";
    public AIProviderType Type => AIProviderType.AzureOpenAI;
    public bool IsAvailable { get; private set; }
    public string ProviderType => "AzureOpenAI";
    public string Model => _deploymentName;

    public AzureOpenAIProvider(
        HttpClient httpClient,
        ILogger<AzureOpenAIProvider> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        _apiKey = _configuration["AzureOpenAI:ApiKey"] ?? string.Empty;
        _endpoint = _configuration["AzureOpenAI:Endpoint"] ?? string.Empty;
        _deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4";
        
        IsAvailable = !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_endpoint);
        
        if (IsAvailable)
        {
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default, params object[] args)
    {
        var parameters = args.Length > 0 && args[0] is Dictionary<string, object> dict ? dict : null;
        return await GenerateTextAsync(prompt, parameters);
    }

    public async Task<string> GenerateTextAsync(string prompt, Dictionary<string, object>? parameters = null)
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException("Azure OpenAI provider is not available. Check configuration.");
        }

        try
        {
            var maxTokens = parameters?.GetValueOrDefault("max_tokens", 2000) ?? 2000;
            var temperature = parameters?.GetValueOrDefault("temperature", 0.7) ?? 0.7;

            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful AI assistant for business analysis and market research." },
                    new { role = "user", content = prompt }
                },
                max_tokens = Convert.ToInt32(maxTokens),
                temperature = Convert.ToDouble(temperature),
                top_p = 0.95,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version=2024-02-01";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var message = result
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                _logger.LogInformation("Azure OpenAI request completed successfully");
                return message ?? string.Empty;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Azure OpenAI API error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure OpenAI API");
            throw;
        }
    }

    public async Task<string> AnalyzeMarketAsync(string ideaDescription, Dictionary<string, object>? parameters = null)
    {
        var prompt = $@"
Conduct a comprehensive market analysis for the following business idea:

Business Idea: {ideaDescription}

Please provide analysis in the following structure:
1. Market Overview & Size
2. Target Customer Segments
3. Competitive Landscape
4. Market Trends & Opportunities
5. Potential Challenges & Risks
6. Revenue Model Recommendations
7. Go-to-Market Strategy Suggestions

Format the response as detailed, actionable insights suitable for business planning.
";

        return await GenerateTextAsync(prompt, parameters);
    }

    public async Task<string> GenerateSwotAnalysisAsync(string ideaDescription, Dictionary<string, object>? parameters = null)
    {
        var prompt = $@"
Create a comprehensive SWOT analysis for the following business idea:

Business Idea: {ideaDescription}

Please provide a detailed SWOT analysis with:

STRENGTHS:
- Internal positive factors and advantages
- Unique capabilities and resources
- Competitive advantages

WEAKNESSES:
- Internal limitations and disadvantages
- Areas needing improvement
- Resource constraints

OPPORTUNITIES:
- External positive factors
- Market trends and gaps
- Growth potential areas

THREATS:
- External negative factors
- Competitive threats
- Market risks and challenges

For each category, provide 3-5 specific, actionable points with brief explanations.
Format as structured analysis suitable for strategic planning.
";

        return await GenerateTextAsync(prompt, parameters);
    }

    public async Task<string> GenerateBusinessPlanAsync(string ideaDescription, Dictionary<string, object>? parameters = null)
    {
        var prompt = $@"
Generate a comprehensive business plan for the following idea:

Business Idea: {ideaDescription}

Please structure the business plan with these sections:
1. Executive Summary
2. Market Analysis
3. Target Customers & Value Proposition
4. Competitive Analysis
5. Marketing & Sales Strategy
6. Operations Plan
7. Financial Projections (3-year)
8. Risk Analysis & Mitigation
9. Implementation Timeline
10. Success Metrics & KPIs

Provide detailed, practical content for each section suitable for investors and stakeholders.
";

        return await GenerateTextAsync(prompt, parameters);
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return await GenerateAsync(prompt, cancellationToken);
    }

    public async Task<bool> ValidateApiKeyAsync(CancellationToken cancellationToken = default)
    {
        return await ValidateConnectionAsync();
    }

    public async Task<bool> ValidateConnectionAsync()
    {
        if (!IsAvailable)
        {
            return false;
        }

        try
        {
            var testPrompt = "Hello, this is a connection test. Please respond with 'Connection successful'.";
            var response = await GenerateTextAsync(testPrompt, new Dictionary<string, object> { ["max_tokens"] = 50 });
            
            _logger.LogInformation("Azure OpenAI connection validation successful");
            return !string.IsNullOrEmpty(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI connection validation failed");
            return false;
        }
    }

    public Dictionary<string, object> GetCapabilities()
    {
        return new Dictionary<string, object>
        {
            ["name"] = Name,
            ["type"] = Type.ToString(),
            ["supports_chat"] = true,
            ["supports_completion"] = true,
            ["supports_streaming"] = false,
            ["max_tokens"] = 4096,
            ["supports_functions"] = true,
            ["deployment_name"] = _deploymentName,
            ["api_version"] = "2024-02-01"
        };
    }
}