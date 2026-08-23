using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Reports;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,Business User")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplicationReport(
        [FromQuery] WorkflowInstanceStatus? status,
        [FromQuery] int? applicationId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _reportService.GetApplicationReportAsync(status, applicationId, dateFrom, dateTo, page, pageSize);
        return Ok(ApiResponse<PagedResult<ApplicationReportRowDto>>.Ok(result));
    }

    [HttpGet("applications/status-summary")]
    public async Task<IActionResult> GetStatusSummary(
        [FromQuery] int? applicationId, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetStatusSummaryAsync(applicationId, dateFrom, dateTo);
        return Ok(ApiResponse<List<StatusCountDto>>.Ok(result));
    }

    [HttpGet("user-tasks")]
    public async Task<IActionResult> GetUserTaskReport([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetUserTaskReportAsync(dateFrom, dateTo);
        return Ok(ApiResponse<List<UserTaskReportDto>>.Ok(result));
    }
}
