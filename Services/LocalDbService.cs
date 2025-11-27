using SQLite;
using PROJECT.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Storage;
using System;

namespace PROJECT.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "mood_journal.db3";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            _connection = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitAsync()
        {
            await _connection.CreateTableAsync<JournalEntry>();
        }

        public async Task<List<JournalEntry>> GetEntries(string userId)
        {
            await InitAsync();
            // CHANGED: Only fetch items that are NOT deleted
            return await _connection.Table<JournalEntry>()
                                    .Where(x => x.UserId == userId && x.IsDeleted == false)
                                    .OrderByDescending(x => x.Date)
                                    .ToListAsync();
        }

        public async Task<List<JournalEntry>> GetUnsyncedEntries(string userId)
        {
            await InitAsync();
            // We still fetch IsDeleted items here so SyncService can process them
            return await _connection.Table<JournalEntry>()
                                    .Where(x => x.IsSynced == false && x.UserId == userId)
                                    .ToListAsync();
        }

        // ... (Keep GetEntryByDate overloads as they are) ...

        public async Task CreateEntry(JournalEntry entry)
        {
            await InitAsync();
            await _connection.InsertAsync(entry);
        }

        public async Task UpdateEntry(JournalEntry entry)
        {
            await InitAsync();
            await _connection.UpdateAsync(entry);
        }

        // CHANGED: "Soft Delete" - Marks it as deleted but keeps it for Sync
        public async Task DeleteEntry(JournalEntry entry)
        {
            await InitAsync();
            entry.IsDeleted = true;
            entry.IsSynced = false; // Mark dirty so SyncService picks it up
            entry.LastUpdated = DateTime.UtcNow;
            await _connection.UpdateAsync(entry);
        }

        // NEW: "Hard Delete" - Actually removes row. Used by SyncService after successful upload.
        public async Task HardDeleteEntry(JournalEntry entry)
        {
            await InitAsync();
            await _connection.DeleteAsync(entry);
        }
    }
}