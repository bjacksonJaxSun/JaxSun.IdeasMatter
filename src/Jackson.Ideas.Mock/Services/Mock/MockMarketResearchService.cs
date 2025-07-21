using Jackson.Ideas.Mock.Models;
using Jackson.Ideas.Mock.Services.Interfaces;

namespace Jackson.Ideas.Mock.Services.Mock;

public class MockMarketResearchService : IMarketResearchService
{
    private readonly List<MarketResearchData> _marketResearchData;

    public MockMarketResearchService()
    {
        _marketResearchData = GenerateMarketResearchData();
    }

    public async Task<MarketResearchData?> GetMarketResearchAsync(string scenarioId)
    {
        await Task.Delay(150);
        return _marketResearchData.FirstOrDefault(m => m.ScenarioId == scenarioId);
    }

    public async Task<List<MarketResearchData>> GetAllMarketResearchAsync()
    {
        await Task.Delay(200);
        return _marketResearchData;
    }

    public async Task<MarketResearchData?> GetMarketResearchByIndustryAsync(string industry)
    {
        await Task.Delay(100);
        return _marketResearchData.FirstOrDefault(m => m.Industry.Equals(industry, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<Competitor>> GetCompetitorsAsync(string industry)
    {
        await Task.Delay(100);
        var marketData = _marketResearchData.FirstOrDefault(m => m.Industry.Equals(industry, StringComparison.OrdinalIgnoreCase));
        return marketData?.Competition.DirectCompetitors ?? new List<Competitor>();
    }

    public async Task<MarketTrends> GetMarketTrendsAsync(string industry)
    {
        await Task.Delay(100);
        var marketData = _marketResearchData.FirstOrDefault(m => m.Industry.Equals(industry, StringComparison.OrdinalIgnoreCase));
        return marketData?.Trends ?? new MarketTrends();
    }

    private static List<MarketResearchData> GenerateMarketResearchData()
    {
        return new List<MarketResearchData>
        {
            new MarketResearchData
            {
                ScenarioId = "eco-eats-001",
                Industry = "Food & Beverage",
                Segmentation = new MarketSegmentation
                {
                    TotalMarketSize = "$150B",
                    ServicableMarketSize = "$2.4B",
                    TargetMarketSize = "$480M",
                    GrowthRate = "15% annually",
                    Segments = new List<MarketSegment>
                    {
                        new MarketSegment
                        {
                            Name = "Eco-Conscious Millennials",
                            Size = "$280M",
                            Demographics = "Ages 25-35, urban, college-educated",
                            Characteristics = new List<string> { "Environmental awareness", "Willing to pay premium", "Tech-savvy" },
                            PurchasingPower = "High - $75K+ household income"
                        },
                        new MarketSegment
                        {
                            Name = "Sustainable Gen Z",
                            Size = "$200M",
                            Demographics = "Ages 18-28, urban and suburban",
                            Characteristics = new List<string> { "Sustainability focused", "Price sensitive", "Social media influenced" },
                            PurchasingPower = "Medium - $45K+ household income"
                        }
                    }
                },
                Competition = new CompetitiveAnalysis
                {
                    DirectCompetitors = new List<Competitor>
                    {
                        new Competitor
                        {
                            Name = "DoorDash",
                            MarketShare = "35%",
                            Strengths = "Largest network, fast delivery",
                            Weaknesses = "Limited sustainability focus",
                            PricingStrategy = "Competitive with promotions",
                            DifferentiationFactor = "Speed and convenience"
                        },
                        new Competitor
                        {
                            Name = "Uber Eats",
                            MarketShare = "28%",
                            Strengths = "Strong brand, technology",
                            Weaknesses = "High fees, environmental impact",
                            PricingStrategy = "Premium pricing",
                            DifferentiationFactor = "Uber ecosystem integration"
                        }
                    },
                    IndirectCompetitors = new List<Competitor>
                    {
                        new Competitor
                        {
                            Name = "Local Pickup Services",
                            MarketShare = "5%",
                            Strengths = "Personal relationships, lower costs",
                            Weaknesses = "Limited scale, technology gaps",
                            PricingStrategy = "Low-cost",
                            DifferentiationFactor = "Community connection"
                        }
                    },
                    CompetitiveAdvantages = new List<string>
                    {
                        "First-mover in sustainable delivery",
                        "Verified eco-friendly restaurant network",
                        "Green Score differentiation",
                        "Electric vehicle delivery fleet"
                    },
                    MarketGaps = new List<string>
                    {
                        "Sustainability-focused delivery platform",
                        "Transparent environmental impact tracking",
                        "Local restaurant support programs"
                    },
                    CompetitiveIntensity = "High - Established players with deep pockets"
                },
                Trends = new MarketTrends
                {
                    EmergingTrends = new List<string>
                    {
                        "Increased demand for sustainable packaging",
                        "Growing environmental consciousness",
                        "Local business support movements",
                        "Carbon footprint transparency"
                    },
                    TechnologyTrends = new List<string>
                    {
                        "Electric vehicle delivery adoption",
                        "AI-powered route optimization",
                        "Sustainable packaging innovations",
                        "Real-time environmental impact tracking"
                    },
                    ConsumerBehaviorTrends = new List<string>
                    {
                        "Willingness to pay premium for sustainability",
                        "Preference for local businesses",
                        "Increased scrutiny of environmental claims",
                        "Demand for transparency in supply chain"
                    },
                    RegulatoryTrends = new List<string>
                    {
                        "Plastic packaging restrictions",
                        "Carbon emission regulations",
                        "Food delivery licensing requirements"
                    },
                    MarketMaturity = "Growth stage with increasing consolidation"
                },
                Customer = new CustomerInsights
                {
                    PainPoints = new List<string>
                    {
                        "Excessive packaging waste",
                        "Lack of sustainable options",
                        "Unknown environmental impact",
                        "Limited local restaurant options"
                    },
                    Motivations = new List<string>
                    {
                        "Environmental responsibility",
                        "Supporting local businesses",
                        "Convenience without guilt",
                        "Quality food delivery"
                    },
                    PreferredChannels = new List<string>
                    {
                        "Mobile apps",
                        "Social media recommendations",
                        "Environmental blogs and websites",
                        "Word-of-mouth referrals"
                    },
                    PurchaseDecisionFactors = "Environmental impact (40%), food quality (35%), price (25%)",
                    CustomerLifetimeValue = "$2,400 over 24 months",
                    AcquisitionCost = "$45 per customer"
                },
                Regulatory = new RegulatoryEnvironment
                {
                    KeyRegulations = new List<string>
                    {
                        "Food safety regulations",
                        "Delivery driver classification",
                        "Environmental packaging standards"
                    },
                    ComplianceRequirements = new List<string>
                    {
                        "Food handling certifications",
                        "Vehicle safety inspections",
                        "Environmental impact reporting"
                    },
                    UpcomingChanges = new List<string>
                    {
                        "Single-use plastic bans",
                        "Carbon emission tracking requirements",
                        "Gig worker protection laws"
                    },
                    RegulatoryRisk = "Medium - Evolving environmental regulations"
                }
            },
            new MarketResearchData
            {
                ScenarioId = "ai-study-buddy-002",
                Industry = "Education Technology",
                Segmentation = new MarketSegmentation
                {
                    TotalMarketSize = "$5.2B",
                    ServicableMarketSize = "$850M",
                    TargetMarketSize = "$170M",
                    GrowthRate = "22% annually",
                    Segments = new List<MarketSegment>
                    {
                        new MarketSegment
                        {
                            Name = "K-12 Students",
                            Size = "$400M",
                            Demographics = "Ages 6-18, all income levels",
                            Characteristics = new List<string> { "Digital natives", "Personalized learning needs", "Homework support" },
                            PurchasingPower = "Parent-dependent - varies widely"
                        },
                        new MarketSegment
                        {
                            Name = "Adult Learners",
                            Size = "$280M",
                            Demographics = "Ages 25-45, professional development",
                            Characteristics = new List<string> { "Career focused", "Time constrained", "Self-motivated" },
                            PurchasingPower = "High - $65K+ individual income"
                        }
                    }
                },
                Competition = new CompetitiveAnalysis
                {
                    DirectCompetitors = new List<Competitor>
                    {
                        new Competitor
                        {
                            Name = "Khan Academy",
                            MarketShare = "25%",
                            Strengths = "Free content, established brand",
                            Weaknesses = "Limited personalization, no real-time tutoring",
                            PricingStrategy = "Freemium model",
                            DifferentiationFactor = "Comprehensive curriculum coverage"
                        },
                        new Competitor
                        {
                            Name = "Duolingo",
                            MarketShare = "20%",
                            Strengths = "Gamification, habit formation",
                            Weaknesses = "Limited to language learning",
                            PricingStrategy = "Freemium with premium subscriptions",
                            DifferentiationFactor = "Gamified learning experience"
                        }
                    },
                    IndirectCompetitors = new List<Competitor>
                    {
                        new Competitor
                        {
                            Name = "Traditional Tutoring",
                            MarketShare = "15%",
                            Strengths = "Personal connection, customized approach",
                            Weaknesses = "Expensive, scheduling challenges",
                            PricingStrategy = "Premium hourly rates ($50-100+)",
                            DifferentiationFactor = "Human interaction and expertise"
                        }
                    },
                    CompetitiveAdvantages = new List<string>
                    {
                        "AI-powered personalization",
                        "Real-time adaptive learning",
                        "Multi-subject platform",
                        "Human tutor integration"
                    },
                    MarketGaps = new List<string>
                    {
                        "Truly personalized AI tutoring",
                        "Cross-subject knowledge integration",
                        "Real-time learning adaptation"
                    },
                    CompetitiveIntensity = "Medium-High - Many players but differentiation possible"
                },
                Trends = new MarketTrends
                {
                    EmergingTrends = new List<string>
                    {
                        "AI-powered personalized learning",
                        "Remote and hybrid education adoption",
                        "Microlearning and bite-sized content",
                        "Learning analytics and progress tracking"
                    },
                    TechnologyTrends = new List<string>
                    {
                        "Machine learning algorithms for education",
                        "Natural language processing for tutoring",
                        "Adaptive assessment technologies",
                        "VR/AR educational experiences"
                    },
                    ConsumerBehaviorTrends = new List<string>
                    {
                        "Demand for flexible learning schedules",
                        "Preference for self-paced learning",
                        "Expectation of immediate feedback",
                        "Multi-modal learning preferences"
                    },
                    RegulatoryTrends = new List<string>
                    {
                        "Student data privacy regulations",
                        "Educational technology standards",
                        "AI ethics in education guidelines"
                    },
                    MarketMaturity = "Growth stage with rapid innovation"
                },
                Customer = new CustomerInsights
                {
                    PainPoints = new List<string>
                    {
                        "One-size-fits-all learning approaches",
                        "Lack of immediate feedback",
                        "Difficulty finding quality tutoring",
                        "Expensive traditional tutoring costs"
                    },
                    Motivations = new List<string>
                    {
                        "Academic achievement",
                        "Personalized learning experience",
                        "Flexible scheduling",
                        "Cost-effective education support"
                    },
                    PreferredChannels = new List<string>
                    {
                        "Mobile apps",
                        "School recommendations",
                        "Parent networks and forums",
                        "Educational technology reviews"
                    },
                    PurchaseDecisionFactors = "Educational effectiveness (50%), ease of use (30%), price (20%)",
                    CustomerLifetimeValue = "$1,800 over 18 months",
                    AcquisitionCost = "$35 per customer"
                },
                Regulatory = new RegulatoryEnvironment
                {
                    KeyRegulations = new List<string>
                    {
                        "FERPA (student privacy)",
                        "COPPA (children's online privacy)",
                        "State education standards compliance"
                    },
                    ComplianceRequirements = new List<string>
                    {
                        "Student data protection protocols",
                        "Educational content standards",
                        "Accessibility compliance (ADA)"
                    },
                    UpcomingChanges = new List<string>
                    {
                        "AI ethics guidelines for education",
                        "Enhanced student data protection",
                        "Educational technology certification standards"
                    },
                    RegulatoryRisk = "Medium - Strict data privacy requirements"
                }
            },
            new MarketResearchData
            {
                ScenarioId = "fitness-vr-004",
                Industry = "Health & Fitness Technology",
                Segmentation = new MarketSegmentation
                {
                    TotalMarketSize = "$15.6B",
                    ServicableMarketSize = "$4.5B",
                    TargetMarketSize = "$900M",
                    GrowthRate = "8.7% annually",
                    Segments = new List<MarketSegment>
                    {
                        new MarketSegment
                        {
                            Name = "VR Gaming Fitness Enthusiasts",
                            Size = "$350M",
                            Demographics = "Ages 18-35, tech-savvy, urban",
                            Characteristics = new List<string> { "Early VR adopters", "Gaming background", "Home fitness preference" },
                            PurchasingPower = "High - $70K+ household income"
                        },
                        new MarketSegment
                        {
                            Name = "Remote Workers Seeking Fitness",
                            Size = "$550M",
                            Demographics = "Ages 25-45, professional, work-from-home",
                            Characteristics = new List<string> { "Time-constrained", "Convenience-focused", "Health-conscious" },
                            PurchasingPower = "High - $85K+ household income"
                        }
                    }
                },
                Competition = new CompetitiveAnalysis
                {
                    DirectCompetitors = new List<Competitor>
                    {
                        new Competitor
                        {
                            Name = "Beat Saber Fitness",
                            MarketShare = "15%",
                            Strengths = "Popular game mechanics, strong community",
                            Weaknesses = "Limited exercise variety, not fitness-focused",
                            PricingStrategy = "One-time purchase model",
                            DifferentiationFactor = "Rhythm-based gaming experience"
                        },
                        new Competitor
                        {
                            Name = "Supernatural",
                            MarketShare = "12%",
                            Strengths = "High-quality workouts, scenic environments",
                            Weaknesses = "Requires Oculus platform, subscription model",
                            PricingStrategy = "Monthly subscription $19/month",
                            DifferentiationFactor = "Premium workout experiences"
                        }
                    },
                    IndirectCompetitors = new List<Competitor>
                    {
                        new Competitor
                        {
                            Name = "Peloton",
                            MarketShare = "8%",
                            Strengths = "Strong brand, community features",
                            Weaknesses = "Expensive hardware, limited exercise types",
                            PricingStrategy = "Hardware + subscription model",
                            DifferentiationFactor = "Live instructor-led classes"
                        }
                    },
                    CompetitiveAdvantages = new List<string>
                    {
                        "Immersive VR fitness experiences",
                        "Gamified workout progression",
                        "Social community features",
                        "Diverse exercise modalities"
                    },
                    MarketGaps = new List<string>
                    {
                        "Comprehensive VR fitness platform",
                        "Social workout experiences in VR",
                        "Progressive fitness tracking in virtual environments"
                    },
                    CompetitiveIntensity = "Medium - Emerging market with room for innovation"
                },
                Trends = new MarketTrends
                {
                    EmergingTrends = new List<string>
                    {
                        "VR/AR adoption in fitness",
                        "Gamification of exercise routines",
                        "Home fitness equipment innovation",
                        "Social fitness and virtual communities"
                    },
                    TechnologyTrends = new List<string>
                    {
                        "Improved VR hardware accessibility",
                        "Haptic feedback integration",
                        "Biometric tracking in VR",
                        "AI-powered workout personalization"
                    },
                    ConsumerBehaviorTrends = new List<string>
                    {
                        "Preference for home workouts",
                        "Demand for engaging fitness experiences",
                        "Social fitness community participation",
                        "Technology integration in health routines"
                    },
                    RegulatoryTrends = new List<string>
                    {
                        "VR safety standards development",
                        "Health data privacy regulations",
                        "Fitness device certification requirements"
                    },
                    MarketMaturity = "Early growth stage with high innovation potential"
                },
                Customer = new CustomerInsights
                {
                    PainPoints = new List<string>
                    {
                        "Boring traditional exercise routines",
                        "Lack of motivation for home workouts",
                        "Limited social interaction in fitness",
                        "Expensive gym memberships and equipment"
                    },
                    Motivations = new List<string>
                    {
                        "Engaging and fun fitness experiences",
                        "Convenience of home workouts",
                        "Social connection through fitness",
                        "Tracking progress and achievements"
                    },
                    PreferredChannels = new List<string>
                    {
                        "VR gaming platforms",
                        "Fitness technology reviews",
                        "Social media fitness communities",
                        "Gaming and technology influencers"
                    },
                    PurchaseDecisionFactors = "Entertainment value (45%), fitness effectiveness (35%), technology quality (20%)",
                    CustomerLifetimeValue = "$3,200 over 30 months",
                    AcquisitionCost = "$65 per customer"
                },
                Regulatory = new RegulatoryEnvironment
                {
                    KeyRegulations = new List<string>
                    {
                        "Consumer product safety standards",
                        "Health data privacy laws",
                        "VR content rating guidelines"
                    },
                    ComplianceRequirements = new List<string>
                    {
                        "VR safety warnings and guidelines",
                        "Biometric data protection protocols",
                        "Content appropriateness standards"
                    },
                    UpcomingChanges = new List<string>
                    {
                        "VR-specific safety regulations",
                        "Enhanced health data protection",
                        "Fitness technology certification standards"
                    },
                    RegulatoryRisk = "Low-Medium - Evolving VR safety standards"
                }
            }
        };
    }
}