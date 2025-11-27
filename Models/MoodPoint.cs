using System;

namespace PROJECT.Models
{
    public class MoodPoint
    {
        public DateTime Day { get; set; }
        public int Value { get; set; } // Represents Mood Enum value (1-5)
    }
}