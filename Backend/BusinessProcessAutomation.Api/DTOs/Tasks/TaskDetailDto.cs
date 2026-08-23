namespace BusinessProcessAutomation.Api.DTOs.Tasks;

public class TaskDetailDto : TaskDto
{
    public string? Comments { get; set; }
    public string SubmissionData { get; set; } = "{}";
    public List<TaskHistoryDto> History { get; set; } = new();
}

public class TaskHistoryDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ActionByUserName { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
}
