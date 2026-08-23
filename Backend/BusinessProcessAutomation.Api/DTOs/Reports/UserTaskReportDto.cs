namespace BusinessProcessAutomation.Api.DTOs.Reports;

public class UserTaskReportDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalActioned { get; set; }
}
