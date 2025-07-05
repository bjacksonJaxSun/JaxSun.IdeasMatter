namespace Jackson.Ideas.Core.DTOs.Research;

public class CustomerSegmentationResult
{
    public string PrimaryTargetSegment { get; set; } = string.Empty;
    
    public List<CustomerSegment> CustomerSegments { get; set; } = new();
    
    public List<CustomerPersona> CustomerPersonas { get; set; } = new();
    
    public List<string> CustomerJourneyInsights { get; set; } = new();
    
    public List<string> UnmetNeeds { get; set; } = new();
    
    public List<string> MarketValidationEvidence { get; set; } = new();
    
    public double ConfidenceScore { get; set; }
    
    public string Summary { get; set; } = string.Empty;
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties for test compatibility
    public CustomerSegment[] PrimarySegments { get; set; } = Array.Empty<CustomerSegment>();
    
    public CustomerSegment[] SecondarySegments { get; set; } = Array.Empty<CustomerSegment>();
    
    public string[] KeyInsights { get; set; } = Array.Empty<string>();
    
    public string[] ValidationMethods { get; set; } = Array.Empty<string>();
    
    public string Industry { get; set; } = string.Empty;
    
    public List<CustomerPersona> PersonaProfiles { get; set; } = new();
    
    public List<string> SegmentPriorities { get; set; } = new();
    
    public Dictionary<string, double> MarketSizingBySegment { get; set; } = new();
}

public class CustomerSegment
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int SizeEstimate { get; set; }
    
    public Dictionary<string, object> Demographics { get; set; } = new();
    
    public List<string> PainPoints { get; set; } = new();
    
    public List<string> JobsToBeDone { get; set; } = new();
    
    public List<string> ValuePropositions { get; set; } = new();
    
    public double PriorityScore { get; set; } // 0-1 scale
    
    public string BuyingBehavior { get; set; } = string.Empty;
    
    public decimal WillingnessToPay { get; set; }
    
    public string PreferredChannels { get; set; } = string.Empty;
    
    public List<string> InfluencingFactors { get; set; } = new();
    
    // Additional properties for test compatibility
    public string Size { get; set; } = string.Empty;
    
    public string[] Demographics2 { get; set; } = Array.Empty<string>();
    
    public string[] Psychographics { get; set; } = Array.Empty<string>();
    
    public string[] BehaviorPatterns { get; set; } = Array.Empty<string>();
    
    public string[] Needs { get; set; } = Array.Empty<string>();
    
    public double MarketSize { get; set; }
    
    public double GrowthPotential { get; set; }
    
    public double AccessibilityScore { get; set; }
    
    public string PreferredSolutionType { get; set; } = string.Empty;
}

public class CustomerPersona
{
    public string Name { get; set; } = string.Empty;
    
    public string Segment { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public Demographics Demographics { get; set; } = new();
    
    public List<string> Goals { get; set; } = new();
    
    public List<string> Frustrations { get; set; } = new();
    
    public List<string> Motivations { get; set; } = new();
    
    public BehaviorProfile BehaviorProfile { get; set; } = new();
    
    public List<string> PreferredSolutions { get; set; } = new();
    
    public string DecisionMakingProcess { get; set; } = string.Empty;
    
    public List<string> InfluencingSources { get; set; } = new();
    
    public string Quote { get; set; } = string.Empty; // Representative quote
}

public class Demographics
{
    public string AgeRange { get; set; } = string.Empty;
    
    public string Gender { get; set; } = string.Empty;
    
    public string Income { get; set; } = string.Empty;
    
    public string Education { get; set; } = string.Empty;
    
    public string Occupation { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;
    
    public string FamilyStatus { get; set; } = string.Empty;
}

public class BehaviorProfile
{
    public string TechnologyAdoption { get; set; } = string.Empty;
    
    public string PurchasingBehavior { get; set; } = string.Empty;
    
    public string CommunicationStyle { get; set; } = string.Empty;
    
    public List<string> PreferredChannels { get; set; } = new();
    
    public string BrandLoyalty { get; set; } = string.Empty;
    
    public string RiskTolerance { get; set; } = string.Empty;
}

public class CustomerJourney
{
    public string Segment { get; set; } = string.Empty;
    
    public List<JourneyStage> Stages { get; set; } = new();
    
    public List<string> Touchpoints { get; set; } = new();
    
    public List<string> PainPoints { get; set; } = new();
    
    public List<string> OpportunityMoments { get; set; } = new();
    
    public string KeyInsights { get; set; } = string.Empty;
}

public class JourneyStage
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public List<string> CustomerActions { get; set; } = new();
    
    public List<string> CustomerThoughts { get; set; } = new();
    
    public List<string> CustomerEmotions { get; set; } = new();
    
    public List<string> Touchpoints { get; set; } = new();
    
    public List<string> PainPoints { get; set; } = new();
    
    public List<string> Opportunities { get; set; } = new();
}

public class CustomerInsightResult
{
    public List<string> KeyInsights { get; set; } = new();
    
    public List<string> ActionableRecommendations { get; set; } = new();
    
    public List<string> ProductFeaturePriorities { get; set; } = new();
    
    public List<string> MarketingMessages { get; set; } = new();
    
    public List<string> ChannelRecommendations { get; set; } = new();
    
    public List<string> PricingInsights { get; set; } = new();
    
    public string CustomerAcquisitionStrategy { get; set; } = string.Empty;
    
    public string CustomerRetentionStrategy { get; set; } = string.Empty;
}