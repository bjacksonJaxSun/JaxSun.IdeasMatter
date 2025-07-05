using Jackson.Ideas.Core.Interfaces.AI;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Jackson.Ideas.Infrastructure.Services.AI;

public class GeminiProvider : IBaseAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _model;

    public string Name => "Google Gemini";
    public AIProviderType Type => AIProviderType.Gemini;
    public bool IsAvailable { get; private set; }
    public string ProviderType => "Gemini";
    public string Model => _model;

    public GeminiProvider(
        HttpClient httpClient,
        ILogger<GeminiProvider> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        _apiKey = _configuration["Gemini:ApiKey"] ?? string.Empty;
        _model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        
        IsAvailable = !string.IsNullOrEmpty(_apiKey);
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
            throw new InvalidOperationException("Gemini provider is not available. Check configuration.");
        }

        try
        {
            var temperature = parameters?.GetValueOrDefault("temperature", 0.7) ?? 0.7;
            var maxTokens = parameters?.GetValueOrDefault("max_tokens", 2048) ?? 2048;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = Convert.ToDouble(temperature),
                    maxOutputTokens = Convert.ToInt32(maxTokens),
                    topP = 0.95,
                    topK = 64
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var text = result
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                _logger.LogInformation("Gemini request completed successfully");
                return text ?? string.Empty;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Gemini API error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            throw;
        }
    }

    public async Task<string> AnalyzeMarketAsync(string ideaDescription, Dictionary<string, object>? parameters = null)
    {
        var prompt = $@"
You are a expert market research analyst. Analyze the following business idea comprehensively:

Business Idea: {ideaDescription}

Provide a structured market analysis covering:

1. MARKET SIZING & OPPORTUNITY
   - Total Addressable Market (TAM)
   - Serviceable Addressable Market (SAM)
   - Serviceable Obtainable Market (SOM)
   - Growth rate and trends

2. TARGET CUSTOMER ANALYSIS
   - Primary customer segments
   - Customer personas and demographics
   - Pain points and needs
   - Buying behavior patterns

3. COMPETITIVE LANDSCAPE
   - Direct competitors analysis
   - Indirect competitors and substitutes
   - Competitive advantages and positioning
   - Market share distribution

4. MARKET TRENDS & DRIVERS
   - Technology trends affecting the market
   - Regulatory environment
   - Economic factors
   - Social and cultural influences

5. BARRIERS TO ENTRY
   - Technical barriers
   - Financial requirements
   - Regulatory hurdles
   - Market saturation levels

6. REVENUE MODEL OPPORTUNITIES
   - Potential pricing strategies
   - Revenue streams identification
   - Monetization approaches
   - Scaling potential

Provide specific, data-driven insights where possible and actionable recommendations.
";

        return await GenerateTextAsync(prompt, parameters);
    }

    public async Task<string> GenerateSwotAnalysisAsync(string ideaDescription, Dictionary<string, object>? parameters = null)
    {
        var prompt = $@"
As a strategic business consultant, perform a thorough SWOT analysis for this business idea:

Business Idea: {ideaDescription}

Structure your analysis as follows:

STRENGTHS (Internal Positive Factors):
- List 4-6 key internal advantages
- Include unique capabilities, resources, or market position
- Focus on what gives competitive advantage
- Consider team, technology, partnerships, IP, etc.

WEAKNESSES (Internal Negative Factors):
- Identify 4-6 key internal limitations
- Include resource constraints, skill gaps, or operational challenges
- Consider what competitors do better
- Focus on areas needing improvement

OPPORTUNITIES (External Positive Factors):
- Highlight 4-6 market opportunities
- Include market trends, gaps, or emerging needs
- Consider technological advances, regulatory changes
- Focus on growth and expansion potential

THREATS (External Negative Factors):
- Identify 4-6 key external risks
- Include competitive threats, market changes, economic factors
- Consider regulatory risks, technology disruption
- Focus on factors that could harm success

STRATEGIC IMPLICATIONS:
- Key strategic insights from the SWOT
- Priority actions to leverage strengths and opportunities
- Critical weaknesses and threats to address
- Overall strategic recommendations

Make each point specific, actionable, and relevant to the business idea.
";

        return await GenerateTextAsync(prompt, parameters);
    }

    public async Task<string> GenerateBusinessPlanAsync(string ideaDescription, Dictionary<string, object>? parameters = null)
    {
        var prompt = $@"
As a business planning expert, create a comprehensive business plan for:

Business Idea: {ideaDescription}

Structure the plan with these detailed sections:

1. EXECUTIVE SUMMARY
   - Business concept overview
   - Market opportunity summary
   - Competitive advantage
   - Financial highlights
   - Funding requirements

2. MARKET ANALYSIS
   - Industry overview and trends
   - Target market definition
   - Market size and growth projections
   - Customer needs analysis

3. COMPETITIVE ANALYSIS
   - Direct and indirect competitors
   - Competitive positioning
   - Differentiation strategy
   - Barriers to entry

4. MARKETING & SALES STRATEGY
   - Value proposition
   - Pricing strategy
   - Distribution channels
   - Customer acquisition plan
   - Sales projections

5. OPERATIONS PLAN
   - Business model canvas
   - Key processes and workflows
   - Technology requirements
   - Staffing plan

6. FINANCIAL PROJECTIONS (3-Year)
   - Revenue projections
   - Cost structure
   - Profitability timeline
   - Cash flow analysis
   - Break-even analysis

7. RISK ANALYSIS
   - Key risks and uncertainties
   - Mitigation strategies
   - Contingency plans

8. IMPLEMENTATION ROADMAP
   - Key milestones and timeline
   - Resource requirements
   - Success metrics and KPIs

Provide realistic, detailed projections and actionable strategies suitable for investors and stakeholders.
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
            var testPrompt = "Hello, please respond with 'Connection test successful' to confirm the API is working.";
            var response = await GenerateTextAsync(testPrompt, new Dictionary<string, object> { ["max_tokens"] = 50 });
            
            _logger.LogInformation("Gemini connection validation successful");
            return !string.IsNullOrEmpty(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini connection validation failed");
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
            ["max_tokens"] = 2048,
            ["supports_multimodal"] = true,
            ["model"] = _model,
            ["api_version"] = "v1beta"
        };
    }
}