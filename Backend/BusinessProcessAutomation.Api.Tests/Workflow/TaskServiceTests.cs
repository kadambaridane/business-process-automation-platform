using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;
using BusinessProcessAutomation.Api.Services;
using BusinessProcessAutomation.Api.SystemTasks;
using Xunit;

namespace BusinessProcessAutomation.Api.Tests.Workflow;

public class TaskServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private record Fixture(AppDbContext Db, ITaskService TaskService, int TaskId, int ManagerUserId, int EmployeeUserId, int WorkflowInstanceId);

    private static async Task<Fixture> SeedAsync()
    {
        var db = CreateDbContext();
        var notificationService = new NotificationService(db);
        var engine = new WorkflowEngineService(db, notificationService, new ISystemTaskHandler[] { new UpdateStatusSystemTaskHandler() });
        var taskService = new TaskService(db, engine, notificationService);

        var managerRole = new Role { Name = "Manager" };
        var employeeRole = new Role { Name = "Employee" };
        db.Roles.AddRange(managerRole, employeeRole);

        var requester = new User { FullName = "Requester", Email = "req@test.com", Username = "requester", PasswordHash = "x" };
        var manager = new User { FullName = "Manager One", Email = "mgr@test.com", Username = "manager1", PasswordHash = "x" };
        var employee = new User { FullName = "Employee One", Email = "emp@test.com", Username = "employee1", PasswordHash = "x" };
        db.Users.AddRange(requester, manager, employee);
        await db.SaveChangesAsync();

        db.UserRoles.Add(new UserRole { UserId = manager.Id, RoleId = managerRole.Id });
        db.UserRoles.Add(new UserRole { UserId = employee.Id, RoleId = employeeRole.Id });

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
            new WorkflowStep { WorkflowDefinitionId = workflowDefinition.Id, Name = "Complete", StepOrder = 2, StepType = WorkflowStepType.End }
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

        await engine.StartWorkflowAsync(submission.Id);

        var task = await db.Tasks.SingleAsync();
        var instance = await db.WorkflowInstances.SingleAsync();

        return new Fixture(db, taskService, task.Id, manager.Id, employee.Id, instance.Id);
    }

    [Fact]
    public async Task ActAsync_ApproveByAssignedRoleUser_AdvancesWorkflowToCompleted()
    {
        var fixture = await SeedAsync();

        var result = await fixture.TaskService.ActAsync(fixture.TaskId, TaskActionType.Approve, "ok", fixture.ManagerUserId);

        Assert.Equal(WorkflowTaskStatus.Approved, result.Status);
        var instance = await fixture.Db.WorkflowInstances.SingleAsync();
        Assert.Equal(WorkflowInstanceStatus.Completed, instance.Status);
    }

    [Fact]
    public async Task ActAsync_RejectByAssignedRoleUser_TerminatesWorkflowAsRejected()
    {
        var fixture = await SeedAsync();

        var result = await fixture.TaskService.ActAsync(fixture.TaskId, TaskActionType.Reject, "no", fixture.ManagerUserId);

        Assert.Equal(WorkflowTaskStatus.Rejected, result.Status);
        var instance = await fixture.Db.WorkflowInstances.SingleAsync();
        Assert.Equal(WorkflowInstanceStatus.Rejected, instance.Status);
    }

    [Fact]
    public async Task ActAsync_ByUserWithoutAssignedRole_ThrowsUnauthorized()
    {
        var fixture = await SeedAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.TaskService.ActAsync(fixture.TaskId, TaskActionType.Approve, null, fixture.EmployeeUserId));
    }

    [Fact]
    public async Task ActAsync_CompleteOnApprovalStep_ThrowsArgumentException()
    {
        var fixture = await SeedAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            fixture.TaskService.ActAsync(fixture.TaskId, TaskActionType.Complete, null, fixture.ManagerUserId));
    }

    [Fact]
    public async Task ActAsync_OnAlreadyActionedTask_ThrowsInvalidOperationException()
    {
        var fixture = await SeedAsync();
        await fixture.TaskService.ActAsync(fixture.TaskId, TaskActionType.Approve, "ok", fixture.ManagerUserId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.TaskService.ActAsync(fixture.TaskId, TaskActionType.Approve, "again", fixture.ManagerUserId));
    }
}
