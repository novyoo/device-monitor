using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Data
{
    public static class CheckInRecorder
    {
        public static async Task<Device?> RecordAsync(AppDbContext db, CheckInDto dto)
        {
            var device = await db.Devices.FirstOrDefaultAsync(d => d.ApiKey == dto.ApiKey);
            if (device == null) return null;

            var checkIn = new CheckIn
            {
                DeviceId = device.Id,
                BatteryHealthPercent = dto.BatteryHealthPercent,
                DiskWearPercent = dto.DiskWearPercent,
                DiskErrorCount = dto.DiskErrorCount,
                CrashCount = dto.CrashCount,
                SuddenShutdownCount = dto.SuddenShutdownCount,
                TemperatureCelsius = dto.TemperatureCelsius,
                RamUsagePercent = dto.RamUsagePercent,
                ActiveUseHours = dto.ActiveUseHours,
                DaysSinceOsUpdate = dto.DaysSinceOsUpdate,
                Timestamp = DateTime.UtcNow
            };
            db.CheckIns.Add(checkIn);

            device.LastBatteryHealthPercent = dto.BatteryHealthPercent;
            device.LastDiskWearPercent = dto.DiskWearPercent;
            device.LastDiskErrorCount = dto.DiskErrorCount;
            device.LastCrashCount = dto.CrashCount;
            device.LastSuddenShutdownCount = dto.SuddenShutdownCount;
            device.LastTemperatureCelsius = dto.TemperatureCelsius;
            device.LastRamUsagePercent = dto.RamUsagePercent;
            device.LastActiveUseHours = dto.ActiveUseHours;
            device.LastDaysSinceOsUpdate = dto.DaysSinceOsUpdate;
            device.LastCheckInAt = checkIn.Timestamp;

            var previousCheckIns = await db.CheckIns
                .Where(c => c.DeviceId == device.Id)
                .OrderByDescending(c => c.Timestamp)
                .Take(2)
                .ToListAsync();

            var recentBattery = new List<int> { dto.BatteryHealthPercent };
            var recentDiskWear = new List<int> { dto.DiskWearPercent };
            var recentDiskErrors = new List<int> { dto.DiskErrorCount };
            var recentShutdowns = new List<int> { dto.SuddenShutdownCount };
            var recentCrashes = new List<int> { dto.CrashCount };
            var recentTemperature = new List<double> { dto.TemperatureCelsius };

            foreach (var previous in previousCheckIns)
            {
                recentBattery.Add(previous.BatteryHealthPercent);
                recentDiskWear.Add(previous.DiskWearPercent);
                recentDiskErrors.Add(previous.DiskErrorCount);
                recentShutdowns.Add(previous.SuddenShutdownCount);
                recentCrashes.Add(previous.CrashCount);
                recentTemperature.Add(previous.TemperatureCelsius);
            }

            device.Avg3BatteryHealthPercent = (int)Math.Round(recentBattery.Average());
            device.Avg3DiskWearPercent = (int)Math.Round(recentDiskWear.Average());
            device.Avg3DiskErrorCount = (int)Math.Round(recentDiskErrors.Average());
            device.Avg3SuddenShutdownCount = (int)Math.Round(recentShutdowns.Average());
            device.Avg3CrashCount = (int)Math.Round(recentCrashes.Average());
            device.Avg3TemperatureCelsius = Math.Round(recentTemperature.Average(), 1);

            await db.SaveChangesAsync();
            return device;
        }
    }
}
