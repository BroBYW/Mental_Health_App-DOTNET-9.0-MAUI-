using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PROJECT.Pages;
using PROJECT.Services;

namespace PROJECT.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isBusy;

        // 1. ADD THIS FIELD
        private string _title = string.Empty;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // 2. ADD THIS PROPERTY
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
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

        public ICommand LogoutCommand => new Command(() =>
        {
            if (Application.Current is App app)
            {
                app.AuthService.SignOut();
            }

            var loginPage = Application.Current?.Handler?.MauiContext?.Services.GetService<LoginPage>();

            if (loginPage != null && App.Current?.Windows.FirstOrDefault() is Window win)
            {
                win.Page = new NavigationPage(loginPage);
            }
        });
    }
}