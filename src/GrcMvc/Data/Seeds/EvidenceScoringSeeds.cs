using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds evidence scoring criteria and sector-framework index for fast lookup
/// Layer 1: Global (Platform) - Used by all tenants
/// </summary>
public static class EvidenceScoringSeeds
{
    public static async Task SeedEvidenceScoringCriteriaAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            if (await context.EvidenceScoringCriteria.AnyAsync())
            {
                logger.LogInformation("✅ Evidence scoring criteria already exist. Skipping seed.");
                return;
            }

            var criteria = GetEvidenceScoringCriteria();
            await context.EvidenceScoringCriteria.AddRangeAsync(criteria);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Successfully seeded {criteria.Count} evidence scoring criteria");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding evidence scoring criteria");
            throw;
        }
    }

    public static async Task SeedSectorFrameworkIndexAsync(GrcDbContext context, ILogger logger)
    {
        try
        {
            if (await context.SectorFrameworkIndexes.AnyAsync())
            {
                logger.LogInformation("✅ Sector framework index already exists. Skipping seed.");
                return;
            }

            var indexes = GetSectorFrameworkIndexes();
            await context.SectorFrameworkIndexes.AddRangeAsync(indexes);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Successfully seeded {indexes.Count} sector framework mappings");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error seeding sector framework index");
            throw;
        }
    }

    private static List<EvidenceScoringCriteria> GetEvidenceScoringCriteria()
    {
        return new List<EvidenceScoringCriteria>
        {
            // Document-based evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "POLICY_DOC", EvidenceTypeName = "Policy Document", DescriptionEn = "Formal policy document approved by management", Category = "Document", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "PROCEDURE_DOC", EvidenceTypeName = "Procedure Documentation", DescriptionEn = "Documented operational procedures", Category = "Document", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "TRAINING_RECORDS", EvidenceTypeName = "Training Records", DescriptionEn = "Employee training completion records", Category = "Document", BaseScore = 50, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.xlsx,.csv", IsActive = true, DisplayOrder = 3 },
            
            // Configuration evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "CONFIG_STANDARDS", EvidenceTypeName = "Configuration Standards", DescriptionEn = "System configuration baseline documentation", Category = "Config", BaseScore = 40, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Quarterly", DefaultValidityDays = 90, AllowedFileTypes = ".pdf,.xlsx,.json,.xml", IsActive = true, DisplayOrder = 4 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "ACCESS_CONTROL_LISTS", EvidenceTypeName = "Access Control Lists", DescriptionEn = "User access permissions and role assignments", Category = "Config", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, CollectionFrequency = "Quarterly", DefaultValidityDays = 90, AllowedFileTypes = ".pdf,.xlsx,.csv,.json", IsActive = true, DisplayOrder = 5 },
            
            // Report-based evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "SOC2_REPORT", EvidenceTypeName = "SOC 2 Type II Report", DescriptionEn = "Third-party SOC 2 audit report", Category = "Report", BaseScore = 60, MaxScore = 100, MinimumScore = 85, RequiresApproval = true, RequiresExpiry = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf", IsActive = true, DisplayOrder = 6 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "PENETRATION_TEST", EvidenceTypeName = "Penetration Test Results", DescriptionEn = "External penetration test report", Category = "Report", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, RequiresExpiry = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf", IsActive = true, DisplayOrder = 7 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "VULNERABILITY_SCAN", EvidenceTypeName = "Vulnerability Scan Report", DescriptionEn = "Automated vulnerability scan results", Category = "Report", BaseScore = 45, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Monthly", DefaultValidityDays = 30, AllowedFileTypes = ".pdf,.html,.csv", IsActive = true, DisplayOrder = 8 },
            
            // Audit-based evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "AUDIT_FINDINGS", EvidenceTypeName = "Audit Findings", DescriptionEn = "Internal or external audit findings", Category = "Report", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 9 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "COMPLIANCE_REPORTS", EvidenceTypeName = "Compliance Reports", DescriptionEn = "Periodic compliance status reports", Category = "Report", BaseScore = 45, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Quarterly", DefaultValidityDays = 90, AllowedFileTypes = ".pdf,.xlsx", IsActive = true, DisplayOrder = 10 },
            
            // Monitoring evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "MONITORING_REPORTS", EvidenceTypeName = "Monitoring Reports", DescriptionEn = "Security monitoring and alerting reports", Category = "Report", BaseScore = 50, MaxScore = 100, MinimumScore = 75, RequiresApproval = false, CollectionFrequency = "Monthly", DefaultValidityDays = 30, AllowedFileTypes = ".pdf,.xlsx,.csv", IsActive = true, DisplayOrder = 11 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "LOG_SAMPLES", EvidenceTypeName = "Log Samples", DescriptionEn = "Sample log entries demonstrating control operation", Category = "Log", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = false, CollectionFrequency = "Quarterly", DefaultValidityDays = 90, AllowedFileTypes = ".txt,.log,.csv,.json", IsActive = true, DisplayOrder = 12 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "ALERT_RECORDS", EvidenceTypeName = "Alert Records", DescriptionEn = "Security alert and notification records", Category = "Log", BaseScore = 45, MaxScore = 100, MinimumScore = 70, RequiresApproval = false, CollectionFrequency = "Monthly", DefaultValidityDays = 30, AllowedFileTypes = ".pdf,.xlsx,.csv", IsActive = true, DisplayOrder = 13 },
            
            // Incident-based evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "INCIDENT_REPORTS", EvidenceTypeName = "Incident Reports", DescriptionEn = "Security incident documentation", Category = "Report", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Continuous", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 14 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "REMEDIATION_LOGS", EvidenceTypeName = "Remediation Logs", DescriptionEn = "Issue remediation tracking and completion", Category = "Log", BaseScore = 45, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Continuous", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.xlsx,.csv", IsActive = true, DisplayOrder = 15 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "ROOT_CAUSE_ANALYSIS", EvidenceTypeName = "Root Cause Analysis", DescriptionEn = "Root cause analysis documentation", Category = "Document", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, CollectionFrequency = "Continuous", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 16 },
            
            // Review-based evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "ACCESS_REVIEW", EvidenceTypeName = "Access Review Documentation", DescriptionEn = "User access review completion records", Category = "Document", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, CollectionFrequency = "Quarterly", DefaultValidityDays = 90, AllowedFileTypes = ".pdf,.xlsx", IsActive = true, DisplayOrder = 17 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "IMPROVEMENT_PLANS", EvidenceTypeName = "Improvement Plans", DescriptionEn = "Continuous improvement action plans", Category = "Document", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx,.xlsx", IsActive = true, DisplayOrder = 18 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "VERIFICATION_RECORDS", EvidenceTypeName = "Verification Records", DescriptionEn = "Control effectiveness verification records", Category = "Document", BaseScore = 50, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.xlsx", IsActive = true, DisplayOrder = 19 },
            
            // Business continuity evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "BCP_TEST", EvidenceTypeName = "Business Continuity Plan Test", DescriptionEn = "BCP/DR testing documentation", Category = "Report", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, RequiresExpiry = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 20 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "DR_TEST", EvidenceTypeName = "Disaster Recovery Test", DescriptionEn = "Disaster recovery testing results", Category = "Report", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, RequiresExpiry = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx", IsActive = true, DisplayOrder = 21 },
            
            // Screenshot-based evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "SCREENSHOT", EvidenceTypeName = "Screenshot Evidence", DescriptionEn = "System configuration or status screenshots", Category = "Screenshot", BaseScore = 30, MaxScore = 80, MinimumScore = 60, RequiresApproval = false, CollectionFrequency = "OnDemand", DefaultValidityDays = 90, AllowedFileTypes = ".png,.jpg,.jpeg,.gif", IsActive = true, DisplayOrder = 22 },
            
            // Certificate evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "CERTIFICATE", EvidenceTypeName = "Certification Document", DescriptionEn = "ISO, SOC, or other certification document", Category = "Certificate", BaseScore = 70, MaxScore = 100, MinimumScore = 90, RequiresApproval = true, RequiresExpiry = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf", IsActive = true, DisplayOrder = 23 },
            
            // Data protection evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "DATA_INVENTORY", EvidenceTypeName = "Data Inventory", DescriptionEn = "Data asset inventory and classification", Category = "Document", BaseScore = 45, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.xlsx,.csv", IsActive = true, DisplayOrder = 24 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "PRIVACY_NOTICE", EvidenceTypeName = "Privacy Notice", DescriptionEn = "Published privacy notice and consent mechanisms", Category = "Document", BaseScore = 40, MaxScore = 100, MinimumScore = 70, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.doc,.docx,.html", IsActive = true, DisplayOrder = 25 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "CONSENT_MANAGEMENT", EvidenceTypeName = "Consent Management Records", DescriptionEn = "Data subject consent tracking", Category = "Document", BaseScore = 45, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Continuous", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.xlsx,.csv", IsActive = true, DisplayOrder = 26 },
            
            // Third-party evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "VENDOR_ASSESSMENT", EvidenceTypeName = "Vendor Security Assessment", DescriptionEn = "Third-party vendor security evaluation", Category = "Report", BaseScore = 50, MaxScore = 100, MinimumScore = 75, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf,.xlsx", IsActive = true, DisplayOrder = 27 },
            
            // Network/OT evidence
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "NETWORK_AUDIT", EvidenceTypeName = "Network Security Audit", DescriptionEn = "Network security assessment results", Category = "Report", BaseScore = 50, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, CollectionFrequency = "Quarterly", DefaultValidityDays = 90, AllowedFileTypes = ".pdf", IsActive = true, DisplayOrder = 28 },
            new() { Id = Guid.NewGuid(), EvidenceTypeCode = "OT_ASSESSMENT", EvidenceTypeName = "OT/ICS Security Assessment", DescriptionEn = "Operational technology security evaluation", Category = "Report", BaseScore = 55, MaxScore = 100, MinimumScore = 80, RequiresApproval = true, CollectionFrequency = "Annual", DefaultValidityDays = 365, AllowedFileTypes = ".pdf", ApplicableSectors = "Energy|Telecom|Government", IsActive = true, DisplayOrder = 29 },
        };
    }

    private static List<SectorFrameworkIndex> GetSectorFrameworkIndexes()
    {
        var indexes = new List<SectorFrameworkIndex>();
        int order = 1;

        // BANKING SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "BANKING", SectorNameEn = "Banking & Financial Services", SectorNameAr = "الخدمات المصرفية والمالية", OrgType = "BANK", OrgTypeNameEn = "Commercial Bank", FrameworkCode = "SAMA-CSF", FrameworkNameEn = "SAMA Cybersecurity Framework", Priority = 1, IsMandatory = true, ReasonEn = "Required for all SAMA-regulated financial institutions", ControlCount = 156, ScoringWeight = 0.35, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "BANKING", SectorNameEn = "Banking & Financial Services", SectorNameAr = "الخدمات المصرفية والمالية", OrgType = "BANK", OrgTypeNameEn = "Commercial Bank", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 2, IsMandatory = true, ReasonEn = "National requirement for critical infrastructure", ControlCount = 114, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "BANKING", SectorNameEn = "Banking & Financial Services", SectorNameAr = "الخدمات المصرفية والمالية", OrgType = "BANK", OrgTypeNameEn = "Commercial Bank", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Customer data processing requirements", ControlCount = 45, ScoringWeight = 0.20, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "BANKING", SectorNameEn = "Banking & Financial Services", SectorNameAr = "الخدمات المصرفية والمالية", OrgType = "BANK", OrgTypeNameEn = "Commercial Bank", FrameworkCode = "SAMA-AML", FrameworkNameEn = "SAMA Anti-Money Laundering", Priority = 4, IsMandatory = true, ReasonEn = "Financial crime prevention", ControlCount = 167, ScoringWeight = 0.15, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "BANKING", SectorNameEn = "Banking & Financial Services", SectorNameAr = "الخدمات المصرفية والمالية", OrgType = "BANK", OrgTypeNameEn = "Commercial Bank", FrameworkCode = "PCI-DSS", FrameworkNameEn = "Payment Card Industry DSS", Priority = 5, IsMandatory = false, ReasonEn = "If processing card payments", ControlCount = 362, ScoringWeight = 0.05, EstimatedImplementationDays = 180, DisplayOrder = order++ },
        });

        // HEALTHCARE SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HEALTHCARE", SectorNameEn = "Healthcare & Medical", SectorNameAr = "الرعاية الصحية والطبية", OrgType = "HOSPITAL", OrgTypeNameEn = "Hospital", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Healthcare is critical infrastructure", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HEALTHCARE", SectorNameEn = "Healthcare & Medical", SectorNameAr = "الرعاية الصحية والطبية", OrgType = "HOSPITAL", OrgTypeNameEn = "Hospital", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 1, IsMandatory = true, ReasonEn = "Patient health data is sensitive personal data", ControlCount = 45, ScoringWeight = 0.35, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HEALTHCARE", SectorNameEn = "Healthcare & Medical", SectorNameAr = "الرعاية الصحية والطبية", OrgType = "HOSPITAL", OrgTypeNameEn = "Hospital", FrameworkCode = "CBAHI-HAS", FrameworkNameEn = "CBAHI Healthcare Accreditation Standards", Priority = 2, IsMandatory = true, ReasonEn = "Healthcare facility accreditation", ControlCount = 298, ScoringWeight = 0.20, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HEALTHCARE", SectorNameEn = "Healthcare & Medical", SectorNameAr = "الرعاية الصحية والطبية", OrgType = "HOSPITAL", OrgTypeNameEn = "Hospital", FrameworkCode = "MOH-HIS", FrameworkNameEn = "MOH Health Information Standards", Priority = 3, IsMandatory = true, ReasonEn = "Health information exchange requirements", ControlCount = 112, ScoringWeight = 0.15, EstimatedImplementationDays = 120, DisplayOrder = order++ },
        });

        // GOVERNMENT SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "GOVERNMENT", SectorNameEn = "Government & Public Sector", SectorNameAr = "القطاع الحكومي والعام", OrgType = "MINISTRY", OrgTypeNameEn = "Ministry", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Mandatory for all government entities", ControlCount = 114, ScoringWeight = 0.35, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "GOVERNMENT", SectorNameEn = "Government & Public Sector", SectorNameAr = "القطاع الحكومي والعام", OrgType = "MINISTRY", OrgTypeNameEn = "Ministry", FrameworkCode = "NCA-CSCC", FrameworkNameEn = "NCA Critical Systems Controls", Priority = 1, IsMandatory = true, ReasonEn = "Government systems are critical infrastructure", ControlCount = 85, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "GOVERNMENT", SectorNameEn = "Government & Public Sector", SectorNameAr = "القطاع الحكومي والعام", OrgType = "MINISTRY", OrgTypeNameEn = "Ministry", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Citizen data protection", ControlCount = 45, ScoringWeight = 0.20, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "GOVERNMENT", SectorNameEn = "Government & Public Sector", SectorNameAr = "القطاع الحكومي والعام", OrgType = "MINISTRY", OrgTypeNameEn = "Ministry", FrameworkCode = "DGA-CLOUD", FrameworkNameEn = "DGA Cloud First Policy", Priority = 2, IsMandatory = true, ReasonEn = "Government cloud adoption requirements", ControlCount = 78, ScoringWeight = 0.20, EstimatedImplementationDays = 90, DisplayOrder = order++ },
        });

        // TELECOM SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TELECOM", SectorNameEn = "Telecommunications", SectorNameAr = "الاتصالات", OrgType = "TELCO", OrgTypeNameEn = "Telecom Operator", FrameworkCode = "CST-CRF", FrameworkNameEn = "CST Cybersecurity Regulatory Framework", Priority = 1, IsMandatory = true, ReasonEn = "CST-regulated entity requirement", ControlCount = 125, ScoringWeight = 0.35, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TELECOM", SectorNameEn = "Telecommunications", SectorNameAr = "الاتصالات", OrgType = "TELCO", OrgTypeNameEn = "Telecom Operator", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Telecom is critical infrastructure", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TELECOM", SectorNameEn = "Telecommunications", SectorNameAr = "الاتصالات", OrgType = "TELCO", OrgTypeNameEn = "Telecom Operator", FrameworkCode = "NCA-CSCC", FrameworkNameEn = "NCA Critical Systems Controls", Priority = 2, IsMandatory = true, ReasonEn = "Critical national infrastructure", ControlCount = 85, ScoringWeight = 0.20, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TELECOM", SectorNameEn = "Telecommunications", SectorNameAr = "الاتصالات", OrgType = "TELCO", OrgTypeNameEn = "Telecom Operator", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Subscriber data protection", ControlCount = 45, ScoringWeight = 0.15, EstimatedImplementationDays = 90, DisplayOrder = order++ },
        });

        // ENERGY SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "ENERGY", SectorNameEn = "Energy & Utilities", SectorNameAr = "الطاقة والمرافق", OrgType = "UTILITY", OrgTypeNameEn = "Utility Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Critical national infrastructure", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "ENERGY", SectorNameEn = "Energy & Utilities", SectorNameAr = "الطاقة والمرافق", OrgType = "UTILITY", OrgTypeNameEn = "Utility Company", FrameworkCode = "NCA-CSCC", FrameworkNameEn = "NCA Critical Systems Controls", Priority = 1, IsMandatory = true, ReasonEn = "OT/ICS systems protection", ControlCount = 85, ScoringWeight = 0.35, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "ENERGY", SectorNameEn = "Energy & Utilities", SectorNameAr = "الطاقة والمرافق", OrgType = "UTILITY", OrgTypeNameEn = "Utility Company", FrameworkCode = "HCIS", FrameworkNameEn = "HCIS Industrial Cybersecurity", Priority = 2, IsMandatory = true, ReasonEn = "Industrial control systems security", ControlCount = 95, ScoringWeight = 0.20, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "ENERGY", SectorNameEn = "Energy & Utilities", SectorNameAr = "الطاقة والمرافق", OrgType = "UTILITY", OrgTypeNameEn = "Utility Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Customer data protection", ControlCount = 45, ScoringWeight = 0.15, EstimatedImplementationDays = 90, DisplayOrder = order++ },
        });

        // RETAIL SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "RETAIL", SectorNameEn = "Retail & E-Commerce", SectorNameAr = "التجزئة والتجارة الإلكترونية", OrgType = "ECOMMERCE", OrgTypeNameEn = "E-Commerce Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 1, IsMandatory = true, ReasonEn = "Customer data processing", ControlCount = 45, ScoringWeight = 0.30, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "RETAIL", SectorNameEn = "Retail & E-Commerce", SectorNameAr = "التجزئة والتجارة الإلكترونية", OrgType = "ECOMMERCE", OrgTypeNameEn = "E-Commerce Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 2, IsMandatory = true, ReasonEn = "Basic cybersecurity requirements", ControlCount = 114, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "RETAIL", SectorNameEn = "Retail & E-Commerce", SectorNameAr = "التجزئة والتجارة الإلكترونية", OrgType = "ECOMMERCE", OrgTypeNameEn = "E-Commerce Company", FrameworkCode = "PCI-DSS", FrameworkNameEn = "Payment Card Industry DSS", Priority = 2, IsMandatory = true, ReasonEn = "Card payment processing", ControlCount = 362, ScoringWeight = 0.35, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "RETAIL", SectorNameEn = "Retail & E-Commerce", SectorNameAr = "التجزئة والتجارة الإلكترونية", OrgType = "ECOMMERCE", OrgTypeNameEn = "E-Commerce Company", FrameworkCode = "MOCI-ECOM", FrameworkNameEn = "MOCI E-Commerce Regulations", Priority = 3, IsMandatory = true, ReasonEn = "E-commerce compliance", ControlCount = 85, ScoringWeight = 0.10, EstimatedImplementationDays = 60, DisplayOrder = order++ },
        });

        // TECHNOLOGY SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TECHNOLOGY", SectorNameEn = "Technology & Software", SectorNameAr = "التقنية والبرمجيات", OrgType = "SAAS", OrgTypeNameEn = "SaaS Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Basic cybersecurity requirements", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TECHNOLOGY", SectorNameEn = "Technology & Software", SectorNameAr = "التقنية والبرمجيات", OrgType = "SAAS", OrgTypeNameEn = "SaaS Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 2, IsMandatory = true, ReasonEn = "User data processing", ControlCount = 45, ScoringWeight = 0.25, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TECHNOLOGY", SectorNameEn = "Technology & Software", SectorNameAr = "التقنية والبرمجيات", OrgType = "SAAS", OrgTypeNameEn = "SaaS Company", FrameworkCode = "OWASP-ASVS", FrameworkNameEn = "OWASP Application Security Verification Standard", Priority = 3, IsMandatory = false, ReasonEn = "Application security best practices", ControlCount = 286, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TECHNOLOGY", SectorNameEn = "Technology & Software", SectorNameAr = "التقنية والبرمجيات", OrgType = "SAAS", OrgTypeNameEn = "SaaS Company", FrameworkCode = "ISO-27001", FrameworkNameEn = "ISO 27001 Information Security", Priority = 4, IsMandatory = false, ReasonEn = "Recommended for enterprise clients", ControlCount = 93, ScoringWeight = 0.20, EstimatedImplementationDays = 180, DisplayOrder = order++ },
        });

        // INSURANCE SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "INSURANCE", SectorNameEn = "Insurance", SectorNameAr = "التأمين", OrgType = "INSURER", OrgTypeNameEn = "Insurance Company", FrameworkCode = "SAMA-CSF", FrameworkNameEn = "SAMA Cybersecurity Framework", Priority = 1, IsMandatory = true, ReasonEn = "Required for SAMA-regulated insurers", ControlCount = 156, ScoringWeight = 0.35, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "INSURANCE", SectorNameEn = "Insurance", SectorNameAr = "التأمين", OrgType = "INSURER", OrgTypeNameEn = "Insurance Company", FrameworkCode = "SAMA-INSURANCE", FrameworkNameEn = "SAMA Insurance Regulations", Priority = 1, IsMandatory = true, ReasonEn = "Insurance-specific SAMA requirements", ControlCount = 78, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "INSURANCE", SectorNameEn = "Insurance", SectorNameAr = "التأمين", OrgType = "INSURER", OrgTypeNameEn = "Insurance Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 2, IsMandatory = true, ReasonEn = "National cybersecurity requirement", ControlCount = 114, ScoringWeight = 0.20, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "INSURANCE", SectorNameEn = "Insurance", SectorNameAr = "التأمين", OrgType = "INSURER", OrgTypeNameEn = "Insurance Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Policyholder data protection", ControlCount = 45, ScoringWeight = 0.15, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "INSURANCE", SectorNameEn = "Insurance", SectorNameAr = "التأمين", OrgType = "INSURER", OrgTypeNameEn = "Insurance Company", FrameworkCode = "SAMA-AML", FrameworkNameEn = "SAMA Anti-Money Laundering", Priority = 4, IsMandatory = true, ReasonEn = "Financial crime prevention", ControlCount = 167, ScoringWeight = 0.05, EstimatedImplementationDays = 120, DisplayOrder = order++ },
        });

        // EDUCATION SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "EDUCATION", SectorNameEn = "Education", SectorNameAr = "التعليم", OrgType = "UNIVERSITY", OrgTypeNameEn = "University", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Public sector cybersecurity requirement", ControlCount = 114, ScoringWeight = 0.35, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "EDUCATION", SectorNameEn = "Education", SectorNameAr = "التعليم", OrgType = "UNIVERSITY", OrgTypeNameEn = "University", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 1, IsMandatory = true, ReasonEn = "Student and faculty data protection", ControlCount = 45, ScoringWeight = 0.30, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "EDUCATION", SectorNameEn = "Education", SectorNameAr = "التعليم", OrgType = "UNIVERSITY", OrgTypeNameEn = "University", FrameworkCode = "MOE-EDUCATION", FrameworkNameEn = "MOE Education Standards", Priority = 2, IsMandatory = true, ReasonEn = "Ministry of Education compliance", ControlCount = 145, ScoringWeight = 0.25, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "EDUCATION", SectorNameEn = "Education", SectorNameAr = "التعليم", OrgType = "UNIVERSITY", OrgTypeNameEn = "University", FrameworkCode = "ISO-27001", FrameworkNameEn = "ISO 27001 Information Security", Priority = 3, IsMandatory = false, ReasonEn = "International research collaboration", ControlCount = 93, ScoringWeight = 0.10, EstimatedImplementationDays = 180, DisplayOrder = order++ },
        });

        // ================================================================
        // NEW 9 SECTORS (Total: 18 sectors)
        // ================================================================

        // TRANSPORTATION SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TRANSPORTATION", SectorNameEn = "Transportation & Logistics", SectorNameAr = "النقل والخدمات اللوجستية", OrgType = "LOGISTICS", OrgTypeNameEn = "Logistics Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Critical infrastructure protection", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TRANSPORTATION", SectorNameEn = "Transportation & Logistics", SectorNameAr = "النقل والخدمات اللوجستية", OrgType = "LOGISTICS", OrgTypeNameEn = "Logistics Company", FrameworkCode = "TGA-TRANSPORT", FrameworkNameEn = "TGA Transport Regulations", Priority = 1, IsMandatory = true, ReasonEn = "Transport authority compliance", ControlCount = 125, ScoringWeight = 0.35, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TRANSPORTATION", SectorNameEn = "Transportation & Logistics", SectorNameAr = "النقل والخدمات اللوجستية", OrgType = "LOGISTICS", OrgTypeNameEn = "Logistics Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 2, IsMandatory = true, ReasonEn = "Customer and shipment data protection", ControlCount = 45, ScoringWeight = 0.20, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "TRANSPORTATION", SectorNameEn = "Transportation & Logistics", SectorNameAr = "النقل والخدمات اللوجستية", OrgType = "LOGISTICS", OrgTypeNameEn = "Logistics Company", FrameworkCode = "ISO-27001", FrameworkNameEn = "ISO 27001 Information Security", Priority = 3, IsMandatory = false, ReasonEn = "International logistics partners", ControlCount = 93, ScoringWeight = 0.15, EstimatedImplementationDays = 180, DisplayOrder = order++ },
        });

        // CONSTRUCTION SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "CONSTRUCTION", SectorNameEn = "Construction & Engineering", SectorNameAr = "البناء والتشييد والهندسة", OrgType = "CONTRACTOR", OrgTypeNameEn = "General Contractor", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Government project requirements", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "CONSTRUCTION", SectorNameEn = "Construction & Engineering", SectorNameAr = "البناء والتشييد والهندسة", OrgType = "CONTRACTOR", OrgTypeNameEn = "General Contractor", FrameworkCode = "SASO-SAFETY", FrameworkNameEn = "SASO Safety Standards", Priority = 1, IsMandatory = true, ReasonEn = "Occupational safety compliance", ControlCount = 189, ScoringWeight = 0.40, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "CONSTRUCTION", SectorNameEn = "Construction & Engineering", SectorNameAr = "البناء والتشييد والهندسة", OrgType = "CONTRACTOR", OrgTypeNameEn = "General Contractor", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 2, IsMandatory = true, ReasonEn = "Worker and client data protection", ControlCount = 45, ScoringWeight = 0.15, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "CONSTRUCTION", SectorNameEn = "Construction & Engineering", SectorNameAr = "البناء والتشييد والهندسة", OrgType = "CONTRACTOR", OrgTypeNameEn = "General Contractor", FrameworkCode = "ISO-45001", FrameworkNameEn = "ISO 45001 Occupational Health & Safety", Priority = 3, IsMandatory = false, ReasonEn = "International safety standards", ControlCount = 68, ScoringWeight = 0.15, EstimatedImplementationDays = 150, DisplayOrder = order++ },
        });

        // MANUFACTURING SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MANUFACTURING", SectorNameEn = "Manufacturing & Industry", SectorNameAr = "الصناعات التحويلية", OrgType = "FACTORY", OrgTypeNameEn = "Manufacturing Factory", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Industrial cybersecurity", ControlCount = 114, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MANUFACTURING", SectorNameEn = "Manufacturing & Industry", SectorNameAr = "الصناعات التحويلية", OrgType = "FACTORY", OrgTypeNameEn = "Manufacturing Factory", FrameworkCode = "SASO-QUALITY", FrameworkNameEn = "SASO Quality Standards", Priority = 1, IsMandatory = true, ReasonEn = "Product quality compliance", ControlCount = 245, ScoringWeight = 0.35, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MANUFACTURING", SectorNameEn = "Manufacturing & Industry", SectorNameAr = "الصناعات التحويلية", OrgType = "FACTORY", OrgTypeNameEn = "Manufacturing Factory", FrameworkCode = "ISO-9001", FrameworkNameEn = "ISO 9001 Quality Management", Priority = 2, IsMandatory = false, ReasonEn = "Quality management system", ControlCount = 84, ScoringWeight = 0.20, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MANUFACTURING", SectorNameEn = "Manufacturing & Industry", SectorNameAr = "الصناعات التحويلية", OrgType = "FACTORY", OrgTypeNameEn = "Manufacturing Factory", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Employee data protection", ControlCount = 45, ScoringWeight = 0.10, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MANUFACTURING", SectorNameEn = "Manufacturing & Industry", SectorNameAr = "الصناعات التحويلية", OrgType = "FACTORY", OrgTypeNameEn = "Manufacturing Factory", FrameworkCode = "ISO-14001", FrameworkNameEn = "ISO 14001 Environmental Management", Priority = 4, IsMandatory = false, ReasonEn = "Environmental compliance", ControlCount = 72, ScoringWeight = 0.10, EstimatedImplementationDays = 150, DisplayOrder = order++ },
        });

        // REAL ESTATE SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "REAL_ESTATE", SectorNameEn = "Real Estate", SectorNameAr = "العقارات", OrgType = "DEVELOPER", OrgTypeNameEn = "Real Estate Developer", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Property data protection", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "REAL_ESTATE", SectorNameEn = "Real Estate", SectorNameAr = "العقارات", OrgType = "DEVELOPER", OrgTypeNameEn = "Real Estate Developer", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 1, IsMandatory = true, ReasonEn = "Buyer and tenant data protection", ControlCount = 45, ScoringWeight = 0.35, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "REAL_ESTATE", SectorNameEn = "Real Estate", SectorNameAr = "العقارات", OrgType = "DEVELOPER", OrgTypeNameEn = "Real Estate Developer", FrameworkCode = "SAMA-AML", FrameworkNameEn = "SAMA Anti-Money Laundering", Priority = 2, IsMandatory = true, ReasonEn = "Real estate AML requirements", ControlCount = 167, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "REAL_ESTATE", SectorNameEn = "Real Estate", SectorNameAr = "العقارات", OrgType = "DEVELOPER", OrgTypeNameEn = "Real Estate Developer", FrameworkCode = "ISO-27001", FrameworkNameEn = "ISO 27001 Information Security", Priority = 3, IsMandatory = false, ReasonEn = "International investors", ControlCount = 93, ScoringWeight = 0.10, EstimatedImplementationDays = 180, DisplayOrder = order++ },
        });

        // HOSPITALITY SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HOSPITALITY", SectorNameEn = "Hospitality & Tourism", SectorNameAr = "الضيافة والسياحة", OrgType = "HOTEL", OrgTypeNameEn = "Hotel", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Guest data protection", ControlCount = 114, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HOSPITALITY", SectorNameEn = "Hospitality & Tourism", SectorNameAr = "الضيافة والسياحة", OrgType = "HOTEL", OrgTypeNameEn = "Hotel", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 1, IsMandatory = true, ReasonEn = "Guest personal data protection", ControlCount = 45, ScoringWeight = 0.35, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HOSPITALITY", SectorNameEn = "Hospitality & Tourism", SectorNameAr = "الضيافة والسياحة", OrgType = "HOTEL", OrgTypeNameEn = "Hotel", FrameworkCode = "PCI-DSS", FrameworkNameEn = "Payment Card Industry DSS", Priority = 2, IsMandatory = true, ReasonEn = "Payment card processing", ControlCount = 362, ScoringWeight = 0.25, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "HOSPITALITY", SectorNameEn = "Hospitality & Tourism", SectorNameAr = "الضيافة والسياحة", OrgType = "HOTEL", OrgTypeNameEn = "Hotel", FrameworkCode = "ISO-22000", FrameworkNameEn = "ISO 22000 Food Safety", Priority = 3, IsMandatory = false, ReasonEn = "Food service safety", ControlCount = 85, ScoringWeight = 0.15, EstimatedImplementationDays = 150, DisplayOrder = order++ },
        });

        // MEDIA SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MEDIA", SectorNameEn = "Media & Entertainment", SectorNameAr = "الإعلام والترفيه", OrgType = "BROADCASTER", OrgTypeNameEn = "Broadcasting Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Content infrastructure protection", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MEDIA", SectorNameEn = "Media & Entertainment", SectorNameAr = "الإعلام والترفيه", OrgType = "BROADCASTER", OrgTypeNameEn = "Broadcasting Company", FrameworkCode = "GCAM-MEDIA", FrameworkNameEn = "GCAM Media Regulations", Priority = 1, IsMandatory = true, ReasonEn = "Media content compliance", ControlCount = 58, ScoringWeight = 0.35, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MEDIA", SectorNameEn = "Media & Entertainment", SectorNameAr = "الإعلام والترفيه", OrgType = "BROADCASTER", OrgTypeNameEn = "Broadcasting Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 2, IsMandatory = true, ReasonEn = "Audience data protection", ControlCount = 45, ScoringWeight = 0.20, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MEDIA", SectorNameEn = "Media & Entertainment", SectorNameAr = "الإعلام والترفيه", OrgType = "BROADCASTER", OrgTypeNameEn = "Broadcasting Company", FrameworkCode = "ISO-27001", FrameworkNameEn = "ISO 27001 Information Security", Priority = 3, IsMandatory = false, ReasonEn = "Content security", ControlCount = 93, ScoringWeight = 0.15, EstimatedImplementationDays = 180, DisplayOrder = order++ },
        });

        // AGRICULTURE SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "AGRICULTURE", SectorNameEn = "Agriculture & Food", SectorNameAr = "الزراعة والغذاء", OrgType = "FARM", OrgTypeNameEn = "Agricultural Farm", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Food supply chain protection", ControlCount = 114, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "AGRICULTURE", SectorNameEn = "Agriculture & Food", SectorNameAr = "الزراعة والغذاء", OrgType = "FARM", OrgTypeNameEn = "Agricultural Farm", FrameworkCode = "SFDA-GMP", FrameworkNameEn = "SFDA Good Manufacturing Practice", Priority = 1, IsMandatory = true, ReasonEn = "Food safety compliance", ControlCount = 125, ScoringWeight = 0.40, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "AGRICULTURE", SectorNameEn = "Agriculture & Food", SectorNameAr = "الزراعة والغذاء", OrgType = "FARM", OrgTypeNameEn = "Agricultural Farm", FrameworkCode = "ISO-22000", FrameworkNameEn = "ISO 22000 Food Safety", Priority = 2, IsMandatory = false, ReasonEn = "Food safety management", ControlCount = 85, ScoringWeight = 0.20, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "AGRICULTURE", SectorNameEn = "Agriculture & Food", SectorNameAr = "الزراعة والغذاء", OrgType = "FARM", OrgTypeNameEn = "Agricultural Farm", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Worker data protection", ControlCount = 45, ScoringWeight = 0.15, EstimatedImplementationDays = 90, DisplayOrder = order++ },
        });

        // MINING SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MINING", SectorNameEn = "Mining & Quarrying", SectorNameAr = "التعدين واستغلال المحاجر", OrgType = "MINE", OrgTypeNameEn = "Mining Company", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Industrial cybersecurity", ControlCount = 114, ScoringWeight = 0.25, EstimatedImplementationDays = 120, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MINING", SectorNameEn = "Mining & Quarrying", SectorNameAr = "التعدين واستغلال المحاجر", OrgType = "MINE", OrgTypeNameEn = "Mining Company", FrameworkCode = "NCA-CSCC", FrameworkNameEn = "NCA Critical Systems Controls", Priority = 1, IsMandatory = true, ReasonEn = "OT/ICS protection", ControlCount = 85, ScoringWeight = 0.30, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MINING", SectorNameEn = "Mining & Quarrying", SectorNameAr = "التعدين واستغلال المحاجر", OrgType = "MINE", OrgTypeNameEn = "Mining Company", FrameworkCode = "SASO-SAFETY", FrameworkNameEn = "SASO Safety Standards", Priority = 2, IsMandatory = true, ReasonEn = "Mining safety compliance", ControlCount = 189, ScoringWeight = 0.30, EstimatedImplementationDays = 150, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "MINING", SectorNameEn = "Mining & Quarrying", SectorNameAr = "التعدين واستغلال المحاجر", OrgType = "MINE", OrgTypeNameEn = "Mining Company", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 3, IsMandatory = true, ReasonEn = "Worker data protection", ControlCount = 45, ScoringWeight = 0.15, EstimatedImplementationDays = 90, DisplayOrder = order++ },
        });

        // PROFESSIONAL SERVICES SECTOR
        indexes.AddRange(new[]
        {
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "PROFESSIONAL_SERVICES", SectorNameEn = "Professional Services", SectorNameAr = "الخدمات المهنية", OrgType = "CONSULTING", OrgTypeNameEn = "Consulting Firm", FrameworkCode = "NCA-ECC", FrameworkNameEn = "NCA Essential Cybersecurity Controls", Priority = 1, IsMandatory = true, ReasonEn = "Client data protection", ControlCount = 114, ScoringWeight = 0.30, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "PROFESSIONAL_SERVICES", SectorNameEn = "Professional Services", SectorNameAr = "الخدمات المهنية", OrgType = "CONSULTING", OrgTypeNameEn = "Consulting Firm", FrameworkCode = "PDPL", FrameworkNameEn = "Personal Data Protection Law", Priority = 1, IsMandatory = true, ReasonEn = "Client and employee data protection", ControlCount = 45, ScoringWeight = 0.35, EstimatedImplementationDays = 90, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "PROFESSIONAL_SERVICES", SectorNameEn = "Professional Services", SectorNameAr = "الخدمات المهنية", OrgType = "CONSULTING", OrgTypeNameEn = "Consulting Firm", FrameworkCode = "ISO-27001", FrameworkNameEn = "ISO 27001 Information Security", Priority = 2, IsMandatory = false, ReasonEn = "Client trust and compliance", ControlCount = 93, ScoringWeight = 0.20, EstimatedImplementationDays = 180, DisplayOrder = order++ },
            new SectorFrameworkIndex { Id = Guid.NewGuid(), SectorCode = "PROFESSIONAL_SERVICES", SectorNameEn = "Professional Services", SectorNameAr = "الخدمات المهنية", OrgType = "CONSULTING", OrgTypeNameEn = "Consulting Firm", FrameworkCode = "ISO-9001", FrameworkNameEn = "ISO 9001 Quality Management", Priority = 3, IsMandatory = false, ReasonEn = "Service quality", ControlCount = 84, ScoringWeight = 0.15, EstimatedImplementationDays = 150, DisplayOrder = order++ },
        });

        return indexes;
    }
}
