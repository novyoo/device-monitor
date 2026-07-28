namespace DeviceOptimizer.Agent
{
    public class CheckInMessage
    {
        public string ApiKey { get; set; } = string.Empty;
        public int BatteryHealthPercent { get; set; }
        public int DiskWearPercent { get; set; }
        public int DiskErrorCount { get; set; }
        public int CrashCount { get; set; }
        public int SuddenShutdownCount { get; set; }
        public double TemperatureCelsius { get; set; }
        public int RamUsagePercent { get; set; }
        public double ActiveUseHours { get; set; }
        public int DaysSinceOsUpdate { get; set; }
    }
}
