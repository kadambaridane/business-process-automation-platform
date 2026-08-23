using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Reports;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IReportService
{
    Task<PagedResult<ApplicationReportRowDto>> GetApplicationReportAsync(
        WorkflowInstanceStatus? status, int? applicationId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);

    Task<List<StatusCountDto>> GetStatusSummaryAsync(int? applicationId, DateTime? dateFrom, DateTime? dateTo);

    Task<List<UserTaskReportDto>> GetUserTaskReportAsync(DateTime? dateFrom, DateTime? dateTo);
}
