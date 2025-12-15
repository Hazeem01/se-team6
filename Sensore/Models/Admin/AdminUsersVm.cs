using System.ComponentModel.DataAnnotations;

namespace Sensore.Models.Admin
{
    public class AdminUsersVm
    {
        public List<AdminUserItem> Users { get; set; } = new();

        public string[] AvailableRoles { get; set; } = Array.Empty<string>();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }

    public class AdminUserItem
    {
        public string Id { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
