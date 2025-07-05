using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Entities;

public class AIProviderConfig : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public AIProviderType Type { get; set; }
    
    [Required]
    public string EncryptedApiKey { get; set; } = string.Empty;
    
    public string ConfigJson { get; set; } = "{}";
    
    public bool IsActive { get; set; } = true;
    
    public int Priority { get; set; } = 1;
    
    public int UsageCount { get; set; } = 0;
    
    public long TotalTokens { get; set; } = 0;
    
    public DateTime? LastUsedAt { get; set; }
    
    public int RateLimitRpm { get; set; } = 60;
    
    public int CurrentRpmUsage { get; set; } = 0;
    
    public decimal CostPerToken { get; set; } = 0;
    
    // Foreign key
    [StringLength(450)]  // Standard length for ASP.NET Identity user IDs
    public string? UserId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    
    // Helper properties for JSON conversion
    [NotMapped]
    public JsonDocument? Config
    {
        get => string.IsNullOrEmpty(ConfigJson) ? null : JsonDocument.Parse(ConfigJson);
        set => ConfigJson = value?.RootElement.GetRawText() ?? "{}";
    }
    
    public bool IsRateLimited()
    {
        return CurrentRpmUsage >= RateLimitRpm;
    }
    
    public void IncrementUsage(int tokens = 0)
    {
        UsageCount++;
        TotalTokens += tokens;
        CurrentRpmUsage++;
        LastUsedAt = DateTime.UtcNow;
    }
    
    public void ResetRpmUsage()
    {
        CurrentRpmUsage = 0;
    }
}