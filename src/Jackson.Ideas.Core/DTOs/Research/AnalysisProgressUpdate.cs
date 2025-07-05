namespace Jackson.Ideas.Core.DTOs.Research;

public class AnalysisProgressUpdate
{
    public Guid StrategyId { get; set; }
    
    public string CurrentPhase { get; set; } = string.Empty;
    
    public double ProgressPercentage { get; set; }
    
    public int EstimatedCompletionMinutes { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties for test compatibility
    public string[] PhaseDetails { get; set; } = Array.Empty<string>();
    
    public string[] CompletedPhases { get; set; } = Array.Empty<string>();
    
    public string[] RemainingPhases { get; set; } = Array.Empty<string>();
    
    public string Phase { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EstimatedTimeRemaining { get; set; }
    public double ConfidenceLevel { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty;
    public bool CanCancel { get; set; }
    public bool CanPause { get; set; }
}