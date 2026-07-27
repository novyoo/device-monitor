using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _db;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext db,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _logger = logger;
        }

        [HttpGet("tenants")]
        public async Task<ActionResult> GetTenants()
        {
            var tenants = await _db.Tenants
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();
            return Ok(tenants);
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var tenantExists = await _db.Tenants.AnyAsync(t => t.Id == dto.TenantId);
            if (!tenantExists)
            {
                return BadRequest(new[] { "Please choose which company this account belongs to." });
            }

            var user = new AppUser { UserName = dto.Email, Email = dto.Email, TenantId = dto.TenantId };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = $"https://localhost:5173/?confirmUserId={user.Id}&confirmToken={Uri.EscapeDataString(token)}";
            _logger.LogInformation(
                "EMAIL VERIFICATION LINK for {Email} (this stands in for a real email in this demo): {Url}",
                user.Email, confirmUrl);

            return Ok(new
            {
                message = "Account created. Check the backend console window for a verification link " +
                           "(this demo logs it instead of sending a real email)."
            });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return BadRequest("This verification link is invalid.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
            if (!result.Succeeded)
            {
                return BadRequest("This verification link is invalid or has expired.");
            }

            return Ok(new { message = "Your email is confirmed. You can log in now." });
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<CurrentUserDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized("Incorrect email or password.");
            }

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: true, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return StatusCode(423, "Too many failed attempts. This account is locked for 15 minutes.");
            }
            if (result.IsNotAllowed)
            {
                return Unauthorized("Please confirm your email first - check the backend console for your verification link.");
            }
            if (!result.Succeeded)
            {
                return Unauthorized("Incorrect email or password.");
            }

            return Ok(await BuildCurrentUserDto(user));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<CurrentUserDto>> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(await BuildCurrentUserDto(user));
        }

        private async Task<CurrentUserDto> BuildCurrentUserDto(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            string? tenantName = null;
            if (user.TenantId != null)
            {
                var tenant = await _db.Tenants.FindAsync(user.TenantId);
                tenantName = tenant?.Name;
            }

            return new CurrentUserDto
            {
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Customer",
                TenantId = user.TenantId,
                TenantName = tenantName
            };
        }
    }
}
