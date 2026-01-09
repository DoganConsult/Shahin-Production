using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Certification Tracking Service Implementation
/// Manages regulatory and compliance certifications lifecycle
/// </summary>
public class CertificationService : ICertificationService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<CertificationService> _logger;
    private readonly ITenantContextService _tenantContext;

    public CertificationService(
        GrcDbContext context,
        ILogger<CertificationService> logger,
        ITenantContextService tenantContext)
    {
        _context = context;
        _logger = logger;
        _tenantContext = tenantContext;
    }

    #region Certification CRUD

    public async Task<CertificationDto> CreateAsync(CreateCertificationRequest request)
    {
        var certification = new Certification
        {
            TenantId = request.TenantId,
            Name = request.Name,
            NameAr = request.NameAr,
            Code = request.Code,
            Description = request.Description,
            Category = request.Category,
            Type = request.Type,
            IssuingBody = request.IssuingBody,
            IssuingBodyAr = request.IssuingBodyAr,
            Status = "Planned",
            Scope = request.Scope,
            Level = request.Level,
            StandardVersion = request.StandardVersion,
            IsMandatory = request.IsMandatory,
            MandatorySource = request.MandatorySource,
            LinkedFrameworkCode = request.LinkedFrameworkCode,
            OwnerId = request.OwnerId,
            OwnerName = request.OwnerName,
            Department = request.Department
        };

        _context.Set<Certification>().Add(certification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created certification {Code} - {Name}", request.Code, request.Name);

        return MapToDto(certification);
    }

    public async Task<CertificationDto?> GetByIdAsync(Guid id)
    {
        var certification = await _context.Set<Certification>().FindAsync(id);
        return certification != null ? MapToDto(certification) : null;
    }

    public async Task<CertificationDetailDto?> GetDetailAsync(Guid id)
    {
        var certification = await _context.Set<Certification>()
            .Include(c => c.Audits)
            .FirstOrDefaultAsync(c => c.Id == id);

        return certification != null ? MapToDetailDto(certification) : null;
    }

    public async Task<CertificationDto> UpdateAsync(Guid id, UpdateCertificationRequest request)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        if (request.Name != null) certification.Name = request.Name;
        if (request.NameAr != null) certification.NameAr = request.NameAr;
        if (request.Description != null) certification.Description = request.Description;
        if (request.Scope != null) certification.Scope = request.Scope;
        if (request.Level != null) certification.Level = request.Level;
        if (request.StandardVersion != null) certification.StandardVersion = request.StandardVersion;
        if (request.AuditorName != null) certification.AuditorName = request.AuditorName;
        if (request.Cost.HasValue) certification.Cost = request.Cost;
        if (request.Notes != null) certification.Notes = request.Notes;
        if (request.RenewalLeadDays.HasValue) certification.RenewalLeadDays = request.RenewalLeadDays.Value;

        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(certification);
    }

    public async Task DeleteAsync(Guid id)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        _context.Set<Certification>().Remove(certification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted certification {Id}", id);
    }

    public async Task<List<CertificationDto>> GetAllAsync(Guid tenantId)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    public async Task<List<CertificationDto>> GetByStatusAsync(Guid tenantId, string status)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && c.Status == status)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    public async Task<List<CertificationDto>> GetByCategoryAsync(Guid tenantId, string category)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && c.Category == category)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    #endregion

    #region Lifecycle Management

    public async Task<CertificationDto> StartCertificationAsync(Guid id, StartCertificationRequest request)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.Status = "InProgress";
        certification.AuditorName = request.AuditorName;
        certification.Cost = request.EstimatedCost;
        certification.Notes = request.Notes;
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Started certification process for {Code}", certification.Code);

        return MapToDto(certification);
    }

    public async Task<CertificationDto> MarkIssuedAsync(Guid id, MarkIssuedRequest request)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.Status = "Active";
        certification.CertificationNumber = request.CertificationNumber;
        certification.IssuedDate = request.IssuedDate;
        certification.ExpiryDate = request.ExpiryDate;
        certification.NextSurveillanceDate = request.NextSurveillanceDate;
        certification.NextRecertificationDate = request.ExpiryDate;
        certification.CertificateUrl = request.CertificateUrl;
        if (request.ActualCost.HasValue) certification.Cost = request.ActualCost;
        if (!string.IsNullOrEmpty(request.Notes)) 
            certification.Notes = string.IsNullOrEmpty(certification.Notes) 
                ? request.Notes 
                : $"{certification.Notes}\n\n{request.Notes}";
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked certification {Code} as issued, expires {ExpiryDate}", 
            certification.Code, request.ExpiryDate);

        return MapToDto(certification);
    }

    public async Task<CertificationDto> RenewAsync(Guid id, RenewCertificationRequest request)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.Status = "Active";
        certification.LastRenewalDate = DateTime.UtcNow;
        certification.ExpiryDate = request.NewExpiryDate;
        certification.NextRecertificationDate = request.NewExpiryDate;
        if (request.NewSurveillanceDate.HasValue) 
            certification.NextSurveillanceDate = request.NewSurveillanceDate;
        if (!string.IsNullOrEmpty(request.NewCertificationNumber)) 
            certification.CertificationNumber = request.NewCertificationNumber;
        if (request.RenewalCost.HasValue) 
            certification.Cost = (certification.Cost ?? 0) + request.RenewalCost.Value;
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Renewed certification {Code}, new expiry: {ExpiryDate}", 
            certification.Code, request.NewExpiryDate);

        return MapToDto(certification);
    }

    public async Task<CertificationDto> SuspendAsync(Guid id, string reason, string suspendedBy)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.Status = "Suspended";
        certification.Notes = string.IsNullOrEmpty(certification.Notes)
            ? $"Suspended on {DateTime.UtcNow:yyyy-MM-dd} by {suspendedBy}: {reason}"
            : $"{certification.Notes}\n\nSuspended on {DateTime.UtcNow:yyyy-MM-dd} by {suspendedBy}: {reason}";
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogWarning("Suspended certification {Code}: {Reason}", certification.Code, reason);

        return MapToDto(certification);
    }

    public async Task<CertificationDto> ReinstateAsync(Guid id, string notes, string reinstatedBy)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.Status = "Active";
        certification.Notes = string.IsNullOrEmpty(certification.Notes)
            ? $"Reinstated on {DateTime.UtcNow:yyyy-MM-dd} by {reinstatedBy}: {notes}"
            : $"{certification.Notes}\n\nReinstated on {DateTime.UtcNow:yyyy-MM-dd} by {reinstatedBy}: {notes}";
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reinstated certification {Code}", certification.Code);

        return MapToDto(certification);
    }

    public async Task<CertificationDto> MarkExpiredAsync(Guid id)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.Status = "Expired";
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogWarning("Certification {Code} has expired", certification.Code);

        return MapToDto(certification);
    }

    #endregion

    #region Audit Management

    public async Task<CertificationAuditDto> ScheduleAuditAsync(Guid certificationId, ScheduleAuditRequest request)
    {
        var certification = await _context.Set<Certification>().FindAsync(certificationId)
            ?? throw new InvalidOperationException($"Certification {certificationId} not found");

        var audit = new CertificationAudit
        {
            TenantId = certification.TenantId,
            CertificationId = certificationId,
            AuditType = request.AuditType,
            AuditDate = request.AuditDate,
            AuditorName = request.AuditorName,
            LeadAuditorName = request.LeadAuditorName,
            Cost = request.EstimatedCost,
            Notes = request.Notes,
            Result = "Pending"
        };

        _context.Set<CertificationAudit>().Add(audit);

        // Update certification surveillance date
        if (request.AuditType == "Surveillance" || request.AuditType == "Recertification")
        {
            if (request.AuditType == "Surveillance")
                certification.NextSurveillanceDate = request.AuditDate;
            else
                certification.NextRecertificationDate = request.AuditDate;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Scheduled {AuditType} audit for certification {Code} on {Date}", 
            request.AuditType, certification.Code, request.AuditDate);

        return MapToAuditDto(audit, certification.Name);
    }

    public async Task<CertificationAuditDto> RecordAuditResultAsync(Guid auditId, RecordAuditResultRequest request)
    {
        var audit = await _context.Set<CertificationAudit>()
            .Include(a => a.Certification)
            .FirstOrDefaultAsync(a => a.Id == auditId)
            ?? throw new InvalidOperationException($"Audit {auditId} not found");

        audit.Result = request.Result;
        audit.MajorFindings = request.MajorFindings;
        audit.MinorFindings = request.MinorFindings;
        audit.Observations = request.Observations;
        audit.CorrectiveActionDeadline = request.CorrectiveActionDeadline;
        audit.ReportReference = request.ReportReference;
        if (request.ActualCost.HasValue) audit.Cost = request.ActualCost;
        audit.NextAuditDate = request.NextAuditDate;
        audit.Notes = request.Notes;
        audit.ModifiedDate = DateTime.UtcNow;

        // Update certification based on result
        if (audit.Certification != null)
        {
            if (request.Result == "Fail")
            {
                audit.Certification.Status = "Suspended";
            }
            else if (request.NextAuditDate.HasValue)
            {
                if (audit.AuditType == "Surveillance")
                    audit.Certification.NextSurveillanceDate = request.NextAuditDate;
                else if (audit.AuditType == "Recertification")
                    audit.Certification.NextRecertificationDate = request.NextAuditDate;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded {AuditType} audit result for certification {Code}: {Result}", 
            audit.AuditType, audit.Certification?.Code, request.Result);

        return MapToAuditDto(audit, audit.Certification?.Name ?? "");
    }

    public async Task<CertificationAuditDto?> GetAuditByIdAsync(Guid auditId)
    {
        var audit = await _context.Set<CertificationAudit>()
            .Include(a => a.Certification)
            .FirstOrDefaultAsync(a => a.Id == auditId);

        return audit != null ? MapToAuditDto(audit, audit.Certification?.Name ?? "") : null;
    }

    public async Task<List<CertificationAuditDto>> GetAuditHistoryAsync(Guid certificationId)
    {
        var certification = await _context.Set<Certification>().FindAsync(certificationId);

        var audits = await _context.Set<CertificationAudit>()
            .Where(a => a.CertificationId == certificationId)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();

        return audits.Select(a => MapToAuditDto(a, certification?.Name ?? "")).ToList();
    }

    public async Task<CertificationAuditDto> CompleteCorrectiveActionsAsync(Guid auditId, string notes, string completedBy)
    {
        var audit = await _context.Set<CertificationAudit>()
            .Include(a => a.Certification)
            .FirstOrDefaultAsync(a => a.Id == auditId)
            ?? throw new InvalidOperationException($"Audit {auditId} not found");

        audit.CorrectiveActionsCompleted = true;
        audit.Notes = string.IsNullOrEmpty(audit.Notes)
            ? $"Corrective actions completed on {DateTime.UtcNow:yyyy-MM-dd} by {completedBy}: {notes}"
            : $"{audit.Notes}\n\nCorrective actions completed on {DateTime.UtcNow:yyyy-MM-dd} by {completedBy}: {notes}";
        audit.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToAuditDto(audit, audit.Certification?.Name ?? "");
    }

    public async Task<List<CertificationAuditDto>> GetUpcomingAuditsAsync(Guid tenantId, int days = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);

        var audits = await _context.Set<CertificationAudit>()
            .Include(a => a.Certification)
            .Where(a => a.TenantId == tenantId && 
                       a.AuditDate >= DateTime.UtcNow && 
                       a.AuditDate <= cutoffDate &&
                       a.Result == "Pending")
            .OrderBy(a => a.AuditDate)
            .ToListAsync();

        return audits.Select(a => MapToAuditDto(a, a.Certification?.Name ?? "")).ToList();
    }

    #endregion

    #region Expiry & Renewal Tracking

    public async Task<List<CertificationDto>> GetExpiringSoonAsync(Guid tenantId, int days = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);

        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && 
                       c.Status == "Active" &&
                       c.ExpiryDate != null && 
                       c.ExpiryDate <= cutoffDate)
            .OrderBy(c => c.ExpiryDate)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    public async Task<List<CertificationDto>> GetExpiredAsync(Guid tenantId)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && 
                       (c.Status == "Expired" || 
                        (c.ExpiryDate != null && c.ExpiryDate < DateTime.UtcNow)))
            .OrderBy(c => c.ExpiryDate)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    public async Task<List<CertificationRenewalDto>> GetRenewalActionsAsync(Guid tenantId)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && c.Status == "Active" && c.ExpiryDate != null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var actions = new List<CertificationRenewalDto>();

        foreach (var cert in certifications)
        {
            var daysUntilExpiry = (int)(cert.ExpiryDate!.Value - now).TotalDays;
            var shouldStartRenewal = daysUntilExpiry <= cert.RenewalLeadDays;

            if (shouldStartRenewal)
            {
                var action = daysUntilExpiry switch
                {
                    <= 0 => "Urgent: Expired",
                    <= 30 => "Urgent: Renew Immediately",
                    <= 60 => "Schedule Recertification",
                    _ => "Start Renewal Process"
                };

                var priority = daysUntilExpiry switch
                {
                    <= 0 => "Critical",
                    <= 30 => "High",
                    <= 60 => "Medium",
                    _ => "Normal"
                };

                actions.Add(new CertificationRenewalDto
                {
                    CertificationId = cert.Id,
                    Name = cert.Name,
                    Code = cert.Code,
                    ExpiryDate = cert.ExpiryDate.Value,
                    DaysUntilExpiry = daysUntilExpiry,
                    ActionRequired = action,
                    Priority = priority,
                    OwnerName = cert.OwnerName
                });
            }
        }

        return actions.OrderBy(a => a.DaysUntilExpiry).ToList();
    }

    public async Task<CertificationDto> UpdateSurveillanceDateAsync(Guid id, DateTime nextDate)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.NextSurveillanceDate = nextDate;
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(certification);
    }

    #endregion

    #region Reporting & Analytics

    public async Task<CertificationDashboardDto> GetDashboardAsync(Guid tenantId)
    {
        return new CertificationDashboardDto
        {
            TenantId = tenantId,
            Statistics = await GetStatisticsAsync(tenantId),
            ActiveCertifications = await GetByStatusAsync(tenantId, "Active"),
            ExpiringSoon = await GetExpiringSoonAsync(tenantId, 90),
            UpcomingAudits = await GetUpcomingAuditsAsync(tenantId, 90),
            RenewalActions = await GetRenewalActionsAsync(tenantId),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<CertificationStatisticsDto> GetStatisticsAsync(Guid tenantId)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var now = DateTime.UtcNow;

        return new CertificationStatisticsDto
        {
            TotalCertifications = certifications.Count,
            ActiveCertifications = certifications.Count(c => c.Status == "Active"),
            ExpiredCertifications = certifications.Count(c => c.Status == "Expired"),
            InProgressCertifications = certifications.Count(c => c.Status == "InProgress"),
            PlannedCertifications = certifications.Count(c => c.Status == "Planned"),
            ExpiringSoon = certifications.Count(c => 
                c.Status == "Active" && 
                c.ExpiryDate != null && 
                c.ExpiryDate <= now.AddDays(90)),
            MandatoryCertifications = certifications.Count(c => c.IsMandatory),
            ByCategory = certifications.GroupBy(c => c.Category).ToDictionary(g => g.Key, g => g.Count()),
            ByType = certifications.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.Count()),
            ByStatus = certifications.GroupBy(c => c.Status).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<CertificationMatrixDto> GetComplianceMatrixAsync(Guid tenantId)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        // Get unique framework codes
        var frameworkCodes = certifications
            .Where(c => !string.IsNullOrEmpty(c.LinkedFrameworkCode))
            .Select(c => c.LinkedFrameworkCode!)
            .Distinct();

        var rows = frameworkCodes.Select(fc =>
        {
            var cert = certifications.FirstOrDefault(c => c.LinkedFrameworkCode == fc);
            return new CertificationMatrixRowDto
            {
                FrameworkCode = fc,
                FrameworkName = cert?.Name ?? fc,
                IsMandatory = cert?.IsMandatory ?? false,
                CertificationCode = cert?.Code,
                CertificationStatus = cert?.Status switch
                {
                    "Active" => "Certified",
                    "InProgress" => "InProgress",
                    "Expired" => "Expired",
                    _ => "NotStarted"
                },
                ExpiryDate = cert?.ExpiryDate,
                ComplianceLevel = cert?.Status == "Active" ? 100 : 
                                  cert?.Status == "InProgress" ? 50 : 0
            };
        }).ToList();

        return new CertificationMatrixDto
        {
            TenantId = tenantId,
            Rows = rows,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<CertificationCostSummaryDto> GetCostSummaryAsync(Guid tenantId, int year)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && c.IssuedDate != null && c.IssuedDate.Value.Year == year)
            .ToListAsync();

        var audits = await _context.Set<CertificationAudit>()
            .Where(a => a.TenantId == tenantId && a.AuditDate.Year == year)
            .ToListAsync();

        var totalCertCost = certifications.Sum(c => c.Cost ?? 0);
        var totalAuditCost = audits.Sum(a => a.Cost ?? 0);

        return new CertificationCostSummaryDto
        {
            TenantId = tenantId,
            Year = year,
            TotalCertificationCost = totalCertCost,
            TotalAuditCost = totalAuditCost,
            TotalCost = totalCertCost + totalAuditCost,
            Currency = "SAR",
            ByCertification = certifications
                .Where(c => c.Cost.HasValue)
                .ToDictionary(c => c.Code, c => c.Cost!.Value),
            ByCategory = certifications
                .GroupBy(c => c.Category)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Cost ?? 0)),
            ByMonth = Enumerable.Range(1, 12)
                .Select(m => new CostByMonthDto(
                    m,
                    certifications.Where(c => c.IssuedDate?.Month == m).Sum(c => c.Cost ?? 0),
                    audits.Where(a => a.AuditDate.Month == m).Sum(a => a.Cost ?? 0)
                ))
                .ToList()
        };
    }

    #endregion

    #region Ownership & Assignment

    public async Task<CertificationDto> AssignOwnerAsync(Guid id, string ownerId, string ownerName, string? department)
    {
        var certification = await _context.Set<Certification>().FindAsync(id)
            ?? throw new InvalidOperationException($"Certification {id} not found");

        certification.OwnerId = ownerId;
        certification.OwnerName = ownerName;
        certification.Department = department;
        certification.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned owner {OwnerName} to certification {Code}", ownerName, certification.Code);

        return MapToDto(certification);
    }

    public async Task<List<CertificationDto>> GetByOwnerAsync(string ownerId)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.OwnerId == ownerId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    public async Task<List<CertificationDto>> GetByDepartmentAsync(Guid tenantId, string department)
    {
        var certifications = await _context.Set<Certification>()
            .Where(c => c.TenantId == tenantId && c.Department == department)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return certifications.Select(MapToDto).ToList();
    }

    #endregion

    #region Private Helper Methods

    private static CertificationDto MapToDto(Certification cert)
    {
        var now = DateTime.UtcNow;
        var daysUntilExpiry = cert.ExpiryDate.HasValue 
            ? (int)(cert.ExpiryDate.Value - now).TotalDays 
            : int.MaxValue;

        return new CertificationDto
        {
            Id = cert.Id,
            TenantId = cert.TenantId ?? Guid.Empty,
            Name = cert.Name,
            NameAr = cert.NameAr,
            Code = cert.Code,
            Description = cert.Description,
            Category = cert.Category,
            Type = cert.Type,
            IssuingBody = cert.IssuingBody,
            Status = cert.Status,
            CertificationNumber = cert.CertificationNumber,
            Scope = cert.Scope,
            IssuedDate = cert.IssuedDate,
            ExpiryDate = cert.ExpiryDate,
            NextSurveillanceDate = cert.NextSurveillanceDate,
            NextRecertificationDate = cert.NextRecertificationDate,
            Level = cert.Level,
            StandardVersion = cert.StandardVersion,
            OwnerName = cert.OwnerName,
            Department = cert.Department,
            IsMandatory = cert.IsMandatory,
            DaysUntilExpiry = daysUntilExpiry,
            IsExpiringSoon = daysUntilExpiry <= 90 && daysUntilExpiry > 0,
            CreatedDate = cert.CreatedDate
        };
    }

    private static CertificationDetailDto MapToDetailDto(Certification cert)
    {
        var now = DateTime.UtcNow;
        var daysUntilExpiry = cert.ExpiryDate.HasValue 
            ? (int)(cert.ExpiryDate.Value - now).TotalDays 
            : int.MaxValue;

        return new CertificationDetailDto
        {
            Id = cert.Id,
            TenantId = cert.TenantId ?? Guid.Empty,
            Name = cert.Name,
            NameAr = cert.NameAr,
            Code = cert.Code,
            Description = cert.Description,
            Category = cert.Category,
            Type = cert.Type,
            IssuingBody = cert.IssuingBody,
            Status = cert.Status,
            CertificationNumber = cert.CertificationNumber,
            Scope = cert.Scope,
            IssuedDate = cert.IssuedDate,
            ExpiryDate = cert.ExpiryDate,
            NextSurveillanceDate = cert.NextSurveillanceDate,
            NextRecertificationDate = cert.NextRecertificationDate,
            Level = cert.Level,
            StandardVersion = cert.StandardVersion,
            OwnerName = cert.OwnerName,
            Department = cert.Department,
            IsMandatory = cert.IsMandatory,
            DaysUntilExpiry = daysUntilExpiry,
            IsExpiringSoon = daysUntilExpiry <= 90 && daysUntilExpiry > 0,
            CreatedDate = cert.CreatedDate,
            AuditorName = cert.AuditorName,
            Cost = cert.Cost,
            CostCurrency = cert.CostCurrency,
            CertificateUrl = cert.CertificateUrl,
            Notes = cert.Notes,
            LinkedFrameworkCode = cert.LinkedFrameworkCode,
            MandatorySource = cert.MandatorySource,
            RenewalLeadDays = cert.RenewalLeadDays,
            LastRenewalDate = cert.LastRenewalDate,
            Audits = cert.Audits.Select(a => MapToAuditDto(a, cert.Name)).ToList()
        };
    }

    private static CertificationAuditDto MapToAuditDto(CertificationAudit audit, string certificationName)
    {
        return new CertificationAuditDto
        {
            Id = audit.Id,
            CertificationId = audit.CertificationId,
            CertificationName = certificationName,
            AuditType = audit.AuditType,
            AuditDate = audit.AuditDate,
            AuditorName = audit.AuditorName,
            LeadAuditorName = audit.LeadAuditorName,
            Result = audit.Result,
            MajorFindings = audit.MajorFindings,
            MinorFindings = audit.MinorFindings,
            Observations = audit.Observations,
            CorrectiveActionDeadline = audit.CorrectiveActionDeadline,
            CorrectiveActionsCompleted = audit.CorrectiveActionsCompleted,
            ReportReference = audit.ReportReference,
            Cost = audit.Cost,
            Notes = audit.Notes,
            NextAuditDate = audit.NextAuditDate
        };
    }

    #endregion
}
