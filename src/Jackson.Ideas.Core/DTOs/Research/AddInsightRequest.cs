using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.Research;

public class AddInsightRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
    
    public int Priority { get; set; } = 1;
    
    public double ConfidenceScore { get; set; } = 0.0;
    
    public string? Phase { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}