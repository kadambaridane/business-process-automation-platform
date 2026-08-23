using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Forms;
using BusinessProcessAutomation.Api.Interfaces;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Authorize]
public class FormsController : ControllerBase
{
    private const string CanManageRoles = "Admin,Business User";

    private readonly IFormService _formService;
    private readonly IFormSubmissionService _formSubmissionService;

    public FormsController(IFormService formService, IFormSubmissionService formSubmissionService)
    {
        _formService = formService;
        _formSubmissionService = formSubmissionService;
    }

    [HttpGet("api/applications/{applicationId:int}/forms")]
    public async Task<IActionResult> GetByApplication(int applicationId)
    {
        var result = await _formService.GetByApplicationAsync(applicationId);
        return Ok(ApiResponse<List<FormDefinitionSummaryDto>>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost("api/applications/{applicationId:int}/forms")]
    public async Task<IActionResult> Create(int applicationId, [FromBody] SaveFormDefinitionDto request)
    {
        var result = await _formService.CreateAsync(applicationId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [HttpGet("api/forms/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _formService.GetByIdAsync(id);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPut("api/forms/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveFormDefinitionDto request)
    {
        var result = await _formService.UpdateAsync(id, request);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPatch("api/forms/{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusDto request)
    {
        var result = await _formService.SetStatusAsync(id, request.IsActive);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost("api/forms/{id:int}/fields")]
    public async Task<IActionResult> AddField(int id, [FromBody] SaveFormFieldDto request)
    {
        var result = await _formService.AddFieldAsync(id, request);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPut("api/forms/fields/{fieldId:int}")]
    public async Task<IActionResult> UpdateField(int fieldId, [FromBody] SaveFormFieldDto request)
    {
        var result = await _formService.UpdateFieldAsync(fieldId, request);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpDelete("api/forms/fields/{fieldId:int}")]
    public async Task<IActionResult> DeleteField(int fieldId)
    {
        var result = await _formService.DeleteFieldAsync(fieldId);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpPost("api/forms/{id:int}/fields/reorder")]
    public async Task<IActionResult> ReorderFields(int id, [FromBody] ReorderFieldsDto request)
    {
        var result = await _formService.ReorderFieldsAsync(id, request);
        return Ok(ApiResponse<FormDefinitionDto>.Ok(result));
    }

    [HttpPost("api/forms/{id:int}/submissions")]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitFormRequestDto request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _formSubmissionService.SubmitAsync(id, request, userId);
        return CreatedAtAction(nameof(GetSubmission), new { submissionId = result.Id }, ApiResponse<FormSubmissionDto>.Ok(result));
    }

    [HttpGet("api/forms/submissions/{submissionId:int}")]
    public async Task<IActionResult> GetSubmission(int submissionId)
    {
        var result = await _formSubmissionService.GetByIdAsync(submissionId);
        return Ok(ApiResponse<FormSubmissionDto>.Ok(result));
    }

    [Authorize(Roles = CanManageRoles)]
    [HttpGet("api/forms/{id:int}/submissions")]
    public async Task<IActionResult> GetSubmissions(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _formSubmissionService.GetByFormAsync(id, page, pageSize);
        return Ok(ApiResponse<PagedResult<FormSubmissionDto>>.Ok(result));
    }
}
