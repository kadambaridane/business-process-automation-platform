using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Workflows;

public class WorkflowInstanceDto
{
    public int Id { get; set; }
    public int WorkflowDefinitionId { get; set; }
    public string WorkflowDefinitionName { get; set; } = string.Empty;
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public int FormSubmissionId { get; set; }
    public string SubmittedByUserName { get; set; } = string.Empty;
    public int? CurrentStepId { get; set; }
    public string? CurrentStepName { get; set; }
    public WorkflowInstanceStatus Status { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class WorkflowStepInstanceDto
{
    public int Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public WorkflowStepType StepType { get; set; }
    public WorkflowStepInstanceStatus Status { get; set; }
    public string? Outcome { get; set; }
    public DateTime EnteredDate { get; set; }
    public DateTime? ExitedDate { get; set; }
}
