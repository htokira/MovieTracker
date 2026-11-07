using MovieTracker.UI.Services;
using MovieTracker.UI.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MovieTracker.UI.ViewModels
{
    public class AccountBarViewModel : BaseViewModel
    {
        private readonly AuthService _auth;

        public bool IsAuthenticated => _auth.IsAuthenticated;
        public bool IsGuest => !_auth.IsAuthenticated;

        public string AvatarInitial
        {
            get
            {
                var name = _auth.CurrentUser?.DisplayName;
                return string.IsNullOrWhiteSpace(name) ? "?" : char.ToUpper(name.Trim()[0]).ToString();
            }
        }

        public string? AvatarImage => _auth.CurrentUser?.AvatarUrl;

        public ICommand LoginCommand { get; }
        public ICommand OpenProfileCommand { get; }

        public AccountBarViewModel(AuthService auth)
        {
            _auth = auth;

            LoginCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(LoginPage)));
            OpenProfileCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(ProfilePage)));

            _auth.AuthStateChanged += OnAuthStateChanged;
        }

        private void OnAuthStateChanged()
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsGuest));
            OnPropertyChanged(nameof(AvatarInitial));
            OnPropertyChanged(nameof(AvatarImage));
        }
    }
}
