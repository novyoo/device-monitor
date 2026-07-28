namespace DeviceOptimizer.Api.DTOs
{
    public class HealthHistoryPointDto
    {
        public DateTime Timestamp { get; set; }
        public int Score { get; set; }
    }

    public class DeviceDetailDto
    {
        public int Id { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastCheckInAt { get; set; }

        public int? HealthScore { get; set; }
        public string? HealthBand { get; set; }
        public List<string> Reasons { get; set; } = new();
        public List<string> Flags { get; set; } = new();
        public string? TrendMessage { get; set; }

        public int? BatteryHealthPercent { get; set; }
        public int? DiskWearPercent { get; set; }
        public int? DiskErrorCount { get; set; }
        public int? CrashCount { get; set; }
        public int? SuddenShutdownCount { get; set; }
        public double? TemperatureCelsius { get; set; }
        public int? RamUsagePercent { get; set; }
        public double? ActiveUseHours { get; set; }
        public int? DaysSinceOsUpdate { get; set; }

        public List<HealthHistoryPointDto> History { get; set; } = new();
    }
}
