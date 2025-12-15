using System.Threading.Tasks;
using GrapheneTrace.Data.Context;
using GrapheneTrace.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly GrapheneTraceContext _db;

        public AuthController(GrapheneTraceContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Username and password are required.");
                return View();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View();
            }

            //var verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            //if (!verified)
            //{
            //    ModelState.AddModelError(string.Empty, "Invalid username or password.");
            //    return View();
            //}

            if (user.Role != UserRole.Clinician)
            {
                ModelState.AddModelError(string.Empty, "Access denied. User is not a clinician.");
                return View();
            }

            var clinician = await _db.Clinicians.FirstOrDefaultAsync(c => c.ClinicianID == user.UserID);
            if (clinician == null)
            {
                ModelState.AddModelError(string.Empty, "Clinician record not found for this user.");
                return View();
            }

            // Store UserID and ClinicianID in session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetInt32("ClinicianID", clinician.ClinicianID);

            return RedirectToAction("Dashboard", "Clinician");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
