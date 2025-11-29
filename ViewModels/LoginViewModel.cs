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
            // 1. Get the current page reference for alerts
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage == null) return;

            // 2. Validate that an email has been entered
            if (string.IsNullOrWhiteSpace(EmailOrPhone))
            {
                await mainPage.DisplayAlert("Error", "Please enter your email address in the field above to reset your password.", "OK");
                return;
            }

            if (IsLoading) return;

            try
            {
                IsLoading = true;

                // 3. Call the service
                await _authService.ResetPasswordAsync(EmailOrPhone);

                // 4. Success Message
                await mainPage.DisplayAlert("Email Sent",
                    $"A password reset link has been sent to {EmailOrPhone}. Please check your inbox (and spam folder).",
                    "OK");
            }
            catch (Exception)
            {
                // 5. Error Message
                await mainPage.DisplayAlert("Error", "Failed to send reset email. Please ensure the email address is correct and registered.", "OK");
            }
            finally
            {
                IsLoading = false;
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