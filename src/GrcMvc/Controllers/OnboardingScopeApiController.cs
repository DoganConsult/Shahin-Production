using System;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Controllers
{
    /// <summary>
    /// Onboarding Scope API Controller
    /// Provides API endpoints for scope management and re-evaluation
    /// </summary>
    [Route("api/onboarding")]
    [ApiController]
    [Authorize]
    [IgnoreAntiforgeryToken] // API endpoints don't require CSRF tokens
    public class OnboardingScopeApiController : ControllerBase
    {
        private readonly GrcDbContext _context;
        private readonly IRulesEngineService _rulesEngine;
        private readonly IOnboardingProvisioningService _provisioningService;
        private readonly ILogger<OnboardingScopeApiController> _logger;

        public OnboardingScopeApiController(
            GrcDbContext context,
            IRulesEngineService rulesEngine,
            IOnboardingProvisioningService provisioningService,
            ILogger<OnboardingScopeApiController> logger)
        {
            _context = context;
            _rulesEngine = rulesEngine;
            _provisioningService = provisioningService;
            _logger = logger;
        }

        /// <summary>
        /// Refresh/re-evaluate scope for a tenant based on current profile and assets
        /// POST /api/onboarding/tenants/{tenantId}/refresh-scope
        ///
        /// Use case: When assets are added/updated, or profile changes, re-derive applicable
        /// baselines, packages, and templates.
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/refresh-scope")]
        public async Task<IActionResult> RefreshScope(Guid tenantId)
        {
            try
            {
                // Validate tenant exists
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted);

                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                // Get current user ID
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

                // Re-derive scope (includes asset-based recognition)
                var executionLog = await _rulesEngine.DeriveAndPersistScopeAsync(tenantId, userId);

                // Get derived scope summary
                var baselines = await _context.TenantBaselines
                    .Where(b => b.TenantId == tenantId)
                    .Select(b => new { b.BaselineCode, b.BaselineName, b.Applicability })
                    .ToListAsync();

                var packages = await _context.TenantPackages
                    .Where(p => p.TenantId == tenantId)
                    .Select(p => new { p.PackageCode, p.PackageName, p.Applicability })
                    .ToListAsync();

                _logger.LogInformation("Scope refreshed for tenant {TenantId}: {Baselines} baselines, {Packages} packages",
                    tenantId, baselines.Count, packages.Count);

                return Ok(new
                {
                    success = true,
                    executionLogId = executionLog.Id,
                    refreshedAt = DateTime.UtcNow,
                    summary = new
                    {
                        baselinesCount = baselines.Count,
                        packagesCount = packages.Count,
                        baselines,
                        packages
                    },
                    message = $"Scope refreshed: {baselines.Count} baselines, {packages.Count} packages derived"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Scope refresh precondition failed for tenant {TenantId}", tenantId);
                return BadRequest(new { error = "An error occurred processing your request." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing scope for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Get current scope status for a tenant
        /// GET /api/onboarding/tenants/{tenantId}/scope-status
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/scope-status")]
        public async Task<IActionResult> GetScopeStatus(Guid tenantId)
        {
            try
            {
                var profile = await _context.OrganizationProfiles
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId);

                if (profile == null)
                    return NotFound(new { error = "Organization profile not found" });

                var baselines = await _context.TenantBaselines
                    .Where(b => b.TenantId == tenantId)
                    .Select(b => new { b.BaselineCode, b.BaselineName, b.Applicability })
                    .ToListAsync();

                var packages = await _context.TenantPackages
                    .Where(p => p.TenantId == tenantId)
                    .Select(p => new { p.PackageCode, p.PackageName, p.Applicability })
                    .ToListAsync();

                var assetCount = await _context.Assets
                    .CountAsync(a => a.TenantId == tenantId && a.Status == "Active" && !a.IsDeleted);

                var lastExecution = await _context.RuleExecutionLogs
                    .Where(l => l.TenantId == tenantId && l.Status == "Completed")
                    .OrderByDescending(l => l.ExecutedAt)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    tenantId,
                    organizationName = profile.LegalEntityName,
                    lastScopeDerivedAt = profile.LastScopeDerivedAt,
                    lastExecutionId = lastExecution?.Id,
                    lastExecutedAt = lastExecution?.ExecutedAt,
                    baselinesCount = baselines.Count,
                    packagesCount = packages.Count,
                    assetCount,
                    baselines,
                    packages,
                    profileSummary = new
                    {
                        profile.Country,
                        profile.Sector,
                        profile.OrganizationType,
                        profile.DataTypes,
                        profile.IsRegulatedEntity,
                        profile.IsCriticalInfrastructure,
                        profile.PrimaryRegulator
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scope status for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Provision default teams and RACI for a tenant
        /// POST /api/onboarding/tenants/{tenantId}/provision
        /// </summary>
        [HttpPost("tenants/{tenantId:guid}/provision")]
        public async Task<IActionResult> ProvisionTenant(Guid tenantId)
        {
            try
            {
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted);

                if (tenant == null)
                    return NotFound(new { error = "Tenant not found" });

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "system";

                var result = await _provisioningService.ProvisionAllAsync(tenantId, userId);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        errors = result.Errors,
                        warnings = result.Warnings
                    });
                }

                return Ok(new
                {
                    success = true,
                    teamsCreated = result.TeamsCreated,
                    raciAssignmentsCreated = result.RACIAssignmentsCreated,
                    usersAssigned = result.UsersAssigned,
                    warnings = result.Warnings,
                    message = $"Provisioned {result.TeamsCreated} teams, {result.RACIAssignmentsCreated} RACI assignments"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
            }
        }

        /// <summary>
        /// Check provisioning status for a tenant
        /// GET /api/onboarding/tenants/{tenantId}/provisioning-status
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/provisioning-status")]
        public async Task<IActionResult> GetProvisioningStatus(Guid tenantId)
        {
            try
            {
                var teamsCount = await _context.Teams
                    .CountAsync(t => t.TenantId == tenantId && !t.IsDeleted);

                var raciCount = await _context.RACIAssignments
                    .CountAsync(r => r.TenantId == tenantId && !r.IsDeleted);

                var teamMembersCount = await _context.TeamMembers
                    .CountAsync(tm => tm.TenantId == tenantId && !tm.IsDeleted);

                var hasFallbackTeam = await _context.Teams
                    .AnyAsync(t => t.TenantId == tenantId && t.IsDefaultFallback && !t.IsDeleted);

                var needsProvisioning = await _provisioningService.IsProvisioningNeededAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    teamsCount,
                    raciAssignmentsCount = raciCount,
                    teamMembersCount,
                    hasFallbackTeam,
                    isProvisioned = !needsProvisioning,
                    needsProvisioning,
                    recommendation = needsProvisioning
                        ? "Call POST /api/onboarding/tenants/{tenantId}/provision to create default teams and RACI"
                        : "Provisioning complete - workflow routing is ready"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provisioning status for tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "An internal error occurred. Please try again later." });
            }
        }
    }
}
