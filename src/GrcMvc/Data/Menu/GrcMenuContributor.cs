using GrcMvc.Application.Permissions;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace GrcMvc.Data.Menu;

/// <summary>
/// Menu contributor that builds navigation menu based on RBAC Features and Permissions.
/// Menu items are only shown if user has the required permission.
/// </summary>
public class GrcMenuContributor : IMenuContributor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GrcMenuContributor> _logger;

    public GrcMenuContributor(
        IServiceProvider serviceProvider,
        ILogger<GrcMenuContributor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        try
        {
            // Get current user's tenant and roles
            var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext == null || !(httpContext.User.Identity?.IsAuthenticated ?? false))
            {
                return;
            }

            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            // Get user's accessible features based on their roles
            var accessibleFeatures = await GetUserAccessibleFeaturesAsync(userId, context.ServiceProvider);

            // Build menu based on accessible features
            var rootMenu = context.Menu;

            // Home - Always visible if authenticated
            rootMenu.AddItem(new ApplicationMenuItem(
                "Grc.Home",
                "الصفحة الرئيسية",
                "/",
                icon: "fas fa-home")
                .RequirePermissions(GrcPermissions.Home.Default));

            // Dashboard
            if (accessibleFeatures.Contains("Dashboard"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Dashboard",
                    "لوحة التحكم",
                    "/dashboard",
                    icon: "fas fa-chart-line")
                    .RequirePermissions(GrcPermissions.Dashboard.Default));
            }

            // Subscriptions
            if (accessibleFeatures.Contains("Subscriptions"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Subscriptions",
                    "الاشتراكات",
                    "/subscriptions",
                    icon: "fas fa-id-card")
                    .RequirePermissions(GrcPermissions.Subscriptions.View));
            }

            // Admin Section
            if (accessibleFeatures.Contains("Admin"))
            {
                var adminMenu = new ApplicationMenuItem(
                    "Grc.Admin",
                    "الإدارة",
                    "/admin",
                    icon: "fas fa-cog")
                    .RequirePermissions(GrcPermissions.Admin.Access);

                adminMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Admin.Users",
                    "المستخدمون",
                    "/admin/users",
                    icon: "fas fa-users")
                    .RequirePermissions(GrcPermissions.Admin.Users));

                adminMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Admin.Roles",
                    "الأدوار",
                    "/admin/roles",
                    icon: "fas fa-user-shield")
                    .RequirePermissions(GrcPermissions.Admin.Roles));

                adminMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Admin.Tenants",
                    "العملاء",
                    "/admin/tenants",
                    icon: "fas fa-building")
                    .RequirePermissions(GrcPermissions.Admin.Tenants));

                rootMenu.AddItem(adminMenu);
            }

            // Frameworks
            if (accessibleFeatures.Contains("Frameworks"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Frameworks",
                    "مكتبة الأطر التنظيمية",
                    "/frameworks",
                    icon: "fas fa-layer-group")
                    .RequirePermissions(GrcPermissions.Frameworks.View));
            }

            // Regulators
            if (accessibleFeatures.Contains("Regulators"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Regulators",
                    "الجهات التنظيمية",
                    "/regulators",
                    icon: "fas fa-landmark")
                    .RequirePermissions(GrcPermissions.Regulators.View));
            }

            // Assessments
            if (accessibleFeatures.Contains("Assessments"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Assessments",
                    "التقييمات",
                    "/assessments",
                    icon: "fas fa-clipboard-check")
                    .RequirePermissions(GrcPermissions.Assessments.View));
            }

            // Control Assessments
            if (accessibleFeatures.Contains("ControlAssessments"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.ControlAssessments",
                    "تقييمات الضوابط",
                    "/control-assessments",
                    icon: "fas fa-tasks")
                    .RequirePermissions(GrcPermissions.ControlAssessments.View));
            }

            // Evidence
            if (accessibleFeatures.Contains("Evidence"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Evidence",
                    "الأدلة",
                    "/evidence",
                    icon: "fas fa-file-alt")
                    .RequirePermissions(GrcPermissions.Evidence.View));
            }

            // Risks
            if (accessibleFeatures.Contains("Risks"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Risks",
                    "إدارة المخاطر",
                    "/risks",
                    icon: "fas fa-exclamation-triangle")
                    .RequirePermissions(GrcPermissions.Risks.View));
            }

            // Audits
            if (accessibleFeatures.Contains("Audits"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Audits",
                    "إدارة المراجعة",
                    "/audits",
                    icon: "fas fa-search")
                    .RequirePermissions(GrcPermissions.Audits.View));
            }

            // Action Plans
            if (accessibleFeatures.Contains("ActionPlans"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.ActionPlans",
                    "خطط العمل",
                    "/action-plans",
                    icon: "fas fa-project-diagram")
                    .RequirePermissions(GrcPermissions.ActionPlans.View));
            }

            // Policies
            if (accessibleFeatures.Contains("Policies"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Policies",
                    "إدارة السياسات",
                    "/policies",
                    icon: "fas fa-gavel")
                    .RequirePermissions(GrcPermissions.Policies.View));
            }

            // Compliance Calendar
            if (accessibleFeatures.Contains("ComplianceCalendar"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.ComplianceCalendar",
                    "تقويم الامتثال",
                    "/compliance-calendar",
                    icon: "fas fa-calendar-alt")
                    .RequirePermissions(GrcPermissions.ComplianceCalendar.View));
            }

            // Workflow
            if (accessibleFeatures.Contains("Workflow"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Workflow",
                    "محرك سير العمل",
                    "/workflow",
                    icon: "fas fa-sitemap")
                    .RequirePermissions(GrcPermissions.Workflow.View));
            }

            // Notifications
            if (accessibleFeatures.Contains("Notifications"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Notifications",
                    "الإشعارات",
                    "/notifications",
                    icon: "fas fa-bell")
                    .RequirePermissions(GrcPermissions.Notifications.View));
            }

            // Vendors
            if (accessibleFeatures.Contains("Vendors"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Vendors",
                    "إدارة الموردين",
                    "/vendors",
                    icon: "fas fa-handshake")
                    .RequirePermissions(GrcPermissions.Vendors.View));
            }

            // Reports
            if (accessibleFeatures.Contains("Reports"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Reports",
                    "التقارير والتحليلات",
                    "/reports",
                    icon: "fas fa-chart-pie")
                    .RequirePermissions(GrcPermissions.Reports.View));
            }

            // Integrations
            if (accessibleFeatures.Contains("Integrations"))
            {
                rootMenu.AddItem(new ApplicationMenuItem(
                    "Grc.Integrations",
                    "مركز التكامل",
                    "/integrations",
                    icon: "fas fa-plug")
                    .RequirePermissions(GrcPermissions.Integrations.View));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring GRC menu");
            // Don't throw - allow menu to render with basic items
        }
    }

    /// <summary>
    /// Gets list of feature codes that the user can access based on their roles
    /// </summary>
    private async Task<HashSet<string>> GetUserAccessibleFeaturesAsync(
        string userId,
        IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GrcDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new HashSet<string> { "Home", "Dashboard" }; // Default features
            }

            // Get user's roles
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                return new HashSet<string> { "Home", "Dashboard" };
            }

            // Get tenant ID from TenantUser
            var tenantUser = await context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);
            var tenantId = tenantUser?.TenantId ?? Guid.Empty;

            // Get all features accessible by user's roles
            var accessibleFeatures = await context.RoleFeatures
                .Include(rf => rf.Feature)
                .Where(rf => userRoles.Contains(rf.Role.Name) && rf.TenantId == tenantId && rf.IsVisible)
                .Select(rf => rf.Feature.Code)
                .Distinct()
                .ToListAsync();

            // Always include Home and Dashboard
            accessibleFeatures.Add("Home");
            accessibleFeatures.Add("Dashboard");

            return accessibleFeatures.ToHashSet();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user accessible features");
            // Return minimal features on error
            return new HashSet<string> { "Home", "Dashboard" };
        }
    }
}
