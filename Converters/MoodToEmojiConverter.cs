using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using PROJECT.Models;

namespace PROJECT.Converters
{
    public class MoodToEmojiConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Mood mood)
            {
                return mood switch
                {
                    Mood.Rad => "😀",
                    Mood.Good => "🙂",
                    Mood.Meh => "😐",
                    Mood.Bad => "🙁",
                    Mood.Awful => "☹️",
                    _ => "🙂"
                };
            }
            return "🙂";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}