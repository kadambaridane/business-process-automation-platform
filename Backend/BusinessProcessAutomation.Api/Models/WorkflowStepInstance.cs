namespace BusinessProcessAutomation.Api.Models;

public class WorkflowStepInstance
{
    public int Id { get; set; }

    public int WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public int WorkflowStepId { get; set; }
    public WorkflowStep WorkflowStep { get; set; } = null!;

    public WorkflowStepInstanceStatus Status { get; set; } = WorkflowStepInstanceStatus.Pending;

    public DateTime EnteredDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExitedDate { get; set; }

    public string? Outcome { get; set; }

    public WorkflowTask? Task { get; set; }
}
