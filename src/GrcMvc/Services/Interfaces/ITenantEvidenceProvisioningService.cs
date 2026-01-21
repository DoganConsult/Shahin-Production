using GrcMvc.Services.Implementations;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Auto-generates evidence requirements for tenants upon onboarding
/// </summary>
public interface ITenantEvidenceProvisioningService
{
    /// <summary>
    /// Auto-generate all evidence requirements for a tenant based on their profile
    /// </summary>
    Task<EvidenceProvisioningResult> ProvisionEvidenceRequirementsAsync(
        Guid tenantId, 
        string sector, 
        string? orgType = null,
        string createdBy = "system");

    /// <summary>
    /// Get evidence requirements summary for a tenant
    /// </summary>
    Task<TenantEvidenceSummary> GetEvidenceSummaryAsync(Guid tenantId);

    /// <summary>
    /// Refresh evidence requirements for a tenant (add new ones, don't remove existing)
    /// </summary>
    Task<EvidenceProvisioningResult> RefreshEvidenceRequirementsAsync(Guid tenantId, string createdBy = "system");
}
