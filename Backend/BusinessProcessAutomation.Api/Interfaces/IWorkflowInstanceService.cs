using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Workflows;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IWorkflowInstanceService
{
    Task<WorkflowInstanceDto> GetByIdAsync(int instanceId);
    Task<List<WorkflowStepInstanceDto>> GetHistoryAsync(int instanceId);
    Task<PagedResult<WorkflowInstanceDto>> GetByApplicationAsync(int applicationId, int page, int pageSize);
}
