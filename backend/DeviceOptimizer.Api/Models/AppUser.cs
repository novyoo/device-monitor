using Microsoft.AspNetCore.Identity;

namespace DeviceOptimizer.Api.Models
{
    public class AppUser : IdentityUser
    {
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
