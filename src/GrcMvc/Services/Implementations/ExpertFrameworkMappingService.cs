using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Expert-driven sector mapping service that automatically determines the full compliance lifecycle
/// based on organization sector: Framework → Control → Evidence → Score → Implementation
/// </summary>
public class ExpertFrameworkMappingService : IExpertFrameworkMappingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpertFrameworkMappingService> _logger;

    // Master sector blueprints - Expert knowledge embedded
    private static readonly Dictionary<string, SectorComplianceBlueprint> SectorBlueprints = new()
    {
        ["Banking"] = new SectorComplianceBlueprint
        {
            SectorCode = "BANKING",
            SectorName = "Banking & Financial Services",
            SectorNameAr = "الخدمات المصرفية والمالية",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "SAMA-CSF", Name = "SAMA Cybersecurity Framework", Priority = 1, Mandatory = true, Reason = "Required for all SAMA-regulated financial institutions" },
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 2, Mandatory = true, Reason = "National requirement for critical infrastructure" },
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 3, Mandatory = true, Reason = "Customer data processing requirements" },
                new() { Code = "SAMA-AML", Name = "SAMA Anti-Money Laundering", Priority = 4, Mandatory = true, Reason = "Financial crime prevention" },
                new() { Code = "PCI-DSS", Name = "Payment Card Industry DSS", Priority = 5, Mandatory = false, Reason = "If processing card payments" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["Access Control"] = 1,
                ["Cryptography"] = 1,
                ["Incident Response"] = 1,
                ["Business Continuity"] = 1,
                ["Third Party Security"] = 2,
                ["Network Security"] = 2
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "SOC2_Report", Required = true, Frequency = "Annual", Description = "SOC 2 Type II Report" },
                new() { EvidenceType = "Penetration_Test", Required = true, Frequency = "Annual", Description = "External penetration test results" },
                new() { EvidenceType = "Access_Review", Required = true, Frequency = "Quarterly", Description = "User access review documentation" },
                new() { EvidenceType = "BCP_Test", Required = true, Frequency = "Annual", Description = "Business continuity plan test results" },
                new() { EvidenceType = "Incident_Log", Required = true, Frequency = "Continuous", Description = "Security incident log" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["Cybersecurity"] = 0.35,
                ["Data Protection"] = 0.25,
                ["Financial Compliance"] = 0.20,
                ["Operational Resilience"] = 0.20
            },
            ImplementationGuidance = "Focus on transaction security, customer data protection, and regulatory reporting. SAMA inspections occur annually.",
            EstimatedImplementationMonths = 9,
            RegulatoryDeadlines = new List<string> { "SAMA CSF: Immediate compliance required", "PDPL: Full compliance by end of year" }
        },

        ["Healthcare"] = new SectorComplianceBlueprint
        {
            SectorCode = "HEALTHCARE",
            SectorName = "Healthcare & Medical",
            SectorNameAr = "الرعاية الصحية والطبية",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 1, Mandatory = true, Reason = "Healthcare is critical infrastructure" },
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 1, Mandatory = true, Reason = "Patient health data is sensitive personal data" },
                new() { Code = "SFDA", Name = "SFDA Medical Device Regulations", Priority = 2, Mandatory = true, Reason = "Medical device cybersecurity requirements" },
                new() { Code = "MOH-HIS", Name = "MOH Health Information Standards", Priority = 3, Mandatory = true, Reason = "Health information exchange requirements" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["Data Protection"] = 1,
                ["Access Control"] = 1,
                ["Medical Device Security"] = 1,
                ["Patient Privacy"] = 1,
                ["Incident Response"] = 2,
                ["Business Continuity"] = 2
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "HIPAA_Assessment", Required = true, Frequency = "Annual", Description = "Privacy and security assessment" },
                new() { EvidenceType = "Access_Audit", Required = true, Frequency = "Quarterly", Description = "Patient data access audit" },
                new() { EvidenceType = "Device_Inventory", Required = true, Frequency = "Continuous", Description = "Medical device inventory and security status" },
                new() { EvidenceType = "Training_Records", Required = true, Frequency = "Annual", Description = "Staff privacy training completion" },
                new() { EvidenceType = "Consent_Management", Required = true, Frequency = "Continuous", Description = "Patient consent documentation" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["Patient Privacy"] = 0.35,
                ["Data Security"] = 0.30,
                ["Medical Device Safety"] = 0.20,
                ["Operational Continuity"] = 0.15
            },
            ImplementationGuidance = "Prioritize patient data protection and medical device security. MOH conducts periodic inspections.",
            EstimatedImplementationMonths = 12,
            RegulatoryDeadlines = new List<string> { "PDPL: Immediate for health data", "SFDA: Device registration required" }
        },

        ["Government"] = new SectorComplianceBlueprint
        {
            SectorCode = "GOVERNMENT",
            SectorName = "Government & Public Sector",
            SectorNameAr = "القطاع الحكومي والعام",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 1, Mandatory = true, Reason = "Mandatory for all government entities" },
                new() { Code = "NCA-CSCC", Name = "NCA Critical Systems Controls", Priority = 1, Mandatory = true, Reason = "Government systems are critical infrastructure" },
                new() { Code = "DGA-CLOUD", Name = "DGA Cloud First Policy", Priority = 2, Mandatory = true, Reason = "Government cloud adoption requirements" },
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 3, Mandatory = true, Reason = "Citizen data protection" },
                new() { Code = "NDMO", Name = "NDMO Data Governance Standards", Priority = 3, Mandatory = true, Reason = "National data management requirements" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["Data Classification"] = 1,
                ["Access Control"] = 1,
                ["Cryptography"] = 1,
                ["Incident Response"] = 1,
                ["Third Party Security"] = 1,
                ["Cloud Security"] = 2
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "NCA_Assessment", Required = true, Frequency = "Annual", Description = "NCA compliance assessment report" },
                new() { EvidenceType = "Data_Classification", Required = true, Frequency = "Continuous", Description = "Data classification inventory" },
                new() { EvidenceType = "Security_Clearance", Required = true, Frequency = "Annual", Description = "Staff security clearance records" },
                new() { EvidenceType = "Cloud_Security", Required = true, Frequency = "Quarterly", Description = "Cloud security posture assessment" },
                new() { EvidenceType = "Vendor_Assessment", Required = true, Frequency = "Annual", Description = "Third-party vendor security assessments" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["National Security"] = 0.30,
                ["Data Governance"] = 0.25,
                ["Cybersecurity"] = 0.25,
                ["Service Continuity"] = 0.20
            },
            ImplementationGuidance = "Focus on data classification, sovereign cloud, and NCA compliance. NCA conducts mandatory annual assessments.",
            EstimatedImplementationMonths = 6,
            RegulatoryDeadlines = new List<string> { "NCA ECC: Immediate", "NCA CSCC: Within 6 months", "Cloud First: Ongoing migration" }
        },

        ["Telecom"] = new SectorComplianceBlueprint
        {
            SectorCode = "TELECOM",
            SectorName = "Telecommunications",
            SectorNameAr = "الاتصالات",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "CST-CRF", Name = "CST Cybersecurity Regulatory Framework", Priority = 1, Mandatory = true, Reason = "CST-regulated entity requirement" },
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 1, Mandatory = true, Reason = "Telecom is critical infrastructure" },
                new() { Code = "NCA-CSCC", Name = "NCA Critical Systems Controls", Priority = 2, Mandatory = true, Reason = "Critical national infrastructure" },
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 3, Mandatory = true, Reason = "Subscriber data protection" },
                new() { Code = "CST-CLOUD", Name = "CST Cloud Regulations", Priority = 4, Mandatory = false, Reason = "If providing cloud services" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["Network Security"] = 1,
                ["Infrastructure Protection"] = 1,
                ["Incident Response"] = 1,
                ["Business Continuity"] = 1,
                ["Data Protection"] = 2,
                ["Access Control"] = 2
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "Network_Audit", Required = true, Frequency = "Quarterly", Description = "Network security audit results" },
                new() { EvidenceType = "Availability_Report", Required = true, Frequency = "Monthly", Description = "Service availability and uptime reports" },
                new() { EvidenceType = "Incident_Report", Required = true, Frequency = "Continuous", Description = "Security incident reports to CST" },
                new() { EvidenceType = "Penetration_Test", Required = true, Frequency = "Quarterly", Description = "Network penetration testing" },
                new() { EvidenceType = "DR_Test", Required = true, Frequency = "Annual", Description = "Disaster recovery test documentation" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["Network Security"] = 0.35,
                ["Service Availability"] = 0.25,
                ["Data Protection"] = 0.20,
                ["Regulatory Compliance"] = 0.20
            },
            ImplementationGuidance = "Focus on network infrastructure security and service continuity. CST conducts regular inspections.",
            EstimatedImplementationMonths = 9,
            RegulatoryDeadlines = new List<string> { "CST CRF: Immediate", "NCA: Within 6 months" }
        },

        ["Energy"] = new SectorComplianceBlueprint
        {
            SectorCode = "ENERGY",
            SectorName = "Energy & Utilities",
            SectorNameAr = "الطاقة والمرافق",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 1, Mandatory = true, Reason = "Critical national infrastructure" },
                new() { Code = "NCA-CSCC", Name = "NCA Critical Systems Controls", Priority = 1, Mandatory = true, Reason = "OT/ICS systems protection" },
                new() { Code = "HCIS", Name = "HCIS Industrial Cybersecurity", Priority = 2, Mandatory = true, Reason = "Industrial control systems security" },
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 3, Mandatory = true, Reason = "Customer data protection" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["OT Security"] = 1,
                ["Physical Security"] = 1,
                ["Incident Response"] = 1,
                ["Business Continuity"] = 1,
                ["Network Segmentation"] = 1,
                ["Access Control"] = 2
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "OT_Assessment", Required = true, Frequency = "Annual", Description = "OT/ICS security assessment" },
                new() { EvidenceType = "Network_Segmentation", Required = true, Frequency = "Quarterly", Description = "IT/OT network segmentation verification" },
                new() { EvidenceType = "Physical_Security", Required = true, Frequency = "Quarterly", Description = "Physical security audit" },
                new() { EvidenceType = "Incident_Drill", Required = true, Frequency = "Semi-annual", Description = "Incident response drill documentation" },
                new() { EvidenceType = "Vendor_Access", Required = true, Frequency = "Continuous", Description = "Third-party vendor access logs" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["OT/ICS Security"] = 0.40,
                ["Physical Security"] = 0.25,
                ["Incident Response"] = 0.20,
                ["IT Security"] = 0.15
            },
            ImplementationGuidance = "Priority on OT/ICS segmentation and industrial control system security. Critical infrastructure audits by NCA.",
            EstimatedImplementationMonths = 12,
            RegulatoryDeadlines = new List<string> { "NCA CSCC: Immediate for critical systems", "HCIS: Within 12 months" }
        },

        ["Retail"] = new SectorComplianceBlueprint
        {
            SectorCode = "RETAIL",
            SectorName = "Retail & E-Commerce",
            SectorNameAr = "التجزئة والتجارة الإلكترونية",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 1, Mandatory = true, Reason = "Customer data processing" },
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 2, Mandatory = true, Reason = "Basic cybersecurity requirements" },
                new() { Code = "PCI-DSS", Name = "Payment Card Industry DSS", Priority = 2, Mandatory = true, Reason = "Card payment processing" },
                new() { Code = "MOCI-ECOM", Name = "MOCI E-Commerce Regulations", Priority = 3, Mandatory = true, Reason = "E-commerce compliance" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["Payment Security"] = 1,
                ["Customer Data Protection"] = 1,
                ["Website Security"] = 2,
                ["Access Control"] = 2,
                ["Incident Response"] = 3
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "PCI_SAQ", Required = true, Frequency = "Annual", Description = "PCI DSS Self-Assessment Questionnaire" },
                new() { EvidenceType = "Web_Scan", Required = true, Frequency = "Quarterly", Description = "Web application security scan" },
                new() { EvidenceType = "Privacy_Notice", Required = true, Frequency = "Annual", Description = "Privacy notice and consent mechanisms" },
                new() { EvidenceType = "Data_Inventory", Required = true, Frequency = "Annual", Description = "Customer data inventory" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["Payment Security"] = 0.35,
                ["Customer Privacy"] = 0.30,
                ["Website Security"] = 0.20,
                ["General Security"] = 0.15
            },
            ImplementationGuidance = "Focus on PCI compliance and customer data protection. MOCI and CITC oversight.",
            EstimatedImplementationMonths = 6,
            RegulatoryDeadlines = new List<string> { "PDPL: Immediate", "PCI-DSS: Ongoing" }
        },

        ["Technology"] = new SectorComplianceBlueprint
        {
            SectorCode = "TECHNOLOGY",
            SectorName = "Technology & Software",
            SectorNameAr = "التقنية والبرمجيات",
            ApplicableFrameworks = new List<FrameworkMapping>
            {
                new() { Code = "NCA-ECC", Name = "NCA Essential Cybersecurity Controls", Priority = 1, Mandatory = true, Reason = "Basic cybersecurity requirements" },
                new() { Code = "PDPL", Name = "Personal Data Protection Law", Priority = 2, Mandatory = true, Reason = "User data processing" },
                new() { Code = "CST-CLOUD", Name = "CST Cloud Regulations", Priority = 3, Mandatory = false, Reason = "If providing cloud services" },
                new() { Code = "ISO27001", Name = "ISO 27001 Information Security", Priority = 4, Mandatory = false, Reason = "Recommended for enterprise clients" }
            },
            ControlPriorities = new Dictionary<string, int>
            {
                ["Secure Development"] = 1,
                ["Access Control"] = 1,
                ["Data Protection"] = 2,
                ["Incident Response"] = 2,
                ["Change Management"] = 2
            },
            EvidenceRequirements = new List<SectorEvidenceRequirement>
            {
                new() { EvidenceType = "Code_Review", Required = true, Frequency = "Continuous", Description = "Secure code review process" },
                new() { EvidenceType = "Vulnerability_Scan", Required = true, Frequency = "Weekly", Description = "Application vulnerability scanning" },
                new() { EvidenceType = "SDLC_Doc", Required = true, Frequency = "Annual", Description = "Secure SDLC documentation" },
                new() { EvidenceType = "SOC2_Report", Required = false, Frequency = "Annual", Description = "SOC 2 Type II (if SaaS)" }
            },
            ScoringWeights = new Dictionary<string, double>
            {
                ["Secure Development"] = 0.35,
                ["Data Security"] = 0.25,
                ["Access Control"] = 0.20,
                ["Operations Security"] = 0.20
            },
            ImplementationGuidance = "Focus on secure SDLC and application security. Consider SOC 2 for enterprise clients.",
            EstimatedImplementationMonths = 6,
            RegulatoryDeadlines = new List<string> { "NCA ECC: Within 6 months", "PDPL: Immediate" }
        }
    };

    public ExpertFrameworkMappingService(IUnitOfWork unitOfWork, ILogger<ExpertFrameworkMappingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get the full compliance blueprint for a sector
    /// </summary>
    public SectorComplianceBlueprint GetSectorBlueprint(string sector)
    {
        // Normalize sector name
        var normalizedSector = NormalizeSectorName(sector);

        if (SectorBlueprints.TryGetValue(normalizedSector, out var blueprint))
        {
            return blueprint;
        }

        // Default to Technology blueprint if sector not found
        _logger.LogWarning("Sector '{Sector}' not found, using Technology blueprint", sector);
        return SectorBlueprints["Technology"];
    }

    /// <summary>
    /// Get applicable frameworks for an organization based on profile
    /// </summary>
    public List<FrameworkMapping> GetApplicableFrameworks(OrganizationProfile profile)
    {
        var blueprint = GetSectorBlueprint(profile.Sector);
        var frameworks = new List<FrameworkMapping>(blueprint.ApplicableFrameworks);

        // Add additional frameworks based on profile attributes
        if (profile.IsCriticalInfrastructure)
        {
            if (!frameworks.Any(f => f.Code == "NCA-CSCC"))
            {
                frameworks.Add(new FrameworkMapping
                {
                    Code = "NCA-CSCC",
                    Name = "NCA Critical Systems Controls",
                    Priority = 1,
                    Mandatory = true,
                    Reason = "Organization classified as critical infrastructure"
                });
            }
        }

        if (profile.DataTypes?.Contains("Personal", StringComparison.OrdinalIgnoreCase) == true ||
            profile.DataTypes?.Contains("Customer", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (!frameworks.Any(f => f.Code == "PDPL"))
            {
                frameworks.Add(new FrameworkMapping
                {
                    Code = "PDPL",
                    Name = "Personal Data Protection Law",
                    Priority = 2,
                    Mandatory = true,
                    Reason = "Organization processes personal data"
                });
            }
        }

        // Sort by priority
        return frameworks.OrderBy(f => f.Priority).ToList();
    }

    /// <summary>
    /// Get evidence requirements for a sector
    /// </summary>
    public List<SectorEvidenceRequirement> GetEvidenceRequirements(string sector)
    {
        var blueprint = GetSectorBlueprint(sector);
        return blueprint.EvidenceRequirements;
    }

    /// <summary>
    /// Get scoring weights for a sector
    /// </summary>
    public Dictionary<string, double> GetScoringWeights(string sector)
    {
        var blueprint = GetSectorBlueprint(sector);
        return blueprint.ScoringWeights;
    }

    /// <summary>
    /// Get implementation guidance for a sector
    /// </summary>
    public SectorImplementationGuidance GetImplementationGuidance(string sector)
    {
        var blueprint = GetSectorBlueprint(sector);
        return new SectorImplementationGuidance
        {
            Guidance = blueprint.ImplementationGuidance,
            EstimatedMonths = blueprint.EstimatedImplementationMonths,
            RegulatoryDeadlines = blueprint.RegulatoryDeadlines,
            ControlPriorities = blueprint.ControlPriorities
        };
    }

    /// <summary>
    /// Apply expert mapping to organization and generate derived scope
    /// </summary>
    public async Task<ExpertMappingResult> ApplyExpertMappingAsync(Guid tenantId, string userId)
    {
        var profile = await _unitOfWork.OrganizationProfiles
            .Query()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId);

        if (profile == null)
            throw new InvalidOperationException($"Organization profile not found for tenant {tenantId}");

        var blueprint = GetSectorBlueprint(profile.Sector);
        var frameworks = GetApplicableFrameworks(profile);

        // Create TenantBaselines for each applicable framework
        foreach (var framework in frameworks.Where(f => f.Mandatory))
        {
            var existing = await _unitOfWork.TenantBaselines
                .Query()
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.BaselineCode == framework.Code);

            if (existing == null)
            {
                var baseline = new TenantBaseline
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    BaselineCode = framework.Code,
                    BaselineName = framework.Name,
                    ReasonJson = JsonSerializer.Serialize(new
                    {
                        Sector = profile.Sector,
                        Reason = framework.Reason,
                        Priority = framework.Priority,
                        AppliedAt = DateTime.UtcNow
                    }),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId
                };
                await _unitOfWork.TenantBaselines.AddAsync(baseline);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Applied expert mapping for tenant {TenantId}: {Sector} -> {FrameworkCount} frameworks",
            tenantId, profile.Sector, frameworks.Count);

        return new ExpertMappingResult
        {
            TenantId = tenantId,
            Sector = profile.Sector,
            Blueprint = blueprint,
            ApplicableFrameworks = frameworks,
            ImplementationGuidance = GetImplementationGuidance(profile.Sector),
            AppliedAt = DateTime.UtcNow
        };
    }

    private string NormalizeSectorName(string sector)
    {
        if (string.IsNullOrWhiteSpace(sector)) return "Technology";

        var lower = sector.ToLower();

        if (lower.Contains("bank") || lower.Contains("financ") || lower.Contains("مصرف") || lower.Contains("مال"))
            return "Banking";
        if (lower.Contains("health") || lower.Contains("medical") || lower.Contains("hospital") || lower.Contains("صح") || lower.Contains("طب"))
            return "Healthcare";
        if (lower.Contains("government") || lower.Contains("public") || lower.Contains("حكوم") || lower.Contains("عام"))
            return "Government";
        if (lower.Contains("telecom") || lower.Contains("communication") || lower.Contains("اتصال"))
            return "Telecom";
        if (lower.Contains("energy") || lower.Contains("utility") || lower.Contains("oil") || lower.Contains("gas") || lower.Contains("طاق") || lower.Contains("نفط"))
            return "Energy";
        if (lower.Contains("retail") || lower.Contains("commerce") || lower.Contains("تجز") || lower.Contains("تجار"))
            return "Retail";
        if (lower.Contains("tech") || lower.Contains("software") || lower.Contains("تقن") || lower.Contains("برمج"))
            return "Technology";

        return "Technology"; // Default
    }
}

