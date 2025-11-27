using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class MoodEntryPage : ContentPage
    {
        // Inject the ViewModel directly
        public MoodEntryPage(MoodEntryViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}