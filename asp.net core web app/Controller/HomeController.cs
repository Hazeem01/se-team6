using Microsoft.AspNetCore.Mvc;

namespace GrapheneTrace.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("SUCCESS! GrapheneTrace is running!");
        }
    }
}
