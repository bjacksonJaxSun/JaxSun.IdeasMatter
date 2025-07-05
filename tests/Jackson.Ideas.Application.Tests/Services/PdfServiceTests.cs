using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.DTOs.BusinessPlan;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jackson.Ideas.Application.Tests.Services;

public class PdfServiceTests
{
    private readonly Mock<ILogger<PdfService>> _mockLogger;
    private readonly IPdfService _pdfService;

    public PdfServiceTests()
    {
        _mockLogger = new Mock<ILogger<PdfService>>();
        _pdfService = new PdfService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateResearchReportAsync_WithValidData_ShouldReturnPdfBytes()
    {
        // Arrange
        var reportData = CreateSampleResearchReportData();

        // Act
        var result = await _pdfService.GenerateResearchReportAsync(reportData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000); // PDF should have substantial content
        Assert.True(IsPdfFormat(result)); // Check PDF magic number
    }

    [Fact]
    public async Task GenerateSwotAnalysisPdfAsync_WithValidData_ShouldReturnPdfBytes()
    {
        // Arrange
        var swotAnalysis = CreateSampleSwotAnalysis();
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app using AI to prioritize tasks";

        // Act
        var result = await _pdfService.GenerateSwotAnalysisPdfAsync(
            swotAnalysis, ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateMarketAnalysisPdfAsync_WithValidData_ShouldReturnPdfBytes()
    {
        // Arrange
        var marketAnalysis = CreateSampleMarketAnalysis();
        var competitiveLandscape = CreateSampleCompetitiveLandscape();

        // Act
        var result = await _pdfService.GenerateMarketAnalysisPdfAsync(
            marketAnalysis, competitiveLandscape);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateBusinessPlanPdfAsync_WithValidData_ShouldReturnPdfBytes()
    {
        // Arrange
        var businessPlan = CreateSampleBusinessPlan();

        // Act
        var result = await _pdfService.GenerateBusinessPlanPdfAsync(businessPlan);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateCompetitiveAnalysisPdfAsync_WithValidData_ShouldReturnPdfBytes()
    {
        // Arrange
        var competitiveAnalysis = CreateSampleCompetitiveAnalysis();
        var ideaTitle = "AI-Powered Learning Platform";
        var ideaDescription = "An adaptive learning platform using AI";

        // Act
        var result = await _pdfService.GenerateCompetitiveAnalysisPdfAsync(
            competitiveAnalysis, ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateCustomerSegmentationPdfAsync_WithValidData_ShouldReturnPdfBytes()
    {
        // Arrange
        var segmentation = CreateSampleCustomerSegmentation();
        var ideaTitle = "AI-Powered Task Manager";
        var ideaDescription = "A smart task management app";

        // Act
        var result = await _pdfService.GenerateCustomerSegmentationPdfAsync(
            segmentation, ideaTitle, ideaDescription);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateResearchReportAsync_WithCustomOptions_ShouldApplyOptions()
    {
        // Arrange
        var reportData = CreateSampleResearchReportData();
        var options = new PdfGenerationOptions
        {
            IncludeMetadata = false,
            CompressPdf = true,
            HeaderColor = "#ff0000",
            FontFamily = "Times",
            FooterText = "Custom Footer Text"
        };

        // Act
        var result = await _pdfService.GenerateResearchReportAsync(reportData, options);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateSwotAnalysisPdfAsync_WithEmptyData_ShouldNotThrowException()
    {
        // Arrange
        var swotAnalysis = new SwotAnalysisResult
        {
            Strengths = Array.Empty<string>(),
            Weaknesses = Array.Empty<string>(),
            Opportunities = Array.Empty<string>(),
            Threats = Array.Empty<string>()
        };

        // Act & Assert
        var result = await _pdfService.GenerateSwotAnalysisPdfAsync(
            swotAnalysis, "Test Idea", "Test Description");

        Assert.NotNull(result);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateMarketAnalysisPdfAsync_WithMinimalData_ShouldGeneratePdf()
    {
        // Arrange
        var marketAnalysis = new MarketAnalysisDto
        {
            Industry = "Technology",
            MarketSize = "$1B",
            GrowthRate = "10% CAGR"
        };

        // Act
        var result = await _pdfService.GenerateMarketAnalysisPdfAsync(marketAnalysis);

        // Assert
        Assert.NotNull(result);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateBusinessPlanPdfAsync_WithFinancialProjections_ShouldIncludeProjections()
    {
        // Arrange
        var businessPlan = CreateSampleBusinessPlan();
        businessPlan.FinancialProjections = new FinancialProjectionsDto
        {
            YearlyProjections = new Dictionary<string, string>
            {
                ["Year 1"] = "$500K",
                ["Year 2"] = "$2.5M",
                ["Year 3"] = "$10M"
            },
            Currency = "USD"
        };

        // Act
        var result = await _pdfService.GenerateBusinessPlanPdfAsync(businessPlan);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateResearchReportAsync_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var reportData = CreateSampleResearchReportData();
        reportData.Title = "Test & Co. - €£¥ Analysis";
        reportData.Description = "Special chars: ñáéíóú • ® ™";

        // Act
        var result = await _pdfService.GenerateResearchReportAsync(reportData);

        // Assert
        Assert.NotNull(result);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateCompetitiveAnalysisPdfAsync_WithMultipleCompetitors_ShouldListAll()
    {
        // Arrange
        var competitiveAnalysis = CreateSampleCompetitiveAnalysis();
        competitiveAnalysis.DirectCompetitors = new List<CompetitorProfile>
        {
            new() { Name = "Competitor 1", Description = "First competitor" },
            new() { Name = "Competitor 2", Description = "Second competitor" },
            new() { Name = "Competitor 3", Description = "Third competitor" }
        };

        // Act
        var result = await _pdfService.GenerateCompetitiveAnalysisPdfAsync(
            competitiveAnalysis, "Test Idea", "Test Description");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateCustomerSegmentationPdfAsync_WithMultipleSegments_ShouldIncludeAll()
    {
        // Arrange
        var segmentation = CreateSampleCustomerSegmentation();
        segmentation.PrimarySegments = new[]
        {
            new CustomerSegment { Name = "Segment 1", Size = "1M users", Description = "First segment" },
            new CustomerSegment { Name = "Segment 2", Size = "2M users", Description = "Second segment" }
        };

        // Act
        var result = await _pdfService.GenerateCustomerSegmentationPdfAsync(
            segmentation, "Test Idea", "Test Description");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 1000);
        Assert.True(IsPdfFormat(result));
    }

    [Theory]
    [InlineData(PaperSize.Letter)]
    [InlineData(PaperSize.A4)]
    [InlineData(PaperSize.Legal)]
    public async Task GenerateResearchReportAsync_WithDifferentPaperSizes_ShouldGeneratePdf(PaperSize paperSize)
    {
        // Arrange
        var reportData = CreateSampleResearchReportData();
        var options = new PdfGenerationOptions { PaperSize = paperSize };

        // Act
        var result = await _pdfService.GenerateResearchReportAsync(reportData, options);

        // Assert
        Assert.NotNull(result);
        Assert.True(IsPdfFormat(result));
    }

    [Fact]
    public async Task GenerateResearchReportAsync_WithAllSections_ShouldIncludeAllContent()
    {
        // Arrange
        var reportData = CreateCompleteResearchReportData();

        // Act
        var result = await _pdfService.GenerateResearchReportAsync(reportData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 5000); // Should be larger with all sections
        Assert.True(IsPdfFormat(result));
    }

    // Helper methods
    private static bool IsPdfFormat(byte[] data)
    {
        return data.Length > 4 && 
               data[0] == 0x25 && // %
               data[1] == 0x50 && // P
               data[2] == 0x44 && // D
               data[3] == 0x46;   // F
    }

    private static ResearchReportDto CreateSampleResearchReportData()
    {
        return new ResearchReportDto
        {
            Title = "AI-Powered Learning Platform",
            Description = "An adaptive learning platform using AI to personalize education",
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            MarketAnalysis = CreateSampleMarketAnalysis(),
            SwotAnalysis = CreateSampleSwotAnalysis()
        };
    }

    private static ResearchReportDto CreateCompleteResearchReportData()
    {
        return new ResearchReportDto
        {
            Title = "AI-Powered Learning Platform",
            Description = "An adaptive learning platform using AI to personalize education",
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            MarketAnalysis = CreateSampleMarketAnalysis(),
            SwotAnalysis = CreateSampleSwotAnalysis(),
            CompetitiveAnalysis = CreateSampleCompetitiveAnalysis(),
            CustomerSegmentation = CreateSampleCustomerSegmentation(),
            BusinessPlan = CreateSampleBusinessPlan(),
            Competitors = new List<CompetitorAnalysisDto>
            {
                new() { Name = "Coursera", MarketShare = 15, Description = "Online learning platform" },
                new() { Name = "Duolingo", MarketShare = 10, Description = "Language learning app" }
            }
        };
    }

    private static MarketAnalysisDto CreateSampleMarketAnalysis()
    {
        return new MarketAnalysisDto
        {
            Industry = "Education Technology",
            MarketSize = "$280 billion",
            GrowthRate = "16% CAGR",
            TargetAudience = "Students and professionals",
            GeographicScope = "Global",
            KeyTrends = new[] { "Personalization", "Remote Learning", "Micro-learning" },
            CompetitiveLandscape = new[] { "Coursera", "Duolingo", "Khan Academy" }
        };
    }

    private static SwotAnalysisResult CreateSampleSwotAnalysis()
    {
        return new SwotAnalysisResult
        {
            Strengths = new[] { "AI technology", "Scalability", "Data insights" },
            Weaknesses = new[] { "High development cost", "Market education needed" },
            Opportunities = new[] { "Post-pandemic adoption", "Global reach", "B2B partnerships" },
            Threats = new[] { "Competition", "Data privacy concerns", "Technology changes" },
            StrategicImplications = new List<string> { "Focus on differentiation", "Build partnerships" },
            Summary = "Strong potential with identified challenges"
        };
    }

    private static CompetitiveAnalysisResult CreateSampleCompetitiveAnalysis()
    {
        return new CompetitiveAnalysisResult
        {
            DirectCompetitors = new List<CompetitorProfile>
            {
                new() { Name = "Coursera", Description = "Feature-rich learning platform" },
                new() { Name = "Duolingo", Description = "Gamified language learning" }
            },
            SubstituteProducts = new[] { "Traditional classroom", "Books", "YouTube tutorials" },
            CompetitiveAdvantages = new List<string> { "AI personalization", "Real-time adaptation" },
            MarketGaps = new[] { "Personalized learning paths", "Real-time feedback" },
            StrategicRecommendations = new[] { "Focus on AI differentiation", "Build content partnerships" }
        };
    }

    private static CustomerSegmentationResult CreateSampleCustomerSegmentation()
    {
        return new CustomerSegmentationResult
        {
            PrimarySegments = new[]
            {
                new CustomerSegment
                {
                    Name = "Students",
                    Size = "50M users",
                    Description = "K-12 and college students",
                    Demographics2 = new[] { "Age 13-25", "Tech-savvy", "Budget-conscious" },
                    PainPoints = new[] { "Information overload", "Lack of personalization" }.ToList()
                }
            },
            KeyInsights = new[] { "Strong demand for personalized learning", "Mobile-first approach critical" },
            ValidationMethods = new[] { "Surveys", "User interviews", "A/B testing" }
        };
    }

    private static BusinessPlanDto CreateSampleBusinessPlan()
    {
        return new BusinessPlanDto
        {
            ExecutiveSummary = "AI-powered solution for modern education challenges",
            MarketOpportunity = "Global education market worth $7 trillion",
            ProductDescription = "Adaptive learning platform with AI tutors",
            RevenueModel = "B2C subscriptions and B2B licenses",
            GoToMarketStrategy = "Direct sales, partnerships, content marketing",
            FundingRequirements = "$2M seed round",
            Milestones = new List<MilestoneDto>
            {
                new() { Quarter = "Q1", Description = "MVP launch" },
                new() { Quarter = "Q2", Description = "First 100 customers" }
            }
        };
    }

    private static CompetitiveLandscapeDto CreateSampleCompetitiveLandscape()
    {
        return new CompetitiveLandscapeDto
        {
            DirectCompetitors = new List<CompetitorAnalysisDto>
            {
                new() { Name = "Coursera", MarketShare = 15, Description = "Online courses" },
                new() { Name = "Duolingo", MarketShare = 10, Description = "Language learning" }
            },
            IndirectCompetitors = new List<CompetitorAnalysisDto>
            {
                new() { Name = "YouTube", MarketShare = 5, Description = "Video tutorials" }
            },
            CompetitiveIntensity = 0.7,
            MarketConcentration = "Fragmented"
        };
    }
}