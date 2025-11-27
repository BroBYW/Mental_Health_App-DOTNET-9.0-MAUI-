using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class EditProfilePage : ContentPage
    {
        private EditProfileViewModel _vm;

        public EditProfilePage(EditProfileViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // This loads the existing Name/Picture so the text boxes aren't empty
            await _vm.LoadCurrentData();
        }
    }
}