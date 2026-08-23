using BusinessProcessAutomation.Api.DTOs.Forms;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IFormService
{
    Task<List<FormDefinitionSummaryDto>> GetByApplicationAsync(int applicationId);
    Task<FormDefinitionDto> GetByIdAsync(int formId);
    Task<FormDefinitionDto> CreateAsync(int applicationId, SaveFormDefinitionDto request);
    Task<FormDefinitionDto> UpdateAsync(int formId, SaveFormDefinitionDto request);
    Task<FormDefinitionDto> SetStatusAsync(int formId, bool isActive);

    Task<FormDefinitionDto> AddFieldAsync(int formId, SaveFormFieldDto request);
    Task<FormDefinitionDto> UpdateFieldAsync(int fieldId, SaveFormFieldDto request);
    Task<FormDefinitionDto> DeleteFieldAsync(int fieldId);
    Task<FormDefinitionDto> ReorderFieldsAsync(int formId, ReorderFieldsDto request);
}
