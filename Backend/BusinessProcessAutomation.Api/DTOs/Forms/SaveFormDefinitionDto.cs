using System.ComponentModel.DataAnnotations;

namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class SaveFormDefinitionDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
