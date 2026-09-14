using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using System.Collections.Concurrent;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Simple thread-safe in-memory user store for testing
        private static readonly ConcurrentDictionary<string, User> _users = new();

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            if (_users.ContainsKey(request.Username.ToLower()))
                return Conflict("Username already exists.");

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password, // Simple check for now
                Role = request.Role
            };

            _users.TryAdd(request.Username.ToLower(), newUser);
            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (_users.TryGetValue(request.Username.ToLower(), out var user))
            {
                if (user.PasswordHash == request.Password)
                {
                    return Ok(new { username = user.Username, role = user.Role, message = "Login successful" });
                }
            }

            return Unauthorized("Invalid username or password.");
        }
    }
}