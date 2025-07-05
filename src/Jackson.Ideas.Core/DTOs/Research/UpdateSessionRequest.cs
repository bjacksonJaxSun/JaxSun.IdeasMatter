using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.Research;

public class UpdateSessionRequest
{
    [StringLength(255)]
    public string? Title { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Legacy properties for backward compatibility
    [StringLength(500)]
    public string? IdeaDescription 
    { 
        get => Description; 
        set => Description = value; 
    }
    
    [StringLength(1000)]
    public string? Goals { get; set; }
    
    public string? ResearchApproach { get; set; }
    
    public int? EstimatedDurationMinutes { get; set; }
    
    public Dictionary<string, object>? Parameters { get; set; }
}