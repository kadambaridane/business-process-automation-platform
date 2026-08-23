using BusinessProcessAutomation.Api.DTOs.Roles;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();
}
