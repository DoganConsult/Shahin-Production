using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces;

/// <summary>
/// Government Integration Service Interface
/// خدمة التكامل الحكومي
/// Integration with Saudi government systems (Nafath, Absher, Etimad, etc.)
/// </summary>
public interface IGovernmentIntegrationService
{
    // ============================================
    // NAFATH Integration (نفاذ)
    // National Single Sign-On
    // ============================================

    /// <summary>
    /// Initiate Nafath authentication
    /// </summary>
    Task<NafathAuthRequest> InitiateNafathAuthAsync(string nationalId);

    /// <summary>
    /// Verify Nafath authentication
    /// </summary>
    Task<NafathAuthResult> VerifyNafathAuthAsync(string transactionId, string otp);

    /// <summary>
    /// Get user identity from Nafath
    /// </summary>
    Task<NafathIdentity?> GetNafathIdentityAsync(string nationalId);

    // ============================================
    // ABSHER Integration (أبشر)
    // Employee/Individual Verification
    // ============================================

    /// <summary>
    /// Verify employee identity via Absher
    /// </summary>
    Task<AbsherVerificationResult> VerifyEmployeeAsync(string nationalId, string dateOfBirth);

    /// <summary>
    /// Get employment status from Absher
    /// </summary>
    Task<AbsherEmploymentStatus?> GetEmploymentStatusAsync(string nationalId);

    // ============================================
    // ETIMAD Integration (اعتماد)
    // Government Procurement
    // ============================================

    /// <summary>
    /// Get procurement compliance status
    /// </summary>
    Task<EtimadComplianceStatus> GetProcurementComplianceAsync(string crNumber);

    /// <summary>
    /// Verify vendor registration
    /// </summary>
    Task<EtimadVendorInfo?> VerifyVendorAsync(string crNumber);

    // ============================================
    // MUQEEM Integration (مقيم)
    // Workforce Compliance
    // ============================================

    /// <summary>
    /// Verify resident worker
    /// </summary>
    Task<MuqeemWorkerInfo?> VerifyWorkerAsync(string iqamaNumber);

    /// <summary>
    /// Get workforce compliance summary
    /// </summary>
    Task<MuqeemComplianceSummary> GetWorkforceComplianceAsync(string crNumber);

    // ============================================
    // QIWA Integration (قوى)
    // Labor Compliance
    // ============================================

    /// <summary>
    /// Get labor compliance status
    /// </summary>
    Task<QiwaComplianceStatus> GetLaborComplianceAsync(string crNumber);

    /// <summary>
    /// Verify Saudization ratio
    /// </summary>
    Task<SaudizationInfo> GetSaudizationInfoAsync(string crNumber);

    // ============================================
    // ZATCA Integration (الزكاة والضريبة)
    // Tax Compliance
    // ============================================

    /// <summary>
    /// Verify tax compliance status
    /// </summary>
    Task<ZatcaComplianceStatus> GetTaxComplianceAsync(string vatNumber);

    /// <summary>
    /// Verify e-invoicing compliance
    /// </summary>
    Task<EInvoicingStatus> GetEInvoicingStatusAsync(string vatNumber);

    // ============================================
    // General Methods
    // ============================================

    /// <summary>
    /// Check integration availability
    /// </summary>
    Task<IntegrationHealthStatus> CheckHealthAsync();

    /// <summary>
    /// Get all compliance statuses for an organization
    /// </summary>
    Task<OrganizationGovernmentCompliance> GetFullComplianceStatusAsync(string crNumber);
}

// ============================================
// NAFATH Models
// ============================================

public class NafathAuthRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string RandomNumber { get; set; } = string.Empty; // Show to user for app verification
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = "Pending";
}

