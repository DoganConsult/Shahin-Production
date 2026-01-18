using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service for managing user consent and legal documents
/// </summary>
public class ConsentService : IConsentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConsentService> _logger;

    public ConsentService(IUnitOfWork unitOfWork, ILogger<ConsentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LegalDocument?> GetActiveDocumentAsync(string documentType)
    {
        return await _unitOfWork.LegalDocuments
            .Query()
            .Where(d => d.DocumentType == documentType && d.IsActive && !d.IsDeleted)
            .OrderByDescending(d => d.EffectiveDate)
            .FirstOrDefaultAsync();
    }

    public async Task<UserConsent> RecordConsentAsync(
        Guid tenantId,
        Guid userId,
        string consentType,
        string documentVersion,
        bool isGranted,
        string? ipAddress = null,
        string? userAgent = null)
    {
        // Check if consent already exists
        var existingConsent = await _unitOfWork.UserConsents
            .Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ConsentType == consentType && !c.IsDeleted);

        if (existingConsent != null)
        {
            // Update existing consent
            existingConsent.IsGranted = isGranted;
            existingConsent.DocumentVersion = documentVersion;
            existingConsent.ConsentedAt = DateTime.UtcNow;
            existingConsent.IpAddress = ipAddress;
            existingConsent.UserAgent = userAgent;
            existingConsent.WithdrawnAt = null;
            existingConsent.WithdrawalReason = null;
            existingConsent.ModifiedDate = DateTime.UtcNow;
            existingConsent.ModifiedBy = userId.ToString();

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Updated consent {ConsentType} for user {UserId}", consentType, userId);
            return existingConsent;
        }

        // Get document hash
        var document = await GetActiveDocumentAsync(consentType);
        var documentHash = document != null ? ComputeHash(document.ContentEn) : null;

        var consent = new UserConsent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            ConsentType = consentType,
            DocumentVersion = documentVersion,
            IsGranted = isGranted,
            ConsentedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DocumentHash = documentHash,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = userId.ToString()
        };

        await _unitOfWork.UserConsents.AddAsync(consent);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Recorded consent {ConsentType} for user {UserId}", consentType, userId);
        return consent;
    }

    public async Task<bool> HasConsentAsync(Guid userId, string consentType)
    {
        return await _unitOfWork.UserConsents
            .Query()
            .AnyAsync(c => c.UserId == userId &&
                          c.ConsentType == consentType &&
                          c.IsGranted &&
                          c.WithdrawnAt == null &&
                          !c.IsDeleted);
    }

    public async Task<IEnumerable<UserConsent>> GetUserConsentsAsync(Guid userId)
    {
        return await _unitOfWork.UserConsents
            .Query()
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.ConsentedAt)
            .ToListAsync();
    }

    public async Task<UserConsent> WithdrawConsentAsync(Guid userId, string consentType, string reason)
    {
        var consent = await _unitOfWork.UserConsents
            .Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ConsentType == consentType && !c.IsDeleted);

        if (consent == null)
            throw new EntityNotFoundException("Consent", $"{consentType}:user:{userId}");

        consent.IsGranted = false;
        consent.WithdrawnAt = DateTime.UtcNow;
        consent.WithdrawalReason = reason;
        consent.ModifiedDate = DateTime.UtcNow;
        consent.ModifiedBy = userId.ToString();

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Withdrew consent {ConsentType} for user {UserId}", consentType, userId);
        return consent;
    }

    public async Task<bool> HasAllMandatoryConsentsAsync(Guid userId)
    {
        // Get all mandatory document types
        var mandatoryTypes = await _unitOfWork.LegalDocuments
            .Query()
            .Where(d => d.IsMandatory && d.IsActive && !d.IsDeleted)
            .Select(d => d.DocumentType)
            .Distinct()
            .ToListAsync();

        // Check if user has consented to all
        foreach (var docType in mandatoryTypes)
        {
            if (!await HasConsentAsync(userId, docType))
                return false;
        }

        return true;
    }

    private static string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(bytes);
    }
}
