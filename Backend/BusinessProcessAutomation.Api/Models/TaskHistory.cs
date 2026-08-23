namespace BusinessProcessAutomation.Api.Models;

public class TaskHistory
{
    public int Id { get; set; }

    public int TaskId { get; set; }
    public WorkflowTask Task { get; set; } = null!;

    public int ActionByUserId { get; set; }
    public User ActionByUser { get; set; } = null!;

    public string Action { get; set; } = string.Empty;
    public string? Comments { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}
