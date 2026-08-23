namespace BusinessProcessAutomation.Api.DTOs.Workflows;

public class WorkflowDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ApplicationId { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowDefinitionSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StepCount { get; set; }
    public DateTime CreatedDate { get; set; }
}
