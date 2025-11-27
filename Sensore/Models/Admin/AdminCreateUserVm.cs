using System.ComponentModel.DataAnnotations;

namespace Sensore.Models.Admin
{
    public class AdminCreateUserVm
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public string[] AvailableRoles { get; set; } = Array.Empty<string>();

        public string? SelectedRole { get; set; } = string.Empty;
    }
}