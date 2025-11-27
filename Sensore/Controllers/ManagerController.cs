using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Models.Dashboard;

namespace Sensore.Controllers
{
    [Authorize(Policy = "IsManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        // -----------------------------------
        // MANAGER DASHBOARD
        // -----------------------------------
        public async Task<IActionResult> Index(string period = "6h")
        {
            //
            // ===== SUMMARY CARD DATA =====
            //

            // Total patients – adjust this query if you separate roles in your Users table
            var totalPatients = await _db.Users.CountAsync();
            ViewBag.TotalPatients = totalPatients;
            ViewBag.ActivePatients = totalPatients;

            // Care team counts – TODO: replace with real role-based queries
            ViewBag.TotalClinicians = 2;
            ViewBag.TotalDoctors = 0;

            // Critical alerts (from Alerts table)
            var criticalQuery = _db.Alerts.Where(a => a.Severity == "Critical");

            var totalCritical = await criticalQuery.CountAsync();
            var reviewedCritical = await criticalQuery.CountAsync(a => a.Acknowledged);
            var pendingCritical = totalCritical - reviewedCritical;

            int resolutionRate = totalCritical == 0
                ? 0
                : (int)Math.Round(100.0 * reviewedCritical / totalCritical);

            ViewBag.TotalCriticalAlerts = totalCritical;
            ViewBag.PendingCriticalAlerts = pendingCritical;
            ViewBag.ReviewedCritical = reviewedCritical;
            ViewBag.ResolutionRate = resolutionRate;

            //
            // ===== METRICS DATA (FROM DB) =====
            //
            // Here we assume you have a table like `SensorMetric` with the
            // columns: Timestamp, PeakPressureIndex, ContactAreaPercentage,
            // AveragePressure, HighPressureRegions.
            //
            // 1. Add this DbSet in ApplicationDbContext:
            //    public DbSet<SensorMetric> SensorMetrics { get; set; }
            //
            // 2. Create the SensorMetric entity in Models.
            //
            // 3. Run Add-Migration / Update-Database if needed.

            DateTime to = DateTime.UtcNow;
            DateTime from = period switch
            {
                "1h" => to.AddHours(-1),
                "6h" => to.AddHours(-6),
                "24h" => to.AddHours(-24),
                "7d" => to.AddDays(-7),
                _ => to.AddHours(-6)
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

            ViewBag.Metrics = metrics;

            return View();
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
        public async Task<IActionResult> Reports()
        {
           
           

            return View(); 
        }
    }
}
