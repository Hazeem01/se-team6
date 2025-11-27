using System;
using System.Collections.Generic;

namespace Sensore.Models.Dashboard
{
    public class KeyMetricVm
    {
        public DateTime Timestamp { get; set; }
        public double PeakPressureIndex { get; set; }
        public double ContactAreaPercentage { get; set; }
        public double AveragePressure { get; set; }
        public int HighPressureRegions { get; set; }
    }

    public class MetricsPageVm
    {
        public string TimePeriod { get; set; } = "LastHour";
        public List<KeyMetricVm> Metrics { get; set; } = new();


        public int[,]? CurrentHeatmap { get; set; }
    }
}
