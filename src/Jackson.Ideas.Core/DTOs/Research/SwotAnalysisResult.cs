namespace Jackson.Ideas.Core.DTOs.Research;

public class SwotAnalysisResult
{
    // Primary properties that can work with both string arrays and SwotFactor lists
    private List<SwotFactor> _strengths = new();
    private List<SwotFactor> _weaknesses = new();
    private List<SwotFactor> _opportunities = new();
    private List<SwotFactor> _threats = new();
    
    public object Strengths 
    { 
        get => _strengths.Any() ? _strengths.Select(s => s.Title).ToArray() : Array.Empty<string>();
        set 
        {
            if (value is string[] strArray)
                _strengths = strArray.Select(s => new SwotFactor { Title = s, Category = "Strengths" }).ToList();
            else if (value is List<SwotFactor> factorList)
                _strengths = factorList;
        }
    }
    
    public object Weaknesses 
    { 
        get => _weaknesses.Any() ? _weaknesses.Select(w => w.Title).ToArray() : Array.Empty<string>();
        set 
        {
            if (value is string[] strArray)
                _weaknesses = strArray.Select(w => new SwotFactor { Title = w, Category = "Weaknesses" }).ToList();
            else if (value is List<SwotFactor> factorList)
                _weaknesses = factorList;
        }
    }
    
    public object Opportunities 
    { 
        get => _opportunities.Any() ? _opportunities.Select(o => o.Title).ToArray() : Array.Empty<string>();
        set 
        {
            if (value is string[] strArray)
                _opportunities = strArray.Select(o => new SwotFactor { Title = o, Category = "Opportunities" }).ToList();
            else if (value is List<SwotFactor> factorList)
                _opportunities = factorList;
        }
    }
    
    public object Threats 
    { 
        get => _threats.Any() ? _threats.Select(t => t.Title).ToArray() : Array.Empty<string>();
        set 
        {
            if (value is string[] strArray)
                _threats = strArray.Select(t => new SwotFactor { Title = t, Category = "Threats" }).ToList();
            else if (value is List<SwotFactor> factorList)
                _threats = factorList;
        }
    }
    
    // Type-safe accessors for when you need SwotFactor lists
    public List<SwotFactor> StrengthsFactors => _strengths;
    public List<SwotFactor> WeaknessesFactors => _weaknesses;
    public List<SwotFactor> OpportunitiesFactors => _opportunities;
    public List<SwotFactor> ThreatsFactors => _threats;
    
    public List<string> StrategicImplications { get; set; } = new();
    
    public List<string> CriticalSuccessFactors { get; set; } = new();
    
    public double ConfidenceScore { get; set; }
    
    public string Summary { get; set; } = string.Empty;
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties for test compatibility
    public string[] StrategicInsights { get; set; } = Array.Empty<string>();
    
    public string OverallAssessment { get; set; } = string.Empty;
    
    public string RiskLevel { get; set; } = string.Empty;
    
    public double OpportunityScore { get; set; }
    
    public double ThreatLevel { get; set; }
    
    public double StrengthRating { get; set; }
    
    public double WeaknessImpact { get; set; }
    
    public string IdeaTitle { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public List<string> StrategicOptions { get; set; } = new();
    public double OverallConfidence { get; set; }
    public string AnalysisDepth { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public List<string> CategoryPriorities { get; set; } = new();
}

public class SwotFactor
{
    public string Category { get; set; } = string.Empty; // Strengths, Weaknesses, Opportunities, Threats
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public double Impact { get; set; } // 1-10 scale
    
    public double Likelihood { get; set; } // 1-10 scale (for Opportunities and Threats)
    
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    
    public List<string> ActionableInsights { get; set; } = new();
    
    public List<string> RelatedFactors { get; set; } = new();
}

public class StrategicImplicationsResult
{
    public List<StrategicRecommendation> SOStrategies { get; set; } = new(); // Strengths-Opportunities
    
    public List<StrategicRecommendation> WOStrategies { get; set; } = new(); // Weaknesses-Opportunities
    
    public List<StrategicRecommendation> STStrategies { get; set; } = new(); // Strengths-Threats
    
    public List<StrategicRecommendation> WTStrategies { get; set; } = new(); // Weaknesses-Threats
    
    public List<string> PriorityActions { get; set; } = new();
    
    public List<string> KeyRisks { get; set; } = new();
    
    public string OverallStrategicDirection { get; set; } = string.Empty;
    
    public double StrategicFitScore { get; set; } // 1-10 scale
}

public class StrategicRecommendation
{
    public string Strategy { get; set; } = string.Empty;
    
    public string Type { get; set; } = string.Empty; // SO, WO, ST, WT
    
    public string Description { get; set; } = string.Empty;
    
    public List<string> RequiredActions { get; set; } = new();
    
    public int Timeline { get; set; } // Months
    
    public string Priority { get; set; } = string.Empty;
    
    public double FeasibilityScore { get; set; }
    
    public double ImpactScore { get; set; }
}