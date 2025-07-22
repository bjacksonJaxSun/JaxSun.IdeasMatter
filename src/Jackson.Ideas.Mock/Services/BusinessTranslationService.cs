using Jackson.Ideas.Mock.Models;

namespace Jackson.Ideas.Mock.Services;

/// <summary>
/// Service for translating technical AI analysis into beginner-friendly plain English
/// </summary>
public class BusinessTranslationService
{
    /// <summary>
    /// Converts technical AI market analysis into plain English explanations
    /// </summary>
    /// <param name="technicalContent">Technical analysis content</param>
    /// <returns>Plain English translation with encouraging tone</returns>
    public string TranslateMarketAnalysis(string technicalContent)
    {
        // Mock translation - in real implementation, this would use AI to translate
        return technicalContent switch
        {
            var content when content.Contains("market penetration") => 
                "Here's who would buy your product: We found thousands of people who have been looking for exactly what you're creating!",
            
            var content when content.Contains("competitive landscape") => 
                "Here are similar businesses and how you can be different: While there are other companies in this space, you have unique advantages that will help you stand out.",
            
            var content when content.Contains("total addressable market") => 
                "Your potential customer pool: There are approximately [X] people who could become your customers, which means plenty of opportunity for growth!",
            
            var content when content.Contains("revenue projections") => 
                "How much money you could make: Based on similar businesses, you could potentially earn between $[X] and $[Y] in your first year if things go well.",
            
            _ => AddEncouragingTone(SimplifyBusinessJargon(technicalContent))
        };
    }

    /// <summary>
    /// Converts technical financial projections into understandable money explanations
    /// </summary>
    public string TranslateFinancialProjections(FinancialProjections data)
    {
        var projections = new List<string>();

        if (data.Revenue.Year1Total > 0)
        {
            projections.Add($"💰 **Your Money Potential**: You could potentially make around ${data.Revenue.Year1Total:N0} in your first year! This is based on similar businesses that started just like yours.");
        }

        var totalStartupCost = data.Expenses.Startup.InitialCapital + data.Expenses.Startup.EquipmentCosts + data.Expenses.Startup.WorkingCapital;
        if (totalStartupCost > 0)
        {
            var encouragement = totalStartupCost < 10000 
                ? "That's totally manageable - many successful businesses started with even less!"
                : "While this seems like a lot, remember you're building something that could change your life!";
                
            projections.Add($"🚀 **What You Need to Get Started**: You'd need about ${totalStartupCost:N0} to launch. {encouragement}");
        }

        if (data.CashFlow.BreakEvenMonth > 0)
        {
            var timeline = data.CashFlow.BreakEvenMonth switch
            {
                <= 6 => "really quickly",
                <= 12 => "within your first year", 
                <= 18 => "in a reasonable timeframe",
                _ => "once you build momentum"
            };
            
            projections.Add($"⚡ **When You'll Start Making Profit**: You could break even around month {data.CashFlow.BreakEvenMonth:0}, which means you'd start making money {timeline}!");
        }

        return string.Join("\n\n", projections);
    }

    /// <summary>
    /// Creates confidence-building summaries of business potential
    /// </summary>
    public string GenerateConfidenceBuilding(BusinessIdeaScenario scenario)
    {
        var strengths = new List<string>();

        // Market opportunity
        if (scenario.MarketSize > 100000)
        {
            strengths.Add("🎯 **Huge Market Opportunity**: There are hundreds of thousands of potential customers for your idea!");
        }

        // Competition level
        if (scenario.CompetitionLevel == "Low")
        {
            strengths.Add("🚀 **Low Competition**: You're entering a space where there's room to grow without fighting for every customer!");
        }
        else if (scenario.CompetitionLevel == "Medium")
        {
            strengths.Add("💪 **Healthy Competition**: The competition proves there's real demand - and you have unique advantages!");
        }

        // Add general encouragement
        strengths.Add("✨ **You've Got This**: Thousands of people just like you have turned similar ideas into successful businesses. There's no reason you can't be next!");

        return string.Join("\n\n", strengths);
    }

