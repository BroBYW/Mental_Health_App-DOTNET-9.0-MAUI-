using Microsoft.Maui.Devices.Sensors;

namespace PROJECT.Models
{
    public class Clinic
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Display properties
        public string DistanceDisplay { get; set; } = string.Empty;
        public double DistanceKm { get; set; } // Used for sorting

        // Helper for the Map Control
        public Location Location => new Location(Latitude, Longitude);
    }
}