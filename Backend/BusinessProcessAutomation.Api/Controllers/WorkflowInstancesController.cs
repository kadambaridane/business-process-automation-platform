using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Workflows;
using BusinessProcessAutomation.Api.Interfaces;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Authorize]
public class WorkflowInstancesController : ControllerBase
{
    private readonly IWorkflowInstanceService _workflowInstanceService;

    public WorkflowInstancesController(IWorkflowInstanceService workflowInstanceService)
    {
        _workflowInstanceService = workflowInstanceService;
    }

    [HttpGet("api/workflow-instances/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workflowInstanceService.GetByIdAsync(id);
        return Ok(ApiResponse<WorkflowInstanceDto>.Ok(result));
    }

    [HttpGet("api/workflow-instances/{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var result = await _workflowInstanceService.GetHistoryAsync(id);
        return Ok(ApiResponse<List<WorkflowStepInstanceDto>>.Ok(result));
    }

    [HttpGet("api/applications/{applicationId:int}/workflow-instances")]
    public async Task<IActionResult> GetByApplication(int applicationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _workflowInstanceService.GetByApplicationAsync(applicationId, page, pageSize);
        return Ok(ApiResponse<PagedResult<WorkflowInstanceDto>>.Ok(result));
    }
}
