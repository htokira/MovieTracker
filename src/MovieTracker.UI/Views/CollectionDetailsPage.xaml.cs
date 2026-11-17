using MovieTracker.UI.Models;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class CollectionDetailsPage : ContentPage, IQueryAttributable
{
    private readonly CollectionDetailsViewModel _viewModel;

    public CollectionDetailsPage(CollectionDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("collectionId", out var value)
            && Guid.TryParse(value?.ToString(), out var id))
        {
            _viewModel.CollectionId = id;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private void OnMovieTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CollectionMovieItem movie })
            _viewModel.OpenMovieCommand.Execute(movie);
    }

    private void OnRemoveClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: CollectionMovieItem movie })
            _viewModel.RemoveMovieCommand.Execute(movie);
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        Root.WidthRequest = Math.Min(640, Width - 40);
    }
}