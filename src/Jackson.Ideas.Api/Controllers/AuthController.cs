using Jackson.Ideas.Core.DTOs.Auth;
using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;
using Jackson.Ideas.Core.Interfaces;
using Jackson.Ideas.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jackson.Ideas.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IApplicationUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        IApplicationUserRepository userRepository,
        IJwtService jwtService,
        ILogger<AuthController> logger,
        IPasswordHasher<ApplicationUser> passwordHasher,
        UserManager<ApplicationUser> userManager)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> RegisterAsync([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "User with this email already exists"
                });
            }

            // Create new user
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.Email, // Required for Identity
                Name = request.Name,
                Role = UserRole.User,
                AuthProvider = "local",
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Permissions = "[]"
            };

            // Use Identity UserManager to create user with proper password hashing
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"User creation failed: {errors}"
                });
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update user with refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = "Registration successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    IsVerified = user.IsVerified,
                    Permissions = Array.Empty<string>()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred during registration"
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                });
            }

            // Verify password using Identity password hasher
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                });
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update user with refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    IsVerified = user.IsVerified,
                    Permissions = GetUserPermissions(user)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Unauthorized(new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid or expired refresh token"
                });
            }

            // Generate new tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Update user with new refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = "Token refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    IsVerified = user.IsVerified,
                    Permissions = GetUserPermissions(user)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred during token refresh"
            });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult<AuthResponse>> LogoutAsync([FromBody] LogoutRequest request)
    {
        try
        {
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (user != null)
            {
                // Clear refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user);
            }

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = "Logged out successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred during logout"
            });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUserAsync()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? "",
                Name = user.Name,
                Role = user.Role.ToString(),
                IsVerified = user.IsVerified,
                Permissions = GetUserPermissions(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponse { IsSuccess = false, Message = "Invalid token" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new AuthResponse { IsSuccess = false, Message = "User not found" });
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new AuthResponse
                {
                    IsSuccess = false,
                    Message = $"Password change failed: {errors}"
                });
            }

            _logger.LogInformation("Password changed successfully for user: {Email}", user.Email);

            return Ok(new AuthResponse
            {
                IsSuccess = true,
                Message = "Password changed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new AuthResponse
            {
                IsSuccess = false,
                Message = "An error occurred while changing password"
            });
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

// Additional request DTOs
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}