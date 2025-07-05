namespace Jackson.Ideas.Core.Interfaces.Services;

public interface IProgressNotificationService
{
    Task NotifyTaskProgressUpdate(string taskId, int progress, string status, string? message = null);
    Task NotifyTaskCompleted(string taskId, object result);
    Task NotifyTaskFailed(string taskId, string errorMessage);
    Task NotifySessionUpdate(string sessionId, string message, object? data = null);
    Task NotifyAnalysisProgress(string sessionId, string analysisType, int progress, string status);
}