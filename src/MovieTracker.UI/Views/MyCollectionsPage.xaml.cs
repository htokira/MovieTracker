using MovieTracker.UI.Models;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class MyCollectionsPage : ContentPage
{
    private readonly MyCollectionsViewModel _viewModel;

    public MyCollectionsPage(MyCollectionsViewModel viewModel)
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

    private void OnCollectionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CollectionListItem item })
            _viewModel.OpenCommand.Execute(item);
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        Root.WidthRequest = Math.Min(640, Width - 40);
    }
}