using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IWorkflowEngineService
{
    /// <summary>Starts a workflow instance for a submission, if the application has an active workflow. No-op otherwise.</summary>
    Task StartWorkflowAsync(int formSubmissionId);

    /// <summary>Drives the instance forward from its current step until it pauses at a human step or reaches a terminal state.</summary>
    Task AdvanceAsync(int workflowInstanceId);

    /// <summary>Ends the instance immediately in a terminal state (used for rejection) without processing further steps.</summary>
    Task TerminateAsync(int workflowInstanceId, WorkflowInstanceStatus finalStatus);
}
