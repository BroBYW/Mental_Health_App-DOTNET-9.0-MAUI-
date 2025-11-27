using Microsoft.Maui.Controls;
using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}