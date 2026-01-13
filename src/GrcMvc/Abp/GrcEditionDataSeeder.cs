using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.FeatureManagement;
using Volo.Abp.TenantManagement;

namespace GrcMvc.Abp;

/// <summary>
/// Seeds edition/plan data for Shahin GRC Platform
///
/// Editions:
/// - Trial: 7-day free trial with basic features
/// - Starter: Small teams, basic GRC
/// - Professional: Growing organizations
/// - Enterprise: Large enterprises, full features
/// - Government: Saudi government sector (NCA, SAMA)
/// </summary>
public class GrcEditionDataSeeder : ITransientDependency
{
    private readonly IFeatureValueRepository _featureValueRepository;
    private readonly ILogger<GrcEditionDataSeeder> _logger;

    public GrcEditionDataSeeder(
        IFeatureValueRepository featureValueRepository,
        ILogger<GrcEditionDataSeeder> logger)
    {
        _featureValueRepository = featureValueRepository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding GRC Edition feature values...");

        try
        {
            // Note: In a full implementation, you would seed feature values
            // for each edition. This requires the Volo.Saas module for full
            // edition management, or custom implementation.
            //
            // For now, we define the edition configurations as reference:

            var editions = GetEditionDefinitions();

            foreach (var edition in editions)
            {
                _logger.LogInformation("Edition defined: {EditionName} - {Features} features",
                    edition.Name, edition.Features.Count);
            }

            _logger.LogInformation("GRC Edition data seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding GRC edition data");
        }
    }

