using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Regulatory Integration Service Interface
/// Provides integration with KSA government regulatory portals:
/// - NCA-ISR (National Cybersecurity Authority - Information Security Registration)
/// - SAMA (Saudi Central Bank) e-Filing
/// - PDPL (Personal Data Protection Law) Compliance Portal
/// - CITC (Communications, Space and Technology Commission)
/// - Ministry of Commerce
/// </summary>
public interface IRegulatoryIntegrationService
{
    #region Portal Connection Management
    
    /// <summary>
    /// Register/configure a regulatory portal connection
    /// </summary>
    Task<RegulatoryPortalConnectionDto> RegisterConnectionAsync(RegisterPortalConnectionRequest request);
    
    /// <summary>
    /// Get all configured portal connections for tenant
    /// </summary>
    Task<List<RegulatoryPortalConnectionDto>> GetConnectionsAsync(Guid tenantId);
    
    /// <summary>
    /// Get connection by portal type
    /// </summary>
    Task<RegulatoryPortalConnectionDto?> GetConnectionAsync(Guid tenantId, string portalType);
    
    /// <summary>
    /// Test portal connection
    /// </summary>
    Task<ConnectionTestResultDto> TestConnectionAsync(Guid connectionId);
    
    /// <summary>
    /// Update portal credentials
    /// </summary>
    Task<RegulatoryPortalConnectionDto> UpdateCredentialsAsync(Guid connectionId, UpdateCredentialsRequest request);
    
    /// <summary>
    /// Deactivate portal connection
    /// </summary>
    Task DeactivateConnectionAsync(Guid connectionId, string reason);
    
    #endregion

    #region NCA-ISR Integration
    
    /// <summary>
    /// Submit NCA-ISR assessment report
    /// </summary>
    Task<SubmissionResultDto> SubmitNcaIsrReportAsync(Guid tenantId, NcaIsrReportRequest request);
    
    /// <summary>
    /// Get NCA-ISR submission status
    /// </summary>
    Task<SubmissionStatusDto> GetNcaIsrStatusAsync(Guid tenantId, string submissionId);
    
    /// <summary>
    /// Get NCA-ISR submission history
    /// </summary>
    Task<List<SubmissionHistoryDto>> GetNcaIsrHistoryAsync(Guid tenantId);
    
    /// <summary>
    /// Generate NCA-ISR compliance report (pre-submission)
    /// </summary>
    Task<NcaIsrComplianceReportDto> GenerateNcaIsrReportAsync(Guid tenantId, Guid assessmentId);
    
    /// <summary>
    /// Validate NCA-ISR data before submission
    /// </summary>
    Task<ValidationResultDto> ValidateNcaIsrDataAsync(Guid tenantId, NcaIsrReportRequest request);
    
    #endregion

    #region SAMA e-Filing Integration
    
    /// <summary>
    /// Submit SAMA compliance report
    /// </summary>
    Task<SubmissionResultDto> SubmitSamaReportAsync(Guid tenantId, SamaReportRequest request);
    
    /// <summary>
    /// Get SAMA submission status
    /// </summary>
    Task<SubmissionStatusDto> GetSamaStatusAsync(Guid tenantId, string submissionId);
    
    /// <summary>
    /// Get SAMA submission history
    /// </summary>
    Task<List<SubmissionHistoryDto>> GetSamaHistoryAsync(Guid tenantId);
    
    /// <summary>
    /// Submit SAMA incident notification
    /// </summary>
    Task<SubmissionResultDto> SubmitSamaIncidentAsync(Guid tenantId, SamaIncidentNotificationRequest request);
    
    /// <summary>
    /// Get SAMA reporting deadlines
    /// </summary>
    Task<List<RegulatoryPortalDeadlineDto>> GetSamaDeadlinesAsync(Guid tenantId);
    
    #endregion

    #region PDPL Compliance Integration
    
    /// <summary>
    /// Submit PDPL breach notification
    /// </summary>
    Task<SubmissionResultDto> SubmitPdplBreachNotificationAsync(Guid tenantId, PdplBreachNotificationRequest request);
    
    /// <summary>
    /// Submit PDPL data processing registration
    /// </summary>
    Task<SubmissionResultDto> SubmitPdplRegistrationAsync(Guid tenantId, PdplRegistrationRequest request);
    
