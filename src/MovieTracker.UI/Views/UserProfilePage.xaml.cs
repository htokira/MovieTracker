using MovieTracker.Core.Dtos;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class UserProfilePage : ContentPage, IQueryAttributable
{
    private readonly UserProfileViewModel _viewModel;

    public UserProfilePage(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("userId", out var value) && Guid.TryParse(value?.ToString(), out var id))
            _ = _viewModel.LoadAsync(id);
    }

    // Натиснули на фільм у відгуку, відкриваємо сторінку фільму
    private async void OnReviewMovieTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: UserReviewItemResponse review })
            await Shell.Current.GoToAsync($"movie-details?movieId={review.MovieId}");
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        FormStack.WidthRequest = Math.Min(420, Width - 40);
    }

    private async void OnCollectionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: CollectionSummaryResponse collection })
            await Shell.Current.GoToAsync($"{nameof(CollectionPage)}?collectionId={collection.Id}");
    }
}
