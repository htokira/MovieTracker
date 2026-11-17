using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieTracker.UI.Models;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class NotificationsViewModel : BaseViewModel
{
    private readonly INotificationService _notifications;
    private readonly AuthService _auth;

    public ObservableCollection<NotificationItem> Items { get; } = new();

    public ICommand MarkReadCommand { get; }
    public ICommand MarkAllReadCommand { get; }
    public ICommand GoBackCommand { get; }

    public NotificationsViewModel(INotificationService notifications, AuthService auth)
    {
        _notifications = notifications;
        _auth = auth;

        MarkReadCommand = new Command<NotificationItem>(async item => await MarkReadAsync(item));
        MarkAllReadCommand = new Command(async () => await MarkAllReadAsync());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadAsync()
    {
        if (!_auth.IsAuthenticated)
        {
            Items.Clear();
            ErrorMessage = "Увійдіть в акаунт, щоб побачити сповіщення.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var (items, error) = await _notifications.GetNotificationsAsync();

            Items.Clear();

            if (error is not null || items is null)
            {
                ErrorMessage = error ?? "Не вдалося завантажити сповіщення.";
                return;
            }

            foreach (var item in items)
                Items.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task MarkReadAsync(NotificationItem? item)
    {
        if (item is null || item.IsRead) return;

        var error = await _notifications.MarkReadAsync(item.Id);
        if (error is null)
            item.IsRead = true;
        else
            ErrorMessage = error;
    }

    private async Task MarkAllReadAsync()
    {
        if (IsBusy || Items.All(i => i.IsRead)) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var error = await _notifications.MarkAllReadAsync();
            if (error is not null)
            {
                ErrorMessage = error;
                return;
            }

            foreach (var item in Items)
                item.IsRead = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
