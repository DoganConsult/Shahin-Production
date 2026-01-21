using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Interface for creating and managing assessment plans.
    /// </summary>
    public interface IPlanService
    {
        /// <summary>
        /// Create a new assessment plan from derived scope.
        /// </summary>
        Task<Plan> CreatePlanAsync(CreatePlanDto request, string createdBy);

        /// <summary>
        /// Get plan by ID.
        /// </summary>
        Task<Plan?> GetPlanAsync(Guid planId);

        /// <summary>
        /// Get all plans for a tenant.
        /// </summary>
        Task<IEnumerable<Plan>> GetTenantPlansAsync(Guid tenantId, int pageNumber = 1, int pageSize = 100);

        /// <summary>
        /// Update plan status.
        /// </summary>
        Task<Plan> UpdatePlanStatusAsync(Guid planId, string status, string modifiedBy);

        /// <summary>
        /// Get plan phases.
        /// </summary>
        Task<IEnumerable<PlanPhase>> GetPlanPhasesAsync(Guid planId);

        /// <summary>
        /// Update phase progress and status.
        /// </summary>
        Task<PlanPhase> UpdatePhaseAsync(Guid phaseId, string status, int progressPercentage, string modifiedBy);
    }
}
