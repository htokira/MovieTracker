using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MovieTracker.Core.Dtos;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class MovieDetailsViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;

    private MovieDetailDto? _movie;
    private bool _isLoading;
    private string? _errorMessage = string.Empty;

    public MovieDetailDto? Movie
    {
        get => _movie;
        set
        {
            if (_movie != value)
            {
                _movie = value;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public MovieDetailsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task LoadMovieAsync(int movieId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Movie = await _apiService.GetMovieDetailsAsync(movieId);

            if (Movie == null)
                ErrorMessage = "Не вдалося знайти інформацію про фільм.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не вдалося завантажити інформацію про фільм.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}