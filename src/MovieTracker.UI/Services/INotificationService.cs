using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

public interface INotificationService
{
    Task<(List<NotificationItem>? Items, string? Error)> GetNotificationsAsync();
    Task<string?> MarkReadAsync(Guid notificationId);
    Task<string?> MarkAllReadAsync();
}