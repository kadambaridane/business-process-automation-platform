using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs.Workflows;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class WorkflowService : IWorkflowService
{
    private readonly AppDbContext _dbContext;

    public WorkflowService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WorkflowDefinitionSummaryDto>> GetByApplicationAsync(int applicationId)
    {
        return await _dbContext.WorkflowDefinitions
            .Where(w => w.ApplicationId == applicationId)
            .OrderByDescending(w => w.CreatedDate)
            .Select(w => new WorkflowDefinitionSummaryDto
            {
                Id = w.Id,
                Name = w.Name,
                IsActive = w.IsActive,
                StepCount = w.WorkflowSteps.Count,
                CreatedDate = w.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<WorkflowDefinitionDto> GetByIdAsync(int workflowId)
    {
        var workflow = await FindWorkflowOrThrowAsync(workflowId);
        return MapWorkflow(workflow);
    }

    public async Task<WorkflowDefinitionDto> CreateAsync(int applicationId, SaveWorkflowDefinitionDto request)
    {
        var applicationExists = await _dbContext.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
        {
            throw new KeyNotFoundException($"Application with id {applicationId} was not found.");
        }

        var workflow = new WorkflowDefinition { ApplicationId = applicationId, Name = request.Name, IsActive = true };
        _dbContext.WorkflowDefinitions.Add(workflow);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(workflow.Id);
    }

    public async Task<WorkflowDefinitionDto> UpdateAsync(int workflowId, SaveWorkflowDefinitionDto request)
    {
        var workflow = await FindWorkflowOrThrowAsync(workflowId);
        workflow.Name = request.Name;
        await _dbContext.SaveChangesAsync();
        return MapWorkflow(workflow);
    }

    public async Task<WorkflowDefinitionDto> SetStatusAsync(int workflowId, bool isActive)
    {
        var workflow = await FindWorkflowOrThrowAsync(workflowId);
        workflow.IsActive = isActive;
        await _dbContext.SaveChangesAsync();
        return MapWorkflow(workflow);
    }

    public async Task<WorkflowDefinitionDto> AddStepAsync(int workflowId, SaveWorkflowStepDto request)
    {
        var workflow = await FindWorkflowOrThrowAsync(workflowId);
        await ValidateStepRulesAsync(request);

        var nextOrder = workflow.WorkflowSteps.Count == 0 ? 1 : workflow.WorkflowSteps.Max(s => s.StepOrder) + 1;

        var step = new WorkflowStep
        {
            WorkflowDefinitionId = workflowId,
            Name = request.Name,
            StepType = request.StepType,
            StepOrder = request.StepOrder ?? nextOrder,
            AssigneeRoleId = request.AssigneeRoleId,
            SystemActionKey = request.SystemActionKey
        };

        _dbContext.WorkflowSteps.Add(step);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(workflowId);
    }

    public async Task<WorkflowDefinitionDto> UpdateStepAsync(int stepId, SaveWorkflowStepDto request)
    {
        var step = await _dbContext.WorkflowSteps.FirstOrDefaultAsync(s => s.Id == stepId)
            ?? throw new KeyNotFoundException($"Workflow step with id {stepId} was not found.");

        await ValidateStepRulesAsync(request);

        step.Name = request.Name;
        step.StepType = request.StepType;
        if (request.StepOrder.HasValue)
        {
            step.StepOrder = request.StepOrder.Value;
        }
        step.AssigneeRoleId = request.AssigneeRoleId;
        step.SystemActionKey = request.SystemActionKey;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(step.WorkflowDefinitionId);
    }

    public async Task<WorkflowDefinitionDto> DeleteStepAsync(int stepId)
    {
        var step = await _dbContext.WorkflowSteps.FirstOrDefaultAsync(s => s.Id == stepId)
            ?? throw new KeyNotFoundException($"Workflow step with id {stepId} was not found.");

        var workflowId = step.WorkflowDefinitionId;
        _dbContext.WorkflowSteps.Remove(step);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(workflowId);
    }

    public async Task<WorkflowDefinitionDto> ReorderStepsAsync(int workflowId, ReorderStepsDto request)
    {
        var workflow = await FindWorkflowOrThrowAsync(workflowId);
        var stepsById = workflow.WorkflowSteps.ToDictionary(s => s.Id);

        if (request.StepIdsInOrder.Count != stepsById.Count || request.StepIdsInOrder.Any(id => !stepsById.ContainsKey(id)))
        {
            throw new ArgumentException("The reorder list must contain exactly the step ids belonging to this workflow.");
        }

        for (var i = 0; i < request.StepIdsInOrder.Count; i++)
        {
            stepsById[request.StepIdsInOrder[i]].StepOrder = i + 1;
        }

        await _dbContext.SaveChangesAsync();
        return MapWorkflow(workflow);
    }

    private async Task ValidateStepRulesAsync(SaveWorkflowStepDto request)
    {
        if (request.StepType is WorkflowStepType.HumanTask or WorkflowStepType.Approval)
        {
            if (request.AssigneeRoleId is null)
            {
                throw new ArgumentException($"A {request.StepType} step requires an assignee role.");
            }

            var roleExists = await _dbContext.Roles.AnyAsync(r => r.Id == request.AssigneeRoleId);
            if (!roleExists)
            {
                throw new KeyNotFoundException($"Role with id {request.AssigneeRoleId} was not found.");
            }
        }

        if (request.StepType == WorkflowStepType.SystemTask && string.IsNullOrWhiteSpace(request.SystemActionKey))
        {
            throw new ArgumentException("A System Task step requires a system action key.");
        }
    }

    private async Task<WorkflowDefinition> FindWorkflowOrThrowAsync(int workflowId)
    {
        var workflow = await _dbContext.WorkflowDefinitions
            .Include(w => w.WorkflowSteps)
                .ThenInclude(s => s.AssigneeRole)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        return workflow ?? throw new KeyNotFoundException($"Workflow with id {workflowId} was not found.");
    }

    private static WorkflowDefinitionDto MapWorkflow(WorkflowDefinition workflow) => new()
    {
        Id = workflow.Id,
        Name = workflow.Name,
        IsActive = workflow.IsActive,
        ApplicationId = workflow.ApplicationId,
        CreatedDate = workflow.CreatedDate,
        Steps = workflow.WorkflowSteps
            .OrderBy(s => s.StepOrder)
            .Select(s => new WorkflowStepDto
            {
                Id = s.Id,
                Name = s.Name,
                StepOrder = s.StepOrder,
                StepType = s.StepType,
                AssigneeRoleId = s.AssigneeRoleId,
                AssigneeRoleName = s.AssigneeRole?.Name,
                SystemActionKey = s.SystemActionKey
            })
            .ToList()
    };
}
