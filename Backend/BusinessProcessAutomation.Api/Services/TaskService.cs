using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Tasks;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly IWorkflowEngineService _workflowEngine;
    private readonly INotificationService _notificationService;

    public TaskService(AppDbContext dbContext, IWorkflowEngineService workflowEngine, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _workflowEngine = workflowEngine;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<TaskDto>> GetMyTasksAsync(int userId, int page, int pageSize)
    {
        var myRoleIds = await _dbContext.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();

        var query = TaskQuery().Where(t =>
            t.Status == WorkflowTaskStatus.Pending &&
            (t.AssignedToUserId == userId || (t.AssignedRoleId != null && myRoleIds.Contains(t.AssignedRoleId.Value))));

        return await PageAsync(query, page, pageSize);
    }

    public async Task<PagedResult<TaskDto>> GetPendingAsync(int page, int pageSize)
    {
        var query = TaskQuery().Where(t => t.Status == WorkflowTaskStatus.Pending);
        return await PageAsync(query, page, pageSize);
    }

    public async Task<PagedResult<TaskDto>> GetCompletedAsync(int userId, int page, int pageSize)
    {
        var completedTaskIds = await _dbContext.TaskHistories
            .Where(h => h.ActionByUserId == userId)
            .Select(h => h.TaskId)
            .Distinct()
            .ToListAsync();

        var query = TaskQuery().Where(t => completedTaskIds.Contains(t.Id));
        return await PageAsync(query, page, pageSize);
    }

    public async Task<TaskDetailDto> GetByIdAsync(int taskId)
    {
        var task = await FindTaskOrThrowAsync(taskId);
        return MapDetail(task);
    }

    public async Task<TaskDetailDto> ActAsync(int taskId, TaskActionType action, string? comments, int actingUserId)
    {
        var task = await FindTaskOrThrowAsync(taskId);
        await EnsureCanActAsync(task, actingUserId);
        EnsureActionMatchesStepType(task.WorkflowStepInstance.WorkflowStep.StepType, action);

        var actingUser = await _dbContext.Users.FirstAsync(u => u.Id == actingUserId);

        task.Status = action switch
        {
            TaskActionType.Approve => WorkflowTaskStatus.Approved,
            TaskActionType.Reject => WorkflowTaskStatus.Rejected,
            TaskActionType.Complete => WorkflowTaskStatus.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
        task.Comments = comments;
        task.CompletedDate = DateTime.UtcNow;

        task.WorkflowStepInstance.Status = WorkflowStepInstanceStatus.Completed;
        task.WorkflowStepInstance.ExitedDate = DateTime.UtcNow;
        task.WorkflowStepInstance.Outcome = task.Status.ToString();

        _dbContext.TaskHistories.Add(new TaskHistory
        {
            TaskId = task.Id,
            ActionByUserId = actingUserId,
            Action = action.ToString(),
            Comments = comments
        });

        await _dbContext.SaveChangesAsync();

        await _notificationService.CreateAsync(
            task.WorkflowInstance.SubmittedByUserId,
            "TaskActioned",
            $"'{task.Title}' was {task.Status.ToString().ToLowerInvariant()} by {actingUser.Username}.");

        if (action == TaskActionType.Reject)
        {
            await _workflowEngine.TerminateAsync(task.WorkflowInstanceId, WorkflowInstanceStatus.Rejected);
        }
        else
        {
            await _workflowEngine.AdvanceAsync(task.WorkflowInstanceId);
        }

        return await GetByIdAsync(taskId);
    }

    private IQueryable<WorkflowTask> TaskQuery() => _dbContext.Tasks
        .Include(t => t.WorkflowInstance).ThenInclude(i => i.Application)
        .Include(t => t.WorkflowStepInstance).ThenInclude(si => si.WorkflowStep)
        .Include(t => t.AssignedRole)
        .Include(t => t.AssignedToUser);

    private async Task<WorkflowTask> FindTaskOrThrowAsync(int taskId)
    {
        var task = await TaskQuery()
            .Include(t => t.WorkflowInstance).ThenInclude(i => i.FormSubmission)
            .Include(t => t.TaskHistories).ThenInclude(h => h.ActionByUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        return task ?? throw new KeyNotFoundException($"Task with id {taskId} was not found.");
    }

    private async Task EnsureCanActAsync(WorkflowTask task, int userId)
    {
        if (task.Status != WorkflowTaskStatus.Pending)
        {
            throw new InvalidOperationException("This task has already been actioned.");
        }

        var isAdmin = await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Admin");
        if (isAdmin || task.AssignedToUserId == userId)
        {
            return;
        }

        if (task.AssignedRoleId is not null &&
            await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == task.AssignedRoleId))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not authorized to act on this task.");
    }

    private static void EnsureActionMatchesStepType(WorkflowStepType stepType, TaskActionType action)
    {
        if (stepType == WorkflowStepType.Approval && action == TaskActionType.Complete)
        {
            throw new ArgumentException("Approval steps must be approved or rejected, not completed.");
        }

        if (stepType == WorkflowStepType.HumanTask && action != TaskActionType.Complete)
        {
            throw new ArgumentException("Human Task steps must be completed, not approved or rejected.");
        }
    }

    private static async Task<PagedResult<TaskDto>> PageAsync(IQueryable<WorkflowTask> query, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TaskDto>
        {
            Items = items.Select(MapDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static TaskDto MapDto(WorkflowTask task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        WorkflowInstanceId = task.WorkflowInstanceId,
        ApplicationId = task.WorkflowInstance.ApplicationId,
        ApplicationName = task.WorkflowInstance.Application.Name,
        StepType = task.WorkflowStepInstance.WorkflowStep.StepType,
        AssignedRoleName = task.AssignedRole?.Name,
        AssignedToUserName = task.AssignedToUser?.Username,
        Status = task.Status,
        CreatedDate = task.CreatedDate,
        CompletedDate = task.CompletedDate
    };

    private static TaskDetailDto MapDetail(WorkflowTask task)
    {
        var dto = MapDto(task);
        return new TaskDetailDto
        {
            Id = dto.Id,
            Title = dto.Title,
            WorkflowInstanceId = dto.WorkflowInstanceId,
            ApplicationId = dto.ApplicationId,
            ApplicationName = dto.ApplicationName,
            StepType = dto.StepType,
            AssignedRoleName = dto.AssignedRoleName,
            AssignedToUserName = dto.AssignedToUserName,
            Status = dto.Status,
            CreatedDate = dto.CreatedDate,
            CompletedDate = dto.CompletedDate,
            Comments = task.Comments,
            SubmissionData = task.WorkflowInstance.FormSubmission.SubmissionData,
            History = task.TaskHistories
                .OrderBy(h => h.ActionDate)
                .Select(h => new TaskHistoryDto
                {
                    Id = h.Id,
                    Action = h.Action,
                    ActionByUserName = h.ActionByUser.Username,
                    Comments = h.Comments,
                    ActionDate = h.ActionDate
                })
                .ToList()
        };
    }
}
