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

        // Field is named _localDb
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

        public async Task PushDataAsync()
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

            var userId = _authService.CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return;

            InitFirebase();

            // FIXED: Used _localDb instead of _localDbService
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

                    // [NEW LOGIC] Handle Deletion
                    if (entry.IsDeleted)
                    {
                        if (match != null)
                        {
                            // Delete from Firebase
                            await _firebaseClient!
                                .Child("users")
                                .Child(userId)
                                .Child("journal")
                                .Child(match.Key)
                                .DeleteAsync();
                        }

                        // Now that it's gone from Cloud, remove it permanently from Local DB
                        // FIXED: Used _localDb instead of _localDbService
                        await _localDb.HardDeleteEntry(entry);
                        continue; // Skip the rest of the loop for this item
                    }

                    // [EXISTING LOGIC] Handle Update/Create
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
                    // FIXED: Used _localDb instead of _localDbService
                    await _localDb.UpdateEntry(entry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Push Error]: {ex.Message}");
                }
            }
        }

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

                // FIXED: Used _localDb instead of _localDbService
                var localEntries = await _localDb.GetEntries(userId);

                foreach (var item in cloudItems)
                {
                    var cloudEntry = item.Object;
                    cloudEntry.UserId = userId;
                    cloudEntry.IsSynced = true;

                    var localEntry = localEntries.FirstOrDefault(x => Math.Abs((x.Date - cloudEntry.Date).TotalSeconds) < 1);

                    if (localEntry == null)
                    {
                        // FIXED: Used _localDb
                        await _localDb.CreateEntry(cloudEntry);
                    }
                    else
                    {
                        // Last Write Wins logic
                        if (cloudEntry.LastUpdated > localEntry.LastUpdated)
                        {
                            cloudEntry.Id = localEntry.Id;
                            // FIXED: Used _localDb
                            await _localDb.UpdateEntry(cloudEntry);
                            System.Diagnostics.Debug.WriteLine($"Synced: Updated {cloudEntry.Date} from Cloud.");
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