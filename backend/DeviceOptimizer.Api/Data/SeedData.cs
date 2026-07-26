using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Devices.Any()) return;

            var tenantNames = new[] { "Sakura Trading", "Fuji Manufacturing", "Tokyo Design Co", "Sunrise Logistics" };
            var tenants = tenantNames.Select(name => new Tenant { Name = name }).ToList();
            context.Tenants.AddRange(tenants);
            context.SaveChanges();

            var models = new[]
            {
                "Dell Latitude 5420", "Dell Latitude 5540", "Lenovo ThinkPad X1",
                "Lenovo ThinkPad T14", "HP EliteBook 840", "Surface Laptop 5"
            };

            var statusPlan = new List<DeviceStatus>();
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.Rented, 55));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.InStock, 25));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.Returned, 10));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.InRepair, 6));
            statusPlan.AddRange(Enumerable.Repeat(DeviceStatus.Retired, 4));

            var random = new Random(42);
            var devices = new List<Device>();

            for (int i = 0; i < statusPlan.Count; i++)
            {
                var tenant = tenants[random.Next(tenants.Count)];
                var daysOwned = random.Next(30, 4 * 365);
                var status = statusPlan[i];

                devices.Add(new Device
                {
                    TenantId = tenant.Id,
                    Model = models[random.Next(models.Length)],
                    SerialNumber = $"YRL-{(i + 1):D6}",
                    PurchaseDate = DateTime.UtcNow.AddDays(-daysOwned),
                    RepairCount = random.Next(0, 4),
                    Status = status,
                    ReturnedAt = DaysAgoIfEverReturned(status, random)
                });
            }

            context.Devices.AddRange(devices);
            context.SaveChanges();
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
