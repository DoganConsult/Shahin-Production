using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Attestation and Certification Service Implementation
/// خدمة التصديق والشهادات
/// </summary>
public class AttestationService : IAttestationService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<AttestationService> _logger;
    private readonly INotificationService _notificationService;

    // In-memory storage (in production, use database)
    private static readonly List<Attestation> _attestations = new();
    private static readonly List<AttestationCertificate> _certificates = new();
    private static readonly List<RecurringAttestation> _recurringAttestations = new();

    public AttestationService(
        GrcDbContext context,
        ILogger<AttestationService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<Attestation> CreateAttestationAsync(CreateAttestationRequest request)
    {
        _logger.LogInformation("Creating attestation for tenant {TenantId}: {Title}", request.TenantId, request.TitleEn);

        var attestation = new Attestation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            AttestationType = request.AttestationType,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            StatementEn = request.StatementEn,
            StatementAr = request.StatementAr,
            FrameworkCode = request.FrameworkCode,
            RegulatorCode = request.RegulatorCode,
            EntityType = request.EntityType ?? string.Empty,
            EntityId = request.EntityId,
            Status = "Pending",
            CreatedDate = DateTime.UtcNow,
            DueDate = request.DueDate,
            ExpiryDate = request.ValidityDays.HasValue ? request.DueDate.AddDays(request.ValidityDays.Value) : null,
            RequiredSignatories = request.RequiredSignatories,
            CreatedBy = request.CreatedBy,
            AuditTrail = new List<AttestationAuditEntry>
            {
                new() { Timestamp = DateTime.UtcNow, Action = "Created", PerformedBy = request.CreatedBy, Details = "Attestation created" }
            }
        };

        _attestations.Add(attestation);

        // Notify signatories
        foreach (var signatory in request.RequiredSignatories)
        {
            await _notificationService.SendNotificationAsync(
                workflowInstanceId: Guid.Empty,
                recipientUserId: signatory.UserId,
                notificationType: "AttestationRequired",
                subject: $"Attestation Required: {request.TitleEn}",
                body: $"You are required to sign the attestation: {request.TitleEn}. Due date: {request.DueDate:yyyy-MM-dd}",
                priority: "High",
                tenantId: request.TenantId);
        }

        _logger.LogInformation("Attestation {AttestationId} created successfully", attestation.Id);
        return await Task.FromResult(attestation);
    }

    public async Task<Attestation?> GetAttestationAsync(Guid attestationId)
    {
        return await Task.FromResult(_attestations.FirstOrDefault(a => a.Id == attestationId));
    }

    public async Task<List<Attestation>> GetPendingAttestationsAsync(string userId)
    {
        var pending = _attestations
            .Where(a => a.Status != "Completed" && a.Status != "Rejected" &&
                        a.RequiredSignatories.Any(s => s.UserId == userId) &&
                        !a.Signatures.Any(s => s.SignerId == userId))
            .OrderBy(a => a.DueDate)
            .ToList();

        return await Task.FromResult(pending);
    }

    public async Task<AttestationSignature> SignAttestationAsync(Guid attestationId, string signerId, SignatureRequest signature)
    {
        _logger.LogInformation("Signing attestation {AttestationId} by {SignerId}", attestationId, signerId);

        var attestation = _attestations.FirstOrDefault(a => a.Id == attestationId);
        if (attestation == null)
            throw new EntityNotFoundException("Attestation", attestationId);

        var signatory = attestation.RequiredSignatories.FirstOrDefault(s => s.UserId == signerId);
        if (signatory == null)
            throw new AuthorizationException($"User {signerId} is not an authorized signatory");

        // Check signing order
        if (signatory.SigningOrder > 1)
        {
            var previousSignatories = attestation.RequiredSignatories.Where(s => s.SigningOrder < signatory.SigningOrder);
            foreach (var prev in previousSignatories)
            {
                if (!attestation.Signatures.Any(s => s.SignerId == prev.UserId && s.Status == "Valid"))
                    throw new ValidationException("SigningOrder", $"Previous signatories must sign first (Signing Order: {signatory.SigningOrder})");
            }
        }

        // Create signature
        var signatureHash = ComputeSignatureHash(attestation.Id, signerId, DateTime.UtcNow);

        var attestationSignature = new AttestationSignature
        {
            Id = Guid.NewGuid(),
            AttestationId = attestationId,
            SignerId = signerId,
            SignerName = signatory.UserName,
            SignerRole = signatory.Role,
            SignerTitle = signatory.Title,
            SignedAt = DateTime.UtcNow,
            SignatureType = signature.SignatureType,
            SignatureData = signature.SignatureData ?? string.Empty,
            SignatureHash = signatureHash,
            Status = "Valid",
            IsNafathVerified = signature.UseNafath
        };

        attestation.Signatures.Add(attestationSignature);
        attestation.AuditTrail.Add(new AttestationAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "Signed",
            PerformedBy = signerId,
            Details = $"Signed by {signatory.UserName} ({signatory.Role})"
        });

        // Check if fully signed
        if (attestation.IsFullySigned)
        {
            attestation.Status = "Signed";
            attestation.SignedDate = DateTime.UtcNow;
            attestation.AuditTrail.Add(new AttestationAuditEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = "Completed",
                PerformedBy = "SYSTEM",
                Details = "All required signatures collected"
            });

            _logger.LogInformation("Attestation {AttestationId} fully signed", attestationId);
        }
        else
        {
            attestation.Status = "In Progress";
        }

        return await Task.FromResult(attestationSignature);
    }

    public async Task<List<Attestation>> GetAttestationHistoryAsync(Guid tenantId, string? entityType = null)
    {
        var history = _attestations
            .Where(a => a.TenantId == tenantId)
            .Where(a => string.IsNullOrEmpty(entityType) || a.EntityType == entityType)
            .OrderByDescending(a => a.CreatedDate)
            .ToList();

        return await Task.FromResult(history);
    }

    public async Task<AttestationCertificate> GenerateCertificateAsync(Guid attestationId)
    {
        _logger.LogInformation("Generating certificate for attestation {AttestationId}", attestationId);

        var attestation = _attestations.FirstOrDefault(a => a.Id == attestationId);
        if (attestation == null)
            throw new EntityNotFoundException("Attestation", attestationId);

        if (!attestation.IsFullySigned)
            throw new ValidationException("Attestation", "Cannot generate certificate for unsigned attestation");

        var certificateId = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var verificationUrl = $"https://verify.shahin-ai.com/cert/{certificateId}";

        var certificate = new AttestationCertificate
        {
            CertificateId = certificateId,
            AttestationId = attestationId,
            TenantId = attestation.TenantId,
            TitleEn = attestation.TitleEn,
            TitleAr = attestation.TitleAr,
            CertificateType = attestation.AttestationType,
            FrameworkCode = attestation.FrameworkCode,
            RegulatorCode = attestation.RegulatorCode,
            IssuedDate = DateTime.UtcNow,
            ExpiryDate = attestation.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
            Signatories = attestation.Signatures.Select(s => new CertificateSignatory
            {
                Name = s.SignerName,
                Role = s.SignerRole,
                Title = s.SignerTitle,
                SignedAt = s.SignedAt
            }).ToList(),
            QrCodeData = $"{verificationUrl}",
            VerificationUrl = verificationUrl,
            CertificateHash = ComputeSignatureHash(attestationId, certificateId, DateTime.UtcNow)
        };

        // Generate PDF content (simplified)
        certificate.PdfContent = GenerateCertificatePdf(certificate, attestation);

        _certificates.Add(certificate);
        attestation.CertificateId = certificateId;

        attestation.AuditTrail.Add(new AttestationAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "CertificateGenerated",
            PerformedBy = "SYSTEM",
            Details = $"Certificate {certificateId} generated"
        });

        _logger.LogInformation("Certificate {CertificateId} generated for attestation {AttestationId}", certificateId, attestationId);
        return await Task.FromResult(certificate);
    }

    public async Task<CertificateVerification> VerifyCertificateAsync(string certificateId)
    {
        _logger.LogInformation("Verifying certificate {CertificateId}", certificateId);

        var certificate = _certificates.FirstOrDefault(c => c.CertificateId == certificateId);

        if (certificate == null)
        {
            return new CertificateVerification
            {
                CertificateId = certificateId,
                IsValid = false,
                Status = "NotFound",
                VerifiedAt = DateTime.UtcNow
            };
        }

        var isExpired = DateTime.UtcNow > certificate.ExpiryDate;

        return await Task.FromResult(new CertificateVerification
        {
            CertificateId = certificateId,
            IsValid = !isExpired,
            Status = isExpired ? "Expired" : "Valid",
            OrganizationName = certificate.TitleEn,
            CertificateType = certificate.CertificateType,
            IssuedDate = certificate.IssuedDate,
            ExpiryDate = certificate.ExpiryDate,
            VerifiedAt = DateTime.UtcNow
        });
    }

    public async Task<List<AttestationRequirement>> GetRequirementsAsync(string frameworkCode)
    {
        var requirements = new List<AttestationRequirement>
        {
            new() { RequirementCode = $"{frameworkCode}-ATT-001", FrameworkCode = frameworkCode,
                    TitleEn = "Annual Board Attestation", TitleAr = "التصديق السنوي لمجلس الإدارة",
                    DescriptionEn = "Annual attestation by board of directors on compliance status",
                    DescriptionAr = "تصديق سنوي من مجلس الإدارة على حالة الامتثال",
                    Frequency = "Annual", RequiredRoles = new() { "BoardMember", "CEO" }, IsMandatory = true },

            new() { RequirementCode = $"{frameworkCode}-ATT-002", FrameworkCode = frameworkCode,
                    TitleEn = "CISO Cybersecurity Attestation", TitleAr = "تصديق مسؤول أمن المعلومات",
                    DescriptionEn = "Quarterly attestation by CISO on cybersecurity posture",
                    DescriptionAr = "تصديق ربع سنوي من مسؤول أمن المعلومات على الوضع الأمني",
                    Frequency = "Quarterly", RequiredRoles = new() { "CISO" }, IsMandatory = true },

            new() { RequirementCode = $"{frameworkCode}-ATT-003", FrameworkCode = frameworkCode,
                    TitleEn = "Assessment Completion Attestation", TitleAr = "تصديق إكمال التقييم",
                    DescriptionEn = "Attestation upon completion of each assessment",
                    DescriptionAr = "تصديق عند إكمال كل تقييم",
                    Frequency = "Per-Assessment", RequiredRoles = new() { "ComplianceOfficer", "Assessor" }, IsMandatory = true }
        };

        return await Task.FromResult(requirements.Where(r => r.FrameworkCode == frameworkCode).ToList());
    }

    public async Task<RecurringAttestation> ScheduleRecurringAttestationAsync(RecurringAttestationRequest request)
    {
        _logger.LogInformation("Scheduling recurring attestation for tenant {TenantId}", request.TenantId);

        var recurring = new RecurringAttestation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            TemplateName = request.TemplateName,
            AttestationType = request.AttestationTemplate.AttestationType,
            Frequency = request.Frequency,
            DayOfMonth = request.DayOfMonth,
            Month = request.Month,
            IsActive = true,
            NextDueDate = CalculateNextDueDate(request.Frequency, request.DayOfMonth, request.Month)
        };

        _recurringAttestations.Add(recurring);

        _logger.LogInformation("Recurring attestation {Id} scheduled, next due: {NextDue}", recurring.Id, recurring.NextDueDate);
        return await Task.FromResult(recurring);
    }

    public async Task<BoardAttestationStatus> GetBoardAttestationStatusAsync(Guid tenantId)
    {
        var tenantAttestations = _attestations.Where(a => a.TenantId == tenantId).ToList();
        var boardAttestations = tenantAttestations.Where(a => a.AttestationType == "Board").ToList();

        return await Task.FromResult(new BoardAttestationStatus
        {
            TenantId = tenantId,
            TotalRequired = 4, // Typical annual requirement
            Completed = boardAttestations.Count(a => a.Status == "Signed"),
            Pending = boardAttestations.Count(a => a.Status == "Pending" || a.Status == "In Progress"),
            Overdue = boardAttestations.Count(a => a.DueDate < DateTime.UtcNow && a.Status != "Signed"),
            LastBoardAttestation = boardAttestations.Where(a => a.SignedDate.HasValue).Max(a => a.SignedDate),
            NextDueDate = boardAttestations.Where(a => a.Status != "Signed").Min(a => a.DueDate),
            PendingAttestations = boardAttestations.Where(a => a.Status == "Pending").Take(5).ToList(),
            RecentAttestations = boardAttestations.Where(a => a.Status == "Signed").OrderByDescending(a => a.SignedDate).Take(5).ToList()
        });
    }

    private static string ComputeSignatureHash(Guid attestationId, string signerId, DateTime timestamp)
    {
        var input = $"{attestationId}:{signerId}:{timestamp:O}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private static byte[] GenerateCertificatePdf(AttestationCertificate certificate, Attestation attestation)
    {
        var html = $@"<!DOCTYPE html>
<html dir='rtl'>
<head><meta charset='UTF-8'><title>شهادة امتثال</title>
<style>
body{{font-family:'Segoe UI',sans-serif;padding:40px;text-align:center;}}
.certificate{{border:3px solid #1a365d;padding:40px;max-width:800px;margin:auto;}}
h1{{color:#1a365d;}}
.details{{text-align:right;margin:20px 0;}}
.signatories{{margin-top:30px;}}
</style>
</head>
<body>
<div class='certificate'>
<h1>شهادة امتثال</h1>
<h2>{certificate.TitleAr}</h2>
<p><strong>رقم الشهادة:</strong> {certificate.CertificateId}</p>
<p><strong>تاريخ الإصدار:</strong> {certificate.IssuedDate:yyyy-MM-dd}</p>
<p><strong>صالحة حتى:</strong> {certificate.ExpiryDate:yyyy-MM-dd}</p>
<div class='signatories'>
<h3>الموقعون</h3>
{string.Join("", certificate.Signatories.Select(s => $"<p>{s.Name} - {s.Role}</p>"))}
</div>
<p><small>للتحقق: {certificate.VerificationUrl}</small></p>
</div>
</body>
</html>";

        return Encoding.UTF8.GetBytes(html);
    }

    private static DateTime CalculateNextDueDate(string frequency, int dayOfMonth, int? month)
    {
        var now = DateTime.UtcNow;
        return frequency switch
        {
            "Annual" => new DateTime(now.Year + (now.Month > month ? 1 : 0), month ?? 1, dayOfMonth),
            "Quarterly" => GetNextQuarterDate(dayOfMonth),
            "Monthly" => new DateTime(now.Year, now.Month, dayOfMonth) < now
                ? new DateTime(now.Year, now.Month, dayOfMonth).AddMonths(1)
                : new DateTime(now.Year, now.Month, dayOfMonth),
            _ => now.AddMonths(1)
        };
    }

    private static DateTime GetNextQuarterDate(int dayOfMonth)
    {
        var now = DateTime.UtcNow;
        var quarterMonths = new[] { 3, 6, 9, 12 };
        foreach (var m in quarterMonths)
        {
            var date = new DateTime(now.Year, m, Math.Min(dayOfMonth, DateTime.DaysInMonth(now.Year, m)));
            if (date > now) return date;
        }
        return new DateTime(now.Year + 1, 3, dayOfMonth);
    }
}
