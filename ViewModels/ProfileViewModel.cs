using PROJECT.Services;
using PROJECT.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace PROJECT.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;
        private readonly SyncService _syncService;

        private string _userName = "User";
        private string _email = "user@example.com";
        private string? _profileImage;

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string? ProfileImage
        {
            get => _profileImage;
            set => SetProperty(ref _profileImage, value);
        }

        public ProfileViewModel(FirebaseAuthService authService, SyncService syncService)
        {
            _authService = authService;
            _syncService = syncService;
        }

        public async Task LoadUserProfileAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId = _authService.CurrentUserId;
                if (string.IsNullOrEmpty(userId)) return;

                // 1. Try fetching fresh data from Realtime DB
                var dbProfile = await _syncService.GetUserProfileAsync(userId);

                if (dbProfile != null)
                {
                    UserName = dbProfile.Username;
                    Email = dbProfile.Email;

                    // ✅ FIX: Convert empty string to null so XAML TargetNullValue works
                    ProfileImage = string.IsNullOrEmpty(dbProfile.PhotoUrl) ? null : dbProfile.PhotoUrl;
                }
                else
                {
                    // 2. Fallback to Auth Data if DB is empty
                    var user = _authService.GetCurrentUser();
                    if (user != null)
                    {
                        UserName = user.Info.DisplayName ?? "User";
                        Email = user.Info.Email;

                        // ✅ FIX: Convert empty string to null here too
                        var photoUrl = user.Info.PhotoUrl;
                        ProfileImage = string.IsNullOrEmpty(photoUrl) ? null : photoUrl;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand GoToEditCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("editProfile");
        });

        public ICommand SettingsCommand => new Command(async () =>
        {
            // Navigate to the Settings Page
            await Shell.Current.GoToAsync("settings");
        });

        public ICommand PoliciesCommand => new Command(async () =>
        {
            await Shell.Current.DisplayAlert("Policies", "Privacy Policy & Terms coming soon.", "OK");
        });
    }
}