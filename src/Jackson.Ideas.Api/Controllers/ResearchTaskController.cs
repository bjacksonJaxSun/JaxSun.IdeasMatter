using Jackson.Ideas.Application.Services;
using Jackson.Ideas.Core.DTOs.Research;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ResearchTaskController : ControllerBase
{
    private readonly ResearchBackgroundService _backgroundService;
    private readonly ILogger<ResearchTaskController> _logger;

    public ResearchTaskController(
        ResearchBackgroundService backgroundService,
        ILogger<ResearchTaskController> logger)
    {
        _backgroundService = backgroundService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartTaskResponse>> StartResearchTaskAsync(
        [FromBody] StartTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting research task of type {TaskType} for session {SessionId}", 
                request.TaskType, request.SessionId);

            var taskId = _backgroundService.EnqueueTask(
                request.TaskType,
                request.SessionId,
                request.Parameters);

            return Ok(new StartTaskResponse
            {
                TaskId = taskId,
                Status = "Queued",
                Message = "Research task has been queued for processing"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for starting research task");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting research task");
            return StatusCode(500, new { error = "An error occurred while starting the research task" });
        }
    }

    [HttpGet("{taskId}/status")]
    public async Task<ActionResult<ResearchTaskStatus>> GetTaskStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = _backgroundService.GetTaskStatus(taskId);
            
            if (status == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task status for {TaskId}", taskId);
            return StatusCode(500, new { error = "An error occurred while retrieving task status" });
        }
    }

    [HttpPost("{taskId}/cancel")]
    public async Task<ActionResult> CancelTaskAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling research task {TaskId}", taskId);

            var status = _backgroundService.GetTaskStatus(taskId);
            if (status == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            if (status.Status == "Completed" || status.Status == "Failed")
            {
                return BadRequest(new { error = "Cannot cancel a completed or failed task" });
            }

            _backgroundService.CancelTask(taskId);

            return Ok(new { message = "Task cancellation requested" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling task {TaskId}", taskId);
            return StatusCode(500, new { error = "An error occurred while cancelling the task" });
        }
    }

    [HttpGet("{taskId}/result")]
    public async Task<ActionResult<object>> GetTaskResultAsync(
        string taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = _backgroundService.GetTaskStatus(taskId);
            
            if (status == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            if (status.Status != "Completed")
            {
                return BadRequest(new { 
                    error = "Task is not completed", 
                    currentStatus = status.Status,
                    progress = status.Progress
                });
            }

            return Ok(new
            {
                taskId = taskId,
                status = status.Status,
                result = status.Result,
                completedAt = status.CompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task result for {TaskId}", taskId);
            return StatusCode(500, new { error = "An error occurred while retrieving task result" });
        }
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<List<ResearchTaskStatus>>> GetTasksForSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: In a real implementation, you'd want to store task-to-session mappings
            // For now, this is a placeholder that would need session-based task tracking
            _logger.LogInformation("Retrieving tasks for session {SessionId}", sessionId);

            return Ok(new List<ResearchTaskStatus>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while retrieving session tasks" });
        }
    }
}

public class StartTaskRequest
{
    public ResearchTaskType TaskType { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class StartTaskResponse
{
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}