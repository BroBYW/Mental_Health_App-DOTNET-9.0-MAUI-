using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PROJECT.Models;
using PROJECT.Services;
using Microsoft.Maui.Controls;

namespace PROJECT.ViewModels
{
    public class JournalViewModel : BaseViewModel
    {
        private readonly LocalDbService _localDbService;
        private readonly FirebaseAuthService _authService;
        private readonly SyncService _syncService; // Added for sync

        private List<JournalEntry> _allEntries = new();

        public ObservableCollection<JournalEntry> Entries { get; } = new();
        public ObservableCollection<int> Years { get; } = new();

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    FilterAndSort();
                }
            }
        }

        private bool _isDateAscending = false;
        private bool _isMoodAscending = false;
        private string _currentSortType = "Date";

        // Inject SyncService directly into the constructor
        public JournalViewModel(LocalDbService localDbService, FirebaseAuthService authService, SyncService syncService)
        {
            _localDbService = localDbService;
            _authService = authService;
            _syncService = syncService;
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var userId = _authService.CurrentUserId;

                if (string.IsNullOrEmpty(userId))
                {
                    Entries.Clear();
                    return;
                }

                // Get entries from DB (Soft-deleted ones are filtered out by LocalDbService)
                var entries = await _localDbService.GetEntries(userId);

                // [IMPORTANT] Assign the Delete Command to each item so the UI can bind to it directly
                var deleteAction = DeleteEntryCommand;
                foreach (var entry in entries)
                {
                    entry.DeleteCommand = deleteAction;
                }

                _allEntries = entries.ToList();

                // Populate Years Picker
                Years.Clear();
                var entryYears = _allEntries.Select(e => e.Date.Year);
                var currentYear = DateTime.Now.Year;
                var defaultRange = Enumerable.Range(currentYear - 9, 10);

                var distinctYears = entryYears.Union(defaultRange)
                                               .Distinct()
                                               .OrderByDescending(y => y)
                                               .ToList();

                foreach (var y in distinctYears) Years.Add(y);

                // Set default year if needed
                if (SelectedYear == 0 && Years.Contains(currentYear))
                {
                    SelectedYear = currentYear;
                }
                else if (SelectedYear == 0 && Years.Count > 0)
                {
                    SelectedYear = Years[0];
                }
                else
                {
                    // If year was already selected, just re-filter
                    FilterAndSort();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading journal: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ICommand SortDateCommand => new Command(() =>
        {
            _currentSortType = "Date";
            _isDateAscending = !_isDateAscending;
            FilterAndSort();
        });

        public ICommand SortMoodCommand => new Command(() =>
        {
            _currentSortType = "Mood";
            _isMoodAscending = !_isMoodAscending;
            FilterAndSort();
        });

        public ICommand DeleteEntryCommand => new Command<JournalEntry>(async (entry) =>
        {
            if (entry == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete Entry", "Are you sure you want to delete this entry?", "Yes", "No");
            if (!confirm) return;

            try
            {
                // 1. Soft Delete in Local DB (Sets IsDeleted = true)
                await _localDbService.DeleteEntry(entry);

                // 2. Remove from UI List immediately so the user sees it disappear
                Entries.Remove(entry);
                _allEntries.Remove(entry);

                // 3. Trigger Background Sync to delete from Firebase
                _ = Task.Run(async () => await _syncService.PushDataAsync());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Error: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Could not delete entry.", "OK");
            }
        });

        private void FilterAndSort()
        {
            var filtered = _allEntries.Where(e => e.Date.Year == SelectedYear);

            IEnumerable<JournalEntry> sorted;

            if (_currentSortType == "Mood")
            {
                sorted = _isMoodAscending
                    ? filtered.OrderBy(e => e.Mood)
                    : filtered.OrderByDescending(e => e.Mood);
            }
            else
            {
                sorted = _isDateAscending
                    ? filtered.OrderBy(e => e.Date)
                    : filtered.OrderByDescending(e => e.Date);
            }

            Entries.Clear();
            foreach (var entry in sorted)
            {
                Entries.Add(entry);
            }
        }

        public ICommand NewEntryCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("moodEntry");
        });
    }
}