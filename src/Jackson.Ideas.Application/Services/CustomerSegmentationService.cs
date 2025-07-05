using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.AI;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Jackson.Ideas.Application.Services;

public class CustomerSegmentationService : ICustomerSegmentationService
{
    private readonly IAIOrchestrator _aiOrchestrator;
    private readonly ILogger<CustomerSegmentationService> _logger;

    public CustomerSegmentationService(
        IAIOrchestrator aiOrchestrator,
        ILogger<CustomerSegmentationService> logger)
    {
        _aiOrchestrator = aiOrchestrator;
        _logger = logger;
    }

    public async Task<CustomerSegmentationResult> AnalyzeCustomerSegmentsAsync(
        string ideaDescription,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Analyzing customer segments for idea: {IdeaDescription}", ideaDescription);

        try
        {
            var prompt = BuildCustomerSegmentationPrompt(ideaDescription);
            var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

            var segmentation = ParseCustomerSegmentationResponse(response);
            
            _logger.LogInformation("Customer segmentation analysis completed successfully");
            return segmentation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during customer segmentation analysis");
            throw;
        }
    }

    public async Task<CustomerPersona> CreateCustomerPersonaAsync(
        string segmentName,
        string ideaDescription,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Creating customer persona for segment: {SegmentName}", segmentName);

        try
        {
            var prompt = BuildPersonaCreationPrompt(segmentName, ideaDescription);
            var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

            var persona = ParseCustomerPersonaResponse(response, segmentName);
            
            _logger.LogInformation("Customer persona created for segment: {SegmentName}", segmentName);
            return persona;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer persona for segment: {SegmentName}", segmentName);
            throw;
        }
    }

    public async Task<List<CustomerJourney>> AnalyzeCustomerJourneysAsync(
        List<CustomerSegment> segments,
        string ideaDescription,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Analyzing customer journeys for {SegmentCount} segments", segments.Count);

        var journeys = new List<CustomerJourney>();

        try
        {
            foreach (var segment in segments)
            {
                var prompt = BuildCustomerJourneyPrompt(segment, ideaDescription);
                var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

                var journey = ParseCustomerJourneyResponse(response, segment.Name);
                journeys.Add(journey);
            }

            _logger.LogInformation("Customer journey analysis completed for all segments");
            return journeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing customer journeys");
            throw;
        }
    }

    public async Task<CustomerInsightResult> GenerateCustomerInsightsAsync(
        CustomerSegmentationResult segmentation,
        Dictionary<string, object>? parameters = null)
    {
        _logger.LogInformation("Generating customer insights from segmentation analysis");

        try
        {
            var prompt = BuildCustomerInsightsPrompt(segmentation);
            var response = await _aiOrchestrator.ProcessRequestAsync(prompt, parameters);

            var insights = ParseCustomerInsightsResponse(response);
            
            _logger.LogInformation("Customer insights generation completed");
            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer insights");
            throw;
        }
    }

    private string BuildCustomerSegmentationPrompt(string ideaDescription)
    {
        return $@"
Conduct comprehensive customer segmentation analysis for the following business idea:

Business Idea: {ideaDescription}

Provide detailed analysis in JSON format:
{{
  ""primaryTargetSegment"": ""Most attractive primary segment"",
  ""customerSegments"": [
    {{
      ""name"": ""Segment Name"",
      ""description"": ""Detailed segment description"",
      ""sizeEstimate"": 500000,
      ""demographics"": {{
        ""ageRange"": ""25-45"",
        ""income"": ""$50K-$100K"",
        ""education"": ""College educated"",
        ""occupation"": ""Professional""
      }},
      ""painPoints"": [""pain1"", ""pain2""],
      ""jobsToBeDone"": [""job1"", ""job2""],
      ""valuePropositions"": [""value1"", ""value2""],
      ""priorityScore"": 0.9,
      ""buyingBehavior"": ""Research-heavy, price-conscious"",
      ""willingnessToPay"": 100.00,
      ""preferredChannels"": ""Digital, direct sales"",
      ""influencingFactors"": [""factor1"", ""factor2""]
    }}
  ],
  ""customerJourneyInsights"": [""insight1"", ""insight2""],
  ""unmetNeeds"": [""need1"", ""need2""],
  ""marketValidationEvidence"": [""evidence1"", ""evidence2""],
  ""confidenceScore"": 0.85,
  ""summary"": ""Customer segmentation summary""
}}

Identify 3-5 distinct customer segments with detailed analysis for each.
";
    }

