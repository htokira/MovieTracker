using MovieTracker.Core.Dtos;
using MovieTracker.UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace MovieTracker.UI.ViewModels
{
    public class RecommendationsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;

        private bool _isLoading;
        private bool _isSectionVisible;
        private bool _isPersonalized;
        private bool _wasGuest;
        private string? _errorMessage;

        public RecommendationsViewModel(ApiService api)
        {
            _api = api;
            RetryCommand = new Command(async () => await LoadAsync());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MovieListItemDto> Movies { get; } = new();

        public ICommand RetryCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                    OnPropertyChanged(nameof(ShowLoader));
            }
        }

        public bool IsSectionVisible
        {
            get => _isSectionVisible;
            private set => SetProperty(ref _isSectionVisible, value);
        }

        public bool IsPersonalized
        {
            get => _isPersonalized;
            private set
            {
                if (SetProperty(ref _isPersonalized, value))
                    OnPropertyChanged(nameof(Subtitle));
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool HasMovies => Movies.Count > 0;

        public bool ShowLoader => IsLoading && !HasMovies;

        public string Subtitle => IsPersonalized
            ? "Підібрано на основі ваших оцінок"
            : "Оцініть кілька фільмів, і добірка стане персональною";

        public async Task LoadAsync()
        {
            if (IsLoading)
                return;

            if (!_wasGuest)
                IsSectionVisible = true;

            ErrorMessage = null;
            IsLoading = true;

            try
            {
                var (response, error) = await _api.GetMyRecommendationsAsync();

                if (error != null)
                {
                    if (!HasMovies)
                        ErrorMessage = error;
                    return;
                }

                if (response is null)
                {
                    _wasGuest = true;
                    Movies.Clear();
                    IsSectionVisible = false;
                    return;
                }

                _wasGuest = false;
                IsPersonalized = response.IsPersonalized;

                Movies.Clear();
                foreach (var movie in response.Items)
                    Movies.Add(movie);

                IsSectionVisible = HasMovies;
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasMovies));
                OnPropertyChanged(nameof(ShowLoader));
            }
        }

        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