    /// <summary>
    /// Returns the edition definitions for Shahin GRC
    /// These can be stored in database or used for runtime configuration
    /// </summary>
    public static List<EditionDefinition> GetEditionDefinitions()
    {
        return new List<EditionDefinition>
        {
            // ══════════════════════════════════════════════════════════════
            // TRIAL - 7 Days Free
            // ══════════════════════════════════════════════════════════════
            new EditionDefinition
            {
                Name = "Trial",
                DisplayName = "تجربة مجانية",
                DisplayNameEn = "Free Trial",
                DurationDays = 7,
                PriceMonthly = 0,
                PriceYearly = 0,
                Features = new Dictionary<string, string>
                {
                    [GrcFeatures.MaxUsers] = "3",
                    [GrcFeatures.MaxWorkspaces] = "1",
                    [GrcFeatures.StorageQuotaGb] = "1",
                    [GrcFeatures.RiskManagement.Enabled] = "true",
                    [GrcFeatures.RiskManagement.MaxRisks] = "25",
                    [GrcFeatures.Compliance.Enabled] = "true",
                    [GrcFeatures.Compliance.MaxFrameworks] = "1",
                    [GrcFeatures.Audit.Enabled] = "true",
                    [GrcFeatures.Audit.MaxAuditsPerYear] = "1",
                    [GrcFeatures.Reporting.Enabled] = "true",
                    [GrcFeatures.Reporting.PdfExport] = "true",
                    [GrcFeatures.AI.Enabled] = "false",
                    [GrcFeatures.Integrations.ApiAccess] = "false",
                    [GrcFeatures.Workflow.Enabled] = "true"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // STARTER - Small Teams
            // ══════════════════════════════════════════════════════════════
            new EditionDefinition
            {
                Name = "Starter",
                DisplayName = "المبتدئ",
                DisplayNameEn = "Starter",
                PriceMonthly = 499,
                PriceYearly = 4990,
                Currency = "SAR",
                Features = new Dictionary<string, string>
                {
                    [GrcFeatures.MaxUsers] = "10",
                    [GrcFeatures.MaxWorkspaces] = "2",
                    [GrcFeatures.StorageQuotaGb] = "10",
                    [GrcFeatures.RiskManagement.Enabled] = "true",
                    [GrcFeatures.RiskManagement.MaxRisks] = "100",
                    [GrcFeatures.RiskManagement.HeatmapEnabled] = "true",
                    [GrcFeatures.Compliance.Enabled] = "true",
                    [GrcFeatures.Compliance.MaxFrameworks] = "2",
                    [GrcFeatures.Compliance.NcaEnabled] = "true",
                    [GrcFeatures.Audit.Enabled] = "true",
                    [GrcFeatures.Audit.MaxAuditsPerYear] = "4",
                    [GrcFeatures.Audit.EvidenceManagement] = "true",
                    [GrcFeatures.Reporting.Enabled] = "true",
                    [GrcFeatures.Reporting.PdfExport] = "true",
                    [GrcFeatures.Reporting.ExcelExport] = "true",
                    [GrcFeatures.AI.Enabled] = "false",
                    [GrcFeatures.Integrations.ApiAccess] = "false",
                    [GrcFeatures.Workflow.Enabled] = "true",
                    [GrcFeatures.Workflow.AutomatedReminders] = "true"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // PROFESSIONAL - Growing Organizations
            // ══════════════════════════════════════════════════════════════
            new EditionDefinition
            {
                Name = "Professional",
                DisplayName = "الاحترافي",
                DisplayNameEn = "Professional",
                PriceMonthly = 1499,
                PriceYearly = 14990,
                Currency = "SAR",
                Features = new Dictionary<string, string>
                {
                    [GrcFeatures.MaxUsers] = "50",
                    [GrcFeatures.MaxWorkspaces] = "5",
                    [GrcFeatures.StorageQuotaGb] = "50",
                    [GrcFeatures.RiskManagement.Enabled] = "true",
                    [GrcFeatures.RiskManagement.MaxRisks] = "500",
                    [GrcFeatures.RiskManagement.HeatmapEnabled] = "true",
                    [GrcFeatures.Compliance.Enabled] = "true",
                    [GrcFeatures.Compliance.MaxFrameworks] = "5",
                    [GrcFeatures.Compliance.NcaEnabled] = "true",
                    [GrcFeatures.Compliance.SamaEnabled] = "true",
                    [GrcFeatures.Compliance.Iso27001Enabled] = "true",
                    [GrcFeatures.Audit.Enabled] = "true",
                    [GrcFeatures.Audit.MaxAuditsPerYear] = "12",
                    [GrcFeatures.Audit.EvidenceManagement] = "true",
                    [GrcFeatures.Reporting.Enabled] = "true",
                    [GrcFeatures.Reporting.PdfExport] = "true",
                    [GrcFeatures.Reporting.ExcelExport] = "true",
                    [GrcFeatures.Reporting.ExecutiveDashboard] = "true",
                    [GrcFeatures.AI.Enabled] = "true",
                    [GrcFeatures.AI.RiskAssessment] = "true",
                    [GrcFeatures.Integrations.ApiAccess] = "true",
                    [GrcFeatures.Workflow.Enabled] = "true",
                    [GrcFeatures.Workflow.AutomatedReminders] = "true"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // ENTERPRISE - Large Organizations
            // ══════════════════════════════════════════════════════════════
            new EditionDefinition
            {
                Name = "Enterprise",
                DisplayName = "المؤسسات",
                DisplayNameEn = "Enterprise",
                PriceMonthly = 4999,
                PriceYearly = 49990,
                Currency = "SAR",
                Features = new Dictionary<string, string>
                {
                    [GrcFeatures.MaxUsers] = "500",
                    [GrcFeatures.MaxWorkspaces] = "20",
                    [GrcFeatures.StorageQuotaGb] = "500",
                    [GrcFeatures.RiskManagement.Enabled] = "true",
                    [GrcFeatures.RiskManagement.MaxRisks] = "10000",
                    [GrcFeatures.RiskManagement.HeatmapEnabled] = "true",
                    [GrcFeatures.Compliance.Enabled] = "true",
                    [GrcFeatures.Compliance.MaxFrameworks] = "20",
                    [GrcFeatures.Compliance.NcaEnabled] = "true",
                    [GrcFeatures.Compliance.SamaEnabled] = "true",
                    [GrcFeatures.Compliance.Iso27001Enabled] = "true",
                    [GrcFeatures.Audit.Enabled] = "true",
                    [GrcFeatures.Audit.MaxAuditsPerYear] = "52",
                    [GrcFeatures.Audit.EvidenceManagement] = "true",
                    [GrcFeatures.Reporting.Enabled] = "true",
                    [GrcFeatures.Reporting.PdfExport] = "true",
                    [GrcFeatures.Reporting.ExcelExport] = "true",
                    [GrcFeatures.Reporting.ExecutiveDashboard] = "true",
                    [GrcFeatures.Reporting.CustomReports] = "true",
                    [GrcFeatures.AI.Enabled] = "true",
                    [GrcFeatures.AI.RiskAssessment] = "true",
                    [GrcFeatures.AI.ComplianceAssistant] = "true",
                    [GrcFeatures.AI.DocumentAnalysis] = "true",
                    [GrcFeatures.Integrations.ApiAccess] = "true",
                    [GrcFeatures.Integrations.SsoEnabled] = "true",
                    [GrcFeatures.Integrations.WebhooksEnabled] = "true",
                    [GrcFeatures.Workflow.Enabled] = "true",
                    [GrcFeatures.Workflow.CustomWorkflows] = "true",
                    [GrcFeatures.Workflow.AutomatedReminders] = "true"
                }
            },

            // ══════════════════════════════════════════════════════════════
            // GOVERNMENT - Saudi Government Sector
            // ══════════════════════════════════════════════════════════════
            new EditionDefinition
            {
                Name = "Government",
                DisplayName = "الحكومي",
                DisplayNameEn = "Government",
                PriceMonthly = 0, // Custom pricing
                PriceYearly = 0,
                Currency = "SAR",
                IsCustomPricing = true,
                Features = new Dictionary<string, string>
                {
                    [GrcFeatures.MaxUsers] = "10000",
                    [GrcFeatures.MaxWorkspaces] = "100",
                    [GrcFeatures.StorageQuotaGb] = "5000",
                    [GrcFeatures.RiskManagement.Enabled] = "true",
                    [GrcFeatures.RiskManagement.MaxRisks] = "100000",
                    [GrcFeatures.RiskManagement.HeatmapEnabled] = "true",
                    [GrcFeatures.Compliance.Enabled] = "true",
                    [GrcFeatures.Compliance.MaxFrameworks] = "50",
                    [GrcFeatures.Compliance.NcaEnabled] = "true",
                    [GrcFeatures.Compliance.SamaEnabled] = "true",
                    [GrcFeatures.Compliance.Iso27001Enabled] = "true",
                    [GrcFeatures.Audit.Enabled] = "true",
                    [GrcFeatures.Audit.MaxAuditsPerYear] = "52",
                    [GrcFeatures.Audit.EvidenceManagement] = "true",
                    [GrcFeatures.Reporting.Enabled] = "true",
                    [GrcFeatures.Reporting.PdfExport] = "true",
                    [GrcFeatures.Reporting.ExcelExport] = "true",
                    [GrcFeatures.Reporting.ExecutiveDashboard] = "true",
                    [GrcFeatures.Reporting.CustomReports] = "true",
                    [GrcFeatures.AI.Enabled] = "true",
                    [GrcFeatures.AI.RiskAssessment] = "true",
                    [GrcFeatures.AI.ComplianceAssistant] = "true",
                    [GrcFeatures.AI.DocumentAnalysis] = "true",
                    [GrcFeatures.Integrations.ApiAccess] = "true",
                    [GrcFeatures.Integrations.SsoEnabled] = "true",
                    [GrcFeatures.Integrations.WebhooksEnabled] = "true",
                    [GrcFeatures.Workflow.Enabled] = "true",
                    [GrcFeatures.Workflow.CustomWorkflows] = "true",
                    [GrcFeatures.Workflow.AutomatedReminders] = "true"
                }
            }
        };
    }
}

/// <summary>
/// Edition definition model
/// </summary>
public class EditionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public int? DurationDays { get; set; }
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }
    public string Currency { get; set; } = "SAR";
    public bool IsCustomPricing { get; set; }
    public Dictionary<string, string> Features { get; set; } = new();
}
