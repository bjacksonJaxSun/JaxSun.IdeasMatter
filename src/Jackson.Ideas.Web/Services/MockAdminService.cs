using Jackson.Ideas.Core.DTOs.Auth;

namespace Jackson.Ideas.Web.Services;

public class MockAdminService : IAdminService
{
    private readonly ILogger<MockAdminService> _logger;
    private readonly List<UserInfo> _mockUsers;

    public MockAdminService(ILogger<MockAdminService> logger)
    {
        _logger = logger;
        
        // Create mock users for demo
        _mockUsers = new List<UserInfo>
        {
            new UserInfo
            {
                Id = "demo-user-123",
                Email = "demo@ideasmatter.com",
                Name = "Demo User",
                Role = "User",
                IsVerified = true,
                Permissions = new[] { "create_ideas", "view_research", "export_reports" }
            },
            new UserInfo
            {
                Id = "admin-user-456",
                Email = "admin@ideasmatter.com",
                Name = "Admin User",
                Role = "Admin",
                IsVerified = true,
                Permissions = new[] { "create_ideas", "view_research", "export_reports", "manage_users", "system_settings" }
            }
        };
    }

    public Task<List<UserInfo>> GetAllUsersAsync()
    {
        _logger.LogInformation("Mock admin service: Getting all users");
        return Task.FromResult(_mockUsers);
    }

    public Task<UserInfo?> GetUserByIdAsync(string id)
    {
        _logger.LogInformation("Mock admin service: Getting user by ID {UserId}", id);
        var user = _mockUsers.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<AuthResponse> UpdateUserRoleAsync(string id, string role)
    {
        _logger.LogInformation("Mock admin service: Updating user {UserId} role to {Role}", id, role);
        
        var user = _mockUsers.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.Role = role;
            return Task.FromResult(new AuthResponse
            {
                IsSuccess = true,
                Message = "Mock user role updated successfully"
            });
        }
        
        return Task.FromResult(new AuthResponse
        {
            IsSuccess = false,
            Message = "Mock user not found"
        });
    }

    public Task<AuthResponse> ToggleUserVerificationAsync(string id)
    {
        _logger.LogInformation("Mock admin service: Toggling user {UserId} verification", id);
        
        var user = _mockUsers.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            user.IsVerified = !user.IsVerified;
            return Task.FromResult(new AuthResponse
            {
                IsSuccess = true,
                Message = $"Mock user verification toggled to {user.IsVerified}"
            });
        }
        
        return Task.FromResult(new AuthResponse
        {
            IsSuccess = false,
            Message = "Mock user not found"
        });
    }

    public Task<Jackson.Ideas.Core.DTOs.Auth.AdminStatsResponse?> GetStatsAsync()
    {
        _logger.LogInformation("Mock admin service: Getting admin stats");
        
        var stats = new Jackson.Ideas.Core.DTOs.Auth.AdminStatsResponse
        {
            TotalUsers = _mockUsers.Count,
            VerifiedUsers = _mockUsers.Count(u => u.IsVerified)
        };
        
        return Task.FromResult<Jackson.Ideas.Core.DTOs.Auth.AdminStatsResponse?>(stats);
    }
}