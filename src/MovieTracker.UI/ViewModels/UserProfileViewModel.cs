using System.Windows.Input;
using MovieTracker.UI.Services;
using System.Collections.ObjectModel;
using MovieTracker.Core.Dtos;

namespace MovieTracker.UI.ViewModels
{
    public class UserProfileViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly AuthService _auth;
        private Guid _userId;

        private string _displayName = string.Empty;
        private string? _bio;
        private string _createdText = string.Empty;
        private string _avatarInitial = "?";
        private string? _avatarImage;
        private int _followersCount;
        private int _followingCount;
        private bool _isFollowedByMe;

        public string DisplayName { get => _displayName; private set => SetProperty(ref _displayName, value); }
        public string CreatedText { get => _createdText; private set => SetProperty(ref _createdText, value); }
        public string AvatarInitial { get => _avatarInitial; private set => SetProperty(ref _avatarInitial, value); }
        public string? AvatarImage { get => _avatarImage; private set => SetProperty(ref _avatarImage, value); }

        public string? Bio
        {
            get => _bio;
            private set
            {
                if (SetProperty(ref _bio, value))
                    OnPropertyChanged(nameof(HasBio));
            }
        }

        public bool HasBio => !string.IsNullOrWhiteSpace(Bio);

        public int FollowersCount { get => _followersCount; private set => SetProperty(ref _followersCount, value); }
        public int FollowingCount { get => _followingCount; private set => SetProperty(ref _followingCount, value); }

        public bool IsFollowedByMe
        {
            get => _isFollowedByMe;
            private set
            {
                if (SetProperty(ref _isFollowedByMe, value))
                    OnPropertyChanged(nameof(FollowButtonText));
            }
        }

        public string FollowButtonText => IsFollowedByMe ? "Відписатися" : "Підписатися";
                public ObservableCollection<UserReviewItemResponse> Reviews { get; } = new();
        public ObservableCollection<CollectionSummaryResponse> Collections { get; } = new();

        private int _reviewsTotal;
        public int ReviewsTotal
        {
            get => _reviewsTotal;
            private set
            {
                if (SetProperty(ref _reviewsTotal, value))
                    OnPropertyChanged(nameof(ReviewsTitle));
            }
        }

        public string ReviewsTitle => $"Відгуки ({ReviewsTotal})";

        private bool _hasReviews;
        public bool HasReviews { get => _hasReviews; private set => SetProperty(ref _hasReviews, value); }

        private bool _hasCollections;
        public bool HasCollections { get => _hasCollections; private set => SetProperty(ref _hasCollections, value); }

        // Кнопку показуємо тільки авторизованим і не на власному профілі
        public bool CanFollow => _auth.IsAuthenticated && _auth.CurrentUser?.Id != _userId;

        public ICommand ToggleFollowCommand { get; }
        public ICommand GoBackCommand { get; }

        public UserProfileViewModel(ApiService api, AuthService auth)
        {
            _api = api;
            _auth = auth;

            ToggleFollowCommand = new Command(async () => await ToggleFollowAsync());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task LoadAsync(Guid userId)
        {
            _userId = userId;
            OnPropertyChanged(nameof(CanFollow));

            if (IsBusy) return;
            ErrorMessage = null;
            IsBusy = true;

            try
            {
                var profile = await _api.GetUserProfileAsync(userId);
                if (profile is null)
                {
                    ErrorMessage = "Користувача не знайдено або сервер недоступний.";
                    return;
                }

                DisplayName = profile.DisplayName;
                Bio = profile.Bio;
                AvatarImage = profile.AvatarUrl;
                CreatedText = $"З нами з {profile.CreatedAt:dd.MM.yyyy}";
                AvatarInitial = string.IsNullOrWhiteSpace(profile.DisplayName)
                    ? "?"
                    : char.ToUpper(profile.DisplayName.Trim()[0]).ToString();

                await LoadStatsAsync();
                await LoadReviewsAsync();
                await LoadCollectionsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadStatsAsync()
        {
            var stats = await _api.GetFollowStatsAsync(_userId);
            if (stats is null) return;

            FollowersCount = stats.FollowersCount;
            FollowingCount = stats.FollowingCount;
            IsFollowedByMe = stats.IsFollowedByMe;
        }
                private async Task LoadReviewsAsync()
        {
            var result = await _api.GetUserReviewsAsync(_userId);

            Reviews.Clear();
            if (result is null)
            {
                ReviewsTotal = 0;
                HasReviews = false;
                return;
            }

            foreach (var item in result.Items)
                Reviews.Add(item);

            ReviewsTotal = result.TotalCount;
            HasReviews = Reviews.Count > 0;
        }

        private async Task LoadCollectionsAsync()
        {
            var result = await _api.GetUserCollectionsAsync(_userId);

            Collections.Clear();
            if (result is not null)
            {
                foreach (var item in result)
                    Collections.Add(item);
            }

            HasCollections = Collections.Count > 0;
        }
        private async Task ToggleFollowAsync()
        {
            if (IsBusy) return;
            ErrorMessage = null;
            IsBusy = true;

            try
            {
                var error = IsFollowedByMe
                    ? await _api.UnfollowUserAsync(_userId)
                    : await _api.FollowUserAsync(_userId);

                if (error is not null)
                {
                    ErrorMessage = error;
                    return;
                }

                await LoadStatsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}