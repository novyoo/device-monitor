using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public DevicesController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<(AppUser User, bool IsAdmin)> GetCurrentUserAsync()
        {
            var user = (await _userManager.GetUserAsync(User))!;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            return (user, isAdmin);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAllDevices()
        {
            var (currentUser, isAdmin) = await GetCurrentUserAsync();

            var query = _db.Devices.Include(d => d.Tenant).AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(d => d.TenantId == currentUser.TenantId);
            }

            var devices = await query.ToListAsync();
            var dtos = devices.Select(MapToDeviceDto).ToList();
            return Ok(dtos);
        }

        [HttpGet("returns")]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetReturnedDevices()
        {
            var (currentUser, isAdmin) = await GetCurrentUserAsync();

            var query = _db.Devices
                .Include(d => d.Tenant)
                .Where(d => d.Status == DeviceStatus.Returned)
                .AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(d => d.TenantId == currentUser.TenantId);
            }

            var devices = await query.ToListAsync();

            var deviceIds = devices.Select(d => d.Id).ToList();
            var lifetimeHoursByDevice = await _db.CheckIns
                .Where(c => deviceIds.Contains(c.DeviceId))
                .GroupBy(c => c.DeviceId)
                .Select(g => new { DeviceId = g.Key, TotalHours = g.Sum(c => c.ActiveUseHours) })
                .ToDictionaryAsync(x => x.DeviceId, x => x.TotalHours);

            var dtos = devices.Select(d =>
            {
                var dto = MapToDeviceDto(d);

                var healthResult = GetHealthResult(d);
                var healthReasons = healthResult?.Reasons ?? new List<string>();
                var healthFlags = healthResult?.Flags ?? new List<string>();

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

            var (currentUser, isAdmin) = await GetCurrentUserAsync();
            if (!isAdmin && device.TenantId != currentUser.TenantId)
            {
                return Forbid();
            }

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

            var currentResult = GetHealthResult(device);
            if (currentResult != null)
            {
                detail.HealthScore = currentResult.Score;
                detail.HealthBand = currentResult.Band;
                detail.Reasons = currentResult.Reasons;
                detail.Flags = currentResult.Flags;
            }

            detail.TrendMessage = TrendPredictor.Predict(detail.History)?.Message;

            return Ok(detail);
        }

        private static HealthScoreResult? GetHealthResult(Device d) => HealthScoreCalculator.CalculateForDevice(d);

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

            var result = GetHealthResult(d);
            if (result != null)
            {
                dto.HealthScore = result.Score;
                dto.HealthBand = result.Band;
            }

            return dto;
        }

        private async Task RecordDecisionAsync(Device device, string actualAction)
        {
            var healthResult = GetHealthResult(device);
            if (healthResult == null) return;

            var lifetimeHours = await _db.CheckIns
                .Where(c => c.DeviceId == device.Id)
                .SumAsync(c => c.ActiveUseHours);

            var recommendation = RecommendationEngine.Recommend(
                healthResult.Score,
                healthResult.Band,
                healthResult.Reasons,
                healthResult.Flags,
                device.PurchaseDate,
                device.RepairCount,
                lifetimeHours);

            _db.DecisionRecords.Add(new DecisionRecord
            {
                DeviceId = device.Id,
                RecommendedAction = recommendation.Action,
                ActualAction = actualAction,
                DecidedAt = DateTime.UtcNow
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RegisteredDeviceDto>> RegisterRealDevice(RegisterDeviceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Model) || string.IsNullOrWhiteSpace(dto.SerialNumber))
                return BadRequest("Model and serial number are both required.");

            var model = dto.Model.Trim();
            var serialNumber = dto.SerialNumber.Trim();

            var tenant = await _db.Tenants.FindAsync(dto.TenantId);
            if (tenant == null) return BadRequest("Unknown tenant.");

            var serialTaken = await _db.Devices.AnyAsync(d => d.SerialNumber == serialNumber);
            if (serialTaken) return BadRequest("A device with that serial number already exists.");

            var device = new Device
            {
                TenantId = tenant.Id,
                Model = model,
                SerialNumber = serialNumber,
                PurchaseDate = dto.PurchaseDate ?? DateTime.UtcNow,
                Status = DeviceStatus.Rented,
                Personality = DevicePersonality.RealDevice
            };

            _db.Devices.Add(device);
            await _db.SaveChangesAsync();

            return Ok(new RegisteredDeviceDto { Id = device.Id, ApiKey = device.ApiKey });
        }

        [HttpPost("{id}/rent")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestockDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot restock it.");

            await RecordDecisionAsync(device, "RentAgain");
            device.Status = DeviceStatus.InStock;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/resell")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResellDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot send it for resale.");

            await RecordDecisionAsync(device, "Resale");
            device.Status = DeviceStatus.Resale;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/repair")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendToRepair(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot send it to repair.");

            await RecordDecisionAsync(device, "Repair");
            device.Status = DeviceStatus.InRepair;
            device.RepairCount += 1;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/retire")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RetireDevice(int id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();
            if (device.Status != DeviceStatus.Returned)
                return BadRequest($"Device is {device.Status}, not Returned. Cannot retire it.");

            await RecordDecisionAsync(device, "Retire");
            device.Status = DeviceStatus.Retired;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("returns/stats")]
        public async Task<ActionResult<ReturnStatsDto>> GetReturnStats()
        {
            var (currentUser, isAdmin) = await GetCurrentUserAsync();
            var now = DateTime.UtcNow;

            var query = _db.Devices.AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(d => d.TenantId == currentUser.TenantId);
            }

            var returnedThisMonth = await query.CountAsync(d =>
                d.ReturnedAt != null &&
                d.ReturnedAt.Value.Month == now.Month &&
                d.ReturnedAt.Value.Year == now.Year);

            var awaitingDecision = await query.CountAsync(d => d.Status == DeviceStatus.Returned);

            var decisionQuery = _db.DecisionRecords.Include(r => r.Device).AsQueryable();
            if (!isAdmin)
            {
                decisionQuery = decisionQuery.Where(r => r.Device!.TenantId == currentUser.TenantId);
            }

            var totalDecisions = await decisionQuery.CountAsync();
            int? agreementPercent = null;
            if (totalDecisions > 0)
            {
                var agreedDecisions = await decisionQuery.CountAsync(r => r.RecommendedAction == r.ActualAction);
                agreementPercent = (int)Math.Round(100.0 * agreedDecisions / totalDecisions);
            }

            return Ok(new ReturnStatsDto
            {
                Month = now.ToString("MMMM yyyy"),
                ReturnedThisMonth = returnedThisMonth,
                AwaitingDecision = awaitingDecision,
                AgreementPercent = agreementPercent
            });
        }
    }
}
