using Jackson.Ideas.Mock.Models;

namespace Jackson.Ideas.Mock.Services.Interfaces;

public interface IMockAuthenticationService
{
    Task<MockUser?> LoginAsync(string email, string password);
    Task<MockUser?> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync(string userId);
    Task<MockUser?> GetUserByIdAsync(string userId);
    Task<MockUser?> GetUserByEmailAsync(string email);
    Task<MockUser?> GetUserByTokenAsync(string token);
    Task<string> GenerateTokenAsync(string userId);
    Task<bool> ResetPasswordAsync(string email);
    Task<bool> VerifyPasswordResetTokenAsync(string token);
    Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task<List<MockUser>> GetAllUsersAsync();
    Task<bool> UpdateUserAsync(MockUser user);
    Task<bool> DeactivateUserAsync(string userId);
    Task<UserSession?> GetUserSessionAsync(string token);
    Task InvalidateAllUserTokensAsync(string userId);
}