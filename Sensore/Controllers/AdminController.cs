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

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new AdminCreateUserVm
            {
                AvailableRoles = SensoreRoles.All
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserVm model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = SensoreRoles.All;
                return View(model);
            }

            // This checks if user with same email already exists
            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "A user with that email already exists.");
                model.AvailableRoles = SensoreRoles.All;
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = string.IsNullOrWhiteSpace(model.UserName) ? model.Email : model.UserName,
                Email = model.Email,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                model.AvailableRoles = SensoreRoles.All;
                return View(model);
            }

        
            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                var r = model.SelectedRole;
                if (!await _roleManager.RoleExistsAsync(r))
                {
                    var createRole = new IdentityRole(r);
                    var roleResult = await _roleManager.CreateAsync(createRole);
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(user);
                        foreach (var err in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Failed to ensure role '{r}': {err.Description}");
                        }
                        model.AvailableRoles = SensoreRoles.All;
                        return View(model);
                    }
                }

                var roleAssignResult = await _userManager.AddToRoleAsync(user, r);
                if (!roleAssignResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    foreach (var err in roleAssignResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                    }
                    model.AvailableRoles = SensoreRoles.All;
                    return View(model);
                }
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, protocol: Request.Scheme);
            TempData["Success"] = "User created successfully. Share the provided password setup link with the user.";
            TempData["PasswordSetupLink"] = callbackUrl;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                TempData["Error"] = "Failed to delete user.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, string selectedRole)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();

            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                if (currentRoles.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        foreach (var err in removeResult.Errors)
                        {
                            ModelState.AddModelError("", err.Description);
                        }
                        return RedirectToAction(nameof(Index));
                    }
                }

                TempData["Success"] = "Roles updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _roleManager.RoleExistsAsync(selectedRole))
            {
                var createRole = new IdentityRole(selectedRole);
                var roleResult = await _roleManager.CreateAsync(createRole);
                if (!roleResult.Succeeded)
                {
                    foreach (var err in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, $"Failed to ensure role '{selectedRole}': {err.Description}");
                    }
                    return RedirectToAction(nameof(Index));
                }
            }

            var rolesToRemove = currentRoles.Where(r => r != selectedRole).ToArray();
            if (rolesToRemove.Length > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var err in removeResult.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!currentRoles.Contains(selectedRole))
            {
                var addResult = await _userManager.AddToRoleAsync(user, selectedRole);
                if (!addResult.Succeeded)
                {
                    foreach (var err in addResult.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["Success"] = "Roles updated successfully.";
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
