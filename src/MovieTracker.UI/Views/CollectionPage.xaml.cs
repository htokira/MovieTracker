using MovieTracker.Core.Dtos;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class CollectionPage : ContentPage, IQueryAttributable
{
    private readonly CollectionViewModel _viewModel;

    public CollectionPage(CollectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("collectionId", out var value) && Guid.TryParse(value?.ToString(), out var id))
            _ = _viewModel.LoadAsync(id);
    }

    private async void OnMovieTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CollectionMovieResponse movie })
            await Shell.Current.GoToAsync($"movie-details?movieId={movie.MovieId}");
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        FormStack.WidthRequest = Math.Min(420, Width - 40);
    }
}