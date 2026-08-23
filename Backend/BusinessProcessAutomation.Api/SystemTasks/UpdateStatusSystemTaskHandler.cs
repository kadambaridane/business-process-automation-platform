using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.SystemTasks;

/// <summary>Marks progress on the workflow instance; a lightweight example of a system-driven step.</summary>
public class UpdateStatusSystemTaskHandler : ISystemTaskHandler
{
    public string Key => "UpdateApplicationStatus";

    public Task<string> ExecuteAsync(WorkflowInstance instance, WorkflowStep step)
    {
        return Task.FromResult($"Status updated at step '{step.Name}'.");
    }
}
