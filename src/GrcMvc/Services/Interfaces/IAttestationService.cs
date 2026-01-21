using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Attestation and Certification Service Interface
/// خدمة التصديق والشهادات
/// Handles board attestations, CEO/CISO sign-offs, and compliance certifications
/// </summary>
public interface IAttestationService
{
    /// <summary>
    /// Create a new attestation request
    /// </summary>
    Task<Attestation> CreateAttestationAsync(CreateAttestationRequest request);

    /// <summary>
    /// Get attestation by ID
    /// </summary>
    Task<Attestation?> GetAttestationAsync(Guid attestationId);

    /// <summary>
    /// Get pending attestations for a user
    /// </summary>
    Task<List<Attestation>> GetPendingAttestationsAsync(string userId);

    /// <summary>
    /// Sign an attestation (with digital signature)
    /// </summary>
    Task<AttestationSignature> SignAttestationAsync(Guid attestationId, string signerId, SignatureRequest signature);

    /// <summary>
    /// Get attestation history for an entity
    /// </summary>
    Task<List<Attestation>> GetAttestationHistoryAsync(Guid tenantId, string? entityType = null);

    /// <summary>
    /// Generate attestation certificate
    /// </summary>
    Task<AttestationCertificate> GenerateCertificateAsync(Guid attestationId);

    /// <summary>
    /// Verify attestation certificate
    /// </summary>
    Task<CertificateVerification> VerifyCertificateAsync(string certificateId);

    /// <summary>
    /// Get attestation requirements for a framework
    /// </summary>
    Task<List<AttestationRequirement>> GetRequirementsAsync(string frameworkCode);

    /// <summary>
    /// Schedule recurring attestation
    /// </summary>
    Task<RecurringAttestation> ScheduleRecurringAttestationAsync(RecurringAttestationRequest request);

    /// <summary>
    /// Get board attestation status
    /// </summary>
    Task<BoardAttestationStatus> GetBoardAttestationStatusAsync(Guid tenantId);
}

/// <summary>
/// Attestation entity
/// </summary>
public class Attestation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string AttestationType { get; set; } = string.Empty; // Board, CEO, CISO, Compliance, Annual
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending"; // Pending, In Progress, Signed, Expired, Rejected
    public string FrameworkCode { get; set; } = string.Empty;
    public string RegulatorCode { get; set; } = string.Empty;

    // Attestation statement
    public string StatementEn { get; set; } = string.Empty;
    public string StatementAr { get; set; } = string.Empty;

    // Related entity
    public string EntityType { get; set; } = string.Empty; // Assessment, Control, Policy, Report
    public Guid? EntityId { get; set; }

    // Timeline
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Signatories
    public List<AttestationSignatory> RequiredSignatories { get; set; } = new();
    public List<AttestationSignature> Signatures { get; set; } = new();

    // Audit trail
    public string CreatedBy { get; set; } = string.Empty;
    public List<AttestationAuditEntry> AuditTrail { get; set; } = new();

    // Certificate
    public string? CertificateId { get; set; }
    public bool HasCertificate => !string.IsNullOrEmpty(CertificateId);

    public bool IsFullySigned => Signatures.Count >= RequiredSignatories.Count &&
                                  RequiredSignatories.All(r => Signatures.Any(s => s.SignerId == r.UserId && s.Status == "Valid"));
}

/// <summary>
/// Required signatory
/// </summary>
public class AttestationSignatory
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // CEO, CISO, Board Member, Compliance Officer
    public string Title { get; set; } = string.Empty;
    public int SigningOrder { get; set; } // For sequential signing
    public bool IsMandatory { get; set; } = true;
}

/// <summary>
/// Attestation signature
/// </summary>
public class AttestationSignature
{
    public Guid Id { get; set; }
    public Guid AttestationId { get; set; }
    public string SignerId { get; set; } = string.Empty;
    public string SignerName { get; set; } = string.Empty;
    public string SignerRole { get; set; } = string.Empty;
    public string SignerTitle { get; set; } = string.Empty;

