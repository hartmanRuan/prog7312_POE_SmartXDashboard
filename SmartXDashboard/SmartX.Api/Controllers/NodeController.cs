using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartX.Api.Data;
using SmartX.Api.Models;

namespace SmartX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NodesController : ControllerBase
    {
        private readonly SmartXDbContext _context;

        public NodesController(SmartXDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterNode([FromBody] SensorNodeDto model)
        {
            var user = await _context.Users
                .Include(u => u.SensorNode)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (user.SensorNode != null)
            {
                return BadRequest(new { message = "User already has a registered node. Only 1 node allowed per account." });
            }

            // Check if MacAddress or Barcode is already globally registered
            if (await _context.SensorNodes.AnyAsync(n => n.MacAddress == model.MacAddress || n.Barcode == model.Barcode))
            {
                return BadRequest(new { message = "Mac Address or Barcode is already registered." });
            }

            var node = new SensorNode
            {
                MacAddress = model.MacAddress,
                Barcode = model.Barcode,
                LocationZone = model.LocationZone,
                UserId = model.UserId
            };

            _context.SensorNodes.Add(node);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Node registered successfully", node });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNode(int userId)
        {
            var node = await _context.SensorNodes.FirstOrDefaultAsync(n => n.UserId == userId);
            if (node == null) return NotFound(new { message = "No node registered for this user." });
            return Ok(node);
        }

        [HttpPost("{macAddress}/upload")]
        public async Task<IActionResult> UploadFile(string macAddress, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var node = await _context.SensorNodes.FirstOrDefaultAsync(n => n.MacAddress == macAddress);
            if (node == null)
                return NotFound(new { message = "Node not found." });

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, $"{Guid.NewGuid()}_{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            node.FilePath = filePath;
            await _context.SaveChangesAsync();

            return Ok(new { message = "File uploaded successfully", path = filePath });
        }
    }

    public class SensorNodeDto
    {
        public int UserId { get; set; }
        public string MacAddress { get; set; }
        public string Barcode { get; set; }
        public string LocationZone { get; set; }
    }
}