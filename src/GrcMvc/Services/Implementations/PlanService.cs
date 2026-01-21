using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using GrcMvc.Application.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for creating and managing assessment plans.
    /// Plans are created from derived scope (baselines/packages/templates).
    /// </summary>
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditEventService _auditService;
        private readonly ILogger<PlanService> _logger;
        private readonly PolicyEnforcementHelper _policyHelper;
        private readonly IWorkspaceContextService? _workspaceContext;

        public PlanService(
            IUnitOfWork unitOfWork,
            IAuditEventService auditService,
            ILogger<PlanService> logger,
            PolicyEnforcementHelper policyHelper,
            IWorkspaceContextService? workspaceContext = null)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
            _policyHelper = policyHelper;
            _workspaceContext = workspaceContext;
        }

        /// <summary>
        /// Create a new assessment plan from derived scope.
        /// </summary>
        public async Task<Plan> CreatePlanAsync(CreatePlanDto request, string createdBy)
        {
            try
            {
                // Get the tenant's derived scope
                var baselines = await _unitOfWork.TenantBaselines
                    .Query()
                    .Where(b => b.TenantId == request.TenantId && !b.IsDeleted)
                    .ToListAsync();

                var packages = await _unitOfWork.TenantPackages
                    .Query()
                    .Where(p => p.TenantId == request.TenantId && !p.IsDeleted)
                    .ToListAsync();

                var templates = await _unitOfWork.TenantTemplates
                    .Query()
                    .Where(t => t.TenantId == request.TenantId && !t.IsDeleted)
                    .ToListAsync();

                // Create the plan
                var plan = new Plan
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    PlanCode = request.PlanCode,
                    Name = request.Name,
                    Status = "Draft",
                    PlanType = request.PlanType,
                    StartDate = request.StartDate,
                    TargetEndDate = request.TargetEndDate,
                    CorrelationId = Guid.NewGuid().ToString(),
                    ScopeSnapshotJson = JsonSerializer.Serialize(new
                    {
                        Baselines = baselines.Select(b => new { b.BaselineCode, b.BaselineName }),
                        Packages = packages.Select(p => new { p.PackageCode, p.PackageName }),
                        Templates = templates.Select(t => new { t.TemplateCode, t.TemplateName })
                    }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    WorkspaceId = _workspaceContext != null && _workspaceContext.HasWorkspaceContext()
                        ? _workspaceContext.GetCurrentWorkspaceId()
                        : null
                };

                // Enforce policy before creating plan
                await _policyHelper.EnforceCreateAsync(
                    resourceType: "Plan",
                    resource: plan,
                    dataClassification: "internal",
                    owner: createdBy);

                await _unitOfWork.Plans.AddAsync(plan);
                await _unitOfWork.SaveChangesAsync();

                // Create plan phases based on plan type
                await CreatePlanPhasesAsync(plan, createdBy);

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: request.TenantId,
                    eventType: "PlanCreated",
                    affectedEntityType: "Plan",
                    affectedEntityId: plan.Id.ToString(),
                    action: "Create",
                    actor: createdBy,
                    payloadJson: JsonSerializer.Serialize(new { plan.PlanCode, plan.PlanType, Baselines = baselines.Count, Packages = packages.Count, Templates = templates.Count }),
                    correlationId: plan.CorrelationId
                );

                _logger.LogInformation($"Plan created: {plan.Id}");
                return plan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plan");
                throw;
            }
        }

        /// <summary>
        /// Get plan by ID with phases and assessments.
        /// </summary>
        public async Task<Plan?> GetPlanAsync(Guid planId)
        {
            return await _unitOfWork.Plans.GetByIdAsync(planId);
        }

        /// <summary>
        /// Get all plans for a tenant.
        /// </summary>
        public async Task<IEnumerable<Plan>> GetTenantPlansAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100)
        {
            var skip = (pageNumber - 1) * pageSize;
            return await _unitOfWork.Plans
                .Query()
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Update plan status.
        /// </summary>
        public async Task<Plan> UpdatePlanStatusAsync(Guid planId, string status, string modifiedBy)
        {
            try
            {
                var plan = await _unitOfWork.Plans.GetByIdAsync(planId);
                if (plan == null)
                {
                    throw new EntityNotFoundException("Plan", planId);
                }

                plan.Status = status;
                plan.ModifiedDate = DateTime.UtcNow;
                plan.ModifiedBy = modifiedBy;

                // Auto-set ActualStartDate when plan transitions to Active
                if (status == "Active" && plan.ActualStartDate == null)
                {
                    plan.ActualStartDate = DateTime.UtcNow;
                }

                // Auto-set ActualEndDate when plan transitions to Completed
                if (status == "Completed" && plan.ActualEndDate == null)
                {
                    plan.ActualEndDate = DateTime.UtcNow;
                }

                await _unitOfWork.Plans.UpdateAsync(plan);
                await _unitOfWork.SaveChangesAsync();

                // Log event
                await _auditService.LogEventAsync(
                    tenantId: plan.TenantId,
                    eventType: "PlanStatusUpdated",
                    affectedEntityType: "Plan",
                    affectedEntityId: plan.Id.ToString(),
                    action: "Update",
                    actor: modifiedBy,
                    payloadJson: JsonSerializer.Serialize(new { plan.Status, plan.ActualStartDate, plan.ActualEndDate }),
                    correlationId: plan.CorrelationId
                );

                return plan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plan status");
                throw;
            }
        }

        /// <summary>
        /// Get plan phases for a plan.
        /// </summary>
        public async Task<IEnumerable<PlanPhase>> GetPlanPhasesAsync(Guid planId)
        {
            return await _unitOfWork.PlanPhases
                .Query()
                .Where(p => p.PlanId == planId && !p.IsDeleted)
                .OrderBy(p => p.Sequence)
                .ToListAsync();
        }

        /// <summary>
        /// Update phase progress and status.
        /// </summary>
        public async Task<PlanPhase> UpdatePhaseAsync(Guid phaseId, string status, int progressPercentage, string modifiedBy)
        {
            try
            {
                var phase = await _unitOfWork.PlanPhases.GetByIdAsync(phaseId);
                if (phase == null)
                {
                    throw new EntityNotFoundException("Phase", phaseId);
                }

                phase.Status = status;
                phase.ProgressPercentage = progressPercentage;
                phase.ModifiedDate = DateTime.UtcNow;
                phase.ModifiedBy = modifiedBy;

                if (status == "InProgress" && phase.ActualStartDate == null)
                {
                    phase.ActualStartDate = DateTime.UtcNow;
                }
                else if (status == "Completed" && phase.ActualEndDate == null)
                {
                    phase.ActualEndDate = DateTime.UtcNow;
                }

                await _unitOfWork.PlanPhases.UpdateAsync(phase);
                await _unitOfWork.SaveChangesAsync();

                return phase;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating phase");
                throw;
            }
        }

        /// <summary>
        /// Create plan phases based on plan type.
        /// </summary>
        private async Task CreatePlanPhasesAsync(Plan plan, string createdBy)
        {
            var phases = new List<PlanPhase>();

            switch (plan.PlanType.ToUpper())
            {
                case "QUICKSCAN":
                    phases.Add(CreatePhase(plan, 1, "PHASE_QUICK_SCAN", "Quick Scan", plan.StartDate, plan.TargetEndDate, createdBy));
                    break;

                case "FULL":
                    phases.Add(CreatePhase(plan, 1, "PHASE_DETAILED_ASSESSMENT", "Detailed Assessment", plan.StartDate, plan.TargetEndDate.AddDays(-7), createdBy));
                    phases.Add(CreatePhase(plan, 2, "PHASE_REMEDIATION", "Remediation Planning", plan.TargetEndDate.AddDays(-7), plan.TargetEndDate, createdBy));
                    break;

                case "REMEDIATION":
                    phases.Add(CreatePhase(plan, 1, "PHASE_REMEDIATION_EXECUTION", "Remediation Execution", plan.StartDate, plan.TargetEndDate.AddDays(-3), createdBy));
                    phases.Add(CreatePhase(plan, 2, "PHASE_REMEDIATION_VALIDATION", "Remediation Validation", plan.TargetEndDate.AddDays(-3), plan.TargetEndDate, createdBy));
                    break;

                default:
                    phases.Add(CreatePhase(plan, 1, "PHASE_ASSESSMENT", "Assessment", plan.StartDate, plan.TargetEndDate, createdBy));
                    break;
            }

            foreach (var phase in phases)
            {
                await _unitOfWork.PlanPhases.AddAsync(phase);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private PlanPhase CreatePhase(Plan plan, int sequence, string phaseCode, string phaseName, DateTime startDate, DateTime endDate, string createdBy)
        {
            return new PlanPhase
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                PhaseCode = phaseCode,
                Name = phaseName,
                Sequence = sequence,
                Status = "NotStarted",
                PlannedStartDate = startDate,
                PlannedEndDate = endDate,
                ProgressPercentage = 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
    }
}
