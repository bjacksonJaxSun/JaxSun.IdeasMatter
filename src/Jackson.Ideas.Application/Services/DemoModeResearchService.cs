using Jackson.Ideas.Core.Configuration;
using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Jackson.Ideas.Application.Services;

/// <summary>
/// Demo-mode aware research service that provides mock data for UX review.
/// Implements UX Blueprint principles including confidence visualization,
/// progress tracking psychology, and contextual scaffolding.
/// </summary>
public class DemoModeResearchService : IResearchSessionService
{
    private readonly IResearchSessionService _realService;
    private readonly MockDataService _mockDataService;
    private readonly DemoModeOptions _demoOptions;

    public DemoModeResearchService(
        IResearchSessionService realService,
        MockDataService mockDataService,
        IOptions<DemoModeOptions> demoOptions)
    {
        _realService = realService;
        _mockDataService = mockDataService;
        _demoOptions = demoOptions.Value;
    }

    public async Task<ResearchSession> CreateSessionAsync(CreateSessionRequest request)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Simulate processing delay for realistic UX
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            // Create mock session with realistic data
            var mockSession = new ResearchSession
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Title = request.Title,
                Description = request.Description,
                Status = Core.Enums.ResearchStatus.Pending,
                Strategy = DetermineStrategy(request.Description),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ProgressPercentage = 0,
                EstimatedCompletionTime = GetEstimatedTimeForRequest(request),
                ConfidenceScore = 85, // High confidence for demo
                Industry = DetectIndustry(request.Description),
                TargetMarket = GenerateTargetMarket(request.Description),
                PrimaryGoal = "Validate market opportunity and create go-to-market strategy"
            };

            return mockSession;
        }

        return await _realService.CreateSessionAsync(request);
    }

    public async Task<ResearchSession?> GetSessionAsync(Guid sessionId)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Return mock session data
            var mockSessions = _mockDataService.GetMockResearchSessions("demo-user", 5);
            return mockSessions.FirstOrDefault(s => s.Id == sessionId) 
                ?? mockSessions.First(); // Fallback to first session for demo
        }

        return await _realService.GetSessionAsync(sessionId);
    }

    public async Task<IEnumerable<ResearchSession>> GetUserSessionsAsync(string userId)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Return comprehensive mock sessions demonstrating various states
            return _mockDataService.GetMockResearchSessions(userId, 8);
        }

        return await _realService.GetUserSessionsAsync(userId);
    }

    public async Task<ResearchSession> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Simulate update with enhanced progress tracking
            var session = await GetSessionAsync(sessionId);
            if (session != null)
            {
                session.UpdatedAt = DateTime.UtcNow;
                session.ProgressPercentage = Math.Min(100, session.ProgressPercentage + 10);
                
                // Update status based on progress following UX Blueprint patterns
                if (session.ProgressPercentage >= 100)
                {
                    session.Status = Core.Enums.ResearchStatus.Completed;
                }
                else if (session.ProgressPercentage > 0)
                {
                    session.Status = Core.Enums.ResearchStatus.InProgress;
                }

                // Update title and description if provided
                if (!string.IsNullOrEmpty(request.Title))
                    session.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Description))
                    session.Description = request.Description;
            }
            return session!;
        }

        return await _realService.UpdateSessionAsync(sessionId, request);
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Simulate successful deletion
            await Task.Delay(100);
            return true;
        }

        return await _realService.DeleteSessionAsync(sessionId);
    }

    public async Task<bool> UpdateStatusAsync(Guid sessionId, UpdateStatusRequest request)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            // Simulate status update with realistic delays
            if (_demoOptions.SimulateProcessingDelays)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            return true;
        }

        return await _realService.UpdateStatusAsync(sessionId, request);
    }

    // Backward compatibility methods
    public async Task<ResearchSession> AddInsightToSessionAsync(Guid sessionId, ResearchInsight insight)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            var session = await GetSessionAsync(sessionId);
            if (session != null)
            {
                insight.Id = Guid.NewGuid();
                insight.ResearchSessionId = sessionId;
                insight.CreatedAt = DateTime.UtcNow;
                session.ResearchInsights ??= new List<ResearchInsight>();
                session.ResearchInsights.Add(insight);
                session.UpdatedAt = DateTime.UtcNow;
            }
            return session!;
        }

        return await _realService.AddInsightToSessionAsync(sessionId, insight);
    }

    public async Task<ResearchSession> AddOptionToSessionAsync(Guid sessionId, ResearchOption option)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            var session = await GetSessionAsync(sessionId);
            if (session != null)
            {
                option.Id = Guid.NewGuid();
                option.ResearchSessionId = sessionId;
                option.CreatedAt = DateTime.UtcNow;
                session.ResearchOptions ??= new List<ResearchOption>();
                session.ResearchOptions.Add(option);
                session.UpdatedAt = DateTime.UtcNow;
            }
            return session!;
        }

        return await _realService.AddOptionToSessionAsync(sessionId, option);
    }

    public async Task<List<ResearchInsight>> GetSessionInsightsAsync(Guid sessionId)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            var session = await GetSessionAsync(sessionId);
            return session?.ResearchInsights?.ToList() ?? new List<ResearchInsight>();
        }

        return await _realService.GetSessionInsightsAsync(sessionId);
    }

    public async Task<List<ResearchOption>> GetSessionOptionsAsync(Guid sessionId)
    {
        if (_demoOptions.Enabled && !_demoOptions.UseRealAI)
        {
            var session = await GetSessionAsync(sessionId);
            return session?.ResearchOptions?.ToList() ?? new List<ResearchOption>();
        }

        return await _realService.GetSessionOptionsAsync(sessionId);
    }

    // Helper methods for mock data generation
    private string DetermineStrategy(string description)
    {
        var strategies = new[] { "Quick Validation", "Market Deep-Dive", "Launch Strategy" };
        
        // Simple heuristics for demo purposes
        if (description.Contains("validate", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            return "Quick Validation";
        }
        
        if (description.Contains("launch", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("go-to-market", StringComparison.OrdinalIgnoreCase))
        {
            return "Launch Strategy";
        }
        
        return "Market Deep-Dive";
    }

    private int GetEstimatedTimeForRequest(CreateSessionRequest request)
    {
        var strategy = DetermineStrategy(request.Description);
        return strategy switch
        {
            "Quick Validation" => 15,
            "Market Deep-Dive" => 45,
            "Launch Strategy" => 90,
            _ => 30
        };
    }

    private string DetectIndustry(string description)
    {
        var industries = new Dictionary<string, string[]>
        {
            ["FinTech"] = ["payment", "banking", "finance", "crypto", "investment"],
            ["E-Commerce"] = ["shopping", "marketplace", "retail", "store", "commerce"],
            ["SaaS"] = ["software", "platform", "tool", "service", "automation"],
            ["HealthTech"] = ["health", "medical", "wellness", "fitness", "therapy"],
            ["EdTech"] = ["education", "learning", "teaching", "course", "training"]
        };

        var lowerDescription = description.ToLowerInvariant();
        
        foreach (var industry in industries)
        {
            if (industry.Value.Any(keyword => lowerDescription.Contains(keyword)))
            {
                return industry.Key;
            }
        }
        
        return "Technology";
    }

    private string GenerateTargetMarket(string description)
    {
        var industry = DetectIndustry(description);
        
        return industry switch
        {
            "FinTech" => "Tech-savvy millennials and professionals seeking modern financial solutions",
            "E-Commerce" => "Online shoppers aged 25-45 with focus on convenience and value",
            "SaaS" => "Small to medium businesses looking to streamline operations and increase efficiency",
            "HealthTech" => "Health-conscious individuals and healthcare providers embracing digital solutions",
            "EdTech" => "Students, educators, and organizations focused on continuous learning",
            _ => "Early adopters and innovation-focused organizations in the technology sector"
        };
    }
}