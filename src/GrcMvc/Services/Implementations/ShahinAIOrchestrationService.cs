using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Shahin-AI Orchestration Service
/// Ties all modules together: MAP, APPLY, PROVE, WATCH, FIX, VAULT
/// "MAP it, APPLY it, PROVE it—WATCH exceptions and FIX gaps; store in VAULT."
/// </summary>
public class ShahinAIOrchestrationService : IShahinAIOrchestrationService
{
    private readonly GrcDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISuiteGenerationService _suiteService;
    private readonly ILogger<ShahinAIOrchestrationService> _logger;

    public ShahinAIOrchestrationService(
        GrcDbContext dbContext,
        IUnitOfWork unitOfWork,
        ISuiteGenerationService suiteService,
        ILogger<ShahinAIOrchestrationService> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _suiteService = suiteService;
        _logger = logger;
    }

    #region MAP Module - Control Library & Mapping

    /// <summary>
    /// MAP: Get canonical controls for a tenant with all mappings
    /// </summary>
    public async Task<MAPModuleResult> GetControlLibraryAsync(Guid tenantId)
    {
        _logger.LogInformation("MAP: Getting control library for tenant {TenantId}", tenantId);

        var controlCount = await _dbContext.CanonicalControls
            .Where(c => c.IsActive)
            .CountAsync();

        return new MAPModuleResult
        {
            TenantId = tenantId,
            Module = "MAP",
            TotalControls = controlCount,
            Message = $"Retrieved {controlCount} canonical controls"
        };
    }

    #endregion

    #region APPLY Module - Scope & Applicability

    /// <summary>
    /// APPLY: Generate control suite based on organization profile
    /// </summary>
    public async Task<APPLYModuleResult> ApplyScopeRulesAsync(Guid tenantId, string userId)
    {
        _logger.LogInformation("APPLY: Applying scope rules for tenant {TenantId}", tenantId);

        var profile = await _dbContext.OrganizationProfiles
            .FirstOrDefaultAsync(p => p.TenantId == tenantId);

        if (profile == null)
        {
            return new APPLYModuleResult
            {
                TenantId = tenantId,
                Module = "APPLY",
                Success = false,
                Message = "Organization profile not found"
            };
        }

        var suite = await _suiteService.GenerateSuiteAsync(tenantId, profile, userId);

        return new APPLYModuleResult
        {
            TenantId = tenantId,
            Module = "APPLY",
            Success = true,
            SuiteCode = suite.SuiteCode,
            TotalControls = suite.TotalControls,
            MandatoryControls = suite.MandatoryControls,
            AppliedOverlays = suite.AppliedOverlaysJson,
            Message = $"Generated suite {suite.SuiteCode} with {suite.TotalControls} controls"
        };
    }

    #endregion

    #region PROVE Module - Evidence & Testing

    /// <summary>
    /// PROVE: Get evidence pack status for a tenant
    /// </summary>
    public async Task<PROVEModuleResult> GetEvidenceStatusAsync(Guid tenantId)
    {
        _logger.LogInformation("PROVE: Getting evidence status for tenant {TenantId}", tenantId);

        var evidenceCount = await _dbContext.AutoTaggedEvidences
            .Where(e => e.TenantId == tenantId)
            .CountAsync();

        var pendingCount = await _dbContext.AutoTaggedEvidences
            .Where(e => e.TenantId == tenantId && e.Status == "Collected")
            .CountAsync();

        var approvedCount = await _dbContext.AutoTaggedEvidences
            .Where(e => e.TenantId == tenantId && e.Status == "Approved")
            .CountAsync();

        return new PROVEModuleResult
        {
            TenantId = tenantId,
            Module = "PROVE",
            TotalEvidence = evidenceCount,
            PendingReview = pendingCount,
            Approved = approvedCount,
            CompletionRate = evidenceCount > 0 ? (decimal)approvedCount / evidenceCount * 100 : 0,
            Message = $"Evidence status: {approvedCount}/{evidenceCount} approved"
        };
    }

