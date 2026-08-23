using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs.Forms;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class FormService : IFormService
{
    private readonly AppDbContext _dbContext;

    public FormService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FormDefinitionSummaryDto>> GetByApplicationAsync(int applicationId)
    {
        return await _dbContext.FormDefinitions
            .Where(f => f.ApplicationId == applicationId)
            .OrderByDescending(f => f.CreatedDate)
            .Select(f => new FormDefinitionSummaryDto
            {
                Id = f.Id,
                Name = f.Name,
                IsActive = f.IsActive,
                FieldCount = f.FormFields.Count,
                CreatedDate = f.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<FormDefinitionDto> GetByIdAsync(int formId)
    {
        var form = await FindFormOrThrowAsync(formId);
        return MapForm(form);
    }

    public async Task<FormDefinitionDto> CreateAsync(int applicationId, SaveFormDefinitionDto request)
    {
        var applicationExists = await _dbContext.Applications.AnyAsync(a => a.Id == applicationId);
        if (!applicationExists)
        {
            throw new KeyNotFoundException($"Application with id {applicationId} was not found.");
        }

        var form = new FormDefinition { ApplicationId = applicationId, Name = request.Name, IsActive = true };
        _dbContext.FormDefinitions.Add(form);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(form.Id);
    }

    public async Task<FormDefinitionDto> UpdateAsync(int formId, SaveFormDefinitionDto request)
    {
        var form = await FindFormOrThrowAsync(formId);
        form.Name = request.Name;
        await _dbContext.SaveChangesAsync();
        return MapForm(form);
    }

    public async Task<FormDefinitionDto> SetStatusAsync(int formId, bool isActive)
    {
        var form = await FindFormOrThrowAsync(formId);
        form.IsActive = isActive;
        await _dbContext.SaveChangesAsync();
        return MapForm(form);
    }

    public async Task<FormDefinitionDto> AddFieldAsync(int formId, SaveFormFieldDto request)
    {
        var form = await FindFormOrThrowAsync(formId);

        var nextOrder = form.FormFields.Count == 0 ? 1 : form.FormFields.Max(f => f.DisplayOrder) + 1;

        var field = new FormField
        {
            FormDefinitionId = formId,
            FieldName = request.FieldName,
            Label = request.Label,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            DisplayOrder = request.DisplayOrder ?? nextOrder,
            DefaultValue = request.DefaultValue,
            ValidationRulesJson = SerializeRules(request.ValidationRules)
        };

        _dbContext.FormFields.Add(field);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(formId);
    }

    public async Task<FormDefinitionDto> UpdateFieldAsync(int fieldId, SaveFormFieldDto request)
    {
        var field = await _dbContext.FormFields.FirstOrDefaultAsync(f => f.Id == fieldId)
            ?? throw new KeyNotFoundException($"Form field with id {fieldId} was not found.");

        field.FieldName = request.FieldName;
        field.Label = request.Label;
        field.FieldType = request.FieldType;
        field.IsRequired = request.IsRequired;
        if (request.DisplayOrder.HasValue)
        {
            field.DisplayOrder = request.DisplayOrder.Value;
        }
        field.DefaultValue = request.DefaultValue;
        field.ValidationRulesJson = SerializeRules(request.ValidationRules);

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(field.FormDefinitionId);
    }

    public async Task<FormDefinitionDto> DeleteFieldAsync(int fieldId)
    {
        var field = await _dbContext.FormFields.FirstOrDefaultAsync(f => f.Id == fieldId)
            ?? throw new KeyNotFoundException($"Form field with id {fieldId} was not found.");

        var formId = field.FormDefinitionId;
        _dbContext.FormFields.Remove(field);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(formId);
    }

    public async Task<FormDefinitionDto> ReorderFieldsAsync(int formId, ReorderFieldsDto request)
    {
        var form = await FindFormOrThrowAsync(formId);
        var fieldsById = form.FormFields.ToDictionary(f => f.Id);

        if (request.FieldIdsInOrder.Count != fieldsById.Count || request.FieldIdsInOrder.Any(id => !fieldsById.ContainsKey(id)))
        {
            throw new ArgumentException("The reorder list must contain exactly the field ids belonging to this form.");
        }

        for (var i = 0; i < request.FieldIdsInOrder.Count; i++)
        {
            fieldsById[request.FieldIdsInOrder[i]].DisplayOrder = i + 1;
        }

        await _dbContext.SaveChangesAsync();
        return MapForm(form);
    }

    private async Task<FormDefinition> FindFormOrThrowAsync(int formId)
    {
        var form = await _dbContext.FormDefinitions
            .Include(f => f.FormFields)
            .FirstOrDefaultAsync(f => f.Id == formId);

        return form ?? throw new KeyNotFoundException($"Form with id {formId} was not found.");
    }

    private static string? SerializeRules(FieldValidationRulesDto? rules)
    {
        if (rules is null)
        {
            return null;
        }

        var isEmpty = rules.MinLength is null && rules.MaxLength is null && rules.Min is null
            && rules.Max is null && string.IsNullOrEmpty(rules.Pattern) && (rules.Options is null || rules.Options.Count == 0);

        return isEmpty ? null : JsonSerializer.Serialize(rules);
    }

    private static FormDefinitionDto MapForm(FormDefinition form) => new()
    {
        Id = form.Id,
        Name = form.Name,
        IsActive = form.IsActive,
        ApplicationId = form.ApplicationId,
        CreatedDate = form.CreatedDate,
        Fields = form.FormFields
            .OrderBy(f => f.DisplayOrder)
            .Select(MapField)
            .ToList()
    };

    private static FormFieldDto MapField(FormField field) => new()
    {
        Id = field.Id,
        FieldName = field.FieldName,
        Label = field.Label,
        FieldType = field.FieldType,
        IsRequired = field.IsRequired,
        DisplayOrder = field.DisplayOrder,
        DefaultValue = field.DefaultValue,
        ValidationRules = string.IsNullOrEmpty(field.ValidationRulesJson)
            ? null
            : JsonSerializer.Deserialize<FieldValidationRulesDto>(field.ValidationRulesJson)
    };
}
