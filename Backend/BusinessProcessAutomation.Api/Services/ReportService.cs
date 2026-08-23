using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Reports;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ApplicationReportRowDto>> GetApplicationReportAsync(
        WorkflowInstanceStatus? status, int? applicationId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var query = FilteredInstanceQuery(status, applicationId, dateFrom, dateTo);

        var totalCount = await query.CountAsync();

        var instances = await query
            .OrderByDescending(i => i.StartedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(i => i.Application)
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.SubmittedByUser)
            .Include(i => i.CurrentStep)
            .ToListAsync();

        var items = instances.Select(i => new ApplicationReportRowDto
        {
            WorkflowInstanceId = i.Id,
            ApplicationId = i.ApplicationId,
            ApplicationName = i.Application.Name,
            WorkflowDefinitionName = i.WorkflowDefinition.Name,
            SubmittedByUserName = i.SubmittedByUser.Username,
            Status = i.Status,
            CurrentStepName = i.CurrentStep?.Name,
            StartedDate = i.StartedDate,
            CompletedDate = i.CompletedDate,
            DurationHours = i.CompletedDate.HasValue ? (i.CompletedDate.Value - i.StartedDate).TotalHours : null
        }).ToList();

        return new PagedResult<ApplicationReportRowDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<List<StatusCountDto>> GetStatusSummaryAsync(int? applicationId, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = FilteredInstanceQuery(null, applicationId, dateFrom, dateTo);

        return await query
            .GroupBy(i => i.Status)
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .ToListAsync();
    }

    public async Task<List<UserTaskReportDto>> GetUserTaskReportAsync(DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _dbContext.TaskHistories.Include(h => h.ActionByUser).AsQueryable();

        if (dateFrom.HasValue) query = query.Where(h => h.ActionDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(h => h.ActionDate <= dateTo.Value);

        var histories = await query.ToListAsync();

        return histories
            .GroupBy(h => h.ActionByUser)
            .Select(g => new UserTaskReportDto
            {
                UserId = g.Key.Id,
                Username = g.Key.Username,
                FullName = g.Key.FullName,
                ApprovedCount = g.Count(h => h.Action == "Approve"),
                RejectedCount = g.Count(h => h.Action == "Reject"),
                CompletedCount = g.Count(h => h.Action == "Complete"),
                TotalActioned = g.Count()
            })
            .OrderByDescending(r => r.TotalActioned)
            .ToList();
    }

    private IQueryable<WorkflowInstance> FilteredInstanceQuery(
        WorkflowInstanceStatus? status, int? applicationId, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _dbContext.WorkflowInstances.AsQueryable();

        if (status.HasValue) query = query.Where(i => i.Status == status.Value);
        if (applicationId.HasValue) query = query.Where(i => i.ApplicationId == applicationId.Value);
        if (dateFrom.HasValue) query = query.Where(i => i.StartedDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(i => i.StartedDate <= dateTo.Value);

        return query;
    }
}
