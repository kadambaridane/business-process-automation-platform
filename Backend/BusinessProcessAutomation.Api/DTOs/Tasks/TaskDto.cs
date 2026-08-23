using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Tasks;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int WorkflowInstanceId { get; set; }
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public WorkflowStepType StepType { get; set; }
    public string? AssignedRoleName { get; set; }
    public string? AssignedToUserName { get; set; }
    public WorkflowTaskStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}
