using MovieTracker.UI.Services;
using MovieTracker.UI.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MovieTracker.UI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _auth;

        private string _email = string.Empty;
        private string _password = string.Empty;

        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        public ICommand GoBackCommand { get; }


        public LoginViewModel(AuthService auth)
        {
            _auth = auth;
            LoginCommand = new Command(async () => await LoginAsync());
            GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(RegisterPage)));
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Введіть email і пароль.";
                return;
            }

            IsBusy = true;
            ErrorMessage = null;

            try
            {
                ErrorMessage = await _auth.LoginAsync(Email.Trim(), Password);

                if (ErrorMessage is null)
                    await Shell.Current.Navigation.PopToRootAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
