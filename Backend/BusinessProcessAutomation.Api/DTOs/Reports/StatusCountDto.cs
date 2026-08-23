using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.DTOs.Reports;

public class StatusCountDto
{
    public WorkflowInstanceStatus Status { get; set; }
    public int Count { get; set; }
}
