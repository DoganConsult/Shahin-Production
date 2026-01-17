using System.Collections.Generic;
using System.Threading.Tasks;
using GrcMvc.Permissions;

namespace GrcMvc.Menus;

/// <summary>
/// GRC Menu Contributor with Arabic menu items
/// Maps routes and permissions exactly as specified
/// </summary>
public class GrcMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var menu = new ApplicationMenu("GrcMain", "القائمة الرئيسية");

        // الصفحة الرئيسية - Home
        menu.AddItem(new ApplicationMenuItem(
            "Home",
            "الصفحة الرئيسية",
            url: "/",
            icon: "fas fa-home",
            requiredPermissionName: GrcPermissions.Home.Default,
            order: 100
        ));

        // لوحة التحكم - Dashboard
        menu.AddItem(new ApplicationMenuItem(
            "Dashboard",
            "لوحة التحكم",
            url: "/dashboard",
            icon: "fas fa-tachometer-alt",
            requiredPermissionName: GrcPermissions.Dashboard.Default,
            order: 200
        ));

        // الاشتراكات - Subscriptions
        menu.AddItem(new ApplicationMenuItem(
            "Subscriptions",
            "الاشتراكات",
            url: "/subscriptions",
            icon: "fas fa-credit-card",
            requiredPermissionName: GrcPermissions.Subscriptions.View,
            order: 300
        ));

        // الإدارة - Admin
        var adminMenu = new ApplicationMenuItem(
            "Admin",
            "الإدارة",
            url: "/admin",
            icon: "fas fa-cog",
            requiredPermissionName: GrcPermissions.Admin.Access,
            order: 400
        );
        adminMenu.AddItem(new ApplicationMenuItem(
            "Admin.Users",
            "إدارة المستخدمين",
            url: "/admin/users",
            requiredPermissionName: GrcPermissions.Admin.ManageUsers
        ));
        adminMenu.AddItem(new ApplicationMenuItem(
            "Admin.Roles",
            "إدارة الأدوار",
            url: "/admin/roles",
            requiredPermissionName: GrcPermissions.Admin.ManageRoles
        ));
        menu.AddItem(adminMenu);

        // مكتبة الأطر التنظيمية - Frameworks
        menu.AddItem(new ApplicationMenuItem(
            "Frameworks",
            "مكتبة الأطر التنظيمية",
            url: "/frameworks",
            icon: "fas fa-book",
            requiredPermissionName: GrcPermissions.Frameworks.View,
            order: 500
        ));

        // الجهات التنظيمية - Regulators
        menu.AddItem(new ApplicationMenuItem(
            "Regulators",
            "الجهات التنظيمية",
            url: "/regulators",
            icon: "fas fa-university",
            requiredPermissionName: GrcPermissions.Regulators.View,
            order: 600
        ));

        // التقييمات - Assessments
        var assessmentMenu = new ApplicationMenuItem(
            "Assessments",
            "التقييمات",
            url: "/assessments",
            icon: "fas fa-clipboard-check",
            requiredPermissionName: GrcPermissions.Assessments.View,
            order: 700
        );
        // تقييمات الضوابط - Control Assessments
        assessmentMenu.AddItem(new ApplicationMenuItem(
            "ControlAssessments",
            "تقييمات الضوابط",
            url: "/control-assessments",
            requiredPermissionName: GrcPermissions.ControlAssessments.View
        ));
        menu.AddItem(assessmentMenu);

        // الأدلة - Evidence
        menu.AddItem(new ApplicationMenuItem(
            "Evidence",
            "الأدلة",
            url: "/evidence",
            icon: "fas fa-file-alt",
            requiredPermissionName: GrcPermissions.Evidence.View,
            order: 800
        ));

        // إدارة المخاطر - Risks
        menu.AddItem(new ApplicationMenuItem(
            "Risks",
            "إدارة المخاطر",
            url: "/risks",
            icon: "fas fa-exclamation-triangle",
            requiredPermissionName: GrcPermissions.Risks.View,
            order: 900
        ));

        // إدارة المراجعة - Audits
        menu.AddItem(new ApplicationMenuItem(
            "Audits",
            "إدارة المراجعة",
            url: "/audits",
            icon: "fas fa-search",
            requiredPermissionName: GrcPermissions.Audits.View,
            order: 1000
        ));

        // خطط العمل - Action Plans
        menu.AddItem(new ApplicationMenuItem(
            "ActionPlans",
            "خطط العمل",
            url: "/action-plans",
            icon: "fas fa-tasks",
            requiredPermissionName: GrcPermissions.ActionPlans.View,
            order: 1100
        ));

        // إدارة السياسات - Policies
        menu.AddItem(new ApplicationMenuItem(
            "Policies",
            "إدارة السياسات",
            url: "/policies",
            icon: "fas fa-file-contract",
            requiredPermissionName: GrcPermissions.Policies.View,
            order: 1200
        ));

        // تقويم الامتثال - Compliance Calendar
        menu.AddItem(new ApplicationMenuItem(
            "ComplianceCalendar",
            "تقويم الامتثال",
            url: "/compliance-calendar",
            icon: "fas fa-calendar-check",
            requiredPermissionName: GrcPermissions.ComplianceCalendar.View,
            order: 1300
        ));

        // محرك سير العمل - Workflow
        menu.AddItem(new ApplicationMenuItem(
            "Workflow",
            "محرك سير العمل",
            url: "/workflow",
            icon: "fas fa-project-diagram",
            requiredPermissionName: GrcPermissions.Workflow.View,
            order: 1400
        ));

        // الإشعارات - Notifications
        menu.AddItem(new ApplicationMenuItem(
            "Notifications",
            "الإشعارات",
            url: "/notifications",
            icon: "fas fa-bell",
            requiredPermissionName: GrcPermissions.Notifications.View,
            order: 1500
        ));

        // إدارة الموردين - Vendors
        menu.AddItem(new ApplicationMenuItem(
            "Vendors",
            "إدارة الموردين",
            url: "/vendors",
            icon: "fas fa-truck",
            requiredPermissionName: GrcPermissions.Vendors.View,
            order: 1600
        ));

        // التقارير والتحليلات - Reports
        menu.AddItem(new ApplicationMenuItem(
            "Reports",
            "التقارير والتحليلات",
            url: "/reports",
            icon: "fas fa-chart-bar",
            requiredPermissionName: GrcPermissions.Reports.View,
            order: 1700
        ));

        // مركز التكامل - Integrations
        menu.AddItem(new ApplicationMenuItem(
            "Integrations",
            "مركز التكامل",
            url: "/integrations",
            icon: "fas fa-plug",
            requiredPermissionName: GrcPermissions.Integrations.View,
            order: 1800
        ));

        context.Menu = menu;
        await Task.CompletedTask;
    }
}

/// <summary>
/// Simple menu models for demonstration
/// In production, use ABP's IMenuManager or similar
/// </summary>
public class MenuConfigurationContext
{
    public ApplicationMenu? Menu { get; set; }
}

public class ApplicationMenu
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public List<ApplicationMenuItem> Items { get; set; } = new();

    public ApplicationMenu(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }

    public void AddItem(ApplicationMenuItem item)
    {
        Items.Add(item);
    }
}

public class ApplicationMenuItem
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public string? RequiredPermissionName { get; set; }
    public int Order { get; set; }
    public List<ApplicationMenuItem> Items { get; set; } = new();

    public ApplicationMenuItem(
        string name,
        string displayName,
        string? url = null,
        string? icon = null,
        string? requiredPermissionName = null,
        int order = 0)
    {
        Name = name;
        DisplayName = displayName;
        Url = url;
        Icon = icon;
        RequiredPermissionName = requiredPermissionName;
        Order = order;
    }

    public void AddItem(ApplicationMenuItem item)
    {
        Items.Add(item);
    }
}
