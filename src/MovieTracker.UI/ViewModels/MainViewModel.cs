using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MovieTracker.Core.Models;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private string _searchQuery = string.Empty;
    private bool _isLoading;

    public ObservableCollection<Movie> Movies { get; set; } = new();

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
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand SearchCommand { get; }

    public MainViewModel(ApiService apiService)
    {
        _apiService = apiService;
        SearchCommand = new Command(async () => await LoadMoviesAsync());

        _ = LoadMoviesAsync();
    }

    public async Task LoadMoviesAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var movies = await _apiService.GetMoviesAsync(SearchQuery);
            Movies.Clear();
            foreach (var movie in movies)
            {
                Movies.Add(movie);
            }
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