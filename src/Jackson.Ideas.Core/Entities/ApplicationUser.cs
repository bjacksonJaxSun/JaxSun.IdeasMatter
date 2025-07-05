using Microsoft.AspNetCore.Identity;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    
    public string? Picture { get; set; }
    
    public UserRole Role { get; set; } = UserRole.User;
    
    public string? TenantId { get; set; }
    
    public string Permissions { get; set; } = "[]"; // JSON array
    
    public bool IsActive { get; set; } = true;
    
    public bool IsVerified { get; set; } = false;
    
    public string AuthProvider { get; set; } = "local";
    
    public DateTime? LastLogin { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public override string? PasswordHash { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Research> Researches { get; set; } = new List<Research>();
    
    public virtual ICollection<AIProviderConfig> AIProviderConfigs { get; set; } = new List<AIProviderConfig>();
    
    public virtual ICollection<ResearchSession> ResearchSessions { get; set; } = new List<ResearchSession>();
}