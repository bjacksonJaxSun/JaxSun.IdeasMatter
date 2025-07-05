using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jackson.Ideas.Application.Services;

public class ResearchBackgroundService : BackgroundService
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

