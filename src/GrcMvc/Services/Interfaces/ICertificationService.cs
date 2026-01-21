using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Certification Tracking Service Interface
/// Manages regulatory and compliance certifications lifecycle:
/// Obtain → Maintain → Renew → Track Audits → Monitor Expiry
/// </summary>
public interface ICertificationService
{
    #region Certification CRUD
    
    /// <summary>
    /// Create a new certification record
    /// </summary>
    Task<CertificationDto> CreateAsync(CreateCertificationRequest request);
    
    /// <summary>
    /// Get certification by ID
    /// </summary>
    Task<CertificationDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get certification with audit history
    /// </summary>
    Task<CertificationDetailDto?> GetDetailAsync(Guid id);
    
    /// <summary>
    /// Update certification
    /// </summary>
    Task<CertificationDto> UpdateAsync(Guid id, UpdateCertificationRequest request);
    
    /// <summary>
    /// Delete certification
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Get all certifications for tenant
    /// </summary>
    Task<List<CertificationDto>> GetAllAsync(Guid tenantId);
    
    /// <summary>
    /// Get certifications by status
    /// </summary>
    Task<List<CertificationDto>> GetByStatusAsync(Guid tenantId, string status);
    
    /// <summary>
    /// Get certifications by category
    /// </summary>
    Task<List<CertificationDto>> GetByCategoryAsync(Guid tenantId, string category);
    
    #endregion

    #region Lifecycle Management
    
    /// <summary>
    /// Start certification process (planning)
    /// </summary>
    Task<CertificationDto> StartCertificationAsync(Guid id, StartCertificationRequest request);
    
    /// <summary>
    /// Mark certification as obtained/issued
    /// </summary>
    Task<CertificationDto> MarkIssuedAsync(Guid id, MarkIssuedRequest request);
    
    /// <summary>
    /// Renew certification
    /// </summary>
    Task<CertificationDto> RenewAsync(Guid id, RenewCertificationRequest request);
    
    /// <summary>
    /// Suspend certification
    /// </summary>
    Task<CertificationDto> SuspendAsync(Guid id, string reason, string suspendedBy);
    
    /// <summary>
    /// Reinstate suspended certification
    /// </summary>
    Task<CertificationDto> ReinstateAsync(Guid id, string notes, string reinstatedBy);
    
    /// <summary>
    /// Mark certification as expired
    /// </summary>
    Task<CertificationDto> MarkExpiredAsync(Guid id);
    
    #endregion

    #region Audit Management
    
    /// <summary>
    /// Schedule an audit
    /// </summary>
    Task<CertificationAuditDto> ScheduleAuditAsync(Guid certificationId, ScheduleAuditRequest request);
    
    /// <summary>
    /// Record audit result
    /// </summary>
    Task<CertificationAuditDto> RecordAuditResultAsync(Guid auditId, RecordAuditResultRequest request);
    
    /// <summary>
    /// Get audit by ID
    /// </summary>
    Task<CertificationAuditDto?> GetAuditByIdAsync(Guid auditId);
    
    /// <summary>
    /// Get audit history for certification
    /// </summary>
    Task<List<CertificationAuditDto>> GetAuditHistoryAsync(Guid certificationId);
    
    /// <summary>
    /// Mark corrective actions complete
    /// </summary>
    Task<CertificationAuditDto> CompleteCorrectiveActionsAsync(Guid auditId, string notes, string completedBy);
    
    /// <summary>
    /// Get upcoming audits
    /// </summary>
    Task<List<CertificationAuditDto>> GetUpcomingAuditsAsync(Guid tenantId, int days = 90);
    
    #endregion

    #region Expiry & Renewal Tracking
    
    /// <summary>
    /// Get certifications expiring soon
    /// </summary>
    Task<List<CertificationDto>> GetExpiringSoonAsync(Guid tenantId, int days = 90);
    
    /// <summary>
    /// Get expired certifications
    /// </summary>
    Task<List<CertificationDto>> GetExpiredAsync(Guid tenantId);
    
    /// <summary>
    /// Get certifications requiring renewal action
    /// </summary>
    Task<List<CertificationRenewalDto>> GetRenewalActionsAsync(Guid tenantId);
    
    /// <summary>
    /// Update next surveillance date
    /// </summary>
    Task<CertificationDto> UpdateSurveillanceDateAsync(Guid id, DateTime nextDate);
    
    #endregion

    #region Reporting & Analytics
    
    /// <summary>
    /// Get certification dashboard
    /// </summary>
    Task<CertificationDashboardDto> GetDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get certification statistics
    /// </summary>
    Task<CertificationStatisticsDto> GetStatisticsAsync(Guid tenantId);
    
    /// <summary>
    /// Get certification compliance matrix
    /// </summary>
    Task<CertificationMatrixDto> GetComplianceMatrixAsync(Guid tenantId);
    
    /// <summary>
    /// Get certification cost summary
    /// </summary>
    Task<CertificationCostSummaryDto> GetCostSummaryAsync(Guid tenantId, int year);
    
