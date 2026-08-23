using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Applications;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IApplicationService
{
    Task<PagedResult<ApplicationDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize);
    Task<ApplicationDto> GetByIdAsync(int id);
    Task<ApplicationDto> CreateAsync(SaveApplicationDto request, int createdByUserId);
    Task<ApplicationDto> UpdateAsync(int id, SaveApplicationDto request);
    Task<ApplicationDto> SetStatusAsync(int id, bool isActive);
}
