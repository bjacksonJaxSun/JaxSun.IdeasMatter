using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Jackson.Ideas.Application.Services;

public class ProgressNotificationService : IProgressNotificationService
{
    private readonly ILogger<ProgressNotificationService> _logger;

    public ProgressNotificationService(ILogger<ProgressNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyTaskProgressUpdate(string taskId, int progress, string status, string? message = null)
    {
        _logger.LogInformation("Task {TaskId} progress: {Progress}% - {Status}", taskId, progress, status);
        // This will be implemented with SignalR integration in the API project
        await Task.CompletedTask;
    }

    public async Task NotifyTaskCompleted(string taskId, object result)
    {
        _logger.LogInformation("Task {TaskId} completed successfully", taskId);
        // This will be implemented with SignalR integration in the API project
        await Task.CompletedTask;
    }

    public async Task NotifyTaskFailed(string taskId, string errorMessage)
    {
        _logger.LogWarning("Task {TaskId} failed: {ErrorMessage}", taskId, errorMessage);
        // This will be implemented with SignalR integration in the API project
        await Task.CompletedTask;
    }

    public async Task NotifySessionUpdate(string sessionId, string message, object? data = null)
    {
        _logger.LogInformation("Session {SessionId} update: {Message}", sessionId, message);
        // This will be implemented with SignalR integration in the API project
        await Task.CompletedTask;
    }

    public async Task NotifyAnalysisProgress(string sessionId, string analysisType, int progress, string status)
    {
        _logger.LogInformation("Session {SessionId} - {AnalysisType} progress: {Progress}% - {Status}", 
            sessionId, analysisType, progress, status);
        // This will be implemented with SignalR integration in the API project
        await Task.CompletedTask;
    }
}