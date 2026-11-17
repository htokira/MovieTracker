using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

public interface INotificationService
{
    Task<List<NotificationItem>> GetNotificationsAsync();
    Task<string?> MarkNotificationReadAsync(int notificationId);
}
