using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private ProfileViewModel _vm;

        public ProfilePage(ProfileViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Call the async method
            await _vm.LoadUserProfileAsync();
        }
    }
}