namespace Sensore.Models.Admin
{
    public class AdminDashboardVm
    {
        public int TotalUsers { get; set; }

        public int AdminCount { get; set; }
        public int ClinicianCount { get; set; }
        public int DoctorCount { get; set; }
        public int ManagerCount { get; set; }
        public int PatientCount { get; set; }

        public int TotalPatients { get; set; }
        public int ActiveAlerts { get; set; }
        public int ActiveClinicians { get; set; }
    }
}
