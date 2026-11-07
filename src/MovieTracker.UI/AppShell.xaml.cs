using MovieTracker.UI.Views;

namespace MovieTracker.UI;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));

        Items.Add(new ShellContent
        {
            Title = "Головна",
            Content = mainPage
        });
    }
}
