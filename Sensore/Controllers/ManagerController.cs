using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Data.Migrations;
using Sensore.Models;
using Sensore.Models.Dashboard;
using System.Linq;


namespace Sensore.Controllers
{
    [Authorize(Policy = "IsManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public ManagerController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager; 
        }

        [HttpGet]
        public async Task<IActionResult> Index(string period = "today")
        {
            var vm = new ManagerDashboardVm
            {
                SelectedPeriod = period
            };
            var clinicians = await _userManager.GetUsersInRoleAsync("Clinician");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
            var patients = await _userManager.GetUsersInRoleAsync("Patient");

            vm.TotalClinicians = clinicians.Count;
            vm.TotalDoctors = doctors.Count;
            vm.TotalPatients = patients.Count;

          
            var patientIds = patients
                .Select(p => p.Id)
                .ToHashSet();

            
            var alerts = await _db.Alerts
                .Include(a => a.User)
                .ToListAsync();

            
            vm.TotalAlerts = alerts.Count;
            vm.UnreviewedAlerts = alerts.Count(a => !a.Acknowledged);
            vm.CriticalAlerts = alerts.Count(a => a.Severity == "Critical" && !a.Acknowledged);
            vm.ReviewedAlerts = alerts.Count(a => a.Acknowledged);

            
            var highRiskAlerts = alerts
                .Where(a => a.Severity == "Critical"
                         && !a.Acknowledged
                         && a.UserId != null
                         && patientIds.Contains(a.UserId))
                .ToList();

            vm.ResolutionRate = vm.TotalAlerts == 0
                ? 0
                : (int)Math.Round(100.0 * (double)vm.ReviewedAlerts / vm.TotalAlerts);


            vm.AverageResponseHours = 2.4;



            vm.AlertSeverity.High = alerts.Count(a => a.Severity == "Critical");
            vm.AlertSeverity.Medium = alerts.Count(a => a.Severity == "Medium");
            vm.AlertSeverity.Low = alerts.Count(a => a.Severity == "Low");


       
            var highRiskGroups = highRiskAlerts
                .GroupBy(a => a.UserId!)
                .ToList();

            foreach (var grp in highRiskGroups)
            {
                var user = grp.First().User;
                if (user == null) continue;

                vm.HighRiskPatients.Add(new HighRiskPatientVm
                {
                    Id = grp.Key,
                    Name = user.UserName ?? user.Email ?? grp.Key,
                    UnreviewedAlertCount = grp.Count()
                });
            }


            DateTime to = DateTime.UtcNow;
            DateTime from = period switch
            {
                "today" => to.Date,
                "week" => to.AddDays(-7),
                "month" => to.AddMonths(-1),
                _ => to.AddDays(-1)
            };

            var metrics = await _db.SensorMetrics
                .Where(m => m.Timestamp >= from && m.Timestamp <= to)
                .OrderBy(m => m.Timestamp)
                .Select(m => new KeyMetricVm
                {
                    Timestamp = m.Timestamp,
                    PeakPressureIndex = m.PeakPressureIndex,
                    ContactAreaPercentage = m.ContactAreaPercentage,
                    AveragePressure = m.AveragePressure,
                    HighPressureRegions = m.HighPressureRegions
                })
                .ToListAsync();

            if (!metrics.Any())
            {
                metrics = BuildDummyMetrics(from, to);
            }

            vm.Metrics = metrics;

            return View(vm);
        }


        private List<KeyMetricVm> BuildDummyMetrics(DateTime from, DateTime to)
        {
            var list = new List<KeyMetricVm>();
            var rand = new Random();

            int points = 20;
            var spanMinutes = (to - from).TotalMinutes;
            var step = spanMinutes / points;

            for (int i = 0; i < points; i++)
            {
                var ts = from.AddMinutes(step * i);

                list.Add(new KeyMetricVm
                {
                    Timestamp = ts,
                    PeakPressureIndex = 200 + rand.Next(-20, 20),
                    ContactAreaPercentage = 60 + rand.Next(-5, 5),
                    AveragePressure = 90 + rand.Next(-10, 10),
                    HighPressureRegions = rand.Next(0, 4)
                });
            }

            return list;
        }

        [HttpGet]
        public async Task<IActionResult> Alerts()
        {
            var alerts = await _db.Alerts
                .Include(a => a.User)
                .OrderByDescending(a => a.StartTs)
                .ToListAsync();
            return View(alerts);
        }

        [HttpGet]
        public IActionResult Reports()
        {
            return View();
        }
    }
}