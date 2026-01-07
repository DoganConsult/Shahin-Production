using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.Controllers;

/// <summary>
/// Dashboard for monitoring migration progress
/// </summary>
[Route("platform-admin/migration-metrics")]
[Authorize(Roles = "PlatformAdmin")]
public class MigrationMetricsController : Controller
{
    private readonly IMetricsService _metrics;
    
    public MigrationMetricsController(IMetricsService metrics)
    {
        _metrics = metrics;
    }
    
    /// <summary>
    /// Show migration metrics dashboard
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(int days = 7)
    {
        var from = DateTime.UtcNow.AddDays(-days);
        var to = DateTime.UtcNow;
        
        var stats = await _metrics.GetStatisticsAsync(from, to);
        
        ViewData["Days"] = days;
        return View("~/Views/PlatformAdmin/MigrationMetrics.cshtml", stats);
    }
    
    /// <summary>
    /// Get metrics as JSON (for AJAX updates)
    /// </summary>
    [HttpGet("api/stats")]
    public async Task<IActionResult> GetStats(int days = 1)
    {
        var from = DateTime.UtcNow.AddDays(-days);
        var to = DateTime.UtcNow;
        
        var stats = await _metrics.GetStatisticsAsync(from, to);
        
        return Json(new
        {
            success = true,
            data = stats,
            timestamp = DateTime.UtcNow
        });
    }
}
