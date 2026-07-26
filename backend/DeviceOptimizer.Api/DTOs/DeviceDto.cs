namespace DeviceOptimizer.Api.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public int RepairCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReturnedAt { get; set; }

        public int? BatteryHealthPercent { get; set; }
        public int? DiskWearPercent { get; set; }
        public int? CrashCount { get; set; }
        public double? TemperatureCelsius { get; set; }
        public double? ActiveUseHours { get; set; }
        public DateTime? LastCheckInAt { get; set; }
    }
}
