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
        Routing.RegisterRoute(nameof(TasteProfilePage), typeof(TasteProfilePage));
        Routing.RegisterRoute(nameof(Views.UserProfilePage), typeof(Views.UserProfilePage));
        Routing.RegisterRoute(nameof(TrackingListsPage), typeof(TrackingListsPage));
        Routing.RegisterRoute(nameof(CreateCollectionPage), typeof(CreateCollectionPage));
        Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
        Routing.RegisterRoute(nameof(MyCollectionsPage), typeof(MyCollectionsPage));
        Routing.RegisterRoute(nameof(CollectionDetailsPage), typeof(CollectionDetailsPage));
        Routing.RegisterRoute(nameof(Views.CollectionPage), typeof(Views.CollectionPage));

        Items.Add(new ShellContent
        {
            Title = "Головна",
            Content = mainPage
        });

        Routing.RegisterRoute("movie-details", typeof(MovieDetailsPage));
    }
}
