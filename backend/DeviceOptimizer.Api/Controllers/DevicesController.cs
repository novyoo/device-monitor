using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;

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
            var devices = await _db.Devices
                .Include(d => d.Tenant)
                .Select(d => new DeviceDto
                {
                    Id = d.Id,
                    TenantName = d.Tenant!.Name,
                    Model = d.Model,
                    SerialNumber = d.SerialNumber,
                    PurchaseDate = d.PurchaseDate,
                    RepairCount = d.RepairCount,
                    Status = d.Status.ToString(),
                    ReturnedAt = d.ReturnedAt,
                    BatteryHealthPercent = d.LastBatteryHealthPercent,
                    DiskWearPercent = d.LastDiskWearPercent,
                    CrashCount = d.LastCrashCount,
                    TemperatureCelsius = d.LastTemperatureCelsius,
                    ActiveUseHours = d.LastActiveUseHours,
                    LastCheckInAt = d.LastCheckInAt
                })
                .ToListAsync();

            return Ok(devices);
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
