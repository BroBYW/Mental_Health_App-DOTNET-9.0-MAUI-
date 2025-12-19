using PROJECT.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace PROJECT.Pages
{
    public partial class ClinicPage : ContentPage
    {
        public ClinicPage(ClinicViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Check and request location permission to ensure the map loads correctly
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            // If permission is granted, nudge the map engine to refresh
            if (status == PermissionStatus.Granted)
            {
                // Accessing the map by its x:Name from XAML
                // This property trigger often forces the map tiles to load if they were stuck
                // MyMap.IsShowingUser = true; 
            }
        }
    }
}