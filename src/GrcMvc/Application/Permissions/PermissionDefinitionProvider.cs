using GrcMvc.Resources;

namespace GrcMvc.Application.Permissions;

/// <summary>
/// Defines all GRC permissions in the system
/// </summary>
public class GrcPermissionDefinitionProvider : IPermissionDefinitionProvider
{
    public void Define(IPermissionDefinitionContext context)
    {
        var grc = context.AddGroup(GrcPermissions.GroupName, "GRC System");

        // Home
        grc.AddPermission(GrcPermissions.Home.Default, "Home");

        // Dashboard
        grc.AddPermission(GrcPermissions.Dashboard.Default, "Dashboard");

        // Subscriptions
        var subs = grc.AddPermission(GrcPermissions.Subscriptions.Default, "Subscriptions");
        subs.AddChild(GrcPermissions.Subscriptions.View, "View");
        subs.AddChild(GrcPermissions.Subscriptions.Manage, "Manage");

        // Admin
        var admin = grc.AddPermission(GrcPermissions.Admin.Default, "Admin");
        admin.AddChild(GrcPermissions.Admin.Access, "Access");
        admin.AddChild(GrcPermissions.Admin.Users, "Users");
        admin.AddChild(GrcPermissions.Admin.Roles, "Roles");
        admin.AddChild(GrcPermissions.Admin.Tenants, "Tenants");

        // Frameworks
        var frameworks = grc.AddPermission(GrcPermissions.Frameworks.Default, "Frameworks");
        frameworks.AddChild(GrcPermissions.Frameworks.View, "View");
        frameworks.AddChild(GrcPermissions.Frameworks.Create, "Create");
        frameworks.AddChild(GrcPermissions.Frameworks.Update, "Update");
        frameworks.AddChild(GrcPermissions.Frameworks.Delete, "Delete");
        frameworks.AddChild(GrcPermissions.Frameworks.Import, "Import");

        // Regulators
        var regulators = grc.AddPermission(GrcPermissions.Regulators.Default, "Regulators");
        regulators.AddChild(GrcPermissions.Regulators.View, "View");
        regulators.AddChild(GrcPermissions.Regulators.Manage, "Manage");

        // Assessments
        var assessments = grc.AddPermission(GrcPermissions.Assessments.Default, "Assessments");
        assessments.AddChild(GrcPermissions.Assessments.View, "View");
        assessments.AddChild(GrcPermissions.Assessments.Create, "Create");
        assessments.AddChild(GrcPermissions.Assessments.Update, "Update");
        assessments.AddChild(GrcPermissions.Assessments.Submit, "Submit");
        assessments.AddChild(GrcPermissions.Assessments.Approve, "Approve");

        // Control Assessments
        var controlAssessments = grc.AddPermission(GrcPermissions.ControlAssessments.Default, "Control Assessments");
        controlAssessments.AddChild(GrcPermissions.ControlAssessments.View, "View");
        controlAssessments.AddChild(GrcPermissions.ControlAssessments.Manage, "Manage");

        // Evidence
        var evidence = grc.AddPermission(GrcPermissions.Evidence.Default, "Evidence");
        evidence.AddChild(GrcPermissions.Evidence.View, "View");
        evidence.AddChild(GrcPermissions.Evidence.Upload, "Upload");
        evidence.AddChild(GrcPermissions.Evidence.Update, "Update");
        evidence.AddChild(GrcPermissions.Evidence.Delete, "Delete");
        evidence.AddChild(GrcPermissions.Evidence.Approve, "Approve");

        // Risks
        var risks = grc.AddPermission(GrcPermissions.Risks.Default, "Risks");
        risks.AddChild(GrcPermissions.Risks.View, "View");
        risks.AddChild(GrcPermissions.Risks.Manage, "Manage");
        risks.AddChild(GrcPermissions.Risks.Accept, "Accept");

        // Audits
        var audits = grc.AddPermission(GrcPermissions.Audits.Default, "Audits");
        audits.AddChild(GrcPermissions.Audits.View, "View");
        audits.AddChild(GrcPermissions.Audits.Manage, "Manage");
        audits.AddChild(GrcPermissions.Audits.Close, "Close");

        // Action Plans
        var actionPlans = grc.AddPermission(GrcPermissions.ActionPlans.Default, "Action Plans");
        actionPlans.AddChild(GrcPermissions.ActionPlans.View, "View");
        actionPlans.AddChild(GrcPermissions.ActionPlans.Manage, "Manage");
        actionPlans.AddChild(GrcPermissions.ActionPlans.Assign, "Assign");
        actionPlans.AddChild(GrcPermissions.ActionPlans.Close, "Close");

        // Policies
        var policies = grc.AddPermission(GrcPermissions.Policies.Default, "Policies");
        policies.AddChild(GrcPermissions.Policies.View, "View");
        policies.AddChild(GrcPermissions.Policies.Manage, "Manage");
        policies.AddChild(GrcPermissions.Policies.Approve, "Approve");
        policies.AddChild(GrcPermissions.Policies.Publish, "Publish");

        // Compliance Calendar
        var complianceCalendar = grc.AddPermission(GrcPermissions.ComplianceCalendar.Default, "Compliance Calendar");
        complianceCalendar.AddChild(GrcPermissions.ComplianceCalendar.View, "View");
        complianceCalendar.AddChild(GrcPermissions.ComplianceCalendar.Manage, "Manage");

        // Workflow
        var workflow = grc.AddPermission(GrcPermissions.Workflow.Default, "Workflow");
        workflow.AddChild(GrcPermissions.Workflow.View, "View");
        workflow.AddChild(GrcPermissions.Workflow.Manage, "Manage");

        // Notifications
        var notifications = grc.AddPermission(GrcPermissions.Notifications.Default, "Notifications");
        notifications.AddChild(GrcPermissions.Notifications.View, "View");
        notifications.AddChild(GrcPermissions.Notifications.Manage, "Manage");

        // Vendors
        var vendors = grc.AddPermission(GrcPermissions.Vendors.Default, "Vendors");
        vendors.AddChild(GrcPermissions.Vendors.View, "View");
        vendors.AddChild(GrcPermissions.Vendors.Manage, "Manage");
        vendors.AddChild(GrcPermissions.Vendors.Assess, "Assess");

        // Reports
        var reports = grc.AddPermission(GrcPermissions.Reports.Default, "Reports");
        reports.AddChild(GrcPermissions.Reports.View, "View");
        reports.AddChild(GrcPermissions.Reports.Export, "Export");

        // Resilience
        var resilience = grc.AddPermission(GrcPermissions.Resilience.Default, "Resilience");
        resilience.AddChild(GrcPermissions.Resilience.View, "View");
        resilience.AddChild(GrcPermissions.Resilience.Manage, "Manage");
        resilience.AddChild(GrcPermissions.Resilience.Create, "Create");
        resilience.AddChild(GrcPermissions.Resilience.Edit, "Edit");
        resilience.AddChild(GrcPermissions.Resilience.Delete, "Delete");
        resilience.AddChild(GrcPermissions.Resilience.AssessRTO, "Assess RTO");
        resilience.AddChild(GrcPermissions.Resilience.AssessRPO, "Assess RPO");
        resilience.AddChild(GrcPermissions.Resilience.ManageDrills, "Manage Drills");
        resilience.AddChild(GrcPermissions.Resilience.ManagePlans, "Manage Plans");
        resilience.AddChild(GrcPermissions.Resilience.Monitor, "Monitor");

        // Certification
        var certification = grc.AddPermission(GrcPermissions.Certification.Default, "Certification");
        certification.AddChild(GrcPermissions.Certification.View, "View");
        certification.AddChild(GrcPermissions.Certification.Create, "Create");
        certification.AddChild(GrcPermissions.Certification.Edit, "Edit");
        certification.AddChild(GrcPermissions.Certification.Delete, "Delete");
        certification.AddChild(GrcPermissions.Certification.Manage, "Manage");
        certification.AddChild(GrcPermissions.Certification.Readiness, "Readiness");

        // Maturity
        var maturity = grc.AddPermission(GrcPermissions.Maturity.Default, "Maturity");
        maturity.AddChild(GrcPermissions.Maturity.View, "View");
        maturity.AddChild(GrcPermissions.Maturity.Create, "Create");
        maturity.AddChild(GrcPermissions.Maturity.Edit, "Edit");
        maturity.AddChild(GrcPermissions.Maturity.Delete, "Delete");
        maturity.AddChild(GrcPermissions.Maturity.Assess, "Assess");
        maturity.AddChild(GrcPermissions.Maturity.Baseline, "Baseline");
        maturity.AddChild(GrcPermissions.Maturity.Roadmap, "Roadmap");

        // Excellence
        var excellence = grc.AddPermission(GrcPermissions.Excellence.Default, "Excellence");
        excellence.AddChild(GrcPermissions.Excellence.View, "View");
        excellence.AddChild(GrcPermissions.Excellence.Create, "Create");
        excellence.AddChild(GrcPermissions.Excellence.Edit, "Edit");
        excellence.AddChild(GrcPermissions.Excellence.Delete, "Delete");
        excellence.AddChild(GrcPermissions.Excellence.Manage, "Manage");
        excellence.AddChild(GrcPermissions.Excellence.Benchmark, "Benchmark");
        excellence.AddChild(GrcPermissions.Excellence.Assess, "Assess");

        // Sustainability
        var sustainability = grc.AddPermission(GrcPermissions.Sustainability.Default, "Sustainability");
        sustainability.AddChild(GrcPermissions.Sustainability.View, "View");
        sustainability.AddChild(GrcPermissions.Sustainability.Create, "Create");
        sustainability.AddChild(GrcPermissions.Sustainability.Edit, "Edit");
        sustainability.AddChild(GrcPermissions.Sustainability.Delete, "Delete");
        sustainability.AddChild(GrcPermissions.Sustainability.Manage, "Manage");
        sustainability.AddChild(GrcPermissions.Sustainability.Dashboard, "Dashboard");
        sustainability.AddChild(GrcPermissions.Sustainability.KPIs, "KPIs");

        // Reports
        reports.AddChild(GrcPermissions.Reports.Generate, "Generate");

        // Integrations
        var integrations = grc.AddPermission(GrcPermissions.Integrations.Default, "Integrations");
        integrations.AddChild(GrcPermissions.Integrations.View, "View");
        integrations.AddChild(GrcPermissions.Integrations.Manage, "Manage");

        // Controls
        var controls = grc.AddPermission(GrcPermissions.Controls.Default, "Controls");
        controls.AddChild(GrcPermissions.Controls.View, "View");
        controls.AddChild(GrcPermissions.Controls.Create, "Create");
        controls.AddChild(GrcPermissions.Controls.Edit, "Edit");
        controls.AddChild(GrcPermissions.Controls.Delete, "Delete");
        controls.AddChild(GrcPermissions.Controls.Implement, "Implement");
        controls.AddChild(GrcPermissions.Controls.Test, "Test");

        // Users
        var users = grc.AddPermission(GrcPermissions.Users.Default, "Users");
        users.AddChild(GrcPermissions.Users.View, "View");
        users.AddChild(GrcPermissions.Users.Create, "Create");
        users.AddChild(GrcPermissions.Users.Edit, "Edit");
        users.AddChild(GrcPermissions.Users.Delete, "Delete");
        users.AddChild(GrcPermissions.Users.AssignRole, "Assign Role");

        // Roles
        var roles = grc.AddPermission(GrcPermissions.Roles.Default, "Roles");
        roles.AddChild(GrcPermissions.Roles.View, "View");
        roles.AddChild(GrcPermissions.Roles.Create, "Create");
        roles.AddChild(GrcPermissions.Roles.Edit, "Edit");
        roles.AddChild(GrcPermissions.Roles.Delete, "Delete");

        // Permissions Management
        grc.AddPermission(GrcPermissions.Permissions.Manage, "Manage Permissions");

        // Features Management
        grc.AddPermission(GrcPermissions.Features.Manage, "Manage Features");
    }
}
