using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sensore.Infrastructure.Auth;
using Sensore.Models.Admin;

namespace Sensore.Controllers
{
    [Authorize(Policy = "IsAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string roleFilter = "", int page = 1, int pageSize = 10)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                var userIdsInRole = (await _userManager.GetUsersInRoleAsync(roleFilter)).Select(u => u.Id).ToHashSet();
                users = users.Where(u => userIdsInRole.Contains(u.Id));
            }

            var total = await Task.FromResult(users.Count());
            var items = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = new AdminUsersVm
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Users = items.Select(u => new AdminUserItem
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName
                }).ToList(),
                AvailableRoles = SensoreRoles.All
            };

            foreach (var userItem in vm.Users)
            {
                var user = await _userManager.FindByIdAsync(userItem.Id);
                var roles = await _userManager.GetRolesAsync(user);
                userItem.Roles = roles.ToArray();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, string[] selectedRoles)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();
            selectedRoles ??= Array.Empty<string>();

            var rolesToAdd = selectedRoles.Except(currentRoles).ToArray();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToArray();

            if (rolesToAdd.Length > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add roles.");
                    return RedirectToAction(nameof(Index));
                }
            }

            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove roles.");
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminDashboardVm();

            // total users
            vm.TotalUsers = await Task.FromResult(_userManager.Users.Count());

            // counts per role
            vm.AdminCount = (await _userManager.GetUsersInRoleAsync(SensoreRoles.Admin)).Count;
            vm.ClinicianCount = (await _userManager.GetUsersInRoleAsync(SensoreRoles.Clinician)).Count;
            vm.DoctorCount = (await _userManager.GetUsersInRoleAsync(SensoreRoles.Doctor)).Count;
            vm.ManagerCount = (await _userManager.GetUsersInRoleAsync(SensoreRoles.Manager)).Count;
            vm.PatientCount = (await _userManager.GetUsersInRoleAsync(SensoreRoles.Patient)).Count;

            vm.TotalPatients = vm.PatientCount;
            vm.ActiveAlerts = 0;
            vm.ActiveClinicians = vm.ClinicianCount;

            return View(vm);
        }
    }
}
