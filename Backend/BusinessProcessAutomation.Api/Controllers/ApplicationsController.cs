using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Applications;
using BusinessProcessAutomation.Api.Interfaces;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private const string CanManageRoles = "Admin,Business User";

    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _applicationService.GetAllAsync(search, isActive, page, pageSize);
        return Ok(ApiResponse<PagedResult<ApplicationDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _applicationService.GetByIdAsync(id);
        return Ok(ApiResponse<ApplicationDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveApplicationDto request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _applicationService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ApplicationDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveApplicationDto request)
    {
        var result = await _applicationService.UpdateAsync(id, request);
        return Ok(ApiResponse<ApplicationDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusDto request)
    {
        var result = await _applicationService.SetStatusAsync(id, request.IsActive);
        return Ok(ApiResponse<ApplicationDto>.Ok(result));
    }
}
