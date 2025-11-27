using PROJECT.Services;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PROJECT.Pages; // Needed to resolve RegisterPage

namespace PROJECT.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;

        private string _emailOrPhone = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;
        private bool _isPasswordHidden = true;
        private bool _isLoading;

        public string EmailOrPhone
        {
            get => _emailOrPhone;
            set => SetProperty(ref _emailOrPhone, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set => SetProperty(ref _isPasswordHidden, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public LoginViewModel(FirebaseAuthService authService)
        {
            _authService = authService;
        }

        public ICommand TogglePasswordCommand => new Command(() => IsPasswordHidden = !IsPasswordHidden);

        public ICommand ForgotPasswordCommand => new Command(async () =>
        {
            // Use the current window's page to avoid obsolete MainPage usage and null dereference
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert("Info", "Forgot Password feature coming soon.", "OK");
            }
        });

        public ICommand LoginCommand => new Command(async () =>
        {
            if (IsLoading) return;

            if (string.IsNullOrWhiteSpace(EmailOrPhone) || string.IsNullOrWhiteSpace(Password))
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Error", "Please enter email and password", "OK");
                }
                return;
            }

            try
            {
                IsLoading = true;
                // CHANGED: Passing the 'RememberMe' boolean property
                var token = await _authService.LoginAsync(EmailOrPhone, Password, RememberMe);

                if (!string.IsNullOrEmpty(token))
                {
                    // Successfully logged in, switch to main app Shell
                    var window = Application.Current?.Windows.FirstOrDefault();
                    if (window != null)
                    {
                        window.Page = new AppShell();
                    }
                }
            }
            catch
            {
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null)
                {
                    await mainPage.DisplayAlert("Error", "Login failed. Please check your credentials.", "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        });

        public ICommand SignUpNavigateCommand => new Command(async () =>
        {
            // FIX: Use Navigation Stack instead of Shell
            var registerPage = Application.Current?.Handler?.MauiContext?.Services.GetService<RegisterPage>();
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (registerPage != null && mainPage?.Navigation != null)
            {
                await mainPage.Navigation.PushAsync(registerPage);
            }
        });
    }
}