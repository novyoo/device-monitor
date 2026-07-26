namespace DeviceOptimizer.Api.Services
{
    public class HealthScoreResult
    {
        public int Score { get; set; }
        public string Band { get; set; } = string.Empty;
        public List<string> Reasons { get; set; } = new();
        public List<string> Flags { get; set; } = new();
    }

    public static class HealthScoreCalculator
    {
        public static HealthScoreResult Calculate(
            int batteryHealthPercent,
            int diskWearPercent,
            int diskErrorCount,
            int suddenShutdownCount,
            int crashCount,
            double temperatureCelsius,
            int ramUsagePercent,
            int daysSinceOsUpdate)
        {
            var score = 100;
            var reasons = new List<string>();

            var batteryPenalty = BatteryPenalty(batteryHealthPercent);
            if (batteryPenalty > 0)
            {
                score -= batteryPenalty;
                reasons.Add($"Battery health is {batteryHealthPercent}% (-{batteryPenalty} points).");
            }

            var diskWearPenalty = DiskWearPenalty(diskWearPercent);
            if (diskWearPenalty > 0)
            {
                score -= diskWearPenalty;
                reasons.Add($"Disk wear is {diskWearPercent}% (-{diskWearPenalty} points).");
            }

            var diskErrorPenalty = DiskErrorPenalty(diskErrorCount);
            if (diskErrorPenalty > 0)
            {
                score -= diskErrorPenalty;
                reasons.Add($"{diskErrorCount} disk read/write error(s) since the last check-in (-{diskErrorPenalty} points).");
            }

            var shutdownPenalty = ShutdownPenalty(suddenShutdownCount);
            if (shutdownPenalty > 0)
            {
                score -= shutdownPenalty;
                reasons.Add($"{suddenShutdownCount} sudden shutdown(s) since the last check-in (-{shutdownPenalty} points).");
            }

            var crashPenalty = CrashPenalty(crashCount);
            if (crashPenalty > 0)
            {
                score -= crashPenalty;
                reasons.Add($"{crashCount} crash(es) since the last check-in (-{crashPenalty} points).");
            }

            var temperaturePenalty = TemperaturePenalty(temperatureCelsius);
            if (temperaturePenalty > 0)
            {
                score -= temperaturePenalty;
                reasons.Add($"Running hot at {temperatureCelsius}°C (-{temperaturePenalty} points).");
            }

            var flags = new List<string>();
            if (ramUsagePercent >= 90)
            {
                flags.Add("Underpowered - RAM usage has been consistently high.");
            }
            if (daysSinceOsUpdate > 60)
            {
                flags.Add("Update overdue - no OS update in over 60 days.");
            }

            score = Math.Max(0, score);

            return new HealthScoreResult
            {
                Score = score,
                Band = GetBand(score),
                Reasons = reasons,
                Flags = flags
            };
        }

        public static string GetBand(int score)
        {
            if (score >= 80) return "Healthy";
            if (score >= 60) return "Watch";
            return "ActNow";
        }

        private static int BatteryPenalty(int value)
        {
            if (value >= 80) return 0;
            if (value >= 70) return 5;
            if (value >= 60) return 12;
            if (value >= 50) return 18;
            return 25;
        }

        private static int DiskWearPenalty(int value)
        {
            if (value < 50) return 0;
            if (value < 70) return 8;
            if (value < 85) return 14;
            return 20;
        }

        private static int DiskErrorPenalty(int value)
        {
            if (value == 0) return 0;
            if (value <= 5) return 8;
            if (value <= 20) return 14;
            return 20;
        }

        private static int ShutdownPenalty(int value)
        {
            if (value == 0) return 0;
            if (value == 1) return 6;
            if (value <= 3) return 10;
            return 15;
        }

        private static int CrashPenalty(int value)
        {
            if (value == 0) return 0;
            if (value <= 2) return 4;
            if (value <= 5) return 7;
            return 10;
        }

        private static int TemperaturePenalty(double value)
        {
            if (value < 45) return 0;
            if (value < 55) return 4;
            if (value < 65) return 7;
            return 10;
        }
    }
}
