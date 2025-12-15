using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sensore.Models.Dashboard;
using System;
using System.Collections.Generic;


namespace Sensore.Controllers
{
    [Authorize(Roles = "Manager,Clinician,Doctor")]
    public class MetricsController : Controller
    {
        public IActionResult Index(string period = "6h")
        {
            var metrics = BuildDummyMetrics(period);
            return View(metrics);  
        }

      
        private List<KeyMetricVm> BuildDummyMetrics(string period)
        {
            var now = DateTime.UtcNow;

          
            var data = Enumerable.Range(0, 73)
                .Select(i => new KeyMetricVm
                {
                    Timestamp = now.AddMinutes(-5 * (72 - i)),
                    PeakPressureIndex = 200 + Math.Sin(i / 3.0) * 30,
                    ContactAreaPercentage = 65 + Math.Cos(i / 5.0) * 2,
                    AveragePressure = 0,
                    HighPressureRegions = 0
                })
                .ToList();

            return data;
        }
    }
}
