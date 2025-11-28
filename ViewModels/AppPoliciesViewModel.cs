using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace PROJECT.ViewModels
{
    public class AppPoliciesViewModel : BaseViewModel
    {
        public AppPoliciesViewModel()
        {
            Title = "App Policies";
        }

        // Command to go back
        public ICommand CloseCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("..");
        });
    }
}