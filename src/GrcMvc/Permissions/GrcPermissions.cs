namespace GrcMvc.Permissions;

/// <summary>
/// Centralized GRC permission constants - NO magic strings
/// Maps to Arabic menu items as specified in requirements
/// </summary>
public static class GrcPermissions
{
    public const string GroupName = "Grc";

    public static class Home
    {
        public const string Default = GroupName + ".Home";
    }

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
    }

    public static class Subscriptions
    {
        public const string View = GroupName + ".Subscriptions.View";
        public const string Create = GroupName + ".Subscriptions.Create";
        public const string Update = GroupName + ".Subscriptions.Update";
        public const string Delete = GroupName + ".Subscriptions.Delete";
    }

    public static class Admin
    {
        public const string Access = GroupName + ".Admin.Access";
        public const string ManageUsers = GroupName + ".Admin.ManageUsers";
        public const string ManageRoles = GroupName + ".Admin.ManageRoles";
        public const string ManageSettings = GroupName + ".Admin.ManageSettings";
    }

    public static class Frameworks
    {
        public const string View = GroupName + ".Frameworks.View";
        public const string Create = GroupName + ".Frameworks.Create";
        public const string Update = GroupName + ".Frameworks.Update";
        public const string Delete = GroupName + ".Frameworks.Delete";
    }

    public static class Regulators
    {
        public const string View = GroupName + ".Regulators.View";
        public const string Create = GroupName + ".Regulators.Create";
        public const string Update = GroupName + ".Regulators.Update";
        public const string Delete = GroupName + ".Regulators.Delete";
    }

    public static class Assessments
    {
        public const string View = GroupName + ".Assessments.View";
        public const string Create = GroupName + ".Assessments.Create";
        public const string Update = GroupName + ".Assessments.Update";
        public const string Submit = GroupName + ".Assessments.Submit";
        public const string Approve = GroupName + ".Assessments.Approve";
        public const string Delete = GroupName + ".Assessments.Delete";
    }

    public static class ControlAssessments
    {
        public const string View = GroupName + ".ControlAssessments.View";
        public const string Create = GroupName + ".ControlAssessments.Create";
        public const string Update = GroupName + ".ControlAssessments.Update";
        public const string Delete = GroupName + ".ControlAssessments.Delete";
    }

    public static class Evidence
    {
        public const string View = GroupName + ".Evidence.View";
        public const string Create = GroupName + ".Evidence.Create";
        public const string Update = GroupName + ".Evidence.Update";
        public const string Submit = GroupName + ".Evidence.Submit";
        public const string Approve = GroupName + ".Evidence.Approve";
        public const string Delete = GroupName + ".Evidence.Delete";
    }

    public static class Risks
    {
        public const string View = GroupName + ".Risks.View";
        public const string Create = GroupName + ".Risks.Create";
        public const string Update = GroupName + ".Risks.Update";
        public const string Accept = GroupName + ".Risks.Accept";
        public const string Delete = GroupName + ".Risks.Delete";
    }

    public static class Audits
    {
        public const string View = GroupName + ".Audits.View";
        public const string Create = GroupName + ".Audits.Create";
        public const string Update = GroupName + ".Audits.Update";
        public const string Close = GroupName + ".Audits.Close";
        public const string Delete = GroupName + ".Audits.Delete";
    }

    public static class ActionPlans
    {
        public const string View = GroupName + ".ActionPlans.View";
        public const string Create = GroupName + ".ActionPlans.Create";
        public const string Update = GroupName + ".ActionPlans.Update";
        public const string Delete = GroupName + ".ActionPlans.Delete";
    }

    public static class Policies
    {
        public const string View = GroupName + ".Policies.View";
        public const string Create = GroupName + ".Policies.Create";
        public const string Update = GroupName + ".Policies.Update";
        public const string Approve = GroupName + ".Policies.Approve";
        public const string Publish = GroupName + ".Policies.Publish";
        public const string Delete = GroupName + ".Policies.Delete";
    }

    public static class ComplianceCalendar
    {
        public const string View = GroupName + ".ComplianceCalendar.View";
        public const string Manage = GroupName + ".ComplianceCalendar.Manage";
    }

    public static class Workflow
    {
        public const string View = GroupName + ".Workflow.View";
        public const string Design = GroupName + ".Workflow.Design";
        public const string Execute = GroupName + ".Workflow.Execute";
    }

    public static class Notifications
    {
        public const string View = GroupName + ".Notifications.View";
        public const string Manage = GroupName + ".Notifications.Manage";
    }

    public static class Vendors
    {
        public const string View = GroupName + ".Vendors.View";
        public const string Create = GroupName + ".Vendors.Create";
        public const string Update = GroupName + ".Vendors.Update";
        public const string Delete = GroupName + ".Vendors.Delete";
    }

    public static class Reports
    {
        public const string View = GroupName + ".Reports.View";
        public const string Create = GroupName + ".Reports.Create";
        public const string Export = GroupName + ".Reports.Export";
    }

    public static class Integrations
    {
        public const string View = GroupName + ".Integrations.View";
        public const string Manage = GroupName + ".Integrations.Manage";
    }
}
