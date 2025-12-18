using PROJECT.Services;
using PROJECT.Models;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System;

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

            // Initial load from cached Auth data
            LoadFromAuth();
        }

        // Helper to load basic info from the logged-in user
        private void LoadFromAuth()
        {
            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                // Only set if we don't already have values (or if resetting)
                if (string.IsNullOrEmpty(UserName) || UserName == "User")
                    UserName = user.Info.DisplayName ?? "User";

                if (string.IsNullOrEmpty(Email) || Email == "user@example.com")
                    Email = user.Info.Email;

                // Always try to grab the Auth photo if current is empty
                if (string.IsNullOrEmpty(ProfileImage))
                    ProfileImage = user.Info.PhotoUrl;
            }
        }

        public async Task LoadUserProfileAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId = _authService.CurrentUserId;
                if (string.IsNullOrEmpty(userId)) return;

                // 1. Fetch fresh data from Realtime DB
                var dbProfile = await _syncService.GetUserProfileAsync(userId);

                if (dbProfile != null)
                {
                    // Update Username if present
                    if (!string.IsNullOrEmpty(dbProfile.Username))
                        UserName = dbProfile.Username;

                    // Update Email if present
                    if (!string.IsNullOrEmpty(dbProfile.Email))
                        Email = dbProfile.Email;

                    // Update Photo: 
                    // If DB has a photo, use it. 
                    // If DB has NO photo, but we currently have none, try falling back to Auth again.
                    if (!string.IsNullOrEmpty(dbProfile.PhotoUrl))
                    {
                        ProfileImage = dbProfile.PhotoUrl;
                    }
                    else if (string.IsNullOrEmpty(ProfileImage))
                    {
                        // DB has no photo, and we have no photo -> Check Auth one last time
                        LoadFromAuth();
                    }
                }
                else
                {
                    // 2. No DB profile found? Ensure we at least show Auth data
                    LoadFromAuth();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Profile Load Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand ClinicCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("clinics");
        });

        public ICommand GoToEditCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("editProfile");
        });

        public ICommand PoliciesCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("policies");
        });
    }
}