    private string BuildPersonaCreationPrompt(string segmentName, string ideaDescription)
    {
        return $@"
Create a detailed customer persona for the ""{segmentName}"" segment for this business idea:

Business Idea: {ideaDescription}
Target Segment: {segmentName}

Provide persona in JSON format:
{{
  ""name"": ""Persona Name (e.g., 'Tech-Savvy Sarah')"",
  ""segment"": ""{segmentName}"",
  ""description"": ""Brief persona description"",
  ""demographics"": {{
    ""ageRange"": ""30-35"",
    ""gender"": ""Female"",
    ""income"": ""$75,000"",
    ""education"": ""Master's Degree"",
    ""occupation"": ""Product Manager"",
    ""location"": ""Urban"",
    ""familyStatus"": ""Married, 1 child""
  }},
  ""goals"": [""goal1"", ""goal2""],
  ""frustrations"": [""frustration1"", ""frustration2""],
  ""motivations"": [""motivation1"", ""motivation2""],
  ""behaviorProfile"": {{
    ""technologyAdoption"": ""Early adopter"",
    ""purchasingBehavior"": ""Research-driven"",
    ""communicationStyle"": ""Direct, data-focused"",
    ""preferredChannels"": [""Email"", ""LinkedIn"", ""Mobile apps""],
    ""brandLoyalty"": ""Medium"",
    ""riskTolerance"": ""Medium""
  }},
  ""preferredSolutions"": [""solution1"", ""solution2""],
  ""decisionMakingProcess"": ""Research → Trial → Purchase"",
  ""influencingSources"": [""Colleagues"", ""Industry reports"", ""Reviews""],
  ""quote"": ""Representative quote from this persona""
}}

Make the persona realistic and detailed with specific characteristics.
";
    }

    private string BuildCustomerJourneyPrompt(CustomerSegment segment, string ideaDescription)
    {
        return $@"
Map the customer journey for the ""{segment.Name}"" segment for this business idea:

Business Idea: {ideaDescription}
Customer Segment: {segment.Name}
Key Pain Points: {string.Join(", ", segment.PainPoints)}

Provide journey mapping in JSON format:
{{
  ""segment"": ""{segment.Name}"",
  ""stages"": [
    {{
      ""name"": ""Awareness"",
      ""description"": ""Customer becomes aware of the problem"",
      ""customerActions"": [""action1"", ""action2""],
      ""customerThoughts"": [""thought1"", ""thought2""],
      ""customerEmotions"": [""emotion1"", ""emotion2""],
      ""touchpoints"": [""touchpoint1"", ""touchpoint2""],
      ""painPoints"": [""pain1"", ""pain2""],
      ""opportunities"": [""opportunity1"", ""opportunity2""]
    }}
  ],
  ""touchpoints"": [""Website"", ""Social media"", ""Referrals""],
  ""painPoints"": [""Overall journey pain points""],
  ""opportunityMoments"": [""Key moments for engagement""],
  ""keyInsights"": ""Critical insights about this customer journey""
}}

Include stages: Awareness, Consideration, Purchase, Onboarding, Usage, Advocacy.
";
    }

    private string BuildCustomerInsightsPrompt(CustomerSegmentationResult segmentation)
    {
        var segmentSummary = string.Join(", ", segmentation.CustomerSegments.Select(s => s.Name));

        return $@"
Generate actionable customer insights from this segmentation analysis:

Primary Target Segment: {segmentation.PrimaryTargetSegment}
Customer Segments: {segmentSummary}
Unmet Needs: {string.Join(", ", segmentation.UnmetNeeds)}

Provide insights in JSON format:
{{
  ""keyInsights"": [""insight1"", ""insight2""],
  ""actionableRecommendations"": [""recommendation1"", ""recommendation2""],
  ""productFeaturePriorities"": [""feature1"", ""feature2""],
  ""marketingMessages"": [""message1"", ""message2""],
  ""channelRecommendations"": [""channel1"", ""channel2""],
  ""pricingInsights"": [""insight1"", ""insight2""],
  ""customerAcquisitionStrategy"": ""Acquisition strategy summary"",
  ""customerRetentionStrategy"": ""Retention strategy summary""
}}

Focus on actionable insights that can drive product and marketing decisions.
";
    }

    private CustomerSegmentationResult ParseCustomerSegmentationResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                return new CustomerSegmentationResult
                {
                    PrimaryTargetSegment = parsed.GetProperty("primaryTargetSegment").GetString() ?? string.Empty,
                    CustomerSegments = ParseCustomerSegments(parsed.GetProperty("customerSegments")),
                    CustomerJourneyInsights = ParseStringArray(parsed.GetProperty("customerJourneyInsights")),
                    UnmetNeeds = ParseStringArray(parsed.GetProperty("unmetNeeds")),
                    MarketValidationEvidence = ParseStringArray(parsed.GetProperty("marketValidationEvidence")),
                    ConfidenceScore = parsed.GetProperty("confidenceScore").GetDouble(),
                    Summary = parsed.GetProperty("summary").GetString() ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse customer segmentation response, using fallback");
        }

