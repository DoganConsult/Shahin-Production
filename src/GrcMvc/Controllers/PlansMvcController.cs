using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC controller for Plans UI - redirects to API endpoint or displays plan management interface
    /// </summary>
    [Route("plans")]
    [Authorize]
    public class PlansMvcController : Controller
    {
        private readonly ILogger<PlansMvcController> _logger;

        public PlansMvcController(ILogger<PlansMvcController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Plans index page - displays plan management UI
        /// </summary>
        [HttpGet]
        [HttpGet("index")]
        public IActionResult Index()
        {
            // Return view that will call API endpoints for data
            return View();
        }
    }
}
