using Microsoft.Maui.Controls;
using MovieTracker.UI.Services;
using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly AuthService _auth;
    private bool _isInitialized;

    public MainPage(MainViewModel viewModel, AccountBarViewModel accountBar, AuthService auth)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _auth = auth;
        BindingContext = _viewModel;
        AccountBar.BindingContext = accountBar;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
            return;

        _isInitialized = true;
        var restoreSession = _auth.RestoreSessionAsync();
        await _viewModel.InitializeAsync();
        await restoreSession;
    }

    private void MoviesCollection_SizeChanged(object sender, EventArgs e)
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