    #endregion

    #region Ownership & Assignment
    
    /// <summary>
    /// Assign owner to certification
    /// </summary>
    Task<CertificationDto> AssignOwnerAsync(Guid id, string ownerId, string ownerName, string? department);
    
    /// <summary>
    /// Get certifications by owner
    /// </summary>
    Task<List<CertificationDto>> GetByOwnerAsync(string ownerId);
    
    /// <summary>
    /// Get certifications by department
    /// </summary>
    Task<List<CertificationDto>> GetByDepartmentAsync(Guid tenantId, string department);
    
    #endregion
    
    #region Readiness & Portfolio
    
    /// <summary>
    /// Get certification readiness assessment for tenant
    /// </summary>
    Task<TenantCertificationReadinessDto> GetReadinessAsync(Guid tenantId);
    
    /// <summary>
    /// Get preparation plan for specific certification
    /// </summary>
    Task<CertificationPreparationPlanDto> GetPreparationPlanAsync(Guid tenantId, Guid certificationId);
    
    /// <summary>
    /// Get default preparation plan template
    /// </summary>
    Task<CertificationPreparationPlanDto> GetDefaultPreparationPlanAsync(Guid tenantId);
    
    /// <summary>
    /// Get audits for specific certification
    /// </summary>
    Task<List<CertificationAuditDto>> GetAuditsForCertificationAsync(Guid tenantId, Guid certificationId);
    
    /// <summary>
    /// Get all certification audits for tenant
    /// </summary>
    Task<List<CertificationAuditDto>> GetAllAuditsAsync(Guid tenantId);
    
    /// <summary>
    /// Get certification portfolio view
    /// </summary>
    Task<CertificationPortfolioDto> GetPortfolioAsync(Guid tenantId);
    
    #endregion
}

#region DTOs

/// <summary>
/// Request to create certification
/// </summary>
public class CreateCertificationRequest
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Standard";
    public string Type { get; set; } = "Compliance";
    public string IssuingBody { get; set; } = string.Empty;
    public string? IssuingBodyAr { get; set; }
    public string? Scope { get; set; }
    public string? Level { get; set; }
    public string? StandardVersion { get; set; }
    public bool IsMandatory { get; set; }
    public string? MandatorySource { get; set; }
    public string? LinkedFrameworkCode { get; set; }
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? Department { get; set; }
}

/// <summary>
/// Request to update certification
/// </summary>
public class UpdateCertificationRequest
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? Scope { get; set; }
    public string? Level { get; set; }
    public string? StandardVersion { get; set; }
    public string? AuditorName { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public int? RenewalLeadDays { get; set; }
}

