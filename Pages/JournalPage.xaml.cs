using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class JournalPage : ContentPage
    {
        private JournalViewModel _vm;

        public JournalPage(JournalViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Refresh list from DB
            await _vm.LoadDataAsync();
        }
    }
}