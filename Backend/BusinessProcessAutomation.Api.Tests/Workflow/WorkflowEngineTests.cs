using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;
using BusinessProcessAutomation.Api.Services;
using BusinessProcessAutomation.Api.SystemTasks;
using Xunit;

namespace BusinessProcessAutomation.Api.Tests.Workflow;

public class WorkflowEngineTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IWorkflowEngineService CreateEngine(AppDbContext dbContext)
    {
        var notificationService = new NotificationService(dbContext);
        var handlers = new ISystemTaskHandler[]
        {
            new UpdateStatusSystemTaskHandler(),
            new SendNotificationSystemTaskHandler(notificationService)
        };
        return new WorkflowEngineService(dbContext, notificationService, handlers);
    }

    /// <summary>Seeds Role/User/Application/Form/4-step WorkflowDefinition (Approval, SystemTask, Notification, End) and one FormSubmission.</summary>
    private static async Task<(AppDbContext db, int submissionId, int managerRoleId)> SeedAsync()
    {
        var db = CreateDbContext();

        var managerRole = new Role { Name = "Manager" };
        var employeeRole = new Role { Name = "Employee" };
        db.Roles.AddRange(managerRole, employeeRole);

        var requester = new User { FullName = "Requester", Email = "req@test.com", Username = "requester", PasswordHash = "x" };
        var manager = new User { FullName = "Manager One", Email = "mgr@test.com", Username = "manager1", PasswordHash = "x" };
        db.Users.AddRange(requester, manager);
        await db.SaveChangesAsync();

        db.UserRoles.Add(new UserRole { UserId = manager.Id, RoleId = managerRole.Id });

        var application = new BusinessApplication { Name = "Leave Request", CreatedByUserId = requester.Id };
        db.Applications.Add(application);
        await db.SaveChangesAsync();

        var formDefinition = new FormDefinition { ApplicationId = application.Id, Name = "Leave Form", IsActive = true };
        db.FormDefinitions.Add(formDefinition);
        await db.SaveChangesAsync();

        var workflowDefinition = new WorkflowDefinition { ApplicationId = application.Id, Name = "Leave Approval", IsActive = true };
        db.WorkflowDefinitions.Add(workflowDefinition);
        await db.SaveChangesAsync();

        db.WorkflowSteps.AddRange(
            new WorkflowStep { WorkflowDefinitionId = workflowDefinition.Id, Name = "Manager Approval", StepOrder = 1, StepType = WorkflowStepType.Approval, AssigneeRoleId = managerRole.Id },
            new WorkflowStep { WorkflowDefinitionId = workflowDefinition.Id, Name = "Update Status", StepOrder = 2, StepType = WorkflowStepType.SystemTask, SystemActionKey = "UpdateApplicationStatus" },
            new WorkflowStep { WorkflowDefinitionId = workflowDefinition.Id, Name = "Notify", StepOrder = 3, StepType = WorkflowStepType.Notification },
            new WorkflowStep { WorkflowDefinitionId = workflowDefinition.Id, Name = "Complete", StepOrder = 4, StepType = WorkflowStepType.End }
        );
        await db.SaveChangesAsync();

        var submission = new FormSubmission
        {
            FormDefinitionId = formDefinition.Id,
            ApplicationId = application.Id,
            SubmittedByUserId = requester.Id,
            SubmissionData = "{}"
        };
        db.FormSubmissions.Add(submission);
        await db.SaveChangesAsync();

        return (db, submission.Id, managerRole.Id);
    }

    [Fact]
    public async Task StartWorkflowAsync_PausesAtFirstHumanStep_AndCreatesPendingTask()
    {
        var (db, submissionId, managerRoleId) = await SeedAsync();
        var engine = CreateEngine(db);

        await engine.StartWorkflowAsync(submissionId);

        var instance = await db.WorkflowInstances.Include(i => i.CurrentStep).SingleAsync();
        Assert.Equal(WorkflowInstanceStatus.InProgress, instance.Status);
        Assert.Equal("Manager Approval", instance.CurrentStep!.Name);

        var task = await db.Tasks.SingleAsync();
        Assert.Equal(WorkflowTaskStatus.Pending, task.Status);
        Assert.Equal(managerRoleId, task.AssignedRoleId);
    }

    [Fact]
    public async Task AdvanceAsync_AfterApproval_RunsRemainingStepsAndCompletesInstance()
    {
        var (db, submissionId, _) = await SeedAsync();
        var engine = CreateEngine(db);
        await engine.StartWorkflowAsync(submissionId);

        var instance = await db.WorkflowInstances.SingleAsync();
        var task = await db.Tasks.SingleAsync();
        task.Status = WorkflowTaskStatus.Approved;
        var stepInstance = await db.WorkflowStepInstances.FirstAsync(si => si.Id == task.WorkflowStepInstanceId);
        stepInstance.Status = WorkflowStepInstanceStatus.Completed;
        await db.SaveChangesAsync();

        await engine.AdvanceAsync(instance.Id);

        var updated = await db.WorkflowInstances.Include(i => i.CurrentStep).SingleAsync();
        Assert.Equal(WorkflowInstanceStatus.Completed, updated.Status);
        Assert.Equal("Complete", updated.CurrentStep!.Name);
        Assert.NotNull(updated.CompletedDate);

        var stepHistory = await db.WorkflowStepInstances.Where(si => si.WorkflowInstanceId == instance.Id).ToListAsync();
        Assert.Equal(4, stepHistory.Count);
        Assert.All(stepHistory, si => Assert.Equal(WorkflowStepInstanceStatus.Completed, si.Status));
    }

    [Fact]
    public async Task TerminateAsync_SetsRejectedStatus_WithoutRunningFurtherSteps()
    {
        var (db, submissionId, _) = await SeedAsync();
        var engine = CreateEngine(db);
        await engine.StartWorkflowAsync(submissionId);

        var instance = await db.WorkflowInstances.SingleAsync();

        await engine.TerminateAsync(instance.Id, WorkflowInstanceStatus.Rejected);

        var updated = await db.WorkflowInstances.SingleAsync();
        Assert.Equal(WorkflowInstanceStatus.Rejected, updated.Status);
        Assert.NotNull(updated.CompletedDate);

        var stepHistory = await db.WorkflowStepInstances.Where(si => si.WorkflowInstanceId == instance.Id).ToListAsync();
        Assert.Single(stepHistory);
    }

    [Fact]
    public async Task StartWorkflowAsync_WithNoActiveWorkflow_DoesNotCreateInstance()
    {
        var (db, submissionId, _) = await SeedAsync();
        var workflowDefinition = await db.WorkflowDefinitions.SingleAsync();
        workflowDefinition.IsActive = false;
        await db.SaveChangesAsync();

        var engine = CreateEngine(db);
        await engine.StartWorkflowAsync(submissionId);

        Assert.False(await db.WorkflowInstances.AnyAsync());
    }
}
