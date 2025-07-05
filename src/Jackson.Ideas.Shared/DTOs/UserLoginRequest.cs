using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Shared.DTOs;

public record UserLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;
    
    public bool RememberMe { get; init; } = false;
}