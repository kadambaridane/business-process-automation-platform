using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Workflows;
using BusinessProcessAutomation.Api.Interfaces;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private const string CanManageRoles = "Admin,Business User";

    private readonly IWorkflowService _workflowService;

    public WorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("api/applications/{applicationId:int}/workflows")]
    public async Task<IActionResult> GetByApplication(int applicationId)
    {
        var result = await _workflowService.GetByApplicationAsync(applicationId);
        return Ok(ApiResponse<List<WorkflowDefinitionSummaryDto>>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost("api/applications/{applicationId:int}/workflows")]
    public async Task<IActionResult> Create(int applicationId, [FromBody] SaveWorkflowDefinitionDto request)
    {
        var result = await _workflowService.CreateAsync(applicationId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [HttpGet("api/workflows/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workflowService.GetByIdAsync(id);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPut("api/workflows/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveWorkflowDefinitionDto request)
    {
        var result = await _workflowService.UpdateAsync(id, request);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPatch("api/workflows/{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusDto request)
    {
        var result = await _workflowService.SetStatusAsync(id, request.IsActive);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost("api/workflows/{id:int}/steps")]
    public async Task<IActionResult> AddStep(int id, [FromBody] SaveWorkflowStepDto request)
    {
        var result = await _workflowService.AddStepAsync(id, request);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPut("api/workflows/steps/{stepId:int}")]
    public async Task<IActionResult> UpdateStep(int stepId, [FromBody] SaveWorkflowStepDto request)
    {
        var result = await _workflowService.UpdateStepAsync(stepId, request);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpDelete("api/workflows/steps/{stepId:int}")]
    public async Task<IActionResult> DeleteStep(int stepId)
    {
        var result = await _workflowService.DeleteStepAsync(stepId);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost("api/workflows/{id:int}/steps/reorder")]
    public async Task<IActionResult> ReorderSteps(int id, [FromBody] ReorderStepsDto request)
    {
        var result = await _workflowService.ReorderStepsAsync(id, request);
        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }
}
