using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;
using System.Linq;

namespace SmartX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            if (UserRepository.VerifyUser(request.Username, request.Password, out var user))
            {
                return Ok(new { message = "Login successful", user.Username, user.Role });
            }

            return Unauthorized(new { message = "Invalid username or password." });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (UserRepository.Users.Any(u => u.Username.Equals(request.Username, System.StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Username already exists." });
            }

            UserRepository.AddUser(request.Username, request.Password, request.Role);
            return Ok(new { message = "User registered successfully." });
        }
    }
}