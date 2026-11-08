using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MovieTracker.Core.Dtos;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;

    private string _searchQuery = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;
    private string _selectedGenre = "Усі жанри";
    private string _selectedSort = "За популярністю";
    private bool _isEmpty;

    private readonly Dictionary<string, int?> _genreIds = new()
    {
        { "Усі жанри", null },
        { "Бойовик", 28 },
        { "Пригоди", 12 },
        { "Анімація", 16 },
        { "Комедія", 35 },
        { "Кримінал", 80 },
        { "Документальний", 99 },
        { "Драма", 18 },
        { "Сімейний", 10751 },
        { "Фентезі", 14 },
        { "Жахи", 27 },
        { "Мелодрама", 10749 },
        { "Фантастика", 878 },
        { "Трилер", 53 }
    };

    private readonly Dictionary<string, string> _sortOptions = new()
    {
        { "За популярністю", "popularity.desc" },
        { "За рейтингом ↓", "vote_average.desc" },
        { "За рейтингом ↑", "vote_average.asc" },
        { "За датою ↓", "primary_release_date.desc" },
        { "За датою ↑", "primary_release_date.asc" }
    };

    public ObservableCollection<MovieListItemDto> PopularMovies { get; } = new();
    public ObservableCollection<MovieListItemDto> Movies { get; } = new();
    public ObservableCollection<string> GenreOptions { get; }

    public ObservableCollection<string> SortOptions { get; }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery != value)
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string SelectedGenre
    {
        get => _selectedGenre;
        set
        {
            if (_selectedGenre != value)
            {
                _selectedGenre = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedSort
    {
        get => _selectedSort;
        set
        {
            if (_selectedSort != value)
            {
                _selectedSort = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set
        {
            if (_isEmpty != value)
            {
                _isEmpty = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SearchCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand OpenMovieCommand { get; }

    public MainViewModel(ApiService apiService)
    {
        _apiService = apiService;
        SearchCommand = new Command(async () => await LoadMoviesAsync());
        ApplyFiltersCommand = new Command(async () => await LoadMoviesAsync());
        GenreOptions = new ObservableCollection<string>(_genreIds.Keys);
        SortOptions = new ObservableCollection<string>(_sortOptions.Keys);
        OpenMovieCommand = new Command<MovieListItemDto>(async movie =>
        {
            if (movie == null)
                return;

            await Shell.Current.GoToAsync(
                $"movie-details?movieId={movie.Id}");
        });
    }

    public async Task InitializeAsync()
    {
        await LoadPopularMoviesAsync();
        await LoadMoviesAsync();
    }

    public async Task LoadPopularMoviesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var popularMovies = await _apiService.GetPopularMoviesAsync();
            PopularMovies.Clear();
            foreach (var movie in popularMovies)
            {
                PopularMovies.Add(movie);
            }
        }
        catch(Exception ex)
        {
            ErrorMessage = $"Error loading popular movies: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadMoviesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            int? genreId = null;
            if (!string.IsNullOrEmpty(SelectedGenre) && _genreIds.TryGetValue(SelectedGenre, out var foundGenreId))
            {
                genreId = foundGenreId;
            }

            string sortBy = "popularity.desc";
            if (!string.IsNullOrEmpty(SelectedSort) && _sortOptions.TryGetValue(SelectedSort, out var foundSort))
            {
                sortBy = foundSort;
            }

            var movies = await _apiService.GetMoviesAsync(
                query: SearchQuery, 
                genreId: genreId, 
                sortBy: sortBy
            );

            Movies.Clear();
            foreach (var movie in movies)
            {
                Movies.Add(movie);
            }

            IsEmpty = Movies.Count == 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading movies: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}