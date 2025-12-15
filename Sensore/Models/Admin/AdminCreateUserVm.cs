using System.ComponentModel.DataAnnotations;

namespace Sensore.Models.Admin
{
    /// ViewModel used by Admin Create User page.
    /// Includes validation attributes to ensure email correctness.
    /// The SelectedRole can be empty to create a user without roles.
    public class AdminCreateUserVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        /// Role options presented to the administrator.
        /// Populated from Sensore.Infrastructure.Auth.SensoreRoles.

        public string[] AvailableRoles { get; set; } = Array.Empty<string>();


        /// The role selected by the admin when creating the user.
        /// Empty or null indicates no role assignment.

        public string? SelectedRole { get; set; } = string.Empty;
    }
}