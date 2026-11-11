using Microsoft.Maui.Controls;
using MovieTracker.UI.Services;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly RecommendationsViewModel _recommendations;
    private readonly AuthService _auth;
    private bool _isInitialized;

    public MainPage(MainViewModel viewModel, AccountBarViewModel accountBar, RecommendationsViewModel recommendations, AuthService auth)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _recommendations = recommendations;
        _auth = auth;

        BindingContext = _viewModel;
        AccountBar.BindingContext = accountBar;
        RecommendationsSection.BindingContext = _recommendations;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            _isInitialized = true;

            var restoreSession = _auth.RestoreSessionAsync();
            var loadCatalog = _viewModel.InitializeAsync();

            await restoreSession;
            await Task.WhenAll(loadCatalog, _recommendations.LoadAsync());
            return;
        }

        await _recommendations.LoadAsync();
    }

    private void MoviesCollection_SizeChanged(object? sender, EventArgs e)
    {
        if (sender is not CollectionView collectionView)
            return;

        if (collectionView.Width <= 0)
            return;

        const double minColumnWidth = 300;

        int columns = Math.Clamp(
            (int)(collectionView.Width / minColumnWidth),
            1,
            5
        );

        if (collectionView.ItemsLayout is GridItemsLayout layout
            && layout.Span != columns)
        {
            layout.Span = columns;
        }
    }
}