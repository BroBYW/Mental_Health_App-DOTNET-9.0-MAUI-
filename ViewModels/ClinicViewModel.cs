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
                Location = new Location(2.3106574382620853, 111.83108172906093)
            });

            Clinics.Add(new Clinic
            {
                Name = "Kelvin Lau Specialist Clinic",
                Address = "1st Floor, No. 7, Jalan Maju, 96000 Sibu",
                Image = "clinic_kelvin.jpg",
                Location = new Location(2.285997345837242, 111.83033869813882)
            });

            Clinics.Add(new Clinic
            {
                Name = "Hospital Sibu (Psychiatric Specialist)",
                Address = "Batu 5 1/2, Jalan Ulu Oya, 96000 Sibu",
                Image = "clinic_sibu.jpg",
                Location = new Location(2.2966182786366507, 111.89183131556717)
            });
        }

        public ICommand OpenMapCommand => new Command<Clinic>(async (clinic) =>
        {
            if (clinic == null) return;

            try
            {
                await Map.OpenAsync(new Placemark
                {
                    Thoroughfare = clinic.Address,
                    Locality = "Sibu",
                    AdminArea = "Sarawak",
                    CountryName = "Malaysia",
                    Location = clinic.Location
                },
                new MapLaunchOptions
                {
                    Name = clinic.Name,
                    NavigationMode = NavigationMode.Driving
                });
            }
            catch (Exception)
            {
                var query = Uri.EscapeDataString($"{clinic.Location.Latitude},{clinic.Location.Longitude}");
                var googleMapsUrl = $"https://www.google.com/maps/search/?api=1&query={query}";
                await Launcher.OpenAsync(new Uri(googleMapsUrl));
            }
        });
    }
}