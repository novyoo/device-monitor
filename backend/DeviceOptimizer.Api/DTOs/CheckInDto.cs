namespace DeviceOptimizer.Api.DTOs
{
    public class CheckInDto
    {
        public Guid ApiKey { get; set; }
        public int BatteryHealthPercent { get; set; }
        public int DiskWearPercent { get; set; }
        public int CrashCount { get; set; }
        public double TemperatureCelsius { get; set; }
        public double ActiveUseHours { get; set; }
    }
}
