using BusinessProcessAutomation.Api.DTOs.Workflows;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IWorkflowService
{
    Task<List<WorkflowDefinitionSummaryDto>> GetByApplicationAsync(int applicationId);
    Task<WorkflowDefinitionDto> GetByIdAsync(int workflowId);
    Task<WorkflowDefinitionDto> CreateAsync(int applicationId, SaveWorkflowDefinitionDto request);
    Task<WorkflowDefinitionDto> UpdateAsync(int workflowId, SaveWorkflowDefinitionDto request);
    Task<WorkflowDefinitionDto> SetStatusAsync(int workflowId, bool isActive);

    Task<WorkflowDefinitionDto> AddStepAsync(int workflowId, SaveWorkflowStepDto request);
    Task<WorkflowDefinitionDto> UpdateStepAsync(int stepId, SaveWorkflowStepDto request);
    Task<WorkflowDefinitionDto> DeleteStepAsync(int stepId);
    Task<WorkflowDefinitionDto> ReorderStepsAsync(int workflowId, ReorderStepsDto request);
}
