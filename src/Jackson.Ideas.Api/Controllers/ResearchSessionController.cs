using Jackson.Ideas.Core.DTOs.Research;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ResearchSessionController : ControllerBase
{
    private readonly IResearchSessionService _researchSessionService;
    private readonly IResearchBackgroundService _backgroundService;
    private readonly ILogger<ResearchSessionController> _logger;

    public ResearchSessionController(
        IResearchSessionService researchSessionService,
        IResearchBackgroundService backgroundService,
        ILogger<ResearchSessionController> logger)
    {
        _researchSessionService = researchSessionService;
        _backgroundService = backgroundService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ResearchSession>> CreateSessionAsync(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("Creating research session for user {UserId}", userId);

            // Update request with user ID
            request.UserId = userId;
            
            var session = await _researchSessionService.CreateSessionAsync(request);

            return CreatedAtAction(
                nameof(GetSessionAsync),
                new { sessionId = session.Id },
                session);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for session creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating research session");
            return StatusCode(500, new { error = "An error occurred while creating the session" });
        }
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<ResearchSession>> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while retrieving the session" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ResearchSession>>> GetUserSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var sessions = await _researchSessionService.GetUserSessionsAsync(userId);

            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user sessions");
            return StatusCode(500, new { error = "An error occurred while retrieving sessions" });
        }
    }

    [HttpPut("{sessionId}")]
    public async Task<ActionResult<ResearchSession>> UpdateSessionAsync(
        Guid sessionId,
        [FromBody] UpdateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            var updatedSession = await _researchSessionService.UpdateSessionAsync(sessionId, request);

            return Ok(updatedSession);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for session update");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while updating the session" });
        }
    }

    [HttpDelete("{sessionId}")]
    public async Task<ActionResult> DeleteSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            await _researchSessionService.DeleteSessionAsync(sessionId);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid session ID for deletion");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while deleting the session" });
        }
    }

    [HttpPost("{sessionId}/insights")]
    public async Task<ActionResult<ResearchSession>> AddInsightAsync(
        Guid sessionId,
        [FromBody] AddInsightRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            var insight = new ResearchInsight
            {
                Title = request.Title,
                Content = request.Content,
                Category = request.Category,
                Priority = request.Priority.ToString()
            };

            var updatedSession = await _researchSessionService.AddInsightToSessionAsync(sessionId, insight);

            return Ok(updatedSession);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for adding insight");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding insight to session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while adding the insight" });
        }
    }

    [HttpPost("{sessionId}/options")]
    public async Task<ActionResult<ResearchSession>> AddOptionAsync(
        Guid sessionId,
        [FromBody] AddOptionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            var option = new ResearchOption
            {
                Title = request.Title,
                Description = request.Description,
                FeasibilityScore = request.FeasibilityScore,
                ImpactScore = request.ImpactScore,
                Notes = request.Notes
            };

            var updatedSession = await _researchSessionService.AddOptionToSessionAsync(sessionId, option);

            return Ok(updatedSession);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid parameters for adding option");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding option to session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while adding the option" });
        }
    }

    [HttpPut("{sessionId}/status")]
    public async Task<ActionResult<ResearchSession>> UpdateSessionStatusAsync(
        Guid sessionId,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);

            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            var success = await _researchSessionService.UpdateStatusAsync(sessionId, request);

            if (success)
            {
                var updatedSession = await _researchSessionService.GetSessionAsync(sessionId);
                return Ok(updatedSession);
            }
            else
            {
                return BadRequest(new { error = "Failed to update session status" });
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid status for session update");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session status {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while updating session status" });
        }
    }

    [HttpPost("{sessionId}/execute")]
    public async Task<ActionResult<object>> ExecuteResearchAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            _logger.LogInformation("Starting research execution for session {SessionId}", sessionId);

            // Trigger the research workflow
            var taskIds = await _backgroundService.EnqueueResearchWorkflowAsync(
                sessionId.ToString(),
                session.Description,
                session.ResearchType ?? "Market Deep-Dive",
                session.Goals ?? "Comprehensive market analysis");

            return Ok(new
            {
                message = "Research execution started",
                sessionId = sessionId,
                taskIds = taskIds,
                estimatedDuration = "30-60 minutes"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting research execution for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while starting research execution" });
        }
    }

    [HttpGet("{sessionId}/progress")]
    public async Task<ActionResult<object>> GetResearchProgressAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _researchSessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Verify the session belongs to the current user
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid();
            }

            return Ok(new
            {
                sessionId = sessionId,
                status = session.Status.ToString(),
                progress = session.ProgressPercentage,
                currentPhase = session.CurrentPhase,
                insightsCount = session.ResearchInsights?.Count ?? 0,
                optionsCount = session.ResearchOptions?.Count ?? 0,
                lastUpdated = session.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting research progress for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An error occurred while getting research progress" });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found in claims");
    }
}