namespace BusinessProcessAutomation.Api.Models;

public class BusinessApplication
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<FormDefinition> FormDefinitions { get; set; } = new List<FormDefinition>();
    public ICollection<WorkflowDefinition> WorkflowDefinitions { get; set; } = new List<WorkflowDefinition>();
    public ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    public ICollection<WorkflowInstance> WorkflowInstances { get; set; } = new List<WorkflowInstance>();
}
