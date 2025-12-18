using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class ClinicPage : ContentPage
    {
        // The app calls this constructor and passes the registered ClinicViewModel
        public ClinicPage(ClinicViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}