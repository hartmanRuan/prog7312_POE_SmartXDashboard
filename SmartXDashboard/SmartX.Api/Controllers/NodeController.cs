using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NodesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetNodes()
        {
            var nodes = SensorRepository.Instance.GetNodes();
            return Ok(nodes.ToList());
        }

        [HttpPost]
        public IActionResult RegisterNode([FromBody] SensorNode node)
        {
            if (node == null) return BadRequest("Invalid node data.");

            SensorRepository.Instance.RegisterNode(node);
            return Ok(new { status = "Node registered successfully" });
        }
    }
}