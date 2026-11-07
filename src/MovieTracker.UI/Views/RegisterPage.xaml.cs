using MovieTracker.UI.ViewModels;

namespace MovieTracker.UI.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        FormStack.WidthRequest = Math.Min(420, Width - 40);
    }
}