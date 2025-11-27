using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using PROJECT.Models;

namespace PROJECT.Converters
{
    public class MoodToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Mood mood)
            {
                return mood switch
                {
                    Mood.Rad => "rad.png",
                    Mood.Good => "good.png",
                    Mood.Meh => "meh.png",
                    Mood.Bad => "bad.png",
                    Mood.Awful => "awful.png",
                    _ => "moods.png"
                };
            }
            return "moods.png";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}