namespace BusinessProcessAutomation.Api.Models;

public class WorkflowDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int ApplicationId { get; set; }
    public BusinessApplication Application { get; set; } = null!;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<WorkflowStep> WorkflowSteps { get; set; } = new List<WorkflowStep>();
    public ICollection<WorkflowInstance> WorkflowInstances { get; set; } = new List<WorkflowInstance>();
}
