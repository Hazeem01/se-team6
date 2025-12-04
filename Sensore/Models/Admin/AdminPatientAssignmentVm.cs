using System.Collections.Generic;

namespace Sensore.Models.Admin
{
    public class AdminPatientAssignmentVm
    {
        public List<UserItem> Patients { get; set; } = new();
        public List<UserItem> Clinicians { get; set; } = new();
        public List<UserItem> Doctors { get; set; } = new();

        public List<Sensore.Models.PatientAssignment> Assignments { get; set; } = new();
    }

    public class UserItem
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}