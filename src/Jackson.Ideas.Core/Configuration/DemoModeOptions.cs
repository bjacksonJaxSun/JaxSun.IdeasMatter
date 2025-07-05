namespace Jackson.Ideas.Core.Configuration;

/// <summary>
/// Configuration options for demo mode functionality.
/// Enables UX review without requiring actual AI processing.
/// </summary>
public class DemoModeOptions
{
    public const string SectionName = "DemoMode";
    
    /// <summary>
    /// Whether demo mode is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// Whether to use real AI providers or mock responses
    /// </summary>
    public bool UseRealAI { get; set; } = true;
    
    /// <summary>
    /// Whether to simulate realistic processing delays for better UX demonstration
    /// </summary>
    public bool SimulateProcessingDelays { get; set; } = true;
    
    /// <summary>
    /// How often to refresh mock data for dynamic demonstration (in minutes)
    /// </summary>
    public int MockDataRefreshIntervalMinutes { get; set; } = 60;
    
    /// <summary>
    /// Whether to show confidence indicators in the UI per UX Blueprint
    /// </summary>
    public bool ShowConfidenceIndicators { get; set; } = true;
    
    /// <summary>
    /// Whether to enable enhanced progress tracking features
    /// </summary>
    public bool EnableProgressTracking { get; set; } = true;
}