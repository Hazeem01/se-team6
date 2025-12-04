using System;
using Microsoft.AspNetCore.Identity;

namespace Sensore.Models
{
    public class PatientAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string PatientId { get; set; } = string.Empty;
        public IdentityUser? Patient { get; set; }

        public string? ClinicianId { get; set; }
        public IdentityUser? Clinician { get; set; }

        public string? DoctorId { get; set; }
        public IdentityUser? Doctor { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}