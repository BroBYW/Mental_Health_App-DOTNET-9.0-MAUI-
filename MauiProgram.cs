using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Maps;
using Plugin.LocalNotification;
using PROJECT.Controls;
using PROJECT.Pages;
using PROJECT.Services;
using PROJECT.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Maui.Maps.Handlers;

namespace PROJECT
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>() // <--- THIS LINE IS CRITICAL. DO NOT DELETE.
                .UseMauiMaps()
                .UseLocalNotification()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                // 👇 ADD THIS BLOCK 👇
                .ConfigureMauiHandlers(handlers =>
                {
                    // This tells MAUI: "When you see ClinicMap, use the standard MapHandler"
                    handlers.AddHandler<ClinicMap, MapHandler>();
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Services
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddSingleton<FirebaseAuthService>();
            builder.Services.AddSingleton<SyncService>();
            builder.Services.AddSingleton<FirebaseStorageService>();
            builder.Services.AddSingleton<OpenAIService>();

            // ViewModels & Pages
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ChatPage>();

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

            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<ProfilePage>();

            builder.Services.AddTransient<EditProfileViewModel>();
            builder.Services.AddTransient<EditProfilePage>();

            builder.Services.AddTransient<AppPoliciesViewModel>();
            builder.Services.AddTransient<AppPoliciesPage>();

            // ... inside CreateMauiApp method, under // ViewModels & Pages ...

            builder.Services.AddTransient<ClinicViewModel>();
            builder.Services.AddTransient<ClinicPage>();

            return builder.Build();
        }
    }
}
