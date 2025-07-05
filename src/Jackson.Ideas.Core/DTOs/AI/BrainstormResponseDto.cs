using System.ComponentModel.DataAnnotations;

namespace Jackson.Ideas.Core.DTOs.AI;

public class BrainstormResponseDto
{
    [Required]
    public string Response { get; set; } = string.Empty;
    
    public List<InsightDto> Insights { get; set; } = new();
    
    public List<OptionDto> Options { get; set; } = new();
    
    public List<string> FollowUpQuestions { get; set; } = new();
    
    // Additional property for strategic options compatibility
    public IEnumerable<OptionDto> GeneratedOptions 
    { 
        get => Options; 
        set => Options = value.ToList(); 
    }
}