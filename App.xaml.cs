using PROJECT.Services;
using Microsoft.Maui.Controls;
using PROJECT.Pages;
using Microsoft.Maui.Networking;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

namespace PROJECT
{
    public partial class App : Application
    {
        private readonly SyncService _syncService;
        private readonly FirebaseAuthService _authService;

        public FirebaseAuthService AuthService => _authService;

        public App(SyncService syncService, FirebaseAuthService authService)
        {
            InitializeComponent();
            _syncService = syncService;
            _authService = authService;

            Connectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginPage = activationState!.Context.Services.GetService<LoginPage>();
            return new Window(new NavigationPage(loginPage));
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // 1. CRITICAL: Authentication check must happen fast to show the correct page.
            await _authService.InitializeAsync();

            if (!string.IsNullOrEmpty(_authService.CurrentUserId))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Application.Current?.Windows.FirstOrDefault() is Window window)
                    {
                        window.Page = new AppShell();
                    }
                });
            }

            // 2. BACKGROUND TASK: Offload heavy lifting to prevent startup freeze (ANR).
            // We fire and forget this task so OnStart returns immediately.
            _ = Task.Run(async () =>
            {
                // A small delay gives the UI time to paint the first frame.
                await Task.Delay(1500);

                // Sync Data
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    await _syncService.PushDataAsync();
                    await _syncService.PullDataAsync();
                }

                // Schedule Notifications
                // We wrap this in MainThread because permission requests usually require the UI thread.
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await ScheduleNotifications();
                });
            });
        }

        private async Task ScheduleNotifications()
        {
            try
            {
                if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
                {
                    await LocalNotificationCenter.Current.RequestNotificationPermission();
                }

                // 1. Morning Check-in (8:00 AM)
                await ScheduleSingleNotification(
                    id: 100,
                    title: "Daily Check-in",
                    message: "Good morning! Ready to start your day with a mental health check-in?",
                    hour: 8,
                    minute: 0);

                // 2. Evening Reflection (8:00 PM)
                await ScheduleSingleNotification(
                    id: 101, // Different ID is crucial
                    title: "Evening Reflection",
                    message: "Time to unwind. How did your day go? Capture it in your journal.",
                    hour: 20,
                    minute: 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notification Error: {ex.Message}");
            }
        }

        private async Task ScheduleSingleNotification(int id, string title, string message, int hour, int minute)
        {
            var notifyTime = DateTime.Today.AddHours(hour).AddMinutes(minute);

            // If time has passed today, schedule for tomorrow
            if (notifyTime < DateTime.Now)
            {
                notifyTime = notifyTime.AddDays(1);
            }

            var request = new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = message,
                BadgeNumber = 1,
                Schedule =
                {
                    NotifyTime = notifyTime,
                    RepeatType = NotificationRepeat.Daily
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }

        private async void Current_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                System.Diagnostics.Debug.WriteLine("Internet connection restored. Starting sync...");
                // Run sync in background to avoid blocking UI during connectivity changes
                await Task.Run(async () =>
                {
                    await _syncService.PushDataAsync();
                    await _syncService.PullDataAsync();
                });
            }
        }
    }
}