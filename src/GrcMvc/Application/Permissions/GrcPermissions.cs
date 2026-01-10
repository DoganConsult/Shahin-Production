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

    // Dashboard - Advanced Monitoring Views
    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
        public const string Executive = Default + ".Executive";
        public const string Operations = Default + ".Operations";
        public const string Security = Default + ".Security";
        public const string DataQuality = Default + ".DataQuality";
        public const string TenantDrilldown = Default + ".TenantDrilldown";
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
        public const string Delete = Default + ".Delete";
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
        public const string Submit = Default + ".Submit";
        public const string Review = Default + ".Review";
        public const string Archive = Default + ".Archive";
    }

    // Risks
    public static class Risks
    {
        public const string Default = GroupName + ".Risks";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Accept = Default + ".Accept";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
        public const string Monitor = Default + ".Monitor";
        public const string Escalate = Default + ".Escalate";
    }

    // Audits
    public static class Audits
    {
        public const string Default = GroupName + ".Audits";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Close = Default + ".Close";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Fieldwork = Default + ".Fieldwork";
        public const string Report = Default + ".Report";
    }

    // Action Plans
    public static class ActionPlans
    {
        public const string Default = GroupName + ".ActionPlans";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Assign = Default + ".Assign";
        public const string Close = Default + ".Close";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    // Policies
    public static class Policies
    {
        public const string Default = GroupName + ".Policies";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Approve = Default + ".Approve";
        public const string Publish = Default + ".Publish";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Review = Default + ".Review";
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
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
        public const string Reject = Default + ".Reject";
        public const string AssignTask = Default + ".AssignTask";
        public const string Escalate = Default + ".Escalate";
        public const string Monitor = Default + ".Monitor";
    }

    // Controls
    public static class Controls
    {
        public const string Default = GroupName + ".Controls";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Implement = Default + ".Implement";
        public const string Test = Default + ".Test";
    }

    // Users
    public static class Users
    {
        public const string Default = GroupName + ".Users";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string AssignRole = Default + ".AssignRole";
    }

    // Roles
    public static class Roles
    {
        public const string Default = GroupName + ".Roles";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    // Permissions Management
    public static class Permissions
    {
        public const string Default = GroupName + ".Permissions";
        public const string Manage = Default + ".Manage";
    }

    // Features Management
    public static class Features
    {
        public const string Default = GroupName + ".Features";
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

    // Resilience
    public static class Resilience
    {
        public const string Default = GroupName + ".Resilience";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string AssessRTO = Default + ".AssessRTO";
        public const string AssessRPO = Default + ".AssessRPO";
        public const string ManageDrills = Default + ".ManageDrills";
        public const string ManagePlans = Default + ".ManagePlans";
        public const string Monitor = Default + ".Monitor";
    }

    // Reports
    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string View = Default + ".View";
        public const string Export = Default + ".Export";
        public const string Generate = Default + ".Generate";
    }

    // Certifications
    public static class Certification
    {
        public const string Default = GroupName + ".Certification";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Manage = Default + ".Manage";
        public const string Readiness = Default + ".Readiness";
    }

    // Maturity Assessment
    public static class Maturity
    {
        public const string Default = GroupName + ".Maturity";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Assess = Default + ".Assess";
        public const string Baseline = Default + ".Baseline";
        public const string Roadmap = Default + ".Roadmap";
    }

    // Excellence & Benchmarking
    public static class Excellence
    {
        public const string Default = GroupName + ".Excellence";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Manage = Default + ".Manage";
        public const string Benchmark = Default + ".Benchmark";
        public const string Assess = Default + ".Assess";
    }

    // Sustainability
    public static class Sustainability
    {
        public const string Default = GroupName + ".Sustainability";
        public const string View = Default + ".View";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Manage = Default + ".Manage";
        public const string Dashboard = Default + ".Dashboard";
        public const string KPIs = Default + ".KPIs";
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
        // Home & Dashboard
        yield return Home.Default;
        yield return Dashboard.Default;
        yield return Dashboard.Executive;
        yield return Dashboard.Operations;
        yield return Dashboard.Security;
        yield return Dashboard.DataQuality;
        yield return Dashboard.TenantDrilldown;

        // Subscriptions
        yield return Subscriptions.View;
        yield return Subscriptions.Manage;

        // Admin
        yield return Admin.Access;
        yield return Admin.Users;
        yield return Admin.Roles;
        yield return Admin.Tenants;

        // Frameworks
        yield return Frameworks.View;
        yield return Frameworks.Create;
        yield return Frameworks.Update;
        yield return Frameworks.Delete;
        yield return Frameworks.Import;

        // Regulators
        yield return Regulators.View;
        yield return Regulators.Manage;

        // Assessments
        yield return Assessments.View;
        yield return Assessments.Create;
        yield return Assessments.Update;
        yield return Assessments.Submit;
        yield return Assessments.Approve;
        yield return Assessments.Delete;

        // Control Assessments
        yield return ControlAssessments.View;
        yield return ControlAssessments.Manage;

        // Evidence
        yield return Evidence.View;
        yield return Evidence.Upload;
        yield return Evidence.Update;
        yield return Evidence.Delete;
        yield return Evidence.Approve;
        yield return Evidence.Submit;
        yield return Evidence.Review;
        yield return Evidence.Archive;

        // Risks
        yield return Risks.View;
        yield return Risks.Manage;
        yield return Risks.Accept;
        yield return Risks.Create;
        yield return Risks.Edit;
        yield return Risks.Delete;
        yield return Risks.Approve;
        yield return Risks.Monitor;
        yield return Risks.Escalate;

        // Audits
        yield return Audits.View;
        yield return Audits.Manage;
        yield return Audits.Close;
        yield return Audits.Create;
        yield return Audits.Edit;
        yield return Audits.Delete;
        yield return Audits.Fieldwork;
        yield return Audits.Report;

        // Action Plans
        yield return ActionPlans.View;
        yield return ActionPlans.Manage;
        yield return ActionPlans.Assign;
        yield return ActionPlans.Close;
        yield return ActionPlans.Create;
        yield return ActionPlans.Edit;
        yield return ActionPlans.Delete;

        // Policies
        yield return Policies.View;
        yield return Policies.Manage;
        yield return Policies.Approve;
        yield return Policies.Publish;
        yield return Policies.Create;
        yield return Policies.Edit;
        yield return Policies.Delete;
        yield return Policies.Review;

        // Compliance Calendar
        yield return ComplianceCalendar.View;
        yield return ComplianceCalendar.Manage;

        // Workflow
        yield return Workflow.View;
        yield return Workflow.Manage;
        yield return Workflow.Create;
        yield return Workflow.Edit;
        yield return Workflow.Delete;
        yield return Workflow.Approve;
        yield return Workflow.Reject;
        yield return Workflow.AssignTask;
        yield return Workflow.Escalate;
        yield return Workflow.Monitor;

        // Notifications
        yield return Notifications.View;
        yield return Notifications.Manage;

        // Vendors
        yield return Vendors.View;
        yield return Vendors.Manage;
        yield return Vendors.Assess;

        // Resilience
        yield return Resilience.View;
        yield return Resilience.Manage;
        yield return Resilience.Create;
        yield return Resilience.Edit;
        yield return Resilience.Delete;
        yield return Resilience.AssessRTO;
        yield return Resilience.AssessRPO;
        yield return Resilience.ManageDrills;
        yield return Resilience.ManagePlans;
        yield return Resilience.Monitor;

        // Reports
        yield return Reports.View;
        yield return Reports.Export;
        yield return Reports.Generate;

        // Certification
        yield return Certification.View;
        yield return Certification.Create;
        yield return Certification.Edit;
        yield return Certification.Delete;
        yield return Certification.Manage;
        yield return Certification.Readiness;

        // Maturity
        yield return Maturity.View;
        yield return Maturity.Create;
        yield return Maturity.Edit;
        yield return Maturity.Delete;
        yield return Maturity.Assess;
        yield return Maturity.Baseline;
        yield return Maturity.Roadmap;

        // Excellence
        yield return Excellence.View;
        yield return Excellence.Create;
        yield return Excellence.Edit;
        yield return Excellence.Delete;
        yield return Excellence.Manage;
        yield return Excellence.Benchmark;
        yield return Excellence.Assess;

        // Sustainability
        yield return Sustainability.View;
        yield return Sustainability.Create;
        yield return Sustainability.Edit;
        yield return Sustainability.Delete;
        yield return Sustainability.Manage;
        yield return Sustainability.Dashboard;
        yield return Sustainability.KPIs;

        // Integrations
        yield return Integrations.View;
        yield return Integrations.Manage;

        // Controls
        yield return Controls.View;
        yield return Controls.Create;
        yield return Controls.Edit;
        yield return Controls.Delete;
        yield return Controls.Implement;
        yield return Controls.Test;

        // Users
        yield return Users.View;
        yield return Users.Create;
        yield return Users.Edit;
        yield return Users.Delete;
        yield return Users.AssignRole;

        // Roles
        yield return Roles.View;
        yield return Roles.Create;
        yield return Roles.Edit;
        yield return Roles.Delete;

        // System
        yield return Permissions.Manage;
        yield return Features.Manage;
    }
}
