using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.DTOs.MarketAnalysis;
using Jackson.Ideas.Core.Interfaces.Services;
using Jackson.Ideas.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jackson.Ideas.Application.Services;

public class ResearchBackgroundService : BackgroundService, IResearchBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ResearchBackgroundService> _logger;
    private readonly ConcurrentQueue<ResearchTaskItem> _taskQueue;
    private readonly ConcurrentDictionary<string, ResearchTaskStatus> _taskStatuses;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public ResearchBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ResearchBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _taskQueue = new ConcurrentQueue<ResearchTaskItem>();
        _taskStatuses = new ConcurrentDictionary<string, ResearchTaskStatus>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public string EnqueueTask(ResearchTaskType taskType, string sessionId, Dictionary<string, object> parameters)
    {
        var taskId = Guid.NewGuid().ToString();
        var taskItem = new ResearchTaskItem
        {
            TaskId = taskId,
            SessionId = sessionId,
            TaskType = taskType,
            Parameters = parameters,
            CreatedAt = DateTime.UtcNow
        };

        _taskQueue.Enqueue(taskItem);
        _taskStatuses[taskId] = new ResearchTaskStatus
        {
            TaskId = taskId,
            Status = "Queued",
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Research task {TaskId} of type {TaskType} enqueued for session {SessionId}", 
            taskId, taskType, sessionId);

        return taskId;
    }

    public ResearchTaskStatus? GetTaskStatus(string taskId)
    {
        _taskStatuses.TryGetValue(taskId, out var status);
        return status;
    }

    public void CancelTask(string taskId)
    {
        if (_taskStatuses.TryGetValue(taskId, out var status) && 
            status.Status != "Completed" && 
            status.Status != "Failed")
        {
            status.Status = "Cancelled";
            status.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Research task {TaskId} cancelled", taskId);
        }
    }

    public async Task<List<string>> EnqueueResearchWorkflowAsync(string sessionId, string ideaDescription, string researchType, string userGoals)
    {
        _logger.LogInformation("Enqueuing research workflow for session {SessionId} with type {ResearchType}", sessionId, researchType);
        
        var taskIds = new List<string>();
        var parameters = new Dictionary<string, object>
        {
            ["ideaDescription"] = ideaDescription,
            ["researchType"] = researchType,
            ["userGoals"] = userGoals,
            ["sessionId"] = sessionId
        };

        // Start with updating session status to InProgress
        await UpdateSessionStatusAsync(sessionId, ResearchStatus.InProgress);

        // Determine workflow based on research type
        var workflow = GetWorkflowTasks(researchType);
        
        foreach (var taskType in workflow)
        {
            var taskId = EnqueueTask(taskType, sessionId, parameters);
            taskIds.Add(taskId);
            
            // Add small delay between tasks to prevent overwhelming
            await Task.Delay(100);
        }

        _logger.LogInformation("Enqueued {TaskCount} tasks for research workflow in session {SessionId}", taskIds.Count, sessionId);
        return taskIds;
    }

    private List<ResearchTaskType> GetWorkflowTasks(string researchType)
    {
        return researchType.ToLower() switch
        {
            "quick validation" or "quick-validation" => new List<ResearchTaskType>
            {
                ResearchTaskType.MarketAnalysis,
                ResearchTaskType.CompetitiveAnalysis,
                ResearchTaskType.SwotAnalysis
            },
            "market deep-dive" or "market-deep-dive" => new List<ResearchTaskType>
            {
                ResearchTaskType.MarketAnalysis,
                ResearchTaskType.CompetitiveAnalysis,
                ResearchTaskType.CustomerSegmentation,
                ResearchTaskType.SwotAnalysis,
                ResearchTaskType.EnhancedSwotAnalysis
            },
            "launch strategy" or "launch-strategy" => new List<ResearchTaskType>
            {
                ResearchTaskType.MarketAnalysis,
                ResearchTaskType.CompetitiveAnalysis,
                ResearchTaskType.CustomerSegmentation,
                ResearchTaskType.SwotAnalysis,
                ResearchTaskType.EnhancedSwotAnalysis,
                ResearchTaskType.StrategicImplications
            },
            _ => new List<ResearchTaskType>
            {
                ResearchTaskType.MarketAnalysis,
                ResearchTaskType.CompetitiveAnalysis,
                ResearchTaskType.SwotAnalysis
            }
        };
    }

    private async Task UpdateSessionStatusAsync(string sessionId, ResearchStatus status)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionService = scope.ServiceProvider.GetService<IResearchSessionService>();
            
            if (sessionService != null && Guid.TryParse(sessionId, out var sessionGuid))
            {
                await sessionService.UpdateStatusAsync(sessionGuid, new UpdateStatusRequest 
                { 
                    Status = status.ToString() 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update session status for session {SessionId}", sessionId);
        }
    }

    private async Task CheckAndCompleteSessionIfAllTasksDone(string sessionId)
    {
        try
        {
            // Get all tasks for this session
            var sessionTasks = _taskStatuses.Values
                .Where(t => t.TaskId.Contains(sessionId) || 
                           _taskQueue.Any(q => q.SessionId == sessionId && q.TaskId == t.TaskId))
                .ToList();

            // Check if all tasks are completed
            var allCompleted = sessionTasks.All(t => t.Status == "Completed" || t.Status == "Failed");
            var hasAnyCompleted = sessionTasks.Any(t => t.Status == "Completed");

            if (allCompleted && hasAnyCompleted)
            {
                _logger.LogInformation("All research tasks completed for session {SessionId}, marking session as completed", sessionId);
                await UpdateSessionStatusAsync(sessionId, ResearchStatus.Completed);
            }
            else if (sessionTasks.Any(t => t.Status == "Failed") && !sessionTasks.Any(t => t.Status == "Processing"))
            {
                _logger.LogWarning("Some research tasks failed for session {SessionId}, marking session as failed", sessionId);
                await UpdateSessionStatusAsync(sessionId, ResearchStatus.Failed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check session completion status for session {SessionId}", sessionId);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Research Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_taskQueue.TryDequeue(out var taskItem))
                {
                    await ProcessTaskAsync(taskItem, stoppingToken);
                }
                else
                {
                    await Task.Delay(1000, stoppingToken); // Wait 1 second if no tasks
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Research Background Service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Research Background Service execution loop");
                await Task.Delay(5000, stoppingToken); // Wait 5 seconds on error
            }
        }

        _logger.LogInformation("Research Background Service stopped");
    }

    private async Task ProcessTaskAsync(ResearchTaskItem taskItem, CancellationToken cancellationToken)
    {
        var taskId = taskItem.TaskId;
        
        if (!_taskStatuses.TryGetValue(taskId, out var status))
        {
            _logger.LogWarning("Task status not found for task {TaskId}", taskId);
            return;
        }

        if (status.Status == "Cancelled")
        {
            _logger.LogInformation("Skipping cancelled task {TaskId}", taskId);
            return;
        }

        try
        {
            _logger.LogInformation("Processing research task {TaskId} of type {TaskType}", 
                taskId, taskItem.TaskType);

            status.Status = "Processing";
            status.StartedAt = DateTime.UtcNow;
            status.Progress = 10;

            // Send progress update via SignalR
            await NotifyProgressUpdate(taskId, 10, "Processing", "Task started");

            using var scope = _serviceProvider.CreateScope();
            
            // Update progress periodically during execution
            await NotifyProgressUpdate(taskId, 30, "Processing", "Analyzing data");
            
            var result = await ExecuteResearchTaskAsync(taskItem, scope.ServiceProvider, cancellationToken);

            status.Status = "Completed";
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.Result = result;

            // Send completion notification
            await NotifyTaskCompleted(taskId, result);

            // Check if all tasks for this session are completed
            await CheckAndCompleteSessionIfAllTasksDone(taskItem.SessionId);

            _logger.LogInformation("Research task {TaskId} completed successfully", taskId);
        }
        catch (OperationCanceledException)
        {
            status.Status = "Cancelled";
            status.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Research task {TaskId} was cancelled", taskId);
        }
        catch (Exception ex)
        {
            status.Status = "Failed";
            status.Progress = 0;
            status.CompletedAt = DateTime.UtcNow;
            status.ErrorMessage = ex.Message;

            // Send failure notification
            await NotifyTaskFailed(taskId, ex.Message);

            _logger.LogError(ex, "Research task {TaskId} failed", taskId);
        }
    }

    private async Task<object?> ExecuteResearchTaskAsync(
        ResearchTaskItem taskItem, 
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var ideaDescription = taskItem.Parameters.GetValueOrDefault("ideaDescription")?.ToString() ?? string.Empty;
        
        return taskItem.TaskType switch
        {
            ResearchTaskType.MarketAnalysis => await ExecuteMarketAnalysisAsync(
                serviceProvider, ideaDescription, taskItem.Parameters, cancellationToken),
                
            ResearchTaskType.CompetitiveAnalysis => await ExecuteCompetitiveAnalysisAsync(
                serviceProvider, ideaDescription, taskItem.Parameters, cancellationToken),
            
            ResearchTaskType.SwotAnalysis => await ExecuteSwotAnalysisAsync(
                serviceProvider, ideaDescription, taskItem.Parameters, cancellationToken),
            
            ResearchTaskType.CustomerSegmentation => await ExecuteCustomerSegmentationAsync(
                serviceProvider, ideaDescription, taskItem.Parameters, cancellationToken),
            
            ResearchTaskType.EnhancedSwotAnalysis => await ExecuteEnhancedSwotAnalysisAsync(
                serviceProvider, taskItem.Parameters, cancellationToken),
            
            ResearchTaskType.StrategicImplications => await ExecuteStrategicImplicationsAsync(
                serviceProvider, taskItem.Parameters, cancellationToken),
            
            _ => throw new ArgumentException($"Unknown task type: {taskItem.TaskType}")
        };
    }

    private async Task<object> ExecuteMarketAnalysisAsync(
        IServiceProvider serviceProvider,
        string ideaDescription,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<IMarketAnalysisService>();
        var sessionId = parameters.GetValueOrDefault("sessionId")?.ToString() ?? string.Empty;
        var ideaTitle = parameters.GetValueOrDefault("ideaTitle")?.ToString() ?? "Market Analysis";
        
        // Execute market analysis and save results to session
        ComprehensiveMarketAnalysisDto analysis;
        if (Guid.TryParse(sessionId, out var sessionGuid))
        {
            // Try to convert Guid to int for the service (use hash code as fallback)
            var sessionIdInt = Math.Abs(sessionGuid.GetHashCode());
            analysis = await service.GenerateComprehensiveMarketAnalysisAsync(sessionIdInt, ideaTitle, ideaDescription, cancellationToken);
            
            // Convert to research insight format and save to session
            var sessionService = serviceProvider.GetRequiredService<IResearchSessionService>();
            var insight = new Core.Entities.ResearchInsight
            {
                Title = "Market Analysis",
                Content = analysis.ExecutiveSummary ?? "Comprehensive market analysis completed",
                Category = "Market Research",
                Priority = "High",
                Phase = "market_context",
                CreatedAt = DateTime.UtcNow
            };
            
            await sessionService.AddInsightToSessionAsync(sessionGuid, insight);
        }
        else
        {
            // Fallback if session ID parsing fails
            analysis = await service.GenerateComprehensiveMarketAnalysisAsync(0, ideaTitle, ideaDescription, cancellationToken);
        }
        
        return analysis;
    }

    private async Task<CompetitiveAnalysisResult> ExecuteCompetitiveAnalysisAsync(
        IServiceProvider serviceProvider,
        string ideaDescription,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<ICompetitiveAnalysisService>();
        var targetMarket = parameters.GetValueOrDefault("targetMarket")?.ToString() ?? string.Empty;
        
        return await service.AnalyzeCompetitorsAsync(ideaDescription, targetMarket, parameters);
    }

    private async Task<SwotAnalysisResult> ExecuteSwotAnalysisAsync(
        IServiceProvider serviceProvider,
        string ideaDescription,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<ISwotAnalysisService>();
        return await service.GenerateSwotAnalysisAsync(ideaDescription, parameters);
    }

    private async Task<CustomerSegmentationResult> ExecuteCustomerSegmentationAsync(
        IServiceProvider serviceProvider,
        string ideaDescription,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<ICustomerSegmentationService>();
        return await service.AnalyzeCustomerSegmentsAsync(ideaDescription, parameters);
    }

    private async Task<SwotAnalysisResult> ExecuteEnhancedSwotAnalysisAsync(
        IServiceProvider serviceProvider,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<ISwotAnalysisService>();
        var ideaDescription = parameters.GetValueOrDefault("ideaDescription")?.ToString() ?? string.Empty;
        var competitiveAnalysis = parameters.GetValueOrDefault("competitiveAnalysis") as CompetitiveAnalysisResult;
        
        return await service.GenerateEnhancedSwotAnalysisAsync(ideaDescription, competitiveAnalysis, parameters);
    }

    private async Task<StrategicImplicationsResult> ExecuteStrategicImplicationsAsync(
        IServiceProvider serviceProvider,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
    {
        var service = serviceProvider.GetRequiredService<ISwotAnalysisService>();
        var swotAnalysis = parameters.GetValueOrDefault("swotAnalysis") as SwotAnalysisResult 
            ?? throw new ArgumentException("SWOT analysis result is required for strategic implications");
        
        return await service.AnalyzeStrategicImplicationsAsync(swotAnalysis, parameters);
    }

    private async Task NotifyProgressUpdate(string taskId, int progress, string status, string message)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetService<IProgressNotificationService>();
            
            if (notificationService != null)
            {
                await notificationService.NotifyTaskProgressUpdate(taskId, progress, status, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send progress update for task {TaskId}", taskId);
        }
    }

    private async Task NotifyTaskCompleted(string taskId, object? result)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetService<IProgressNotificationService>();
            
            if (notificationService != null)
            {
                await notificationService.NotifyTaskCompleted(taskId, result ?? new { });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send completion notification for task {TaskId}", taskId);
        }
    }

    private async Task NotifyTaskFailed(string taskId, string errorMessage)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetService<IProgressNotificationService>();
            
            if (notificationService != null)
            {
                await notificationService.NotifyTaskFailed(taskId, errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send failure notification for task {TaskId}", taskId);
        }
    }

    public override void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.Dispose();
    }
}

