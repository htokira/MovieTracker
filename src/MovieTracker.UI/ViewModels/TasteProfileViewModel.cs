using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MovieTracker.Core.Dtos;
using MovieTracker.UI.Models;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class TasteProfileViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private readonly TasteProfileInterpreter _interpreter;

    private TasteProfileDto? _profile;
    private TasteProfileInterpretation? _interpretation;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public event PropertyChangedEventHandler? PropertyChanged;

    public TasteProfileDto? Profile
    {
        get => _profile;
        set
        {
            _profile = value;
            OnPropertyChanged();
        }
    }

    public TasteProfileInterpretation? Interpretation
    {
        get => _interpretation;
        set
        {
            _interpretation = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasProfile => Profile != null;

    public ICommand LoadProfileCommand { get; }

    public TasteProfileViewModel(
        ApiService apiService,
        TasteProfileInterpreter interpreter)
    {
        _apiService = apiService;
        _interpreter = interpreter;

        LoadProfileCommand = new Command(async () => await LoadProfileAsync());
    }

    public async Task LoadProfileAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Profile = await _apiService.GetTasteProfileAsync();

            if (Profile == null)
            {
                Interpretation = null;
                ErrorMessage = "Поки що немає даних для аналізу. Оціни декілька фільмів, щоб сформувати свій смаковий профіль.";
                OnPropertyChanged(nameof(HasProfile));
                return;
            }

            Interpretation = _interpreter.Interpret(Profile);

            OnPropertyChanged(nameof(HasProfile));
        }
        catch (Exception)
        {
            ErrorMessage = "Не вдалося завантажити смаковий профіль. Спробуй пізніше.";
            Interpretation = null;
            Profile = null;

            OnPropertyChanged(nameof(HasProfile));
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}