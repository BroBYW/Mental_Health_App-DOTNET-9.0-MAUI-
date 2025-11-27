using Microsoft.Maui.Controls;
using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}