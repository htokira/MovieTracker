using MovieTracker.UI.ViewModels;
using MovieTracker.Core.Dtos;

namespace MovieTracker.UI.Views;

public partial class MovieDetailsPage : ContentPage, IQueryAttributable
{
    private readonly MovieDetailsViewModel _viewModel;

    public MovieDetailsPage(MovieDetailsViewModel viewModel, AccountBarViewModel accountBar)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        AccountBar.BindingContext = accountBar;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("movieId", out var value)
            && int.TryParse(value?.ToString(), out int movieId))
        {
            _ = _viewModel.LoadMovieAsync(movieId);
        }
    }
        private async void OnAuthorTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: ReviewResponse review })
            await Shell.Current.GoToAsync($"{nameof(UserProfilePage)}?userId={review.UserId}");
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.RefreshMyReviewAsync();
    }
}
