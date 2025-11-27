using PROJECT.Services;
using Microsoft.Maui.Controls;
using PROJECT.Pages;
using Microsoft.Maui.Networking;
using System.Threading.Tasks;

namespace PROJECT
{
    public partial class App : Application
    {
        private readonly SyncService _syncService;
        private readonly FirebaseAuthService _authService;

        // ✅ ADD THIS PROPERTY: Allows other pages/viewmodels to access Auth safely
        public FirebaseAuthService AuthService => _authService;

        public App(SyncService syncService, FirebaseAuthService authService)
        {
            InitializeComponent();
            _syncService = syncService;
            _authService = authService;

            // Listen for internet connection changes
            Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginPage = activationState!.Context.Services.GetService<LoginPage>();
            return new Window(new NavigationPage(loginPage));
        }

        // ADDED: Perform initialization here instead
        protected override void OnStart()
        {
            base.OnStart();

            Task.Run(async () =>
            {
                // 1. Restore the session
                await _authService.InitializeAsync();

                // 2. CHECK: If user is logged in, switch to the main app immediately
                if (!string.IsNullOrEmpty(_authService.CurrentUserId))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Replace the Login Page with the App Shell
                        if (Application.Current?.Windows.FirstOrDefault() is Window window)
                        {
                            window.Page = new AppShell();
                        }
                    });
                }

                // 3. Existing Sync Logic...
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    await _syncService.PushDataAsync();
                    await _syncService.PullDataAsync();
                }
            });
        }

        private async void Current_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                System.Diagnostics.Debug.WriteLine("Internet connection restored. Starting sync...");
                await _syncService.PushDataAsync();
                await _syncService.PullDataAsync();
            }
        }
    }
}