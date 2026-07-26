using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;
using DeviceOptimizer.Api.Services;

namespace DeviceOptimizer.Api.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DevicesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAllDevices()
        {
            var devices = await _db.Devices.Include(d => d.Tenant).ToListAsync();
            var dtos = devices.Select(MapToDeviceDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("returns")]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetReturnedDevices()
        {
            var devices = await _db.Devices
                .Include(d => d.Tenant)
                .Where(d => d.Status == DeviceStatus.Returned)
                .ToListAsync();

            var deviceIds = devices.Select(d => d.Id).ToList();
            var lifetimeHoursByDevice = await _db.CheckIns
                .Where(c => deviceIds.Contains(c.DeviceId))
                .GroupBy(c => c.DeviceId)
                .Select(g => new { DeviceId = g.Key, TotalHours = g.Sum(c => c.ActiveUseHours) })
                .ToDictionaryAsync(x => x.DeviceId, x => x.TotalHours);

            var dtos = devices.Select(d =>
            {
                var dto = MapToDeviceDto(d);

                var healthReasons = new List<string>();
                var healthFlags = new List<string>();
                if (d.LastCheckInAt != null)
                {
                    var healthResult = HealthScoreCalculator.Calculate(
                        d.LastBatteryHealthPercent!.Value,
                        d.LastDiskWearPercent!.Value,
                        d.LastDiskErrorCount!.Value,
                        d.LastSuddenShutdownCount!.Value,
                        d.LastCrashCount!.Value,
                        d.LastTemperatureCelsius!.Value,
                        d.LastRamUsagePercent!.Value,
                        d.LastDaysSinceOsUpdate!.Value);
                    healthReasons = healthResult.Reasons;
                    healthFlags = healthResult.Flags;
                }

                var lifetimeHours = lifetimeHoursByDevice.TryGetValue(d.Id, out var hours) ? hours : 0;

                var recommendation = RecommendationEngine.Recommend(
                    dto.HealthScore,
                    dto.HealthBand,
                    healthReasons,
                    healthFlags,
                    d.PurchaseDate,
                    d.RepairCount,
                    lifetimeHours);

                dto.Recommendation = recommendation.Action;
                dto.RecommendationReasons = recommendation.Reasons;
                return dto;
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}/detail")]
        public async Task<ActionResult<DeviceDetailDto>> GetDeviceDetail(int id)
        {
            var device = await _db.Devices.Include(d => d.Tenant).FirstOrDefaultAsync(d => d.Id == id);
            if (device == null) return NotFound();

            var recentCheckIns = await _db.CheckIns
                .Where(c => c.DeviceId == id)
                .OrderByDescending(c => c.Timestamp)
                .Take(30)
                .ToListAsync();
            recentCheckIns.Reverse();

            var detail = new DeviceDetailDto
            {
                Id = device.Id,
                TenantName = device.Tenant!.Name,
                Model = device.Model,
                SerialNumber = device.SerialNumber,
                Status = device.Status.ToString(),
                LastCheckInAt = device.LastCheckInAt,
                BatteryHealthPercent = device.LastBatteryHealthPercent,
                DiskWearPercent = device.LastDiskWearPercent,
                DiskErrorCount = device.LastDiskErrorCount,
                CrashCount = device.LastCrashCount,
                SuddenShutdownCount = device.LastSuddenShutdownCount,
                TemperatureCelsius = device.LastTemperatureCelsius,
                RamUsagePercent = device.LastRamUsagePercent,
                ActiveUseHours = device.LastActiveUseHours,
                DaysSinceOsUpdate = device.LastDaysSinceOsUpdate,
                History = recentCheckIns.Select(c => new HealthHistoryPointDto
                {
                    Timestamp = c.Timestamp,
                    Score = HealthScoreCalculator.Calculate(
                        c.BatteryHealthPercent,
                        c.DiskWearPercent,
                        c.DiskErrorCount,
                        c.SuddenShutdownCount,
                        c.CrashCount,
                        c.TemperatureCelsius,
                        c.RamUsagePercent,
                        c.DaysSinceOsUpdate).Score
                }).ToList()
            };

            if (device.LastCheckInAt != null)
            {
                var result = HealthScoreCalculator.Calculate(
                    device.LastBatteryHealthPercent!.Value,
                    device.LastDiskWearPercent!.Value,
                    device.LastDiskErrorCount!.Value,
                    device.LastSuddenShutdownCount!.Value,
                    device.LastCrashCount!.Value,
                    device.LastTemperatureCelsius!.Value,
                    device.LastRamUsagePercent!.Value,
                    device.LastDaysSinceOsUpdate!.Value);

                detail.HealthScore = result.Score;
                detail.HealthBand = result.Band;
                detail.Reasons = result.Reasons;
                detail.Flags = result.Flags;
            }

            return Ok(detail);
        }

        private static DeviceDto MapToDeviceDto(Device d)
        {
            var dto = new DeviceDto
            {
                Id = d.Id,
                TenantName = d.Tenant!.Name,
                Model = d.Model,
                SerialNumber = d.SerialNumber,
                PurchaseDate = d.PurchaseDate,
                RepairCount = d.RepairCount,
                Status = d.Status.ToString(),
                ReturnedAt = d.ReturnedAt
            };

            if (d.LastCheckInAt != null)
            {
                var result = HealthScoreCalculator.Calculate(
                    d.LastBatteryHealthPercent!.Value,
                    d.LastDiskWearPercent!.Value,
                    d.LastDiskErrorCount!.Value,
                    d.LastSuddenShutdownCount!.Value,
                    d.LastCrashCount!.Value,
                    d.LastTemperatureCelsius!.Value,
                    d.LastRamUsagePercent!.Value,
                    d.LastDaysSinceOsUpdate!.Value);

                dto.HealthScore = result.Score;
                dto.HealthBand = result.Band;
            }

            return dto;
        }

        [HttpPost("{id}/rent")]
        public async Task<IActionResult> RentDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.InStock)
                return BadRequest($"Device is {device.Status}, not InStock. Cannot rent it out.");

            device.Status = DeviceStatus.Rented;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Rented)
                return BadRequest($"Device is {device.Status}, not Rented. Cannot return it.");

            device.Status = DeviceStatus.Returned;
            device.ReturnedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/restock")]
        public async Task<IActionResult> RestockDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot restock it.");

            device.Status = DeviceStatus.InStock;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/repair")]
        public async Task<IActionResult> SendToRepair(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot send it to repair.");

            device.Status = DeviceStatus.InRepair;
            device.RepairCount += 1;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/retire")]
        public async Task<IActionResult> RetireDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot retire it.");

            device.Status = DeviceStatus.Retired;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("returns/stats")]
        public async Task<ActionResult<ReturnStatsDto>> GetReturnStats()
        {
            var now = DateTime.UtcNow;

            var returnedThisMonth = await _db.Devices.CountAsync(d =>
                d.ReturnedAt != null &&
                d.ReturnedAt.Value.Month == now.Month &&
                d.ReturnedAt.Value.Year == now.Year);

            var awaitingDecision = await _db.Devices.CountAsync(d => d.Status == DeviceStatus.Returned);

            return Ok(new ReturnStatsDto
            {
                Month = now.ToString("MMMM yyyy"),
                ReturnedThisMonth = returnedThisMonth,
                AwaitingDecision = awaitingDecision
            });
        }
    }
}
