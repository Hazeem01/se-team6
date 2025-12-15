using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrapheneTrace.Core.Models;
using GrapheneTrace.Data.Context;
using GrapheneTrace.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Web.Controllers
{
    public class ClinicianController : Controller
    {
        private readonly GrapheneTraceContext _db;

        public ClinicianController(GrapheneTraceContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var clinicianId = HttpContext.Session.GetInt32("ClinicianID");
            if (!clinicianId.HasValue) return Unauthorized("Clinician not logged in.");

            var assigned = await _db.ClinicianPatients
                .Where(cp => cp.ClinicianID == clinicianId.Value)
                .Include(cp => cp.Patient)
                    .ThenInclude(p => p.User)
                .ToListAsync();

            var patientIds = assigned.Select(a => a.PatientID).ToList();

            // Get alert counts for these patients in a single query
            var alertCounts = await _db.Alerts
                .Where(a => patientIds.Contains(a.PatientID) && !a.IsAcknowledged)
                .GroupBy(a => a.PatientID)
                .Select(g => new { PatientID = g.Key, Count = g.Count() })
                .ToListAsync();

            var alertLookup = alertCounts.ToDictionary(x => x.PatientID, x => x.Count);

            // Get latest sensor frame timestamp per patient in a single query
            var latestFrames = await _db.SensorFrames
                .Where(sf => patientIds.Contains(sf.PatientID))
                .GroupBy(sf => sf.PatientID)
                .Select(g => new { PatientID = g.Key, Latest = g.Max(sf => sf.Timestamp) })
                .ToListAsync();

            var latestLookup = latestFrames.ToDictionary(x => x.PatientID, x => (DateTime?)x.Latest);

            var model = assigned.Select(a => new PatientSummaryViewModel
            {
                PatientID = a.Patient.PatientID,
                FullName = a.Patient.User != null ? $"{a.Patient.User.FirstName} {a.Patient.User.LastName}" : string.Empty,
                MedicalRecordNumber = a.Patient.MedicalRecordNumber,
                RiskLevel = a.Patient.RiskLevel.ToString(),
                UnacknowledgedAlertCount = alertLookup.TryGetValue(a.Patient.PatientID, out var c) ? c : 0,
                LastSensorUpdate = latestLookup.TryGetValue(a.Patient.PatientID, out var t) ? t : null
            }).OrderByDescending(x => x.RiskLevel).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PatientDetail(int id)
        {
            var clinicianId = HttpContext.Session.GetInt32("ClinicianID");
            if (!clinicianId.HasValue) return Unauthorized("Clinician not logged in.");

            var since = DateTime.UtcNow.AddHours(-24);

            var patient = await _db.Patients
                .Where(p => p.PatientID == id)
                .Include(p => p.User)
                .Include(p => p.SensorFrames.Where(sf => sf.Timestamp >= since))
                    .ThenInclude(sf => sf.FrameMetric)
                .Include(p => p.Alerts)
                .FirstOrDefaultAsync();

            if (patient == null) return NotFound("Patient not found.");

            var recentFrames = patient.SensorFrames
                .OrderByDescending(f => f.Timestamp)
                .Select(f => new FrameDetailViewModel
                {
                    FrameID = f.FrameID,
                    Timestamp = f.Timestamp,
                    Metrics = f.FrameMetric != null ? new FrameMetricsViewModel
                    {
                        AveragePressure = f.FrameMetric.AveragePressure,
                        MaxPressureValue = f.FrameMetric.MaxPressureValue,
                        PeakPressureIndex = f.FrameMetric.PeakPressureIndex,
                        ContactAreaPercentage = f.FrameMetric.ContactAreaPercentage
                    } : new FrameMetricsViewModel()
                }).ToList();

            var unackAlerts = patient.Alerts
                .Where(a => !a.IsAcknowledged)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AlertViewModel
                {
                    AlertID = a.AlertID,
                    AlertType = a.AlertType,
                    Severity = a.Severity,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt,
                    IsAcknowledged = a.IsAcknowledged,
                    PatientName = patient.User != null ? $"{patient.User.FirstName} {patient.User.LastName}" : string.Empty
                })
                .ToList();

            var vm = new PatientDashboardViewModel
            {
                Patient = new PatientInfoViewModel
                {
                    PatientID = patient.PatientID,
                    FullName = patient.User != null ? $"{patient.User.FirstName} {patient.User.LastName}" : string.Empty,
                    MedicalRecordNumber = patient.MedicalRecordNumber,
                    RiskLevel = patient.RiskLevel,
                    AdmissionDate = patient.AdmissionDate,
                    DateOfBirth = patient.DateOfBirth
                },
                // timeline from recent frames
                MetricsTimeline = patient.SensorFrames
                    .OrderBy(f => f.Timestamp)
                    .Where(f => f.FrameMetric != null)
                    .Select(f => new MetricDataPoint
                    {
                        Timestamp = f.Timestamp,
                        PeakPressure = f.FrameMetric!.PeakPressureIndex,
                        ContactArea = f.FrameMetric.ContactAreaPercentage
                    }).ToList(),
                Alerts = unackAlerts
            };

            if (vm.MetricsTimeline.Any())
            {
                vm.AvgPeakPressure = vm.MetricsTimeline.Average(m => m.PeakPressure);
                vm.AvgContactArea = vm.MetricsTimeline.Average(m => m.ContactArea);
            }

            vm.CriticalAlertCount = vm.Alerts.Count(a => a.Severity == AlertSeverity.Critical);

            return View(vm);
        }
    }
}
