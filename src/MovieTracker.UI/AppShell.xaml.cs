using MovieTracker.UI.Views;

namespace MovieTracker.UI;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Головна",
            Content = mainPage
        });

        Routing.RegisterRoute("movie-details", typeof(MovieDetailsPage));
    }
}
