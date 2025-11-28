using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class AppPoliciesPage : ContentPage
    {
        public AppPoliciesPage(AppPoliciesViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}