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
    private int? _selectedGenreId;
    private string _selectedSortBy = "popularity.desc";

    public ObservableCollection<MovieListItemDto> PopularMovies { get; } = new();
    public ObservableCollection<MovieListItemDto> Movies { get; } = new();

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

    public int? SelectedGenreId
    {
        get => _selectedGenreId;
        set
        {
            if (_selectedGenreId != value)
            {
                _selectedGenreId = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedSortBy
    {
        get => _selectedSortBy;
        set
        {
            if (_selectedSortBy != value)
            {
                _selectedSortBy = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SearchCommand { get; }
    public ICommand ApplyFiltersCommand { get; }

    public MainViewModel(ApiService apiService)
    {
        _apiService = apiService;
        SearchCommand = new Command(async () => await LoadMoviesAsync());
        ApplyFiltersCommand = new Command(async () => await LoadMoviesAsync());
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
            var movies = await _apiService.GetMoviesAsync(
                query: SearchQuery, 
                genreId: SelectedGenreId, 
                sortBy: SelectedSortBy
            );

            Movies.Clear();
            foreach (var movie in movies)
            {
                Movies.Add(movie);
            }
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