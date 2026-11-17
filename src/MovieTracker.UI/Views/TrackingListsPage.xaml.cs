using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class TrackingListsPage : ContentPage
{
    private readonly TrackingListsViewModel _viewModel;

    public TrackingListsPage(TrackingListsViewModel viewModel)
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

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        Root.WidthRequest = Math.Min(640, Width - 40);
    }
}
