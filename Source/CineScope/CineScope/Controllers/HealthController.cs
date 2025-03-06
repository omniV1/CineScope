using Microsoft.AspNetCore.Mvc;

namespace CineScope.Controllers
{
    [ApiController]
    [Route("health")]  // Simple, direct route - no 'api' prefix
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "API is working", time = DateTime.UtcNow });
        }
    }
}