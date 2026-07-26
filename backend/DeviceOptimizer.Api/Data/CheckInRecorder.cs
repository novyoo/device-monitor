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

            await db.SaveChangesAsync();
            return device;
        }
    }
}
