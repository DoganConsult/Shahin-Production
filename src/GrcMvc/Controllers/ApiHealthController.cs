using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// API Health check endpoint
    /// </summary>
    [Route("api")]
    [ApiController]
    public class ApiHealthController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
