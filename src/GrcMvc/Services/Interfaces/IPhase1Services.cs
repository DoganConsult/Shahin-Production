using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// PHASE 1: Framework Data Service
    /// Manages regulatory frameworks, controls, and baselines
    /// NOTE: Framework, Baseline, ControlOwnership entities should be created in Models/Entities
    /// For now, this interface is a placeholder for Phase 1 framework management
    /// </summary>
    public interface IFrameworkService
    {
        // Placeholder methods - implement when entities are available
        // Framework operations
        // Task<Framework> GetFrameworkAsync(Guid frameworkId);
        // Task<List<Framework>> GetAllFrameworksAsync(Guid tenantId);
        // Task<Framework> CreateFrameworkAsync(Guid tenantId, string name, string code, string description);
        // Task UpdateFrameworkAsync(Framework framework);
        // Task DeleteFrameworkAsync(Guid frameworkId);

        // Control operations
        Task<Models.Entities.Control> GetControlAsync(Guid controlId);
        Task<List<Models.Entities.Control>> GetControlsByFrameworkAsync(Guid frameworkId);
        Task<Models.Entities.Control> CreateControlAsync(Guid tenantId, Guid frameworkId, string controlCode, string controlName, string description);
        Task UpdateControlAsync(Models.Entities.Control control);
        Task DeleteControlAsync(Guid controlId);
        Task<List<Models.Entities.Control>> SearchControlsAsync(Guid tenantId, string searchTerm);
    }

    /// <summary>
    /// PHASE 1: HRIS Integration Service
    /// NOTE: HRISIntegration and HRISEmployee entities should be created in Models/Entities
    /// For now, this service is a placeholder
    /// </summary>
    public interface IHRISService
    {
        // Placeholder - implement when entities are available
        Task<ApplicationUser> CreateUserFromApplicationAsync(Guid userId, Guid tenantId);
    }

    /// <summary>
    /// PHASE 1: Audit Trail Service
    /// Tracks all system changes for compliance auditing
    /// </summary>
    public interface IAuditTrailService
    {
        // Logging changes
        Task LogChangeAsync(Guid tenantId, string entityType, Guid entityId, string action,
                           string? fieldName = null, string? oldValue = null, string? newValue = null,
                           Guid? userId = null, string? ipAddress = null);

        Task LogCreatedAsync(Guid tenantId, string entityType, Guid entityId, Guid? userId = null);
        Task LogUpdatedAsync(Guid tenantId, string entityType, Guid entityId, string fieldName,
                            string oldValue, string newValue, Guid? userId = null);
        Task LogDeletedAsync(Guid tenantId, string entityType, Guid entityId, Guid? userId = null);
    }

    /// <summary>
    /// PHASE 1: Rules Engine Service
    /// Evaluates rules for compliance scope derivation
    /// </summary>
    public interface IRulesEngineService
    {
        // Control derivation
        Task<List<Models.Entities.Control>> DeriveApplicableControlsAsync(Guid tenantId, List<Guid> frameworkIds);

        // Custom rule evaluation
        Task<bool> EvaluateRuleAsync(string ruleExpression, Dictionary<string, object> context);

        /// <summary>
        /// Evaluate all active rules for a tenant based on their organization profile.
        /// Returns RuleExecutionLog with derived scope (Baselines, Packages, Templates).
        /// </summary>
        Task<RuleExecutionLog> EvaluateRulesAsync(
            Guid tenantId,
            OrganizationProfile profile,
            Ruleset ruleset,
            string userId);

        /// <summary>
        /// Derive and persist scope (TenantBaselines, TenantPackages, TenantTemplates) based on rules.
        /// </summary>
        Task<RuleExecutionLog> DeriveAndPersistScopeAsync(Guid tenantId, string userId);
    }
}