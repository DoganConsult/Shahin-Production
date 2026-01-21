using GrcMvc.Services.Implementations;

namespace GrcMvc.Services;

/// <summary>
/// Shahin-AI Orchestration Service Interface
/// Coordinates all 6 modules: MAP, APPLY, PROVE, WATCH, FIX, VAULT
/// </summary>
public interface IShahinAIOrchestrationService
{
    /// <summary>
    /// MAP: Get canonical controls for a tenant
    /// </summary>
    Task<MAPModuleResult> GetControlLibraryAsync(Guid tenantId);

    /// <summary>
    /// APPLY: Generate control suite based on organization profile
    /// </summary>
    Task<APPLYModuleResult> ApplyScopeRulesAsync(Guid tenantId, string userId);

    /// <summary>
    /// PROVE: Get evidence pack status for a tenant
    /// </summary>
    Task<PROVEModuleResult> GetEvidenceStatusAsync(Guid tenantId);

    /// <summary>
    /// WATCH: Get KRI/KPI dashboard for a tenant
    /// </summary>
    Task<WATCHModuleResult> GetMonitoringDashboardAsync(Guid tenantId);

    /// <summary>
    /// FIX: Get remediation status for a tenant
    /// </summary>
    Task<FIXModuleResult> GetRemediationStatusAsync(Guid tenantId);

    /// <summary>
    /// VAULT: Get evidence repository statistics
    /// </summary>
    Task<VAULTModuleResult> GetVaultStatisticsAsync(Guid tenantId);

    /// <summary>
    /// Get complete Shahin-AI dashboard for a tenant
    /// </summary>
    Task<ShahinAIDashboard> GetCompleteDashboardAsync(Guid tenantId);
}
