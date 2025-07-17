using Jackson.Ideas.Core.DTOs.Research;

namespace Jackson.Ideas.Core.Interfaces.Services;

public interface IResearchBackgroundService
{
    /// <summary>
    /// Enqueues a research task for background processing
    /// </summary>
    /// <param name="taskType">Type of research task to execute</param>
    /// <param name="sessionId">Research session ID</param>
    /// <param name="parameters">Task-specific parameters</param>
    /// <returns>Task ID for tracking progress</returns>
    string EnqueueTask(ResearchTaskType taskType, string sessionId, Dictionary<string, object> parameters);

    /// <summary>
    /// Gets the current status of a research task
    /// </summary>
    /// <param name="taskId">Task ID to check</param>
    /// <returns>Current task status or null if not found</returns>
    ResearchTaskStatus? GetTaskStatus(string taskId);

    /// <summary>
    /// Cancels a research task if it's not yet completed
    /// </summary>
    /// <param name="taskId">Task ID to cancel</param>
    void CancelTask(string taskId);

    /// <summary>
    /// Enqueues a complete research workflow for a session
    /// </summary>
    /// <param name="sessionId">Research session ID</param>
    /// <param name="ideaDescription">Description of the idea to research</param>
    /// <param name="researchType">Type of research strategy to execute</param>
    /// <param name="userGoals">User's specific research goals</param>
    /// <returns>List of task IDs for tracking workflow progress</returns>
    Task<List<string>> EnqueueResearchWorkflowAsync(string sessionId, string ideaDescription, string researchType, string userGoals);
}