using PROJECT.Models;
using PROJECT.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace PROJECT.ViewModels
{
    public class MoodEntryViewModel : BaseViewModel
    {
        private readonly LocalDbService _localDbService;
        private readonly FirebaseAuthService _authService;
        private readonly SyncService _syncService;
        private readonly FirebaseStorageService _storageService; // 1. Add field

        private Mood _selectedMood = Mood.Good;
        private string _note = string.Empty;
        private string? _imagePath;

        // 2. Inject FirebaseStorageService
        public MoodEntryViewModel(LocalDbService localDbService,
                                  FirebaseAuthService authService,
                                  SyncService syncService,
                                  FirebaseStorageService storageService)
        {
            _localDbService = localDbService;
            _authService = authService;
            _syncService = syncService;
            _storageService = storageService;
        }

        // ... (SelectedMood, Note properties remain the same) ...
        public Mood SelectedMood
        {
            get => _selectedMood;
            set => SetProperty(ref _selectedMood, value);
        }

        public string Note
        {
            get => _note;
            set
            {
                SetProperty(ref _note, value);
                OnPropertyChanged(nameof(CharCount));
            }
        }

        public int CharCount => _note?.Length ?? 0;

        public string? ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public ICommand SelectMoodCommand => new Command<Mood>(m => SelectedMood = m);
        public ICommand UploadImageCommand => new Command(async () => await UploadImage());

        public ICommand SubmitCommand => new Command(async () =>
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId = _authService.CurrentUserId;
                if (string.IsNullOrEmpty(userId))
                {
                    var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (mainPage != null) await mainPage.DisplayAlert("Error", "You must be logged in.", "OK");
                    return;
                }

                // 3. Handle Image Upload (if an image was picked)
                string finalImagePath = ImagePath ?? string.Empty;

                if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                {
                    // Ensure we have a unique name so we don't overwrite others
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImagePath)}";

                    using (var stream = File.OpenRead(ImagePath))
                    {
                        // Upload and get the Web URL
                        finalImagePath = await _storageService.UploadImageAsync(stream, fileName);
                    }
                }

                var entry = new JournalEntry
                {
                    UserId = userId,
                    Mood = SelectedMood,
                    Date = DateTime.Now,
                    Summary = Note,
                    ImagePath = finalImagePath, // This is now a URL (https://...)
                    IsSynced = false,
                    LastUpdated = DateTime.UtcNow
                };

                await _localDbService.CreateEntry(entry);
                await Shell.Current.GoToAsync("..");

                _ = Task.Run(async () => await _syncService.PushDataAsync());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving/uploading: {ex.Message}");
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage != null) await mainPage.DisplayAlert("Error", "Failed to save entry. Check internet.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        });

        private async Task UploadImage()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a photo",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    // Save locally FIRST for preview purposes
                    var newFile = Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                    using (var stream = await result.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                    }

                    // Update the UI to show the selected image
                    ImagePath = newFile;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image pick failed: {ex.Message}");
            }
        }
    }
}