using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Data.Seeds;

/// <summary>
/// Seeds control-to-evidence mappings
/// Links controls to their required evidence packs based on control domain/category
/// </summary>
public static class ControlEvidenceMappingSeeds
{
    public static async Task SeedAsync(GrcDbContext context, ILogger logger)
    {
        // Check if evidence packs already exist
        var existingCount = await context.EvidencePacks.CountAsync();
        if (existingCount >= 20)
        {
            logger.LogInformation("Evidence packs already seeded ({Count})", existingCount);
            return;
        }

        logger.LogInformation("Seeding evidence packs...");

        // Create base evidence packs
        var evidencePacks = CreateBaseEvidencePacks();
        context.EvidencePacks.AddRange(evidencePacks);
        await context.SaveChangesAsync();
        
        logger.LogInformation("Created {Count} base evidence packs", evidencePacks.Count);
    }

    private static List<EvidencePack> CreateBaseEvidencePacks()
    {
        return new List<EvidencePack>
        {
            // Governance & Organization
            CreateEvidencePack("EP_GOV_001", "Governance Documentation Pack",
                new[] { "Information Security Policy", "Security Organization Chart", "Roles & Responsibilities", "Board Meeting Minutes" },
                "Annual"),
            
            CreateEvidencePack("EP_GOV_002", "Policy Management Pack",
                new[] { "Policy Approval Records", "Policy Version History", "Policy Distribution Logs", "Acknowledgment Records" },
                "Annual"),

            // Identity & Access Management
            CreateEvidencePack("EP_IAM_001", "Access Control Pack",
                new[] { "User Access List", "Privileged Account List", "Access Review Reports", "Access Request Forms" },
                "Quarterly"),
            
            CreateEvidencePack("EP_IAM_002", "Authentication Pack",
                new[] { "MFA Configuration", "Password Policy Settings", "Authentication Logs", "Session Timeout Settings" },
                "Quarterly"),

            CreateEvidencePack("EP_IAM_003", "Privileged Access Pack",
                new[] { "PAM System Config", "Privileged Session Logs", "Emergency Access Logs", "Admin Account Review" },
                "Monthly"),

            // Asset Management
            CreateEvidencePack("EP_ASM_001", "Asset Inventory Pack",
                new[] { "Hardware Inventory", "Software Inventory", "Data Asset Register", "Asset Classification" },
                "Quarterly"),

            CreateEvidencePack("EP_ASM_002", "Data Classification Pack",
                new[] { "Data Classification Policy", "Data Flow Diagrams", "Data Handling Procedures", "Classification Labels" },
                "Annual"),

            // Human Resources Security
            CreateEvidencePack("EP_HRS_001", "Personnel Security Pack",
                new[] { "Background Check Records", "Security Agreement", "NDA", "Termination Checklist" },
                "Continuous"),

            CreateEvidencePack("EP_HRS_002", "Security Awareness Pack",
                new[] { "Training Attendance", "Training Materials", "Quiz Results", "Completion Certificates" },
                "Annual"),

            // Physical Security
            CreateEvidencePack("EP_PHY_001", "Physical Access Pack",
                new[] { "Access Card Logs", "Visitor Logs", "CCTV Footage Samples", "Physical Access Reviews" },
                "Quarterly"),

            CreateEvidencePack("EP_PHY_002", "Environmental Controls Pack",
                new[] { "UPS Test Records", "HVAC Monitoring", "Fire Suppression Tests", "Environmental Monitoring Logs" },
                "Quarterly"),

            // Operations Security
            CreateEvidencePack("EP_OPS_001", "Change Management Pack",
                new[] { "Change Request Forms", "CAB Meeting Minutes", "Change Approval Records", "Post-Implementation Review" },
                "Monthly"),

            CreateEvidencePack("EP_OPS_002", "Patch Management Pack",
                new[] { "Patch Status Report", "Patch Testing Records", "Patch Deployment Logs", "Vulnerability Scans" },
                "Monthly"),

            CreateEvidencePack("EP_OPS_003", "Backup & Recovery Pack",
                new[] { "Backup Schedule", "Backup Success Logs", "Restore Test Records", "Offsite Storage Records" },
                "Monthly"),

            CreateEvidencePack("EP_OPS_004", "Logging & Monitoring Pack",
                new[] { "Log Retention Settings", "SIEM Dashboard", "Alert Rules Config", "Log Review Reports" },
                "Monthly"),

            // Network Security
            CreateEvidencePack("EP_NET_001", "Network Security Pack",
                new[] { "Firewall Rules", "Network Diagrams", "Segmentation Evidence", "IDS/IPS Logs" },
                "Quarterly"),

            CreateEvidencePack("EP_NET_002", "Encryption Pack",
                new[] { "TLS/SSL Certificates", "Encryption Standards", "Key Management Records", "Data-at-Rest Encryption" },
                "Quarterly"),

            // Vulnerability Management
            CreateEvidencePack("EP_VUL_001", "Vulnerability Assessment Pack",
                new[] { "Vulnerability Scan Reports", "Penetration Test Reports", "Remediation Tracking", "Risk Acceptance" },
                "Quarterly"),

            CreateEvidencePack("EP_VUL_002", "Secure Development Pack",
                new[] { "SAST Scan Results", "DAST Scan Results", "Code Review Records", "Security Requirements" },
                "Continuous"),

            // Incident Management
            CreateEvidencePack("EP_INC_001", "Incident Response Pack",
                new[] { "Incident Reports", "Investigation Records", "Root Cause Analysis", "Lessons Learned" },
                "Continuous"),

            CreateEvidencePack("EP_INC_002", "Incident Readiness Pack",
                new[] { "IR Plan", "IR Team Roster", "Tabletop Exercise Records", "Communication Templates" },
                "Annual"),

            // Business Continuity
            CreateEvidencePack("EP_BCM_001", "Business Continuity Pack",
                new[] { "BCP Document", "BIA Results", "Recovery Procedures", "BC Test Results" },
                "Annual"),

            CreateEvidencePack("EP_BCM_002", "Disaster Recovery Pack",
                new[] { "DR Plan", "DR Test Results", "Failover Records", "RTO/RPO Evidence" },
                "Annual"),

            // Compliance & Audit
            CreateEvidencePack("EP_CMP_001", "Compliance Monitoring Pack",
                new[] { "Compliance Dashboard", "Exception Register", "Remediation Plans", "Regulatory Correspondence" },
                "Quarterly"),

            CreateEvidencePack("EP_CMP_002", "Audit Evidence Pack",
                new[] { "Audit Reports", "Management Response", "Finding Remediation", "Audit Follow-up" },
                "Annual"),

            // Third Party Risk
            CreateEvidencePack("EP_TPR_001", "Vendor Management Pack",
                new[] { "Vendor Inventory", "Risk Assessment", "Due Diligence Records", "SLA Monitoring" },
                "Annual"),

            CreateEvidencePack("EP_TPR_002", "Third Party Assurance Pack",
                new[] { "SOC 2 Reports", "ISO Certificates", "Penetration Test Reports", "Questionnaire Responses" },
                "Annual"),

            // Data Protection
            CreateEvidencePack("EP_DPR_001", "Privacy Compliance Pack",
                new[] { "Privacy Policy", "Consent Records", "DPIA Records", "Subject Request Logs" },
                "Quarterly"),

            CreateEvidencePack("EP_DPR_002", "Data Retention Pack",
                new[] { "Retention Schedule", "Disposal Records", "Archive Logs", "Legal Hold Records" },
                "Annual")
        };
    }

    private static EvidencePack CreateEvidencePack(string code, string name, string[] items, string frequency)
    {
        return new EvidencePack
        {
            Id = Guid.NewGuid(),
            PackCode = code,
            Name = name,
            NameAr = name, // Could be translated
            Description = $"Standard evidence pack for {name}",
            EvidenceItemsJson = System.Text.Json.JsonSerializer.Serialize(items),
            RequiredFrequency = frequency,
            RetentionMonths = 84, // 7 years
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };
    }
}
