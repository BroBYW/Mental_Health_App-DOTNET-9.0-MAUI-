using System.Collections.ObjectModel;
using System.Windows.Input;
using PROJECT.Services;
using Microsoft.Maui.ApplicationModel;

namespace PROJECT.ViewModels
{
    public class Clinic
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        // 👇 Add this property
        public Location Location { get; set; }
    }

    public class ClinicViewModel : BaseViewModel
    {
        public ObservableCollection<Clinic> Clinics { get; } = new();

        public ClinicViewModel()
        {
            Title = "Mental Health Clinics";
            LoadClinics();
        }

        private void LoadClinics()
        {
            Clinics.Add(new Clinic
            {
                Name = "Eddy Su Specialist Clinic",
                Address = "No. 8 (GF), Taman Damai, Jalan Tun Abang Haji Openg, 96000 Sibu",
                Image = "clinic_eddy.jpg",
                Location = new Location(2.3105, 111.8319) // 👈 Added Coordinates
            });

            Clinics.Add(new Clinic
            {
                Name = "Kelvin Lau Specialist Clinic",
                Address = "1st Floor, No. 7, Jalan Maju, 96000 Sibu",
                Image = "clinic_kelvin.jpg",
                Location = new Location(2.2882, 111.8316) // 👈 Added Coordinates
            });

            Clinics.Add(new Clinic
            {
                Name = "Hospital Sibu (Psychiatric Specialist)",
                Address = "Batu 5 1/2, Jalan Ulu Oya, 96000 Sibu",
                Image = "clinic_sibu.jpg",
                Location = new Location(2.2969, 111.8925) // 👈 Added Coordinates
            });
        }

        public ICommand OpenMapCommand => new Command<Clinic>(async (clinic) =>
        {
            if (clinic == null) return;

            try
            {
                // Try to open the native map app
                await Map.OpenAsync(new Placemark
                {
                    Thoroughfare = clinic.Address,
                    Locality = "Sibu",
                    AdminArea = "Sarawak",
                    CountryName = "Malaysia"
                },
                new MapLaunchOptions
                {
                    Name = clinic.Name,
                    NavigationMode = NavigationMode.Driving
                });
            }
            catch (Exception)
            {
                // Fallback: Open Google Maps in Browser
                // Use the standard search URL
                var query = Uri.EscapeDataString($"{clinic.Name} {clinic.Address}");
                var googleMapsUrl = $"https://www.google.com/maps/search/?api=1&query={query}";

                await Launcher.OpenAsync(new Uri(googleMapsUrl));
            }
        });
    }
}