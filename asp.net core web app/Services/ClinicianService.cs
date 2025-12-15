using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Core.Models;
using GrapheneTrace.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Web.Services
{
    public class ClinicianService
    {
        private readonly GrapheneTraceContext _db;

        public ClinicianService(GrapheneTraceContext db)
        {
            _db = db;
        }

        public async Task<List<AssignedPatientDto>> GetAssignedPatients(int clinicianId)
        {
            var assigned = await _db.ClinicianPatients
                .Where(cp => cp.ClinicianID == clinicianId)
                .Include(cp => cp.Patient)
                    .ThenInclude(p => p.User)
                .ToListAsync();

            var result = new List<AssignedPatientDto>();

            foreach (var cp in assigned)
            {
                var patient = cp.Patient;
                var unackedCount = await _db.Alerts
                    .Where(a => a.PatientID == patient.PatientID && !a.IsAcknowledged)
                    .CountAsync();

                result.Add(new AssignedPatientDto
                {
                    PatientID = patient.PatientID,
                    FullName = patient.User != null ? $"{patient.User.FirstName} {patient.User.LastName}" : string.Empty,
                    RiskLevel = patient.RiskLevel,
                    UnacknowledgedAlertCount = unackedCount
                });
            }

            return result;
        }

        public async Task<PatientDashboardDto?> GetPatientDashboard(int patientId, int hours)
        {
            var since = DateTime.Now.AddHours(-hours);

            var patient = await _db.Patients
                .Include(p => p.User)
                .Include(p => p.SensorFrames.Where(sf => sf.Timestamp >= since))
                    .ThenInclude(sf => sf.FrameMetric)
                .Include(p => p.Alerts.Where(a => a.CreatedAt >= since))
                .FirstOrDefaultAsync(p => p.PatientID == patientId);

            if (patient == null) return null;

            var frames = patient.SensorFrames
                .OrderByDescending(f => f.Timestamp)
                .ToList();

            var alerts = patient.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return new PatientDashboardDto
            {
                Patient = patient,
                SensorFrames = frames,
                Alerts = alerts
            };
        }

        public async Task<List<MetricPointDto>> GetPatientMetricsTimeline(int patientId, DateTime start, DateTime end)
        {
            var points = await _db.SensorFrames
                .Where(sf => sf.PatientID == patientId && sf.Timestamp >= start && sf.Timestamp <= end)
                .Include(sf => sf.FrameMetric)
                .OrderBy(sf => sf.Timestamp)
                .ToListAsync();

            return points
                .Where(sf => sf.FrameMetric != null)
                .Select(sf => new MetricPointDto
                {
                    Timestamp = sf.Timestamp,
                    AveragePressure = sf.FrameMetric!.AveragePressure,
                    MaxPressureValue = sf.FrameMetric!.MaxPressureValue,
                    PeakPressureIndex = sf.FrameMetric!.PeakPressureIndex,
                    ContactAreaPercentage = sf.FrameMetric!.ContactAreaPercentage
                })
                .ToList();
        }

        public async Task<Alert?> GetAlertDetails(int alertId)
        {
            var alert = await _db.Alerts
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.SensorFrame)
                    .ThenInclude(sf => sf.FrameMetric)
                .Include(a => a.SensorFrame)
                    .ThenInclude(sf => sf.Comments)
                        .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(a => a.AlertID == alertId);

            return alert;
        }

        public async Task<Comment> AddComment(int frameId, int authorId, string text, int? parentCommentId)
        {
            var frame = await _db.SensorFrames
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FrameID == frameId);

            var comment = new Comment
            {
                FrameID = frameId,
                AuthorID = authorId,
                CommentText = text,
                CreatedAt = DateTime.Now,
                ParentCommentID = parentCommentId,
                PatientID = frame?.PatientID
            };

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            return comment;
        }

        public async Task<Alert?> AcknowledgeAlert(int alertId, int clinicianId)
        {
            var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.AlertID == alertId);
            if (alert == null) return null;

            alert.IsAcknowledged = true;
            alert.AcknowledgedBy = clinicianId;
            alert.AcknowledgedAt = DateTime.Now;

            _db.Alerts.Update(alert);
            await _db.SaveChangesAsync();

            return alert;
        }

        public async Task<ClinicalNote> AddClinicalNote(int patientId, int clinicianId, int? frameId, string noteText, NoteType noteType)
        {
            var note = new ClinicalNote
            {
                PatientID = patientId,
                ClinicianID = clinicianId,
                FrameID = frameId,
                NoteText = noteText,
                NoteType = noteType,
                CreatedAt = DateTime.Now
            };

            _db.ClinicalNotes.Add(note);
            await _db.SaveChangesAsync();

            return note;
        }

        // DTOs
        public class AssignedPatientDto
        {
            public int PatientID { get; set; }
            public string FullName { get; set; } = string.Empty;
            public RiskLevel RiskLevel { get; set; }
            public int UnacknowledgedAlertCount { get; set; }
        }

        public class PatientDashboardDto
        {
            public Patient Patient { get; set; } = null!;
            public List<SensorFrame> SensorFrames { get; set; } = new();
            public List<Alert> Alerts { get; set; } = new();
        }

        public class MetricPointDto
        {
            public DateTime Timestamp { get; set; }
            public double AveragePressure { get; set; }
            public double MaxPressureValue { get; set; }
            public double PeakPressureIndex { get; set; }
            public double ContactAreaPercentage { get; set; }
        }
    }
}
