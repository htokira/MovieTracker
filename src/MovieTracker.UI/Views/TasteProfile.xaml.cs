using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class TasteProfilePage : ContentPage
{
    private readonly TasteProfileViewModel _viewModel;

    public TasteProfilePage(TasteProfileViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadProfileAsync();
    }
}