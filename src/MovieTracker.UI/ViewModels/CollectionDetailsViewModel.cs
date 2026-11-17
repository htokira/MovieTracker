using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieTracker.UI.Models;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class CollectionDetailsViewModel : BaseViewModel
{
    private readonly ITrackingService _tracking;

    private string _name = string.Empty;
    private string? _description;
    private string _privacyText = string.Empty;
    private bool _isEmpty;

    public Guid CollectionId { get; set; }

    public ObservableCollection<CollectionMovieItem> Movies { get; } = new();

    public string Name { get => _name; private set => SetProperty(ref _name, value); }

    public string? Description
    {
        get => _description;
        private set
        {
            if (SetProperty(ref _description, value))
                OnPropertyChanged(nameof(HasDescription));
        }
    }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public string PrivacyText { get => _privacyText; private set => SetProperty(ref _privacyText, value); }

    public bool IsEmpty { get => _isEmpty; private set => SetProperty(ref _isEmpty, value); }

    public ICommand RemoveMovieCommand { get; }
    public ICommand OpenMovieCommand { get; }
    public ICommand DeleteCollectionCommand { get; }
    public ICommand GoBackCommand { get; }

    public CollectionDetailsViewModel(ITrackingService tracking)
    {
        _tracking = tracking;

        RemoveMovieCommand = new Command<CollectionMovieItem>(async m => await RemoveMovieAsync(m));
        OpenMovieCommand = new Command<CollectionMovieItem>(async m =>
        {
            if (m is not null)
                await Shell.Current.GoToAsync($"movie-details?movieId={m.MovieId}");
        });
        DeleteCollectionCommand = new Command(async () => await DeleteCollectionAsync());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public async Task LoadAsync()
    {
        if (CollectionId == Guid.Empty) return;

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var (detail, error) = await _tracking.GetCollectionAsync(CollectionId);

            if (error is not null || detail is null)
            {
                ErrorMessage = error ?? "Не вдалося завантажити колекцію.";
                return;
            }

            Name = detail.Name;
            Description = detail.Description;
            PrivacyText = detail.IsPublic ? "Публічна колекція" : "Приватна колекція";

            Movies.Clear();
            foreach (var movie in detail.Movies)
                Movies.Add(movie);

            IsEmpty = Movies.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveMovieAsync(CollectionMovieItem? movie)
    {
        if (movie is null || IsBusy) return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Прибрати фільм?", $"«{movie.Title}» буде прибрано з колекції.", "Прибрати", "Скасувати");
        if (!confirmed) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var error = await _tracking.RemoveMovieFromCollectionAsync(CollectionId, movie.MovieId);
            if (error is not null)
            {
                ErrorMessage = error;
                return;
            }

            Movies.Remove(movie);
            IsEmpty = Movies.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteCollectionAsync()
    {
        if (IsBusy) return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Видалити колекцію?", "Цю дію не можна скасувати.", "Видалити", "Скасувати");
        if (!confirmed) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var error = await _tracking.DeleteCollectionAsync(CollectionId);
            if (error is not null)
            {
                ErrorMessage = error;
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
