namespace Jackson.Ideas.Core.DTOs.Research;

public class StrategicOptionDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int Score { get; set; }
    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();
    public string Timeline { get; set; } = "";
    public decimal? EstimatedCost { get; set; }
    public int RiskLevel { get; set; } // 1-10 scale
    public Dictionary<string, int> Metrics { get; set; } = new();
}