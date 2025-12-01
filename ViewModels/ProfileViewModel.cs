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

            // Load cached info immediately so the user sees something while we fetch fresh data
            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                UserName = user.Info.DisplayName ?? "User";
                Email = user.Info.Email;
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

                // 1. Try fetching fresh data from Realtime DB
                var dbProfile = await _syncService.GetUserProfileAsync(userId);

                if (dbProfile != null)
                {
                    UserName = dbProfile.Username;

                    // FIX: Only overwrite Email if the DB actually has one. 
                    // Otherwise, keep the Auth email we loaded in the constructor.
                    if (!string.IsNullOrEmpty(dbProfile.Email))
                    {
                        Email = dbProfile.Email;
                    }
                    else
                    {
                        // Fallback: If DB email is empty, double-check Auth
                        var user = _authService.GetCurrentUser();
                        if (user != null) Email = user.Info.Email;
                    }

                    // Only update if we have a valid URL
                    if (!string.IsNullOrEmpty(dbProfile.PhotoUrl))
                    {
                        ProfileImage = dbProfile.PhotoUrl;
                    }
                }
                else
                {
                    // 2. Fallback to Auth Data if NO db profile exists
                    if (string.IsNullOrEmpty(ProfileImage))
                    {
                        var user = _authService.GetCurrentUser();
                        if (user != null)
                        {
                            UserName = user.Info.DisplayName ?? "User";
                            Email = user.Info.Email;

                            if (!string.IsNullOrEmpty(user.Info.PhotoUrl))
                            {
                                ProfileImage = user.Info.PhotoUrl;
                            }
                        }
                    }
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