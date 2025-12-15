using System;
using System.Collections.Generic;
using GrapheneTrace.Core.Models;

namespace GrapheneTrace.Web.ViewModels
{
    public class PatientSummaryViewModel
    {
        public int PatientID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public int UnacknowledgedAlertCount { get; set; }
        public DateTime? LastSensorUpdate { get; set; }
    }

    public class MetricDataPoint
    {
        public DateTime Timestamp { get; set; }
        public double PeakPressure { get; set; }
        public double ContactArea { get; set; }
    }

    public class AlertViewModel
    {
        public int AlertID { get; set; }
        public AlertType AlertType { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsAcknowledged { get; set; }
        public string PatientName { get; set; } = string.Empty;
    }

    public class PatientInfoViewModel
    {
        public int PatientID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? MedicalRecordNumber { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class PatientDashboardViewModel
    {
        public PatientInfoViewModel Patient { get; set; } = new PatientInfoViewModel();
        public List<MetricDataPoint> MetricsTimeline { get; set; } = new List<MetricDataPoint>();
        public List<AlertViewModel> Alerts { get; set; } = new List<AlertViewModel>();

        // Summary statistics
        public double AvgPeakPressure { get; set; }
        public double AvgContactArea { get; set; }
        public int CriticalAlertCount { get; set; }
    }

    public class FrameMetricsViewModel
    {
        public double AveragePressure { get; set; }
        public double MaxPressureValue { get; set; }
        public double PeakPressureIndex { get; set; }
        public double ContactAreaPercentage { get; set; }
    }

    public class FrameDetailViewModel
    {
        public int FrameID { get; set; }
        public DateTime Timestamp { get; set; }
        public double[,] HeatMapData { get; set; } = new double[32, 32];
        public FrameMetricsViewModel Metrics { get; set; } = new FrameMetricsViewModel();
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public List<AlertViewModel> Alerts { get; set; } = new List<AlertViewModel>();
    }

    public class CommentViewModel
    {
        public int CommentID { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }

}
