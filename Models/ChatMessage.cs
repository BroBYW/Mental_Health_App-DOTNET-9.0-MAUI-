using System;

namespace PROJECT.Models // <--- This Namespace MUST match the XAML
{
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; }
    }
}