using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.Research;

public class UpdateStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    
    public Dictionary<string, object>? Parameters { get; set; }
}