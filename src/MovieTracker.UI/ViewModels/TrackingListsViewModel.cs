using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieTracker.Core.Dtos;
using MovieTracker.Core.Models;
using MovieTracker.UI.Models;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class TrackingListsViewModel : BaseViewModel
{
    private readonly ITrackingService _tracking;
    private readonly AuthService _auth;
    private WatchStatus _status = WatchStatus.Watched;
    private bool _isEmpty;

    public ObservableCollection<TrackedMovieItem> Movies { get; } = new();

    public bool IsWatchedSelected => _status == WatchStatus.Watched;
    public bool IsPlannedSelected => _status == WatchStatus.Planned;

    public bool IsEmpty { get => _isEmpty; private set => SetProperty(ref _isEmpty, value); }

    public string EmptyText => _status == WatchStatus.Watched
        ? "Ви ще не позначили жодного фільму як переглянутий."
        : "У списку «Заплановано» поки порожньо.";

    public ICommand ShowWatchedCommand { get; }
    public ICommand ShowPlannedCommand { get; }
    public ICommand GoBackCommand { get; }

    public TrackingListsViewModel(ITrackingService tracking, AuthService auth)
    {
        _tracking = tracking;
        _auth = auth;

        ShowWatchedCommand = new Command(async () => await SelectAsync(WatchStatus.Watched));
        ShowPlannedCommand = new Command(async () => await SelectAsync(WatchStatus.Planned));
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadAsync()
    {
        if (!_auth.IsAuthenticated)
        {
            Movies.Clear();
            IsEmpty = false;
            ErrorMessage = "Увійдіть в акаунт, щоб побачити свої списки.";
            return;
        }

        var requested = _status;
        IsBusy = true;
        ErrorMessage = null;
        IsEmpty = false;

        try
        {
            var (items, error) = await _tracking.GetMyMoviesAsync(requested);

            // Користувач міг переключити вкладку, поки йшов запит
            if (requested != _status) return;

            Movies.Clear();

            if (error is not null || items is null)
            {
                ErrorMessage = error ?? "Не вдалося завантажити список.";
                return;
            }

            foreach (var item in items)
                Movies.Add(item);

            IsEmpty = Movies.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SelectAsync(WatchStatus status)
    {
        if (_status == status) return;

        _status = status;
        OnPropertyChanged(nameof(IsWatchedSelected));
        OnPropertyChanged(nameof(IsPlannedSelected));
        OnPropertyChanged(nameof(EmptyText));

        await LoadAsync();
    }
}
