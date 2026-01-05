using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Data;
using GrcMvc.Data.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IUnitOfWork unitOfWork, ILogger<DashboardController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard overview for authenticated user's tenant
        /// </summary>
        [HttpGet("overview/{tenantId}")]
        public async Task<IActionResult> GetDashboardOverview(Guid tenantId)
        {
            try
            {
                var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
                if (tenant == null)
                    return NotFound(new { message = "Tenant not found" });

                var orgProfile = await _unitOfWork.OrganizationProfiles
                    .Query()
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId);

                var plans = await _unitOfWork.Plans
                    .Query()
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                var activeBaselines = await _unitOfWork.TenantBaselines
                    .Query()
                    .Where(b => b.TenantId == tenantId && !b.IsDeleted)
                    .CountAsync();

                var activePackages = await _unitOfWork.TenantPackages
                    .Query()
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .CountAsync();

                var auditEvents = await _unitOfWork.AuditEvents
                    .Query()
                    .Where(a => a.TenantId == tenantId)
                    .OrderByDescending(a => a.EventTimestamp)
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    tenant = new
                    {
                        tenant.Id,
                        tenant.TenantSlug,
                        tenant.OrganizationName,
                        tenant.Status,
                        tenant.ActivatedAt
                    },
                    organization = orgProfile == null ? null : new
                    {
                        orgProfile.Id,
                        orgProfile.OrganizationType,
                        orgProfile.Sector,
                        orgProfile.Country,
                        orgProfile.HostingModel,
                        orgProfile.OrganizationSize,
                        orgProfile.ComplianceMaturity
                    },
                    stats = new
                    {
                        totalPlans = plans.Count,
                        activePlans = plans.Count(p => p.Status == "Active"),
                        completedPlans = plans.Count(p => p.Status == "Completed"),
                        activeBaselines,
                        activePackages,
                        recentAuditEvents = auditEvents.Count
                    },
                    recentPlans = plans.Select(p => new
                    {
                        p.Id,
                        p.PlanCode,
                        p.Name,
                        p.Status,
                        p.PlanType,
                        p.StartDate,
                        p.TargetEndDate,
                        p.ActualStartDate,
                        p.ActualEndDate
                    }),
                    recentActivity = auditEvents.Select(a => new
                    {
                        a.EventId,
                        a.EventType,
                        a.Action,
                        a.Actor,
                        a.EventTimestamp,
                        a.AffectedEntityType
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, new { message = "Error retrieving dashboard data" });
            }
        }

        /// <summary>
        /// Get detailed plan view
        /// </summary>
        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetPlanDetail(Guid planId)
        {
            try
            {
                var plan = await _unitOfWork.Plans.GetByIdAsync(planId);
                if (plan == null)
                    return NotFound();

                var phases = await _unitOfWork.PlanPhases
                    .Query()
                    .Where(p => p.PlanId == planId && !p.IsDeleted)
                    .OrderBy(p => p.Sequence)
                    .ToListAsync();

                return Ok(new
                {
                    plan = new
                    {
                        plan.Id,
                        plan.TenantId,
                        plan.PlanCode,
                        plan.Name,
                        plan.Description,
                        plan.Status,
                        plan.PlanType,
                        plan.StartDate,
                        plan.TargetEndDate,
                        plan.ActualStartDate,
                        plan.ActualEndDate,
                        plan.RulesetVersion,
                        plan.CreatedDate
                    },
                    phases = phases.Select(p => new
                    {
                        p.Id,
                        p.PhaseCode,
                        p.Name,
                        p.Sequence,
                        p.Status,
                        p.PlannedStartDate,
                        p.PlannedEndDate,
                        p.ActualStartDate,
                        p.ActualEndDate,
                        p.ProgressPercentage
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan detail");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get compliance status summary
        /// </summary>
        [HttpGet("compliance-status/{tenantId}")]
        public async Task<IActionResult> GetComplianceStatus(Guid tenantId)
        {
            try
            {
                var baselines = await _unitOfWork.TenantBaselines
                    .Query()
                    .Where(b => b.TenantId == tenantId && !b.IsDeleted)
                    .ToListAsync();

                var packages = await _unitOfWork.TenantPackages
                    .Query()
                    .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                    .ToListAsync();

                return Ok(new
                {
                    tenantId,
                    scopeApplicable = new
                    {
                        baselinesCount = baselines.Count,
                        packagesCount = packages.Count,
                        estimatedControlCount = (baselines.Count * 48) + (packages.Count * 16)
                    },
                    lastEvaluatedAt = baselines.FirstOrDefault()?.CreatedDate ?? DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance status");
                return StatusCode(500);
            }
        }
    }
}
