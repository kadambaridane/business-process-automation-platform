using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class WorkflowEngineService : IWorkflowEngineService
{
    private const int MaxStepIterations = 100;

    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IEnumerable<ISystemTaskHandler> _systemTaskHandlers;

    public WorkflowEngineService(AppDbContext dbContext, INotificationService notificationService, IEnumerable<ISystemTaskHandler> systemTaskHandlers)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _systemTaskHandlers = systemTaskHandlers;
    }

    public async Task StartWorkflowAsync(int formSubmissionId)
    {
        var submission = await _dbContext.FormSubmissions.FirstOrDefaultAsync(s => s.Id == formSubmissionId)
            ?? throw new KeyNotFoundException($"Form submission with id {formSubmissionId} was not found.");

        var activeWorkflow = await _dbContext.WorkflowDefinitions
            .Where(w => w.ApplicationId == submission.ApplicationId && w.IsActive)
            .OrderByDescending(w => w.CreatedDate)
            .FirstOrDefaultAsync();

        if (activeWorkflow is null)
        {
            return;
        }

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = activeWorkflow.Id,
            ApplicationId = submission.ApplicationId,
            FormSubmissionId = submission.Id,
            SubmittedByUserId = submission.SubmittedByUserId,
            Status = WorkflowInstanceStatus.InProgress
        };

        _dbContext.WorkflowInstances.Add(instance);
        await _dbContext.SaveChangesAsync();

        await AdvanceAsync(instance.Id);
    }

    public async Task AdvanceAsync(int workflowInstanceId)
    {
        var instance = await _dbContext.WorkflowInstances
            .Include(i => i.WorkflowDefinition)
                .ThenInclude(d => d.WorkflowSteps)
            .Include(i => i.Application)
            .FirstOrDefaultAsync(i => i.Id == workflowInstanceId)
            ?? throw new KeyNotFoundException($"Workflow instance with id {workflowInstanceId} was not found.");

        if (instance.Status != WorkflowInstanceStatus.InProgress)
        {
            return;
        }

        var steps = instance.WorkflowDefinition.WorkflowSteps.OrderBy(s => s.StepOrder).ToList();
        var currentIndex = instance.CurrentStepId is null ? -1 : steps.FindIndex(s => s.Id == instance.CurrentStepId);
        var nextIndex = currentIndex + 1;
        var iterations = 0;

        while (true)
        {
            if (++iterations > MaxStepIterations)
            {
                throw new InvalidOperationException("Workflow exceeded the maximum number of step transitions; check the workflow definition for misconfiguration.");
            }

            if (nextIndex >= steps.Count)
            {
                await CompleteInstanceAsync(instance, WorkflowInstanceStatus.Completed);
                return;
            }

            var step = steps[nextIndex];
            instance.CurrentStepId = step.Id;

            var stepInstance = new WorkflowStepInstance
            {
                WorkflowInstanceId = instance.Id,
                WorkflowStepId = step.Id,
                Status = WorkflowStepInstanceStatus.InProgress
            };
            _dbContext.WorkflowStepInstances.Add(stepInstance);
            await _dbContext.SaveChangesAsync();

            if (step.StepType is WorkflowStepType.HumanTask or WorkflowStepType.Approval)
            {
                await CreateHumanTaskAsync(instance, step, stepInstance);
                await _dbContext.SaveChangesAsync();
                return;
            }

            if (step.StepType == WorkflowStepType.End)
            {
                stepInstance.Status = WorkflowStepInstanceStatus.Completed;
                stepInstance.ExitedDate = DateTime.UtcNow;
                stepInstance.Outcome = "End";
                await _dbContext.SaveChangesAsync();
                await CompleteInstanceAsync(instance, WorkflowInstanceStatus.Completed);
                return;
            }

            stepInstance.Outcome = step.StepType switch
            {
                WorkflowStepType.SystemTask => await ExecuteSystemTaskAsync(instance, step),
                WorkflowStepType.Notification => await ExecuteNotificationAsync(instance, step),
                WorkflowStepType.Decision => "Auto-passed (no branch conditions configured).",
                _ => "Skipped"
            };
            stepInstance.Status = WorkflowStepInstanceStatus.Completed;
            stepInstance.ExitedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            nextIndex++;
        }
    }

    public async Task TerminateAsync(int workflowInstanceId, WorkflowInstanceStatus finalStatus)
    {
        var instance = await _dbContext.WorkflowInstances
            .Include(i => i.Application)
            .FirstOrDefaultAsync(i => i.Id == workflowInstanceId)
            ?? throw new KeyNotFoundException($"Workflow instance with id {workflowInstanceId} was not found.");

        await CompleteInstanceAsync(instance, finalStatus);
    }

    private async Task CreateHumanTaskAsync(WorkflowInstance instance, WorkflowStep step, WorkflowStepInstance stepInstance)
    {
        var task = new WorkflowTask
        {
            WorkflowInstanceId = instance.Id,
            WorkflowStepInstanceId = stepInstance.Id,
            Title = step.Name,
            AssignedRoleId = step.AssigneeRoleId,
            Status = WorkflowTaskStatus.Pending
        };
        _dbContext.Tasks.Add(task);

        if (step.AssigneeRoleId is not null)
        {
            await _notificationService.CreateForRoleAsync(step.AssigneeRoleId.Value, "TaskAssigned", $"New task '{step.Name}' assigned for {instance.Application.Name}.");
        }
    }

    private async Task<string> ExecuteSystemTaskAsync(WorkflowInstance instance, WorkflowStep step)
    {
        var handler = _systemTaskHandlers.FirstOrDefault(h => h.Key == step.SystemActionKey);
        return handler is null
            ? $"No system task handler registered for key '{step.SystemActionKey}'; step skipped."
            : await handler.ExecuteAsync(instance, step);
    }

    private async Task<string> ExecuteNotificationAsync(WorkflowInstance instance, WorkflowStep step)
    {
        await _notificationService.CreateAsync(instance.SubmittedByUserId, "WorkflowProgress", $"Your '{instance.Application.Name}' request has reached step '{step.Name}'.");
        return "Notification sent.";
    }

    private async Task CompleteInstanceAsync(WorkflowInstance instance, WorkflowInstanceStatus finalStatus)
    {
        instance.Status = finalStatus;
        instance.CompletedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var message = finalStatus switch
        {
            WorkflowInstanceStatus.Rejected => $"Your '{instance.Application.Name}' request was rejected.",
            _ => $"Your '{instance.Application.Name}' request has been completed."
        };
        await _notificationService.CreateAsync(instance.SubmittedByUserId, "WorkflowCompleted", message);
    }
}
