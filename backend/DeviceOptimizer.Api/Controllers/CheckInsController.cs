using Microsoft.AspNetCore.Mvc;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;

namespace DeviceOptimizer.Api.Controllers
{
    [ApiController]
    [Route("api/checkins")]
    public class CheckInsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CheckInsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CheckInDto dto)
        {
            var device = await CheckInRecorder.RecordAsync(_db, dto);
            if (device == null) return Unauthorized("Unknown API key.");

            return Ok();
        }
    }
}
