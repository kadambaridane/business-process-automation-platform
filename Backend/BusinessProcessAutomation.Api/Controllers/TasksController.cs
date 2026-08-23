using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Tasks;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _taskService.GetMyTasksAsync(CurrentUserId, page, pageSize);
        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result));
    }

    [Authorize(Roles = "Admin,Business User")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _taskService.GetPendingAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result));
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompleted([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _taskService.GetCompletedAsync(CurrentUserId, page, pageSize);
        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _taskService.GetByIdAsync(id);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] TaskActionRequestDto request)
    {
        var result = await _taskService.ActAsync(id, TaskActionType.Approve, request.Comments, CurrentUserId);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result));
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] TaskActionRequestDto request)
    {
        var result = await _taskService.ActAsync(id, TaskActionType.Reject, request.Comments, CurrentUserId);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result));
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] TaskActionRequestDto request)
    {
        var result = await _taskService.ActAsync(id, TaskActionType.Complete, request.Comments, CurrentUserId);
        return Ok(ApiResponse<TaskDetailDto>.Ok(result));
    }
}
