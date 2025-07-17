using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.AI;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jackson.Ideas.Application.Tests.Services;

public class CompetitiveAnalysisServiceTests
{
    private readonly Mock<IAIOrchestrator> _mockAIOrchestrator;
    private readonly Mock<ILogger<CompetitiveAnalysisService>> _mockLogger;
    private readonly CompetitiveAnalysisService _service;

    public CompetitiveAnalysisServiceTests()
    {
        _mockAIOrchestrator = new Mock<IAIOrchestrator>();
        _mockLogger = new Mock<ILogger<CompetitiveAnalysisService>>();
        
        _service = new CompetitiveAnalysisService(
            _mockAIOrchestrator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeCompetitiveLandscapeAsync_WithValidInput_ShouldReturnCompetitiveAnalysis()
    {
        // Arrange
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app using AI to prioritize tasks and optimize workflows";
        
        var expectedAnalysis = new CompetitiveAnalysisResult
        {
            DirectCompetitors = new List<CompetitorProfile>
            {
                new CompetitorProfile { Name = "Todoist", Description = "Feature-rich task manager with natural language processing" },
                new CompetitorProfile { Name = "Any.do", Description = "Simple, elegant task management with AI-powered suggestions" },
                new CompetitorProfile { Name = "TickTick", Description = "Comprehensive task manager with calendar integration" }
            },
            IndirectCompetitors = new List<CompetitorProfile>
            {
                new CompetitorProfile { Name = "Asana", Description = "Project management platform with task tracking capabilities" },
                new CompetitorProfile { Name = "Notion", Description = "All-in-one workspace with task management features" },
                new CompetitorProfile { Name = "Monday.com", Description = "Work operating system with task management components" }
            },
            SubstituteProducts = new string[]
            {
                "Google Calendar - Basic task and event scheduling",
                "Apple Reminders - Simple task management built into iOS/macOS",
                "Microsoft To Do - Basic task management integrated with Office 365",
                "Paper planners and notebooks - Traditional analog task management"
            },
            CompetitiveAdvantages = new List<string>
            {
                "Advanced AI-powered task prioritization based on user behavior patterns",
                "Intelligent deadline prediction and workload optimization",
                "Smart context switching to minimize productivity loss",
                "Personalized productivity insights and recommendations"
            },
            MarketGaps = new string[]
            {
                "Limited AI integration in existing solutions beyond basic NLP",
                "Poor handling of dynamic priority changes and context switching",
                "Lack of personalized productivity coaching and insights",
                "Insufficient integration with modern AI tools and workflows"
            },
            CompetitiveThreats = new string[]
            {
                "Todoist or Asana could add similar AI features with existing user base",
                "Microsoft could integrate advanced AI into To Do leveraging their AI investments",
                "Google could enhance Calendar/Tasks with their AI capabilities",
                "New well-funded startups entering the AI productivity space"
            },
            StrategicRecommendations = new string[]
            {
                "Focus on AI-powered productivity insights as key differentiator",
                "Build strong data moat through user behavior analytics",
                "Partner with AI tool providers for seamless workflow integration",
                "Target power users who value advanced automation and insights"
            },
            MarketPositioning = "Premium AI-enhanced task manager for productivity-focused professionals",
            CompetitiveRating = 7.2,
            MarketOpportunity = 8.1,
            ThreatLevel = 6.5,
            DifferentiationStrength = 7.8
        };

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalysis);

        // Act
        var result = await _service.AnalyzeCompetitiveLandscapeAsync(ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.DirectCompetitors);
        Assert.NotEmpty(result.IndirectCompetitors);
        Assert.NotEmpty(result.SubstituteProducts);
        Assert.NotEmpty(result.CompetitiveAdvantages);
        Assert.NotEmpty(result.MarketGaps);
        Assert.NotEmpty(result.CompetitiveThreats);
        Assert.NotEmpty(result.StrategicRecommendations);
        Assert.NotEmpty(result.MarketPositioning);
        
        // Verify competitive analysis contains expected elements
        Assert.Contains("Todoist", result.DirectCompetitors[0].Name);
        Assert.Contains("AI-powered", result.CompetitiveAdvantages[0]);
        Assert.Contains("AI integration", result.MarketGaps[0]);
        
        // Verify scores are within valid ranges
        Assert.True(result.CompetitiveRating >= 0 && result.CompetitiveRating <= 10);
        Assert.True(result.MarketOpportunity >= 0 && result.MarketOpportunity <= 10);
        Assert.True(result.ThreatLevel >= 0 && result.ThreatLevel <= 10);
        Assert.True(result.DifferentiationStrength >= 0 && result.DifferentiationStrength <= 10);
        
        // Verify AI orchestrator was called
        _mockAIOrchestrator.Verify(x => x.GenerateCompetitiveAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeCompetitiveLandscapeAsync_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyTitle = "";
        var ideaDescription = "Valid description";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.AnalyzeCompetitiveLandscapeAsync(emptyTitle, ideaDescription));
    }

    [Fact]
    public async Task AnalyzeCompetitiveLandscapeAsync_WithNullDescription_ShouldThrowArgumentException()
    {
        // Arrange
        var ideaTitle = "Valid Title";
        string? nullDescription = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.AnalyzeCompetitiveLandscapeAsync(ideaTitle, nullDescription!));
    }

