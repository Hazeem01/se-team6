using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    public enum AlertType
    {
        HighPressure,
        ProlongedPressure,
        LowContactArea,
        SystemWarning
    }

    public enum AlertSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    [Table("Alerts")]
    public class Alert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertID { get; set; }

        [Required]
        public int PatientID { get; set; }

        public int? FrameID { get; set; }

        [Required]
        public AlertType AlertType { get; set; }

        [Required]
        public AlertSeverity Severity { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsAcknowledged { get; set; } = false;

        public int? AcknowledgedBy { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual SensorFrame? SensorFrame { get; set; }
    }
}
