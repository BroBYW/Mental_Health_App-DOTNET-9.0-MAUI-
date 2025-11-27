using PROJECT.Services;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace PROJECT.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;

        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isPasswordHidden = true;
        private bool _isLoading;

        // Renamed EmailOrPhone to Email for clarity
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        // Removed FullName and Username properties

        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
        public bool IsPasswordHidden { get => _isPasswordHidden; set => SetProperty(ref _isPasswordHidden, value); }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Helper to get the current main page safely and without using obsolete API
        private static Page? GetCurrentPage()
        {
            // For single-window apps, this is safe
            return Application.Current?.Windows.FirstOrDefault()?.Page;
        }

        public RegisterViewModel(FirebaseAuthService authService)
        {
            _authService = authService;
        }

        public ICommand TogglePasswordCommand => new Command(() => IsPasswordHidden = !IsPasswordHidden);

        public ICommand RegisterCommand => new Command(async () =>
        {
            if (IsLoading) return;

            var page = GetCurrentPage();
            if (page == null)
                return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await page.DisplayAlert("Error", "Please fill in all fields", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await page.DisplayAlert("Error", "Passwords do not match", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                await _authService.RegisterAsync(Email, Password);
                // Navigate to AppShell on success
                if (Application.Current?.Windows.FirstOrDefault() is Window window)
                {
                    window.Page = new AppShell();
                }
            }
            catch
            {
                await page.DisplayAlert("Error", "Registration failed. Email might be in use or invalid.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        });

        public ICommand LoginNavigateCommand => new Command(async () =>
        {
            var page = GetCurrentPage();
            if (page?.Navigation != null)
            {
                await page.Navigation.PopAsync();
            }
        });
    }
}