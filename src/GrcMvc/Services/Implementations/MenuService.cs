using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Service that builds navigation menu based on RBAC Features and Permissions
/// </summary>
public class MenuService : IMenuService
{
    private readonly GrcDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<MenuService> _logger;

    public MenuService(
        GrcDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<MenuService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<MenuItemDto>> GetUserMenuItemsAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return GetDefaultMenuItems();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                return GetDefaultMenuItems();
            }

            // Get tenant from TenantUser
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);
            var tenantId = tenantUser?.TenantId ?? Guid.Empty;

            // Get role IDs for user's role names (lookup from auth DB via UserManager)
            var allRoles = await _context.Set<Microsoft.AspNetCore.Identity.IdentityRole>().ToListAsync();
            var userRoleIds = allRoles.Where(r => userRoles.Contains(r.Name)).Select(r => r.Id).ToList();

            // Get all features accessible by user's roles (using RoleId instead of navigation)
            var accessibleFeatures = await _context.RoleFeatures
                .Include(rf => rf.Feature)
                .Where(rf => userRoleIds.Contains(rf.RoleId) && rf.TenantId == tenantId && rf.IsVisible)
                .Select(rf => rf.Feature.Code)
                .Distinct()
                .ToListAsync();

            // Always include Home and Dashboard
            accessibleFeatures.Add("Home");
            accessibleFeatures.Add("Dashboard");

            // Build menu items based on accessible features
            var menuItems = new List<MenuItemDto>();

            // Home
            menuItems.Add(new MenuItemDto
            {
                Id = "Grc.Home",
                Name = "Home",
                NameAr = "الصفحة الرئيسية",
                Url = "/",
                Icon = "fas fa-home",
                Permission = "Grc.Home",
                Order = 0
            });

            // Dashboard - Order 1 (Audit Story: Overview)
            if (accessibleFeatures.Contains("Dashboard"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Dashboard",
                    Name = "Dashboard",
                    NameAr = "لوحة التحكم",
                    Url = "/dashboard",
                    Icon = "fas fa-chart-line",
                    Permission = "Grc.Dashboard",
                    Order = 1
                });
            }

            // Assessments - Order 2 (Audit Story: Define what needs assessment)
            if (accessibleFeatures.Contains("Assessments"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Assessments",
                    Name = "Assessments",
                    NameAr = "التقييمات",
                    Url = "/assessments",
                    Icon = "fas fa-clipboard-check",
                    Permission = "Grc.Assessments.View",
                    Order = 2
                });
            }

            // Control Assessments - Order 3 (Audit Story: Evaluate control effectiveness)
            if (accessibleFeatures.Contains("ControlAssessments"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.ControlAssessments",
                    Name = "Control Assessments",
                    NameAr = "تقييمات الضوابط",
                    Url = "/control-assessments",
                    Icon = "fas fa-tasks",
                    Permission = "Grc.ControlAssessments.View",
                    Order = 3
                });
            }

            // Evidence - Order 4 (Audit Story: Prove compliance with evidence)
            if (accessibleFeatures.Contains("Evidence"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Evidence",
                    Name = "Evidence",
                    NameAr = "الأدلة",
                    Url = "/evidence",
                    Icon = "fas fa-file-alt",
                    Permission = "Grc.Evidence.View",
                    Order = 4
                });
            }

            // Risks - Order 5 (Audit Story: Identify and manage risks)
            if (accessibleFeatures.Contains("Risks"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Risks",
                    Name = "Risk Management",
                    NameAr = "إدارة المخاطر",
                    Url = "/risks",
                    Icon = "fas fa-exclamation-triangle",
                    Permission = "Grc.Risks.View",
                    Order = 5
                });
            }

            // Action Plans - Order 6 (Audit Story: Remediate gaps) - MUST be BEFORE Audits
            if (accessibleFeatures.Contains("ActionPlans"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.ActionPlans",
                    Name = "Action Plans",
                    NameAr = "خطط العمل",
                    Url = "/action-plans",
                    Icon = "fas fa-project-diagram",
                    Permission = "Grc.ActionPlans.View",
                    Order = 6
                });
            }

            // Policies - Order 7 (Audit Story: Governance documents)
            if (accessibleFeatures.Contains("Policies"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Policies",
                    Name = "Policies",
                    NameAr = "إدارة السياسات",
                    Url = "/policies",
                    Icon = "fas fa-gavel",
                    Permission = "Grc.Policies.View",
                    Order = 7
                });
            }

            // Audits - Order 8 (Audit Story: External/internal audits) - MUST be AFTER Action Plans
            if (accessibleFeatures.Contains("Audits"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Audits",
                    Name = "Audits",
                    NameAr = "إدارة المراجعة",
                    Url = "/audits",
                    Icon = "fas fa-search",
                    Permission = "Grc.Audits.View",
                    Order = 8
                });
            }

            // Reports - Order 9 (Audit Story: Generate compliance reports)
            if (accessibleFeatures.Contains("Reports"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Reports",
                    Name = "Reports & Analytics",
                    NameAr = "التقارير والتحليلات",
                    Url = "/reports",
                    Icon = "fas fa-chart-pie",
                    Permission = "Grc.Reports.View",
                    Order = 9
                });
            }

            // Administration - Order 10 (Audit Story: System management)
            if (accessibleFeatures.Contains("Admin"))
            {
                var adminMenu = new MenuItemDto
                {
                    Id = "Grc.Admin",
                    Name = "Administration",
                    NameAr = "الإدارة",
                    Url = "/admin",
                    Icon = "fas fa-cog",
                    Permission = "Grc.Admin.Access",
                    Order = 10,
                    Children = new List<MenuItemDto>
                    {
                        new MenuItemDto
                        {
                            Id = "Grc.Admin.Users",
                            Name = "Users",
                            NameAr = "المستخدمون",
                            Url = "/admin/users",
                            Icon = "fas fa-users",
                            Permission = "Grc.Admin.Users",
                            Order = 1
                        },
                        new MenuItemDto
                        {
                            Id = "Grc.Admin.Roles",
                            Name = "Roles",
                            NameAr = "الأدوار",
                            Url = "/admin/roles",
                            Icon = "fas fa-user-shield",
                            Permission = "Grc.Admin.Roles",
                            Order = 2
                        },
                        new MenuItemDto
                        {
                            Id = "Grc.Admin.Tenants",
                            Name = "Tenants",
                            NameAr = "العملاء",
                            Url = "/admin/tenants",
                            Icon = "fas fa-building",
                            Permission = "Grc.Admin.Tenants",
                            Order = 3
                        }
                    }
                };
                menuItems.Add(adminMenu);
            }

            // Subscriptions - Order 11 (Supporting feature)
            if (accessibleFeatures.Contains("Subscriptions"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Subscriptions",
                    Name = "Subscriptions",
                    NameAr = "الاشتراكات",
                    Url = "/subscriptions",
                    Icon = "fas fa-id-card",
                    Permission = "Grc.Subscriptions.View",
                    Order = 11
                });
            }

            // Frameworks - Order 12 (Supporting feature)
            if (accessibleFeatures.Contains("Frameworks"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Frameworks",
                    Name = "Regulatory Frameworks",
                    NameAr = "مكتبة الأطر التنظيمية",
                    Url = "/frameworks",
                    Icon = "fas fa-layer-group",
                    Permission = "Grc.Frameworks.View",
                    Order = 12
                });
            }

            // Regulators - Order 13 (Supporting feature)
            if (accessibleFeatures.Contains("Regulators"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Regulators",
                    Name = "Regulators",
                    NameAr = "الجهات التنظيمية",
                    Url = "/regulators",
                    Icon = "fas fa-landmark",
                    Permission = "Grc.Regulators.View",
                    Order = 13
                });
            }

            // Compliance Calendar - Order 14 (Supporting feature)
            if (accessibleFeatures.Contains("ComplianceCalendar"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.ComplianceCalendar",
                    Name = "Compliance Calendar",
                    NameAr = "تقويم الامتثال",
                    Url = "/compliance-calendar",
                    Icon = "fas fa-calendar-alt",
                    Permission = "Grc.ComplianceCalendar.View",
                    Order = 14
                });
            }

            // Workflow - Order 15 (Supporting feature)
            if (accessibleFeatures.Contains("Workflow"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Workflow",
                    Name = "Workflows",
                    NameAr = "محرك سير العمل",
                    Url = "/workflow",
                    Icon = "fas fa-sitemap",
                    Permission = "Grc.Workflow.View",
                    Order = 15
                });
            }

            // Notifications - Order 16 (Supporting feature)
            if (accessibleFeatures.Contains("Notifications"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Notifications",
                    Name = "Notifications",
                    NameAr = "الإشعارات",
                    Url = "/notifications",
                    Icon = "fas fa-bell",
                    Permission = "Grc.Notifications.View",
                    Order = 16
                });
            }

            // Vendors - Order 17 (Supporting feature)
            if (accessibleFeatures.Contains("Vendors"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Vendors",
                    Name = "Vendors",
                    NameAr = "إدارة الموردين",
                    Url = "/vendors",
                    Icon = "fas fa-handshake",
                    Permission = "Grc.Vendors.View",
                    Order = 17
                });
            }

            // Integrations - Order 18 (Supporting feature)
            if (accessibleFeatures.Contains("Integrations"))
            {
                menuItems.Add(new MenuItemDto
                {
                    Id = "Grc.Integrations",
                    Name = "Integrations",
                    NameAr = "مركز التكامل",
                    Url = "/integrations",
                    Icon = "fas fa-plug",
                    Permission = "Grc.Integrations.View",
                    Order = 18
                });
            }

            // Sort by Order (audit story flow)
            var sortedMenuItems = menuItems.OrderBy(m => m.Order).ToList();

            // #region agent log
            _logger.LogInformation("Menu items ordered for user {UserId}. Total items: {Count}. Order sequence: {OrderSequence}",
                userId,
                sortedMenuItems.Count,
                string.Join(", ", sortedMenuItems.Select(m => $"{m.Name}({m.Order})")));
            // #endregion

            return sortedMenuItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user menu items");
            return GetDefaultMenuItems();
        }
    }

    public async Task<bool> HasFeatureAccessAsync(string userId, string featureCode)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any()) return false;

            // Get tenant from TenantUser
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);
            var tenantId = tenantUser?.TenantId ?? Guid.Empty;

            // Always allow Home and Dashboard
            if (featureCode == "Home" || featureCode == "Dashboard")
                return true;

            // Get role IDs for user's role names
            var allRoles = await _context.Set<Microsoft.AspNetCore.Identity.IdentityRole>().ToListAsync();
            var userRoleIds = allRoles.Where(r => userRoles.Contains(r.Name)).Select(r => r.Id).ToList();

            return await _context.RoleFeatures
                .Include(rf => rf.Feature)
                .AnyAsync(rf => userRoleIds.Contains(rf.RoleId) &&
                               rf.Feature.Code == featureCode &&
                               rf.TenantId == tenantId &&
                               rf.IsVisible);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature access");
            return false;
        }
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionCode)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any()) return false;

            // Get tenant from TenantUser
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);
            var tenantId = tenantUser?.TenantId ?? Guid.Empty;

            // Get role IDs for user's role names
            var allRoles = await _context.Set<Microsoft.AspNetCore.Identity.IdentityRole>().ToListAsync();
            var userRoleIds = allRoles.Where(r => userRoles.Contains(r.Name)).Select(r => r.Id).ToList();

            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => userRoleIds.Contains(rp.RoleId) &&
                               rp.Permission.Code == permissionCode &&
                               rp.TenantId == tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission");
            return false;
        }
    }

    private List<MenuItemDto> GetDefaultMenuItems()
    {
        return new List<MenuItemDto>
        {
            new MenuItemDto
            {
                Id = "Grc.Home",
                Name = "Home",
                NameAr = "الصفحة الرئيسية",
                Url = "/",
                Icon = "fas fa-home",
                Order = 0
            },
            new MenuItemDto
            {
                Id = "Grc.Dashboard",
                Name = "Dashboard",
                NameAr = "لوحة التحكم",
                Url = "/dashboard",
                Icon = "fas fa-chart-line",
                Order = 1
            }
        };
    }
}
