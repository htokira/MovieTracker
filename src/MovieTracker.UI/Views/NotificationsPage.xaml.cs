using MovieTracker.UI.Models;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;

    public NotificationsPage(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not NotificationItem item)
            return;

        if (_viewModel.MarkReadCommand.CanExecute(item))
            _viewModel.MarkReadCommand.Execute(item);

        NotificationsCollection.SelectedItem = null;
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        Root.WidthRequest = Math.Min(640, Width - 40);
    }
}
