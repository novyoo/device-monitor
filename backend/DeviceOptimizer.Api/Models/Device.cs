namespace DeviceOptimizer.Api.Models
{
    public enum DeviceStatus
    {
        InStock,
        Rented,
        Returned,
        InRepair,
        Retired
    }

    public enum DevicePersonality
    {
        Boring,
        Aging,
        TroubleProne
    }

    public class Device
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public Guid ApiKey { get; set; } = Guid.NewGuid();

        public DateTime PurchaseDate { get; set; }
        public int RepairCount { get; set; }

        public DeviceStatus Status { get; set; } = DeviceStatus.InStock;
        public DateTime? ReturnedAt { get; set; }

        public DevicePersonality Personality { get; set; } = DevicePersonality.Boring;

        public int? LastBatteryHealthPercent { get; set; }
        public int? LastDiskWearPercent { get; set; }
        public int? LastCrashCount { get; set; }
        public double? LastTemperatureCelsius { get; set; }
        public double? LastActiveUseHours { get; set; }
        public DateTime? LastCheckInAt { get; set; }

        public List<CheckIn> CheckIns { get; set; } = new();
    }
}
