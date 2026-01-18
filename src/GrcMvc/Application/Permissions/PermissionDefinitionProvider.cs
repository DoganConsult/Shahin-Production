using GrcMvc.Resources;
using Volo.Abp.Localization;
using AbpPermissions = Volo.Abp.Authorization.Permissions;

namespace GrcMvc.Application.Permissions;

/// <summary>
/// Defines all GRC permissions in the system.
/// Now extends ABP's PermissionDefinitionProvider for full ABP integration.
/// </summary>
public class GrcPermissionDefinitionProvider : AbpPermissions.PermissionDefinitionProvider
{
    /// <summary>
    /// Helper to create a fixed (non-localized) string for permission display names.
    /// For full localization, use L("Key") pattern with resource files.
    /// </summary>
    private static ILocalizableString L(string value) => new FixedLocalizableString(value);

    public override void Define(AbpPermissions.IPermissionDefinitionContext context)
    {
        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\Shahin-ai\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new{location="PermissionDefinitionProvider.cs:Define",message="ABP PermissionDefinitionProvider.Define() called",data=new{contextType=context.GetType().Name},timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),sessionId="debug-session",hypothesisId="B"})+"\n"); } catch {}
        // #endregion
        
        var grc = context.AddGroup(GrcPermissions.GroupName, L("GRC System"));

        // Home
        grc.AddPermission(GrcPermissions.Home.Default, L("Home"));

        // Dashboard
        grc.AddPermission(GrcPermissions.Dashboard.Default, L("Dashboard"));

        // Subscriptions
        var subs = grc.AddPermission(GrcPermissions.Subscriptions.Default, L("Subscriptions"));
        subs.AddChild(GrcPermissions.Subscriptions.View, L("View"));
        subs.AddChild(GrcPermissions.Subscriptions.Manage, L("Manage"));

        // Admin
        var admin = grc.AddPermission(GrcPermissions.Admin.Default, L("Admin"));
        admin.AddChild(GrcPermissions.Admin.Access, L("Access"));
        admin.AddChild(GrcPermissions.Admin.Users, L("Users"));
        admin.AddChild(GrcPermissions.Admin.Roles, L("Roles"));
        admin.AddChild(GrcPermissions.Admin.Tenants, L("Tenants"));

        // Frameworks
        var frameworks = grc.AddPermission(GrcPermissions.Frameworks.Default, L("Frameworks"));
        frameworks.AddChild(GrcPermissions.Frameworks.View, L("View"));
        frameworks.AddChild(GrcPermissions.Frameworks.Create, L("Create"));
        frameworks.AddChild(GrcPermissions.Frameworks.Update, L("Update"));
        frameworks.AddChild(GrcPermissions.Frameworks.Delete, L("Delete"));
        frameworks.AddChild(GrcPermissions.Frameworks.Import, L("Import"));

        // Regulators
        var regulators = grc.AddPermission(GrcPermissions.Regulators.Default, L("Regulators"));
        regulators.AddChild(GrcPermissions.Regulators.View, L("View"));
        regulators.AddChild(GrcPermissions.Regulators.Manage, L("Manage"));

        // Assessments
        var assessments = grc.AddPermission(GrcPermissions.Assessments.Default, L("Assessments"));
        assessments.AddChild(GrcPermissions.Assessments.View, L("View"));
        assessments.AddChild(GrcPermissions.Assessments.Create, L("Create"));
        assessments.AddChild(GrcPermissions.Assessments.Update, L("Update"));
        assessments.AddChild(GrcPermissions.Assessments.Submit, L("Submit"));
        assessments.AddChild(GrcPermissions.Assessments.Approve, L("Approve"));

        // Control Assessments
        var controlAssessments = grc.AddPermission(GrcPermissions.ControlAssessments.Default, L("Control Assessments"));
        controlAssessments.AddChild(GrcPermissions.ControlAssessments.View, L("View"));
        controlAssessments.AddChild(GrcPermissions.ControlAssessments.Manage, L("Manage"));

        // Evidence
        var evidence = grc.AddPermission(GrcPermissions.Evidence.Default, L("Evidence"));
        evidence.AddChild(GrcPermissions.Evidence.View, L("View"));
        evidence.AddChild(GrcPermissions.Evidence.Upload, L("Upload"));
        evidence.AddChild(GrcPermissions.Evidence.Update, L("Update"));
        evidence.AddChild(GrcPermissions.Evidence.Delete, L("Delete"));
        evidence.AddChild(GrcPermissions.Evidence.Approve, L("Approve"));

        // Risks
        var risks = grc.AddPermission(GrcPermissions.Risks.Default, L("Risks"));
        risks.AddChild(GrcPermissions.Risks.View, L("View"));
        risks.AddChild(GrcPermissions.Risks.Manage, L("Manage"));
        risks.AddChild(GrcPermissions.Risks.Accept, L("Accept"));

        // Audits
        var audits = grc.AddPermission(GrcPermissions.Audits.Default, L("Audits"));
        audits.AddChild(GrcPermissions.Audits.View, L("View"));
        audits.AddChild(GrcPermissions.Audits.Manage, L("Manage"));
        audits.AddChild(GrcPermissions.Audits.Close, L("Close"));

        // Action Plans
        var actionPlans = grc.AddPermission(GrcPermissions.ActionPlans.Default, L("Action Plans"));
        actionPlans.AddChild(GrcPermissions.ActionPlans.View, L("View"));
        actionPlans.AddChild(GrcPermissions.ActionPlans.Manage, L("Manage"));
        actionPlans.AddChild(GrcPermissions.ActionPlans.Assign, L("Assign"));
        actionPlans.AddChild(GrcPermissions.ActionPlans.Close, L("Close"));

        // Policies
        var policies = grc.AddPermission(GrcPermissions.Policies.Default, L("Policies"));
        policies.AddChild(GrcPermissions.Policies.View, L("View"));
        policies.AddChild(GrcPermissions.Policies.Manage, L("Manage"));
        policies.AddChild(GrcPermissions.Policies.Approve, L("Approve"));
        policies.AddChild(GrcPermissions.Policies.Publish, L("Publish"));

        // Compliance Calendar
        var complianceCalendar = grc.AddPermission(GrcPermissions.ComplianceCalendar.Default, L("Compliance Calendar"));
        complianceCalendar.AddChild(GrcPermissions.ComplianceCalendar.View, L("View"));
        complianceCalendar.AddChild(GrcPermissions.ComplianceCalendar.Manage, L("Manage"));

        // Workflow
        var workflow = grc.AddPermission(GrcPermissions.Workflow.Default, L("Workflow"));
        workflow.AddChild(GrcPermissions.Workflow.View, L("View"));
        workflow.AddChild(GrcPermissions.Workflow.Manage, L("Manage"));

        // Notifications
        var notifications = grc.AddPermission(GrcPermissions.Notifications.Default, L("Notifications"));
        notifications.AddChild(GrcPermissions.Notifications.View, L("View"));
        notifications.AddChild(GrcPermissions.Notifications.Manage, L("Manage"));

        // Vendors
        var vendors = grc.AddPermission(GrcPermissions.Vendors.Default, L("Vendors"));
        vendors.AddChild(GrcPermissions.Vendors.View, L("View"));
        vendors.AddChild(GrcPermissions.Vendors.Manage, L("Manage"));
        vendors.AddChild(GrcPermissions.Vendors.Assess, L("Assess"));

        // Reports
        var reports = grc.AddPermission(GrcPermissions.Reports.Default, L("Reports"));
        reports.AddChild(GrcPermissions.Reports.View, L("View"));
        reports.AddChild(GrcPermissions.Reports.Export, L("Export"));

        // Resilience
        var resilience = grc.AddPermission(GrcPermissions.Resilience.Default, L("Resilience"));
        resilience.AddChild(GrcPermissions.Resilience.View, L("View"));
        resilience.AddChild(GrcPermissions.Resilience.Manage, L("Manage"));
        resilience.AddChild(GrcPermissions.Resilience.Create, L("Create"));
        resilience.AddChild(GrcPermissions.Resilience.Edit, L("Edit"));
        resilience.AddChild(GrcPermissions.Resilience.Delete, L("Delete"));
        resilience.AddChild(GrcPermissions.Resilience.AssessRTO, L("Assess RTO"));
        resilience.AddChild(GrcPermissions.Resilience.AssessRPO, L("Assess RPO"));
        resilience.AddChild(GrcPermissions.Resilience.ManageDrills, L("Manage Drills"));
        resilience.AddChild(GrcPermissions.Resilience.ManagePlans, L("Manage Plans"));
        resilience.AddChild(GrcPermissions.Resilience.Monitor, L("Monitor"));

        // Certification
        var certification = grc.AddPermission(GrcPermissions.Certification.Default, L("Certification"));
        certification.AddChild(GrcPermissions.Certification.View, L("View"));
        certification.AddChild(GrcPermissions.Certification.Create, L("Create"));
        certification.AddChild(GrcPermissions.Certification.Edit, L("Edit"));
        certification.AddChild(GrcPermissions.Certification.Delete, L("Delete"));
        certification.AddChild(GrcPermissions.Certification.Manage, L("Manage"));
        certification.AddChild(GrcPermissions.Certification.Readiness, L("Readiness"));

        // Maturity
        var maturity = grc.AddPermission(GrcPermissions.Maturity.Default, L("Maturity"));
        maturity.AddChild(GrcPermissions.Maturity.View, L("View"));
        maturity.AddChild(GrcPermissions.Maturity.Create, L("Create"));
        maturity.AddChild(GrcPermissions.Maturity.Edit, L("Edit"));
        maturity.AddChild(GrcPermissions.Maturity.Delete, L("Delete"));
        maturity.AddChild(GrcPermissions.Maturity.Assess, L("Assess"));
        maturity.AddChild(GrcPermissions.Maturity.Baseline, L("Baseline"));
        maturity.AddChild(GrcPermissions.Maturity.Roadmap, L("Roadmap"));

        // Excellence
        var excellence = grc.AddPermission(GrcPermissions.Excellence.Default, L("Excellence"));
        excellence.AddChild(GrcPermissions.Excellence.View, L("View"));
        excellence.AddChild(GrcPermissions.Excellence.Create, L("Create"));
        excellence.AddChild(GrcPermissions.Excellence.Edit, L("Edit"));
        excellence.AddChild(GrcPermissions.Excellence.Delete, L("Delete"));
        excellence.AddChild(GrcPermissions.Excellence.Manage, L("Manage"));
        excellence.AddChild(GrcPermissions.Excellence.Benchmark, L("Benchmark"));
        excellence.AddChild(GrcPermissions.Excellence.Assess, L("Assess"));

        // Sustainability
        var sustainability = grc.AddPermission(GrcPermissions.Sustainability.Default, L("Sustainability"));
        sustainability.AddChild(GrcPermissions.Sustainability.View, L("View"));
        sustainability.AddChild(GrcPermissions.Sustainability.Create, L("Create"));
        sustainability.AddChild(GrcPermissions.Sustainability.Edit, L("Edit"));
        sustainability.AddChild(GrcPermissions.Sustainability.Delete, L("Delete"));
        sustainability.AddChild(GrcPermissions.Sustainability.Manage, L("Manage"));
        sustainability.AddChild(GrcPermissions.Sustainability.Dashboard, L("Dashboard"));
        sustainability.AddChild(GrcPermissions.Sustainability.KPIs, L("KPIs"));

        // Reports
        reports.AddChild(GrcPermissions.Reports.Generate, L("Generate"));

        // Integrations
        var integrations = grc.AddPermission(GrcPermissions.Integrations.Default, L("Integrations"));
        integrations.AddChild(GrcPermissions.Integrations.View, L("View"));
        integrations.AddChild(GrcPermissions.Integrations.Manage, L("Manage"));

        // Controls
        var controls = grc.AddPermission(GrcPermissions.Controls.Default, L("Controls"));
        controls.AddChild(GrcPermissions.Controls.View, L("View"));
        controls.AddChild(GrcPermissions.Controls.Create, L("Create"));
        controls.AddChild(GrcPermissions.Controls.Edit, L("Edit"));
        controls.AddChild(GrcPermissions.Controls.Delete, L("Delete"));
        controls.AddChild(GrcPermissions.Controls.Implement, L("Implement"));
        controls.AddChild(GrcPermissions.Controls.Test, L("Test"));

        // Users
        var users = grc.AddPermission(GrcPermissions.Users.Default, L("Users"));
        users.AddChild(GrcPermissions.Users.View, L("View"));
        users.AddChild(GrcPermissions.Users.Create, L("Create"));
        users.AddChild(GrcPermissions.Users.Edit, L("Edit"));
        users.AddChild(GrcPermissions.Users.Delete, L("Delete"));
        users.AddChild(GrcPermissions.Users.AssignRole, L("Assign Role"));

        // Roles
        var roles = grc.AddPermission(GrcPermissions.Roles.Default, L("Roles"));
        roles.AddChild(GrcPermissions.Roles.View, L("View"));
        roles.AddChild(GrcPermissions.Roles.Create, L("Create"));
        roles.AddChild(GrcPermissions.Roles.Edit, L("Edit"));
        roles.AddChild(GrcPermissions.Roles.Delete, L("Delete"));

        // Permissions Management
        grc.AddPermission(GrcPermissions.Permissions.Manage, L("Manage Permissions"));

        // Features Management
        grc.AddPermission(GrcPermissions.Features.Manage, L("Manage Features"));
    }
}
