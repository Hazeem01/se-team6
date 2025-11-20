using System;
using Microsoft.AspNetCore.Identity; 

namespace Sensore.Models
{
    public class Alert
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = default!;
        public IdentityUser? User { get; set; }  

        public string Severity { get; set; } = "Warning";
        public string Type { get; set; } = "HIGH_PRESSURE";
        public string? Reason { get; set; }
        public DateTime StartTs { get; set; } = DateTime.UtcNow;
        public DateTime? EndTs { get; set; }
        public bool Acknowledged { get; set; } = false;
    }
}
