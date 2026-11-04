using MovieTracker.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace MovieTracker.UI.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private bool _isInitialized;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
            return;

        _isInitialized = true;

        await _viewModel.InitializeAsync();
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