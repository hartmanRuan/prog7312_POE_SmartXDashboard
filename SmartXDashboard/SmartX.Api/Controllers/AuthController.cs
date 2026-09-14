using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartX.Api.Data;
using SmartX.Api.Models;

namespace SmartX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SmartXDbContext _context;

        public AuthController(SmartXDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto model)
        {
            var user = await _context.Users
                .Include(u => u.SensorNode)
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null || user.PasswordHash != model.Password) // Simple check for prototype; use proper hashing in production
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            return Ok(new
            {
                userId = user.Id,
                username = user.Username,
                hasNode = user.SensorNode != null,
                node = user.SensorNode
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto model)
        {
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password // Again, use hashing for production
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }
    }

    public class UserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}