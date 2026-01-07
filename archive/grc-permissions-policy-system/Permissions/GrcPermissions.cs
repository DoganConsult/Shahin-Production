namespace GrcMvc.Application.Permissions;

/// <summary>
/// Centralized permission constants for GRC system
/// All permissions follow the pattern: Grc.{Module}.{Action}
/// </summary>
public static class GrcPermissions
{
    public const string GroupName = "Grc";

    // Home
    public static class Home
    {
        public const string Default = GroupName + ".Home";
    }

    // Dashboard
    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
    }

    // Subscriptions
    public static class Subscriptions
    {
        public const string Default = GroupName + ".Subscriptions";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    // Admin
    public static class Admin
    {
        public const string Default = GroupName + ".Admin";
        public const string Access = Default + ".Access";
        public const string Users = Default + ".Users";
        public const string Roles = Default + ".Roles";
        public const string Tenants = Default + ".Tenants";
    }

    // Frameworks
    public static class Frameworks
    {
        public const string Default = GroupName + ".Frameworks";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Import = Default + ".Import";
    }

    // Regulators
    public static class Regulators
    {
        public const string Default = GroupName + ".Regulators";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    // Assessments
    public static class Assessments
    {
        public const string Default = GroupName + ".Assessments";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Submit = Default + ".Submit";
        public const string Approve = Default + ".Approve";
    }

    // Control Assessments
    public static class ControlAssessments
    {
        public const string Default = GroupName + ".ControlAssessments";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    // Evidence
    public static class Evidence
    {
        public const string Default = GroupName + ".Evidence";
        public const string View = Default + ".View";
        public const string Upload = Default + ".Upload";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
    }

    // Risks
    public static class Risks
    {
        public const string Default = GroupName + ".Risks";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Accept = Default + ".Accept";
    }

    // Audits
    public static class Audits
    {
        public const string Default = GroupName + ".Audits";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Close = Default + ".Close";
    }

    // Action Plans
    public static class ActionPlans
    {
        public const string Default = GroupName + ".ActionPlans";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Assign = Default + ".Assign";
        public const string Close = Default + ".Close";
    }

    // Policies
    public static class Policies
    {
        public const string Default = GroupName + ".Policies";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Approve = Default + ".Approve";
        public const string Publish = Default + ".Publish";
    }

    // Compliance Calendar
    public static class ComplianceCalendar
    {
        public const string Default = GroupName + ".ComplianceCalendar";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    // Workflow
    public static class Workflow
    {
        public const string Default = GroupName + ".Workflow";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    // Notifications
    public static class Notifications
    {
        public const string Default = GroupName + ".Notifications";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    // Vendors
    public static class Vendors
    {
        public const string Default = GroupName + ".Vendors";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Assess = Default + ".Assess";
    }

    // Reports
    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string View = Default + ".View";
        public const string Export = Default + ".Export";
    }

    // Integrations
    public static class Integrations
    {
        public const string Default = GroupName + ".Integrations";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    /// <summary>
    /// Get all permission strings as a flat list
    /// </summary>
    public static IEnumerable<string> GetAllPermissions()
    {
        yield return Home.Default;
        yield return Dashboard.Default;
        yield return Subscriptions.View;
        yield return Subscriptions.Manage;
        yield return Admin.Access;
        yield return Admin.Users;
        yield return Admin.Roles;
        yield return Admin.Tenants;
        yield return Frameworks.View;
        yield return Frameworks.Create;
        yield return Frameworks.Update;
        yield return Frameworks.Delete;
        yield return Frameworks.Import;
        yield return Regulators.View;
        yield return Regulators.Manage;
        yield return Assessments.View;
        yield return Assessments.Create;
        yield return Assessments.Update;
        yield return Assessments.Submit;
        yield return Assessments.Approve;
        yield return ControlAssessments.View;
        yield return ControlAssessments.Manage;
        yield return Evidence.View;
        yield return Evidence.Upload;
        yield return Evidence.Update;
        yield return Evidence.Delete;
        yield return Evidence.Approve;
        yield return Risks.View;
        yield return Risks.Manage;
        yield return Risks.Accept;
        yield return Audits.View;
        yield return Audits.Manage;
        yield return Audits.Close;
        yield return ActionPlans.View;
        yield return ActionPlans.Manage;
        yield return ActionPlans.Assign;
        yield return ActionPlans.Close;
        yield return Policies.View;
        yield return Policies.Manage;
        yield return Policies.Approve;
        yield return Policies.Publish;
        yield return ComplianceCalendar.View;
        yield return ComplianceCalendar.Manage;
        yield return Workflow.View;
        yield return Workflow.Manage;
        yield return Notifications.View;
        yield return Notifications.Manage;
        yield return Vendors.View;
        yield return Vendors.Manage;
        yield return Vendors.Assess;
        yield return Reports.View;
        yield return Reports.Export;
        yield return Integrations.View;
        yield return Integrations.Manage;
    }
}
