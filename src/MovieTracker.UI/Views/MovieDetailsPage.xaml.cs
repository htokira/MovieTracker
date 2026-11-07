using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class MovieDetailsPage : ContentPage, IQueryAttributable
{
    private readonly MovieDetailsViewModel _viewModel;

    public MovieDetailsPage(MovieDetailsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("movieId", out var value)
            && int.TryParse(value?.ToString(), out int movieId))
        {
            _ = _viewModel.LoadMovieAsync(movieId);
        }
    }
}