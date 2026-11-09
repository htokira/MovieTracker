using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MovieTracker.Core.Dtos;
using MovieTracker.UI.Services;
using MovieTracker.UI.Views;

namespace MovieTracker.UI.ViewModels;

public class MovieDetailsViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private readonly AuthService _auth;

    private MovieDetailDto? _movie;
    private bool _isLoading;
    private string? _errorMessage = string.Empty;
    private MovieReviewsResponse? _reviews;
    private bool _isLoadingReviews;

    private int _movieId;
    private bool _isAuthenticated;
    private bool _hasMyReview;
    private int? _myRating;
    private string _myReviewText = string.Empty;
    private bool _isReviewBusy;
    private string? _reviewFormError;
    private string? _reviewFormStatus;

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

    public MovieReviewsResponse? Reviews
    {
        get => _reviews;
        set
        {
            if (_reviews != value)
            {
                _reviews = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasReviews));
                OnPropertyChanged(nameof(HasNoReviews));
            }
        }
    }

    public bool IsLoadingReviews
    {
        get => _isLoadingReviews;
        set
        {
            if (_isLoadingReviews != value)
            {
                _isLoadingReviews = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoReviews));
            }
        }
    }

    public bool HasReviews => Reviews?.Items?.Count > 0;

    public bool HasNoReviews => !IsLoadingReviews && Reviews != null && Reviews.TotalReviews == 0;

    // Власний відгук

    public int[] RatingOptions { get; } = Enumerable.Range(1, 10).ToArray();

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (SetProperty(ref _isAuthenticated, value))
                OnPropertyChanged(nameof(IsGuest));
        }
    }

    public bool IsGuest => !IsAuthenticated;

    public bool HasMyReview
    {
        get => _hasMyReview;
        private set
        {
            if (SetProperty(ref _hasMyReview, value))
            {
                OnPropertyChanged(nameof(ReviewFormTitle));
                OnPropertyChanged(nameof(SaveReviewButtonText));
            }
        }
    }

    public string ReviewFormTitle => HasMyReview ? "Ваш відгук" : "Залишити відгук";
    public string SaveReviewButtonText => HasMyReview ? "Зберегти зміни" : "Опублікувати";

    public int? MyRating { get => _myRating; set => SetProperty(ref _myRating, value); }
    public string MyReviewText { get => _myReviewText; set => SetProperty(ref _myReviewText, value); }

    public bool IsReviewBusy
    {
        get => _isReviewBusy;
        private set
        {
            if (SetProperty(ref _isReviewBusy, value))
                OnPropertyChanged(nameof(IsReviewNotBusy));
        }
    }

    public bool IsReviewNotBusy => !IsReviewBusy;

    public string? ReviewFormError
    {
        get => _reviewFormError;
        private set
        {
            if (SetProperty(ref _reviewFormError, value))
                OnPropertyChanged(nameof(HasReviewFormError));
        }
    }

    public bool HasReviewFormError => !string.IsNullOrWhiteSpace(ReviewFormError);

    public string? ReviewFormStatus
    {
        get => _reviewFormStatus;
        private set
        {
            if (SetProperty(ref _reviewFormStatus, value))
                OnPropertyChanged(nameof(HasReviewFormStatus));
        }
    }

    public bool HasReviewFormStatus => !string.IsNullOrWhiteSpace(ReviewFormStatus);

    public ICommand SaveReviewCommand { get; }
    public ICommand DeleteReviewCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MovieDetailsViewModel(ApiService apiService, AuthService auth)
    {
        _apiService = apiService;
        _auth = auth;

        SaveReviewCommand = new Command(async () => await SaveMyReviewAsync());
        DeleteReviewCommand = new Command(async () => await DeleteMyReviewAsync());
        GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(LoginPage)));
    }

    public async Task LoadMovieAsync(int movieId)
    {
        _movieId = movieId;
        _ = RefreshMyReviewAsync();

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Movie = await _apiService.GetMovieDetailsAsync(movieId);

            if (Movie == null)
                ErrorMessage = "Не вдалося знайти інформацію про фільм.";

            await LoadReviewsAsync(movieId);
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

    public async Task LoadReviewsAsync(int movieId)
    {
        try
        {
            IsLoadingReviews = true;
            Reviews = null;

            Reviews = await _apiService.GetMovieReviewsAsync(movieId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading reviews: {ex.Message}");
        }
        finally
        {
            IsLoadingReviews = false;
        }
    }

    public async Task RefreshMyReviewAsync()
    {
        if (_movieId <= 0) return;

        IsAuthenticated = _auth.IsAuthenticated;
        if (!IsAuthenticated)
        {
            ApplyMyReview(null);
            return;
        }

        if (IsReviewBusy) return;

        IsReviewBusy = true;
        try
        {
            var (review, error) = await _apiService.GetMyReviewAsync(_movieId);
            ReviewFormError = error;
            ApplyMyReview(review);
        }
        finally
        {
            IsReviewBusy = false;
        }
    }

    private async Task SaveMyReviewAsync()
    {
        if (IsReviewBusy || _movieId <= 0) return;
        ClearReviewMessages();

        if (MyRating is null)
        {
            ReviewFormError = "Оберіть оцінку від 1 до 10.";
            return;
        }

        IsReviewBusy = true;
        try
        {
            var text = string.IsNullOrWhiteSpace(MyReviewText) ? null : MyReviewText.Trim();

            var error = await _apiService.SaveMyReviewAsync(_movieId, MyRating.Value, text);
            if (error is not null)
            {
                ReviewFormError = error;
                return;
            }

            MyReviewText = text ?? string.Empty;
            HasMyReview = true;
            ReviewFormStatus = "Відгук збережено.";

            await LoadReviewsAsync(_movieId);
        }
        finally
        {
            IsReviewBusy = false;
        }
    }

    private async Task DeleteMyReviewAsync()
    {
        if (IsReviewBusy || _movieId <= 0) return;
        ClearReviewMessages();

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Видалити відгук?",
            "Вашу оцінку та відгук буде видалено.",
            "Видалити",
            "Скасувати");

        if (!confirmed)
            return;

        IsReviewBusy = true;
        try
        {
            var error = await _apiService.DeleteMyReviewAsync(_movieId);
            if (error is not null)
            {
                ReviewFormError = error;
                return;
            }

            ApplyMyReview(null);
            ReviewFormStatus = "Відгук видалено.";

            await LoadReviewsAsync(_movieId);
        }
        finally
        {
            IsReviewBusy = false;
        }
    }

    private void ApplyMyReview(ReviewResponse? review)
    {
        HasMyReview = review is not null;
        MyRating = review?.Rating;
        MyReviewText = review?.Text ?? string.Empty;
    }

    private void ClearReviewMessages()
    {
        ReviewFormError = null;
        ReviewFormStatus = null;
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}