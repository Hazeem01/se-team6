using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sensore.Models;

namespace Sensore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                if (User.IsInRole("Clinician"))
                    return RedirectToAction("Clinician", "Dashboard");

                if (User.IsInRole("Doctor"))
                    return RedirectToAction("Doctor", "Dashboard");

                if (User.IsInRole("Manager"))
                    return RedirectToAction("Manager", "Dashboard");

                if (User.IsInRole("Patient"))
                    return RedirectToAction("Patient", "Dashboard");

                // fallback to main dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
