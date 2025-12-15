using Microsoft.AspNetCore.Identity;
using Sensore.Infrastructure.Auth;

namespace Sensore.Infrastructure.Seed
{
    /// Seeds Identity roles and a small set of demo usersfor the project..
    public static class IdentitySeeder
    {
        /// Create required roles and demo users if they do not exist.
        /// This method is called at application startup from the Program.cs file.

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            foreach (var role in SensoreRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await CreateUserIfNotExists(userManager, "admin@sensore.local", "Admin#12345    ", SensoreRoles.Admin);
            await CreateUserIfNotExists(userManager, "clinician@sensore.local", "Clinician#123", SensoreRoles.Clinician);
            await CreateUserIfNotExists(userManager, "patient@sensore.local", "Patient#123", SensoreRoles.Patient);
            await CreateUserIfNotExists(userManager, "doctor@sensore.local", "Doctor#123", SensoreRoles.Doctor);
            await CreateUserIfNotExists(userManager, "manager@sensore.local", "Manager#123", SensoreRoles.Manager);
        }

        /// Helper to create a user if it does not already exist and assign the provided role.
        private static async Task CreateUserIfNotExists(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
