using System.ComponentModel.DataAnnotations;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Entities;

public class ResearchSession
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }

    [Required]
    [StringLength(450)]  // Standard length for ASP.NET Identity user IDs
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public ResearchStatus Status { get; set; } = ResearchStatus.Pending;
    
    [StringLength(50)]
    public string ResearchApproach { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ResearchType { get; set; }
    
    [StringLength(1000)]
    public string? Goals { get; set; }
    
    public int EstimatedDurationMinutes { get; set; }
    
    public double ProgressPercentage { get; set; } = 0.0;
    
    [StringLength(50)]
    public string? CurrentPhase { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public double? AnalysisConfidence { get; set; }
    
    public double? AnalysisCompleteness { get; set; }
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    // JSON serialized array of next steps
    public string NextSteps { get; set; } = "[]";
    
    // Additional properties for demo mode compatibility
    [StringLength(100)]
    public string? Strategy { get; set; }
    
    public int EstimatedCompletionTime { get; set; }
    
    public int ConfidenceScore { get; set; }
    
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [StringLength(500)]
    public string? TargetMarket { get; set; }
    
    [StringLength(500)]
    public string? PrimaryGoal { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    
    public virtual ICollection<ResearchInsight> ResearchInsights { get; set; } = new List<ResearchInsight>();
    
    public virtual ICollection<ResearchOption> ResearchOptions { get; set; } = new List<ResearchOption>();
}