        return CreateFallbackSegmentation(response);
    }

    private CustomerPersona ParseCustomerPersonaResponse(string response, string segmentName)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                return new CustomerPersona
                {
                    Name = parsed.GetProperty("name").GetString() ?? segmentName,
                    Segment = segmentName,
                    Description = parsed.GetProperty("description").GetString() ?? string.Empty,
                    Demographics = ParseDemographics(parsed.GetProperty("demographics")),
                    Goals = ParseStringArray(parsed.GetProperty("goals")),
                    Frustrations = ParseStringArray(parsed.GetProperty("frustrations")),
                    Motivations = ParseStringArray(parsed.GetProperty("motivations")),
                    BehaviorProfile = ParseBehaviorProfile(parsed.GetProperty("behaviorProfile")),
                    PreferredSolutions = ParseStringArray(parsed.GetProperty("preferredSolutions")),
                    DecisionMakingProcess = parsed.GetProperty("decisionMakingProcess").GetString() ?? string.Empty,
                    InfluencingSources = ParseStringArray(parsed.GetProperty("influencingSources")),
                    Quote = parsed.GetProperty("quote").GetString() ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse customer persona, using fallback");
        }

        return new CustomerPersona
        {
            Name = $"Representative {segmentName}",
            Segment = segmentName,
            Description = "Customer persona based on segment analysis"
        };
    }

    private CustomerJourney ParseCustomerJourneyResponse(string response, string segmentName)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                return new CustomerJourney
                {
                    Segment = segmentName,
                    Stages = ParseJourneyStages(parsed.GetProperty("stages")),
                    Touchpoints = ParseStringArray(parsed.GetProperty("touchpoints")),
                    PainPoints = ParseStringArray(parsed.GetProperty("painPoints")),
                    OpportunityMoments = ParseStringArray(parsed.GetProperty("opportunityMoments")),
                    KeyInsights = parsed.GetProperty("keyInsights").GetString() ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse customer journey, using fallback");
        }

        return new CustomerJourney
        {
            Segment = segmentName,
            KeyInsights = "Customer journey analysis for segment"
        };
    }

    private CustomerInsightResult ParseCustomerInsightsResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                return new CustomerInsightResult
                {
                    KeyInsights = ParseStringArray(parsed.GetProperty("keyInsights")),
                    ActionableRecommendations = ParseStringArray(parsed.GetProperty("actionableRecommendations")),
                    ProductFeaturePriorities = ParseStringArray(parsed.GetProperty("productFeaturePriorities")),
                    MarketingMessages = ParseStringArray(parsed.GetProperty("marketingMessages")),
                    ChannelRecommendations = ParseStringArray(parsed.GetProperty("channelRecommendations")),
                    PricingInsights = ParseStringArray(parsed.GetProperty("pricingInsights")),
                    CustomerAcquisitionStrategy = parsed.GetProperty("customerAcquisitionStrategy").GetString() ?? string.Empty,
                    CustomerRetentionStrategy = parsed.GetProperty("customerRetentionStrategy").GetString() ?? string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse customer insights, using fallback");
        }

        return new CustomerInsightResult
        {
            KeyInsights = new List<string> { "Customer-focused product development", "Targeted marketing approach" },
            CustomerAcquisitionStrategy = "Multi-channel customer acquisition strategy",
            CustomerRetentionStrategy = "Value-driven customer retention approach"
        };
    }

    private List<CustomerSegment> ParseCustomerSegments(JsonElement segmentsElement)
    {
        var segments = new List<CustomerSegment>();

        try
        {
            foreach (var segment in segmentsElement.EnumerateArray())
            {
                var demographics = new Dictionary<string, object>();
                
                if (segment.TryGetProperty("demographics", out var demoElement))
                {
                    foreach (var demo in demoElement.EnumerateObject())
                    {
                        demographics[demo.Name] = demo.Value.GetString() ?? string.Empty;
                    }
                }

                segments.Add(new CustomerSegment
                {
                    Name = segment.GetProperty("name").GetString() ?? string.Empty,
                    Description = segment.GetProperty("description").GetString() ?? string.Empty,
                    SizeEstimate = segment.GetProperty("sizeEstimate").GetInt32(),
                    Demographics = demographics,
                    PainPoints = ParseStringArray(segment.GetProperty("painPoints")),
                    JobsToBeDone = ParseStringArray(segment.GetProperty("jobsToBeDone")),
                    ValuePropositions = ParseStringArray(segment.GetProperty("valuePropositions")),
                    PriorityScore = segment.GetProperty("priorityScore").GetDouble(),
                    BuyingBehavior = segment.GetProperty("buyingBehavior").GetString() ?? string.Empty,
                    WillingnessToPay = segment.GetProperty("willingnessToPay").GetDecimal(),
                    PreferredChannels = segment.GetProperty("preferredChannels").GetString() ?? string.Empty,
                    InfluencingFactors = ParseStringArray(segment.GetProperty("influencingFactors"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse customer segments");
        }

        return segments;
    }

    private Demographics ParseDemographics(JsonElement demoElement)
    {
        try
        {
            return new Demographics
            {
                AgeRange = demoElement.GetProperty("ageRange").GetString() ?? string.Empty,
                Gender = demoElement.GetProperty("gender").GetString() ?? string.Empty,
                Income = demoElement.GetProperty("income").GetString() ?? string.Empty,
                Education = demoElement.GetProperty("education").GetString() ?? string.Empty,
                Occupation = demoElement.GetProperty("occupation").GetString() ?? string.Empty,
                Location = demoElement.GetProperty("location").GetString() ?? string.Empty,
                FamilyStatus = demoElement.GetProperty("familyStatus").GetString() ?? string.Empty
            };
        }
        catch
        {
            return new Demographics();
        }
    }

    private BehaviorProfile ParseBehaviorProfile(JsonElement behaviorElement)
    {
        try
        {
            return new BehaviorProfile
            {
                TechnologyAdoption = behaviorElement.GetProperty("technologyAdoption").GetString() ?? string.Empty,
                PurchasingBehavior = behaviorElement.GetProperty("purchasingBehavior").GetString() ?? string.Empty,
                CommunicationStyle = behaviorElement.GetProperty("communicationStyle").GetString() ?? string.Empty,
                PreferredChannels = ParseStringArray(behaviorElement.GetProperty("preferredChannels")),
                BrandLoyalty = behaviorElement.GetProperty("brandLoyalty").GetString() ?? string.Empty,
                RiskTolerance = behaviorElement.GetProperty("riskTolerance").GetString() ?? string.Empty
            };
        }
        catch
        {
            return new BehaviorProfile();
        }
    }

    private List<JourneyStage> ParseJourneyStages(JsonElement stagesElement)
    {
        var stages = new List<JourneyStage>();

        try
        {
            foreach (var stage in stagesElement.EnumerateArray())
            {
                stages.Add(new JourneyStage
                {
                    Name = stage.GetProperty("name").GetString() ?? string.Empty,
                    Description = stage.GetProperty("description").GetString() ?? string.Empty,
                    CustomerActions = ParseStringArray(stage.GetProperty("customerActions")),
                    CustomerThoughts = ParseStringArray(stage.GetProperty("customerThoughts")),
                    CustomerEmotions = ParseStringArray(stage.GetProperty("customerEmotions")),
                    Touchpoints = ParseStringArray(stage.GetProperty("touchpoints")),
                    PainPoints = ParseStringArray(stage.GetProperty("painPoints")),
                    Opportunities = ParseStringArray(stage.GetProperty("opportunities"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse journey stages");
        }

        return stages;
    }

    private List<string> ParseStringArray(JsonElement arrayElement)
    {
        var items = new List<string>();

        try
        {
            foreach (var item in arrayElement.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    items.Add(value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse string array");
        }

        return items;
    }

    private CustomerSegmentationResult CreateFallbackSegmentation(string response)
    {
        return new CustomerSegmentationResult
        {
            PrimaryTargetSegment = "Early Adopters",
            CustomerSegments = new List<CustomerSegment>
            {
                new()
                {
                    Name = "Early Adopters",
                    Description = "Technology enthusiasts willing to try new solutions",
                    SizeEstimate = 100000,
                    PainPoints = new List<string> { "Time constraints", "Complexity", "Cost" },
                    JobsToBeDone = new List<string> { "Increase productivity", "Reduce costs", "Improve quality" },
                    ValuePropositions = new List<string> { "Time savings", "Cost reduction", "Better outcomes" },
                    PriorityScore = 0.9,
                    BuyingBehavior = "Research-driven, early adoption",
                    WillingnessToPay = 100.00m,
                    PreferredChannels = "Digital channels, direct sales"
                }
            },
            UnmetNeeds = new List<string> { "Simplified interface", "Better integration", "Lower cost" },
            ConfidenceScore = 0.75,
            Summary = "Customer segmentation analysis identifying key target segments"
        };
    }
}