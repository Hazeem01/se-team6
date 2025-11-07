using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sensore.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index() => View();

        [Authorize(Policy = "IsAdmin")]
        public IActionResult Admin() => View();

        [Authorize(Policy = "IsClinician")]
        public IActionResult Clinician() => View();

        [Authorize(Policy = "IsPatient")]
        public IActionResult Patient() => View();

        [Authorize(Policy = "IsDoctor")]
        public IActionResult Doctor() => View();

        [Authorize(Policy = "IsManager")]
        public IActionResult Manager() => View();
    }
}
