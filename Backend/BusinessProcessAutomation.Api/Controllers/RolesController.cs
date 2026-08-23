using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Roles;
using BusinessProcessAutomation.Api.Interfaces;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Ok(ApiResponse<List<RoleDto>>.Ok(result));
    }
}
