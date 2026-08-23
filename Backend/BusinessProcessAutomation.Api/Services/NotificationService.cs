using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Notifications;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;

    public NotificationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(int userId, string type, string message)
    {
        _dbContext.Notifications.Add(new Notification { UserId = userId, Type = type, Message = message });
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateForRoleAsync(int roleId, string type, string message)
    {
        var userIds = await _dbContext.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        foreach (var userId in userIds)
        {
            _dbContext.Notifications.Add(new Notification { UserId = userId, Type = type, Message = message });
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<NotificationDto>> GetForUserAsync(int userId, bool unreadOnly, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _dbContext.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate
            })
            .ToListAsync();

        return new PagedResult<NotificationDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _dbContext.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId)
            ?? throw new KeyNotFoundException($"Notification with id {notificationId} was not found.");

        if (notification.UserId != userId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this notification.");
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var unread = await _dbContext.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }
        await _dbContext.SaveChangesAsync();
    }
}
