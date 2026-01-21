using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers;

/// <summary>
/// KRI/KPI Dashboard Controller
/// Real-time risk indicator monitoring
/// </summary>
[Authorize]
[RequireTenant]
[Route("[controller]")]
public class KRIDashboardController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<KRIDashboardController> _logger;

    public KRIDashboardController(GrcDbContext db, ILogger<KRIDashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var indicators = await _db.RiskIndicators.Where(r => r.TenantId == tenantId && r.IsActive).ToListAsync();
            var alerts = await _db.RiskIndicatorAlerts.Include(a => a.Indicator)
                .Where(a => a.Indicator.TenantId == tenantId && a.Status == "Open")
                .OrderByDescending(a => a.TriggeredAt).Take(20).ToListAsync();

            ViewBag.Indicators = indicators;
            ViewBag.Alerts = alerts;
            ViewBag.CriticalCount = alerts.Count(a => a.Severity == "Critical");
            ViewBag.HighCount = alerts.Count(a => a.Severity == "High");
            ViewBag.MediumCount = alerts.Count(a => a.Severity == "Medium");

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KRI dashboard");
            TempData["Error"] = "Error loading dashboard";
            return View();
        }
    }

    [HttpPost("Acknowledge/{alertId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcknowledgeAlert(Guid alertId)
    {
        try
        {
            var alert = await _db.RiskIndicatorAlerts.FirstOrDefaultAsync(a => a.Id == alertId);
            if (alert == null) return NotFound();

            alert.Status = "Acknowledged";
            alert.AcknowledgedBy = User.Identity?.Name ?? "system";
            alert.AcknowledgedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Alert acknowledged";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", alertId);
            TempData["Error"] = "Error acknowledging alert";
        }
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}
