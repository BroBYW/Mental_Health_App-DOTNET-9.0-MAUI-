using System;
using System.Globalization;
using Microsoft.Maui.Controls;

// IMPORTANT: This namespace must match your folder structure
namespace PROJECT.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}