using PROJECT.ViewModels;
using PROJECT.Drawables;
using System.ComponentModel; // Needed for PropertyChangedEventArgs
using System.Linq;

namespace PROJECT.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _vm;

        public DashboardPage(DashboardViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Subscribe to events so we know when data changes
            _vm.PropertyChanged += OnViewModelPropertyChanged;

            // Initial load
            await _vm.LoadDataAsync();
            UpdateChart();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Unsubscribe to prevent memory leaks
            _vm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // This runs whenever any property in the ViewModel changes (like SelectedYear or Points)
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Only redraw if the "Points" collection was updated
            if (e.PropertyName == nameof(DashboardViewModel.Points))
            {
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            // Give the new data to the Drawable and force a redraw
            ChartView.Drawable = new MoodChartDrawable { Points = _vm.Points.ToList() };
            ChartView.Invalidate();
        }
    }
}