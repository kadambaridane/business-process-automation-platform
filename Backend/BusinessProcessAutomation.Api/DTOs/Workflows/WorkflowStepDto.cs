using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Workflows;

public class WorkflowStepDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public WorkflowStepType StepType { get; set; }
    public int? AssigneeRoleId { get; set; }
    public string? AssigneeRoleName { get; set; }
    public string? SystemActionKey { get; set; }
}
