using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Auto-generates evidence requirements for a tenant upon onboarding
/// Based on sector → org type → frameworks → controls → evidence mapping
/// </summary>
public class TenantEvidenceProvisioningService : ITenantEvidenceProvisioningService
{
    private readonly GrcDbContext _context;
    private readonly ISectorFrameworkCacheService _cacheService;
    private readonly IExpertFrameworkMappingService _expertService;
    private readonly ILogger<TenantEvidenceProvisioningService> _logger;

    public TenantEvidenceProvisioningService(
        GrcDbContext context,
        ISectorFrameworkCacheService cacheService,
        IExpertFrameworkMappingService expertService,
        ILogger<TenantEvidenceProvisioningService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _expertService = expertService;
        _logger = logger;
    }

    /// <summary>
    /// Auto-generate all evidence requirements for a tenant based on their profile
    /// Called automatically after onboarding completion
    /// </summary>
    public async Task<EvidenceProvisioningResult> ProvisionEvidenceRequirementsAsync(
        Guid tenantId, 
        string sector, 
        string? orgType = null,
        string createdBy = "system")
    {
        var result = new EvidenceProvisioningResult
        {
            TenantId = tenantId,
            Sector = sector,
            OrgType = orgType ?? "ALL"
        };

        try
        {
            _logger.LogInformation("Starting evidence provisioning for tenant {TenantId}, sector {Sector}", tenantId, sector);

            // Step 1: Get sector compliance bundle (cached)
            var bundle = await _cacheService.GetSectorBundleAsync(sector, orgType);

            // Step 2: Get the tenant's workspace
            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.IsDefault);

            Guid? workspaceId = workspace?.Id;

            // Step 3: Get existing evidence requirements to avoid duplicates
            var existingRequirements = await _context.TenantEvidenceRequirements
                .Where(r => r.TenantId == tenantId)
                .Select(r => new { r.FrameworkCode, r.ControlNumber, r.EvidenceTypeCode })
                .ToListAsync();

            var existingKeys = existingRequirements
                .Select(r => $"{r.FrameworkCode}|{r.ControlNumber}|{r.EvidenceTypeCode}")
                .ToHashSet();

            // Step 4: For each framework, get controls and their evidence requirements
            var newRequirements = new List<TenantEvidenceRequirement>();

            foreach (var framework in bundle.Frameworks.Where(f => f.IsMandatory))
            {
                var controls = await _context.FrameworkControls
                    .Where(c => c.FrameworkCode == framework.FrameworkCode && !string.IsNullOrEmpty(c.EvidenceRequirements))
                    .Select(c => new { c.ControlNumber, c.EvidenceRequirements, c.Domain })
                    .ToListAsync();

                foreach (var control in controls)
                {
                    var evidenceTypes = control.EvidenceRequirements.Split('|')
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Select(e => e.Trim())
                        .Distinct();

                    foreach (var evidenceType in evidenceTypes)
                    {
                        var key = $"{framework.FrameworkCode}|{control.ControlNumber}|{evidenceType}";
                        if (existingKeys.Contains(key))
                            continue;

                        // Get scoring criteria for this evidence type
                        var criteria = bundle.EvidenceByFramework
                            .GetValueOrDefault(framework.FrameworkCode)?
                            .FirstOrDefault(e => e.EvidenceType == evidenceType)?.Criteria;

                        var requirement = new TenantEvidenceRequirement
                        {
                            Id = Guid.NewGuid(),
                            TenantId = tenantId,
                            EvidenceTypeCode = evidenceType.Replace(" ", "_").ToUpper(),
                            EvidenceTypeName = evidenceType,
                            FrameworkCode = framework.FrameworkCode,
                            ControlNumber = control.ControlNumber,
                            MinimumScore = criteria?.MinimumScore ?? 70,
                            CollectionFrequency = criteria?.CollectionFrequency ?? "Annual",
                            DefaultValidityDays = criteria?.DefaultValidityDays ?? 365,
                            Status = "NotStarted",
                            DueDate = CalculateDueDate(criteria?.CollectionFrequency ?? "Annual"),
                            WorkspaceId = workspaceId,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = createdBy
                        };

                        newRequirements.Add(requirement);
                        existingKeys.Add(key); // Prevent duplicates within this batch
                    }
                }

                result.FrameworksProcessed.Add(framework.FrameworkCode);
            }

            // Step 5: Batch insert
            if (newRequirements.Count > 0)
            {
                await _context.TenantEvidenceRequirements.AddRangeAsync(newRequirements);
                await _context.SaveChangesAsync();
            }

            result.Success = true;
            result.EvidenceRequirementsCreated = newRequirements.Count;
            result.ControlsProcessed = newRequirements.Select(r => r.ControlNumber).Distinct().Count();

            _logger.LogInformation(
                "Evidence provisioning complete for tenant {TenantId}: {Count} requirements created for {Frameworks} frameworks",
                tenantId, newRequirements.Count, result.FrameworksProcessed.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error provisioning evidence requirements for tenant {TenantId}", tenantId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Get evidence requirements summary for a tenant
    /// </summary>
    public async Task<TenantEvidenceSummary> GetEvidenceSummaryAsync(Guid tenantId)
    {
        var requirements = await _context.TenantEvidenceRequirements
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        return new TenantEvidenceSummary
        {
            TenantId = tenantId,
            TotalRequirements = requirements.Count,
            ByStatus = requirements.GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByFramework = requirements.GroupBy(r => r.FrameworkCode)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByFrequency = requirements.GroupBy(r => r.CollectionFrequency)
                .ToDictionary(g => g.Key, g => g.Count()),
            OverdueCount = requirements.Count(r => r.DueDate.HasValue && r.DueDate < DateTime.UtcNow && r.Status != "Approved"),
            ExpiringCount = requirements.Count(r => r.ExpiryDate.HasValue && r.ExpiryDate < DateTime.UtcNow.AddDays(30)),
            AverageScore = requirements.Where(r => r.CurrentScore > 0).Select(r => r.CurrentScore).DefaultIfEmpty(0).Average()
        };
    }

    /// <summary>
    /// Refresh evidence requirements for a tenant (add new ones, don't remove existing)
    /// </summary>
    public async Task<EvidenceProvisioningResult> RefreshEvidenceRequirementsAsync(Guid tenantId, string createdBy = "system")
    {
        var profile = await _context.OrganizationProfiles
            .FirstOrDefaultAsync(p => p.TenantId == tenantId);

        if (profile == null)
        {
            return new EvidenceProvisioningResult
            {
                TenantId = tenantId,
                Success = false,
                ErrorMessage = "Organization profile not found"
            };
        }

        return await ProvisionEvidenceRequirementsAsync(tenantId, profile.Sector, null, createdBy);
    }

    private DateTime? CalculateDueDate(string frequency)
    {
        return frequency.ToLower() switch
        {
            "continuous" => DateTime.UtcNow.AddDays(7),
            "weekly" => DateTime.UtcNow.AddDays(7),
            "monthly" => DateTime.UtcNow.AddDays(30),
            "quarterly" => DateTime.UtcNow.AddDays(90),
            "semi-annual" => DateTime.UtcNow.AddDays(180),
            "annual" => DateTime.UtcNow.AddDays(365),
            "ondemand" => null,
            _ => DateTime.UtcNow.AddDays(90) // Default 90 days
        };
    }
}

#region DTOs

public class EvidenceProvisioningResult
{
    public Guid TenantId { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string OrgType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int EvidenceRequirementsCreated { get; set; }
    public int ControlsProcessed { get; set; }
    public List<string> FrameworksProcessed { get; set; } = new();
}

public class TenantEvidenceSummary
{
    public Guid TenantId { get; set; }
    public int TotalRequirements { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByFramework { get; set; } = new();
    public Dictionary<string, int> ByFrequency { get; set; } = new();
    public int OverdueCount { get; set; }
    public int ExpiringCount { get; set; }
    public double AverageScore { get; set; }
}

#endregion
