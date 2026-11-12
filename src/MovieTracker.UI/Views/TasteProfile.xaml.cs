using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class TasteProfilePage : ContentPage
{
    private readonly TasteProfileViewModel _viewModel;

    public TasteProfilePage(TasteProfileViewModel viewModel, AccountBarViewModel accountBar)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;

        AccountBar.BindingContext = accountBar;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadProfileAsync();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}