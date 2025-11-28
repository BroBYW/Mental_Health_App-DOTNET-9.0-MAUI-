using Microsoft.Extensions.Logging;
using PROJECT.Services;
using PROJECT.ViewModels;
using PROJECT.Pages;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Plugin.LocalNotification;

namespace PROJECT
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // 2. ADD THIS LINE HERE:
                .UseMauiMaps()
                .UseLocalNotification()
                .UseSkiaSharp() // You can keep this if you still use Skia elsewhere
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<FirebaseAuthService>();
            builder.Services.AddSingleton<SyncService>();
            builder.Services.AddSingleton<FirebaseStorageService>();

            // 1. Register the AI Service
            builder.Services.AddSingleton<OpenAIService>();

            // 2. Register the Chat ViewModel and Page
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ChatPage>();

            // ViewModels & Pages
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<MoodEntryViewModel>();
            builder.Services.AddTransient<MoodEntryPage>();
            builder.Services.AddTransient<JournalViewModel>();
            builder.Services.AddTransient<JournalPage>();
            builder.Services.AddTransient<LocationViewModel>();
            builder.Services.AddTransient<LocationPage>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<ProfilePage>();     
            builder.Services.AddTransient<EditProfileViewModel>();
            builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<AppPoliciesViewModel>();
            builder.Services.AddTransient<AppPoliciesPage>();

            return builder.Build();
        }
    }
}