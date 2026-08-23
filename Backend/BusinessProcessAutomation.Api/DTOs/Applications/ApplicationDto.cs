namespace BusinessProcessAutomation.Api.DTOs.Applications;

public class ApplicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int FormDefinitionCount { get; set; }
    public int WorkflowDefinitionCount { get; set; }
}
