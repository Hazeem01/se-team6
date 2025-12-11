using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    [Table("ClinicianPatients")]
    public class ClinicianPatient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClinicianPatientID { get; set; }

        [Required]
        public int ClinicianID { get; set; }

        [Required]
        public int PatientID { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Clinician Clinician { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
