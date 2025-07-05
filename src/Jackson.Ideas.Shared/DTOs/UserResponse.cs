namespace Jackson.Ideas.Shared.DTOs;

public record UserResponse
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Picture { get; init; }
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsVerified { get; init; }
    public string AuthProvider { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public DateTime CreatedAt { get; init; }
}