    /// <summary>
    /// Converts business model complexity into simple "how you'll make money" explanations
    /// </summary>
    public string ExplainBusinessModel(string businessType)
    {
        return businessType.ToLower() switch
        {
            "saas" or "software" => 
                "💻 **How You'll Make Money**: People will pay you a monthly subscription to use your software. It's like Netflix, but for your amazing product!",
            
            "ecommerce" or "retail" => 
                "🛍️ **How You'll Make Money**: You'll sell products directly to customers online. Every sale puts money straight in your pocket!",
            
            "service" or "consulting" => 
                "🤝 **How You'll Make Money**: You'll get paid for your expertise and time helping people solve their problems. Your knowledge is valuable!",
            
            "marketplace" => 
                "🏪 **How You'll Make Money**: You'll connect buyers and sellers, taking a small percentage of each transaction. More activity means more income!",
            
            "freemium" => 
                "🎁 **How You'll Make Money**: You'll offer a free version to attract users, then some will upgrade to premium features that they pay for monthly!",
            
            _ => 
                "💰 **How You'll Make Money**: You've got multiple ways to earn from your idea - we'll help you pick the best approach for your situation!"
        };
    }

    /// <summary>
    /// Creates actionable next steps from complex business strategy
    /// </summary>
    public List<ActionItem> CreateActionableSteps(BusinessIdeaScenario scenario)
    {
        var steps = new List<ActionItem>
        {
            new ActionItem
            {
                Title = "Validate Your Idea with Real People",
                Description = "Talk to 10-15 people who might use your product. Ask them about their problems and see if your solution excites them.",
                TimeEstimate = "1-2 weeks",
                Difficulty = "Easy",
                Priority = "High",
                Category = "Research"
            },
            
            new ActionItem
            {
                Title = "Create a Simple Version of Your Product",
                Description = "Build the most basic version that solves the main problem. Don't worry about making it perfect - just make it work!",
                TimeEstimate = "2-4 weeks",
                Difficulty = "Medium",
                Priority = "High",
                Category = "Development"
            },
            
            new ActionItem
            {
                Title = "Find Your First 10 Customers",
                Description = "Focus on getting 10 people to actually pay for and use your product. These early customers will teach you everything.",
                TimeEstimate = "2-6 weeks",
                Difficulty = "Medium",
                Priority = "High",
                Category = "Sales"
            }
        };

        // Add specific steps based on scenario
        if (scenario.StartupCost > 5000)
        {
            steps.Insert(1, new ActionItem
            {
                Title = "Figure Out Your Funding",
                Description = $"You'll need about ${scenario.StartupCost:N0} to get started. Look into small business loans, investors, or saving up gradually.",
                TimeEstimate = "2-8 weeks",
                Difficulty = "Medium",
                Priority = "High",
                Category = "Funding"
            });
        }

        if (scenario.CompetitionLevel == "High")
        {
            steps.Insert(0, new ActionItem
            {
                Title = "Study Your Competition",
                Description = "Check out what similar businesses are doing. Find what customers complain about - that's your opportunity to be better!",
                TimeEstimate = "1 week",
                Difficulty = "Easy",
                Priority = "High",
                Category = "Research"
            });
        }

        return steps;
    }

    /// <summary>
    /// Adds encouraging, confidence-building language to any content
    /// </summary>
    private string AddEncouragingTone(string content)
    {
        // Add encouraging phrases and positive framing
        var encouragingPhrases = new[]
        {
            "This is exciting because",
            "The great news is",
            "You're going to love this",
            "Here's what's promising",
            "This is actually fantastic news"
        };

        var random = new Random();
        var phrase = encouragingPhrases[random.Next(encouragingPhrases.Length)];
        
        return $"{phrase}: {content}";
    }

    /// <summary>
    /// Replaces business jargon with plain English terms
    /// </summary>
    private string SimplifyBusinessJargon(string content)
    {
        var replacements = new Dictionary<string, string>
        {
            { "market penetration", "getting customers" },
            { "customer acquisition", "finding customers" },
            { "value proposition", "what makes you special" },
            { "go-to-market strategy", "plan to get customers" },
            { "competitive advantage", "what makes you better" },
            { "total addressable market", "potential customers" },
            { "customer lifetime value", "how much each customer is worth" },
            { "burn rate", "how much you spend monthly" },
            { "runway", "how long your money will last" },
            { "pivot", "change direction" },
            { "iterate", "improve step by step" },
            { "monetization", "making money" },
            { "scalability", "ability to grow" },
            { "KPIs", "important measurements" },
            { "ROI", "return on investment" }
        };

        foreach (var replacement in replacements)
        {
            content = content.Replace(replacement.Key, replacement.Value);
        }

        return content;
    }
}

/// <summary>
/// Represents an actionable step for business beginners
/// </summary>
public class ActionItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string TimeEstimate { get; set; } = "";
    public string Difficulty { get; set; } = ""; // Easy, Medium, Hard
    public string Priority { get; set; } = ""; // High, Medium, Low
    public string Category { get; set; } = ""; // Research, Development, Marketing, etc.
}