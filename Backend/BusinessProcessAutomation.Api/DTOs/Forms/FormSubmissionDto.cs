namespace BusinessProcessAutomation.Api.DTOs.Forms;

public class FormSubmissionDto
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public string FormDefinitionName { get; set; } = string.Empty;
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string SubmittedByUserName { get; set; } = string.Empty;

    /// <summary>Raw JSON object of fieldName -&gt; value; the client parses it against the form's field definitions.</summary>
    public string SubmissionData { get; set; } = "{}";

    public DateTime SubmittedDate { get; set; }
    public int? WorkflowInstanceId { get; set; }
}
