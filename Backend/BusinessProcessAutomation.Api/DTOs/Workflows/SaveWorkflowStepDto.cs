using System.ComponentModel.DataAnnotations;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Workflows;

public class SaveWorkflowStepDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public WorkflowStepType StepType { get; set; }
    public int? StepOrder { get; set; }
    public int? AssigneeRoleId { get; set; }
    public string? SystemActionKey { get; set; }
}
