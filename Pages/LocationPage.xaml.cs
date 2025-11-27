using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps; // Required for Map interactions
using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class LocationPage : ContentPage
    {
        private readonly LocationViewModel _vm;

        public LocationPage(LocationViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;

            // Subscribe to ViewModel changes to move the map camera
            _vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When the ViewModel calculates a new region (centered on nearest clinic), move the map
            if (e.PropertyName == nameof(LocationViewModel.MapRegion))
            {
                if (_vm.MapRegion != null)
                {
                    ClinicMap.MoveToRegion(_vm.MapRegion);
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.PropertyChanged -= Vm_PropertyChanged;

            // Stop tracking GPS when user leaves this page to save battery
            _vm.StopTracking();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Restart tracking if needed (optional, depending on if StopTracking kills it permanently)
            _ = _vm.StartTrackingLocation();
        }
    }
}