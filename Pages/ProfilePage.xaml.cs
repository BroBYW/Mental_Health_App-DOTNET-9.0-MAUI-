using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _vm;

        public ProfilePage(ProfileViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        // THIS IS CRITICAL: The data must be fetched every time the page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // This forces the ViewModel to check the database/auth 
            // every time you look at the profile.
            if (_vm != null)
            {
                await _vm.LoadUserProfileAsync();
            }
        }
    }
}