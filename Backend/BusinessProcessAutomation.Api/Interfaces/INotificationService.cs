using BusinessProcessAutomation.Api.DTOs;
using BusinessProcessAutomation.Api.DTOs.Notifications;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface INotificationService
{
    Task CreateAsync(int userId, string type, string message);
    Task CreateForRoleAsync(int roleId, string type, string message);

    Task<PagedResult<NotificationDto>> GetForUserAsync(int userId, bool unreadOnly, int page, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId);
}
