using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Jackson.Ideas.Core.DTOs.BusinessPlan;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Jackson.Ideas.Application.Services;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateResearchReportAsync(
        ResearchReportDto reportData,
        PdfGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= new PdfGenerationOptions();
            
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize(options.PaperSize));

            // Set document metadata
            SetDocumentMetadata(pdf, $"{reportData.Title} - Research Report", "Ideas Matter");

            // Configure fonts
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title page
            await AddTitlePageAsync(document, reportData, boldFont, regularFont, options);

            // Table of contents
            AddTableOfContents(document, reportData, boldFont, regularFont);

            // Executive summary
            if (!string.IsNullOrEmpty(reportData.Description))
            {
                AddExecutiveSummary(document, reportData, boldFont, regularFont);
            }

            // Market analysis section
            if (reportData.MarketAnalysis != null)
            {
                await AddMarketAnalysisSectionAsync(document, reportData.MarketAnalysis, boldFont, regularFont, options);
            }

            // SWOT analysis section
            if (reportData.SwotAnalysis != null)
            {
                await AddSwotAnalysisSectionAsync(document, reportData.SwotAnalysis, boldFont, regularFont, options);
            }

            // Competitive analysis section
            if (reportData.CompetitiveAnalysis != null)
            {
                await AddCompetitiveAnalysisSectionAsync(document, reportData.CompetitiveAnalysis, boldFont, regularFont);
            }

            // Customer segmentation section
            if (reportData.CustomerSegmentation != null)
            {
                await AddCustomerSegmentationSectionAsync(document, reportData.CustomerSegmentation, boldFont, regularFont);
            }

            // Business plan section
            if (reportData.BusinessPlan != null)
            {
                await AddBusinessPlanSectionAsync(document, reportData.BusinessPlan, boldFont, regularFont, options);
            }

            // Footer
            if (options.IncludeMetadata)
            {
                AddDocumentFooter(document, regularFont, options);
            }

            document.Close();

            var result = stream.ToArray();
            _logger.LogInformation("Generated research report PDF with {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate research report PDF");
            throw;
        }
    }

    public async Task<byte[]> GenerateSwotAnalysisPdfAsync(
        SwotAnalysisResult swotAnalysis,
        string ideaTitle,
        string ideaDescription,
        PdfGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= new PdfGenerationOptions();
            
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize(options.PaperSize));

            SetDocumentMetadata(pdf, $"{ideaTitle} - SWOT Analysis", "Ideas Matter");

            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title
            document.Add(new Paragraph("SWOT Analysis Report")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Idea details
            document.Add(new Paragraph($"Idea: {ideaTitle}")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginBottom(10));

            if (!string.IsNullOrEmpty(ideaDescription))
            {
                document.Add(new Paragraph(ideaDescription)
                    .SetFont(regularFont)
                    .SetFontSize(12)
                    .SetMarginBottom(20));
            }

            // SWOT Matrix
            await AddSwotMatrixAsync(document, swotAnalysis, boldFont, regularFont);

            // Detailed sections
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
            document.Add(new Paragraph("Detailed Analysis")
                .SetFont(boldFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            AddDetailedSwotSection(document, "Strengths", GetStrengthsList(swotAnalysis), ColorConstants.GREEN, boldFont, regularFont);
            AddDetailedSwotSection(document, "Weaknesses", GetWeaknessesList(swotAnalysis), ColorConstants.RED, boldFont, regularFont);
            AddDetailedSwotSection(document, "Opportunities", GetOpportunitiesList(swotAnalysis), ColorConstants.BLUE, boldFont, regularFont);
            AddDetailedSwotSection(document, "Threats", GetThreatsList(swotAnalysis), ColorConstants.ORANGE, boldFont, regularFont);

            // Strategic implications
            if (swotAnalysis.StrategicImplications.Any())
            {
                document.Add(new Paragraph("Strategic Implications")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetMarginTop(20)
                    .SetMarginBottom(10));

                foreach (var implication in swotAnalysis.StrategicImplications)
                {
                    document.Add(new Paragraph($"• {implication}")
                        .SetFont(regularFont)
                        .SetFontSize(12)
                        .SetMarginLeft(20)
                        .SetMarginBottom(5));
                }
            }

            if (options.IncludeMetadata)
            {
                AddDocumentFooter(document, regularFont, options);
            }

            document.Close();

            var result = stream.ToArray();
            _logger.LogInformation("Generated SWOT analysis PDF with {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SWOT analysis PDF");
            throw;
        }
    }

    public async Task<byte[]> GenerateMarketAnalysisPdfAsync(
        MarketAnalysisDto marketAnalysis,
        CompetitiveLandscapeDto? competitiveLandscape = null,
        PdfGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= new PdfGenerationOptions();
            
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize(options.PaperSize));

            SetDocumentMetadata(pdf, "Market Analysis Report", "Ideas Matter");

            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title
            document.Add(new Paragraph("Market Analysis Report")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(30));

            // Market overview
            AddMarketOverviewSection(document, marketAnalysis, boldFont, regularFont);

            // Market sizing
            AddMarketSizingSection(document, marketAnalysis, boldFont, regularFont);

            // Competitive landscape
            if (competitiveLandscape != null)
            {
                AddCompetitiveLandscapeSection(document, competitiveLandscape, boldFont, regularFont);
            }

            // Key trends
            if (marketAnalysis.KeyTrends?.Any() == true)
            {
                AddKeyTrendsSection(document, marketAnalysis.KeyTrends.ToArray(), boldFont, regularFont);
            }

            if (options.IncludeMetadata)
            {
                AddDocumentFooter(document, regularFont, options);
            }

            document.Close();

            var result = stream.ToArray();
            _logger.LogInformation("Generated market analysis PDF with {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate market analysis PDF");
            throw;
        }
    }

    public async Task<byte[]> GenerateBusinessPlanPdfAsync(
        BusinessPlanDto businessPlan,
        PdfGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= new PdfGenerationOptions();
            
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize(options.PaperSize));

            SetDocumentMetadata(pdf, "Business Plan", "Ideas Matter");

            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title page
            document.Add(new Paragraph("Business Plan")
                .SetFont(boldFont)
                .SetFontSize(28)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(50));

            // Executive summary
            AddBusinessPlanSection(document, "Executive Summary", businessPlan.ExecutiveSummary, boldFont, regularFont);

            // Market opportunity
            AddBusinessPlanSection(document, "Market Opportunity", businessPlan.MarketOpportunity, boldFont, regularFont);

            // Product description
            AddBusinessPlanSection(document, "Product Description", businessPlan.ProductDescription, boldFont, regularFont);

            // Revenue model
            AddBusinessPlanSection(document, "Revenue Model", businessPlan.RevenueModel, boldFont, regularFont);

            // Financial projections
            if (businessPlan.FinancialProjections != null)
            {
                await AddFinancialProjectionsSectionAsync(document, businessPlan.FinancialProjections, boldFont, regularFont, options);
            }

            // Go-to-market strategy
            AddBusinessPlanSection(document, "Go-to-Market Strategy", businessPlan.GoToMarketStrategy, boldFont, regularFont);

            // Team structure
            if (businessPlan.Team != null)
            {
                AddTeamStructureSection(document, businessPlan.Team, boldFont, regularFont);
            }

            // Funding requirements
            AddBusinessPlanSection(document, "Funding Requirements", businessPlan.FundingRequirements, boldFont, regularFont);

            // Milestones
            if (businessPlan.Milestones.Any())
            {
                AddMilestonesSection(document, businessPlan.Milestones, boldFont, regularFont);
            }

            if (options.IncludeMetadata)
            {
                AddDocumentFooter(document, regularFont, options);
            }

            document.Close();

            var result = stream.ToArray();
            _logger.LogInformation("Generated business plan PDF with {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate business plan PDF");
            throw;
        }
    }

    public async Task<byte[]> GenerateCompetitiveAnalysisPdfAsync(
        CompetitiveAnalysisResult competitiveAnalysis,
        string ideaTitle,
        string ideaDescription,
        PdfGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= new PdfGenerationOptions();
            
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize(options.PaperSize));

            SetDocumentMetadata(pdf, $"{ideaTitle} - Competitive Analysis", "Ideas Matter");

            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title
            document.Add(new Paragraph("Competitive Analysis Report")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Idea details
            document.Add(new Paragraph($"Idea: {ideaTitle}")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginBottom(10));

            if (!string.IsNullOrEmpty(ideaDescription))
            {
                document.Add(new Paragraph(ideaDescription)
                    .SetFont(regularFont)
                    .SetFontSize(12)
                    .SetMarginBottom(20));
            }

            // Direct competitors
            if (competitiveAnalysis.DirectCompetitors.Any())
            {
                AddCompetitorSection(document, "Direct Competitors", 
                    competitiveAnalysis.DirectCompetitors.Select(c => c.Name).ToList(), 
                    boldFont, regularFont);
            }

            // Indirect competitors
            if (competitiveAnalysis.IndirectCompetitors.Any())
            {
                AddCompetitorSection(document, "Indirect Competitors", 
                    competitiveAnalysis.IndirectCompetitors.Select(c => c.Name).ToList(), 
                    boldFont, regularFont);
            }

            // Substitute products
            if (competitiveAnalysis.SubstituteProducts.Any())
            {
                AddListSection(document, "Substitute Products", competitiveAnalysis.SubstituteProducts.ToList(), boldFont, regularFont);
            }

            // Competitive advantages
            if (competitiveAnalysis.CompetitiveAdvantages.Any())
            {
                AddListSection(document, "Competitive Advantages", competitiveAnalysis.CompetitiveAdvantages, boldFont, regularFont);
            }

            // Market gaps
            if (competitiveAnalysis.MarketGaps.Any())
            {
                AddListSection(document, "Market Gaps", competitiveAnalysis.MarketGaps.ToList(), boldFont, regularFont);
            }

            // Strategic recommendations
            if (competitiveAnalysis.StrategicRecommendations.Any())
            {
                AddListSection(document, "Strategic Recommendations", competitiveAnalysis.StrategicRecommendations.ToList(), boldFont, regularFont);
            }

            if (options.IncludeMetadata)
            {
                AddDocumentFooter(document, regularFont, options);
            }

            document.Close();

            var result = stream.ToArray();
            _logger.LogInformation("Generated competitive analysis PDF with {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate competitive analysis PDF");
            throw;
        }
    }

    public async Task<byte[]> GenerateCustomerSegmentationPdfAsync(
        CustomerSegmentationResult segmentation,
        string ideaTitle,
        string ideaDescription,
        PdfGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            options ??= new PdfGenerationOptions();
            
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, GetPageSize(options.PaperSize));

            SetDocumentMetadata(pdf, $"{ideaTitle} - Customer Segmentation", "Ideas Matter");

            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Title
            document.Add(new Paragraph("Customer Segmentation Analysis")
                .SetFont(boldFont)
                .SetFontSize(24)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Idea details
            document.Add(new Paragraph($"Idea: {ideaTitle}")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetMarginBottom(10));

            if (!string.IsNullOrEmpty(ideaDescription))
            {
                document.Add(new Paragraph(ideaDescription)
                    .SetFont(regularFont)
                    .SetFontSize(12)
                    .SetMarginBottom(20));
            }

            // Primary segments
            if (segmentation.PrimarySegments.Any())
            {
                AddCustomerSegmentsSection(document, "Primary Target Segments", segmentation.PrimarySegments, boldFont, regularFont);
            }

            // Secondary segments
            if (segmentation.SecondarySegments.Any())
            {
                AddCustomerSegmentsSection(document, "Secondary Target Segments", segmentation.SecondarySegments, boldFont, regularFont);
            }

            // Key insights
            if (segmentation.KeyInsights.Any())
            {
                AddListSection(document, "Key Insights", segmentation.KeyInsights.ToList(), boldFont, regularFont);
            }

            // Validation methods
            if (segmentation.ValidationMethods.Any())
            {
                AddListSection(document, "Validation Methods", segmentation.ValidationMethods.ToList(), boldFont, regularFont);
            }

            if (options.IncludeMetadata)
            {
                AddDocumentFooter(document, regularFont, options);
            }

            document.Close();

            var result = stream.ToArray();
            _logger.LogInformation("Generated customer segmentation PDF with {Size} bytes", result.Length);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate customer segmentation PDF");
            throw;
        }
    }

    // Helper methods
    private static PageSize GetPageSize(PaperSize paperSize)
    {
        return paperSize switch
        {
            PaperSize.A4 => PageSize.A4,
            PaperSize.Legal => PageSize.LEGAL,
            _ => PageSize.LETTER
        };
    }

    private static void SetDocumentMetadata(PdfDocument pdf, string title, string author)
    {
        var info = pdf.GetDocumentInfo();
        info.SetTitle(title);
        info.SetAuthor(author);
        info.SetCreator("Ideas Matter Platform");
        info.SetProducer("Ideas Matter PDF Service");
        // Creation date is set automatically by iText7
    }

    private async Task AddTitlePageAsync(Document document, ResearchReportDto reportData, 
        PdfFont boldFont, PdfFont regularFont, PdfGenerationOptions options)
    {
        document.Add(new Paragraph(reportData.Title)
            .SetFont(boldFont)
            .SetFontSize(28)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(30));

        document.Add(new Paragraph("Research Report")
            .SetFont(boldFont)
            .SetFontSize(20)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(50));

        if (!string.IsNullOrEmpty(reportData.Description))
        {
            document.Add(new Paragraph(reportData.Description)
                .SetFont(regularFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(30));
        }

        document.Add(new Paragraph($"Generated on: {DateTime.UtcNow:MMMM dd, yyyy}")
            .SetFont(regularFont)
            .SetFontSize(12)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(100));

        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
    }

    private void AddTableOfContents(Document document, ResearchReportDto reportData, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Table of Contents")
            .SetFont(boldFont)
            .SetFontSize(20)
            .SetMarginBottom(20));

        var tocItems = new List<string> { "Executive Summary" };
        
        if (reportData.MarketAnalysis != null) tocItems.Add("Market Analysis");
        if (reportData.SwotAnalysis != null) tocItems.Add("SWOT Analysis");
        if (reportData.CompetitiveAnalysis != null) tocItems.Add("Competitive Analysis");
        if (reportData.CustomerSegmentation != null) tocItems.Add("Customer Segmentation");
        if (reportData.BusinessPlan != null) tocItems.Add("Business Plan");

        foreach (var item in tocItems)
        {
            document.Add(new Paragraph($"• {item}")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginLeft(20)
                .SetMarginBottom(5));
        }

        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
    }

    private void AddExecutiveSummary(Document document, ResearchReportDto reportData, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Executive Summary")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15));

        document.Add(new Paragraph(reportData.Description)
            .SetFont(regularFont)
            .SetFontSize(12)
            .SetMarginBottom(20));

        document.Add(new LineSeparator(new SolidLine())
            .SetMarginBottom(20));
    }

    private async Task AddMarketAnalysisSectionAsync(Document document, MarketAnalysisDto marketAnalysis, 
        PdfFont boldFont, PdfFont regularFont, PdfGenerationOptions options)
    {
        document.Add(new Paragraph("Market Analysis")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15));

        AddMarketOverviewSection(document, marketAnalysis, boldFont, regularFont);
        AddMarketSizingSection(document, marketAnalysis, boldFont, regularFont);

        if (marketAnalysis.KeyTrends?.Any() == true)
        {
            AddKeyTrendsSection(document, marketAnalysis.KeyTrends.ToArray(), boldFont, regularFont);
        }

        document.Add(new LineSeparator(new SolidLine())
            .SetMarginBottom(20));
    }

    private async Task AddSwotAnalysisSectionAsync(Document document, SwotAnalysisResult swotAnalysis, 
        PdfFont boldFont, PdfFont regularFont, PdfGenerationOptions options)
    {
        document.Add(new Paragraph("SWOT Analysis")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15));

        await AddSwotMatrixAsync(document, swotAnalysis, boldFont, regularFont);

        document.Add(new LineSeparator(new SolidLine())
            .SetMarginBottom(20));
    }

    private async Task AddCompetitiveAnalysisSectionAsync(Document document, CompetitiveAnalysisResult competitiveAnalysis, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Competitive Analysis")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15));

        if (competitiveAnalysis.DirectCompetitors.Any())
        {
            AddCompetitorSection(document, "Direct Competitors", 
                competitiveAnalysis.DirectCompetitors.Select(c => c.Name).ToList(), 
                boldFont, regularFont);
        }

        if (competitiveAnalysis.CompetitiveAdvantages.Any())
        {
            AddListSection(document, "Competitive Advantages", competitiveAnalysis.CompetitiveAdvantages, boldFont, regularFont);
        }

        document.Add(new LineSeparator(new SolidLine())
            .SetMarginBottom(20));
    }

    private async Task AddCustomerSegmentationSectionAsync(Document document, CustomerSegmentationResult segmentation, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Customer Segmentation")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15));

        if (segmentation.PrimarySegments.Any())
        {
            AddCustomerSegmentsSection(document, "Primary Segments", segmentation.PrimarySegments, boldFont, regularFont);
        }

        if (segmentation.KeyInsights.Any())
        {
            AddListSection(document, "Key Insights", segmentation.KeyInsights.ToList(), boldFont, regularFont);
        }

        document.Add(new LineSeparator(new SolidLine())
            .SetMarginBottom(20));
    }

    private async Task AddBusinessPlanSectionAsync(Document document, BusinessPlanDto businessPlan, 
        PdfFont boldFont, PdfFont regularFont, PdfGenerationOptions options)
    {
        document.Add(new Paragraph("Business Plan")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetMarginBottom(15));

        AddBusinessPlanSection(document, "Executive Summary", businessPlan.ExecutiveSummary, boldFont, regularFont);
        AddBusinessPlanSection(document, "Revenue Model", businessPlan.RevenueModel, boldFont, regularFont);

        if (businessPlan.FinancialProjections != null)
        {
            await AddFinancialProjectionsSectionAsync(document, businessPlan.FinancialProjections, boldFont, regularFont, options);
        }

        document.Add(new LineSeparator(new SolidLine())
            .SetMarginBottom(20));
    }

    private async Task AddSwotMatrixAsync(Document document, SwotAnalysisResult swotAnalysis, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("SWOT Matrix")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        var table = new Table(2).UseAllAvailableWidth();

        // Headers
        table.AddHeaderCell(new Cell().Add(new Paragraph("Strengths").SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(ColorConstants.GREEN)
            .SetTextAlignment(TextAlignment.CENTER));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Weaknesses").SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(ColorConstants.RED)
            .SetTextAlignment(TextAlignment.CENTER));

        // Content
        table.AddCell(new Cell().Add(CreateBulletList(GetStrengthsList(swotAnalysis), regularFont))
            .SetBackgroundColor(new DeviceRgb(240, 253, 244)));
        table.AddCell(new Cell().Add(CreateBulletList(GetWeaknessesList(swotAnalysis), regularFont))
            .SetBackgroundColor(new DeviceRgb(254, 242, 242)));

        // Second row headers
        table.AddCell(new Cell().Add(new Paragraph("Opportunities").SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(ColorConstants.BLUE)
            .SetTextAlignment(TextAlignment.CENTER));
        table.AddCell(new Cell().Add(new Paragraph("Threats").SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(ColorConstants.ORANGE)
            .SetTextAlignment(TextAlignment.CENTER));

        // Second row content
        table.AddCell(new Cell().Add(CreateBulletList(GetOpportunitiesList(swotAnalysis), regularFont))
            .SetBackgroundColor(new DeviceRgb(239, 246, 255)));
        table.AddCell(new Cell().Add(CreateBulletList(GetThreatsList(swotAnalysis), regularFont))
            .SetBackgroundColor(new DeviceRgb(254, 252, 232)));

        document.Add(table);
        document.Add(new Paragraph("\n"));
    }

    private Div CreateBulletList(List<string> items, PdfFont font)
    {
        var div = new Div();
        if (!items.Any())
        {
            div.Add(new Paragraph("No items identified").SetFont(font).SetFontSize(10));
            return div;
        }

        foreach (var item in items)
        {
            div.Add(new Paragraph($"• {item}")
                .SetFont(font)
                .SetFontSize(10)
                .SetMarginBottom(3));
        }
        return div;
    }

    private void AddDetailedSwotSection(Document document, string title, List<string> items, 
        Color color, PdfFont boldFont, PdfFont regularFont)
    {
        var headerTable = new Table(1).UseAllAvailableWidth();
        headerTable.AddCell(new Cell().Add(new Paragraph(title).SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
            .SetBackgroundColor(color)
            .SetPadding(10));

        document.Add(headerTable);

        if (items.Any())
        {
            for (int i = 0; i < items.Count; i++)
            {
                document.Add(new Paragraph($"{i + 1}. {items[i]}")
                    .SetFont(regularFont)
                    .SetFontSize(12)
                    .SetMarginLeft(10)
                    .SetMarginBottom(5));
            }
        }
        else
        {
            document.Add(new Paragraph("No items identified in this category")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginLeft(10)
                .SetMarginBottom(10));
        }

        document.Add(new Paragraph("\n"));
    }

    private void AddMarketOverviewSection(Document document, MarketAnalysisDto marketAnalysis, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Market Overview")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        var table = new Table(2).UseAllAvailableWidth();
        
        if (!string.IsNullOrEmpty(marketAnalysis.Industry))
        {
            table.AddCell(new Cell().Add(new Paragraph("Industry").SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(marketAnalysis.Industry).SetFont(regularFont)));
        }

        if (!string.IsNullOrEmpty(marketAnalysis.GeographicScope))
        {
            table.AddCell(new Cell().Add(new Paragraph("Geographic Scope").SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(marketAnalysis.GeographicScope).SetFont(regularFont)));
        }

        if (!string.IsNullOrEmpty(marketAnalysis.TargetAudience))
        {
            table.AddCell(new Cell().Add(new Paragraph("Target Audience").SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(marketAnalysis.TargetAudience).SetFont(regularFont)));
        }

        document.Add(table);
        document.Add(new Paragraph("\n"));
    }

    private void AddMarketSizingSection(Document document, MarketAnalysisDto marketAnalysis, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Market Sizing")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        var table = new Table(2).UseAllAvailableWidth();

        if (!string.IsNullOrEmpty(marketAnalysis.MarketSize))
        {
            table.AddCell(new Cell().Add(new Paragraph("Market Size").SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(marketAnalysis.MarketSize).SetFont(regularFont)));
        }

        if (!string.IsNullOrEmpty(marketAnalysis.GrowthRate))
        {
            table.AddCell(new Cell().Add(new Paragraph("Growth Rate").SetFont(boldFont)));
            table.AddCell(new Cell().Add(new Paragraph(marketAnalysis.GrowthRate).SetFont(regularFont)));
        }

        document.Add(table);
        document.Add(new Paragraph("\n"));
    }

    private void AddCompetitiveLandscapeSection(Document document, CompetitiveLandscapeDto landscape, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Competitive Landscape")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        if (landscape.DirectCompetitors.Any())
        {
            AddCompetitorSection(document, "Direct Competitors", 
                landscape.DirectCompetitors.Select(c => c.Name).ToList(), 
                boldFont, regularFont);
        }

        if (landscape.IndirectCompetitors.Any())
        {
            AddCompetitorSection(document, "Indirect Competitors", 
                landscape.IndirectCompetitors.Select(c => c.Name).ToList(), 
                boldFont, regularFont);
        }
    }

    private void AddKeyTrendsSection(Document document, string[] trends, PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Key Market Trends")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        foreach (var trend in trends)
        {
            document.Add(new Paragraph($"• {trend}")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginLeft(20)
                .SetMarginBottom(5));
        }

        document.Add(new Paragraph("\n"));
    }

    private void AddBusinessPlanSection(Document document, string title, string content, 
        PdfFont boldFont, PdfFont regularFont)
    {
        if (string.IsNullOrEmpty(content)) return;

        document.Add(new Paragraph(title)
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        document.Add(new Paragraph(content)
            .SetFont(regularFont)
            .SetFontSize(12)
            .SetMarginBottom(15));
    }

    private async Task AddFinancialProjectionsSectionAsync(Document document, FinancialProjectionsDto projections, 
        PdfFont boldFont, PdfFont regularFont, PdfGenerationOptions options)
    {
        document.Add(new Paragraph("Financial Projections")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        if (projections.YearlyProjections.Any())
        {
            var table = new Table(2).UseAllAvailableWidth();
            table.AddHeaderCell(new Cell().Add(new Paragraph("Year").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Projection").SetFont(boldFont)));

            foreach (var projection in projections.YearlyProjections)
            {
                table.AddCell(new Cell().Add(new Paragraph(projection.Key).SetFont(regularFont)));
                table.AddCell(new Cell().Add(new Paragraph(projection.Value).SetFont(regularFont)));
            }

            document.Add(table);
        }

        document.Add(new Paragraph("\n"));
    }

    private void AddTeamStructureSection(Document document, TeamStructureDto team, PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Team Structure")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        if (team.Founders.Any())
        {
            AddListSection(document, "Founders", team.Founders, boldFont, regularFont);
        }

        if (team.KeyHires.Any())
        {
            AddListSection(document, "Key Hires", team.KeyHires, boldFont, regularFont);
        }
    }

    private void AddMilestonesSection(Document document, List<MilestoneDto> milestones, PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph("Key Milestones")
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        var table = new Table(2).UseAllAvailableWidth();
        table.AddHeaderCell(new Cell().Add(new Paragraph("Timeline").SetFont(boldFont)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Milestone").SetFont(boldFont)));

        foreach (var milestone in milestones)
        {
            table.AddCell(new Cell().Add(new Paragraph(milestone.Quarter).SetFont(regularFont)));
            table.AddCell(new Cell().Add(new Paragraph(milestone.Description).SetFont(regularFont)));
        }

        document.Add(table);
        document.Add(new Paragraph("\n"));
    }

    private void AddCompetitorSection(Document document, string title, List<string> competitors, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph(title)
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        foreach (var competitor in competitors)
        {
            document.Add(new Paragraph($"• {competitor}")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginLeft(20)
                .SetMarginBottom(5));
        }

        document.Add(new Paragraph("\n"));
    }

    private void AddListSection(Document document, string title, List<string> items, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph(title)
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        foreach (var item in items)
        {
            document.Add(new Paragraph($"• {item}")
                .SetFont(regularFont)
                .SetFontSize(12)
                .SetMarginLeft(20)
                .SetMarginBottom(5));
        }

        document.Add(new Paragraph("\n"));
    }

    private void AddCustomerSegmentsSection(Document document, string title, CustomerSegment[] segments, 
        PdfFont boldFont, PdfFont regularFont)
    {
        document.Add(new Paragraph(title)
            .SetFont(boldFont)
            .SetFontSize(14)
            .SetMarginBottom(10));

        foreach (var segment in segments)
        {
            document.Add(new Paragraph(segment.Name)
                .SetFont(boldFont)
                .SetFontSize(12)
                .SetMarginBottom(5));

            if (!string.IsNullOrEmpty(segment.Description))
            {
                document.Add(new Paragraph(segment.Description)
                    .SetFont(regularFont)
                    .SetFontSize(11)
                    .SetMarginLeft(20)
                    .SetMarginBottom(5));
            }

            if (!string.IsNullOrEmpty(segment.Size))
            {
                document.Add(new Paragraph($"Size: {segment.Size}")
                    .SetFont(regularFont)
                    .SetFontSize(11)
                    .SetMarginLeft(20)
                    .SetMarginBottom(10));
            }
        }

        document.Add(new Paragraph("\n"));
    }

    private void AddDocumentFooter(Document document, PdfFont regularFont, PdfGenerationOptions options)
    {
        document.Add(new Paragraph("\n\n"));
        document.Add(new LineSeparator(new SolidLine()));
        document.Add(new Paragraph($"Generated on: {DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm 'UTC'}")
            .SetFont(regularFont)
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginTop(10));

        if (!string.IsNullOrEmpty(options.FooterText))
        {
            document.Add(new Paragraph(options.FooterText)
                .SetFont(regularFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));
        }
    }

    private List<string> GetStrengthsList(SwotAnalysisResult swot)
    {
        if (swot.Strengths is string[] strArray) return strArray.ToList();
        if (swot.Strengths is List<string> strList) return strList;
        return swot.StrengthsFactors.Select(f => f.Title).ToList();
    }

    private List<string> GetWeaknessesList(SwotAnalysisResult swot)
    {
        if (swot.Weaknesses is string[] strArray) return strArray.ToList();
        if (swot.Weaknesses is List<string> strList) return strList;
        return swot.WeaknessesFactors.Select(f => f.Title).ToList();
    }

    private List<string> GetOpportunitiesList(SwotAnalysisResult swot)
    {
        if (swot.Opportunities is string[] strArray) return strArray.ToList();
        if (swot.Opportunities is List<string> strList) return strList;
        return swot.OpportunitiesFactors.Select(f => f.Title).ToList();
    }

    private List<string> GetThreatsList(SwotAnalysisResult swot)
    {
        if (swot.Threats is string[] strArray) return strArray.ToList();
        if (swot.Threats is List<string> strList) return strList;
        return swot.ThreatsFactors.Select(f => f.Title).ToList();
    }
}