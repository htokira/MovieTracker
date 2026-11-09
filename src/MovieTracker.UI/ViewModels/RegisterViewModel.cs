using MovieTracker.UI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Windows.Input;

namespace MovieTracker.UI.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _auth;

        private string _displayName = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;

        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel(AuthService auth)
        {
            _auth = auth;
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            var name = DisplayName.Trim();
            var email = Email.Trim();

            if (name.Length is < 2 or > 50)
            {
                ErrorMessage = "Ім’я має містити від 2 до 50 символів.";
                return;
            }

            if (!new EmailAddressAttribute().IsValid(email))
            {
                ErrorMessage = "Введіть коректний email.";
                return;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "Пароль має містити щонайменше 8 символів.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Паролі не збігаються.";
                return;
            }

            IsBusy = true;
            ErrorMessage = null;

            try
            {
                ErrorMessage = await _auth.RegisterAsync(email, Password, name);

                if (ErrorMessage is null)
                    await Shell.Current.GoToAsync("../..");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
