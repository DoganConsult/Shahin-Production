using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace GrcMvc.Controllers.Api;

/// <summary>
/// Real-Time Compliance Dashboard API Controller
/// Provides live regulatory compliance data for KSA frameworks (NCA-ECC, SAMA-CSF, PDPL)
/// Supports Arabic/English bilingual content
/// </summary>
[Route("api/compliance/realtime")]
[ApiController]
[Authorize]
[IgnoreAntiforgeryToken]
public class RealTimeComplianceDashboardController : ControllerBase
{
    private readonly GrcDbContext _dbContext;
    
    // ABP Service for modern tenant management
    private readonly ICurrentTenant _currentTenant;
    
    // Legacy service for backward compatibility
    private readonly ITenantContextService _tenantContext;
    private readonly ILogger<RealTimeComplianceDashboardController> _logger;

    public RealTimeComplianceDashboardController(
        GrcDbContext dbContext,
        ICurrentTenant currentTenant,
        ITenantContextService tenantContext,
        ILogger<RealTimeComplianceDashboardController> logger)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Main Dashboard

    /// <summary>
    /// Get Real-Time Compliance Overview
    /// GET /api/compliance/realtime/overview
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<ComplianceOverviewDto>> GetOverview([FromQuery] string? lang = "en")
    {
        try
        {
            // Use ABP's ICurrentTenant (modern approach)
            var tenantId = _currentTenant.Id ?? _tenantContext.GetCurrentTenantId();
            var isArabic = lang?.ToLower() == "ar";

            // Get framework controls and tenant controls
            var frameworkControls = await _dbContext.FrameworkControls
                .Where(fc => fc.Status == "Active")
                .ToListAsync();

            var tenantControls = await _dbContext.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .ToListAsync();

            var evidences = await _dbContext.Evidences
                .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                .ToListAsync();

            // Calculate compliance by framework
            var frameworks = new List<FrameworkComplianceDto>();

            // NCA-ECC
            var ncaControls = frameworkControls.Where(fc => fc.FrameworkCode == "NCA-ECC").ToList();
            var ncaTenantControls = tenantControls.Where(c => c.SourceFrameworkCode == "NCA-ECC").ToList();
            frameworks.Add(CalculateFrameworkCompliance(
                "NCA-ECC",
                isArabic ? "ضوابط الأمن السيبراني الأساسية" : "Essential Cybersecurity Controls",
                ncaControls, ncaTenantControls, evidences, isArabic));

            // SAMA-CSF
            var samaControls = frameworkControls.Where(fc => fc.FrameworkCode == "SAMA-CSF").ToList();
            var samaTenantControls = tenantControls.Where(c => c.SourceFrameworkCode == "SAMA-CSF").ToList();
            frameworks.Add(CalculateFrameworkCompliance(
                "SAMA-CSF",
                isArabic ? "إطار الأمن السيبراني لساما" : "SAMA Cybersecurity Framework",
                samaControls, samaTenantControls, evidences, isArabic));

            // PDPL
            var pdplControls = frameworkControls.Where(fc => fc.FrameworkCode == "PDPL").ToList();
            var pdplTenantControls = tenantControls.Where(c => c.SourceFrameworkCode == "PDPL").ToList();
            frameworks.Add(CalculateFrameworkCompliance(
                "PDPL",
                isArabic ? "نظام حماية البيانات الشخصية" : "Personal Data Protection Law",
                pdplControls, pdplTenantControls, evidences, isArabic));

            // Overall compliance score
            var totalControls = ncaTenantControls.Count + samaTenantControls.Count + pdplTenantControls.Count;
            var controlsWithEvidence = tenantControls.Count(c => evidences.Any(e => e.ControlId == c.Id));
            var overallScore = totalControls > 0
                ? Math.Round((decimal)controlsWithEvidence / totalControls * 100, 1)
                : 0;

            return Ok(new ComplianceOverviewDto
            {
                OverallComplianceScore = overallScore,
                OverallStatus = GetComplianceStatus(overallScore, isArabic),
                TotalFrameworks = 3,
                TotalControls = totalControls,
                ImplementedControls = controlsWithEvidence,
                PendingControls = totalControls - controlsWithEvidence,
                Frameworks = frameworks,
                LastUpdated = DateTime.UtcNow,
                RefreshInterval = 30, // seconds
                Language = isArabic ? "ar" : "en"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance overview");
            return StatusCode(500, new { error = "Failed to load compliance overview" });
        }
    }

    /// <summary>
    /// Get Framework-specific compliance details
    /// GET /api/compliance/realtime/framework/{frameworkCode}
    /// </summary>
    [HttpGet("framework/{frameworkCode}")]
    public async Task<ActionResult<FrameworkDetailDto>> GetFrameworkDetails(
        string frameworkCode,
        [FromQuery] string? lang = "en")
    {
        try
        {
            // Use ABP's ICurrentTenant (modern approach)
            var tenantId = _currentTenant.Id ?? _tenantContext.GetCurrentTenantId();
            var isArabic = lang?.ToLower() == "ar";

            var frameworkControls = await _dbContext.FrameworkControls
                .Where(fc => fc.FrameworkCode == frameworkCode && fc.Status == "Active")
                .OrderBy(fc => fc.Domain)
                .ThenBy(fc => fc.ControlNumber)
                .ToListAsync();

            if (!frameworkControls.Any())
            {
                return NotFound(new { error = $"Framework {frameworkCode} not found" });
            }

            var tenantControls = await _dbContext.Controls
                .Where(c => c.TenantId == tenantId && c.SourceFrameworkCode == frameworkCode && !c.IsDeleted)
                .ToListAsync();

            var evidences = await _dbContext.Evidences
                .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                .ToListAsync();

            // Group by domain
            var domains = frameworkControls
                .GroupBy(fc => fc.Domain)
                .Select(g =>
                {
                    var domainControls = g.ToList();
                    var implementedControls = tenantControls
                        .Where(tc => domainControls.Any(dc => dc.ControlNumber == tc.SourceControlCode?.Replace($"{frameworkCode}-", "")))
                        .ToList();
                    var withEvidence = implementedControls
                        .Where(ic => evidences.Any(e => e.ControlId == ic.Id))
                        .Count();

                    return new DomainComplianceDto
                    {
                        DomainCode = g.Key,
                        DomainName = isArabic
                            ? GetArabicDomainName(g.Key)
                            : g.Key,
                        TotalControls = domainControls.Count,
                        ImplementedControls = implementedControls.Count,
                        ControlsWithEvidence = withEvidence,
                        CompliancePercentage = domainControls.Count > 0
                            ? Math.Round((decimal)withEvidence / domainControls.Count * 100, 1)
                            : 0,
                        Status = GetDomainStatus(domainControls.Count, withEvidence, isArabic),
                        Controls = domainControls.Select(dc => new ControlStatusDto
                        {
                            ControlNumber = dc.ControlNumber,
                            FullControlId = dc.FullControlId,
                            Title = isArabic ? dc.TitleAr : dc.TitleEn,
                            Requirement = isArabic ? dc.RequirementAr : dc.RequirementEn,
                            ControlType = dc.ControlType,
                            MaturityLevel = dc.MaturityLevel,
                            IsImplemented = tenantControls.Any(tc => tc.SourceControlCode?.Contains(dc.ControlNumber) == true),
                            HasEvidence = implementedControls.Any(ic =>
                                ic.SourceControlCode?.Contains(dc.ControlNumber) == true &&
                                evidences.Any(e => e.ControlId == ic.Id)),
                            EvidenceCount = implementedControls
                                .Where(ic => ic.SourceControlCode?.Contains(dc.ControlNumber) == true)
                                .SelectMany(ic => evidences.Where(e => e.ControlId == ic.Id))
                                .Count(),
                            IsoMapping = dc.MappingIso27001,
                            NistMapping = dc.MappingNist,
                            EvidenceTypes = dc.EvidenceRequirements?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()
                        }).ToList()
                    };
                })
                .OrderBy(d => d.DomainCode)
                .ToList();

            var totalControls = frameworkControls.Count;
            var implementedTotal = tenantControls.Count;
            var withEvidenceTotal = tenantControls.Count(tc => evidences.Any(e => e.ControlId == tc.Id));

            return Ok(new FrameworkDetailDto
            {
                FrameworkCode = frameworkCode,
                FrameworkName = GetFrameworkName(frameworkCode, isArabic),
                Version = frameworkControls.FirstOrDefault()?.Version ?? "1.0",
                TotalControls = totalControls,
                ImplementedControls = implementedTotal,
                ControlsWithEvidence = withEvidenceTotal,
                CompliancePercentage = totalControls > 0
                    ? Math.Round((decimal)withEvidenceTotal / totalControls * 100, 1)
                    : 0,
                Domains = domains,
                LastUpdated = DateTime.UtcNow,
                Language = isArabic ? "ar" : "en"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting framework details for {Framework}", frameworkCode);
            return StatusCode(500, new { error = "Failed to load framework details" });
        }
    }

    /// <summary>
    /// Get Cross-Framework Mapping (NIST/ISO Auto-mapping)
    /// GET /api/compliance/realtime/mapping
    /// </summary>
    [HttpGet("mapping")]
    public async Task<ActionResult<CrossFrameworkMappingDto>> GetCrossFrameworkMapping(
        [FromQuery] string? sourceFramework = "NCA-ECC",
        [FromQuery] string? lang = "en")
    {
        try
        {
            var isArabic = lang?.ToLower() == "ar";

            var controls = await _dbContext.FrameworkControls
                .Where(fc => fc.FrameworkCode == sourceFramework && fc.Status == "Active")
                .OrderBy(fc => fc.ControlNumber)
                .ToListAsync();

            var mappings = controls
                .Where(c => !string.IsNullOrEmpty(c.MappingIso27001) || !string.IsNullOrEmpty(c.MappingNist))
                .Select(c => new ControlMappingDto
                {
                    SourceControl = c.FullControlId,
                    SourceTitle = isArabic ? c.TitleAr : c.TitleEn,
                    Iso27001Controls = c.MappingIso27001?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                    NistCsfControls = c.MappingNist?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                    Domain = c.Domain,
                    ControlType = c.ControlType
                })
                .ToList();

            // Summary statistics
            var isoMappingCount = mappings.Count(m => m.Iso27001Controls.Any());
            var nistMappingCount = mappings.Count(m => m.NistCsfControls.Any());

            return Ok(new CrossFrameworkMappingDto
            {
                SourceFramework = sourceFramework,
                SourceFrameworkName = GetFrameworkName(sourceFramework, isArabic),
                TotalControls = controls.Count,
                ControlsWithIsoMapping = isoMappingCount,
                ControlsWithNistMapping = nistMappingCount,
                IsoMappingPercentage = controls.Count > 0
                    ? Math.Round((decimal)isoMappingCount / controls.Count * 100, 1)
                    : 0,
                NistMappingPercentage = controls.Count > 0
                    ? Math.Round((decimal)nistMappingCount / controls.Count * 100, 1)
                    : 0,
                Mappings = mappings,
                LastUpdated = DateTime.UtcNow,
                Language = isArabic ? "ar" : "en"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cross-framework mapping");
            return StatusCode(500, new { error = "Failed to load cross-framework mapping" });
        }
    }

    /// <summary>
    /// Get Evidence Collection Status (Real-time)
    /// GET /api/compliance/realtime/evidence-status
    /// </summary>
    [HttpGet("evidence-status")]
    public async Task<ActionResult<EvidenceCollectionStatusDto>> GetEvidenceCollectionStatus(
        [FromQuery] string? frameworkCode = null,
        [FromQuery] string? lang = "en")
    {
        try
        {
            // Use ABP's ICurrentTenant (modern approach)
            var tenantId = _currentTenant.Id ?? _tenantContext.GetCurrentTenantId();
            var isArabic = lang?.ToLower() == "ar";

            var query = _dbContext.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted);

            if (!string.IsNullOrEmpty(frameworkCode))
            {
                query = query.Where(c => c.SourceFrameworkCode == frameworkCode);
            }

            var controls = await query.ToListAsync();

            var evidences = await _dbContext.Evidences
                .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                .ToListAsync();

            var controlStatuses = controls.Select(c =>
            {
                var controlEvidences = evidences.Where(e => e.ControlId == c.Id).ToList();
                var requiredEvidenceTypes = c.SourceFrameworkCode != null
                    ? GetRequiredEvidenceTypes(c.SourceFrameworkCode, c.SourceControlCode)
                    : new List<string>();

                return new ControlEvidenceStatusDto
                {
                    ControlId = c.Id,
                    ControlCode = c.SourceControlCode ?? c.ControlCode,
                    ControlName = c.Name,
                    FrameworkCode = c.SourceFrameworkCode ?? "Custom",
                    TotalEvidences = controlEvidences.Count,
                    VerifiedEvidences = controlEvidences.Count(e => e.VerificationStatus == "Verified"),
                    PendingEvidences = controlEvidences.Count(e => e.VerificationStatus == "Pending"),
                    RejectedEvidences = controlEvidences.Count(e => e.VerificationStatus == "Rejected"),
                    RequiredEvidenceTypes = requiredEvidenceTypes,
                    CollectedEvidenceTypes = controlEvidences.Select(e => e.Type).Distinct().ToList(),
                    CompletionPercentage = requiredEvidenceTypes.Count > 0
                        ? Math.Round((decimal)controlEvidences.Select(e => e.Type).Distinct().Count() / requiredEvidenceTypes.Count * 100, 1)
                        : controlEvidences.Any() ? 100 : 0,
                    Status = GetEvidenceStatus(controlEvidences, isArabic),
                    LastUpdated = controlEvidences.Any() ? controlEvidences.Max(e => e.ModifiedDate) : null
                };
            }).ToList();

            var summary = new EvidenceCollectionSummaryDto
            {
                TotalControls = controls.Count,
                ControlsWithEvidence = controlStatuses.Count(s => s.TotalEvidences > 0),
                ControlsWithoutEvidence = controlStatuses.Count(s => s.TotalEvidences == 0),
                TotalEvidences = evidences.Count,
                VerifiedEvidences = evidences.Count(e => e.VerificationStatus == "Verified"),
                PendingEvidences = evidences.Count(e => e.VerificationStatus == "Pending"),
                OverallCompletionPercentage = controls.Count > 0
                    ? Math.Round((decimal)controlStatuses.Count(s => s.TotalEvidences > 0) / controls.Count * 100, 1)
                    : 0
            };

            return Ok(new EvidenceCollectionStatusDto
            {
                Summary = summary,
                ControlStatuses = controlStatuses.OrderByDescending(s => s.CompletionPercentage == 0).ThenBy(s => s.ControlCode).ToList(),
                ByFramework = controlStatuses
                    .GroupBy(s => s.FrameworkCode)
                    .Select(g => new FrameworkEvidenceSummaryDto
                    {
                        FrameworkCode = g.Key,
                        FrameworkName = GetFrameworkName(g.Key, isArabic),
                        TotalControls = g.Count(),
                        ControlsWithEvidence = g.Count(s => s.TotalEvidences > 0),
                        CompletionPercentage = g.Count() > 0
                            ? Math.Round((decimal)g.Count(s => s.TotalEvidences > 0) / g.Count() * 100, 1)
                            : 0
                    })
                    .ToList(),
                LastUpdated = DateTime.UtcNow,
                Language = isArabic ? "ar" : "en"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting evidence collection status");
            return StatusCode(500, new { error = "Failed to load evidence collection status" });
        }
    }

    /// <summary>
    /// Get Compliance Alerts (Real-time)
    /// GET /api/compliance/realtime/alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<ActionResult<ComplianceAlertsDto>> GetComplianceAlerts([FromQuery] string? lang = "en")
    {
        try
        {
            // Use ABP's ICurrentTenant (modern approach)
            var tenantId = _currentTenant.Id ?? _tenantContext.GetCurrentTenantId();
            var isArabic = lang?.ToLower() == "ar";
            var now = DateTime.UtcNow;

            var alerts = new List<ComplianceAlertDto>();

            // Check for controls without evidence
            var controlsWithoutEvidence = await _dbContext.Controls
                .Where(c => c.TenantId == tenantId && !c.IsDeleted)
                .Where(c => !_dbContext.Evidences.Any(e => e.ControlId == c.Id && !e.IsDeleted))
                .Take(10)
                .ToListAsync();

            foreach (var control in controlsWithoutEvidence)
            {
                alerts.Add(new ComplianceAlertDto
                {
                    Id = Guid.NewGuid(),
                    AlertType = "MissingEvidence",
                    Severity = "High",
                    Title = isArabic
                        ? $"ضابط بدون شواهد: {control.SourceControlCode ?? control.ControlCode}"
                        : $"Control Missing Evidence: {control.SourceControlCode ?? control.ControlCode}",
                    Description = isArabic
                        ? $"الضابط {control.Name} لا يحتوي على أي شواهد مرفقة"
                        : $"Control {control.Name} has no evidence attached",
                    FrameworkCode = control.SourceFrameworkCode,
                    ControlCode = control.SourceControlCode ?? control.ControlCode,
                    ActionUrl = $"/controls/{control.Id}",
                    CreatedAt = now
                });
            }

            // Check for expiring evidence
            var expiringEvidence = await _dbContext.Evidences
                .Where(e => e.TenantId == tenantId && !e.IsDeleted)
                .Where(e => e.RetentionEndDate != null && e.RetentionEndDate < now.AddDays(30))
                .Take(10)
                .ToListAsync();

            foreach (var evidence in expiringEvidence)
            {
                alerts.Add(new ComplianceAlertDto
                {
                    Id = Guid.NewGuid(),
                    AlertType = "ExpiringEvidence",
                    Severity = evidence.RetentionEndDate < now.AddDays(7) ? "Critical" : "Medium",
                    Title = isArabic
                        ? $"شاهد منتهي الصلاحية: {evidence.Title}"
                        : $"Expiring Evidence: {evidence.Title}",
                    Description = isArabic
                        ? $"الشاهد سينتهي في {evidence.RetentionEndDate:yyyy-MM-dd}"
                        : $"Evidence expires on {evidence.RetentionEndDate:yyyy-MM-dd}",
                    FrameworkCode = evidence.FrameworkCode,
                    ControlCode = null,
                    ActionUrl = $"/evidence/{evidence.Id}",
                    CreatedAt = now
                });
            }

            // Check for rejected evidence
            var rejectedEvidence = await _dbContext.Evidences
                .Where(e => e.TenantId == tenantId && !e.IsDeleted && e.VerificationStatus == "Rejected")
                .Take(5)
                .ToListAsync();

            foreach (var evidence in rejectedEvidence)
            {
                alerts.Add(new ComplianceAlertDto
                {
                    Id = Guid.NewGuid(),
                    AlertType = "RejectedEvidence",
                    Severity = "High",
                    Title = isArabic
                        ? $"شاهد مرفوض: {evidence.Title}"
                        : $"Rejected Evidence: {evidence.Title}",
                    Description = isArabic
                        ? "يجب مراجعة الشاهد المرفوض وتقديم بديل"
                        : "Review rejected evidence and submit replacement",
                    FrameworkCode = evidence.FrameworkCode,
                    ControlCode = null,
                    ActionUrl = $"/evidence/{evidence.Id}",
                    CreatedAt = evidence.VerificationDate ?? now
                });
            }

            return Ok(new ComplianceAlertsDto
            {
                TotalAlerts = alerts.Count,
                CriticalAlerts = alerts.Count(a => a.Severity == "Critical"),
                HighAlerts = alerts.Count(a => a.Severity == "High"),
                MediumAlerts = alerts.Count(a => a.Severity == "Medium"),
                Alerts = alerts.OrderByDescending(a => a.Severity == "Critical" ? 3 : a.Severity == "High" ? 2 : 1).ToList(),
                LastUpdated = now,
                Language = isArabic ? "ar" : "en"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance alerts");
            return StatusCode(500, new { error = "Failed to load compliance alerts" });
        }
    }

    #endregion

    #region Helper Methods

    private FrameworkComplianceDto CalculateFrameworkCompliance(
        string code, string name,
        List<FrameworkControl> frameworkControls,
        List<Control> tenantControls,
        List<Evidence> evidences,
        bool isArabic)
    {
        var totalControls = frameworkControls.Count;
        var implementedControls = tenantControls.Count;
        var controlsWithEvidence = tenantControls.Count(tc => evidences.Any(e => e.ControlId == tc.Id));
        var compliancePercentage = totalControls > 0
            ? Math.Round((decimal)controlsWithEvidence / totalControls * 100, 1)
            : 0;

        return new FrameworkComplianceDto
        {
            FrameworkCode = code,
            FrameworkName = name,
            TotalControls = totalControls,
            ImplementedControls = implementedControls,
            ControlsWithEvidence = controlsWithEvidence,
            CompliancePercentage = compliancePercentage,
            Status = GetComplianceStatus(compliancePercentage, isArabic),
            StatusColor = GetStatusColor(compliancePercentage),
            Trend = "stable", // Would calculate from historical data
            DomainBreakdown = frameworkControls
                .GroupBy(fc => fc.Domain)
                .Select(g => new DomainSummaryDto
                {
                    Domain = isArabic ? GetArabicDomainName(g.Key) : g.Key,
                    TotalControls = g.Count(),
                    ImplementedControls = tenantControls.Count(tc =>
                        g.Any(fc => tc.SourceControlCode?.Contains(fc.ControlNumber) == true)),
                    CompliancePercentage = g.Count() > 0
                        ? Math.Round((decimal)tenantControls.Count(tc =>
                            g.Any(fc => tc.SourceControlCode?.Contains(fc.ControlNumber) == true) &&
                            evidences.Any(e => e.ControlId == tc.Id)) / g.Count() * 100, 1)
                        : 0
                })
                .ToList()
        };
    }

    private static string GetComplianceStatus(decimal percentage, bool isArabic)
    {
        return percentage switch
        {
            >= 90 => isArabic ? "ممتاز" : "Excellent",
            >= 80 => isArabic ? "جيد جداً" : "Very Good",
            >= 70 => isArabic ? "جيد" : "Good",
            >= 50 => isArabic ? "يحتاج تحسين" : "Needs Improvement",
            _ => isArabic ? "حرج" : "Critical"
        };
    }

    private static string GetDomainStatus(int total, int withEvidence, bool isArabic)
    {
        if (total == 0) return isArabic ? "غير محدد" : "N/A";
        var percentage = (decimal)withEvidence / total * 100;
        return GetComplianceStatus(percentage, isArabic);
    }

    private static string GetStatusColor(decimal percentage)
    {
        return percentage switch
        {
            >= 80 => "success",
            >= 60 => "warning",
            _ => "danger"
        };
    }

    private static string GetFrameworkName(string? code, bool isArabic)
    {
        return code switch
        {
            "NCA-ECC" => isArabic ? "ضوابط الأمن السيبراني الأساسية" : "NCA Essential Cybersecurity Controls",
            "SAMA-CSF" => isArabic ? "إطار الأمن السيبراني لساما" : "SAMA Cybersecurity Framework",
            "PDPL" => isArabic ? "نظام حماية البيانات الشخصية" : "Personal Data Protection Law",
            "ISO-27001" => isArabic ? "آيزو 27001" : "ISO 27001",
            _ => code ?? (isArabic ? "مخصص" : "Custom")
        };
    }

    private static string GetArabicDomainName(string? domain)
    {
        return domain switch
        {
            "Governance" => "الحوكمة",
            "Defense" => "الدفاع",
            "Resilience" => "المرونة",
            "Third-Party" => "الأطراف الثالثة",
            "ICS" => "أنظمة التحكم الصناعي",
            "Leadership" => "القيادة",
            "Risk" => "إدارة المخاطر",
            "Operations" => "العمليات",
            "Principles" => "المبادئ",
            "Rights" => "الحقوق",
            "Security" => "الأمن",
            "Breach" => "الإخطار",
            "Transfer" => "النقل",
            _ => domain ?? "عام"
        };
    }

    private static string GetEvidenceStatus(List<Evidence> evidences, bool isArabic)
    {
        if (!evidences.Any()) return isArabic ? "لا توجد شواهد" : "No Evidence";
        if (evidences.All(e => e.VerificationStatus == "Verified")) return isArabic ? "مكتمل" : "Complete";
        if (evidences.Any(e => e.VerificationStatus == "Rejected")) return isArabic ? "مرفوض" : "Rejected";
        if (evidences.Any(e => e.VerificationStatus == "Pending")) return isArabic ? "قيد المراجعة" : "Pending Review";
        return isArabic ? "جزئي" : "Partial";
    }

    private List<string> GetRequiredEvidenceTypes(string frameworkCode, string? controlCode)
    {
        // This would lookup the EvidenceRequirements from FrameworkControl
        // For now, return common evidence types
        return new List<string> { "POLICY_DOC", "CONFIG_EXPORT", "AUDIT_LOG" };
    }

    #endregion
}

#region DTOs

public record ComplianceOverviewDto
{
    public decimal OverallComplianceScore { get; init; }
    public string OverallStatus { get; init; } = "";
    public int TotalFrameworks { get; init; }
    public int TotalControls { get; init; }
    public int ImplementedControls { get; init; }
    public int PendingControls { get; init; }
    public List<FrameworkComplianceDto> Frameworks { get; init; } = new();
    public DateTime LastUpdated { get; init; }
    public int RefreshInterval { get; init; }
    public string Language { get; init; } = "en";
}

public record FrameworkComplianceDto
{
    public string FrameworkCode { get; init; } = "";
    public string FrameworkName { get; init; } = "";
    public int TotalControls { get; init; }
    public int ImplementedControls { get; init; }
    public int ControlsWithEvidence { get; init; }
    public decimal CompliancePercentage { get; init; }
    public string Status { get; init; } = "";
    public string StatusColor { get; init; } = "";
    public string Trend { get; init; } = "";
    public List<DomainSummaryDto> DomainBreakdown { get; init; } = new();
}

public record DomainSummaryDto
{
    public string Domain { get; init; } = "";
    public int TotalControls { get; init; }
    public int ImplementedControls { get; init; }
    public decimal CompliancePercentage { get; init; }
}

public record FrameworkDetailDto
{
    public string FrameworkCode { get; init; } = "";
    public string FrameworkName { get; init; } = "";
    public string Version { get; init; } = "";
    public int TotalControls { get; init; }
    public int ImplementedControls { get; init; }
    public int ControlsWithEvidence { get; init; }
    public decimal CompliancePercentage { get; init; }
    public List<DomainComplianceDto> Domains { get; init; } = new();
    public DateTime LastUpdated { get; init; }
    public string Language { get; init; } = "en";
}

public record DomainComplianceDto
{
    public string DomainCode { get; init; } = "";
    public string DomainName { get; init; } = "";
    public int TotalControls { get; init; }
    public int ImplementedControls { get; init; }
    public int ControlsWithEvidence { get; init; }
    public decimal CompliancePercentage { get; init; }
    public string Status { get; init; } = "";
    public List<ControlStatusDto> Controls { get; init; } = new();
}

public record ControlStatusDto
{
    public string ControlNumber { get; init; } = "";
    public string FullControlId { get; init; } = "";
    public string Title { get; init; } = "";
    public string Requirement { get; init; } = "";
    public string ControlType { get; init; } = "";
    public int MaturityLevel { get; init; }
    public bool IsImplemented { get; init; }
    public bool HasEvidence { get; init; }
    public int EvidenceCount { get; init; }
    public string? IsoMapping { get; init; }
    public string? NistMapping { get; init; }
    public string[] EvidenceTypes { get; init; } = Array.Empty<string>();
}

public record CrossFrameworkMappingDto
{
    public string? SourceFramework { get; init; }
    public string SourceFrameworkName { get; init; } = "";
    public int TotalControls { get; init; }
    public int ControlsWithIsoMapping { get; init; }
    public int ControlsWithNistMapping { get; init; }
    public decimal IsoMappingPercentage { get; init; }
    public decimal NistMappingPercentage { get; init; }
    public List<ControlMappingDto> Mappings { get; init; } = new();
    public DateTime LastUpdated { get; init; }
    public string Language { get; init; } = "en";
}

public record ControlMappingDto
{
    public string SourceControl { get; init; } = "";
    public string SourceTitle { get; init; } = "";
    public string[] Iso27001Controls { get; init; } = Array.Empty<string>();
    public string[] NistCsfControls { get; init; } = Array.Empty<string>();
    public string Domain { get; init; } = "";
    public string ControlType { get; init; } = "";
}

public record EvidenceCollectionStatusDto
{
    public EvidenceCollectionSummaryDto Summary { get; init; } = new();
    public List<ControlEvidenceStatusDto> ControlStatuses { get; init; } = new();
    public List<FrameworkEvidenceSummaryDto> ByFramework { get; init; } = new();
    public DateTime LastUpdated { get; init; }
    public string Language { get; init; } = "en";
}

public record EvidenceCollectionSummaryDto
{
    public int TotalControls { get; init; }
    public int ControlsWithEvidence { get; init; }
    public int ControlsWithoutEvidence { get; init; }
    public int TotalEvidences { get; init; }
    public int VerifiedEvidences { get; init; }
    public int PendingEvidences { get; init; }
    public decimal OverallCompletionPercentage { get; init; }
}

public record ControlEvidenceStatusDto
{
    public Guid ControlId { get; init; }
    public string? ControlCode { get; init; }
    public string? ControlName { get; init; }
    public string FrameworkCode { get; init; } = "";
    public int TotalEvidences { get; init; }
    public int VerifiedEvidences { get; init; }
    public int PendingEvidences { get; init; }
    public int RejectedEvidences { get; init; }
    public List<string> RequiredEvidenceTypes { get; init; } = new();
    public List<string?> CollectedEvidenceTypes { get; init; } = new();
    public decimal CompletionPercentage { get; init; }
    public string Status { get; init; } = "";
    public DateTime? LastUpdated { get; init; }
}

public record FrameworkEvidenceSummaryDto
{
    public string FrameworkCode { get; init; } = "";
    public string FrameworkName { get; init; } = "";
    public int TotalControls { get; init; }
    public int ControlsWithEvidence { get; init; }
    public decimal CompletionPercentage { get; init; }
}

public record ComplianceAlertsDto
{
    public int TotalAlerts { get; init; }
    public int CriticalAlerts { get; init; }
    public int HighAlerts { get; init; }
    public int MediumAlerts { get; init; }
    public List<ComplianceAlertDto> Alerts { get; init; } = new();
    public DateTime LastUpdated { get; init; }
    public string Language { get; init; } = "en";
}

public record ComplianceAlertDto
{
    public Guid Id { get; init; }
    public string AlertType { get; init; } = "";
    public string Severity { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string? FrameworkCode { get; init; }
    public string? ControlCode { get; init; }
    public string ActionUrl { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}

#endregion
