using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Applications;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class ApplicationService : IApplicationService
{
    private readonly AppDbContext _dbContext;

    public ApplicationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ApplicationDto>> GetAllAsync(string? search, bool? isActive, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _dbContext.Applications.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.Name.Contains(search) || (a.Description != null && a.Description.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                IsActive = a.IsActive,
                CreatedByUserName = a.CreatedByUser.Username,
                CreatedDate = a.CreatedDate,
                FormDefinitionCount = a.FormDefinitions.Count,
                WorkflowDefinitionCount = a.WorkflowDefinitions.Count
            })
            .ToListAsync();

        return new PagedResult<ApplicationDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<ApplicationDto> GetByIdAsync(int id)
    {
        var application = await FindOrThrowAsync(id);
        return MapToDto(application);
    }

    public async Task<ApplicationDto> CreateAsync(SaveApplicationDto request, int createdByUserId)
    {
        var application = new BusinessApplication
        {
            Name = request.Name,
            Description = request.Description,
            CreatedByUserId = createdByUserId,
            IsActive = true
        };

        _dbContext.Applications.Add(application);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(application.Id);
    }

    public async Task<ApplicationDto> UpdateAsync(int id, SaveApplicationDto request)
    {
        var application = await FindOrThrowAsync(id);

        application.Name = request.Name;
        application.Description = request.Description;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(application.Id);
    }

    public async Task<ApplicationDto> SetStatusAsync(int id, bool isActive)
    {
        var application = await FindOrThrowAsync(id);
        application.IsActive = isActive;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(application.Id);
    }

    private async Task<BusinessApplication> FindOrThrowAsync(int id)
    {
        var application = await _dbContext.Applications
            .Include(a => a.CreatedByUser)
            .Include(a => a.FormDefinitions)
            .Include(a => a.WorkflowDefinitions)
            .FirstOrDefaultAsync(a => a.Id == id);

        return application ?? throw new KeyNotFoundException($"Application with id {id} was not found.");
    }

    private static ApplicationDto MapToDto(BusinessApplication a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        IsActive = a.IsActive,
        CreatedByUserName = a.CreatedByUser.Username,
        CreatedDate = a.CreatedDate,
        FormDefinitionCount = a.FormDefinitions.Count,
        WorkflowDefinitionCount = a.WorkflowDefinitions.Count
    };
}
