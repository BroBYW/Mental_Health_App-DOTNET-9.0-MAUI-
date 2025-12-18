using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace PROJECT.Controls
{
    public class ClinicMap : Microsoft.Maui.Controls.Maps.Map
    {
        public static readonly BindableProperty CoordinatesProperty = BindableProperty.Create(
            nameof(Coordinates),
            typeof(Location),
            typeof(ClinicMap),
            null,
            propertyChanged: OnCoordinatesChanged);

        public static readonly BindableProperty ClinicNameProperty = BindableProperty.Create(
            nameof(ClinicName),
            typeof(string),
            typeof(ClinicMap),
            string.Empty);

        public Location Coordinates
        {
            get => (Location)GetValue(CoordinatesProperty);
            set => SetValue(CoordinatesProperty, value);
        }

        public string ClinicName
        {
            get => (string)GetValue(ClinicNameProperty);
            set => SetValue(ClinicNameProperty, value);
        }

        private static void OnCoordinatesChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ClinicMap map && newValue is Location location)
            {
                map.Pins.Clear();

                var pin = new Pin
                {
                    Label = map.ClinicName,
                    Location = location,
                    Type = PinType.Place
                };

                map.Pins.Add(pin);

                map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(500)));
            }
        }
    }
}