using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Workflows;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class WorkflowInstanceService : IWorkflowInstanceService
{
    private readonly AppDbContext _dbContext;

    public WorkflowInstanceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkflowInstanceDto> GetByIdAsync(int instanceId)
    {
        var instance = await FindInstanceOrThrowAsync(instanceId);
        return MapInstance(instance);
    }

    public async Task<List<WorkflowStepInstanceDto>> GetHistoryAsync(int instanceId)
    {
        var exists = await _dbContext.WorkflowInstances.AnyAsync(i => i.Id == instanceId);
        if (!exists)
        {
            throw new KeyNotFoundException($"Workflow instance with id {instanceId} was not found.");
        }

        return await _dbContext.WorkflowStepInstances
            .Include(si => si.WorkflowStep)
            .Where(si => si.WorkflowInstanceId == instanceId)
            .OrderBy(si => si.EnteredDate)
            .Select(si => new WorkflowStepInstanceDto
            {
                Id = si.Id,
                StepName = si.WorkflowStep.Name,
                StepType = si.WorkflowStep.StepType,
                Status = si.Status,
                Outcome = si.Outcome,
                EnteredDate = si.EnteredDate,
                ExitedDate = si.ExitedDate
            })
            .ToListAsync();
    }

    public async Task<PagedResult<WorkflowInstanceDto>> GetByApplicationAsync(int applicationId, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = InstanceQuery().Where(i => i.ApplicationId == applicationId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.StartedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WorkflowInstanceDto>
        {
            Items = items.Select(MapInstance).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private IQueryable<WorkflowInstance> InstanceQuery() => _dbContext.WorkflowInstances
        .Include(i => i.WorkflowDefinition)
        .Include(i => i.Application)
        .Include(i => i.SubmittedByUser)
        .Include(i => i.CurrentStep);

    private async Task<WorkflowInstance> FindInstanceOrThrowAsync(int instanceId)
    {
        var instance = await InstanceQuery().FirstOrDefaultAsync(i => i.Id == instanceId);
        return instance ?? throw new KeyNotFoundException($"Workflow instance with id {instanceId} was not found.");
    }

    private static WorkflowInstanceDto MapInstance(WorkflowInstance instance) => new()
    {
        Id = instance.Id,
        WorkflowDefinitionId = instance.WorkflowDefinitionId,
        WorkflowDefinitionName = instance.WorkflowDefinition.Name,
        ApplicationId = instance.ApplicationId,
        ApplicationName = instance.Application.Name,
        FormSubmissionId = instance.FormSubmissionId,
        SubmittedByUserName = instance.SubmittedByUser.Username,
        CurrentStepId = instance.CurrentStepId,
        CurrentStepName = instance.CurrentStep?.Name,
        Status = instance.Status,
        StartedDate = instance.StartedDate,
        CompletedDate = instance.CompletedDate
    };
}
