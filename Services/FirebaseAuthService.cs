using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace PROJECT.Services
{
    public class FirebaseAuthService
    {
        // Use the key from Secrets.cs (or your hardcoded key if you haven't created Secrets.cs yet)
        private const string WebApiKey = Secrets.FirebaseApiKey;

        private readonly FirebaseAuthClient _authClient;

        public string? CurrentUserId { get; private set; }

        public FirebaseAuthService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = WebApiKey,
                AuthDomain = "mad-mental.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };

            _authClient = new FirebaseAuthClient(config);
        }

        // Call this from App.xaml.cs to restore the user session on startup
        public async Task InitializeAsync()
        {
            try
            {
                var token = await SecureStorage.Default.GetAsync("auth_token");
                var userId = await SecureStorage.Default.GetAsync("user_id");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                {
                    CurrentUserId = userId;
                    // Optional: You could validate the token expiry here if needed
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth Initialization Failed: {ex.Message}");
            }
        }

        public async Task<string> RegisterAsync(string email, string password)
        {
            try
            {
                // We pass "" (empty string) as the display name since we aren't collecting a username anymore
                var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password, displayName: "");

                CurrentUserId = userCredential.User.Uid;
                var token = await userCredential.User.GetIdTokenAsync();

                // Save both Token and UserId
                await SaveSessionAsync(token, CurrentUserId);

                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration Failed: {ex.Message}");
                throw;
            }
        }

        // CHANGED: Added 'rememberMe' parameter
        public async Task<string> LoginAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

                CurrentUserId = userCredential.User.Uid;
                var token = await userCredential.User.GetIdTokenAsync();

                // CHANGED: Only save session if the user checked the box
                if (rememberMe)
                {
                    await SaveSessionAsync(token, CurrentUserId);
                }

                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login Failed: {ex.Message}");
                throw;
            }
        }

        public void SignOut()
        {
            // 1. Safety Check: Only call the library's SignOut if the library actually has a user.
            // This prevents the crash if _authClient.User is already null.
            if (_authClient.User != null)
            {
                _authClient.SignOut();
            }

            // 2. Always clear your local app session
            CurrentUserId = null;
            SecureStorage.Default.Remove("auth_token");
            SecureStorage.Default.Remove("user_id");
        }

        private async Task SaveSessionAsync(string token, string userId)
        {
            await SecureStorage.Default.SetAsync("auth_token", token);
            await SecureStorage.Default.SetAsync("user_id", userId);
        }

        public async Task<string> GetStoredTokenAsync()
        {
            return await SecureStorage.Default.GetAsync("auth_token") ?? string.Empty;
        }
    }
}