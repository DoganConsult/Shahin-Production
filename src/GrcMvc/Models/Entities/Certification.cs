using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities;

/// <summary>
/// Certification Entity - Tracks regulatory/compliance certifications
/// Examples: ISO 27001, SOC 2 Type II, PCI-DSS, NCA Tier 3, etc.
/// </summary>
public class Certification : BaseEntity
{
    /// <summary>
    /// Certification name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Certification name in Arabic
    /// </summary>
    [MaxLength(200)]
    public string? NameAr { get; set; }
    
    /// <summary>
    /// Short code (e.g., ISO27001, SOC2T2, PCI-DSS)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the certification
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Category of certification
    /// </summary>
    public string Category { get; set; } = "Standard"; // Standard, Regulatory, Industry, Internal
    
    /// <summary>
    /// Type of certification
    /// </summary>
    public string Type { get; set; } = "Compliance"; // Compliance, Security, Quality, Industry, Environmental
    
    /// <summary>
    /// Issuing body/authority
    /// </summary>
    [Required]
    public string IssuingBody { get; set; } = string.Empty;
    
    /// <summary>
    /// Issuing body in Arabic
    /// </summary>
    public string? IssuingBodyAr { get; set; }
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public string Status { get; set; } = "Active"; // Active, Expired, Suspended, Revoked, InProgress, Planned
    
    /// <summary>
    /// Certification number/ID from issuing body
    /// </summary>
    public string? CertificationNumber { get; set; }
    
    /// <summary>
    /// Scope of certification
    /// </summary>
    public string? Scope { get; set; }
    
    /// <summary>
    /// Date certification was issued
    /// </summary>
    public DateTime? IssuedDate { get; set; }
    
    /// <summary>
    /// Date certification expires
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    /// <summary>
    /// Last renewal date
    /// </summary>
    public DateTime? LastRenewalDate { get; set; }
    
    /// <summary>
    /// Next surveillance audit date
    /// </summary>
    public DateTime? NextSurveillanceDate { get; set; }
    
    /// <summary>
    /// Next recertification date
    /// </summary>
    public DateTime? NextRecertificationDate { get; set; }
    
    /// <summary>
    /// Days before expiry to start renewal process
    /// </summary>
    public int RenewalLeadDays { get; set; } = 90;
    
    /// <summary>
    /// Certification level/tier (if applicable)
    /// </summary>
    public string? Level { get; set; } // Tier 1, Tier 2, Level 1, etc.
    
    /// <summary>
    /// Version of the standard
    /// </summary>
    public string? StandardVersion { get; set; } // e.g., "ISO 27001:2022"
    
    /// <summary>
    /// Primary owner responsible for certification
    /// </summary>
    public string? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    
    /// <summary>
    /// Department responsible
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// External auditor/registrar
    /// </summary>
    public string? AuditorName { get; set; }
    
    /// <summary>
    /// Cost of certification
    /// </summary>
    public decimal? Cost { get; set; }
    public string CostCurrency { get; set; } = "SAR";
    
    /// <summary>
    /// Certificate document reference/URL
    /// </summary>
    public string? CertificateUrl { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Related framework code (if linked to a regulatory framework)
    /// </summary>
    public string? LinkedFrameworkCode { get; set; }
    
    /// <summary>
    /// Is this certification mandatory for operations?
    /// </summary>
    public bool IsMandatory { get; set; }
    
    /// <summary>
    /// Regulatory requirement source (if mandatory)
    /// </summary>
    public string? MandatorySource { get; set; }
    
    /// <summary>
    /// Navigation: Audit history
    /// </summary>
    public virtual ICollection<CertificationAudit> Audits { get; set; } = new List<CertificationAudit>();
}

/// <summary>
/// Certification Audit Record - Tracks surveillance and recertification audits
/// </summary>
public class CertificationAudit : BaseEntity
{
    public Guid CertificationId { get; set; }
    
    [ForeignKey("CertificationId")]
    public virtual Certification? Certification { get; set; }
    
    /// <summary>
    /// Type of audit
    /// </summary>
    [Required]
    public string AuditType { get; set; } = "Surveillance"; // Initial, Surveillance, Recertification, Special
    
    /// <summary>
    /// Audit date
    /// </summary>
    public DateTime AuditDate { get; set; }
    
    /// <summary>
    /// Auditor/Registrar name
    /// </summary>
    public string? AuditorName { get; set; }
    
    /// <summary>
    /// Lead auditor name
    /// </summary>
    public string? LeadAuditorName { get; set; }
    
    /// <summary>
    /// Audit result
    /// </summary>
    [Required]
    public string Result { get; set; } = "Pass"; // Pass, ConditionalPass, Fail, Pending
    
    /// <summary>
    /// Number of major findings
    /// </summary>
    public int MajorFindings { get; set; }
    
    /// <summary>
    /// Number of minor findings
    /// </summary>
    public int MinorFindings { get; set; }
    
    /// <summary>
    /// Number of observations
    /// </summary>
    public int Observations { get; set; }
    
    /// <summary>
    /// Corrective action deadline
    /// </summary>
    public DateTime? CorrectiveActionDeadline { get; set; }
    
    /// <summary>
    /// Are corrective actions completed?
    /// </summary>
    public bool CorrectiveActionsCompleted { get; set; }
    
    /// <summary>
    /// Audit report reference
    /// </summary>
    public string? ReportReference { get; set; }
    
    /// <summary>
    /// Audit cost
    /// </summary>
    public decimal? Cost { get; set; }
    
    /// <summary>
    /// Notes/summary
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Next audit date (calculated or scheduled)
    /// </summary>
    public DateTime? NextAuditDate { get; set; }
}
