using Firebase.Database;
using Firebase.Database.Query;
using PROJECT.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using System.Collections.Generic;

namespace PROJECT.Services
{
    public class SyncService
    {
        private const string BaseUrl = "https://mad-mental-default-rtdb.asia-southeast1.firebasedatabase.app/";

        private readonly LocalDbService _localDb;
        private readonly FirebaseAuthService _authService;
        private FirebaseClient? _firebaseClient;

        public SyncService(LocalDbService localDb, FirebaseAuthService authService)
        {
            _localDb = localDb;
            _authService = authService;
        }

        private void InitFirebase()
        {
            if (_firebaseClient == null)
            {
                _firebaseClient = new FirebaseClient(
                    BaseUrl,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => _authService.GetStoredTokenAsync()
                    });
            }
        }

        // --- NEW: Save Profile to Realtime Database ---
        public async Task SaveProfileToDbAsync(string userId, string name, string email, string photoUrl)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

            InitFirebase();

            try
            {
                // Create the profile object
                var profileData = new UserProfile
                {
                    Username = name,
                    Email = email,
                    PhotoUrl = photoUrl
                };

                // Save to: users/{userId}/profile
                await _firebaseClient!
                    .Child("users")
                    .Child(userId)
                    .Child("profile")
                    .PutAsync(profileData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB Profile Save Error]: {ex.Message}");
                throw;
            }
        }

        // --- NEW: Get Profile from Realtime Database ---
        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return null;

            InitFirebase();

            try
            {
                // Fetch from: users/{userId}/profile
                var profile = await _firebaseClient!
                    .Child("users")
                    .Child(userId)
                    .Child("profile")
                    .OnceSingleAsync<UserProfile>();

                return profile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Get Profile Error]: {ex.Message}");
                return null;
            }
        }

        // --- EXISTING: Push Journal Entries ---
        public async Task PushDataAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

            var userId = _authService.CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return;

            InitFirebase();

            var dirtyEntries = await _localDb.GetUnsyncedEntries(userId);

            foreach (var entry in dirtyEntries)
            {
                try
                {
                    var cloudItems = await _firebaseClient!
                        .Child("users")
                        .Child(userId)
                        .Child("journal")
                        .OnceAsync<JournalEntry>();

                    var match = cloudItems.FirstOrDefault(c => Math.Abs((c.Object.Date - entry.Date).TotalSeconds) < 1);

                    // Handle Deletion
                    if (entry.IsDeleted)
                    {
                        if (match != null)
                        {
                            await _firebaseClient!
                                .Child("users")
                                .Child(userId)
                                .Child("journal")
                                .Child(match.Key)
                                .DeleteAsync();
                        }
                        await _localDb.HardDeleteEntry(entry);
                        continue;
                    }

                    // Handle Update/Create
                    if (match != null)
                    {
                        await _firebaseClient!
                            .Child("users")
                            .Child(userId)
                            .Child("journal")
                            .Child(match.Key)
                            .PutAsync(entry);
                    }
                    else
                    {
                        await _firebaseClient!
                            .Child("users")
                            .Child(userId)
                            .Child("journal")
                            .PostAsync(entry);
                    }

                    entry.IsSynced = true;
                    await _localDb.UpdateEntry(entry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Push Error]: {ex.Message}");
                }
            }
        }

        // --- EXISTING: Pull Journal Entries ---
        public async Task PullDataAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

            var userId = _authService.CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return;

            InitFirebase();

            try
            {
                var cloudItems = await _firebaseClient!
                    .Child("users")
                    .Child(userId)
                    .Child("journal")
                    .OnceAsync<JournalEntry>();

                var localEntries = await _localDb.GetEntries(userId);

                foreach (var item in cloudItems)
                {
                    var cloudEntry = item.Object;
                    cloudEntry.UserId = userId;
                    cloudEntry.IsSynced = true;

                    var localEntry = localEntries.FirstOrDefault(x => Math.Abs((x.Date - cloudEntry.Date).TotalSeconds) < 1);

                    if (localEntry == null)
                    {
                        await _localDb.CreateEntry(cloudEntry);
                    }
                    else
                    {
                        // Last Write Wins logic
                        if (cloudEntry.LastUpdated > localEntry.LastUpdated)
                        {
                            cloudEntry.Id = localEntry.Id;
                            await _localDb.UpdateEntry(cloudEntry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Pull Error]: {ex.Message}");
            }
        }
    }
}