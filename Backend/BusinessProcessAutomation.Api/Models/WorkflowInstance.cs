namespace BusinessProcessAutomation.Api.Models;

public class WorkflowInstance
{
    public int Id { get; set; }

    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    public int ApplicationId { get; set; }
    public BusinessApplication Application { get; set; } = null!;

    public int FormSubmissionId { get; set; }
    public FormSubmission FormSubmission { get; set; } = null!;

    public int SubmittedByUserId { get; set; }
    public User SubmittedByUser { get; set; } = null!;

    public int? CurrentStepId { get; set; }
    public WorkflowStep? CurrentStep { get; set; }

    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.InProgress;

    public DateTime StartedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }

    public ICollection<WorkflowStepInstance> WorkflowStepInstances { get; set; } = new List<WorkflowStepInstance>();
    public ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
}
