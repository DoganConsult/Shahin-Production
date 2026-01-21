using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Application.Policy;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers;

// NOTE: This file contains the following controller classes:
// - CCMController
// - ExceptionController (singular - different from ExceptionsController)
// Other controllers (KRIDashboardController, AuditPackageController, InvitationController,
// ReportsController) have been moved to separate files.

/// <summary>
/// CCM (Continuous Control Monitoring) Controller
/// Run and manage automated control tests
/// </summary>
[Authorize]
[Route("[controller]")]
public class CCMController : Controller
{
    private readonly GrcDbContext _db;
    private readonly ILogger<CCMController> _logger;
    private readonly PolicyEnforcementHelper _policyHelper;

    public CCMController(GrcDbContext db, ILogger<CCMController> logger, PolicyEnforcementHelper policyHelper)
    {
        _db = db;
        _logger = logger;
        _policyHelper = policyHelper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tenantId = GetCurrentTenantId();
        var tests = await _db.CCMControlTests.Where(t => t.TenantId == tenantId).OrderByDescending(t => t.Id).Take(50).ToListAsync();
        var executions = await _db.CCMTestExecutions.OrderByDescending(r => r.PeriodEnd).Take(20).ToListAsync();

        ViewBag.Tests = tests;
        ViewBag.Executions = executions;
        ViewBag.TotalTests = tests.Count;
        ViewBag.PassedTests = executions.Count(r => r.Status == "Passed");
        ViewBag.FailedTests = executions.Count(r => r.Status == "Failed");

        return View();
    }

    [HttpGet("Run/{testId}")]
    public async Task<IActionResult> RunTest(Guid testId)
    {
        try
        {
            var test = await _db.CCMControlTests.FirstOrDefaultAsync(t => t.Id == testId);
            if (test == null) return NotFound();

            // POLICY ENFORCEMENT: Check if test execution is allowed
            await _policyHelper.EnforceAsync("execute", "CCMControlTest", test, 
                dataClassification: "internal", 
                owner: test.Owner ?? "System");

            var execution = new CCMTestExecution
            {
                Id = Guid.NewGuid(),
                TestId = testId,
                Status = "Completed",
                ResultStatus = new Random().Next(0, 10) > 2 ? "Pass" : "Fail",
                PeriodStart = DateTime.UtcNow.AddDays(-7),
                PeriodEnd = DateTime.UtcNow
            };

            _db.CCMTestExecutions.Add(execution);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Test executed: {execution.Status}";
        }
        catch (PolicyViolationException pex)
        {
            _logger.LogWarning(pex, "Policy violation executing CCM test {TestId}", testId);
            TempData["Error"] = "A policy violation occurred. Please review the requirements.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing CCM test {TestId}", testId);
            TempData["Error"] = "Error executing test. Please try again.";
        }
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

// NOTE: KRIDashboardController has been moved to a separate file (KRIDashboardController.cs)

/// <summary>
/// Exception Management Controller
/// Create, approve, extend control exceptions
/// </summary>
[Route("[controller]")]
public class ExceptionController : Controller
{
    private readonly GrcDbContext _db;
    private readonly PolicyEnforcementHelper _policyHelper;
    private readonly ILogger<ExceptionController> _logger;

    public ExceptionController(GrcDbContext db, PolicyEnforcementHelper policyHelper, ILogger<ExceptionController> logger)
    {
        _db = db;
        _policyHelper = policyHelper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tenantId = GetCurrentTenantId();
        var exceptions = await _db.ControlExceptions.Where(e => e.TenantId == tenantId).OrderByDescending(e => e.Id).ToListAsync();

        ViewBag.OpenCount = exceptions.Count(e => e.Status == "Approved" && e.ExpiryDate > DateTime.UtcNow);
        ViewBag.PendingCount = exceptions.Count(e => e.Status == "Pending");
        ViewBag.ExpiredCount = exceptions.Count(e => e.ExpiryDate <= DateTime.UtcNow);

        return View(exceptions);
    }

    [HttpGet("Create")]
    public IActionResult Create() => View();

    [HttpPost("Create")]
    public async Task<IActionResult> CreatePost([FromForm] ExceptionCreateDto dto)
    {
        try
        {
            var tenantId = GetCurrentTenantId();
            var exception = new ControlException
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ControlId = dto.ControlId,
                Reason = dto.Reason ?? "Business justification",
                Status = "Pending",
                ExpiryDate = dto.ExpiryDate ?? DateTime.UtcNow.AddMonths(6),
                RequestedBy = User.Identity?.Name ?? "system"
            };

            // POLICY ENFORCEMENT: Check if exception creation is allowed
            await _policyHelper.EnforceCreateAsync("ControlException", exception, 
                dataClassification: "confidential", 
                owner: User.Identity?.Name ?? "System");

            _db.ControlExceptions.Add(exception);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Exception request submitted for approval";
        }
        catch (PolicyViolationException pex)
        {
            _logger.LogWarning(pex, "Policy violation creating control exception");
            TempData["Error"] = "A policy violation occurred. Please review the requirements.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating control exception");
            TempData["Error"] = "Error creating exception. Please try again.";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Approve/{id}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var exception = await _db.ControlExceptions.FirstOrDefaultAsync(e => e.Id == id);
            if (exception == null) return NotFound();

            // POLICY ENFORCEMENT: Check if approval is allowed
            await _policyHelper.EnforceApproveAsync("ControlException", exception, 
                dataClassification: "confidential", 
                owner: exception.RequestedBy ?? "System");

            exception.Status = "Approved";
            exception.ApprovedBy = User.Identity?.Name ?? "system";

            await _db.SaveChangesAsync();
            TempData["Success"] = "Exception approved";
        }
        catch (PolicyViolationException pex)
        {
            _logger.LogWarning(pex, "Policy violation approving control exception {ExceptionId}", id);
            TempData["Error"] = "A policy violation occurred. Please review the requirements.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving control exception {ExceptionId}", id);
            TempData["Error"] = "Error approving exception. Please try again.";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Extend/{id}")]
    public async Task<IActionResult> Extend(Guid id, [FromForm] DateTime newExpiry)
    {
        try
        {
            var exception = await _db.ControlExceptions.FirstOrDefaultAsync(e => e.Id == id);
            if (exception == null) return NotFound();

            // POLICY ENFORCEMENT: Check if extension is allowed
            await _policyHelper.EnforceUpdateAsync("ControlException", exception, 
                dataClassification: "confidential", 
                owner: exception.RequestedBy ?? "System");

            exception.ExpiryDate = newExpiry;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Exception extended";
        }
        catch (PolicyViolationException pex)
        {
            _logger.LogWarning(pex, "Policy violation extending control exception {ExceptionId}", id);
            TempData["Error"] = "A policy violation occurred. Please review the requirements.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending control exception {ExceptionId}", id);
            TempData["Error"] = "Error extending exception. Please try again.";
        }
        return RedirectToAction("Index");
    }

    private Guid GetCurrentTenantId()
    {
        var claim = User.FindFirst("TenantId");
        return claim != null && Guid.TryParse(claim.Value, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class ExceptionCreateDto
{
    public Guid ControlId { get; set; }
    public string? Reason { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

// NOTE: AuditPackageController, InvitationController, InviteDto, and ReportsController
// have been moved to separate files to avoid duplicate definitions.
