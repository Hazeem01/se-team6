using System;
using Microsoft.AspNetCore.Identity;   

namespace Sensore.Models
{
    public class SensorMetric
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public int PeakPressureIndex { get; set; }
        public double ContactAreaPercentage { get; set; }
        public double AveragePressure { get; set; }
        public int HighPressureRegions { get; set; }


        public string? UserId { get; set; }
    }
}