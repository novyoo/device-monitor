namespace DeviceOptimizer.Api.Models
{
    public class CheckIn
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device? Device { get; set; }

        public int BatteryHealthPercent { get; set; }
        public int DiskWearPercent { get; set; }
        public int CrashCount { get; set; }
        public double TemperatureCelsius { get; set; }
        public double ActiveUseHours { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
