using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Interfaces.AI;
using Jackson.Ideas.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Jackson.Ideas.Infrastructure.Services.AI;

public class AIProviderManager : IAIProviderManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, IBaseAIProvider> _providers;
    private readonly byte[] _encryptionKey;

    public AIProviderManager(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _providers = new ConcurrentDictionary<string, IBaseAIProvider>();
        
        // Get encryption key from configuration or generate one
        var encryptionKeyString = _configuration["ENCRYPTION_KEY"] ?? throw new InvalidOperationException("ENCRYPTION_KEY not configured");
        _encryptionKey = Convert.FromBase64String(encryptionKeyString);
    }

    public string EncryptApiKey(string apiKey)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);

        swEncrypt.Write(apiKey);
        
        var encrypted = msEncrypt.ToArray();
        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
        
        return Convert.ToBase64String(result);
    }

    public string DecryptApiKey(string encryptedKey)
    {
        var fullCipher = Convert.FromBase64String(encryptedKey);
        
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        
        var iv = new byte[aes.IV.Length];
        var cipher = new byte[fullCipher.Length - iv.Length];
        
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
        
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    public async Task<IBaseAIProvider> LoadProviderAsync(AIProviderConfig providerConfig, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var apiKey = DecryptApiKey(providerConfig.EncryptedApiKey);

        // Parse configuration from JSON
        var config = providerConfig.Config?.RootElement;
        var model = GetConfigValue(config, "model", GetDefaultModel(providerConfig.Type));
        var temperature = GetConfigValue(config, "temperature", 0.7);
        var maxTokens = GetConfigValue(config, "max_tokens", 2000);

        return providerConfig.Type switch
        {
            AIProviderType.OpenAI => new OpenAIProvider(
                httpClient,
                apiKey,
                model,
                temperature,
                maxTokens
            ),
            AIProviderType.Claude => new AnthropicProvider(
                httpClient,
                apiKey,
                model,
                temperature,
                maxTokens
            ),
            AIProviderType.Gemini => CreateGeminiProvider(httpClient),
            AIProviderType.AzureOpenAI => CreateAzureOpenAIProvider(httpClient),
            _ => throw new ArgumentException($"Unknown provider type: {providerConfig.Type}")
        };
    }

    public async Task<IBaseAIProvider> GetProviderAsync(string providerId, CancellationToken cancellationToken = default)
    {
        if (_providers.TryGetValue(providerId, out var cachedProvider))
        {
            return cachedProvider;
        }

        // In a real implementation, this would load from database
        // For now, create providers from environment variables
        var openAiKey = _configuration["OPENAI_API_KEY"];
        var anthropicKey = _configuration["ANTHROPIC_API_KEY"];

        if (!string.IsNullOrEmpty(openAiKey))
        {
            var httpClient = _httpClientFactory.CreateClient();
            var provider = new OpenAIProvider(httpClient, openAiKey);
            _providers.TryAdd("default_openai", provider);
            return provider;
        }

        if (!string.IsNullOrEmpty(anthropicKey))
        {
            var httpClient = _httpClientFactory.CreateClient();
            var provider = new AnthropicProvider(httpClient, anthropicKey);
            _providers.TryAdd("default_anthropic", provider);
            return provider;
        }

        throw new InvalidOperationException("No AI provider configured. Please set up an AI provider in the admin panel or environment variables.");
    }

    public async Task<IBaseAIProvider> GetProviderAsync(AIProviderType providerType, CancellationToken cancellationToken = default)
    {
        var providerId = $"default_{providerType.ToString().ToLower()}";
        
        if (_providers.TryGetValue(providerId, out var cachedProvider))
        {
            return cachedProvider;
        }

        // Create provider based on type from environment variables
        var httpClient = _httpClientFactory.CreateClient();
        
        IBaseAIProvider provider = providerType switch
        {
            AIProviderType.OpenAI => CreateOpenAIProvider(httpClient),
            AIProviderType.Claude => CreateAnthropicProvider(httpClient),
            AIProviderType.AzureOpenAI => CreateAzureOpenAIProvider(httpClient),
            AIProviderType.Gemini => CreateGeminiProvider(httpClient),
            _ => throw new ArgumentException($"Unknown provider type: {providerType}")
        };

        _providers.TryAdd(providerId, provider);
        return provider;
    }
    
    private IBaseAIProvider CreateOpenAIProvider(HttpClient httpClient)
    {
        var openAiKey = _configuration["OPENAI_API_KEY"];
        if (string.IsNullOrEmpty(openAiKey))
            throw new InvalidOperationException("OpenAI API key not configured");
        return new OpenAIProvider(httpClient, openAiKey);
    }
    
    private IBaseAIProvider CreateAnthropicProvider(HttpClient httpClient)
    {
        var anthropicKey = _configuration["ANTHROPIC_API_KEY"];
        if (string.IsNullOrEmpty(anthropicKey))
            throw new InvalidOperationException("Anthropic API key not configured");
        return new AnthropicProvider(httpClient, anthropicKey);
    }
    
    private IBaseAIProvider CreateAzureOpenAIProvider(HttpClient httpClient)
    {
        var azureKey = _configuration["AZURE_OPENAI_API_KEY"];
        var azureEndpoint = _configuration["AZURE_OPENAI_ENDPOINT"];
        var azureDeployment = _configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];
        
        if (string.IsNullOrEmpty(azureKey) || string.IsNullOrEmpty(azureEndpoint))
            throw new InvalidOperationException("Azure OpenAI API key and endpoint not configured");
            
        // Create a temporary configuration for Azure OpenAI
        var tempConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureOpenAI:ApiKey"] = azureKey,
                ["AzureOpenAI:Endpoint"] = azureEndpoint,
                ["AzureOpenAI:DeploymentName"] = azureDeployment ?? "gpt-4"
            })
            .Build();
            
        return new AzureOpenAIProvider(httpClient, null!, tempConfig);
    }
    
    private IBaseAIProvider CreateGeminiProvider(HttpClient httpClient)
    {
        var geminiKey = _configuration["GEMINI_API_KEY"];
        if (string.IsNullOrEmpty(geminiKey))
            throw new InvalidOperationException("Gemini API key not configured");
            
        // Create a temporary configuration for Gemini
        var tempConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Gemini:ApiKey"] = geminiKey,
                ["Gemini:Model"] = "gemini-1.5-flash"
            })
            .Build();
            
        return new GeminiProvider(httpClient, null!, tempConfig);
    }

    public async Task<bool> TestProviderAsync(IBaseAIProvider provider, CancellationToken cancellationToken = default)
    {
        return await provider.ValidateApiKeyAsync(cancellationToken);
    }

    private static string GetDefaultModel(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => "gpt-4-turbo-preview",
            AIProviderType.Claude => "claude-3-opus-20240229",
            AIProviderType.Gemini => "gemini-pro",
            AIProviderType.AzureOpenAI => "gpt-4",
            _ => "gpt-3.5-turbo"
        };
    }

    private static string GetConfigValue(JsonElement? config, string key, string defaultValue)
    {
        if (config?.TryGetProperty(key, out var property) == true)
        {
            return property.GetString() ?? defaultValue;
        }
        return defaultValue;
    }

    private static double GetConfigValue(JsonElement? config, string key, double defaultValue)
    {
        if (config?.TryGetProperty(key, out var property) == true)
        {
            return property.GetDouble();
        }
        return defaultValue;
    }

    private static int GetConfigValue(JsonElement? config, string key, int defaultValue)
    {
        if (config?.TryGetProperty(key, out var property) == true)
        {
            return property.GetInt32();
        }
        return defaultValue;
    }
}