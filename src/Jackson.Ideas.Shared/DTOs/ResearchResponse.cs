using System.Text.Json;

namespace Jackson.Ideas.Shared.DTOs;

public record ResearchResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ResearchType { get; init; } = string.Empty;
    public string? AIProvider { get; init; }
    public int ProgressPercentage { get; init; }
    public string? CurrentStep { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int TokensUsed { get; init; }
    public decimal EstimatedCost { get; init; }
    
    // JSON objects
    public JsonElement? MarketAnalysis { get; init; }
    public JsonElement? SwotAnalysis { get; init; }
    public JsonElement? BusinessPlan { get; init; }
    public JsonElement? Competitors { get; init; }
    public JsonElement? ProviderInsights { get; init; }
    
    // User info
    public string UserEmail { get; init; } = string.Empty;
}