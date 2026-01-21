using GrcMvc.Data;
using GrcMvc.Models.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Seeds the global catalog data (Layer 1: Platform)
    /// Roles, Titles, Baselines, Packages, Templates, Evidence Types
    /// </summary>
    public class CatalogSeederService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<CatalogSeederService> _logger;

        public CatalogSeederService(GrcDbContext context, ILogger<CatalogSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAllCatalogsAsync()
        {
            await SeedRolesAndTitlesAsync();
            await SeedBaselinesAsync();
            await SeedPackagesAsync();
            await SeedTemplatesAsync();
            await SeedEvidenceTypesAsync();
            _logger.LogInformation("✅ All catalogs seeded successfully");
        }

        public async Task SeedRolesAndTitlesAsync()
        {
            if (await _context.Set<RoleCatalog>().AnyAsync())
            {
                _logger.LogInformation("Roles already seeded, skipping...");
                return;
            }

            var roles = new List<RoleCatalog>
            {
                // Executive Layer
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "CISO",
                    RoleName = "Chief Information Security Officer",
                    Description = "Executive responsible for information security strategy",
                    Layer = "Executive",
                    Department = "Security",
                    ApprovalLevel = 4,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = true,
                    DisplayOrder = 1
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "DPO",
                    RoleName = "Data Protection Officer",
                    Description = "Executive responsible for data privacy compliance (PDPL)",
                    Layer = "Executive",
                    Department = "Legal/Compliance",
                    ApprovalLevel = 4,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = true,
                    DisplayOrder = 2
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "CRO",
                    RoleName = "Chief Risk Officer",
                    Description = "Executive responsible for enterprise risk management",
                    Layer = "Executive",
                    Department = "Risk",
                    ApprovalLevel = 4,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = true,
                    DisplayOrder = 3
                },

                // Management Layer
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "GRC_MANAGER",
                    RoleName = "GRC Manager",
                    Description = "Manages governance, risk, and compliance programs",
                    Layer = "Management",
                    Department = "GRC",
                    ApprovalLevel = 3,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = true,
                    DisplayOrder = 10
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "COMPLIANCE_OFFICER",
                    RoleName = "Compliance Officer",
                    Description = "Oversees regulatory compliance activities",
                    Layer = "Management",
                    Department = "Compliance",
                    ApprovalLevel = 3,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = true,
                    DisplayOrder = 11
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "RISK_MANAGER",
                    RoleName = "Risk Manager",
                    Description = "Manages risk assessment and treatment activities",
                    Layer = "Management",
                    Department = "Risk",
                    ApprovalLevel = 3,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 12
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "SECURITY_OFFICER",
                    RoleName = "Security Officer",
                    Description = "Manages security controls and incident response",
                    Layer = "Management",
                    Department = "Security",
                    ApprovalLevel = 3,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 13
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "AUDIT_MANAGER",
                    RoleName = "Audit Manager",
                    Description = "Manages internal audit activities",
                    Layer = "Management",
                    Department = "Audit",
                    ApprovalLevel = 3,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = true,
                    CanReassign = true,
                    DisplayOrder = 14
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "LEGAL_COUNSEL",
                    RoleName = "Legal Counsel",
                    Description = "Provides legal review and guidance",
                    Layer = "Management",
                    Department = "Legal",
                    ApprovalLevel = 3,
                    CanApprove = true,
                    CanReject = true,
                    CanEscalate = false,
                    CanReassign = false,
                    DisplayOrder = 15
                },

                // Operational Layer
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "CONTROL_OWNER",
                    RoleName = "Control Owner",
                    Description = "Owns and maintains specific controls",
                    Layer = "Operational",
                    Department = "Various",
                    ApprovalLevel = 2,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 20
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "RISK_ANALYST",
                    RoleName = "Risk Analyst",
                    Description = "Performs risk analysis and assessment",
                    Layer = "Operational",
                    Department = "Risk",
                    ApprovalLevel = 1,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 21
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "PRIVACY_ANALYST",
                    RoleName = "Privacy Analyst",
                    Description = "Performs privacy impact assessments",
                    Layer = "Operational",
                    Department = "Privacy",
                    ApprovalLevel = 1,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 22
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "AUDITOR",
                    RoleName = "Auditor",
                    Description = "Performs audit activities and finding validation",
                    Layer = "Operational",
                    Department = "Audit",
                    ApprovalLevel = 2,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 23
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "POLICY_OWNER",
                    RoleName = "Policy Owner",
                    Description = "Owns and maintains policies",
                    Layer = "Operational",
                    Department = "Various",
                    ApprovalLevel = 2,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 24
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "ACTION_OWNER",
                    RoleName = "Action Owner",
                    Description = "Owns and executes remediation actions",
                    Layer = "Operational",
                    Department = "Various",
                    ApprovalLevel = 1,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 25
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "OPERATIONS_MANAGER",
                    RoleName = "Operations Manager",
                    Description = "Manages operational resilience activities",
                    Layer = "Operational",
                    Department = "Operations",
                    ApprovalLevel = 2,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 26
                },

                // Support Layer
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "SME",
                    RoleName = "Subject Matter Expert",
                    Description = "Provides technical expertise for reviews",
                    Layer = "Support",
                    Department = "Various",
                    ApprovalLevel = 1,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 30
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "POLICY_ADMIN",
                    RoleName = "Policy Administrator",
                    Description = "Administers policy publication and acknowledgment",
                    Layer = "Support",
                    Department = "Compliance",
                    ApprovalLevel = 1,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = false,
                    CanReassign = false,
                    DisplayOrder = 31
                },
                new RoleCatalog
                {
                    Id = Guid.NewGuid(),
                    RoleCode = "PROCESS_OWNER",
                    RoleName = "Process Owner",
                    Description = "Owns business processes affected by findings",
                    Layer = "Support",
                    Department = "Various",
                    ApprovalLevel = 2,
                    CanApprove = false,
                    CanReject = false,
                    CanEscalate = true,
                    CanReassign = false,
                    DisplayOrder = 32
                }
            };

            await _context.Set<RoleCatalog>().AddRangeAsync(roles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} roles", roles.Count);

            // Seed titles for each role
            var titles = new List<TitleCatalog>();
            foreach (var role in roles)
            {
                titles.Add(new TitleCatalog
                {
                    Id = Guid.NewGuid(),
                    TitleCode = $"JR_{role.RoleCode}",
                    TitleName = $"Junior {role.RoleName}",
                    Description = $"Junior level {role.RoleName}",
                    RoleCatalogId = role.Id,
                    DisplayOrder = 1
                });
                titles.Add(new TitleCatalog
                {
                    Id = Guid.NewGuid(),
                    TitleCode = role.RoleCode,
                    TitleName = role.RoleName,
                    Description = $"Standard {role.RoleName}",
                    RoleCatalogId = role.Id,
                    DisplayOrder = 2
                });
                titles.Add(new TitleCatalog
                {
                    Id = Guid.NewGuid(),
                    TitleCode = $"SR_{role.RoleCode}",
                    TitleName = $"Senior {role.RoleName}",
                    Description = $"Senior level {role.RoleName}",
                    RoleCatalogId = role.Id,
                    DisplayOrder = 3
                });
            }

            await _context.Set<TitleCatalog>().AddRangeAsync(titles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} titles", titles.Count);
        }

        public async Task SeedBaselinesAsync()
        {
            if (await _context.Set<BaselineCatalog>().AnyAsync())
            {
                _logger.LogInformation("Baselines already seeded, skipping...");
                return;
            }

            var baselines = new List<BaselineCatalog>
            {
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "NCA_ECC_2024",
                    BaselineName = "NCA Essential Cybersecurity Controls (ECC-2:2024)",
                    Description = "National Cybersecurity Authority Essential Cybersecurity Controls for organizations in Saudi Arabia",
                    RegulatorCode = "NCA",
                    Version = "2.0",
                    EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 109,
                    DisplayOrder = 1
                },
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "NCA_CSCC_2024",
                    BaselineName = "NCA Critical Systems Cybersecurity Controls (CSCC)",
                    Description = "Controls for critical national infrastructure systems",
                    RegulatorCode = "NCA",
                    Version = "1.0",
                    EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 50,
                    DisplayOrder = 2
                },
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "PDPL_2023",
                    BaselineName = "Personal Data Protection Law (PDPL)",
                    Description = "SDAIA Personal Data Protection Law and Implementing Regulations",
                    RegulatorCode = "SDAIA",
                    Version = "1.0",
                    EffectiveDate = new DateTime(2023, 9, 14, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 45,
                    DisplayOrder = 3
                },
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "SAMA_CSF_2024",
                    BaselineName = "SAMA Cyber Security Framework",
                    Description = "Saudi Central Bank Cybersecurity Framework for financial institutions",
                    RegulatorCode = "SAMA",
                    Version = "2.0",
                    EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 85,
                    DisplayOrder = 4
                },
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "CST_CRF_2024",
                    BaselineName = "CST Cybersecurity Regulatory Framework",
                    Description = "Communications, Space & Technology Commission ICT sector cybersecurity framework",
                    RegulatorCode = "CST",
                    Version = "1.0",
                    EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 60,
                    DisplayOrder = 5
                },
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "CST_CLOUD_2024",
                    BaselineName = "CST Cloud Computing Service Provisioning Regulations",
                    Description = "Regulations for cloud service providers in Saudi Arabia",
                    RegulatorCode = "CST",
                    Version = "4.0",
                    EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 40,
                    DisplayOrder = 6
                },
                new BaselineCatalog
                {
                    Id = Guid.NewGuid(),
                    BaselineCode = "NDMO_DG_2024",
                    BaselineName = "NDMO Data Governance Standards",
                    Description = "National Data Management Office data governance and protection standards",
                    RegulatorCode = "NDMO",
                    Version = "1.0",
                    EffectiveDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ControlCount = 35,
                    DisplayOrder = 7
                }
            };

            await _context.Set<BaselineCatalog>().AddRangeAsync(baselines);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} baselines", baselines.Count);
        }

        public async Task SeedPackagesAsync()
        {
            if (await _context.Set<PackageCatalog>().AnyAsync())
            {
                _logger.LogInformation("Packages already seeded, skipping...");
                return;
            }

            var packages = new List<PackageCatalog>
            {
                // Security Packages
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "CYBERSECURITY_ESSENTIALS",
                    PackageName = "Cybersecurity Essentials",
                    Description = "Core cybersecurity controls for all organizations",
                    Category = "Security",
                    RequirementCount = 50,
                    EstimatedDays = 30,
                    DisplayOrder = 1
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "NETWORK_SECURITY",
                    PackageName = "Network Security",
                    Description = "Network protection and monitoring controls",
                    Category = "Security",
                    RequirementCount = 25,
                    EstimatedDays = 14,
                    DisplayOrder = 2
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "ACCESS_CONTROL",
                    PackageName = "Access Control & Identity",
                    Description = "Identity management and access control",
                    Category = "Security",
                    RequirementCount = 30,
                    EstimatedDays = 21,
                    DisplayOrder = 3
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "INCIDENT_RESPONSE",
                    PackageName = "Incident Response",
                    Description = "Security incident detection and response",
                    Category = "Security",
                    RequirementCount = 20,
                    EstimatedDays = 14,
                    DisplayOrder = 4
                },

                // Privacy Packages
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "DATA_PROTECTION",
                    PackageName = "Data Protection & Privacy",
                    Description = "Personal data protection and privacy compliance",
                    Category = "Privacy",
                    RequirementCount = 45,
                    EstimatedDays = 30,
                    DisplayOrder = 10
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "CONSENT_MANAGEMENT",
                    PackageName = "Consent Management",
                    Description = "Data subject consent collection and management",
                    Category = "Privacy",
                    RequirementCount = 15,
                    EstimatedDays = 14,
                    DisplayOrder = 11
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "DATA_RIGHTS",
                    PackageName = "Data Subject Rights",
                    Description = "Managing data subject access and deletion requests",
                    Category = "Privacy",
                    RequirementCount = 12,
                    EstimatedDays = 10,
                    DisplayOrder = 12
                },

                // Governance Packages
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "POLICY_GOVERNANCE",
                    PackageName = "Policy & Governance",
                    Description = "Policy management and governance framework",
                    Category = "Governance",
                    RequirementCount = 20,
                    EstimatedDays = 21,
                    DisplayOrder = 20
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "VENDOR_MANAGEMENT",
                    PackageName = "Third-Party & Vendor Management",
                    Description = "Vendor risk assessment and management",
                    Category = "Governance",
                    RequirementCount = 18,
                    EstimatedDays = 14,
                    DisplayOrder = 21
                },

                // Risk Packages
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "RISK_MANAGEMENT",
                    PackageName = "Enterprise Risk Management",
                    Description = "Risk identification, assessment, and treatment",
                    Category = "Risk",
                    RequirementCount = 25,
                    EstimatedDays = 21,
                    DisplayOrder = 30
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "BUSINESS_CONTINUITY",
                    PackageName = "Business Continuity",
                    Description = "Business continuity and disaster recovery",
                    Category = "Risk",
                    RequirementCount = 22,
                    EstimatedDays = 21,
                    DisplayOrder = 31
                },

                // Sector-Specific Packages
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "FINANCIAL_SERVICES",
                    PackageName = "Financial Services Compliance",
                    Description = "SAMA-specific requirements for financial institutions",
                    Category = "Sector",
                    RequirementCount = 40,
                    EstimatedDays = 30,
                    DisplayOrder = 40
                },
                new PackageCatalog
                {
                    Id = Guid.NewGuid(),
                    PackageCode = "CLOUD_PROVIDER",
                    PackageName = "Cloud Service Provider",
                    Description = "CST requirements for cloud service providers",
                    Category = "Sector",
                    RequirementCount = 35,
                    EstimatedDays = 28,
                    DisplayOrder = 41
                }
            };

            await _context.Set<PackageCatalog>().AddRangeAsync(packages);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} packages", packages.Count);
        }

        public async Task SeedTemplatesAsync()
        {
            if (await _context.Set<TemplateCatalog>().AnyAsync())
            {
                _logger.LogInformation("Templates already seeded, skipping...");
                return;
            }

            var templates = new List<TemplateCatalog>
            {
                new TemplateCatalog
                {
                    Id = Guid.NewGuid(),
                    TemplateCode = "NCA_ECC_QUICK",
                    TemplateName = "NCA ECC Quick Scan",
                    Description = "Rapid assessment of critical ECC controls",
                    TemplateType = "QuickScan",
                    RequirementCount = 30,
                    EstimatedDays = 7,
                    DisplayOrder = 1
                },
                new TemplateCatalog
                {
                    Id = Guid.NewGuid(),
                    TemplateCode = "NCA_ECC_FULL",
                    TemplateName = "NCA ECC Full Assessment",
                    Description = "Complete assessment of all 109 ECC controls",
                    TemplateType = "Full",
                    RequirementCount = 109,
                    EstimatedDays = 30,
                    DisplayOrder = 2
                },
                new TemplateCatalog
                {
                    Id = Guid.NewGuid(),
                    TemplateCode = "PDPL_PIA",
                    TemplateName = "PDPL Privacy Impact Assessment",
                    Description = "Privacy impact assessment for PDPL compliance",
                    TemplateType = "Full",
                    RequirementCount = 45,
                    EstimatedDays = 21,
                    DisplayOrder = 3
                },
                new TemplateCatalog
                {
                    Id = Guid.NewGuid(),
                    TemplateCode = "SAMA_CSF_FULL",
                    TemplateName = "SAMA CSF Full Assessment",
                    Description = "Complete SAMA Cybersecurity Framework assessment",
                    TemplateType = "Full",
                    RequirementCount = 85,
                    EstimatedDays = 30,
                    DisplayOrder = 4
                },
                new TemplateCatalog
                {
                    Id = Guid.NewGuid(),
                    TemplateCode = "ERM_ASSESSMENT",
                    TemplateName = "Enterprise Risk Assessment",
                    Description = "Comprehensive enterprise risk assessment",
                    TemplateType = "Full",
                    RequirementCount = 25,
                    EstimatedDays = 14,
                    DisplayOrder = 5
                },
                new TemplateCatalog
                {
                    Id = Guid.NewGuid(),
                    TemplateCode = "GAP_REMEDIATION",
                    TemplateName = "Gap Remediation Plan",
                    Description = "Template for addressing identified gaps",
                    TemplateType = "Remediation",
                    RequirementCount = 0,
                    EstimatedDays = 30,
                    DisplayOrder = 6
                }
            };

            await _context.Set<TemplateCatalog>().AddRangeAsync(templates);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} templates", templates.Count);
        }

        public async Task SeedEvidenceTypesAsync()
        {
            if (await _context.Set<EvidenceTypeCatalog>().AnyAsync())
            {
                _logger.LogInformation("Evidence types already seeded, skipping...");
                return;
            }

            var evidenceTypes = new List<EvidenceTypeCatalog>
            {
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "POLICY_DOC",
                    EvidenceTypeName = "Policy Document",
                    Description = "Approved policy or procedure document",
                    Category = "Document",
                    AllowedFileTypes = ".pdf,.doc,.docx",
                    MaxFileSizeMB = 20,
                    RequiresApproval = true,
                    MinScore = 70,
                    DisplayOrder = 1
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "PROCEDURE_DOC",
                    EvidenceTypeName = "Procedure Document",
                    Description = "Standard operating procedure or work instruction",
                    Category = "Document",
                    AllowedFileTypes = ".pdf,.doc,.docx",
                    MaxFileSizeMB = 20,
                    RequiresApproval = true,
                    MinScore = 70,
                    DisplayOrder = 2
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "SCREENSHOT",
                    EvidenceTypeName = "System Screenshot",
                    Description = "Screenshot showing system configuration or status",
                    Category = "Screenshot",
                    AllowedFileTypes = ".png,.jpg,.jpeg,.gif",
                    MaxFileSizeMB = 10,
                    RequiresApproval = true,
                    MinScore = 60,
                    DisplayOrder = 3
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "CONFIG_EXPORT",
                    EvidenceTypeName = "Configuration Export",
                    Description = "Exported system configuration file",
                    Category = "Config",
                    AllowedFileTypes = ".json,.xml,.yaml,.yml,.txt,.csv",
                    MaxFileSizeMB = 50,
                    RequiresApproval = true,
                    MinScore = 80,
                    DisplayOrder = 4
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "AUDIT_LOG",
                    EvidenceTypeName = "Audit Log",
                    Description = "System audit log or access log",
                    Category = "Log",
                    AllowedFileTypes = ".log,.txt,.csv,.json",
                    MaxFileSizeMB = 100,
                    RequiresApproval = true,
                    MinScore = 75,
                    DisplayOrder = 5
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "CERTIFICATE",
                    EvidenceTypeName = "Certificate",
                    Description = "Certification or attestation document",
                    Category = "Certificate",
                    AllowedFileTypes = ".pdf,.png,.jpg",
                    MaxFileSizeMB = 10,
                    RequiresApproval = true,
                    MinScore = 90,
                    DisplayOrder = 6
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "TRAINING_RECORD",
                    EvidenceTypeName = "Training Record",
                    Description = "Training completion record or certificate",
                    Category = "Document",
                    AllowedFileTypes = ".pdf,.xlsx,.csv",
                    MaxFileSizeMB = 20,
                    RequiresApproval = true,
                    MinScore = 70,
                    DisplayOrder = 7
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "SCAN_REPORT",
                    EvidenceTypeName = "Vulnerability Scan Report",
                    Description = "Security scan or vulnerability assessment report",
                    Category = "Document",
                    AllowedFileTypes = ".pdf,.html,.xml,.json",
                    MaxFileSizeMB = 50,
                    RequiresApproval = true,
                    MinScore = 80,
                    DisplayOrder = 8
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "MEETING_MINUTES",
                    EvidenceTypeName = "Meeting Minutes",
                    Description = "Minutes from governance or review meetings",
                    Category = "Document",
                    AllowedFileTypes = ".pdf,.doc,.docx",
                    MaxFileSizeMB = 10,
                    RequiresApproval = true,
                    MinScore = 60,
                    DisplayOrder = 9
                },
                new EvidenceTypeCatalog
                {
                    Id = Guid.NewGuid(),
                    EvidenceTypeCode = "CONTRACT",
                    EvidenceTypeName = "Contract/Agreement",
                    Description = "Signed contract or service agreement",
                    Category = "Document",
                    AllowedFileTypes = ".pdf",
                    MaxFileSizeMB = 20,
                    RequiresApproval = true,
                    MinScore = 85,
                    DisplayOrder = 10
                }
            };

            await _context.Set<EvidenceTypeCatalog>().AddRangeAsync(evidenceTypes);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seeded {Count} evidence types", evidenceTypes.Count);
        }
    }
}
