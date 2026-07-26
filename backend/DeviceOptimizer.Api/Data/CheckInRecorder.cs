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
                CrashCount = dto.CrashCount,
                TemperatureCelsius = dto.TemperatureCelsius,
                ActiveUseHours = dto.ActiveUseHours,
                Timestamp = DateTime.UtcNow
            };
            db.CheckIns.Add(checkIn);

            device.LastBatteryHealthPercent = dto.BatteryHealthPercent;
            device.LastDiskWearPercent = dto.DiskWearPercent;
            device.LastCrashCount = dto.CrashCount;
            device.LastTemperatureCelsius = dto.TemperatureCelsius;
            device.LastActiveUseHours = dto.ActiveUseHours;
            device.LastCheckInAt = checkIn.Timestamp;

            await db.SaveChangesAsync();
            return device;
        }
    }
}
