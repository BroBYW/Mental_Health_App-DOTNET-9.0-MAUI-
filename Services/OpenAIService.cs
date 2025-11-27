using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace PROJECT.Services
{
    public class OpenAIService
    {
        // CHANGED: OpenAI Endpoint
        private const string BaseUrl = "https://api.openai.com/v1/chat/completions";
        private readonly HttpClient _httpClient;

        public OpenAIService()
        {
            _httpClient = new HttpClient();
            // CHANGED: Use the new key from Secrets
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {PROJECT.Secrets.OpenAiApiKey}");
        }

        public async Task<string> GetChatResponseAsync(List<Models.ChatMessage> history, string newUserMessage)
        {
            try
            {
                var messages = new List<object>
                {
                    new { role = "system", content = "You are a helpful and empathetic mental health companion." }
                };

                foreach (var msg in history)
                {
                    messages.Add(new { role = msg.IsUser ? "user" : "assistant", content = msg.Text });
                }

                messages.Add(new { role = "user", content = newUserMessage });

                // CHANGED: Model name (e.g., "gpt-3.5-turbo" or "gpt-4o")
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = messages,
                    temperature = 0.7
                };

                var response = await _httpClient.PostAsJsonAsync(BaseUrl, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    return doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "No response.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"OpenAI API Error: {error}");
                    return "I'm having trouble connecting to OpenAI. Please check your quota or key.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return "Connection error. Please try again.";
            }
        }
    }
}