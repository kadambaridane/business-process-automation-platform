using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs.Dashboard;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var totalApplications = await _dbContext.Applications.CountAsync();

        // The engine collapses a successfully finished run into WorkflowInstanceStatus.Completed
        // (there's no separate terminal "Approved" state) — so "Approved" here means "Completed".
        var pendingApplications = await _dbContext.WorkflowInstances.CountAsync(i => i.Status == WorkflowInstanceStatus.InProgress);
        var approvedApplications = await _dbContext.WorkflowInstances.CountAsync(i => i.Status == WorkflowInstanceStatus.Completed);
        var rejectedApplications = await _dbContext.WorkflowInstances.CountAsync(i => i.Status == WorkflowInstanceStatus.Rejected);

        var pendingTasks = await _dbContext.Tasks.CountAsync(t => t.Status == WorkflowTaskStatus.Pending);
        var completedTasks = await _dbContext.Tasks.CountAsync(t => t.Status != WorkflowTaskStatus.Pending);

        var recentApplications = await _dbContext.Applications
            .OrderByDescending(a => a.CreatedDate)
            .Take(5)
            .Select(a => new RecentApplicationDto
            {
                Id = a.Id,
                Name = a.Name,
                IsActive = a.IsActive,
                CreatedByUserName = a.CreatedByUser.Username,
                CreatedDate = a.CreatedDate
            })
            .ToListAsync();

        var recentWorkflowActivity = await _dbContext.WorkflowInstances
            .OrderByDescending(i => i.CompletedDate ?? i.StartedDate)
            .Take(5)
            .Select(i => new RecentWorkflowActivityDto
            {
                WorkflowInstanceId = i.Id,
                ApplicationName = i.Application.Name,
                WorkflowDefinitionName = i.WorkflowDefinition.Name,
                SubmittedByUserName = i.SubmittedByUser.Username,
                Status = i.Status,
                CurrentStepName = i.CurrentStep != null ? i.CurrentStep.Name : null,
                ActivityDate = i.CompletedDate ?? i.StartedDate
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalApplications = totalApplications,
            PendingApplications = pendingApplications,
            ApprovedApplications = approvedApplications,
            RejectedApplications = rejectedApplications,
            PendingTasks = pendingTasks,
            CompletedTasks = completedTasks,
            RecentApplications = recentApplications,
            RecentWorkflowActivity = recentWorkflowActivity
        };
    }
}
