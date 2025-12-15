namespace Sensore.Infrastructure.Auth
{
    public static class SensoreRoles
    {
        public const string Admin = "Admin";
        public const string Clinician = "Clinician";
        public const string Patient = "Patient";
        public const string Manager = "Manager";
        public const string Doctor = "Doctor";

        public static readonly string[] All = {
            Admin,
            Clinician,
            Patient,
            Manager,
            Doctor
        };
    }
}
