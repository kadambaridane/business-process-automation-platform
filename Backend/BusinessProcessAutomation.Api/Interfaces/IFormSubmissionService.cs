using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Forms;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IFormSubmissionService
{
    Task<FormSubmissionDto> SubmitAsync(int formId, SubmitFormRequestDto request, int submittedByUserId);
    Task<FormSubmissionDto> GetByIdAsync(int submissionId);
    Task<PagedResult<FormSubmissionDto>> GetByFormAsync(int formId, int page, int pageSize);
}
