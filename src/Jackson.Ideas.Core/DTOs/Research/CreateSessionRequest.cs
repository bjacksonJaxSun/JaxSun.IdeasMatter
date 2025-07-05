using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.Research;

public class CreateSessionRequest
{
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? Title { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    // Legacy properties for backward compatibility
    [Required]
    [StringLength(500)]
    public string IdeaDescription
    {
        get => Description;
        set => Description = value;
    }
    
    [StringLength(1000)]
    public string Goals { get; set; } = string.Empty;
    
    public string? ResearchApproach { get; set; }
    
    public int EstimatedDurationMinutes { get; set; } = 30;
    
    public Dictionary<string, object>? Parameters { get; set; }
}