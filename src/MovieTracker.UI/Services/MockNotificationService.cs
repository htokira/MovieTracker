using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

// Ендпоінта сповіщень на бекенді ще немає. Коли з'явиться, замінюємо цей клас.
public class MockNotificationService : INotificationService
{
    private readonly List<NotificationItem> _notifications = new()
    {
        new() { Id = 1, Text = "Олена підписалась на вас", CreatedAt = DateTime.Now.AddMinutes(-5) },
        new() { Id = 2, Text = "Вашу колекцію «Фантастика» переглянули 12 разів", CreatedAt = DateTime.Now.AddHours(-3) },
        new() { Id = 3, Text = "Максим створив нову публічну колекцію", CreatedAt = DateTime.Now.AddDays(-2), IsRead = true },
    };

    public async Task<List<NotificationItem>> GetNotificationsAsync()
    {
        await Task.Delay(300);
        return _notifications.OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task<string?> MarkNotificationReadAsync(int notificationId)
    {
        await Task.Delay(100);
        var item = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (item is not null) item.IsRead = true;
        return null;
    }
}