    #endregion

    #region WATCH Module - Monitoring & Alerts

    /// <summary>
    /// WATCH: Get KRI/KPI dashboard for a tenant
    /// </summary>
    public async Task<WATCHModuleResult> GetMonitoringDashboardAsync(Guid tenantId)
    {
        _logger.LogInformation("WATCH: Getting monitoring dashboard for tenant {TenantId}", tenantId);

        var indicatorCount = await _dbContext.RiskIndicators
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .CountAsync();

        var alerts = await _dbContext.RiskIndicatorAlerts
            .Where(a => a.Indicator.TenantId == tenantId && a.Status == "Open")
            .CountAsync();

        var criticalAlerts = await _dbContext.RiskIndicatorAlerts
            .Where(a => a.Indicator.TenantId == tenantId && a.Status == "Open" && a.Severity == "Critical")
            .CountAsync();

        return new WATCHModuleResult
        {
            TenantId = tenantId,
            Module = "WATCH",
            TotalIndicators = indicatorCount,
            OpenAlerts = alerts,
            CriticalAlerts = criticalAlerts,
            Message = $"Monitoring {indicatorCount} KRIs, {alerts} open alerts ({criticalAlerts} critical)"
        };
    }

    #endregion

    #region FIX Module - Remediation & Exceptions

    /// <summary>
    /// FIX: Get remediation status for a tenant
    /// </summary>
    public async Task<FIXModuleResult> GetRemediationStatusAsync(Guid tenantId)
    {
        _logger.LogInformation("FIX: Getting remediation status for tenant {TenantId}", tenantId);

        var exceptions = await _dbContext.ControlExceptions
            .Where(e => e.TenantId == tenantId)
            .ToListAsync();

        var openExceptions = exceptions.Count(e => e.Status == "Approved" && !e.IsExpired);
        var expiringExceptions = exceptions.Count(e => e.Status == "Approved" && e.DaysUntilExpiry <= 30 && e.DaysUntilExpiry > 0);

        var ccmExceptions = await _dbContext.CCMExceptions
            .Where(e => e.TestExecution.Test.TenantId == tenantId && e.Status == "Open")
            .CountAsync();

        return new FIXModuleResult
        {
            TenantId = tenantId,
            Module = "FIX",
            OpenExceptions = openExceptions,
            ExpiringExceptions = expiringExceptions,
            OpenCCMExceptions = ccmExceptions,
            Message = $"Remediation: {openExceptions} open exceptions, {expiringExceptions} expiring soon, {ccmExceptions} CCM exceptions"
        };
    }

    #endregion

    #region VAULT Module - Evidence Repository

    /// <summary>
    /// VAULT: Get evidence repository statistics
    /// </summary>
    public async Task<VAULTModuleResult> GetVaultStatisticsAsync(Guid tenantId)
    {
        _logger.LogInformation("VAULT: Getting vault statistics for tenant {TenantId}", tenantId);

        var capturedEvidence = await _dbContext.CapturedEvidences
            .Where(e => e.TenantId == tenantId)
            .CountAsync();

        var autoTaggedEvidence = await _dbContext.AutoTaggedEvidences
            .Where(e => e.TenantId == tenantId)
            .CountAsync();

        return new VAULTModuleResult
        {
            TenantId = tenantId,
            Module = "VAULT",
            TotalCapturedEvidence = capturedEvidence,
            TotalAutoTaggedEvidence = autoTaggedEvidence,
            TotalEvidence = capturedEvidence + autoTaggedEvidence,
            Message = $"Vault contains {capturedEvidence + autoTaggedEvidence} evidence items"
        };
    }

    #endregion

    #region Complete Dashboard

