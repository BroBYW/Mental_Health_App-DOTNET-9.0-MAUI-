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
    public class QuoteItem
    {
        public string Image { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    // 1. NewsItem with initialized properties to fix warnings
    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class DashboardViewModel : BaseViewModel
    {
        private readonly LocalDbService _localDbService;
        private readonly FirebaseAuthService _authService;
        private readonly IDispatcherTimer? _quoteTimer;

        private readonly List<QuoteItem> _quotes = new()
        {
            new QuoteItem { Image = "quote1.png", Text = "Every day may not be good, but there is something good in every day." },
            new QuoteItem { Image = "quote2.png", Text = "You are enough just as you are." },
            new QuoteItem { Image = "quote3.png", Text = "Healing isn't linear. Be gentle with yourself." },
            new QuoteItem { Image = "quote4.png", Text = "Your mental health is a priority. Your happiness is an essential. Your self-care is a necessity." },
            new QuoteItem { Image = "quote5.png", Text = "It’s okay not to be okay." }
        };

        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<MoodPoint> Points { get; } = new();
        public ObservableCollection<NewsItem> NewsList { get; } = new();

        private int _selectedYear;
        private string _quote = string.Empty;
        private string _quoteImage = string.Empty;
        private string _averageMoodImage = "nodata.png";
        private string _averageMoodText = "No Data";
        private NewsItem? _selectedNewsItem; // Nullable backing field

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

        public string QuoteImage
        {
            get => _quoteImage;
            set => SetProperty(ref _quoteImage, value);
        }

        public string AverageMoodImage
        {
            get => _averageMoodImage;
            set => SetProperty(ref _averageMoodImage, value);
        }

        public string AverageMoodText
        {
            get => _averageMoodText;
            set => SetProperty(ref _averageMoodText, value);
        }

        // 2. SelectedNewsItem property handling navigation
        public NewsItem? SelectedNewsItem
        {
            get => _selectedNewsItem;
            set
            {
                if (_selectedNewsItem != value)
                {
                    _selectedNewsItem = value;
                    OnPropertyChanged();

                    // If a valid item was selected, navigate
                    if (_selectedNewsItem != null)
                    {
                        OpenNewsUrl(_selectedNewsItem.Url);
                        // Reset selection so the user can click it again later
                        SelectedNewsItem = null;
                    }
                }
            }
        }

        public DashboardViewModel(LocalDbService localDbService, FirebaseAuthService authService)
        {
            _localDbService = localDbService;
            _authService = authService;

            var year = DateTime.Today.Year;
            for (var y = year - 9; y <= year; y++)
            {
                Years.Add(y);
            }

            SelectedYear = year;

            if (Application.Current != null)
            {
                _quoteTimer = Application.Current.Dispatcher.CreateTimer();
                _quoteTimer.Interval = TimeSpan.FromSeconds(5);
                _quoteTimer.Tick += (s, e) => UpdateQuote();
                _quoteTimer.Start();
            }

            UpdateQuote();
            LoadNews();
        }

        private void LoadNews()
        {
            NewsList.Clear();
            NewsList.Add(new NewsItem
            {
                Title = "5 Ways to Improve Sleep",
                Summary = "Better sleep equals better mental health. Here are 5 tips.",
                ImageUrl = "news1.png",
                Url = "https://www.sleepfoundation.org/mental-health"
            });

            NewsList.Add(new NewsItem
            {
                Title = "The Power of Mindfulness",
                Summary = "How staying in the moment can reduce anxiety.",
                ImageUrl = "news2.jpg",
                Url = "https://www.mindful.org/"
            });

            NewsList.Add(new NewsItem
            {
                Title = "Exercise and Mood",
                Summary = "Why moving your body helps your brain.",
                ImageUrl = "news3.png",
                Url = "https://www.helpguide.org/articles/healthy-living/the-mental-health-benefits-of-exercise.htm"
            });
        }

        private void UpdateQuote()
        {
            if (_quotes.Count > 0)
            {
                var random = new Random();
                var selectedItem = _quotes[random.Next(_quotes.Count)];
                Quote = selectedItem.Text;
                QuoteImage = selectedItem.Image;
            }
        }

        private async void OpenNewsUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                // Navigate to the internal ArticlePage
                await Shell.Current.GoToAsync($"article?url={Uri.EscapeDataString(url)}");
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
                    AverageMoodText = "No Data";
                    AverageMoodImage = "nodata.png";
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

                    AverageMoodImage = avgMoodEnum switch
                    {
                        Mood.Awful => "emo1.png",
                        Mood.Bad => "emo2.png",
                        Mood.Meh => "emo3.png",
                        Mood.Good => "emo4.png",
                        Mood.Rad => "emo5.png",
                        _ => "emo3.png"
                    };

                    AverageMoodText = avgMoodEnum.ToString();
                }
                else
                {
                    AverageMoodText = "No Data";
                    AverageMoodImage = "nodata.png";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
                AverageMoodText = "Error";
                AverageMoodImage = "nodata.png";
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