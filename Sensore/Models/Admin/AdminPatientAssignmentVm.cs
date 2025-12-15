using System.Collections.Generic;

namespace Sensore.Models.Admin
{
    /// ViewModel for the Admin patient-assignment page.
    /// Contains lists of selectable patients, clinicians and doctors and the current assignments.
    public class AdminPatientAssignmentVm
    {
        public List<UserItem> Patients { get; set; } = new();
        public List<UserItem> Clinicians { get; set; } = new();
        public List<UserItem> Doctors { get; set; } = new();

        /// Current assignments are domain entities (PatientAssignment).
        /// The view binds to this list to display existing assignments and provide edit/delete UI.
        public List<Sensore.Models.PatientAssignment> Assignments { get; set; } = new();
    }

    /// Minimal user reference used in admin selection lists.

    public class UserItem
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}