public class NafathAuthResult
{
    public bool IsAuthenticated { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? FullNameAr { get; set; }
    public string? FullNameEn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? AuthenticatedAt { get; set; }
}

public class NafathIdentity
{
    public string NationalId { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string IdExpiryDate { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}

// ============================================
// ABSHER Models
// ============================================

public class AbsherVerificationResult
{
    public bool IsVerified { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class AbsherEmploymentStatus
{
    public string NationalId { get; set; } = string.Empty;
    public bool IsEmployed { get; set; }
    public string? EmployerName { get; set; }
    public string? EmployerCrNumber { get; set; }
    public DateTime? EmploymentStartDate { get; set; }
    public string JobTitle { get; set; } = string.Empty;
}

// ============================================
// ETIMAD Models
// ============================================

public class EtimadComplianceStatus
{
    public string CrNumber { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public bool IsRegistered { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
    public DateTime? RegistrationExpiryDate { get; set; }
    public List<string> ActiveCategories { get; set; } = new();
    public List<string> ComplianceIssues { get; set; } = new();
}

public class EtimadVendorInfo
{
    public string CrNumber { get; set; } = string.Empty;
    public string CompanyNameAr { get; set; } = string.Empty;
    public string CompanyNameEn { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public double? Rating { get; set; }
}

// ============================================
// MUQEEM Models
// ============================================

public class MuqeemWorkerInfo
{
    public string IqamaNumber { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string SponsorCrNumber { get; set; } = string.Empty;
    public DateTime IqamaExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsValid { get; set; }
}

public class MuqeemComplianceSummary
{
    public string CrNumber { get; set; } = string.Empty;
    public int TotalWorkers { get; set; }
    public int ValidIqamas { get; set; }
    public int ExpiringIqamas { get; set; }
    public int ExpiredIqamas { get; set; }
    public double ComplianceRate { get; set; }
    public List<string> Issues { get; set; } = new();
}

// ============================================
// QIWA Models
// ============================================

public class QiwaComplianceStatus
{
    public string CrNumber { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string NitaqatCategory { get; set; } = string.Empty; // Platinum, Green, Yellow, Red
    public double SaudizationRatio { get; set; }
    public double RequiredSaudizationRatio { get; set; }
    public int TotalEmployees { get; set; }
    public int SaudiEmployees { get; set; }
    public int NonSaudiEmployees { get; set; }
    public List<string> ViolationsEn { get; set; } = new();
    public List<string> ViolationsAr { get; set; } = new();
}

public class SaudizationInfo
{
    public string CrNumber { get; set; } = string.Empty;
    public string ActivityCode { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public double CurrentRatio { get; set; }
    public double RequiredRatio { get; set; }
    public bool IsMeetingTarget { get; set; }
    public string NitaqatColor { get; set; } = string.Empty;
    public int SaudisNeededToMeetTarget { get; set; }
}

// ============================================
// ZATCA Models
// ============================================

public class ZatcaComplianceStatus
{
    public string VatNumber { get; set; } = string.Empty;
    public bool IsVatRegistered { get; set; }
    public bool IsVatCompliant { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string TaxpayerName { get; set; } = string.Empty;
    public string TaxpayerNameAr { get; set; } = string.Empty;
    public List<string> ComplianceIssues { get; set; } = new();
}

public class EInvoicingStatus
{
    public string VatNumber { get; set; } = string.Empty;
    public bool IsOnboarded { get; set; }
    public string Phase { get; set; } = string.Empty; // Phase1, Phase2
    public DateTime? OnboardingDate { get; set; }
    public bool IsCompliant { get; set; }
    public int TotalInvoicesThisMonth { get; set; }
    public int RejectedInvoices { get; set; }
    public List<string> Issues { get; set; } = new();
}

// ============================================
// General Models
// ============================================

public class IntegrationHealthStatus
{
    public bool OverallHealthy { get; set; }
    public Dictionary<string, ServiceHealth> Services { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}

public class ServiceHealth
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}

public class OrganizationGovernmentCompliance
{
    public string CrNumber { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }

    public double OverallComplianceScore { get; set; }
    public string OverallStatus { get; set; } = string.Empty;

    public EtimadComplianceStatus? Etimad { get; set; }
    public QiwaComplianceStatus? Qiwa { get; set; }
    public MuqeemComplianceSummary? Muqeem { get; set; }
    public ZatcaComplianceStatus? Zatca { get; set; }
    public EInvoicingStatus? EInvoicing { get; set; }

    public List<string> CriticalIssues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}
