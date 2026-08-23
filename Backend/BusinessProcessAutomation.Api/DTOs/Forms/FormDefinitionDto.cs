namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class FormDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ApplicationId { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new();
}

public class FormDefinitionSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int FieldCount { get; set; }
    public DateTime CreatedDate { get; set; }
}