/// <summary>
/// Certification DTO
/// </summary>
public class CertificationDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string IssuingBody { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CertificationNumber { get; set; }
    public string? Scope { get; set; }
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? NextSurveillanceDate { get; set; }
    public DateTime? NextRecertificationDate { get; set; }
    public string? Level { get; set; }
    public string? StandardVersion { get; set; }
    public string? OwnerName { get; set; }
    public string? Department { get; set; }
    public bool IsMandatory { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsExpiringSoon { get; set; }
    public DateTime CreatedDate { get; set; }
    
    // Additional properties for view compatibility
    public string Standard => Name; // Standard is alias for Name
    public int? ReadinessScore { get; set; } = null; // Nullable readiness score
}

/// <summary>
/// Certification detail with audits
/// </summary>
public class CertificationDetailDto : CertificationDto
{
    public string? AuditorName { get; set; }
    public decimal? Cost { get; set; }
    public string CostCurrency { get; set; } = "SAR";
    public string? CertificateUrl { get; set; }
    public string? Notes { get; set; }
    public string? LinkedFrameworkCode { get; set; }
    public string? MandatorySource { get; set; }
    public int RenewalLeadDays { get; set; }
    public DateTime? LastRenewalDate { get; set; }
    public List<CertificationAuditDto> Audits { get; set; } = new();
}

/// <summary>
/// Certification audit DTO
/// </summary>
public class CertificationAuditDto
{
    public Guid Id { get; set; }
    public Guid CertificationId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; }
    public string? AuditorName { get; set; }
    public string? LeadAuditorName { get; set; }
    public string Result { get; set; } = string.Empty;
    public int MajorFindings { get; set; }
    public int MinorFindings { get; set; }
    public int Observations { get; set; }
    public DateTime? CorrectiveActionDeadline { get; set; }
    public bool CorrectiveActionsCompleted { get; set; }
    public string? ReportReference { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime? NextAuditDate { get; set; }
}

/// <summary>
/// Request to start certification
/// </summary>
public class StartCertificationRequest
{
    public DateTime TargetDate { get; set; }
    public string? AuditorName { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Request to mark certification issued
/// </summary>
public class MarkIssuedRequest
{
    public string CertificationNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? NextSurveillanceDate { get; set; }
    public string? CertificateUrl { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to renew certification
/// </summary>
public class RenewCertificationRequest
{
    public DateTime NewExpiryDate { get; set; }
    public DateTime? NewSurveillanceDate { get; set; }
    public string? NewCertificationNumber { get; set; }
    public decimal? RenewalCost { get; set; }
    public string RenewedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Request to schedule audit
/// </summary>
public class ScheduleAuditRequest
{
    public string AuditType { get; set; } = "Surveillance";
    public DateTime AuditDate { get; set; }
    public string? AuditorName { get; set; }
    public string? LeadAuditorName { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to record audit result
/// </summary>
public class RecordAuditResultRequest
{
    public string Result { get; set; } = "Pass";
    public int MajorFindings { get; set; }
    public int MinorFindings { get; set; }
    public int Observations { get; set; }
    public DateTime? CorrectiveActionDeadline { get; set; }
    public string? ReportReference { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime? NextAuditDate { get; set; }
    public string? Notes { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}

/// <summary>
/// Certification renewal action needed
/// </summary>
public class CertificationRenewalDto
{
    public Guid CertificationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string ActionRequired { get; set; } = string.Empty; // StartRenewal, ScheduleAudit, ReviewScope, etc.
    public string Priority { get; set; } = "Normal";
    public string? OwnerName { get; set; }
}

/// <summary>
/// Certification dashboard
/// </summary>
public class CertificationDashboardDto
{
    public Guid TenantId { get; set; }
    public CertificationStatisticsDto Statistics { get; set; } = new();
    public List<CertificationDto> ActiveCertifications { get; set; } = new();
    public List<CertificationDto> ExpiringSoon { get; set; } = new();
    public List<CertificationAuditDto> UpcomingAudits { get; set; } = new();
    public List<CertificationRenewalDto> RenewalActions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Certification statistics
/// </summary>
public class CertificationStatisticsDto
{
    public int TotalCertifications { get; set; }
    public int ActiveCertifications { get; set; }
    public int ExpiredCertifications { get; set; }
    public int InProgressCertifications { get; set; }
    public int PlannedCertifications { get; set; }
    public int ExpiringSoon { get; set; } // Within 90 days
    public int MandatoryCertifications { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByType { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
}

/// <summary>
/// Certification compliance matrix
/// </summary>
public class CertificationMatrixDto
{
    public Guid TenantId { get; set; }
    public List<CertificationMatrixRowDto> Rows { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Certification matrix row
/// </summary>
public class CertificationMatrixRowDto
{
    public string FrameworkCode { get; set; } = string.Empty;
    public string FrameworkName { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public string? CertificationCode { get; set; }
    public string CertificationStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Certified, Expired
    public DateTime? ExpiryDate { get; set; }
    public decimal ComplianceLevel { get; set; } // 0-100
}

/// <summary>
/// Certification cost summary
/// </summary>
public class CertificationCostSummaryDto
{
    public Guid TenantId { get; set; }
    public int Year { get; set; }
    public decimal TotalCertificationCost { get; set; }
    public decimal TotalAuditCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Currency { get; set; } = "SAR";
    public Dictionary<string, decimal> ByCertification { get; set; } = new();
    public Dictionary<string, decimal> ByCategory { get; set; } = new();
    public List<CostByMonthDto> ByMonth { get; set; } = new();
}

/// <summary>
/// Cost by month
/// </summary>
public record CostByMonthDto(int Month, decimal CertificationCost, decimal AuditCost);

/// <summary>
/// Tenant-level certification readiness assessment
/// </summary>
public class TenantCertificationReadinessDto
{
    public Guid TenantId { get; set; }
    public int OverallReadinessScore { get; set; }
    public string ReadinessLevel { get; set; } = "Low"; // Low, Medium, High, Ready
    public List<CertificationDto> UpcomingCertifications { get; set; } = new();
    public List<ReadinessGapDto> Gaps { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Readiness gap
/// </summary>
public class ReadinessGapDto
{
    public string Area { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string SuggestedAction { get; set; } = string.Empty;
}

/// <summary>
/// Certification preparation plan
/// </summary>
public class CertificationPreparationPlanDto
{
    public Guid TenantId { get; set; }
    public Guid? CertificationId { get; set; }
    public string CertificationName { get; set; } = string.Empty;
    public List<PreparationPhaseDto> Phases { get; set; } = new();
    public int TotalDuration { get; set; } // in days
    public DateTime? TargetDate { get; set; }
}

/// <summary>
/// Preparation phase
/// </summary>
public class PreparationPhaseDto
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int Duration { get; set; } // in days
    public List<string> Tasks { get; set; } = new();
    public string Status { get; set; } = "NotStarted";
}

/// <summary>
/// Certification portfolio view
/// </summary>
public class CertificationPortfolioDto
{
    public Guid TenantId { get; set; }
    public int TotalCertifications { get; set; }
    public int ActiveCertifications { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int ExpiredCount { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public List<CertificationDto> Certifications { get; set; } = new();
}

#endregion