    /// <summary>
    /// Get complete Shahin-AI dashboard for a tenant
    /// </summary>
    public async Task<ShahinAIDashboard> GetCompleteDashboardAsync(Guid tenantId)
    {
        _logger.LogInformation("Getting complete Shahin-AI dashboard for tenant {TenantId}", tenantId);

        var mapResult = await GetControlLibraryAsync(tenantId);
        var proveResult = await GetEvidenceStatusAsync(tenantId);
        var watchResult = await GetMonitoringDashboardAsync(tenantId);
        var fixResult = await GetRemediationStatusAsync(tenantId);
        var vaultResult = await GetVaultStatisticsAsync(tenantId);

        return new ShahinAIDashboard
        {
            TenantId = tenantId,
            GeneratedAt = DateTime.UtcNow,
            MAP = mapResult,
            PROVE = proveResult,
            WATCH = watchResult,
            FIX = fixResult,
            VAULT = vaultResult,
            OverallHealthScore = CalculateHealthScore(proveResult, watchResult, fixResult)
        };
    }

    private decimal CalculateHealthScore(PROVEModuleResult prove, WATCHModuleResult watch, FIXModuleResult fix)
    {
        decimal score = 100;

        // Deduct for missing evidence
        if (prove.CompletionRate < 100)
            score -= (100 - prove.CompletionRate) * 0.3m;

        // Deduct for open alerts
        score -= watch.OpenAlerts * 2;
        score -= watch.CriticalAlerts * 5;

        // Deduct for open exceptions
        score -= fix.OpenExceptions * 1;
        score -= fix.ExpiringExceptions * 2;
        score -= fix.OpenCCMExceptions * 1;

        return Math.Max(0, Math.Min(100, score));
    }

    #endregion
}

#region Result DTOs

public class MAPModuleResult
{
    public Guid TenantId { get; set; }
    public string Module { get; set; } = "MAP";
    public int TotalControls { get; set; }
    public string Message { get; set; } = "";
}

public class APPLYModuleResult
{
    public Guid TenantId { get; set; }
    public string Module { get; set; } = "APPLY";
    public bool Success { get; set; }
    public string? SuiteCode { get; set; }
    public int TotalControls { get; set; }
    public int MandatoryControls { get; set; }
    public string? AppliedOverlays { get; set; }
    public string Message { get; set; } = "";
}

public class PROVEModuleResult
{
    public Guid TenantId { get; set; }
    public string Module { get; set; } = "PROVE";
    public int TotalEvidence { get; set; }
    public int PendingReview { get; set; }
    public int Approved { get; set; }
    public decimal CompletionRate { get; set; }
    public string Message { get; set; } = "";
}

public class WATCHModuleResult
{
    public Guid TenantId { get; set; }
    public string Module { get; set; } = "WATCH";
    public int TotalIndicators { get; set; }
    public int OpenAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public string Message { get; set; } = "";
}

public class FIXModuleResult
{
    public Guid TenantId { get; set; }
    public string Module { get; set; } = "FIX";
    public int OpenExceptions { get; set; }
    public int ExpiringExceptions { get; set; }
    public int OpenCCMExceptions { get; set; }
    public string Message { get; set; } = "";
}

public class VAULTModuleResult
{
    public Guid TenantId { get; set; }
    public string Module { get; set; } = "VAULT";
    public int TotalCapturedEvidence { get; set; }
    public int TotalAutoTaggedEvidence { get; set; }
    public int TotalEvidence { get; set; }
    public string Message { get; set; } = "";
}

public class ShahinAIDashboard
{
    public Guid TenantId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public MAPModuleResult MAP { get; set; } = new();
    public APPLYModuleResult? APPLY { get; set; }
    public PROVEModuleResult PROVE { get; set; } = new();
    public WATCHModuleResult WATCH { get; set; } = new();
    public FIXModuleResult FIX { get; set; } = new();
    public VAULTModuleResult VAULT { get; set; } = new();
    public decimal OverallHealthScore { get; set; }
}

#endregion
