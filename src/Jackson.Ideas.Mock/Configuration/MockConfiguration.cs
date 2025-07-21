namespace Jackson.Ideas.Mock.Configuration;

public class MockConfiguration
{
    public bool EnableRealisticDelays { get; set; } = true;
    public int MinProcessingDelayMs { get; set; } = 1000;
    public int MaxProcessingDelayMs { get; set; } = 5000;
    public int DefaultScenarioCount { get; set; } = 20;
    public bool SimulateErrors { get; set; } = false;
    public double ErrorRate { get; set; } = 0.05;
}