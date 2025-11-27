using PROJECT.ViewModels;

namespace PROJECT.Pages
{
    public partial class ChatPage : ContentPage
    {
        public ChatPage(ChatViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}