using BusinessProcessAutomation.Api.DTOs.Dashboard;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
