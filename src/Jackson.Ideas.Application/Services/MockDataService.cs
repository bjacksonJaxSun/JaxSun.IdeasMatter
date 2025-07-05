using Jackson.Ideas.Core.DTOs.AI;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Application.Services;

/// <summary>
/// Provides comprehensive mock data for UX review and demonstration purposes.
/// Implements UX Blueprint principles including confidence visualization, 
/// multi-tier information architecture, and progressive disclosure.
/// </summary>
public class MockDataService
{
    private readonly Random _random = new();
    
    // Industry-specific data following template marketplace concept
    private readonly Dictionary<string, IndustryTemplate> _industryTemplates = new()
    {
        ["fintech"] = new IndustryTemplate
        {
            Name = "FinTech",
            CompetitorTypes = ["Traditional Banks", "Digital Banks", "Payment Processors", "Crypto Platforms"],
            KeyMetrics = ["Customer Acquisition Cost", "Monthly Active Users", "Transaction Volume", "Revenue per User"],
            RegulatoryFactors = ["PCI DSS Compliance", "AML/KYC Requirements", "Banking Regulations", "Data Privacy Laws"],
            MarketSize = new MarketSizingData { TAM = 1200000M, SAM = 120000M, SOM = 12000M }
        },
        ["ecommerce"] = new IndustryTemplate
        {
            Name = "E-Commerce",
            CompetitorTypes = ["Marketplace Giants", "D2C Brands", "Social Commerce", "Subscription Boxes"],
            KeyMetrics = ["Conversion Rate", "Average Order Value", "Customer Lifetime Value", "Cart Abandonment Rate"],
            RegulatoryFactors = ["Consumer Protection", "Tax Compliance", "Shipping Regulations", "Product Safety"],
            MarketSize = new MarketSizingData { TAM = 5400000M, SAM = 540000M, SOM = 54000M }
        },
        ["saas"] = new IndustryTemplate
        {
            Name = "SaaS",
            CompetitorTypes = ["Enterprise Solutions", "SMB Tools", "Freemium Platforms", "Industry-Specific"],
            KeyMetrics = ["Monthly Recurring Revenue", "Churn Rate", "Customer Acquisition Cost", "Net Promoter Score"],
            RegulatoryFactors = ["Data Security", "SOC 2 Compliance", "GDPR Compliance", "Industry Standards"],
            MarketSize = new MarketSizingData { TAM = 800000M, SAM = 80000M, SOM = 8000M }
        },
        ["healthtech"] = new IndustryTemplate
        {
            Name = "HealthTech",
            CompetitorTypes = ["Telemedicine Platforms", "Health Apps", "Medical Devices", "Healthcare Systems"],
            KeyMetrics = ["Patient Engagement", "Clinical Outcomes", "Provider Adoption", "Cost Reduction"],
            RegulatoryFactors = ["HIPAA Compliance", "FDA Regulations", "Medical Licensing", "Clinical Trials"],
            MarketSize = new MarketSizingData { TAM = 350000M, SAM = 35000M, SOM = 3500M }
        }
    };

