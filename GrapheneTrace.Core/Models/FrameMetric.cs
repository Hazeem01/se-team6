using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Core.Models
{
    [Table("FrameMetrics")]
    public class FrameMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MetricID { get; set; }

        [Required]
        [ForeignKey("SensorFrame")]
        public int FrameID { get; set; }

        public double PeakPressureIndex { get; set; }

        public double ContactAreaPercentage { get; set; }

        public double AveragePressure { get; set; }

        public double MaxPressureValue { get; set; }

        // Navigation property
        public virtual SensorFrame SensorFrame { get; set; } = null!;
    }
}