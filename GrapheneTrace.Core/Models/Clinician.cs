using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    [Table("Clinicians")]
    public class Clinician
    {
        [Key]
        [ForeignKey("User")]
        public int ClinicianID { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ClinicianPatient> ClinicianPatients { get; set; } = new List<ClinicianPatient>();
        public virtual ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    }
}