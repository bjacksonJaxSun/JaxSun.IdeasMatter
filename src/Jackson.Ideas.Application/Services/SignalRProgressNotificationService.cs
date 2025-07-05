using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Jackson.Ideas.Application.Services;

public class SignalRProgressNotificationService : IProgressNotificationService
{
    private readonly ILogger<SignalRProgressNotificationService> _logger;

    public SignalRProgressNotificationService(ILogger<SignalRProgressNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyTaskProgressUpdate(string taskId, int progress, string status, string? message = null)
    {
        _logger.LogInformation("Task {TaskId} progress: {Progress}% - {Status} - {Message}", 
            taskId, progress, status, message);
        
        // This will be enhanced with actual SignalR integration when injected via DI
        await Task.CompletedTask;
    }

    public async Task NotifyTaskCompleted(string taskId, object result)
    {
        _logger.LogInformation("Task {TaskId} completed successfully", taskId);
        await Task.CompletedTask;
    }

    public async Task NotifyTaskFailed(string taskId, string errorMessage)
    {
        _logger.LogWarning("Task {TaskId} failed: {ErrorMessage}", taskId, errorMessage);
        await Task.CompletedTask;
    }

    public async Task NotifySessionUpdate(string sessionId, string message, object? data = null)
    {
        _logger.LogInformation("Session {SessionId} update: {Message}", sessionId, message);
        await Task.CompletedTask;
    }

    public async Task NotifyAnalysisProgress(string sessionId, string analysisType, int progress, string status)
    {
        _logger.LogInformation("Session {SessionId} - {AnalysisType} progress: {Progress}% - {Status}", 
            sessionId, analysisType, progress, status);
        await Task.CompletedTask;
    }
}