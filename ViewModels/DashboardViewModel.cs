using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using PROJECT.Models;
using PROJECT.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace PROJECT.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly LocalDbService _localDbService;
        private readonly FirebaseAuthService _authService;
        private readonly IDispatcherTimer? _quoteTimer;

        private readonly List<string> _quotes = new()
        {
            "Every day may not be good, but there is something good in every day.",
            "You are enough just as you are.",
            "Healing isn't linear. Be gentle with yourself.",
            "Your mental health is a priority. Your happiness is an essential. Your self-care is a necessity.",
            "It’s okay not to be okay.",
            "Small steps are still progress.",
            "Breathe. This is just a chapter, not the whole story.",
            "You don’t have to control your thoughts. You just have to stop letting them control you.",
            "Self-care is how you take your power back.",
            "One day at a time."
        };

        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<MoodPoint> Points { get; } = new();

        private int _selectedYear;
        private string _quote = string.Empty;
        private string _averageMood = "No Data";

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    _ = LoadDataAsync();
                }
            }
        }

        public string Quote
        {
            get => _quote;
            set => SetProperty(ref _quote, value);
        }

        public string AverageMood
        {
            get => _averageMood;
            set => SetProperty(ref _averageMood, value);
        }

        public DashboardViewModel(LocalDbService localDbService, FirebaseAuthService authService)
        {
            _localDbService = localDbService;
            _authService = authService;

            // --- CHANGED: Populate last 10 years instead of 3 ---
            var year = DateTime.Today.Year;
            for (var y = year - 9; y <= year; y++)
            {
                Years.Add(y);
            }

            SelectedYear = year;

            if (Application.Current != null)
            {
                _quoteTimer = Application.Current.Dispatcher.CreateTimer();
                _quoteTimer.Interval = TimeSpan.FromSeconds(30);
                _quoteTimer.Tick += (s, e) => UpdateQuote();
                _quoteTimer.Start();
            }

            UpdateQuote();
        }

        private void UpdateQuote()
        {
            if (_quotes.Count > 0)
            {
                var random = new Random();
                Quote = _quotes[random.Next(_quotes.Count)];
            }
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
                    Points.Clear();
                    AverageMood = "No Data";
                    return;
                }

                var allEntries = await _localDbService.GetEntries(userId);

                Points.Clear();

                var yearlyEntries = allEntries
                    .Where(e => e.Date.Year == SelectedYear)
                    .OrderByDescending(e => e.Date)
                    .Take(7)
                    .Reverse()
                    .ToList();

                foreach (var entry in yearlyEntries)
                {
                    Points.Add(new MoodPoint
                    {
                        Day = entry.Date,
                        Value = (int)entry.Mood
                    });
                }

                if (yearlyEntries.Any())
                {
                    double avgScore = yearlyEntries.Average(e => (int)e.Mood);
                    int roundedScore = (int)Math.Round(avgScore);
                    Mood avgMoodEnum = (Mood)roundedScore;

                    string emoji = avgMoodEnum switch
                    {
                        Mood.Rad => "😀",
                        Mood.Good => "🙂",
                        Mood.Meh => "😐",
                        Mood.Bad => "🙁",
                        Mood.Awful => "☹️",
                        _ => "🙂"
                    };

                    AverageMood = $"Avg: {avgMoodEnum} {emoji}";
                }
                else
                {
                    AverageMood = "No Data";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
                AverageMood = "Error";
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(Points));
            }
        }

        public ICommand NewEntryCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("moodEntry");
        });
    }
}