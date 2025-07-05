using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Jackson.Ideas.Api.Hubs;

[Authorize]
public class ResearchProgressHub : Hub
{
    private readonly ILogger<ResearchProgressHub> _logger;

    public ResearchProgressHub(ILogger<ResearchProgressHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} connected to research progress hub", userId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from research progress hub", userId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with exception", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinSessionGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetSessionGroupName(sessionId));
        _logger.LogInformation("User {UserId} joined session group {SessionId}", 
            Context.UserIdentifier, sessionId);
    }

    public async Task LeaveSessionGroup(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetSessionGroupName(sessionId));
        _logger.LogInformation("User {UserId} left session group {SessionId}", 
            Context.UserIdentifier, sessionId);
    }

    public async Task JoinTaskGroup(string taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetTaskGroupName(taskId));
        _logger.LogInformation("User {UserId} joined task group {TaskId}", 
            Context.UserIdentifier, taskId);
    }

    public async Task LeaveTaskGroup(string taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetTaskGroupName(taskId));
        _logger.LogInformation("User {UserId} left task group {TaskId}", 
            Context.UserIdentifier, taskId);
    }

    private static string GetSessionGroupName(string sessionId) => $"session_{sessionId}";
    private static string GetTaskGroupName(string taskId) => $"task_{taskId}";
}

