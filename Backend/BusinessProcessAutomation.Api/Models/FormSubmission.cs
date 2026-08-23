namespace BusinessProcessAutomation.Api.Models;

public class FormSubmission
{
    public int Id { get; set; }

    public int FormDefinitionId { get; set; }
    public FormDefinition FormDefinition { get; set; } = null!;

    public int ApplicationId { get; set; }
    public BusinessApplication Application { get; set; } = null!;

    public int SubmittedByUserId { get; set; }
    public User SubmittedByUser { get; set; } = null!;

    /// <summary>JSON object of fieldName -&gt; submitted value, since the form schema is dynamic per FormDefinition.</summary>
    public string SubmissionData { get; set; } = "{}";

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Inverse of WorkflowInstance.FormSubmissionId; the FK column lives on WorkflowInstance.</summary>
    public WorkflowInstance? WorkflowInstance { get; set; }
}
