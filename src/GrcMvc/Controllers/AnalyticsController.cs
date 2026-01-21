using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// MVC Controller for Analytics dashboard pages.
    /// Uses /api/analyticsdashboard/* API endpoints for data.
    /// </summary>
    [Authorize(Roles = "Admin,PlatformAdmin,TenantAdmin")]
    [Route("[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(ILogger<AnalyticsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Main analytics dashboard - overview of all metrics
        /// </summary>
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation("Analytics dashboard accessed by {User}", User.Identity?.Name);
            return View();
        }

        /// <summary>
        /// Compliance-focused analytics
        /// </summary>
        [HttpGet("Compliance")]
        public IActionResult Compliance()
        {
            return View();
        }

        /// <summary>
        /// Risk-focused analytics with heatmap
        /// </summary>
        [HttpGet("Risk")]
        public IActionResult Risk()
        {
            return View();
        }

        /// <summary>
        /// Assessment analytics and trends
        /// </summary>
        [HttpGet("Assessments")]
        public IActionResult Assessments()
        {
            return View();
        }

        /// <summary>
        /// User activity and engagement metrics
        /// </summary>
        [HttpGet("Activity")]
        public IActionResult Activity()
        {
            return View();
        }
    }
}
