using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.SystemTasks;

public class SendNotificationSystemTaskHandler : ISystemTaskHandler
{
    private readonly INotificationService _notificationService;

    public SendNotificationSystemTaskHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public string Key => "SendNotification";

    public async Task<string> ExecuteAsync(WorkflowInstance instance, WorkflowStep step)
    {
        await _notificationService.CreateAsync(instance.SubmittedByUserId, "WorkflowSystemNotification", $"Automated step '{step.Name}' ran for your request.");
        return "Notification sent.";
    }
}