    public DateTime SignedAt { get; set; }
    public string SignatureType { get; set; } = string.Empty; // Digital, Handwritten, Electronic
    public string SignatureData { get; set; } = string.Empty; // Encrypted signature data
    public string SignatureHash { get; set; } = string.Empty;

    public string Status { get; set; } = "Valid"; // Valid, Revoked, Expired
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Optional: integration with Nafath/Absher
    public string? NafathTransactionId { get; set; }
    public bool IsNafathVerified { get; set; }
}

/// <summary>
/// Signature request
/// </summary>
public class SignatureRequest
{
    public string SignatureType { get; set; } = "Electronic"; // Digital, Handwritten, Electronic
    public string? SignatureData { get; set; } // Base64 encoded signature image or certificate
    public string? Pin { get; set; } // For digital signatures
    public bool UseNafath { get; set; } // Use Nafath for verification
    public string? NafathOtp { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Attestation audit entry
/// </summary>
public class AttestationAuditEntry
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

/// <summary>
/// Create attestation request
/// </summary>
public class CreateAttestationRequest
{
    public Guid TenantId { get; set; }
    public string AttestationType { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string StatementEn { get; set; } = string.Empty;
    public string StatementAr { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public string RegulatorCode { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public DateTime DueDate { get; set; }
    public int? ValidityDays { get; set; } // Days until expiry after signing
    public List<AttestationSignatory> RequiredSignatories { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Attestation certificate
/// </summary>
public class AttestationCertificate
{
    public string CertificateId { get; set; } = string.Empty;
    public Guid AttestationId { get; set; }
    public Guid TenantId { get; set; }

    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;

    public string CertificateType { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public string RegulatorCode { get; set; } = string.Empty;

    public DateTime IssuedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsValid => DateTime.UtcNow <= ExpiryDate;

    public List<CertificateSignatory> Signatories { get; set; } = new();

    public string QrCodeData { get; set; } = string.Empty; // For verification
    public string VerificationUrl { get; set; } = string.Empty;
    public string CertificateHash { get; set; } = string.Empty;

    public byte[]? PdfContent { get; set; }
}

/// <summary>
/// Certificate signatory info
/// </summary>
public class CertificateSignatory
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
}

/// <summary>
/// Certificate verification result
/// </summary>
public class CertificateVerification
{
    public string CertificateId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty; // Valid, Expired, Revoked, NotFound
    public string OrganizationName { get; set; } = string.Empty;
    public string CertificateType { get; set; } = string.Empty;
    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? RevokedReason { get; set; }
    public DateTime VerifiedAt { get; set; }
}

/// <summary>
/// Attestation requirement
/// </summary>
public class AttestationRequirement
{
    public string RequirementCode { get; set; } = string.Empty;
    public string FrameworkCode { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Annual, Quarterly, Per-Assessment
    public List<string> RequiredRoles { get; set; } = new(); // Roles that must sign
    public bool IsMandatory { get; set; }
}

/// <summary>
/// Recurring attestation configuration
/// </summary>
public class RecurringAttestation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string AttestationType { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Annual, Quarterly, Monthly
    public int DayOfMonth { get; set; }
    public int? Month { get; set; } // For annual
    public bool IsActive { get; set; }
    public DateTime? NextDueDate { get; set; }
    public DateTime? LastGeneratedDate { get; set; }
}

/// <summary>
/// Recurring attestation request
/// </summary>
public class RecurringAttestationRequest
{
    public Guid TenantId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public CreateAttestationRequest AttestationTemplate { get; set; } = new();
    public string Frequency { get; set; } = string.Empty;
    public int DayOfMonth { get; set; }
    public int? Month { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Board attestation status
/// </summary>
public class BoardAttestationStatus
{
    public Guid TenantId { get; set; }
    public int TotalRequired { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Overdue { get; set; }
    public DateTime? LastBoardAttestation { get; set; }
    public DateTime? NextDueDate { get; set; }
    public List<Attestation> PendingAttestations { get; set; } = new();
    public List<Attestation> RecentAttestations { get; set; } = new();
}
