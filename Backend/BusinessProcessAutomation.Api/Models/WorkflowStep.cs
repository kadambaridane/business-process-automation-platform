namespace BusinessProcessAutomation.Api.Models;

public class WorkflowStep
{
    public int Id { get; set; }

    public int WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public WorkflowStepType StepType { get; set; }

    public int? AssigneeRoleId { get; set; }
    public Role? AssigneeRole { get; set; }

    /// <summary>Registry key resolved to an ISystemTaskHandler at runtime, e.g. "UpdateStatus", "SendNotification".</summary>
    public string? SystemActionKey { get; set; }

    public ICollection<WorkflowStepInstance> WorkflowStepInstances { get; set; } = new List<WorkflowStepInstance>();
}
