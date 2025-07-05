using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.Entities;

public class ResearchInsight
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }

    [Required]
    public Guid ResearchSessionId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Phase { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Priority { get; set; } = "Medium";
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Range(0.0, 1.0)]
    public double ConfidenceScore { get; set; } = 0.0;
    
    // JSON serialized metadata for structured data
    public string Metadata { get; set; } = "{}";
    
    [StringLength(100)]
    public string? InsightType { get; set; }
    
    public int? SortOrder { get; set; }
    
    // Navigation properties
    public virtual ResearchSession? ResearchSession { get; set; }
}