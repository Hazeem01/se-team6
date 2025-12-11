using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    [Table("Patients")]
    public class Patient
    {
        [Key]
        [ForeignKey("User")]
        public int PatientID { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;

        public DateTime? AdmissionDate { get; set; }

        [MaxLength(50)]
        public string? MedicalRecordNumber { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ClinicianPatient> ClinicianPatients { get; set; } = new List<ClinicianPatient>();
        public virtual ICollection<SensorFrame> SensorFrames { get; set; } = new List<SensorFrame>();
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    }
}
