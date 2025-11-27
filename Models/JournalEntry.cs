using SQLite;
using System;
using System.Windows.Input; // Required for ICommand

namespace PROJECT.Models
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        public Mood Mood { get; set; }
        public string? Summary { get; set; }
        public string? ImagePath { get; set; }
        public bool IsSynced { get; set; } = false;
        public DateTime LastUpdated { get; set; }
        public bool IsDeleted { get; set; } = false;

        [Ignore]
        public int MoodScore => (int)Mood;

        // [NEW] Direct command reference for the UI
        [Ignore]
        public ICommand? DeleteCommand { get; set; }
    }
}