    /// <summary>
    /// Get PDPL notification status
    /// </summary>
    Task<SubmissionStatusDto> GetPdplStatusAsync(Guid tenantId, string submissionId);
    
    /// <summary>
    /// Get PDPL submission history
    /// </summary>
    Task<List<SubmissionHistoryDto>> GetPdplHistoryAsync(Guid tenantId);
    
    /// <summary>
    /// Calculate PDPL notification deadline (72 hours from detection)
    /// </summary>
    Task<DateTime> CalculatePdplDeadlineAsync(DateTime detectionTime);
    
    #endregion

    #region General Regulatory Filing
    
    /// <summary>
    /// Get all pending regulatory submissions
    /// </summary>
    Task<List<PendingSubmissionDto>> GetPendingSubmissionsAsync(Guid tenantId);
    
    /// <summary>
    /// Get upcoming regulatory deadlines
    /// </summary>
    Task<List<RegulatoryPortalDeadlineDto>> GetUpcomingDeadlinesAsync(Guid tenantId, int days = 90);
    
    /// <summary>
    /// Get all submission history across portals
    /// </summary>
    Task<List<SubmissionHistoryDto>> GetAllSubmissionHistoryAsync(Guid tenantId);
    
    /// <summary>
    /// Get regulatory compliance status
    /// </summary>
    Task<RegulatoryComplianceStatusDto> GetComplianceStatusAsync(Guid tenantId);
    
    /// <summary>
    /// Generate regulatory calendar
    /// </summary>
    Task<RegulatoryCalendarDto> GenerateRegulatoryCalendarAsync(Guid tenantId, int year);
    
    #endregion

    #region Document & Evidence Integration
    
    /// <summary>
    /// Upload supporting document for submission
    /// </summary>
    Task<DocumentUploadResultDto> UploadDocumentAsync(Guid submissionId, UploadDocumentRequest request);
    
    /// <summary>
    /// Get documents for submission
    /// </summary>
    Task<List<SubmissionDocumentDto>> GetDocumentsAsync(Guid submissionId);
    
    /// <summary>
    /// Link evidence to regulatory submission
    /// </summary>
    Task LinkEvidenceAsync(Guid submissionId, List<Guid> evidenceIds);
    
    #endregion

    #region Audit & Reporting
    
