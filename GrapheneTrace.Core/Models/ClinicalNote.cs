using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    public enum NoteType
    {
        Assessment,
        Treatment,
        Observation,
        General
    }

    [Table("ClinicalNotes")]
    public class ClinicalNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoteID { get; set; }

        [Required]
        public int PatientID { get; set; }

        [Required]
        public int ClinicianID { get; set; }

        public int? FrameID { get; set; }

        [Required]
        public string NoteText { get; set; } = string.Empty;

        [Required]
        public NoteType NoteType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual Clinician Clinician { get; set; } = null!;
        public virtual SensorFrame? SensorFrame { get; set; }
    }
}