using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Reports;

public class ApplicationReportRowDto
{
    public int WorkflowInstanceId { get; set; }
    public int ApplicationId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string WorkflowDefinitionName { get; set; } = string.Empty;
    public string SubmittedByUserName { get; set; } = string.Empty;
    public WorkflowInstanceStatus Status { get; set; }
    public string? CurrentStepName { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    /// <summary>Null while still InProgress.</summary>
    public double? DurationHours { get; set; }
}
