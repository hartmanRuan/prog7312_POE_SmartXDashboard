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
        public IActionResult GetTelemetry([FromQuery] string macAddress = "ALL")
        {
            var telemetry = SensorRepository.Instance.GetTelemetry();

            if (!string.IsNullOrEmpty(macAddress) && !macAddress.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                telemetry = telemetry.Where(t => t.MacAddress.Equals(macAddress, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(telemetry.ToList());
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
                    LocationZone = packet.LocationZone
                });
            }

            return Ok(new { status = "Telemetry stored successfully" });
        }
    }
}