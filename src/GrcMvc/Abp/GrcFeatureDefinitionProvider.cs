using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace GrcMvc.Abp;

/// <summary>
/// Defines all features available in Shahin GRC Platform
///
/// Features are organized by GRC lifecycle stage:
/// - Stage 1: Onboarding & Setup
/// - Stage 2: Assessment & Mapping
/// - Stage 3: Compliance & Evidence
/// - Stage 4: Monitoring & Reporting
/// - Stage 5: Advanced AI & Automation
/// </summary>
public class GrcFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="GrcFeatureDefinitionProvider.cs:Define",message="ABP FeatureDefinitionProvider.Define() called",data=new{contextType=context.GetType().Name},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="C"})+"\n"); } catch {}
        // #endregion
        
        // ══════════════════════════════════════════════════════════════
        // GRC CORE FEATURES
        // ══════════════════════════════════════════════════════════════
        var grcGroup = context.AddGroup(
            GrcFeatures.GroupName,
            L("Feature:GrcGroup"));

        // Maximum Users per Tenant
        grcGroup.AddFeature(
            GrcFeatures.MaxUsers,
            defaultValue: "5",
            displayName: L("Feature:MaxUsers"),
            description: L("Feature:MaxUsers:Description"),
            valueType: new FreeTextStringValueType(
                new NumericValueValidator(1, 10000)));

        // Maximum Workspaces per Tenant
        grcGroup.AddFeature(
            GrcFeatures.MaxWorkspaces,
            defaultValue: "1",
            displayName: L("Feature:MaxWorkspaces"),
            description: L("Feature:MaxWorkspaces:Description"),
            valueType: new FreeTextStringValueType(
                new NumericValueValidator(1, 100)));

        // Storage Quota (GB)
        grcGroup.AddFeature(
            GrcFeatures.StorageQuotaGb,
            defaultValue: "5",
            displayName: L("Feature:StorageQuota"),
            description: L("Feature:StorageQuota:Description"),
            valueType: new FreeTextStringValueType(
                new NumericValueValidator(1, 10000)));

        // ══════════════════════════════════════════════════════════════
        // RISK MANAGEMENT
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.RiskManagement.Enabled,
            defaultValue: "true",
            displayName: L("Feature:RiskManagement"),
            description: L("Feature:RiskManagement:Description"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.RiskManagement.MaxRisks,
            defaultValue: "100",
            displayName: L("Feature:MaxRisks"),
            valueType: new FreeTextStringValueType(
                new NumericValueValidator(1, 100000)));

        grcGroup.AddFeature(
            GrcFeatures.RiskManagement.HeatmapEnabled,
            defaultValue: "true",
            displayName: L("Feature:RiskHeatmap"),
            valueType: new ToggleStringValueType());

        // ══════════════════════════════════════════════════════════════
        // COMPLIANCE & FRAMEWORKS
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.Compliance.Enabled,
            defaultValue: "true",
            displayName: L("Feature:Compliance"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Compliance.MaxFrameworks,
            defaultValue: "3",
            displayName: L("Feature:MaxFrameworks"),
            description: L("Feature:MaxFrameworks:Description"),
            valueType: new FreeTextStringValueType(
                new NumericValueValidator(1, 50)));

        grcGroup.AddFeature(
            GrcFeatures.Compliance.NcaEnabled,
            defaultValue: "true",
            displayName: L("Feature:NcaFramework"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Compliance.SamaEnabled,
            defaultValue: "false",
            displayName: L("Feature:SamaFramework"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Compliance.Iso27001Enabled,
            defaultValue: "false",
            displayName: L("Feature:Iso27001Framework"),
            valueType: new ToggleStringValueType());

        // ══════════════════════════════════════════════════════════════
        // AUDIT & EVIDENCE
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.Audit.Enabled,
            defaultValue: "true",
            displayName: L("Feature:Audit"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Audit.MaxAuditsPerYear,
            defaultValue: "4",
            displayName: L("Feature:MaxAuditsPerYear"),
            valueType: new FreeTextStringValueType(
                new NumericValueValidator(1, 52)));

        grcGroup.AddFeature(
            GrcFeatures.Audit.EvidenceManagement,
            defaultValue: "true",
            displayName: L("Feature:EvidenceManagement"),
            valueType: new ToggleStringValueType());

        // ══════════════════════════════════════════════════════════════
        // REPORTING & ANALYTICS
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.Reporting.Enabled,
            defaultValue: "true",
            displayName: L("Feature:Reporting"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Reporting.PdfExport,
            defaultValue: "true",
            displayName: L("Feature:PdfExport"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Reporting.ExcelExport,
            defaultValue: "true",
            displayName: L("Feature:ExcelExport"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Reporting.ExecutiveDashboard,
            defaultValue: "false",
            displayName: L("Feature:ExecutiveDashboard"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Reporting.CustomReports,
            defaultValue: "false",
            displayName: L("Feature:CustomReports"),
            valueType: new ToggleStringValueType());

        // ══════════════════════════════════════════════════════════════
        // AI & AUTOMATION (Premium)
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.AI.Enabled,
            defaultValue: "false",
            displayName: L("Feature:AI"),
            description: L("Feature:AI:Description"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.AI.RiskAssessment,
            defaultValue: "false",
            displayName: L("Feature:AIRiskAssessment"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.AI.ComplianceAssistant,
            defaultValue: "false",
            displayName: L("Feature:AIComplianceAssistant"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.AI.DocumentAnalysis,
            defaultValue: "false",
            displayName: L("Feature:AIDocumentAnalysis"),
            valueType: new ToggleStringValueType());

        // ══════════════════════════════════════════════════════════════
        // INTEGRATIONS
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.Integrations.ApiAccess,
            defaultValue: "false",
            displayName: L("Feature:ApiAccess"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Integrations.SsoEnabled,
            defaultValue: "false",
            displayName: L("Feature:SsoEnabled"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Integrations.WebhooksEnabled,
            defaultValue: "false",
            displayName: L("Feature:Webhooks"),
            valueType: new ToggleStringValueType());

        // ══════════════════════════════════════════════════════════════
        // WORKFLOW & AUTOMATION
        // ══════════════════════════════════════════════════════════════
        grcGroup.AddFeature(
            GrcFeatures.Workflow.Enabled,
            defaultValue: "true",
            displayName: L("Feature:Workflow"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Workflow.CustomWorkflows,
            defaultValue: "false",
            displayName: L("Feature:CustomWorkflows"),
            valueType: new ToggleStringValueType());

        grcGroup.AddFeature(
            GrcFeatures.Workflow.AutomatedReminders,
            defaultValue: "true",
            displayName: L("Feature:AutomatedReminders"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<GrcFeatureResource>(name);
    }
}

/// <summary>
/// Feature name constants for Shahin GRC
/// Usage: await _featureChecker.IsEnabledAsync(GrcFeatures.AI.Enabled)
/// </summary>
public static class GrcFeatures
{
    public const string GroupName = "GrcFeatures";

    // Tenant Limits
    public const string MaxUsers = GroupName + ".MaxUsers";
    public const string MaxWorkspaces = GroupName + ".MaxWorkspaces";
    public const string StorageQuotaGb = GroupName + ".StorageQuotaGb";

    public static class RiskManagement
    {
        public const string Default = GroupName + ".RiskManagement";
        public const string Enabled = Default + ".Enabled";
        public const string MaxRisks = Default + ".MaxRisks";
        public const string HeatmapEnabled = Default + ".Heatmap";
    }

    public static class Compliance
    {
        public const string Default = GroupName + ".Compliance";
        public const string Enabled = Default + ".Enabled";
        public const string MaxFrameworks = Default + ".MaxFrameworks";
        public const string NcaEnabled = Default + ".NCA";
        public const string SamaEnabled = Default + ".SAMA";
        public const string Iso27001Enabled = Default + ".ISO27001";
    }

    public static class Audit
    {
        public const string Default = GroupName + ".Audit";
        public const string Enabled = Default + ".Enabled";
        public const string MaxAuditsPerYear = Default + ".MaxAuditsPerYear";
        public const string EvidenceManagement = Default + ".EvidenceManagement";
    }

    public static class Reporting
    {
        public const string Default = GroupName + ".Reporting";
        public const string Enabled = Default + ".Enabled";
        public const string PdfExport = Default + ".PdfExport";
        public const string ExcelExport = Default + ".ExcelExport";
        public const string ExecutiveDashboard = Default + ".ExecutiveDashboard";
        public const string CustomReports = Default + ".CustomReports";
    }

    public static class AI
    {
        public const string Default = GroupName + ".AI";
        public const string Enabled = Default + ".Enabled";
        public const string RiskAssessment = Default + ".RiskAssessment";
        public const string ComplianceAssistant = Default + ".ComplianceAssistant";
        public const string DocumentAnalysis = Default + ".DocumentAnalysis";
    }

    public static class Integrations
    {
        public const string Default = GroupName + ".Integrations";
        public const string ApiAccess = Default + ".ApiAccess";
        public const string SsoEnabled = Default + ".SSO";
        public const string WebhooksEnabled = Default + ".Webhooks";
    }

    public static class Workflow
    {
        public const string Default = GroupName + ".Workflow";
        public const string Enabled = Default + ".Enabled";
        public const string CustomWorkflows = Default + ".CustomWorkflows";
        public const string AutomatedReminders = Default + ".AutomatedReminders";
    }
}

/// <summary>
/// Localization resource marker for GRC features
/// </summary>
public class GrcFeatureResource { }
