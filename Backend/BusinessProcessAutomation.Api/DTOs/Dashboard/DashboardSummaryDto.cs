using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }

    public List<RecentApplicationDto> RecentApplications { get; set; } = new();
    public List<RecentWorkflowActivityDto> RecentWorkflowActivity { get; set; } = new();
}

public class RecentApplicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class RecentWorkflowActivityDto
{
    public int WorkflowInstanceId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string WorkflowDefinitionName { get; set; } = string.Empty;
    public string SubmittedByUserName { get; set; } = string.Empty;
    public WorkflowInstanceStatus Status { get; set; }
    public string? CurrentStepName { get; set; }
    public DateTime ActivityDate { get; set; }
}
