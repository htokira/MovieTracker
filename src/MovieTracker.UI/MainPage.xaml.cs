using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}