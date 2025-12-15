using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Infrastructure.Auth;
using Sensore.Models;
using Sensore.Models.Admin;

namespace Sensore.Controllers
{
    /// Controller responsible for Admin area functionality:
    /// - user management (create, delete, list)
    /// - role assignment and role maintenance
    /// - admin dashboard (high-level KPIs)
    /// - patient, clinician assignments
    ///
    /// - Authorization is enforced by the "IsAdmin" policy.
    /// - This controller uses ASP.NET Core Identity APIs (UserManager, RoleManager)
    ///   directly from controllers.
 
    [Authorize(Policy = "IsAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        /// This injects Identity managers and the application DbContext.
        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        /// List users with optional role filter and pagination.
        /// Builds an AdminUsersVm that the view will render.
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
                    UserName = u.Email
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

        /// Render the Create User form.
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new AdminCreateUserVm
            {
                AvailableRoles = SensoreRoles.All
            };
            return View(vm);
        }

        /// Create a new user account. If a role is selected this action will ensure the role exists
        /// and will assign the new user to the role. On success it stores a password-setup link in TempData.
        /// After successful, the admin can copy the generated password-setup link.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserVm model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = SensoreRoles.All;
                return View(model);
            }

            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "A user with that email already exists.");
                model.AvailableRoles = SensoreRoles.All;
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
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

            // Generate a password setup token and URL for the new user.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, protocol: Request.Scheme);

            // Store link in TempData, the admin remains on the create page.
            TempData["Success"] = "User created successfully. Copy the password setup link below and send it to the user.";
            TempData["PasswordSetupLink"] = callbackUrl;

            return RedirectToAction(nameof(Create));
        }

        /// Delete a user by id. It also prevents deleting the current admin self account.
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

        /// Update roles for a user. Passing an empty selectedRole will remove all roles.
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

        /// Build the admin dashboard VM and return the dashboard view.
        /// This pulls counts from UserManager (Identity) and uses simple
        /// placeholders for alerts/clinicians until the data layer is populated.
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

        /// Display patient assignment UI. Attempts to apply pending migrations automatically
        /// if a database object is missing (academic convenience).
        [HttpGet]
        public async Task<IActionResult> Assignments()
        {
            // Try to fetch assignments; if the database is missing the PatientAssignments table, attempt to apply migrations and retry.
            try
            {
                var patients = await _userManager.GetUsersInRoleAsync(SensoreRoles.Patient);
                var clinicians = await _userManager.GetUsersInRoleAsync(SensoreRoles.Clinician);
                var doctors = await _userManager.GetUsersInRoleAsync(SensoreRoles.Doctor);

                var assignments = await _db.PatientAssignments.AsNoTracking().ToListAsync();

                var vm = new AdminPatientAssignmentVm
                {
                    Patients = patients.Select(u => new UserItem { Id = u.Id, Email = u.Email ?? u.UserName }).ToList(),
                    Clinicians = clinicians.Select(u => new UserItem { Id = u.Id, Email = u.Email ?? u.UserName }).ToList(),
                    Doctors = doctors.Select(u => new UserItem { Id = u.Id, Email = u.Email ?? u.UserName }).ToList(),
                    Assignments = assignments
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                // If the failure looks like a missing table / pending model changes, try applying migrations and retry once.
                if (ex.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("pending model", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("There are pending model changes", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        await _db.Database.MigrateAsync();

                        // Retry fetch
                        var patients = await _userManager.GetUsersInRoleAsync(SensoreRoles.Patient);
                        var clinicians = await _userManager.GetUsersInRoleAsync(SensoreRoles.Clinician);
                        var doctors = await _userManager.GetUsersInRoleAsync(SensoreRoles.Doctor);

                        var assignments = await _db.PatientAssignments.AsNoTracking().ToListAsync();

                        var vm = new AdminPatientAssignmentVm
                        {
                            Patients = patients.Select(u => new UserItem { Id = u.Id, Email = u.Email ?? u.UserName }).ToList(),
                            Clinicians = clinicians.Select(u => new UserItem { Id = u.Id, Email = u.Email ?? u.UserName }).ToList(),
                            Doctors = doctors.Select(u => new UserItem { Id = u.Id, Email = u.Email ?? u.UserName }).ToList(),
                            Assignments = assignments
                        };

                        TempData["Success"] = "Applied pending migrations automatically.";
                        return View(vm);
                    }
                    catch (Exception migrateEx)
                    {
                        // If automatic migration fails, it show the message below and surface the original error.
                        TempData["Error"] = "Database schema is out of sync. Please run migrations (dotnet ef database update) or check the connection string. " + migrateEx.Message;
                        return RedirectToAction("Index");
                    }
                }

                // Unknown error - rethrow to be handled by exception middleware
                throw;
            }
        }

        /// Assign or update patient assignment (clinician or doctor).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPatient(string patientId, string? clinicianId, string? doctorId)
        {
            if (string.IsNullOrEmpty(patientId))
                return BadRequest();

            var patient = await _userManager.FindByIdAsync(patientId);
            if (patient == null)
                return NotFound();

            var assignment = await _db.PatientAssignments.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (assignment == null)
            {
                assignment = new PatientAssignment
                {
                    PatientId = patientId,
                    ClinicianId = string.IsNullOrWhiteSpace(clinicianId) ? null : clinicianId,
                    DoctorId = string.IsNullOrWhiteSpace(doctorId) ? null : doctorId,
                    AssignedAt = DateTime.UtcNow
                };
                _db.PatientAssignments.Add(assignment);
            }
            else
            {
                assignment.ClinicianId = string.IsNullOrWhiteSpace(clinicianId) ? null : clinicianId;
                assignment.DoctorId = string.IsNullOrWhiteSpace(doctorId) ? null : doctorId;
                assignment.AssignedAt = DateTime.UtcNow;
                _db.PatientAssignments.Update(assignment);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Patient assignment updated.";
            return RedirectToAction(nameof(Assignments));
        }

        /// Remove a patient assignment (by assignment id or patient id).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(Guid? assignmentId, string? patientId)
        {
            PatientAssignment? assignment = null;

            if (assignmentId.HasValue)
            {
                assignment = await _db.PatientAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(patientId))
            {
                assignment = await _db.PatientAssignments.FirstOrDefaultAsync(a => a.PatientId == patientId);
            }

            if (assignment == null)
            {
                TempData["Error"] = "Assignment not found.";
                return RedirectToAction(nameof(Assignments));
            }

            _db.PatientAssignments.Remove(assignment);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Assignment removed.";
            return RedirectToAction(nameof(Assignments));
        }
    }
}
