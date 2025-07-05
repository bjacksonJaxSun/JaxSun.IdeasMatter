namespace Jackson.Ideas.Core.DTOs.Research;

public class ResearchTaskItem
{
    public string TaskId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public ResearchTaskType TaskType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ResearchTaskStatus
{
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Queued, Processing, Completed, Failed, Cancelled
    public int Progress { get; set; } // 0-100
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public object? Result { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum ResearchTaskType
{
    CompetitiveAnalysis,
    SwotAnalysis,
    CustomerSegmentation,
    EnhancedSwotAnalysis,
    StrategicImplications
}