namespace BusinessProcessAutomation.Api.Models;

public enum FieldType
{
    Text,
    Number,
    Email,
    Date,
    Dropdown,
    Radio,
    Checkbox,
    Textarea
}

public enum WorkflowStepType
{
    HumanTask,
    SystemTask,
    Approval,
    Decision,
    Notification,
    End
}

public enum WorkflowInstanceStatus
{
    InProgress,
    Approved,
    Rejected,
    Completed
}

public enum WorkflowStepInstanceStatus
{
    Pending,
    InProgress,
    Completed,
    Skipped
}

public enum WorkflowTaskStatus
{
    Pending,
    Approved,
    Rejected,
    Completed
}

public enum TaskActionType
{
    Approve,
    Reject,
    Complete
}
