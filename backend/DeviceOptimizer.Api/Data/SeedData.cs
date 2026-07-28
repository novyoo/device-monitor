using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Data
{
    public static class SeedData
    {
        private const string DemoPassword = "FleetPulse-Yrl-2026!";

        private static readonly string[] Models =
        {
            "Dell Latitude 5420", "Dell Latitude 5540", "Lenovo ThinkPad X1",
            "Lenovo ThinkPad T14", "HP EliteBook 840", "Surface Laptop 5"
        };

        public static void Initialize(AppDbContext context)
        {
            SeedModelFootprints(context);
            SeedRealDeviceLabTenant(context);

            if (context.Devices.Any()) return;

            var tenantNames = new[] { "Sakura Trading", "Fuji Manufacturing", "Tokyo Design Co", "Sunrise Logistics" };
            var tenants = tenantNames.Select(name => new Tenant { Name = name }).ToList();
            context.Tenants.AddRange(tenants);
            context.SaveChanges();

            var statusPlan = new List<DeviceStatus>();
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.Rented, 55));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.InStock, 25));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.Returned, 10));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.InRepair, 6));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.Retired, 4));

            var personalityPlan = new List<DevicePersonality>();
            personalityPlan.AddRange(Enumerable.Repeat(DevicePersonality.Boring, 85));
            personalityPlan.AddRange(Enumerable.Repeat(DevicePersonality.Aging, 10));
            personalityPlan.AddRange(Enumerable.Repeat(DevicePersonality.TroubleProne, 5));

            var random = new Random(42);
            personalityPlan = personalityPlan.OrderBy(_ => random.Next()).ToList();

            var devices = new List<Device>();

            for (int i = 0; i < statusPlan.Count; i++)
            {
                var tenant = tenants[random.Next(tenants.Count)];
                var daysOwned = random.Next(30, 4 * 365);
                var status = statusPlan[i];

                var isHeavyRamUser = random.NextDouble() < 0.15;
                var ramUsageBaseline = isHeavyRamUser ? random.Next(85, 99) : random.Next(35, 80);
                var isSlowToUpdate = random.NextDouble() < 0.1;

                devices.Add(new Device
                {
                    TenantId = tenant.Id,
                    Model = Models[random.Next(Models.Length)],
                    SerialNumber = $"YRL-{(i + 1):D6}",
                    PurchaseDate = DateTime.UtcNow.AddDays(-daysOwned),
                    RepairCount = random.Next(0, 4),
                    Status = status,
                    ReturnedAt = DaysAgoIfEverReturned(status, random),
                    Personality = personalityPlan[i],
                    RamUsageBaselinePercent = ramUsageBaseline,
                    IsSlowToUpdate = isSlowToUpdate
                });
            }

            context.Devices.AddRange(devices);
            context.SaveChanges();
        }

        private static void SeedRealDeviceLabTenant(AppDbContext context)
        {
            if (context.Tenants.Any(t => t.Name == "Real Device Lab")) return;

            context.Tenants.Add(new Tenant { Name = "Real Device Lab" });
            context.SaveChanges();
        }

        private static void SeedModelFootprints(AppDbContext context)
        {
            if (context.ModelFootprints.Any()) return;

            var footprints = new List<ModelFootprint>
            {
                new ModelFootprint { Model = "Dell Latitude 5420", ManufacturingKgCo2e = 274, UsePhaseKgCo2ePerYear = 26 },
                new ModelFootprint { Model = "Dell Latitude 5540", ManufacturingKgCo2e = 281, UsePhaseKgCo2ePerYear = 27 },
                new ModelFootprint { Model = "Lenovo ThinkPad X1", ManufacturingKgCo2e = 293, UsePhaseKgCo2ePerYear = 24 },
                new ModelFootprint { Model = "Lenovo ThinkPad T14", ManufacturingKgCo2e = 268, UsePhaseKgCo2ePerYear = 25 },
                new ModelFootprint { Model = "HP EliteBook 840", ManufacturingKgCo2e = 277, UsePhaseKgCo2ePerYear = 26 },
                new ModelFootprint { Model = "Surface Laptop 5", ManufacturingKgCo2e = 253, UsePhaseKgCo2ePerYear = 22 },
            };

            context.ModelFootprints.AddRange(footprints);
            context.SaveChanges();
        }

        public static async Task SeedIdentityAsync(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { "Admin", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            await CreateDemoUserIfMissing(userManager, "admin@fleetpulse.demo", "Admin", tenantId: null);

            var tenants = await context.Tenants.ToListAsync();
            foreach (var tenant in tenants)
            {
                var emailName = tenant.Name.Split(' ')[0].ToLowerInvariant();
                await CreateDemoUserIfMissing(userManager, $"{emailName}@fleetpulse.demo", "Customer", tenant.Id);
            }
        }

        private static async Task CreateDemoUserIfMissing(UserManager<AppUser> userManager, string email, string role, int? tenantId)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                TenantId = tenantId
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        private static DateTime? DaysAgoIfEverReturned(DeviceStatus status, Random random)
        {
            var daysAgo = status switch
            {
                DeviceStatus.Returned => random.Next(1, 15),
                DeviceStatus.InRepair => random.Next(5, 60),
                DeviceStatus.Retired => random.Next(10, 120),
                _ => (int?)null
            };

            return daysAgo == null ? null : DateTime.UtcNow.AddDays(-daysAgo.Value);
        }
    }
}