#region DTOs and Models

public class SectorComplianceBlueprint
{
    public string SectorCode { get; set; } = string.Empty;
    public string SectorName { get; set; } = string.Empty;
    public string SectorNameAr { get; set; } = string.Empty;
    public List<FrameworkMapping> ApplicableFrameworks { get; set; } = new();
    public Dictionary<string, int> ControlPriorities { get; set; } = new();
    public List<SectorEvidenceRequirement> EvidenceRequirements { get; set; } = new();
    public Dictionary<string, double> ScoringWeights { get; set; } = new();
    public string ImplementationGuidance { get; set; } = string.Empty;
    public int EstimatedImplementationMonths { get; set; }
    public List<string> RegulatoryDeadlines { get; set; } = new();
}

public class FrameworkMapping
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool Mandatory { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SectorEvidenceRequirement
{
    public string EvidenceType { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SectorImplementationGuidance
{
    public string Guidance { get; set; } = string.Empty;
    public int EstimatedMonths { get; set; }
    public List<string> RegulatoryDeadlines { get; set; } = new();
    public Dictionary<string, int> ControlPriorities { get; set; } = new();
}

public class ExpertMappingResult
{
    public Guid TenantId { get; set; }
    public string Sector { get; set; } = string.Empty;
    public SectorComplianceBlueprint Blueprint { get; set; } = new();
    public List<FrameworkMapping> ApplicableFrameworks { get; set; } = new();
    public SectorImplementationGuidance ImplementationGuidance { get; set; } = new();
    public DateTime AppliedAt { get; set; }
}

#endregion
