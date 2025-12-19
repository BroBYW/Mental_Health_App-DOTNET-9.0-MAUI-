using Microsoft.Maui.Controls;
using System.Threading.Tasks; // Added for Task
using System.Windows.Input;

namespace PROJECT.ViewModels
{
    public class AppPoliciesViewModel : BaseViewModel
    {
        public AppPoliciesViewModel()
        {
            Title = "App Policies";
            LoadPolicies(); // Trigger loading simulation
        }

        private async void LoadPolicies()
        {
            IsBusy = true;
            await Task.Delay(1500); // Simulate a 1.5s delay
            IsBusy = false;
        }

        // Command to go back
        public ICommand CloseCommand => new Command(async () =>
        {
            await Shell.Current.GoToAsync("..");
        });
    }
}