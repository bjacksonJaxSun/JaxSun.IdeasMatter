namespace Jackson.Ideas.Shared.DTOs;

public record AuthTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public UserResponse User { get; init; } = null!;
}