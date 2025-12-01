using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // Necessary for Permissions
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using PROJECT.Models;

namespace PROJECT.ViewModels
{
    public class LocationViewModel : BaseViewModel
    {
        private string _address = "Waiting for location...";
        private Location? _userLocation;

        // Map focus region
        private MapSpan? _mapRegion;
        public MapSpan? MapRegion
        {
            get => _mapRegion;
            set => SetProperty(ref _mapRegion, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        // [FIX] New property to control Map's IsShowingUser safely
        private bool _isLocationPermitted;
        public bool IsLocationPermitted
        {
            get => _isLocationPermitted;
            set => SetProperty(ref _isLocationPermitted, value);
        }

        public ObservableCollection<Clinic> Clinics { get; } = new();

        public LocationViewModel()
        {
            // Start tracking location immediately when ViewModel is created
            _ = StartTrackingLocation();
        }

        public async Task StartTrackingLocation()
        {
            try
            {
                // [FIX] 1. Explicitly request permission before starting
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                // [FIX] 2. Only enable map location if granted
                if (status == PermissionStatus.Granted)
                {
                    IsLocationPermitted = true; // Safe to enable the blue dot on the map now
                }
                else
                {
                    Address = "Permission denied. Unable to show location.";
                    return; // Stop execution if permission is denied
                }

                // 3. Configure Tracking Request
                var request = new GeolocationListeningRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));

                // 4. Define Event Handler for when location changes
                Geolocation.LocationChanged += async (s, e) =>
                {
                    var newLocation = e.Location;

                    // Update logic only if we moved more than 50 meters (0.05 km) 
                    // or if this is the first location found
                    if (_userLocation == null ||
                        Location.CalculateDistance(_userLocation, newLocation, DistanceUnits.Kilometers) > 0.05)
                    {
                        _userLocation = newLocation;
                        Address = $"My Location: {newLocation.Latitude:F4}, {newLocation.Longitude:F4}";

                        // Recalculate distance to Sibu clinics based on new position
                        await FindNearbyClinics(newLocation);
                    }
                };

                // 5. Start Listening
                await Geolocation.StartListeningForegroundAsync(request);
            }
            catch (Exception ex)
            {
                Address = "Location Error: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error tracking location: {ex.Message}");
            }
        }

        private async Task FindNearbyClinics(Location userLocation)
        {
            // Run the calculation and sorting on a background thread
            // to prevent blocking the UI if the list gets large.
            var sortedList = await Task.Run(() =>
            {
                // 1. Sibu Clinic Data (Real Coordinates)
                var rawList = new List<Clinic>
                {
                    new Clinic {
                        Name = "MENTARI Sibu (Community Mental Health)",
                        Address = "Poliklinik Kesihatan Oya, Jalan Oya",
                        Latitude = 2.290,
                        Longitude = 111.832
                    },
                    new Clinic {
                        Name = "Klinik Kesihatan Jalan Oya",
                        Address = "Jalan Oya / Jalan Brother Albinus",
                        Latitude = 2.290,
                        Longitude = 111.832
                    },
                    new Clinic {
                        Name = "Kelvin Lau Specialist Clinic",
                        Address = "7, Jalan Maju, Pekan Sibu",
                        Latitude = 2.291,
                        Longitude = 111.830
                    },
                    new Clinic {
                        Name = "Klinik Nur Sejahtera Sibu",
                        Address = "No. 27, Jalan Chew Sik Hiong",
                        Latitude = 2.292,
                        Longitude = 111.834
                    }
                };

                // 2. Calculate Distance
                foreach (var clinic in rawList)
                {
                    double dist = Location.CalculateDistance(userLocation, clinic.Location, DistanceUnits.Kilometers);
                    clinic.DistanceKm = dist;

                    if (dist < 1)
                        clinic.DistanceDisplay = $"{dist * 1000:F0} m";
                    else
                        clinic.DistanceDisplay = $"{dist:F2} km";
                }

                // 3. Sort by Nearest
                return rawList.OrderBy(x => x.DistanceKm).ToList();
            });

            // 4. Update UI (Back on the main thread)
            Clinics.Clear();
            foreach (var clinic in sortedList)
            {
                Clinics.Add(clinic);
            }

            // 5. Move Map Camera to the NEAREST clinic
            if (Clinics.Count > 0)
            {
                var nearest = Clinics[0];
                MapRegion = MapSpan.FromCenterAndRadius(nearest.Location, Distance.FromKilometers(2));
            }
        }

        // Call this from OnDisappearing in code-behind to save battery
        public void StopTracking()
        {
            Geolocation.StopListeningForeground();
        }
    }
}