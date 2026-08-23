using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Forms;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class FormSubmissionService : IFormSubmissionService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly AppDbContext _dbContext;
    private readonly IWorkflowEngineService _workflowEngine;

    public FormSubmissionService(AppDbContext dbContext, IWorkflowEngineService workflowEngine)
    {
        _dbContext = dbContext;
        _workflowEngine = workflowEngine;
    }

    public async Task<FormSubmissionDto> SubmitAsync(int formId, SubmitFormRequestDto request, int submittedByUserId)
    {
        var form = await _dbContext.FormDefinitions
            .Include(f => f.FormFields)
            .Include(f => f.Application)
            .FirstOrDefaultAsync(f => f.Id == formId)
            ?? throw new KeyNotFoundException($"Form with id {formId} was not found.");

        if (!form.IsActive)
        {
            throw new InvalidOperationException("This form is not active and cannot accept submissions.");
        }

        ValidateSubmission(form.FormFields, request.Values);

        var submission = new FormSubmission
        {
            FormDefinitionId = form.Id,
            ApplicationId = form.ApplicationId,
            SubmittedByUserId = submittedByUserId,
            SubmissionData = JsonSerializer.Serialize(request.Values)
        };

        _dbContext.FormSubmissions.Add(submission);
        await _dbContext.SaveChangesAsync();

        await _workflowEngine.StartWorkflowAsync(submission.Id);

        return await GetByIdAsync(submission.Id);
    }

    public async Task<FormSubmissionDto> GetByIdAsync(int submissionId)
    {
        var submission = await _dbContext.FormSubmissions
            .Include(s => s.FormDefinition)
            .Include(s => s.Application)
            .Include(s => s.SubmittedByUser)
            .Include(s => s.WorkflowInstance)
            .FirstOrDefaultAsync(s => s.Id == submissionId)
            ?? throw new KeyNotFoundException($"Form submission with id {submissionId} was not found.");

        return MapSubmission(submission);
    }

    public async Task<PagedResult<FormSubmissionDto>> GetByFormAsync(int formId, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _dbContext.FormSubmissions
            .Include(s => s.FormDefinition)
            .Include(s => s.Application)
            .Include(s => s.SubmittedByUser)
            .Include(s => s.WorkflowInstance)
            .Where(s => s.FormDefinitionId == formId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.SubmittedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<FormSubmissionDto>
        {
            Items = items.Select(MapSubmission).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static void ValidateSubmission(ICollection<FormField> fields, Dictionary<string, JsonElement> values)
    {
        var errors = new List<string>();

        foreach (var field in fields)
        {
            values.TryGetValue(field.FieldName, out var element);
            var isMissing = element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
                || (element.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(element.GetString()));

            if (field.IsRequired && isMissing)
            {
                errors.Add($"'{field.Label}' is required.");
                continue;
            }

            if (isMissing)
            {
                continue;
            }

            var rules = string.IsNullOrEmpty(field.ValidationRulesJson)
                ? null
                : JsonSerializer.Deserialize<FieldValidationRulesDto>(field.ValidationRulesJson);

            var raw = element.ValueKind == JsonValueKind.String ? element.GetString() ?? string.Empty : element.GetRawText();

            switch (field.FieldType)
            {
                case FieldType.Number:
                    if (!decimal.TryParse(raw, out var numberValue))
                    {
                        errors.Add($"'{field.Label}' must be a number.");
                    }
                    else
                    {
                        if (rules?.Min is not null && numberValue < rules.Min) errors.Add($"'{field.Label}' must be at least {rules.Min}.");
                        if (rules?.Max is not null && numberValue > rules.Max) errors.Add($"'{field.Label}' must be at most {rules.Max}.");
                    }
                    break;

                case FieldType.Email:
                    if (!EmailRegex.IsMatch(raw))
                    {
                        errors.Add($"'{field.Label}' must be a valid email address.");
                    }
                    break;

                case FieldType.Date:
                    if (!DateTime.TryParse(raw, out _))
                    {
                        errors.Add($"'{field.Label}' must be a valid date.");
                    }
                    break;

                case FieldType.Dropdown:
                case FieldType.Radio:
                    if (rules?.Options is { Count: > 0 } && !rules.Options.Contains(raw))
                    {
                        errors.Add($"'{field.Label}' must be one of the configured options.");
                    }
                    break;

                case FieldType.Text:
                case FieldType.Textarea:
                    if (rules?.MinLength is not null && raw.Length < rules.MinLength) errors.Add($"'{field.Label}' must be at least {rules.MinLength} characters.");
                    if (rules?.MaxLength is not null && raw.Length > rules.MaxLength) errors.Add($"'{field.Label}' must be at most {rules.MaxLength} characters.");
                    if (!string.IsNullOrEmpty(rules?.Pattern) && !Regex.IsMatch(raw, rules.Pattern))
                    {
                        errors.Add($"'{field.Label}' does not match the required format.");
                    }
                    break;

                case FieldType.Checkbox:
                    break;
            }
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }

    private static FormSubmissionDto MapSubmission(FormSubmission submission) => new()
    {
        Id = submission.Id,
        FormDefinitionId = submission.FormDefinitionId,
        FormDefinitionName = submission.FormDefinition.Name,
        ApplicationId = submission.ApplicationId,
        ApplicationName = submission.Application.Name,
        SubmittedByUserName = submission.SubmittedByUser.Username,
        SubmissionData = submission.SubmissionData,
        SubmittedDate = submission.SubmittedDate,
        WorkflowInstanceId = submission.WorkflowInstance?.Id
    };
}