    /// <summary>
    /// Generates mock research session data with realistic progression and status variety
    /// Following wireflow documentation patterns from UX Blueprint
    /// </summary>
    public List<ResearchSession> GetMockResearchSessions(string userId, int count = 5)
    {
        var sessions = new List<ResearchSession>();
        var statuses = Enum.GetValues<ResearchStatus>();
        var strategies = new[] { "Quick Validation", "Market Deep-Dive", "Launch Strategy" };
        var industries = _industryTemplates.Keys.ToArray();

        for (int i = 0; i < count; i++)
        {
            var industry = industries[_random.Next(industries.Length)];
            var template = _industryTemplates[industry];
            var status = statuses[_random.Next(statuses.Length)];
            
            sessions.Add(new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = GenerateIdeaTitle(industry),
                Description = GenerateIdeaDescription(industry, template),
                Status = status,
                Strategy = strategies[_random.Next(strategies.Length)],
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30)),
                UpdatedAt = DateTime.UtcNow.AddHours(-_random.Next(0, 24)),
                ProgressPercentage = GetProgressForStatus(status),
                EstimatedCompletionTime = GetEstimatedTimeForStrategy(strategies[_random.Next(strategies.Length)]),
                ConfidenceScore = _random.Next(75, 95), // High confidence for demo
                Industry = template.Name,
                TargetMarket = GenerateTargetMarket(industry),
                PrimaryGoal = GeneratePrimaryGoal(industry)
            });
        }

        return sessions.OrderByDescending(s => s.UpdatedAt).ToList();
    }

    /// <summary>
    /// Generates comprehensive market analysis with multi-tier information architecture
    /// Implements inverted pyramid presentation from UX Blueprint
    /// </summary>
    public ComprehensiveMarketAnalysisDto GetMockMarketAnalysis(string industry)
    {
        var template = _industryTemplates.GetValueOrDefault(industry.ToLower()) ?? _industryTemplates["saas"];
        
        return new ComprehensiveMarketAnalysisDto
        {
            // Strategic Level (High-level overview)
            ExecutiveSummary = GenerateExecutiveSummary(template),
            MarketSizing = template.MarketSize,
            ConfidenceScore = _random.Next(85, 95),
            
            // Operational Level (Detailed metrics)
            CompetitiveLandscape = GenerateCompetitiveLandscape(template),
            MarketTrends = GenerateMarketTrends(template),
            CustomerSegments = GenerateCustomerSegments(template),
            
            // Analytical Level (Deep-dive data)
            RegulatoryAnalysis = GenerateRegulatoryAnalysis(template),
            MarketOpportunities = GenerateMarketOpportunities(template),
            RiskFactors = GenerateRiskFactors(template),
            
            // Metadata for confidence visualization
            AnalysisMetadata = new Jackson.Ideas.Core.DTOs.MarketAnalysis.AnalysisMetadata
            {
                ProcessingTime = TimeSpan.FromMinutes(_random.Next(15, 45)),
                DataSources = GetDataSources(template),
                ConfidenceFactors = GetConfidenceFactors(),
                LastUpdated = DateTime.UtcNow.AddMinutes(-_random.Next(5, 120))
            }
        };
    }

    /// <summary>
    /// Generates SWOT analysis with optimized 2x2 grid visualization
    /// Implements hierarchical information with expandable sections
    /// </summary>
    public SwotAnalysisResult GetMockSwotAnalysis(string industry, string ideaTitle)
    {
        var template = _industryTemplates.GetValueOrDefault(industry.ToLower()) ?? _industryTemplates["saas"];
        
        return new SwotAnalysisResult
        {
            IdeaTitle = ideaTitle,
            Industry = template.Name,
            
            // 2x2 Grid Layout with clear section delineation
            Strengths = string.Join(", ", GenerateSwotStrengths(template)),
            Weaknesses = string.Join(", ", GenerateSwotWeaknesses(template)),
            Opportunities = GenerateSwotOpportunities(template),
            Threats = GenerateSwotThreats(template),
            
            // Strategic options with scoring for comparison
            StrategicOptions = GenerateStrategicOptions(template).Select(x => x.ToString()).ToList(),
            
            // Confidence and metadata
            OverallConfidence = _random.Next(80, 95),
            AnalysisDepth = "Comprehensive",
            RecommendedAction = GenerateRecommendedAction(template),
            
            // Interactive elements data
            CategoryPriorities = new Dictionary<string, int>
            {
                ["Market Entry"] = _random.Next(70, 90),
                ["Product Development"] = _random.Next(75, 95),
                ["Competitive Positioning"] = _random.Next(65, 85),
                ["Risk Mitigation"] = _random.Next(60, 80)
            }.Select(kvp => $"{kvp.Key}: {kvp.Value}").ToList()
        };
    }

    /// <summary>
    /// Generates realistic progress updates with psychology-based indicators
    /// Implements determinate progress patterns from UX Blueprint
    /// </summary>
    public List<AnalysisProgressUpdate> GetMockProgressUpdates(string sessionId, string strategy)
    {
        var phases = GetAnalysisPhases(strategy);
        var updates = new List<AnalysisProgressUpdate>();
        
        var currentTime = DateTime.UtcNow.AddMinutes(-GetEstimatedTimeForStrategy(strategy));
        var phaseInterval = GetEstimatedTimeForStrategy(strategy) / phases.Count;
        
        for (int i = 0; i < phases.Count; i++)
        {
            var isCompleted = i < phases.Count - 1; // All but last are completed for demo
            var progress = isCompleted ? 100 : _random.Next(75, 90);
            
            updates.Add(new AnalysisProgressUpdate
            {
                Phase = phases[i],
                Status = isCompleted ? "Completed" : "In Progress",
                ProgressPercentage = progress,
                EstimatedTimeRemaining = isCompleted ? 0 : _random.Next(2, 8),
                ConfidenceLevel = _random.Next(85, 95),
                Timestamp = currentTime.AddMinutes(i * phaseInterval),
                Details = GeneratePhaseDetails(phases[i]),
                CanCancel = !isCompleted,
                CanPause = !isCompleted
            });
        }
        
        return updates;
    }

    /// <summary>
    /// Generates competitive analysis with visual indicators
    /// Implements comparative visualization from UX Blueprint
    /// </summary>
    public CompetitiveAnalysisResult GetMockCompetitiveAnalysis(string industry)
    {
        var template = _industryTemplates.GetValueOrDefault(industry.ToLower()) ?? _industryTemplates["saas"];
        
        return new CompetitiveAnalysisResult
        {
            Industry = template.Name,
            CompetitorProfiles = GenerateCompetitorProfiles(template),
            CompetitiveMatrix = GenerateCompetitiveMatrix(template),
            MarketPositioning = GenerateMarketPositioning(template),
            DifferentiationOpportunities = GenerateDifferentiationOpportunities(template),
            CompetitiveThreatLevel = _random.Next(3, 8), // 1-10 scale
            ConfidenceScore = _random.Next(80, 95)
        };
    }

    /// <summary>
    /// Generates customer segmentation with visual persona representations
    /// </summary>
    public CustomerSegmentationResult GetMockCustomerSegmentation(string industry)
    {
        var template = _industryTemplates.GetValueOrDefault(industry.ToLower()) ?? _industryTemplates["saas"];
        
        return new CustomerSegmentationResult
        {
            Industry = template.Name,
            PrimarySegments = GeneratePrimarySegments(template).ToArray(),
            PersonaProfiles = GeneratePersonaProfiles(template),
            SegmentPriorities = GenerateSegmentPriorities(),
            MarketSizingBySegment = GenerateSegmentSizing(template),
            ConfidenceScore = _random.Next(85, 95)
        };
    }

    // Private helper methods for data generation
    private string GenerateIdeaTitle(string industry) => industry switch
    {
        "fintech" => $"AI-Powered {new[] { "Payment", "Investment", "Banking", "Insurance" }[_random.Next(4)]} Platform",
        "ecommerce" => $"Sustainable {new[] { "Fashion", "Electronics", "Home", "Beauty" }[_random.Next(4)]} Marketplace",
        "saas" => $"Smart {new[] { "CRM", "Analytics", "Automation", "Collaboration" }[_random.Next(4)]} Solution",
        "healthtech" => $"Digital {new[] { "Wellness", "Therapy", "Fitness", "Nutrition" }[_random.Next(4)]} Platform",
        _ => "Innovative Business Solution"
    };

    private string GenerateIdeaDescription(string industry, IndustryTemplate template)
    {
        return $"A revolutionary platform that leverages AI and modern technology to transform the {template.Name.ToLower()} industry. " +
               $"Our solution addresses key pain points including {string.Join(", ", template.KeyMetrics.Take(2))} while ensuring " +
               $"compliance with {template.RegulatoryFactors.First()}. The platform targets early adopters and aims to capture " +
               $"market share through superior user experience and innovative features.";
    }

    private int GetProgressForStatus(ResearchStatus status) => status switch
    {
        ResearchStatus.Pending => 5,
        ResearchStatus.InProgress => _random.Next(25, 85),
        ResearchStatus.Completed => 100,
        ResearchStatus.Failed => _random.Next(30, 70),
        _ => 0
    };

    private int GetEstimatedTimeForStrategy(string strategy) => strategy switch
    {
        "Quick Validation" => 15,
        "Market Deep-Dive" => 45,
        "Launch Strategy" => 90,
        _ => 30
    };

    private List<string> GetAnalysisPhases(string strategy) => strategy switch
    {
        "Quick Validation" => ["Market Context", "Competitive Overview", "Feasibility Assessment"],
        "Market Deep-Dive" => ["Market Context", "Competitive Intelligence", "Customer Understanding", "Strategic Assessment"],
        "Launch Strategy" => ["Market Context", "Competitive Intelligence", "Customer Understanding", "Strategic Assessment", "Launch Planning", "Risk Analysis"],
        _ => ["Analysis", "Validation", "Recommendations"]
    };

    private string GenerateTargetMarket(string industry) => industry switch
    {
        "fintech" => "Tech-savvy millennials and Gen Z seeking modern financial solutions",
        "ecommerce" => "Environmentally conscious consumers aged 25-45 with disposable income",
        "saas" => "Small to medium businesses looking to streamline operations",
        "healthtech" => "Health-conscious individuals and healthcare providers",
        _ => "Early adopters and innovation-focused organizations"
    };

    private string GeneratePrimaryGoal(string industry) => industry switch
    {
        "fintech" => "Achieve 100K active users within 18 months",
        "ecommerce" => "Capture 2% market share in sustainable products",
        "saas" => "Reach $1M ARR within 24 months",
        "healthtech" => "Partner with 50+ healthcare providers",
        _ => "Establish market presence and achieve profitability"
    };

    // Additional comprehensive helper methods would continue here...
    // This structure provides the foundation for all mock data generation
    // following UX Blueprint principles and PRD requirements

    private string GenerateExecutiveSummary(IndustryTemplate template)
    {
        return $"The {template.Name} market presents significant opportunities with a TAM of ${template.MarketSize.TAM:N0}M. " +
               $"Key growth drivers include digital transformation trends and evolving customer expectations. " +
               $"Market entry is recommended with focus on {template.KeyMetrics.First()} optimization and " +
               $"compliance with {template.RegulatoryFactors.First()}.";
    }

    private CompetitiveLandscapeDto GenerateCompetitiveLandscape(IndustryTemplate template)
    {
        return new CompetitiveLandscapeDto
        {
            DirectCompetitors = GenerateCompetitors(template, "Direct", 5),
            IndirectCompetitors = GenerateCompetitors(template, "Indirect", 3),
            SubstituteProducts = GenerateCompetitors(template, "Substitute", 3),
            CompetitiveIntensityValue = _random.Next(6, 9), // High intensity for most markets
            MarketConcentration = "Moderately Concentrated"
        };
    }

    private List<CompetitorAnalysisDto> GenerateCompetitors(IndustryTemplate template, string type, int count)
    {
        var competitors = new List<CompetitorAnalysisDto>();
        var companyNames = GenerateCompanyNames(template.Name, count);
        
        for (int i = 0; i < count; i++)
        {
            competitors.Add(new CompetitorAnalysisDto
            {
                Name = companyNames[i],
                MarketShare = _random.Next(5, 25),
                Strengths = string.Join(", ", GenerateCompetitorStrengths(template)),
                Weaknesses = string.Join(", ", GenerateCompetitorWeaknesses()),
                Revenue = _random.Next(10, 500) * 1000000M,
                EmployeeCount = _random.Next(50, 5000),
                FundingAmount = _random.Next(1, 100) * 1000000M,
                CompetitiveTier = ((CompetitorTier)_random.Next(1, 4)).ToString()
            });
        }
        
        return competitors;
    }

    private List<string> GenerateCompanyNames(string industry, int count)
    {
        var prefixes = industry switch
        {
            "FinTech" => new List<string> {"Quantum", "Apex", "Neo", "Digital", "Smart"},
            "E-Commerce" => new List<string> {"Global", "Direct", "Prime", "Swift", "Market"},
            "SaaS" => new List<string> {"Cloud", "Pro", "Enterprise", "Smart", "Auto"},
            "HealthTech" => new List<string> {"Vital", "Health", "Care", "Medical", "Wellness"},
            _ => new List<string> {"Tech", "Digital", "Smart", "Pro", "Advanced"}
        };
        
        var suffixes = industry switch
        {
            "FinTech" => new List<string> {"Pay", "Bank", "Finance", "Capital", "Invest"},
            "E-Commerce" => new List<string> {"Store", "Market", "Shop", "Commerce", "Trade"},
            "SaaS" => new List<string> {"Soft", "Systems", "Solutions", "Platform", "Tools"},
            "HealthTech" => new List<string> {"Health", "Care", "Medical", "Wellness", "Therapy"},
            _ => new List<string> {"Tech", "Systems", "Solutions", "Platform", "Labs"}
        };

        var names = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var prefix = prefixes[_random.Next(prefixes.Count)];
            var suffix = suffixes[_random.Next(suffixes.Count)];
            names.Add($"{prefix}{suffix}");
        }
        
        return names;
    }

    private List<MarketTrendDto> GenerateMarketTrends(IndustryTemplate template)
    {
        var trends = new List<MarketTrendDto>();
        var baseTrends = new[]
        {
            "Digital Transformation Acceleration",
            "AI/ML Integration",
            "Mobile-First Approaches",
            "Sustainability Focus",
            "Data Privacy Emphasis"
        };
        
        foreach (var trend in baseTrends.Take(4))
        {
            trends.Add(new MarketTrendDto
            {
                Name = trend,
                Description = GenerateTrendDescription(trend, template.Name),
                ImpactLevel = _random.Next(7, 10),
                Timeline = "12-18 months",
                Confidence = _random.Next(80, 95)
            });
        }
        
        return trends;
    }

    private string GenerateTrendDescription(string trend, string industry)
    {
        return trend switch
        {
            "Digital Transformation Acceleration" => $"Rapid adoption of digital solutions in {industry} driven by changing consumer expectations",
            "AI/ML Integration" => $"Increasing use of artificial intelligence to enhance {industry} operations and customer experience",
            "Mobile-First Approaches" => $"Shift towards mobile-optimized platforms and services in the {industry} sector",
            "Sustainability Focus" => $"Growing emphasis on sustainable practices and ESG compliance in {industry}",
            "Data Privacy Emphasis" => $"Heightened focus on data protection and privacy compliance in {industry} operations",
            _ => $"Emerging trend impacting {industry} market dynamics and competitive landscape"
        };
    }

    private List<MarketSegmentDto> GenerateCustomerSegments(IndustryTemplate template)
    {
        if (template.Name == "FinTech") {
            return new List<MarketSegmentDto>
            {
                new() { Name = "Digital Natives", Size = 35, GrowthRate = 15, Description = "Tech-savvy millennials and Gen Z" },
                new() { Name = "SMB Owners", Size = 25, GrowthRate = 12, Description = "Small business seeking financial tools" },
                new() { Name = "Traditional Users", Size = 40, GrowthRate = 5, Description = "Users transitioning from traditional banking" }
            };
        } else if (template.Name == "E-Commerce") {
            return new List<MarketSegmentDto>
            {
                new() { Name = "Conscious Consumers", Size = 30, GrowthRate = 18, Description = "Environmentally aware shoppers" },
                new() { Name = "Convenience Seekers", Size = 45, GrowthRate = 10, Description = "Time-pressed professionals" },
                new() { Name = "Value Hunters", Size = 25, GrowthRate = 8, Description = "Price-conscious buyers" }
            };
        } else {
            return new List<MarketSegmentDto>
            {
                new() { Name = "Early Adopters", Size = 20, GrowthRate = 20, Description = "Innovation-focused segment" },
                new() { Name = "Mainstream Users", Size = 60, GrowthRate = 12, Description = "General market adoption" },
                new() { Name = "Late Adopters", Size = 20, GrowthRate = 5, Description = "Conservative users" }
            };
        }
    }

    private List<string> GenerateRegulatoryAnalysis(IndustryTemplate template)
    {
        return template.RegulatoryFactors.Select(factor => 
            $"{factor}: {GenerateRegulatoryDetails(factor)}").ToList();
    }

    private string GenerateRegulatoryDetails(string factor)
    {
        return factor switch
        {
            "PCI DSS Compliance" => "Payment card industry standards requiring secure handling of cardholder data",
            "HIPAA Compliance" => "Healthcare privacy regulations protecting patient health information",
            "GDPR Compliance" => "European data protection regulation affecting user data handling",
            "SOC 2 Compliance" => "Security and availability standards for service organizations",
            _ => "Regulatory requirement ensuring compliance with industry standards and legal obligations"
        };
    }

    private List<MarketOpportunityDto> GenerateMarketOpportunities(IndustryTemplate template)
    {
        var opportunities = new List<MarketOpportunityDto>
        {
            new() { 
                Title = "Underserved Market Segments", 
                Description = $"Significant opportunity to serve overlooked customer groups in {template.Name}",
                PotentialValue = _random.Next(10, 50) * 1000000M,
                Timeline = "6-12 months",
                Probability = _random.Next(70, 90)
            },
            new() { 
                Title = "Technology Integration", 
                Description = "Leverage emerging technologies for competitive advantage",
                PotentialValue = _random.Next(20, 100) * 1000000M,
                Timeline = "12-18 months",
                Probability = _random.Next(60, 85)
            },
            new() { 
                Title = "Geographic Expansion", 
                Description = "Expand into new markets with proven solution",
                PotentialValue = _random.Next(15, 75) * 1000000M,
                Timeline = "18-24 months",
                Probability = _random.Next(65, 80)
            }
        };
        
        return opportunities;
    }

    private List<string> GenerateRiskFactors(IndustryTemplate template)
    {
        return new List<string>
        {
            "Regulatory changes impacting market dynamics",
            "Increased competition from established players",
            "Technology disruption changing customer expectations",
            "Economic conditions affecting customer spending",
            "Talent acquisition challenges in competitive market"
        };
    }

    private List<string> GetDataSources(IndustryTemplate template)
    {
        return new List<string>
        {
            "Industry Reports & Research",
            "Competitor Analysis",
            "Market Surveys",
            "Government Data",
            "Expert Interviews",
            "Financial Filings",
            "Trade Publications"
        };
    }

    private Dictionary<string, int> GetConfidenceFactors()
    {
        return new Dictionary<string, int>
        {
            ["Data Quality"] = _random.Next(85, 95),
            ["Sample Size"] = _random.Next(80, 90),
            ["Source Credibility"] = _random.Next(90, 95),
            ["Methodology"] = _random.Next(85, 92),
            ["Recency"] = _random.Next(88, 96)
        };
    }

    private string GeneratePhaseDetails(string phase)
    {
        return phase switch
        {
            "Market Context" => "Analyzing market size, growth trends, and competitive landscape",
            "Competitive Intelligence" => "Deep-dive analysis of direct and indirect competitors",
            "Customer Understanding" => "Segmentation analysis and customer persona development",
            "Strategic Assessment" => "SWOT analysis and strategic option evaluation",
            "Launch Planning" => "Go-to-market strategy and execution planning",
            "Risk Analysis" => "Risk identification and mitigation strategy development",
            _ => "Comprehensive analysis in progress"
        };
    }

    // SWOT Analysis Generation Methods
    private List<string> GenerateSwotStrengths(IndustryTemplate template)
    {
        return new List<string>
        {
            $"Deep understanding of {template.Name} market dynamics",
            "Strong technical team with relevant expertise",
            "Innovative approach to solving market problems",
            "Agile development and rapid iteration capability",
            "Cost-effective solution compared to alternatives"
        };
    }

    private List<string> GenerateSwotWeaknesses(IndustryTemplate template)
    {
        return new List<string>
        {
            "Limited brand recognition in established market",
            "Resource constraints compared to larger competitors",
            "Dependence on key team members",
            "Limited customer base and market presence",
            "Need for additional funding for scale"
        };
    }

    private List<string> GenerateSwotOpportunities(IndustryTemplate template)
    {
        return new List<string>
        {
            $"Growing demand for digital solutions in {template.Name}",
            "Underserved customer segments with specific needs",
            "Technology trends enabling new business models",
            "Regulatory changes creating market opportunities",
            "Partnership opportunities with industry leaders"
        };
    }

    private List<string> GenerateSwotThreats(IndustryTemplate template)
    {
        return new List<string>
        {
            "Large competitors with significant resources",
            "Rapid technology changes affecting market",
            "Economic uncertainty impacting customer spending",
            "Regulatory risks and compliance requirements",
            "Potential market saturation and price pressure"
        };
    }

    private List<StrategicOptionDto> GenerateStrategicOptions(IndustryTemplate template)
    {
        return new List<StrategicOptionDto>
        {
            new() {
                Title = "Rapid Market Entry",
                Description = "Focus on quick market penetration with core features",
                Score = _random.Next(75, 90),
                Pros = ["Fast time to market", "Lower initial investment", "Early customer feedback"],
                Cons = ["Limited feature set", "Higher competitive risk"],
                Timeline = "3-6 months"
            },
            new() {
                Title = "Comprehensive Solution",
                Description = "Develop full-featured platform before launch",
                Score = _random.Next(70, 85),
                Pros = ["Complete solution", "Stronger competitive position", "Higher customer satisfaction"],
                Cons = ["Longer development time", "Higher initial costs"],
                Timeline = "9-12 months"
            },
            new() {
                Title = "Partnership Strategy",
                Description = "Partner with established players for market access",
                Score = _random.Next(65, 80),
                Pros = ["Leverage existing relationships", "Shared risk and investment", "Faster market access"],
                Cons = ["Reduced control", "Revenue sharing", "Dependency on partner"],
                Timeline = "6-9 months"
            }
        };
    }

    private string GenerateRecommendedAction(IndustryTemplate template)
    {
        return $"Proceed with Rapid Market Entry strategy for {template.Name} market. " +
               "Focus on core value proposition and iterate based on early customer feedback. " +
               "Prepare for follow-up funding round to support scaling operations.";
    }

    private List<string> GenerateCompetitorStrengths(IndustryTemplate template)
    {
        if (template.Name == "FinTech")
            return new List<string> { "Strong brand", "Innovative tech", "Large user base" };
        if (template.Name == "E-Commerce")
            return new List<string> { "Wide selection", "Fast shipping", "Loyal customers" };
        return new List<string> { "Agile team", "Customer focus", "Scalable platform" };
    }

    private List<string> GenerateCompetitorWeaknesses()
    {
        return new List<string> { "Limited funding", "Niche market", "Low brand awareness" };
    }

    private List<CompetitorProfile> GenerateCompetitorProfiles(IndustryTemplate template) => new List<CompetitorProfile> { new CompetitorProfile { Name = "Mock Competitor" } };
    private string GenerateCompetitiveMatrix(IndustryTemplate template) => "Mock Matrix";
    private string GenerateMarketPositioning(IndustryTemplate template) => "Mock Positioning";
    private List<string> GenerateDifferentiationOpportunities(IndustryTemplate template) => new List<string> { "Opportunity 1", "Opportunity 2" };
    private List<CustomerSegment> GeneratePrimarySegments(IndustryTemplate template) => new List<CustomerSegment> { new CustomerSegment { Name = "Primary Segment" } };
    private List<CustomerPersona> GeneratePersonaProfiles(IndustryTemplate template) => new List<CustomerPersona> { new CustomerPersona { Name = "Persona" } };
    private List<string> GenerateSegmentPriorities() => new List<string> { "Priority 1", "Priority 2" };
    private Dictionary<string, double> GenerateSegmentSizing(IndustryTemplate template) => new Dictionary<string, double> { { "Segment", 100.0 } };
}

// Supporting data structures
public class IndustryTemplate
{
    public string Name { get; set; } = "";
    public List<string> CompetitorTypes { get; set; } = new();
    public List<string> KeyMetrics { get; set; } = new();
    public List<string> RegulatoryFactors { get; set; } = new();
    public MarketSizingData MarketSize { get; set; } = new();
}

public class MarketSizingData
{
    public decimal TAM { get; set; } // Total Addressable Market
    public decimal SAM { get; set; } // Serviceable Addressable Market  
    public decimal SOM { get; set; } // Serviceable Obtainable Market
}

public class AnalysisMetadata
{
    public TimeSpan ProcessingTime { get; set; }
    public List<string> DataSources { get; set; } = new();
    public Dictionary<string, int> ConfidenceFactors { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}