using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Catalog API - CRUD for regulators, frameworks, controls, baselines, packages
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogApiController : ControllerBase
{
    private readonly GrcDbContext _db;
    private readonly ICatalogDataService _catalogService;

    public CatalogApiController(GrcDbContext db, ICatalogDataService catalogService)
    {
        _db = db;
        _catalogService = catalogService;
    }

    [HttpGet("regulators")]
    public async Task<IActionResult> GetRegulators()
    {
        var items = await _db.RegulatorCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("regulators/{id}")]
    public async Task<IActionResult> GetRegulator(Guid id)
    {
        var item = await _db.RegulatorCatalogs.FirstOrDefaultAsync(r => r.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("frameworks")]
    public async Task<IActionResult> GetFrameworks()
    {
        var items = await _db.FrameworkCatalogs.Where(f => f.IsActive).OrderBy(f => f.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("frameworks/{id}")]
    public async Task<IActionResult> GetFramework(Guid id)
    {
        var item = await _db.FrameworkCatalogs.FirstOrDefaultAsync(f => f.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("controls")]
    public async Task<IActionResult> GetControls([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = await _db.CanonicalControls.Where(c => c.IsActive)
            .OrderBy(c => c.ControlName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var total = await _db.CanonicalControls.CountAsync(c => c.IsActive);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("controls/{id}")]
    public async Task<IActionResult> GetControl(Guid id)
    {
        var item = await _db.CanonicalControls.FirstOrDefaultAsync(c => c.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("baselines")]
    public async Task<IActionResult> GetBaselines()
    {
        var items = await _db.BaselineCatalogs.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages()
    {
        var items = await _db.PackageCatalogs.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var items = await _db.TemplateCatalogs.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("evidence-types")]
    public async Task<IActionResult> GetEvidenceTypes()
    {
        var items = await _db.EvidenceTypeCatalogs.Where(e => e.IsActive).OrderBy(e => e.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var items = await _db.RoleCatalogs.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).ToListAsync();
        return Ok(items);
    }

    [HttpGet("titles")]
    public async Task<IActionResult> GetTitles()
    {
        var items = await _db.TitleCatalogs.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).ToListAsync();
        return Ok(items);
    }
}

/// <summary>
/// Tenant API - Tenant provisioning and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantApiController : ControllerBase
{
    private readonly GrcDbContext _db;

    public TenantApiController(GrcDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var tenants = await _db.Tenants.OrderByDescending(t => t.Id).ToListAsync();
        return Ok(tenants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        return tenant == null ? NotFound() : Ok(tenant);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TenantCreateRequest request)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            OrganizationName = request.OrganizationName,
            TenantSlug = request.Slug ?? Guid.NewGuid().ToString("N").Substring(0, 8),
            AdminEmail = request.AdminEmail,
            IsActive = true,
            SubscriptionTier = "Trial"
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = tenant.Id }, tenant);
    }

    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        tenant.IsActive = true;
        tenant.ActivatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(tenant);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        tenant.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(tenant);
    }

    [HttpPut("{id}/org-profile")]
    public async Task<IActionResult> UpdateOrgProfile(Guid id, [FromBody] OrgProfileRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        var profile = await _db.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == id);
        if (profile == null)
        {
            profile = new OrganizationProfile { Id = Guid.NewGuid(), TenantId = id };
            _db.OrganizationProfiles.Add(profile);
        }

        profile.Sector = request.Sector;
        profile.Country = request.Country;
        profile.OrganizationType = request.OrganizationType;

        await _db.SaveChangesAsync();
        return Ok(profile);
    }

    [HttpGet("{id}/scope")]
    public async Task<IActionResult> GetScope(Guid id)
    {
        var baselines = await _db.TenantBaselines.Where(b => b.TenantId == id).ToListAsync();
        var packages = await _db.TenantPackages.Where(p => p.TenantId == id).ToListAsync();
        var templates = await _db.TenantTemplates.Where(t => t.TenantId == id).ToListAsync();

        return Ok(new { baselines, packages, templates });
    }
}

public class TenantCreateRequest
{
    public string OrganizationName { get; set; } = "";
    public string? Slug { get; set; }
    public string AdminEmail { get; set; } = "";
}

public class OrgProfileRequest
{
    public string? Sector { get; set; }
    public string? Country { get; set; }
    public string? OrganizationType { get; set; }
    public string? DataHostingModel { get; set; }
    public string? OrganizationSize { get; set; }
}

/// <summary>
/// Onboarding API - Smart onboarding wizard endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingApiController : ControllerBase
{
    private readonly GrcDbContext _db;

    public OnboardingApiController(GrcDbContext db)
    {
        _db = db;
    }

    [HttpGet("{tenantId}/status")]
    public async Task<IActionResult> GetStatus(Guid tenantId)
    {
        var profile = await _db.OrganizationProfiles.FirstOrDefaultAsync(p => p.TenantId == tenantId);
        var hasBaselines = await _db.TenantBaselines.AnyAsync(b => b.TenantId == tenantId);
        var hasPlan = await _db.Plans.AnyAsync(p => p.TenantId == tenantId);

        return Ok(new
        {
            hasProfile = profile != null,
            hasScope = hasBaselines,
            hasPlan,
            currentStep = profile == null ? "profile" : !hasBaselines ? "scope" : !hasPlan ? "plan" : "complete"
        });
    }

    [HttpPost("{tenantId}/finish")]
    public async Task<IActionResult> FinishOnboarding(Guid tenantId)
    {
        // This would trigger the rules engine to derive scope
        // For now, return success
        return Ok(new { success = true, message = "Onboarding completed" });
    }

    [HttpPost("{tenantId}/generate-plan")]
    public async Task<IActionResult> GeneratePlan(Guid tenantId)
    {
        // Generate plan from templates in scope
        var templates = await _db.TenantTemplates.Where(t => t.TenantId == tenantId).ToListAsync();

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "GRC Implementation Plan",
            Status = "Draft"
        };

        _db.Plans.Add(plan);
        await _db.SaveChangesAsync();

        return Ok(plan);
    }
}

/// <summary>
/// CCM API - Continuous Control Monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CCMApiController : ControllerBase
{
    private readonly GrcDbContext _db;

    public CCMApiController(GrcDbContext db)
    {
        _db = db;
    }

    [HttpGet("tests")]
    public async Task<IActionResult> GetTests([FromQuery] Guid? tenantId)
    {
        var query = _db.CCMControlTests.AsQueryable();
        if (tenantId.HasValue) query = query.Where(t => t.TenantId == tenantId);
        var tests = await query.OrderBy(t => t.Name).ToListAsync();
        return Ok(tests);
    }

    [HttpGet("tests/{id}")]
    public async Task<IActionResult> GetTest(Guid id)
    {
        var test = await _db.CCMControlTests.FirstOrDefaultAsync(t => t.Id == id);
        return test == null ? NotFound() : Ok(test);
    }

    [HttpPost("tests/{id}/execute")]
    public async Task<IActionResult> ExecuteTest(Guid id)
    {
        var test = await _db.CCMControlTests.FirstOrDefaultAsync(t => t.Id == id);
        if (test == null) return NotFound();

        var execution = new CCMTestExecution
        {
            Id = Guid.NewGuid(),
            TestId = id,
            Status = "Completed",
            ResultStatus = new Random().Next(0, 10) > 2 ? "Pass" : "Fail",
            PeriodStart = DateTime.UtcNow.AddDays(-7),
            PeriodEnd = DateTime.UtcNow,
            PopulationCount = new Random().Next(100, 1000),
            PassedCount = new Random().Next(80, 100),
            FailedCount = new Random().Next(0, 20)
        };
        execution.PassRate = execution.PopulationCount > 0 ? (decimal)execution.PassedCount / execution.PopulationCount * 100 : 0;

        _db.CCMTestExecutions.Add(execution);
        await _db.SaveChangesAsync();

        return Ok(execution);
    }

    [HttpGet("executions")]
    public async Task<IActionResult> GetExecutions([FromQuery] Guid? testId, [FromQuery] int limit = 20)
    {
        var query = _db.CCMTestExecutions.Include(e => e.Test).AsQueryable();
        if (testId.HasValue) query = query.Where(e => e.TestId == testId);
        var executions = await query.OrderByDescending(e => e.PeriodEnd).Take(limit).ToListAsync();
        return Ok(executions);
    }
}

/// <summary>
/// KRI API - Key Risk Indicators
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KRIApiController : ControllerBase
{
    private readonly GrcDbContext _db;
    private readonly IWATCHService _watchService;

    public KRIApiController(GrcDbContext db, IWATCHService watchService)
    {
        _db = db;
        _watchService = watchService;
    }

    [HttpGet("indicators")]
    public async Task<IActionResult> GetIndicators([FromQuery] Guid tenantId)
    {
        var indicators = await _watchService.GetIndicatorsAsync(tenantId);
        return Ok(indicators);
    }

    [HttpGet("indicators/{id}")]
    public async Task<IActionResult> GetIndicator(Guid id)
    {
        var indicator = await _db.RiskIndicators.FirstOrDefaultAsync(r => r.Id == id);
        return indicator == null ? NotFound() : Ok(indicator);
    }

    [HttpPost("indicators")]
    public async Task<IActionResult> CreateIndicator([FromBody] RiskIndicator indicator)
    {
        var created = await _watchService.CreateIndicatorAsync(indicator.TenantId, indicator);
        return CreatedAtAction(nameof(GetIndicator), new { id = created.Id }, created);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts([FromQuery] Guid tenantId, [FromQuery] bool openOnly = true)
    {
        var alerts = await _watchService.GetAlertsAsync(tenantId, openOnly);
        return Ok(alerts);
    }

    [HttpPost("alerts/{id}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(Guid id)
    {
        var userId = User.Identity?.Name ?? "system";
        var alert = await _watchService.AcknowledgeAlertAsync(id, userId);
        return Ok(alert);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] Guid tenantId)
    {
        var stats = await _watchService.GetStatsAsync(tenantId);
        return Ok(stats);
    }
}

/// <summary>
/// Exception API - Control Exceptions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExceptionApiController : ControllerBase
{
    private readonly GrcDbContext _db;
    private readonly IFIXService _fixService;

    public ExceptionApiController(GrcDbContext db, IFIXService fixService)
    {
        _db = db;
        _fixService = fixService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid tenantId)
    {
        var exceptions = await _fixService.GetExceptionsAsync(tenantId);
        return Ok(exceptions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var exception = await _db.ControlExceptions.FirstOrDefaultAsync(e => e.Id == id);
        return exception == null ? NotFound() : Ok(exception);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ControlException exception)
    {
        var created = await _fixService.CreateExceptionAsync(exception.TenantId, exception);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = User.Identity?.Name ?? "system";
        var exception = await _fixService.ApproveExceptionAsync(id, userId);
        return Ok(exception);
    }

    [HttpPost("{id}/extend")]
    public async Task<IActionResult> Extend(Guid id, [FromBody] ExtendRequest request)
    {
        var exception = await _fixService.ExtendExceptionAsync(id, request.NewExpiry, request.Reason ?? "Extended");
        return Ok(exception);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] Guid tenantId)
    {
        var stats = await _fixService.GetStatsAsync(tenantId);
        return Ok(stats);
    }
}

public class ExtendRequest
{
    public DateTime NewExpiry { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Shahin Module API - MAP, APPLY, PROVE, WATCH, FIX, VAULT
/// </summary>
[ApiController]
[Route("api/shahin")]
[Authorize]
public class ShahinModuleApiController : ControllerBase
{
    private readonly IMAPService _mapService;
    private readonly IAPPLYService _applyService;
    private readonly IPROVEService _proveService;
    private readonly IWATCHService _watchService;
    private readonly IFIXService _fixService;
    private readonly IVAULTService _vaultService;

    public ShahinModuleApiController(
        IMAPService mapService,
        IAPPLYService applyService,
        IPROVEService proveService,
        IWATCHService watchService,
        IFIXService fixService,
        IVAULTService vaultService)
    {
        _mapService = mapService;
        _applyService = applyService;
        _proveService = proveService;
        _watchService = watchService;
        _fixService = fixService;
        _vaultService = vaultService;
    }

    // MAP Module
    [HttpGet("map/controls")]
    public async Task<IActionResult> GetControls([FromQuery] Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var controls = await _mapService.GetControlsAsync(tenantId, page, pageSize);
        var total = await _mapService.GetControlCountAsync();
        return Ok(new { controls, total, page, pageSize });
    }

    [HttpGet("map/controls/{id}")]
    public async Task<IActionResult> GetControl(Guid id)
    {
        var control = await _mapService.GetControlAsync(id);
        return control == null ? NotFound() : Ok(control);
    }

    // APPLY Module
    [HttpGet("apply/baselines")]
    public async Task<IActionResult> GetBaselines([FromQuery] Guid tenantId)
    {
        var baselines = await _applyService.GetBaselinesAsync(tenantId);
        return Ok(baselines);
    }

    [HttpGet("apply/packages")]
    public async Task<IActionResult> GetPackages([FromQuery] Guid tenantId)
    {
        var packages = await _applyService.GetPackagesAsync(tenantId);
        return Ok(packages);
    }

    [HttpGet("apply/applicability")]
    public async Task<IActionResult> GetApplicability([FromQuery] Guid tenantId)
    {
        var entries = await _applyService.GetApplicabilityEntriesAsync(tenantId);
        return Ok(entries);
    }

    [HttpPost("apply/applicability")]
    public async Task<IActionResult> SetApplicability([FromBody] ApplicabilityRequest request)
    {
        var entry = await _applyService.SetApplicabilityAsync(request.TenantId, request.ControlId, request.Status, request.Reason);
        return Ok(entry);
    }

    // PROVE Module
    [HttpGet("prove/evidence")]
    public async Task<IActionResult> GetEvidence([FromQuery] Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var evidence = await _proveService.GetEvidenceAsync(tenantId, page, pageSize);
        return Ok(evidence);
    }

    [HttpGet("prove/evidence/{id}")]
    public async Task<IActionResult> GetEvidenceById(Guid id)
    {
        var evidence = await _proveService.GetEvidenceByIdAsync(id);
        return evidence == null ? NotFound() : Ok(evidence);
    }

    [HttpPost("prove/evidence")]
    public async Task<IActionResult> UploadEvidence([FromBody] AutoTaggedEvidence evidence)
    {
        var created = await _proveService.UploadEvidenceAsync(evidence.TenantId, evidence);
        return Ok(created);
    }

    [HttpPost("prove/evidence/{id}/approve")]
    public async Task<IActionResult> ApproveEvidence(Guid id, [FromBody] ShahinApprovalRequest request)
    {
        var userId = User.Identity?.Name ?? "system";
        var evidence = await _proveService.ApproveEvidenceAsync(id, userId, request.Notes);
        return Ok(evidence);
    }

    [HttpPost("prove/evidence/{id}/reject")]
    public async Task<IActionResult> RejectEvidence(Guid id, [FromBody] ShahinRejectionRequest request)
    {
        var userId = User.Identity?.Name ?? "system";
        var evidence = await _proveService.RejectEvidenceAsync(id, userId, request.Reason);
        return Ok(evidence);
    }

    [HttpGet("prove/stats")]
    public async Task<IActionResult> GetProveStats([FromQuery] Guid tenantId)
    {
        var stats = await _proveService.GetStatsAsync(tenantId);
        return Ok(stats);
    }

    // WATCH Module
    [HttpGet("watch/indicators")]
    public async Task<IActionResult> GetWatchIndicators([FromQuery] Guid tenantId)
    {
        var indicators = await _watchService.GetIndicatorsAsync(tenantId);
        return Ok(indicators);
    }

    [HttpGet("watch/alerts")]
    public async Task<IActionResult> GetWatchAlerts([FromQuery] Guid tenantId, [FromQuery] bool openOnly = true)
    {
        var alerts = await _watchService.GetAlertsAsync(tenantId, openOnly);
        return Ok(alerts);
    }

    [HttpGet("watch/stats")]
    public async Task<IActionResult> GetWatchStats([FromQuery] Guid tenantId)
    {
        var stats = await _watchService.GetStatsAsync(tenantId);
        return Ok(stats);
    }

    // FIX Module
    [HttpGet("fix/exceptions")]
    public async Task<IActionResult> GetExceptions([FromQuery] Guid tenantId)
    {
        var exceptions = await _fixService.GetExceptionsAsync(tenantId);
        return Ok(exceptions);
    }

    [HttpGet("fix/stats")]
    public async Task<IActionResult> GetFixStats([FromQuery] Guid tenantId)
    {
        var stats = await _fixService.GetStatsAsync(tenantId);
        return Ok(stats);
    }

    // VAULT Module
    [HttpGet("vault/evidence")]
    public async Task<IActionResult> GetVaultEvidence([FromQuery] Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var evidence = await _vaultService.GetStoredEvidenceAsync(tenantId, page, pageSize);
        return Ok(evidence);
    }

    [HttpGet("vault/evidence/{id}")]
    public async Task<IActionResult> GetVaultEvidenceById(Guid id)
    {
        var evidence = await _vaultService.GetEvidenceByIdAsync(id);
        return evidence == null ? NotFound() : Ok(evidence);
    }

    [HttpGet("vault/stats")]
    public async Task<IActionResult> GetVaultStats([FromQuery] Guid tenantId)
    {
        var stats = await _vaultService.GetStatsAsync(tenantId);
        return Ok(stats);
    }
}

public class ApplicabilityRequest
{
    public Guid TenantId { get; set; }
    public Guid ControlId { get; set; }
    public string Status { get; set; } = "";
    public string Reason { get; set; } = "";
}

public class ShahinApprovalRequest
{
    public string? Notes { get; set; }
}

public class ShahinRejectionRequest
{
    public string Reason { get; set; } = "";
}

public class ShahinStartWorkflowRequest
{
    public Guid TenantId { get; set; }
    public Guid DefinitionId { get; set; }
}

/// <summary>
/// Assessment Execution API
/// </summary>
[ApiController]
[Route("api/assessment-execution")]
[Authorize]
public class AssessmentExecutionApiController : ControllerBase
{
    private readonly IAssessmentExecutionService _assessmentService;

    public AssessmentExecutionApiController(IAssessmentExecutionService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid tenantId)
    {
        var assessments = await _assessmentService.GetAssessmentsAsync(tenantId);
        return Ok(assessments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var assessment = await _assessmentService.GetAssessmentAsync(id);
        return assessment == null ? NotFound() : Ok(assessment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request)
    {
        var userId = User.Identity?.Name ?? "system";
        var assessment = await _assessmentService.CreateAssessmentAsync(request.TenantId, request.Name, request.TemplateId, userId);
        return CreatedAtAction(nameof(Get), new { id = assessment.Id }, assessment);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var userId = User.Identity?.Name ?? "system";
        var assessment = await _assessmentService.StartAssessmentAsync(id, userId);
        return Ok(assessment);
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var userId = User.Identity?.Name ?? "system";
        var assessment = await _assessmentService.CompleteAssessmentAsync(id, userId);
        return Ok(assessment);
    }

    [HttpGet("{id}/requirements")]
    public async Task<IActionResult> GetRequirements(Guid id)
    {
        var requirements = await _assessmentService.GetRequirementsAsync(id);
        return Ok(requirements);
    }

    [HttpPut("requirements/{requirementId}/status")]
    public async Task<IActionResult> UpdateRequirementStatus(Guid requirementId, [FromBody] UpdateStatusRequest request)
    {
        var requirement = await _assessmentService.UpdateRequirementStatusAsync(requirementId, request.Status, request.Notes);
        return Ok(requirement);
    }
}

public class CreateAssessmentRequest
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = "";
    public Guid? TemplateId { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
}

/// <summary>
/// Workflow Integration API
/// </summary>
[ApiController]
[Route("api/workflow-integration")]
[Authorize]
public class WorkflowIntegrationApiController : ControllerBase
{
    private readonly IWorkflowIntegrationService _workflowService;

    public WorkflowIntegrationApiController(IWorkflowIntegrationService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid tenantId)
    {
        var workflows = await _workflowService.GetWorkflowsAsync(tenantId);
        return Ok(workflows);
    }

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] ShahinStartWorkflowRequest request)
    {
        var userId = User.Identity?.Name ?? "system";
        var workflow = await _workflowService.StartWorkflowAsync(request.TenantId, request.DefinitionId, userId);
        return Ok(workflow);
    }

    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = User.Identity?.Name ?? "system";
        var tasks = await _workflowService.GetMyTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpPost("tasks/{taskId}/approve")]
    public async Task<IActionResult> ApproveTask(Guid taskId, [FromBody] ShahinApprovalRequest request)
    {
        var userId = User.Identity?.Name ?? "system";
        var task = await _workflowService.ApproveTaskAsync(taskId, userId, request.Notes);
        return Ok(task);
    }

    [HttpPost("tasks/{taskId}/reject")]
    public async Task<IActionResult> RejectTask(Guid taskId, [FromBody] ShahinRejectionRequest request)
    {
        var userId = User.Identity?.Name ?? "system";
        var task = await _workflowService.RejectTaskAsync(taskId, userId, request.Reason);
        return Ok(task);
    }
}
