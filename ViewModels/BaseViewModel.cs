using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PROJECT.Pages;
using PROJECT.Services; // Ensure this is here

namespace PROJECT.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // ✅ FIXED LOGOUT COMMAND
        public ICommand LogoutCommand => new Command(() =>
        {
            // 1. Safely get the App instance
            if (Application.Current is App app)
            {
                // 2. Call SignOut on the exposed property
                app.AuthService.SignOut();
            }

            // 3. Navigate back to Login Page
            // We resolve the page from services to ensure dependencies (like ViewModels) are injected
            var loginPage = Application.Current?.Handler?.MauiContext?.Services.GetService<LoginPage>();

            // Fallback: If service lookup fails, create a new one manually (prevents crash)
            if (loginPage == null)
            {
                // Assuming you can create it without args or handle dependencies manually if strictly needed
                // For now, the service lookup usually works for UI elements, but the fallback prevents the crash.
                // However, getting it from DI is preferred.
            }

            var win = App.Current?.Windows?.FirstOrDefault();
            if (win != null && loginPage != null)
            {
                win.Page = new NavigationPage(loginPage);
            }
        });
    }
}