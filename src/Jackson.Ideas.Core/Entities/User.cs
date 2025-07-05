using System.ComponentModel.DataAnnotations;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? HashedPassword { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Picture { get; set; }
    
    public UserRole Role { get; set; } = UserRole.User;
    
    [StringLength(100)]
    public string? TenantId { get; set; }
    
    public string Permissions { get; set; } = "[]"; // JSON array
    
    public bool IsActive { get; set; } = true;
    
    public bool IsVerified { get; set; } = false;
    
    [StringLength(50)]
    public string AuthProvider { get; set; } = "local";
    
    public DateTime? LastLogin { get; set; }
    
    // Navigation properties
    public virtual ICollection<Research> Researches { get; set; } = new List<Research>();
    
    public virtual ICollection<AIProviderConfig> AIProviderConfigs { get; set; } = new List<AIProviderConfig>();
}