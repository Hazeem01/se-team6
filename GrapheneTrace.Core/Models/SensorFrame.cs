using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    [Table("SensorFrames")]
    public class SensorFrame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FrameID { get; set; }

        [Required]
        public int PatientID { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        public string FrameData { get; set; } = string.Empty; // JSON string of 32x32 matrix

        [MaxLength(255)]
        public string? FilePath { get; set; }

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual FrameMetric? FrameMetric { get; set; }
        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    }
}