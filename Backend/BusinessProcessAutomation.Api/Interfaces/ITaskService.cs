using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Tasks;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetMyTasksAsync(int userId, int page, int pageSize);
    Task<PagedResult<TaskDto>> GetPendingAsync(int page, int pageSize);
    Task<PagedResult<TaskDto>> GetCompletedAsync(int userId, int page, int pageSize);
    Task<TaskDetailDto> GetByIdAsync(int taskId);
    Task<TaskDetailDto> ActAsync(int taskId, TaskActionType action, string? comments, int actingUserId);
}