    /// <summary>
    /// Get integration audit log
    /// </summary>
    Task<List<IntegrationAuditLogDto>> GetAuditLogAsync(Guid tenantId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get integration dashboard
    /// </summary>
    Task<RegulatoryIntegrationDashboardDto> GetDashboardAsync(Guid tenantId);
    
    /// <summary>
    /// Get integration statistics
    /// </summary>
    Task<IntegrationStatisticsDto> GetStatisticsAsync(Guid tenantId);
    
    #endregion
}

#region Portal Connection DTOs

/// <summary>
/// Regulatory portal types supported
/// </summary>
public static class RegulatoryPortalTypes
{
    public const string NcaIsr = "NCA-ISR";
    public const string SamaEfiling = "SAMA-EFILING";
    public const string Pdpl = "PDPL";
    public const string Citc = "CITC";
    public const string Moc = "MOC"; // Ministry of Commerce
}

/// <summary>
/// Request to register portal connection
/// </summary>
public class RegisterPortalConnectionRequest
{
    public Guid TenantId { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string PortalName { get; set; } = string.Empty;
    public string? PortalNameAr { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string AuthType { get; set; } = "ApiKey"; // ApiKey, OAuth2, Certificate, BasicAuth
    public string? ApiKey { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? CertificateThumbprint { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? OrganizationId { get; set; } // Organization ID in the portal
    public Dictionary<string, string>? AdditionalSettings { get; set; }
}

/// <summary>
/// Request to update credentials
/// </summary>
public class UpdateCredentialsRequest
{
    public string? ApiKey { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Portal connection DTO
/// </summary>
public class RegulatoryPortalConnectionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string PortalName { get; set; } = string.Empty;
    public string? PortalNameAr { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string AuthType { get; set; } = string.Empty;
    public string? OrganizationId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastConnectedAt { get; set; }
    public string? LastConnectionStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Connection test result
/// </summary>
public class ConnectionTestResultDto
{
    public Guid ConnectionId { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public int? ResponseTimeMs { get; set; }
    public DateTime TestedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Submission DTOs

/// <summary>
/// Submission result
/// </summary>
public class SubmissionResultDto
{
    public Guid InternalId { get; set; }
    public string? ExternalSubmissionId { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty; // Submitted, Accepted, Rejected, Pending, Failed
    public string? ConfirmationNumber { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }
}

/// <summary>
/// Submission status
/// </summary>
public class SubmissionStatusDto
{
    public string SubmissionId { get; set; } = string.Empty;
    public string PortalType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? StatusDescription { get; set; }
    public DateTime? LastUpdated { get; set; }
    public DateTime? NextActionDeadline { get; set; }
    public List<string>? Comments { get; set; }
}

/// <summary>
/// Submission history
/// </summary>
public class SubmissionHistoryDto
{
    public Guid InternalId { get; set; }
    public string? ExternalSubmissionId { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string SubmissionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? SubmittedByName { get; set; }
    public string? ConfirmationNumber { get; set; }
    public DateTime? ReportingPeriodStart { get; set; }
    public DateTime? ReportingPeriodEnd { get; set; }
}

/// <summary>
/// Pending submission
/// </summary>
public class PendingSubmissionDto
{
    public Guid Id { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string SubmissionType { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public int DaysUntilDeadline { get; set; }
    public string Priority { get; set; } = "Normal";
    public string? AssignedTo { get; set; }
    public string Status { get; set; } = "NotStarted";
}

#endregion

#region NCA-ISR DTOs

/// <summary>
/// NCA-ISR report submission request
/// </summary>
public class NcaIsrReportRequest
{
    public Guid AssessmentId { get; set; }
    public DateTime ReportingPeriodStart { get; set; }
    public DateTime ReportingPeriodEnd { get; set; }
    public string ReportType { get; set; } = "Annual"; // Annual, Quarterly, AdHoc
    public string? Notes { get; set; }
    public string SubmittedById { get; set; } = string.Empty;
    public string SubmittedByName { get; set; } = string.Empty;
    public List<Guid>? EvidenceIds { get; set; }
}

/// <summary>
/// NCA-ISR compliance report (generated)
/// </summary>
public class NcaIsrComplianceReportDto
{
    public Guid AssessmentId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public DateTime ReportingPeriodStart { get; set; }
    public DateTime ReportingPeriodEnd { get; set; }
    public decimal OverallComplianceScore { get; set; }
    public Dictionary<string, decimal> DomainScores { get; set; } = new();
    public int TotalControls { get; set; }
    public int ImplementedControls { get; set; }
    public int PartialControls { get; set; }
    public int NotImplementedControls { get; set; }
    public List<NcaFindingDto> Findings { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// NCA finding
/// </summary>
public class NcaFindingDto
{
    public string ControlId { get; set; } = string.Empty;
    public string ControlName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Gap { get; set; }
    public string? RemediationPlan { get; set; }
    public DateTime? TargetDate { get; set; }
}

#endregion

#region SAMA DTOs

/// <summary>
/// SAMA report submission request
/// </summary>
public class SamaReportRequest
{
    public string ReportType { get; set; } = "CyberSecurityAssessment"; // CyberSecurityAssessment, BcpDrTest, RiskAssessment
    public Guid? AssessmentId { get; set; }
    public DateTime ReportingPeriodStart { get; set; }
    public DateTime ReportingPeriodEnd { get; set; }
    public string? Notes { get; set; }
    public string SubmittedById { get; set; } = string.Empty;
    public string SubmittedByName { get; set; } = string.Empty;
    public List<Guid>? EvidenceIds { get; set; }
}

/// <summary>
/// SAMA incident notification request
/// </summary>
public class SamaIncidentNotificationRequest
{
    public Guid? IncidentId { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImpactDescription { get; set; } = string.Empty;
    public int? AffectedCustomers { get; set; }
    public string? ImmediateActions { get; set; }
    public string SubmittedById { get; set; } = string.Empty;
    public string SubmittedByName { get; set; } = string.Empty;
}

#endregion

#region PDPL DTOs

/// <summary>
/// PDPL breach notification request
/// </summary>
public class PdplBreachNotificationRequest
{
    public Guid? IncidentId { get; set; }
    public DateTime DetectedAt { get; set; }
    public string BreachType { get; set; } = string.Empty; // Unauthorized Access, Data Loss, Disclosure, etc.
    public string DataCategories { get; set; } = string.Empty; // Personal, Sensitive, Financial, etc.
    public int? AffectedIndividuals { get; set; }
    public string? Description { get; set; }
    public string? ImpactAssessment { get; set; }
    public string? MitigationMeasures { get; set; }
    public bool NotifyDataSubjects { get; set; }
    public string SubmittedById { get; set; } = string.Empty;
    public string SubmittedByName { get; set; } = string.Empty;
}

/// <summary>
/// PDPL data processing registration request
/// </summary>
public class PdplRegistrationRequest
{
    public string ProcessingPurpose { get; set; } = string.Empty;
    public string DataCategories { get; set; } = string.Empty;
    public string DataSubjectCategories { get; set; } = string.Empty;
    public string? ThirdPartyTransfers { get; set; }
    public string? CrossBorderTransfers { get; set; }
    public string RetentionPeriod { get; set; } = string.Empty;
    public string SecurityMeasures { get; set; } = string.Empty;
    public string DpoContactInfo { get; set; } = string.Empty;
    public string SubmittedById { get; set; } = string.Empty;
    public string SubmittedByName { get; set; } = string.Empty;
}

#endregion

#region General DTOs

/// <summary>
/// Regulatory portal deadline
/// </summary>
public class RegulatoryPortalDeadlineDto
{
    public string PortalType { get; set; } = string.Empty;
    public string DeadlineType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public DateTime Deadline { get; set; }
    public int DaysUntilDeadline { get; set; }
    public string Priority { get; set; } = "Normal";
    public bool IsMandatory { get; set; }
    public string? AssignedTo { get; set; }
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Validation error
/// </summary>
public class ValidationErrorDto
{
    public string Field { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Regulatory compliance status
/// </summary>
public class RegulatoryComplianceStatusDto
{
    public Guid TenantId { get; set; }
    public Dictionary<string, PortalComplianceStatusDto> PortalStatuses { get; set; } = new();
    public int TotalOverdueSubmissions { get; set; }
    public int TotalPendingSubmissions { get; set; }
    public DateTime NextDeadline { get; set; }
    public string? NextDeadlineType { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Portal compliance status
/// </summary>
public class PortalComplianceStatusDto
{
    public string PortalType { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public string? LastSubmissionStatus { get; set; }
    public DateTime? LastSubmissionDate { get; set; }
    public DateTime? NextDeadline { get; set; }
    public int PendingSubmissions { get; set; }
    public int OverdueSubmissions { get; set; }
}

/// <summary>
/// Regulatory calendar
/// </summary>
public class RegulatoryCalendarDto
{
    public Guid TenantId { get; set; }
    public int Year { get; set; }
    public List<RegulatoryCalendarEventDto> Events { get; set; } = new();
}

/// <summary>
/// Regulatory calendar event
/// </summary>
public class RegulatoryCalendarEventDto
{
    public string PortalType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? TitleAr { get; set; }
    public DateTime Date { get; set; }
    public bool IsMandatory { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Document upload request
/// </summary>
public class UploadDocumentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
}

/// <summary>
/// Document upload result
/// </summary>
public class DocumentUploadResultDto
{
    public Guid DocumentId { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ExternalDocumentId { get; set; }
}

/// <summary>
/// Submission document
/// </summary>
public class SubmissionDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
}

/// <summary>
/// Integration audit log entry
/// </summary>
public class IntegrationAuditLogDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string PortalType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? SubmissionId { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorDetails { get; set; }
    public string PerformedByName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Regulatory integration dashboard
/// </summary>
public class RegulatoryIntegrationDashboardDto
{
    public Guid TenantId { get; set; }
    public List<RegulatoryPortalConnectionDto> Connections { get; set; } = new();
    public List<PendingSubmissionDto> PendingSubmissions { get; set; } = new();
    public List<RegulatoryPortalDeadlineDto> UpcomingDeadlines { get; set; } = new();
    public List<SubmissionHistoryDto> RecentSubmissions { get; set; } = new();
    public IntegrationStatisticsDto Statistics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Integration statistics
/// </summary>
public class IntegrationStatisticsDto
{
    public int TotalSubmissions { get; set; }
    public int SuccessfulSubmissions { get; set; }
    public int FailedSubmissions { get; set; }
    public int PendingSubmissions { get; set; }
    public Dictionary<string, int> ByPortal { get; set; } = new();
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public decimal SuccessRate { get; set; }
}

#endregion
