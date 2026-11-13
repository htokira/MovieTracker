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
    private int _currentPage = 1;
    private int _totalPages = 1;

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
                OnPropertyChanged(nameof(HasError));
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
    public ICommand LoadMoreCommand { get; }
    public ICommand OpenMovieCommand { get; }

    public MainViewModel(ApiService apiService)
    {
        _apiService = apiService;
        SearchCommand = new Command(async () => await ResetAndLoadMoviesAsync());
        ApplyFiltersCommand = new Command(async () => await ResetAndLoadMoviesAsync());
        GenreOptions = new ObservableCollection<string>(_genreIds.Keys);
        SortOptions = new ObservableCollection<string>(_sortOptions.Keys);
        LoadMoreCommand = new Command(async () => await LoadMoreMoviesAsync());
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
        await ResetAndLoadMoviesAsync();
    }

    public async Task LoadPopularMoviesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var pagedResult = await _apiService.GetPopularMoviesAsync(page: 1);
            PopularMovies.Clear();
            foreach (var movie in pagedResult.Items)
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

    public async Task ResetAndLoadMoviesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var (genreId, sortBy) = GetCurrentFilters();

            var pagedResult = await _apiService.GetMoviesAsync(
                query: SearchQuery,
                page: _currentPage,
                genreId: genreId,
                sortBy: sortBy
            );

            Movies.Clear();
            foreach (var movie in pagedResult.Items)
            {
                Movies.Add(movie);
            }

            _totalPages = pagedResult.TotalPages;
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

    private async Task LoadMoreMoviesAsync()
    {
        if (IsLoading || _currentPage >= _totalPages) return;

        IsLoading = true;

        try
        {
            _currentPage++;
            var (genreId, sortBy) = GetCurrentFilters();

            var pagedResult = await _apiService.GetMoviesAsync(
                query: SearchQuery,
                page: _currentPage,
                genreId: genreId,
                sortBy: sortBy
            );

            foreach (var movie in pagedResult.Items)
            {
                Movies.Add(movie);
            }

            _totalPages = pagedResult.TotalPages;
        }
        catch (Exception ex)
        {
            _currentPage--;
            ErrorMessage = $"Error loading more movies: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private (int? GenreId, string SortBy) GetCurrentFilters()
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

        return (genreId, sortBy);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}