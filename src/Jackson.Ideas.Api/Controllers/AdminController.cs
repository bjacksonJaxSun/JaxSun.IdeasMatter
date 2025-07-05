using Jackson.Ideas.Core.DTOs.Auth;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IApplicationUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IApplicationUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminController> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserInfo>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetActiveUsersAsync();
            var userInfos = users.Select(u => new UserInfo
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Name = u.Name,
                Role = u.Role.ToString(),
                IsVerified = u.IsVerified,
                Permissions = GetUserPermissions(u)
            });

            return Ok(userInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserInfo>> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? "",
                Name = user.Name,
                Role = user.Role.ToString(),
                IsVerified = user.IsVerified,
                Permissions = GetUserPermissions(user)
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    [HttpPut("users/{id}/role")]
    public async Task<ActionResult<AuthResponse>> UpdateUserRoleAsync(string id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == currentUserId)
            {
                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Cannot modify your own role"
                });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            if (!Enum.TryParse<UserRole>(request.Role, out var newRole))
            {
                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid role specified"
                });
            }

            user.Role = newRole;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User role updated: {UserId} to {Role} by {AdminId}", 
                id, newRole, currentUserId);

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = $"User role updated to {newRole}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role for ID: {UserId}", id);
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred while updating user role"
            });
        }
    }

    [HttpPost("users/{id}/toggle-verification")]
    public async Task<ActionResult<AuthResponse>> ToggleUserVerificationAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            user.IsVerified = !user.IsVerified;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user);

            var action = user.IsVerified ? "verified" : "unverified";
            _logger.LogInformation("User {Action}: {UserId} by {AdminId}", 
                action, id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = $"User {action} successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user verification for ID: {UserId}", id);
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred while updating user verification"
            });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStatsAsync()
    {
        try
        {
            var allUsers = await _userRepository.GetActiveUsersAsync();
            var usersByRole = allUsers.GroupBy(u => u.Role)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var stats = new AdminStatsResponse
            {
                TotalUsers = allUsers.Count(),
                VerifiedUsers = allUsers.Count(u => u.IsVerified),
                UnverifiedUsers = allUsers.Count(u => !u.IsVerified),
                UsersByRole = usersByRole,
                RecentRegistrations = allUsers
                    .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                    .Count()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin stats");
            return StatusCode(500, new { message = "An error occurred while retrieving stats" });
        }
    }

    private static string[] GetUserPermissions(ApplicationUser user)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<string[]>(user.Permissions) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}

// Admin-specific DTOs
public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class AdminStatsResponse
{
    public int TotalUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int UnverifiedUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public int RecentRegistrations { get; set; }
}