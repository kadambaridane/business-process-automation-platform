using System.ComponentModel.DataAnnotations;

namespace BusinessProcessAutomation.Api.DTOs.Workflows;

public class SaveWorkflowDefinitionDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
