using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public AlertsController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlertDto>>> GetAlerts()
        {
            var user = (await _userManager.GetUserAsync(User))!;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var query = _db.Alerts
                .Include(a => a.Device)
                .Where(a => !a.Dismissed)
                .AsQueryable();
            if (!isAdmin)
            {
                query = query.Where(a => a.Device!.TenantId == user.TenantId);
            }

            var alerts = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

            var dtos = alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                DeviceId = a.DeviceId,
                Model = a.Device!.Model,
                SerialNumber = a.Device.SerialNumber,
                Message = a.Message,
                CreatedAt = a.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost("{id}/dismiss")]
        public async Task<IActionResult> DismissAlert(int id)
        {
            var alert = await _db.Alerts.Include(a => a.Device).FirstOrDefaultAsync(a => a.Id == id);
            if (alert == null) return NotFound();

            var user = (await _userManager.GetUserAsync(User))!;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && alert.Device!.TenantId != user.TenantId)
            {
                return Forbid();
            }

            alert.Dismissed = true;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
