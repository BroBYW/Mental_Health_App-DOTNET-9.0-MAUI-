using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using PROJECT.Models;
using PROJECT.Services;
using System.Linq;
using System; // Required for Math

namespace PROJECT.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        // CHANGED: Service type
        private readonly OpenAIService _aiService;
        private string _userInput = string.Empty;
        private bool _isTyping;

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value);
        }

        public bool IsTyping
        {
            get => _isTyping;
            set => SetProperty(ref _isTyping, value);
        }

        // CHANGED: Constructor Injection
        public ChatViewModel(OpenAIService aiService)
        {
            _aiService = aiService;

            Messages.Add(new ChatMessage
            {
                Text = "Hello! I'm powered by ChatGPT. How are you feeling today?",
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }

        public ICommand SendCommand => new Command(async () => await SendMessage());

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(UserInput)) return;

            var text = UserInput;
            UserInput = string.Empty;

            var userMsg = new ChatMessage { Text = text, IsUser = true, Timestamp = DateTime.Now };
            Messages.Add(userMsg);

            IsTyping = true;

            // Pass context window
            var history = Messages.Skip(Math.Max(0, Messages.Count - 6)).Take(5).ToList();

            // CHANGED: Call the new service
            var response = await _aiService.GetChatResponseAsync(history, text);

            Messages.Add(new ChatMessage
            {
                Text = response,
                IsUser = false,
                Timestamp = DateTime.Now
            });

            IsTyping = false;
        }
    }
}