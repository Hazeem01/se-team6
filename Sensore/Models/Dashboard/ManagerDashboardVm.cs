using System;
using System.Collections.Generic;

namespace Sensore.Models.Dashboard
{
    public class HighRiskPatientVm
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UnreviewedAlertCount { get; set; }
    }

    public class AlertSeveritySummaryVm
    {
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
    }

    public class ManagerDashboardVm
    {
        // Header / filters
        public string SelectedPeriod { get; set; } = "today"; // "today" 

        // Key metrics (top row)
        public int TotalPatients { get; set; }
        public int TotalClinicians { get; set; }
        public int TotalDoctors { get; set; }

        public int TotalAlerts { get; set; }
        public int UnreviewedAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int ReviewedAlerts { get; set; }
        public int ResolutionRate { get; set; } // %

        public double AverageResponseHours { get; set; }

        // Chart data (Pressure Metrics)
        public List<KeyMetricVm> Metrics { get; set; } = new();

        // Alert trends
        public AlertSeveritySummaryVm AlertSeverity { get; set; } = new();

        // High-risk patients
        public List<HighRiskPatientVm> HighRiskPatients { get; set; } = new();
    }
}
