using System.Collections.ObjectModel;
using PROJECT.Models;

namespace PROJECT.ViewModels
{
    public class ClinicViewModel : BaseViewModel
    {
        public ObservableCollection<Clinic> Clinics { get; } = new();

        public ClinicViewModel()
        {
            LoadClinics();
        }

        private void LoadClinics()
        {
            Clinics.Add(new Clinic
            {
                Name = "Eddy Su Specialist Clinic\n蘇熙善心理精神專科診所",
                Address = "No. 8 (GF), Taman Damai, Jalan Tun Abang Haji Openg, 96000 Sibu",
                Image = "clinic_eddy.jpg"
            });

            Clinics.Add(new Clinic
            {
                Name = "Kelvin Lau Specialist Clinic\n刘会建心理精神专科诊所",
                Address = "1st Floor, No. 7, Jalan Maju, 96000 Sibu",
                Image = "clinic_kelvin.jpg"
            });

            Clinics.Add(new Clinic
            {
                Name = "Hospital Sibu (Psychiatric Specialist Clinic)",
                Address = "Batu 5 1/2, Jalan Ulu Oya, 96000 Sibu",
                Image = "clinic_sibu.jpg"
            });
        }
    }
}