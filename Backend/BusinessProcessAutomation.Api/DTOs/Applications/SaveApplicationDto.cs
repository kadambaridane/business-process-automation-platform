using System.ComponentModel.DataAnnotations;

namespace BusinessProcessAutomation.Api.DTOs.Applications;

public class SaveApplicationDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
