namespace BusinessProcessAutomation.Api.Models;

public class WorkflowTask
{
    public int Id { get; set; }

    public int WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public int WorkflowStepInstanceId { get; set; }
    public WorkflowStepInstance WorkflowStepInstance { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public int? AssignedRoleId { get; set; }
    public Role? AssignedRole { get; set; }

    public WorkflowTaskStatus Status { get; set; } = WorkflowTaskStatus.Pending;
    public string? Comments { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }

    public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
}
