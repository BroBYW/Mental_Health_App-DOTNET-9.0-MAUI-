using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace PROJECT.Services
{
    public class FirebaseAuthService
    {
        // Use the key from Secrets.cs
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
                // We pass "" (empty string) as the display name initially
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

        public async Task<string> LoginAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);

                CurrentUserId = userCredential.User.Uid;
                var token = await userCredential.User.GetIdTokenAsync();

                // Only save session if the user checked the box
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
            // Safety Check: Prevent crash if library thinks no one is logged in
            if (_authClient.User != null)
            {
                _authClient.SignOut();
            }

            // Always clear local app session
            CurrentUserId = null;
            SecureStorage.Default.Remove("auth_token");
            SecureStorage.Default.Remove("user_id");
        }

        // --- NEW METHODS FOR PROFILE MANAGEMENT ---

        public Firebase.Auth.User? GetCurrentUser()
        {
            return _authClient.User;
        }

        // Updates Display Name and Photo URL using Firebase REST API directly
        // because the .NET wrapper library is missing this specific method.
        public async Task UpdateUserProfileAsync(string displayName, string photoUrl)
        {
            var user = _authClient.User;
            if (user == null) return;

            try
            {
                // 1. Get the current user's ID token
                var token = await user.GetIdTokenAsync();

                // 2. Prepare the request
                string requestUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={WebApiKey}";

                var payload = new
                {
                    idToken = token,
                    displayName = displayName,
                    photoUrl = photoUrl,
                    returnSecureToken = true
                };

                // 3. Send the request
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync(requestUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[Profile Update Fail] {error}");
                    throw new Exception("Failed to update profile on server.");
                }

                // --- ADD THIS LINE ---
                // 4. Force a token refresh so the local 'user' object updates its Info immediately
                await user.GetIdTokenAsync(forceRefresh: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateUserProfileAsync Error] {ex.Message}");
                throw;
            }
        }

        // --- HELPER METHODS ---

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