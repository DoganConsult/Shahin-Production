using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Suite Generation Service - Automatically generates a control suite from onboarding profile
/// Uses Baseline + Overlays model with Rules Catalog
/// </summary>
public class SuiteGenerationService : ISuiteGenerationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExpertFrameworkMappingService _expertMappingService;
    private readonly ILogger<SuiteGenerationService> _logger;

    // Standard baseline controls (domain codes that apply to everyone)
    private static readonly List<string> BaselineControlDomains = new()
    {
        "IAM",      // Identity & Access Management
        "LOG",      // Logging & Monitoring
        "VUL",      // Vulnerability & Patch
        "CHG",      // Change Management
        "INC",      // Incident Response
        "BCP",      // Backup/DR/BCP
        "TPR",      // Third-Party/Vendor Risk
        "GOV",      // Governance & Policy
        "DAT"       // Data Classification & Retention
    };

    // Sector overlay definitions
    private static readonly Dictionary<string, SectorOverlay> SectorOverlays = new()
    {
        ["Banking"] = new SectorOverlay
        {
            Code = "OVL-BANKING",
            Name = "Banking & Financial Services Overlay",
            AdditionalDomains = new[] { "TXN", "FRD", "AML", "SEG" }, // Transaction, Fraud, AML, Segregation
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "84",      // 7 years
                ["AccessReviewFrequency"] = "Quarterly",
                ["PenTestFrequency"] = "Annual",
                ["PrivilegedAccessReview"] = "Monthly",
                ["SegregationOfDuties"] = "Strict"
            },
            RequiredFrameworks = new[] { "SAMA-CSF", "NCA-ECC", "PDPL", "SAMA-AML" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "TXN", "AML", "BCP" }
        },
        ["Healthcare"] = new SectorOverlay
        {
            Code = "OVL-HEALTHCARE",
            Name = "Healthcare Overlay",
            AdditionalDomains = new[] { "PHI", "MED", "PAT" }, // PHI, Medical Device, Patient Privacy
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "84",
                ["AccessReviewFrequency"] = "Quarterly",
                ["PatientDataAccess"] = "RoleBasedWithAudit",
                ["ConsentManagement"] = "Required"
            },
            RequiredFrameworks = new[] { "NCA-ECC", "PDPL", "SFDA", "MOH-HIS" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "PHI", "INC" }
        },
        ["Government"] = new SectorOverlay
        {
            Code = "OVL-GOVERNMENT",
            Name = "Government & Public Sector Overlay",
            AdditionalDomains = new[] { "CLS", "SOV", "CLD" }, // Classification, Sovereignty, Cloud
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "120",     // 10 years
                ["DataClassification"] = "Mandatory",
                ["CloudSovereignty"] = "Required",
                ["SecurityClearance"] = "Required"
            },
            RequiredFrameworks = new[] { "NCA-ECC", "NCA-CSCC", "DGA-CLOUD", "PDPL", "NDMO" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "CLS", "GOV", "TPR" }
        },
        ["Telecom"] = new SectorOverlay
        {
            Code = "OVL-TELECOM",
            Name = "Telecommunications Overlay",
            AdditionalDomains = new[] { "NET", "INF", "SVC" }, // Network, Infrastructure, Service
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "60",
                ["NetworkMonitoring"] = "24x7",
                ["AvailabilitySLA"] = "99.99",
                ["IncidentResponseTime"] = "15min"
            },
            RequiredFrameworks = new[] { "CST-CRF", "NCA-ECC", "NCA-CSCC", "PDPL" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "NET", "INC", "BCP" }
        },
        ["Energy"] = new SectorOverlay
        {
            Code = "OVL-ENERGY",
            Name = "Energy & Utilities Overlay",
            AdditionalDomains = new[] { "OT", "ICS", "PHY" }, // OT, ICS, Physical Security
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "84",
                ["OTSegmentation"] = "AirGap",
                ["PhysicalSecurityLevel"] = "High",
                ["IncidentResponseTime"] = "30min"
            },
            RequiredFrameworks = new[] { "NCA-ECC", "NCA-CSCC", "HCIS", "PDPL" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "OT", "PHY", "INC", "BCP" }
        },
        ["Retail"] = new SectorOverlay
        {
            Code = "OVL-RETAIL",
            Name = "Retail & E-Commerce Overlay",
            AdditionalDomains = new[] { "PCI", "WEB", "CUS" }, // PCI, Web Security, Customer Data
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "36",
                ["WebAppScanning"] = "Weekly",
                ["PCI_Compliance"] = "Required"
            },
            RequiredFrameworks = new[] { "PDPL", "NCA-ECC", "PCI-DSS", "MOCI-ECOM" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "PCI", "WEB", "TPR" }
        },
        ["Technology"] = new SectorOverlay
        {
            Code = "OVL-TECHNOLOGY",
            Name = "Technology & Software Overlay",
            AdditionalDomains = new[] { "SDL", "API", "CLD" }, // Secure Dev, API, Cloud
            ParameterOverrides = new Dictionary<string, string>
            {
                ["LogRetentionMonths"] = "36",
                ["CodeReview"] = "Required",
                ["VulnScanFrequency"] = "Weekly",
                ["SDLC"] = "Secure"
            },
            RequiredFrameworks = new[] { "NCA-ECC", "PDPL", "CST-CLOUD" },
            MandatoryEvidencePacks = new[] { "IAM", "LOG", "CHG", "VUL", "SDL" }
        }
    };

    // Jurisdiction overlay definitions
    private static readonly Dictionary<string, JurisdictionOverlay> JurisdictionOverlays = new()
    {
        ["KSA"] = new JurisdictionOverlay
        {
            Code = "OVL-KSA",
            Name = "Saudi Arabia Jurisdiction",
            MandatoryFrameworks = new[] { "NCA-ECC", "PDPL" },
            DataResidency = "InCountry",
            ParameterOverrides = new Dictionary<string, string>
            {
                ["DataResidency"] = "KSA",
                ["BreachNotification"] = "72hours",
                ["ArabicDocumentation"] = "Required"
            }
        },
        ["GCC"] = new JurisdictionOverlay
        {
            Code = "OVL-GCC",
            Name = "GCC Region",
            MandatoryFrameworks = new[] { "NCA-ECC" },
            DataResidency = "Regional",
            ParameterOverrides = new Dictionary<string, string>
            {
                ["DataResidency"] = "GCC",
                ["CrossBorderTransfer"] = "Restricted"
            }
        },
        ["EU"] = new JurisdictionOverlay
        {
            Code = "OVL-EU",
            Name = "European Union Jurisdiction",
            MandatoryFrameworks = new[] { "GDPR" },
            DataResidency = "EEA",
            ParameterOverrides = new Dictionary<string, string>
            {
                ["DataResidency"] = "EEA",
                ["BreachNotification"] = "72hours",
                ["DPO"] = "Required",
                ["DPIA"] = "Required"
            }
        }
    };

    // Data type overlay definitions
    private static readonly Dictionary<string, DataTypeOverlay> DataTypeOverlays = new()
    {
        ["PCI"] = new DataTypeOverlay
        {
            Code = "OVL-PCI",
            Name = "Payment Card Data Overlay",
            AdditionalControls = new[] { "PCI-001", "PCI-002", "PCI-003" },
            ParameterOverrides = new Dictionary<string, string>
            {
                ["Encryption"] = "AES256",
                ["KeyRotation"] = "Annual",
                ["PenTestFrequency"] = "Quarterly",
                ["LogRetentionMonths"] = "12"
            },
            RequiredEvidencePacks = new[] { "PCI" }
        },
        ["PII"] = new DataTypeOverlay
        {
            Code = "OVL-PII",
            Name = "Personal Data Overlay",
            AdditionalControls = new[] { "PII-001", "PII-002" },
            ParameterOverrides = new Dictionary<string, string>
            {
                ["Encryption"] = "Required",
                ["AccessLogging"] = "Required",
                ["DataMinimization"] = "Required"
            },
            RequiredEvidencePacks = new[] { "PRI" }
        },
        ["PHI"] = new DataTypeOverlay
        {
            Code = "OVL-PHI",
            Name = "Health Data Overlay",
            AdditionalControls = new[] { "PHI-001", "PHI-002", "PHI-003" },
            ParameterOverrides = new Dictionary<string, string>
            {
                ["Encryption"] = "Required",
                ["AccessLogging"] = "Required",
                ["PatientConsent"] = "Required",
                ["MinimumNecessary"] = "Required"
            },
            RequiredEvidencePacks = new[] { "PHI", "PRI" }
        }
    };

    public SuiteGenerationService(
        IUnitOfWork unitOfWork,
        IExpertFrameworkMappingService expertMappingService,
        ILogger<SuiteGenerationService> logger)
    {
        _unitOfWork = unitOfWork;
        _expertMappingService = expertMappingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a control suite from organization profile
    /// </summary>
    public async Task<GeneratedControlSuite> GenerateSuiteAsync(
        Guid tenantId,
        OrganizationProfile profile,
        string generatedBy)
    {
        _logger.LogInformation("Generating control suite for tenant {TenantId}, sector: {Sector}",
            tenantId, profile.Sector);

        var appliedOverlays = new List<string>();
        var rulesLog = new List<object>();
        var controlEntries = new List<SuiteControlEntry>();
        var evidenceRequests = new List<SuiteEvidenceRequest>();

        // Step 1: Start with baseline controls
        var baselineControls = await GetBaselineControlsAsync();
        foreach (var control in baselineControls)
        {
            controlEntries.Add(new SuiteControlEntry
            {
                Id = Guid.NewGuid(),
                ControlId = control.Id,
                Source = "Baseline",
                IsMandatory = true,
                InclusionReason = "Core baseline control applicable to all organizations",
                DisplayOrder = controlEntries.Count + 1
            });
        }

        rulesLog.Add(new { Step = "Baseline", ControlsAdded = baselineControls.Count });

        // Step 2: Apply sector overlay
        var normalizedSector = NormalizeSector(profile.Sector);
        if (SectorOverlays.TryGetValue(normalizedSector, out var sectorOverlay))
        {
            appliedOverlays.Add(sectorOverlay.Code);

            // Add sector-specific controls
            var sectorControls = await GetControlsByDomainsAsync(sectorOverlay.AdditionalDomains);
            foreach (var control in sectorControls)
            {
                if (!controlEntries.Any(c => c.ControlId == control.Id))
                {
                    controlEntries.Add(new SuiteControlEntry
                    {
                        Id = Guid.NewGuid(),
                        ControlId = control.Id,
                        Source = "Overlay",
                        SourceOverlayCode = sectorOverlay.Code,
                        IsMandatory = true,
                        InclusionReason = $"Required by {normalizedSector} sector overlay",
                        AppliedParametersJson = JsonSerializer.Serialize(sectorOverlay.ParameterOverrides),
                        DisplayOrder = controlEntries.Count + 1
                    });
                }
            }

            rulesLog.Add(new
            {
                Step = "SectorOverlay",
                Overlay = sectorOverlay.Code,
                ControlsAdded = sectorControls.Count,
                Frameworks = sectorOverlay.RequiredFrameworks
            });
        }

        // Step 3: Apply jurisdiction overlay
        var jurisdiction = profile.Country ?? "KSA";
        if (JurisdictionOverlays.TryGetValue(jurisdiction, out var jurisdictionOverlay))
        {
            appliedOverlays.Add(jurisdictionOverlay.Code);

            rulesLog.Add(new
            {
                Step = "JurisdictionOverlay",
                Overlay = jurisdictionOverlay.Code,
                DataResidency = jurisdictionOverlay.DataResidency,
                MandatoryFrameworks = jurisdictionOverlay.MandatoryFrameworks
            });
        }

        // Step 4: Apply data type overlays
        var dataTypes = ParseDataTypes(profile.DataTypes);
        foreach (var dataType in dataTypes)
        {
            if (DataTypeOverlays.TryGetValue(dataType, out var dataOverlay))
            {
                appliedOverlays.Add(dataOverlay.Code);

                rulesLog.Add(new
                {
                    Step = "DataTypeOverlay",
                    Overlay = dataOverlay.Code,
                    DataType = dataType,
                    RequiredEvidencePacks = dataOverlay.RequiredEvidencePacks
                });
            }
        }

        // Step 5: Apply critical infrastructure overlay if applicable
        if (profile.IsCriticalInfrastructure)
        {
            appliedOverlays.Add("OVL-CRITICAL");

            rulesLog.Add(new
            {
                Step = "CriticalInfrastructureOverlay",
                Overlay = "OVL-CRITICAL",
                AdditionalRequirements = new[] { "NCA-CSCC", "Enhanced Monitoring", "Shorter SLAs" }
            });
        }

        // Step 6: Generate evidence requests
        foreach (var entry in controlEntries.Take(50)) // Limit for demo
        {
            evidenceRequests.Add(new SuiteEvidenceRequest
            {
                Id = Guid.NewGuid(),
                ControlId = entry.ControlId,
                EvidenceItemCode = $"EVD-{entry.ControlId.ToString()[..8].ToUpper()}",
                EvidenceItemName = "Standard Evidence Pack",
                RequiredFrequency = "Quarterly",
                RetentionMonths = 84,
                Status = "NotStarted"
            });
        }

        // Create the suite
        var suite = new GeneratedControlSuite
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SuiteCode = $"SUITE-{tenantId.ToString()[..8].ToUpper()}-{DateTime.UtcNow:yyyyMMdd}",
            Name = $"Control Suite for {profile.Sector} Organization",
            AppliedOverlaysJson = JsonSerializer.Serialize(appliedOverlays),
            TotalControls = controlEntries.Count,
            MandatoryControls = controlEntries.Count(c => c.IsMandatory),
            OptionalControls = controlEntries.Count(c => !c.IsMandatory),
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = generatedBy,
            Version = "1.0",
            ProfileSnapshotJson = JsonSerializer.Serialize(new
            {
                profile.Sector,
                profile.Country,
                profile.OrganizationType,
                profile.DataTypes,
                profile.HostingModel,
                profile.IsCriticalInfrastructure,
                profile.ComplianceMaturity
            }),
            RulesExecutionLogJson = JsonSerializer.Serialize(rulesLog),
            IsActive = true
        };

        // Link entries to suite
        foreach (var entry in controlEntries)
        {
            entry.SuiteId = suite.Id;
        }
        foreach (var request in evidenceRequests)
        {
            request.SuiteId = suite.Id;
        }

        suite.ControlEntries = controlEntries;
        suite.EvidenceRequests = evidenceRequests;

        _logger.LogInformation("Generated suite {SuiteCode} with {TotalControls} controls, {OverlayCount} overlays",
            suite.SuiteCode, suite.TotalControls, appliedOverlays.Count);

        return suite;
    }

    /// <summary>
    /// Generate suite for a specific organization entity (for multi-sector orgs)
    /// </summary>
    public async Task<GeneratedControlSuite> GenerateSuiteForEntityAsync(
        Guid tenantId,
        OrganizationEntity entity,
        string generatedBy)
    {
        // Create a profile from entity attributes
        var profile = new OrganizationProfile
        {
            TenantId = tenantId,
            Sector = entity.Sectors?.Split(',').FirstOrDefault() ?? "Technology",
            Country = entity.Jurisdictions?.Split(',').FirstOrDefault() ?? "KSA",
            DataTypes = entity.DataTypes,
            HostingModel = entity.TechnologyProfile ?? "Hybrid",
            IsCriticalInfrastructure = entity.CriticalityTier == "Tier1"
        };

        return await GenerateSuiteAsync(tenantId, profile, generatedBy);
    }

    private async Task<List<CanonicalControl>> GetBaselineControlsAsync()
    {
        // In production, this would query from the database
        // For now, return mock baseline controls
        return new List<CanonicalControl>();
    }

    private async Task<List<CanonicalControl>> GetControlsByDomainsAsync(string[] domains)
    {
        // In production, query controls by domain
        return new List<CanonicalControl>();
    }

    private string NormalizeSector(string? sector)
    {
        if (string.IsNullOrWhiteSpace(sector)) return "Technology";

        var lower = sector.ToLower();
        if (lower.Contains("bank") || lower.Contains("financ")) return "Banking";
        if (lower.Contains("health") || lower.Contains("medical")) return "Healthcare";
        if (lower.Contains("government") || lower.Contains("public")) return "Government";
        if (lower.Contains("telecom")) return "Telecom";
        if (lower.Contains("energy") || lower.Contains("oil") || lower.Contains("utility")) return "Energy";
        if (lower.Contains("retail") || lower.Contains("commerce")) return "Retail";
        return "Technology";
    }

    private List<string> ParseDataTypes(string? dataTypes)
    {
        if (string.IsNullOrWhiteSpace(dataTypes)) return new List<string>();

        var result = new List<string>();
        var lower = dataTypes.ToLower();

        if (lower.Contains("pci") || lower.Contains("card") || lower.Contains("payment")) result.Add("PCI");
        if (lower.Contains("pii") || lower.Contains("personal")) result.Add("PII");
        if (lower.Contains("phi") || lower.Contains("health") || lower.Contains("medical")) result.Add("PHI");

        return result;
    }
}

#region Overlay Models

internal class SectorOverlay
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] AdditionalDomains { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> ParameterOverrides { get; set; } = new();
    public string[] RequiredFrameworks { get; set; } = Array.Empty<string>();
    public string[] MandatoryEvidencePacks { get; set; } = Array.Empty<string>();
}

internal class JurisdictionOverlay
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] MandatoryFrameworks { get; set; } = Array.Empty<string>();
    public string DataResidency { get; set; } = string.Empty;
    public Dictionary<string, string> ParameterOverrides { get; set; } = new();
}

internal class DataTypeOverlay
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] AdditionalControls { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> ParameterOverrides { get; set; } = new();
    public string[] RequiredEvidencePacks { get; set; } = Array.Empty<string>();
}

#endregion