    [Fact]
    public async Task AnalyzeCompetitiveLandscapeAsync_WhenAIOrchestratorFails_ShouldThrowException()
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(ideaTitle, ideaDescription, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI service failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.AnalyzeCompetitiveLandscapeAsync(ideaTitle, ideaDescription));
    }

    [Fact]
    public async Task AnalyzeMarketPositioningAsync_WithValidInput_ShouldReturnPositioningAnalysis()
    {
        // Arrange
        var ideaTitle = "Smart Home Security System";
        var ideaDescription = "AI-powered security system with facial recognition and behavioral analysis";
        var competitiveAnalysis = new CompetitiveAnalysisResult
        {
            DirectCompetitors = new List<CompetitorProfile>
            {
                new CompetitorProfile { Name = "Ring", Description = "Video doorbell and security camera leader" },
                new CompetitorProfile { Name = "Nest", Description = "Google's smart home security ecosystem" },
                new CompetitorProfile { Name = "Arlo", Description = "Wireless security camera systems" }
            },
            CompetitiveAdvantages = new List<string> { "Advanced AI recognition", "Behavioral analysis" },
            MarketGaps = new string[] { "Limited AI capabilities", "Poor false alarm handling" }
        };
        
        var expectedPositioning = new MarketPositioningResult
        {
            PrimaryPositioning = "Premium AI-enhanced home security for tech-savvy homeowners",
            SecondaryPositioning = "Professional-grade security intelligence for residential use",
            TargetMarketSegments = new List<string>
            {
                "Affluent tech-early adopters seeking advanced security",
                "Security-conscious families in suburban areas",
                "Small business owners needing reliable security"
            },
            ValueProposition = "Reduce false alarms by 90% while increasing threat detection accuracy through advanced AI",
            PricingStrategy = "Premium pricing 40-60% above traditional systems, justified by AI capabilities",
            DistributionChannels = new List<string>
            {
                "Direct-to-consumer online sales",
                "Professional security installer partnerships",
                "Premium home automation retailers"
            },
            MessageingFramework = new List<string>
            {
                "Core message: Finally, home security that actually understands your home",
                "Proof points: 90% fewer false alarms, instant threat recognition",
                "Emotional appeal: Peace of mind through intelligent protection"
            },
            CompetitiveDifferentiation = new List<string>
            {
                "Only system with true behavioral learning AI",
                "Professional-grade threat assessment for homes",
                "Proactive security insights and recommendations"
            },
            MarketEntryStrategy = "Focus on high-value early adopters in tech-forward metropolitan areas",
            PositioningRisks = new List<string>
            {
                "Premium pricing may limit market size",
                "AI complexity could intimidate mainstream users",
                "Privacy concerns around behavioral monitoring"
            }
        };

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(
                It.Is<string>(s => s.Contains("positioning") && s.Contains(ideaTitle)), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompetitiveAnalysisResult
            {
                DirectCompetitors = expectedPositioning.TargetMarketSegments.Select(s => new CompetitorProfile { Name = s, Description = s }).ToList(),
                CompetitiveAdvantages = expectedPositioning.CompetitiveDifferentiation.ToList(),
                MarketGaps = expectedPositioning.PositioningRisks.ToList(),
                MarketPositioning = expectedPositioning.PrimaryPositioning,
                StrategicRecommendations = new string[] { expectedPositioning.MarketEntryStrategy }
            });

        // Act
        var result = await _service.AnalyzeMarketPositioningAsync(ideaTitle, ideaDescription, competitiveAnalysis);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.PrimaryPositioning);
        Assert.NotEmpty(result.TargetMarketSegments);
        Assert.NotEmpty(result.ValueProposition);
        Assert.NotEmpty(result.CompetitiveDifferentiation);
        Assert.NotEmpty(result.MarketEntryStrategy);
        
        // Verify positioning contains relevant keywords
        Assert.Contains("AI", result.PrimaryPositioning);
        Assert.Contains("security", result.PrimaryPositioning);
        
        // Verify AI orchestrator was called with positioning-specific prompt
        _mockAIOrchestrator.Verify(x => x.GenerateCompetitiveAnalysisAsync(
            It.Is<string>(s => s.Contains("positioning")), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task IdentifyCompetitiveThreatsAsync_WithValidInput_ShouldReturnThreatAnalysis()
    {
        // Arrange
        var ideaTitle = "Virtual Fitness Coaching App";
        var ideaDescription = "AI-powered virtual personal trainer with real-time form correction";
        var timeframe = "2-year";
        
        var expectedThreats = new CompetitiveThreatAnalysis
        {
            ImmediateThreats = new List<CompetitiveThreat>
            {
                new CompetitiveThreat
                {
                    ThreatSource = "Peloton",
                    ThreatLevel = "High",
                    Description = "Could add AI form correction to existing platform with large user base",
                    TimeToMarket = "6-12 months",
                    ImpactAssessment = "Would leverage existing brand and customer base",
                    MitigationStrategies = new List<string> { "Focus on broader exercise types beyond cycling", "Build superior AI accuracy" }
                },
                new CompetitiveThreat
                {
                    ThreatSource = "Apple Fitness+",
                    ThreatLevel = "Medium-High",
                    Description = "Integration with Apple Watch could enable form monitoring",
                    TimeToMarket = "12-18 months",
                    ImpactAssessment = "Would have built-in hardware advantage with Apple ecosystem",
                    MitigationStrategies = new List<string> { "Multi-platform approach", "Partner with wearable manufacturers" }
                }
            },
            EmergingThreats = new List<CompetitiveThreat>
            {
                new CompetitiveThreat
                {
                    ThreatSource = "Google Health AI",
                    ThreatLevel = "Medium",
                    Description = "Could integrate fitness coaching into Google Fit ecosystem",
                    TimeToMarket = "18-24 months",
                    ImpactAssessment = "Would leverage Google's AI expertise and Android ecosystem",
                    MitigationStrategies = new List<string> { "Build specialized fitness AI expertise", "Focus on premium user experience" }
                }
            },
            MarketShifts = new List<string>
            {
                "Increasing integration of AI in fitness and health apps",
                "Growing importance of data privacy in health applications",
                "Shift toward holistic wellness platforms rather than single-purpose apps"
            },
            DefensiveStrategies = new List<string>
            {
                "Build strong IP portfolio around AI form correction technology",
                "Create network effects through social features and community",
                "Establish partnerships with fitness equipment manufacturers",
                "Focus on specialized markets where generalist platforms struggle"
            }
        };

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(
                It.Is<string>(s => s.Contains("threat") && s.Contains(timeframe)), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompetitiveAnalysisResult
            {
                CompetitiveThreats = expectedThreats.ImmediateThreats.Concat(expectedThreats.EmergingThreats)
                    .Select(t => $"{t.ThreatSource}: {t.Description}").ToList(),
                StrategicRecommendations = expectedThreats.DefensiveStrategies.ToList(),
                MarketGaps = expectedThreats.MarketShifts.ToList()
            });

        // Act
        var result = await _service.IdentifyCompetitiveThreatsAsync(ideaTitle, ideaDescription, timeframe);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.ImmediateThreats);
        Assert.NotEmpty(result.EmergingThreats);
        Assert.NotEmpty(result.MarketShifts);
        Assert.NotEmpty(result.DefensiveStrategies);
        
        // Verify threat analysis structure
        Assert.All(result.ImmediateThreats, threat =>
        {
            Assert.NotEmpty(threat.ThreatSource);
            Assert.NotEmpty(threat.ThreatLevel);
            Assert.NotEmpty(threat.Description);
            Assert.NotEmpty(threat.TimeToMarket);
            Assert.NotEmpty(threat.ImpactAssessment);
            Assert.NotEmpty(threat.MitigationStrategies);
        });
        
        // Verify AI orchestrator was called with threat-specific analysis
        _mockAIOrchestrator.Verify(x => x.GenerateCompetitiveAnalysisAsync(
            It.Is<string>(s => s.Contains("threat") && s.Contains(timeframe)), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Theory]
    [InlineData("1-year")]
    [InlineData("2-year")]
    [InlineData("5-year")]
    public async Task IdentifyCompetitiveThreatsAsync_WithDifferentTimeframes_ShouldAdjustAnalysis(string timeframe)
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        
        var mockResult = new CompetitiveAnalysisResult
        {
            CompetitiveThreats = new string[] { "Test threat" },
            StrategicRecommendations = new string[] { "Test strategy" }
        };

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _service.IdentifyCompetitiveThreatsAsync(ideaTitle, ideaDescription, timeframe);

        // Assert
        Assert.NotNull(result);
        
        // Verify the timeframe was included in the analysis prompt
        _mockAIOrchestrator.Verify(x => x.GenerateCompetitiveAnalysisAsync(
            It.Is<string>(s => s.Contains(timeframe)), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task CompareWithCompetitorsAsync_WithValidInput_ShouldReturnComparison()
    {
        // Arrange
        var ideaTitle = "AI Code Review Tool";
        var ideaDescription = "Automated code review with AI-powered suggestions";
        var competitors = new[] { "GitHub Copilot", "SonarQube", "CodeClimate" };
        
        var expectedComparison = new CompetitorComparisonResult
        {
            ComparisonMatrix = new Dictionary<string, CompetitorComparison>
            {
                ["GitHub Copilot"] = new CompetitorComparison
                {
                    CompetitorName = "GitHub Copilot",
                    Strengths = new List<string> { "Massive training data", "GitHub integration", "Microsoft backing" },
                    Weaknesses = new List<string> { "Limited to code generation", "Privacy concerns", "Subscription cost" },
                    MarketPosition = "Code generation leader",
                    CompetitiveAdvantage = "First-mover advantage in AI coding assistance",
                    ThreatLevel = "High",
                    DifferentiationOpportunities = new List<string> { "Focus on code review vs generation", "Better security analysis" }
                },
                ["SonarQube"] = new CompetitorComparison
                {
                    CompetitorName = "SonarQube",
                    Strengths = new List<string> { "Established in code quality", "Enterprise adoption", "Multiple languages" },
                    Weaknesses = new List<string> { "Limited AI capabilities", "Complex setup", "Dated UI/UX" },
                    MarketPosition = "Traditional code quality leader",
                    CompetitiveAdvantage = "Deep enterprise relationships",
                    ThreatLevel = "Medium",
                    DifferentiationOpportunities = new List<string> { "Superior AI analysis", "Better developer experience" }
                }
            },
            OverallMarketPosition = "AI-first code review positioned between code generation and traditional quality tools",
            KeyDifferentiators = new List<string>
            {
                "Focus specifically on code review quality rather than generation",
                "Advanced AI understanding of code patterns and best practices",
                "Better integration with modern development workflows"
            },
            CompetitiveGaps = new List<string>
            {
                "Limited AI-powered code review tools in market",
                "Gap between simple linting and complex code generation",
                "Need for context-aware security and performance analysis"
            }
        };

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(
                It.Is<string>(s => s.Contains("compare") && competitors.All(c => s.Contains(c))), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompetitiveAnalysisResult
            {
                DirectCompetitors = expectedComparison.ComparisonMatrix.Values.Select(c => new CompetitorProfile { Name = c.CompetitorName, Description = c.MarketPosition }).ToList(),
                CompetitiveAdvantages = expectedComparison.KeyDifferentiators.ToList(),
                MarketGaps = expectedComparison.CompetitiveGaps.ToList(),
                MarketPositioning = expectedComparison.OverallMarketPosition
            });

        // Act
        var result = await _service.CompareWithCompetitorsAsync(ideaTitle, ideaDescription, competitors);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.ComparisonMatrix);
        Assert.NotEmpty(result.OverallMarketPosition);
        Assert.NotEmpty(result.KeyDifferentiators);
        Assert.NotEmpty(result.CompetitiveGaps);
        
        // Verify comparison includes all specified competitors
        foreach (var competitor in competitors)
        {
            Assert.Contains(competitor, result.ComparisonMatrix.Keys);
        }
        
        // Verify each competitor comparison is complete
        Assert.All(result.ComparisonMatrix.Values, comparison =>
        {
            Assert.NotEmpty(comparison.CompetitorName);
            Assert.NotEmpty(comparison.Strengths);
            Assert.NotEmpty(comparison.Weaknesses);
            Assert.NotEmpty(comparison.MarketPosition);
        });
        
        // Verify AI orchestrator was called with comparison prompt
        _mockAIOrchestrator.Verify(x => x.GenerateCompetitiveAnalysisAsync(
            It.Is<string>(s => s.Contains("compare") && competitors.All(c => s.Contains(c))), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task CompareWithCompetitorsAsync_WithEmptyCompetitorsList_ShouldThrowArgumentException()
    {
        // Arrange
        var ideaTitle = "Test Idea";
        var ideaDescription = "Test Description";
        var emptyCompetitors = Array.Empty<string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CompareWithCompetitorsAsync(ideaTitle, ideaDescription, emptyCompetitors));
    }

    [Fact]
    public async Task AnalyzeBarriersToEntryAsync_WithValidInput_ShouldReturnBarrierAnalysis()
    {
        // Arrange
        var ideaTitle = "Quantum Computing Cloud Service";
        var ideaDescription = "Cloud-based quantum computing platform for enterprises";
        var marketContext = "Emerging quantum computing market with high technical barriers";
        
        var expectedBarriers = new MarketBarrierAnalysis
        {
            TechnicalBarriers = new List<string>
            {
                "Requires quantum physics expertise and specialized talent",
                "Need for extremely expensive quantum hardware infrastructure",
                "Complex quantum error correction and algorithm development"
            },
            FinancialBarriers = new List<string>
            {
                "Minimum $100M+ investment for viable quantum hardware",
                "Ongoing R&D costs for quantum algorithm development",
                "High operational costs for quantum computer maintenance"
            },
            RegulatoryBarriers = new List<string>
            {
                "Export control restrictions on quantum technology",
                "National security considerations for quantum access",
                "Emerging quantum cryptography regulations"
            },
            NetworkEffectBarriers = new List<string>
            {
                "Quantum ecosystem requires critical mass of developers",
                "Need partnerships with hardware manufacturers",
                "Academic and research institution relationships crucial"
            },
            BrandBarriers = new List<string>
            {
                "IBM and Google have early quantum computing brand recognition",
                "Enterprise trust required for mission-critical quantum workloads",
                "Need to establish thought leadership in quantum algorithms"
            },
            BarrierHeight = "Very High",
            EntryDifficulty = 9.2,
            TimeToEntry = "5-7 years",
            CapitalRequirement = "$500M+",
            MitigationStrategies = new List<string>
            {
                "Partner with existing quantum hardware providers",
                "Focus on specific quantum algorithms and applications",
                "Build quantum software layer rather than full stack",
                "Target specific industries with clear quantum advantage"
            }
        };

        _mockAIOrchestrator.Setup(x => x.GenerateCompetitiveAnalysisAsync(
                It.Is<string>(s => s.Contains("barriers") && s.Contains("entry")), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompetitiveAnalysisResult
            {
                MarketGaps = expectedBarriers.TechnicalBarriers
                    .Concat(expectedBarriers.FinancialBarriers)
                    .Concat(expectedBarriers.RegulatoryBarriers).ToList(),
                StrategicRecommendations = expectedBarriers.MitigationStrategies.ToList(),
                ThreatLevel = expectedBarriers.EntryDifficulty
            });

        // Act
        var result = await _service.AnalyzeBarriersToEntryAsync(ideaTitle, ideaDescription, marketContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.TechnicalBarriers);
        Assert.NotEmpty(result.FinancialBarriers);
        Assert.NotEmpty(result.MitigationStrategies);
        Assert.NotEmpty(result.BarrierHeight);
        Assert.NotEmpty(result.TimeToEntry);
        Assert.NotEmpty(result.CapitalRequirement);
        
        // Verify barrier analysis is comprehensive
        Assert.True(result.EntryDifficulty >= 0 && result.EntryDifficulty <= 10);
        
        // Verify AI orchestrator was called with barrier analysis prompt
        _mockAIOrchestrator.Verify(x => x.GenerateCompetitiveAnalysisAsync(
            It.Is<string>(s => s.Contains("barriers") && s.Contains("entry")), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}

// Supporting classes for competitive analysis tests
public class MarketPositioningResult
{
    public string PrimaryPositioning { get; set; } = string.Empty;
    public string SecondaryPositioning { get; set; } = string.Empty;
    public List<string> TargetMarketSegments { get; set; } = new();
    public string ValueProposition { get; set; } = string.Empty;
    public string PricingStrategy { get; set; } = string.Empty;
    public List<string> DistributionChannels { get; set; } = new();
    public List<string> MessageingFramework { get; set; } = new();
    public List<string> CompetitiveDifferentiation { get; set; } = new();
    public string MarketEntryStrategy { get; set; } = string.Empty;
    public List<string> PositioningRisks { get; set; } = new();
}

public class CompetitiveThreatAnalysis
{
    public List<CompetitiveThreat> ImmediateThreats { get; set; } = new();
    public List<CompetitiveThreat> EmergingThreats { get; set; } = new();
    public List<string> MarketShifts { get; set; } = new();
    public List<string> DefensiveStrategies { get; set; } = new();
}

public class CompetitiveThreat
{
    public string ThreatSource { get; set; } = string.Empty;
    public string ThreatLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeToMarket { get; set; } = string.Empty;
    public string ImpactAssessment { get; set; } = string.Empty;
    public List<string> MitigationStrategies { get; set; } = new();
}

public class CompetitorComparisonResult
{
    public Dictionary<string, CompetitorComparison> ComparisonMatrix { get; set; } = new();
    public string OverallMarketPosition { get; set; } = string.Empty;
    public List<string> KeyDifferentiators { get; set; } = new();
    public List<string> CompetitiveGaps { get; set; } = new();
}

public class CompetitorComparison
{
    public string CompetitorName { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string MarketPosition { get; set; } = string.Empty;
    public string CompetitiveAdvantage { get; set; } = string.Empty;
    public string ThreatLevel { get; set; } = string.Empty;
    public List<string> DifferentiationOpportunities { get; set; } = new();
}

public class MarketBarrierAnalysis
{
    public List<string> TechnicalBarriers { get; set; } = new();
    public List<string> FinancialBarriers { get; set; } = new();
    public List<string> RegulatoryBarriers { get; set; } = new();
    public List<string> NetworkEffectBarriers { get; set; } = new();
    public List<string> BrandBarriers { get; set; } = new();
    public string BarrierHeight { get; set; } = string.Empty;
    public double EntryDifficulty { get; set; }
    public string TimeToEntry { get; set; } = string.Empty;
    public string CapitalRequirement { get; set; } = string.Empty;
    public List<string> MitigationStrategies { get; set; } = new();
}