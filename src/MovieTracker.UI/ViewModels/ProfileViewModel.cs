using MovieTracker.UI.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MovieTracker.UI.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _auth;

        // Шапка профілю
        private string _headerName = string.Empty;
        private string _email = string.Empty;
        private string _createdText = string.Empty;
        private string _avatarInitial = "?";
        private string? _avatarImage;

        // Поля редагування
        private string _displayName = string.Empty;
        private string? _avatarUrl;
        private string? _bio;

        // Зміна пароля
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmNewPassword = string.Empty;

        private string? _statusMessage;

        public string HeaderName { get => _headerName; private set => SetProperty(ref _headerName, value); }
        public string Email { get => _email; private set => SetProperty(ref _email, value); }
        public string CreatedText { get => _createdText; private set => SetProperty(ref _createdText, value); }
        public string AvatarInitial { get => _avatarInitial; private set => SetProperty(ref _avatarInitial, value); }
        public string? AvatarImage { get => _avatarImage; private set => SetProperty(ref _avatarImage, value); }

        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public string? AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }
        public string? Bio { get => _bio; set => SetProperty(ref _bio, value); }

        public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
        public string ConfirmNewPassword { get => _confirmNewPassword; set => SetProperty(ref _confirmNewPassword, value); }

        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (SetProperty(ref _statusMessage, value))
                    OnPropertyChanged(nameof(HasStatus));
            }
        }

        public bool HasStatus => !string.IsNullOrWhiteSpace(StatusMessage);

        public ICommand SaveProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand GoBackCommand { get; }

        public ProfileViewModel(AuthService auth)
        {
            _auth = auth;

            SaveProfileCommand = new Command(async () => await SaveProfileAsync());
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            Load();
        }

        private void Load()
        {
            var user = _auth.CurrentUser;
            if (user is null)
                return;

            HeaderName = user.DisplayName;
            Email = user.Email;
            CreatedText = $"Дата реєстрації: {user.CreatedAt:dd.MM.yyyy}";
            AvatarInitial = string.IsNullOrWhiteSpace(user.DisplayName)
                ? "?"
                : char.ToUpper(user.DisplayName.Trim()[0]).ToString();
            AvatarImage = user.AvatarUrl;

            DisplayName = user.DisplayName;
            AvatarUrl = user.AvatarUrl;
            Bio = user.Bio;
        }

        private async Task SaveProfileAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            var name = DisplayName.Trim();
            if (name.Length is < 2 or > 50)
            {
                ErrorMessage = "Ім’я має містити від 2 до 50 символів.";
                return;
            }

            IsBusy = true;
            try
            {
                var avatar = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim();
                var bio = string.IsNullOrWhiteSpace(Bio) ? null : Bio.Trim();

                var error = await _auth.UpdateProfileAsync(name, avatar, bio);
                if (error is null)
                {
                    Load();
                    StatusMessage = "Профіль збережено.";
                }
                else
                {
                    ErrorMessage = error;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ChangePasswordAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            if (string.IsNullOrEmpty(CurrentPassword))
            {
                ErrorMessage = "Введіть поточний пароль.";
                return;
            }

            if (NewPassword.Length < 8)
            {
                ErrorMessage = "Новий пароль має містити щонайменше 8 символів.";
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "Паролі не збігаються.";
                return;
            }

            IsBusy = true;
            try
            {
                var error = await _auth.ChangePasswordAsync(CurrentPassword, NewPassword);
                if (error is not null)
                {
                    ErrorMessage = error;
                    return;
                }
                await _auth.LogoutAsync();
                await Shell.Current.Navigation.PopToRootAsync();
                await Shell.Current.DisplayAlertAsync("Пароль змінено", "Увійдіть знову з новим паролем.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LogoutAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                await _auth.LogoutAsync();
                await Shell.Current.Navigation.PopToRootAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteAccountAsync()
        {
            if (IsBusy) return;
            ClearMessages();

            var confirmed = await Shell.Current.DisplayAlertAsync("Видалити акаунт?", "Цю дію не можна скасувати. Усі ваші відгуки теж буде видалено.", "Видалити", "Скасувати");

            if (!confirmed)
                return;

            IsBusy = true;
            try
            {
                var error = await _auth.DeleteAccountAsync();
                if (error is not null)
                {
                    ErrorMessage = error;
                    return;
                }

                await Shell.Current.Navigation.PopToRootAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearMessages()
        {
            ErrorMessage = null;
            StatusMessage = null;
        }
    }
}
