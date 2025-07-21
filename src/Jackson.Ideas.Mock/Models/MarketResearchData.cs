namespace Jackson.Ideas.Mock.Models;

public class MarketResearchData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ScenarioId { get; set; } = "";
    public string Industry { get; set; } = "";
    public MarketSegmentation Segmentation { get; set; } = new();
    public CompetitiveAnalysis Competition { get; set; } = new();
    public MarketTrends Trends { get; set; } = new();
    public CustomerInsights Customer { get; set; } = new();
    public RegulatoryEnvironment Regulatory { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class MarketSegmentation
{
    public string TotalMarketSize { get; set; } = "";
    public string ServicableMarketSize { get; set; } = "";
    public string TargetMarketSize { get; set; } = "";
    public List<MarketSegment> Segments { get; set; } = new();
    public string GrowthRate { get; set; } = "";
}

public class MarketSegment
{
    public string Name { get; set; } = "";
    public string Size { get; set; } = "";
    public string Demographics { get; set; } = "";
    public List<string> Characteristics { get; set; } = new();
    public string PurchasingPower { get; set; } = "";
}

public class CompetitiveAnalysis
{
    public List<Competitor> DirectCompetitors { get; set; } = new();
    public List<Competitor> IndirectCompetitors { get; set; } = new();
    public List<string> CompetitiveAdvantages { get; set; } = new();
    public List<string> MarketGaps { get; set; } = new();
    public string CompetitiveIntensity { get; set; } = "";
}

public class Competitor
{
    public string Name { get; set; } = "";
    public string MarketShare { get; set; } = "";
    public string Strengths { get; set; } = "";
    public string Weaknesses { get; set; } = "";
    public string PricingStrategy { get; set; } = "";
    public string DifferentiationFactor { get; set; } = "";
}

public class MarketTrends
{
    public List<string> EmergingTrends { get; set; } = new();
    public List<string> TechnologyTrends { get; set; } = new();
    public List<string> ConsumerBehaviorTrends { get; set; } = new();
    public List<string> RegulatoryTrends { get; set; } = new();
    public string MarketMaturity { get; set; } = "";
}

public class CustomerInsights
{
    public List<string> PainPoints { get; set; } = new();
    public List<string> Motivations { get; set; } = new();
    public List<string> PreferredChannels { get; set; } = new();
    public string PurchaseDecisionFactors { get; set; } = "";
    public string CustomerLifetimeValue { get; set; } = "";
    public string AcquisitionCost { get; set; } = "";
}

public class RegulatoryEnvironment
{
    public List<string> KeyRegulations { get; set; } = new();
    public List<string> ComplianceRequirements { get; set; } = new();
    public List<string> UpcomingChanges { get; set; } = new();
    public string RegulatoryRisk { get; set; } = "";
}