using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class ClinicPage : ContentPage
    {
        public ClinicPage(ClinicViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}