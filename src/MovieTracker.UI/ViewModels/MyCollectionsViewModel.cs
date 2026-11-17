using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieTracker.UI.Models;
using MovieTracker.UI.Services;
using MovieTracker.UI.Views;

namespace MovieTracker.UI.ViewModels;

public class MyCollectionsViewModel : BaseViewModel
{
    private readonly ITrackingService _tracking;
    private readonly AuthService _auth;
    private bool _isEmpty;

    public ObservableCollection<CollectionListItem> Items { get; } = new();

    public bool IsEmpty { get => _isEmpty; private set => SetProperty(ref _isEmpty, value); }

    public ICommand OpenCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand GoBackCommand { get; }

    public MyCollectionsViewModel(ITrackingService tracking, AuthService auth)
    {
        _tracking = tracking;
        _auth = auth;

        OpenCommand = new Command<CollectionListItem>(async item =>
        {
            if (item is not null)
                await Shell.Current.GoToAsync($"{nameof(CollectionDetailsPage)}?collectionId={item.Id}");
        });
        CreateCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(CreateCollectionPage)));
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadAsync()
    {
        if (!_auth.IsAuthenticated)
        {
            Items.Clear();
            IsEmpty = false;
            ErrorMessage = "Увійдіть в акаунт, щоб побачити свої колекції.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        IsEmpty = false;

        try
        {
            var (items, error) = await _tracking.GetMyCollectionsAsync();

            Items.Clear();

            if (error is not null || items is null)
            {
                ErrorMessage = error ?? "Не вдалося завантажити колекції.";
                return;
            }

            foreach (var item in items)
                Items.Add(item);

            IsEmpty = Items.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
