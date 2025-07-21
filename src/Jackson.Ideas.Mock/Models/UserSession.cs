namespace Jackson.Ideas.Mock.Models;

public class UserSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "";
    public string Token { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public DateTime? LastActivity { get; set; }
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string DeviceInfo { get; set; } = "";
    public string SessionType { get; set; } = "Standard"; // Standard, Extended, API
}