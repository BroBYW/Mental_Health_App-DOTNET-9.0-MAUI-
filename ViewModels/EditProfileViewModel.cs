using PROJECT.Services;
using PROJECT.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace PROJECT.ViewModels
{
    public class EditProfileViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;
        private readonly FirebaseStorageService _storageService;
        private readonly SyncService _syncService;

        private string _userName = string.Empty;
        private string _email = string.Empty;
        private string? _profileImage;
        private string? _localImagePath;

        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string? ProfileImage { get => _profileImage; set => SetProperty(ref _profileImage, value); }

        public EditProfileViewModel(FirebaseAuthService authService, FirebaseStorageService storageService, SyncService syncService)
        {
            _authService = authService;
            _storageService = storageService;
            _syncService = syncService;
        }

        // Load data from DB to ensure we are editing current values
        public async Task LoadCurrentData()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId = _authService.CurrentUserId;
                if (string.IsNullOrEmpty(userId)) return;

                // 1. Try fetching from Database
                var dbProfile = await _syncService.GetUserProfileAsync(userId);

                // Always get Auth user as backup source of truth
                var authUser = _authService.GetCurrentUser();

                if (dbProfile != null)
                {
                    UserName = dbProfile.Username;

                    // FIX: If DB email is missing, grab it from Auth
                    if (!string.IsNullOrEmpty(dbProfile.Email))
                        Email = dbProfile.Email;
                    else if (authUser != null)
                        Email = authUser.Info.Email;

                    ProfileImage = dbProfile.PhotoUrl;
                }
                else
                {
                    // 2. Fallback to Auth Cache if DB is empty
                    if (authUser != null)
                    {
                        UserName = authUser.Info.DisplayName ?? "";
                        Email = authUser.Info.Email;
                        ProfileImage = authUser.Info.PhotoUrl;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand ChangeImageCommand => new Command(async () => await PickImage());

        public ICommand SaveCommand => new Command(async () =>
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Ensure we don't save nulls
                string finalPhotoUrl = ProfileImage ?? "";
                string finalName = UserName ?? "";

                // 1. Upload new image if a local file was picked
                if (!string.IsNullOrEmpty(_localImagePath))
                {
                    var userId = _authService.CurrentUserId;
                    var fileName = $"{userId}_profile_{DateTime.Now.Ticks}.jpg";
                    using (var stream = File.OpenRead(_localImagePath))
                    {
                        finalPhotoUrl = await _storageService.UploadImageAsync(stream, fileName, "profile_images");
                    }
                }

                // 2. Update Auth Profile
                await _authService.UpdateUserProfileAsync(finalName, finalPhotoUrl);

                // 3. Save to Realtime Database
                var currentUserId = _authService.CurrentUserId;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    // This will now save the correct Email (recovered from Auth) back to the DB
                    await _syncService.SaveProfileToDbAsync(currentUserId, finalName, Email, finalPhotoUrl);
                }

                await Shell.Current.DisplayAlert("Success", "Profile updated!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Profile Update Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to update profile.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        });

        private async Task PickImage()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select Profile Photo", FileTypes = FilePickerFileType.Images });

                if (result != null)
                {
                    var newFile = Path.Combine(FileSystem.CacheDirectory, result.FileName);

                    using (var stream = await result.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                    }

                    _localImagePath = newFile;
                    ProfileImage = newFile;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PickImage error: {ex.Message}");
            }
        }
    }
}