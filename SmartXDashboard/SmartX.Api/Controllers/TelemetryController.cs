using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTelemetry()
        {
            var logs = SensorRepository.Instance.GetTelemetry();
            return Ok(logs);
        }

        [HttpPost]
        public IActionResult PostTelemetry([FromBody] TelemetryPacket<double> packet)
        {
            if (packet == null) return BadRequest("Invalid packet data.");

            SensorRepository.Instance.AddTelemetry(packet);

            if (!string.IsNullOrEmpty(packet.MacAddress))
            {
                SensorRepository.Instance.RegisterNode(new SensorNode
                {
                    MacAddress = packet.MacAddress,
                    LocationZone = packet.LocationZone.ToString()
                });
            }

            return Ok(new { status = "Telemetry stored successfully" });
        }

        [HttpGet("nodes")]
        public IActionResult GetNodes()
        {
            var nodes = SensorRepository.Instance.GetNodes();
            return Ok(nodes);
        }
    }
}