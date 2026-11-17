using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieTracker.UI.Models;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class NotificationsViewModel : BaseViewModel
{
    private readonly INotificationService _notifications;

    public ObservableCollection<NotificationItem> Items { get; } = new();

    public ICommand MarkReadCommand { get; }
    public ICommand GoBackCommand { get; }

    public NotificationsViewModel(INotificationService notifications)
    {
        _notifications = notifications;

        MarkReadCommand = new Command<NotificationItem>(async item => await MarkReadAsync(item));
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var items = await _notifications.GetNotificationsAsync();

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = "Не вдалося завантажити сповіщення.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkReadAsync(NotificationItem? item)
    {
        if (item is null || item.IsRead) return;

        var error = await _notifications.MarkNotificationReadAsync(item.Id);
        if (error is null)
            item.IsRead = true;
        else
            ErrorMessage = error;
    }
}
