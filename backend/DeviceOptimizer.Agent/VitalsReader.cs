using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Xml.Linq;

namespace DeviceOptimizer.Agent
{
    public static class VitalsReader
    {
        private static DateTime? lastReadTime;
        private static long previousDiskErrorTotal;
        private static bool hasPreviousDiskErrorTotal;

        public static CheckInMessage ReadCheckIn(string apiKey, int intervalMinutes)
        {
            var since = lastReadTime ?? DateTime.Now.AddMinutes(-intervalMinutes);
            lastReadTime = DateTime.Now;

            var (diskWearPercent, diskErrorCount) = ReadDiskWearAndErrors();

            return new CheckInMessage
            {
                ApiKey = apiKey,
                BatteryHealthPercent = ReadBatteryHealthPercent(),
                DiskWearPercent = diskWearPercent,
                DiskErrorCount = diskErrorCount,
                CrashCount = CountWindowsEvents("Application", 1000, since),
                SuddenShutdownCount = CountWindowsEvents("System", 41, since),
                TemperatureCelsius = ReadTemperatureCelsius(),
                RamUsagePercent = ReadRamUsagePercent(),
                ActiveUseHours = ReadActiveUseHours(since),
                DaysSinceOsUpdate = ReadDaysSinceOsUpdate()
            };
        }

        private static int ReadBatteryHealthPercent()
        {
            try
            {
                var reportPath = Path.Combine(Path.GetTempPath(), "pulsle-battery-report.xml");

                using var powercfg = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/batteryreport /output \"{reportPath}\" /xml",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                powercfg!.WaitForExit(10000);

                var report = XDocument.Load(reportPath);
                File.Delete(reportPath);

                var designCapacity = ReadXmlNumber(report, "DesignCapacity");
                var fullChargeCapacity = ReadXmlNumber(report, "FullChargeCapacity");
                if (designCapacity <= 0 || fullChargeCapacity <= 0) return 100;

                return (int)Math.Clamp(Math.Round(100.0 * fullChargeCapacity / designCapacity), 0, 100);
            }
            catch
            {
                return 100;
            }
        }

        private static double ReadXmlNumber(XDocument document, string elementName)
        {
            var element = document.Descendants().FirstOrDefault(e => e.Name.LocalName == elementName);
            return element == null ? 0 : Convert.ToDouble(element.Value, CultureInfo.InvariantCulture);
        }

        private static (int WearPercent, int ErrorsSinceLastCheckIn) ReadDiskWearAndErrors()
        {
            try
            {
                double highestWear = 0;
                long errorTotal = 0;

                using var searcher = new ManagementObjectSearcher(
                    "root\\microsoft\\windows\\storage",
                    "SELECT Wear, ReadErrorsTotal, WriteErrorsTotal FROM MSFT_StorageReliabilityCounter");

                foreach (ManagementObject disk in searcher.Get())
                {
                    highestWear = Math.Max(highestWear, NumberOrZero(disk["Wear"]));
                    errorTotal += (long)NumberOrZero(disk["ReadErrorsTotal"]) + (long)NumberOrZero(disk["WriteErrorsTotal"]);
                }

                var errorsSinceLast = hasPreviousDiskErrorTotal ? Math.Max(0, errorTotal - previousDiskErrorTotal) : 0;
                previousDiskErrorTotal = errorTotal;
                hasPreviousDiskErrorTotal = true;

                return ((int)highestWear, (int)errorsSinceLast);
            }
            catch
            {
                return (0, 0);
            }
        }

        private static int CountWindowsEvents(string logName, int eventCode, DateTime since)
        {
            try
            {
                var sinceInWmiFormat = ManagementDateTimeConverter.ToDmtfDateTime(since);
                var query = $"SELECT * FROM Win32_NTLogEvent WHERE Logfile = '{logName}' AND EventCode = {eventCode} AND TimeGenerated > '{sinceInWmiFormat}'";

                using var searcher = new ManagementObjectSearcher(query);
                return searcher.Get().Count;
            }
            catch
            {
                return 0;
            }
        }

        private static double ReadTemperatureCelsius()
        {
            try
            {
                var tenthsOfKelvin = ReadSingleWmiNumber("root\\wmi", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature", "CurrentTemperature");
                if (tenthsOfKelvin <= 0) return 0;

                return Math.Round(tenthsOfKelvin / 10.0 - 273.15, 1);
            }
            catch
            {
                return 0;
            }
        }

        private static int ReadRamUsagePercent()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");
                foreach (ManagementObject os in searcher.Get())
                {
                    var totalKb = NumberOrZero(os["TotalVisibleMemorySize"]);
                    var freeKb = NumberOrZero(os["FreePhysicalMemory"]);
                    if (totalKb <= 0) return 0;

                    return (int)Math.Round(100.0 * (totalKb - freeKb) / totalKb);
                }
            }
            catch
            {
            }
            return 0;
        }

        private static double ReadActiveUseHours(DateTime since)
        {
            var hoursSinceBoot = Environment.TickCount64 / 3600000.0;
            var hoursSinceLastCheckIn = (DateTime.Now - since).TotalHours;

            return Math.Round(Math.Min(hoursSinceBoot, hoursSinceLastCheckIn), 1);
        }

        private static int ReadDaysSinceOsUpdate()
        {
            try
            {
                var newestUpdate = DateTime.MinValue;

                using var searcher = new ManagementObjectSearcher("SELECT InstalledOn FROM Win32_QuickFixEngineering");
                foreach (ManagementObject update in searcher.Get())
                {
                    if (DateTime.TryParse(update["InstalledOn"]?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var installedOn) && installedOn > newestUpdate)
                    {
                        newestUpdate = installedOn;
                    }
                }

                if (newestUpdate == DateTime.MinValue) return 0;

                return Math.Max(0, (int)(DateTime.Now - newestUpdate).TotalDays);
            }
            catch
            {
                return 0;
            }
        }

        private static double ReadSingleWmiNumber(string scope, string query, string propertyName)
        {
            using var searcher = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject item in searcher.Get())
            {
                return NumberOrZero(item[propertyName]);
            }
            return 0;
        }

        private static double NumberOrZero(object? value)
        {
            return value == null ? 0 : Convert.ToDouble(value);
        }
    }
}
