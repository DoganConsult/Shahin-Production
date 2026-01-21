using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Constants;

namespace GrcMvc.Services
{
    /// <summary>
    /// Centralized service for role-based post-login routing
    /// Following ASP.NET Core best practices
    /// </summary>
    public interface IPostLoginRoutingService
    {
        Task<(string Controller, string Action, object? RouteValues)> GetRouteForUserAsync(ApplicationUser user);
    }

    public class PostLoginRoutingService : IPostLoginRoutingService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GrcDbContext _context;
        private readonly ILogger<PostLoginRoutingService> _logger;

        // Define role routing configuration
        private readonly Dictionary<string, Func<ApplicationUser, Task<(string, string, object?)>>> _roleRoutes;

        public PostLoginRoutingService(
            UserManager<ApplicationUser> userManager,
            GrcDbContext context,
            ILogger<PostLoginRoutingService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;

            // Configure role-based routing rules
            _roleRoutes = new Dictionary<string, Func<ApplicationUser, Task<(string, string, object?)>>>
            {
                // Platform-level roles
                ["PlatformAdmin"] = async (user) => ("PlatformAdmin", "Dashboard", null),
                ["SystemAdministrator"] = async (user) => ("Admin", "SystemDashboard", null),

                // Tenant-level roles
                ["TenantAdmin"] = GetTenantAdminRoute,
                ["TenantOwner"] = GetTenantOwnerRoute,

                // Executive Layer
                ["ChiefRiskOfficer"] = async (user) => ("Executive", "RiskDashboard", null),
                ["ChiefComplianceOfficer"] = async (user) => ("Executive", "ComplianceDashboard", null),
                ["ExecutiveDirector"] = async (user) => ("Executive", "Dashboard", null),

                // Management Layer
                ["RiskManager"] = async (user) => ("Risk", "Dashboard", null),
                ["ComplianceManager"] = async (user) => ("Compliance", "Dashboard", null),
                ["AuditManager"] = async (user) => ("Audit", "Dashboard", null),
                ["SecurityManager"] = async (user) => ("Security", "Dashboard", null),
                ["LegalManager"] = async (user) => ("Legal", "Dashboard", null),

                // Operational Layer
                ["ComplianceOfficer"] = async (user) => ("Compliance", "MyTasks", null),
                ["RiskAnalyst"] = async (user) => ("Risk", "Analysis", null),
                ["PrivacyOfficer"] = async (user) => ("Privacy", "Dashboard", null),
                ["QualityAssuranceManager"] = async (user) => ("QA", "Dashboard", null),
                ["ProcessOwner"] = async (user) => ("Process", "MyProcesses", null),

                // Support Layer
                ["OperationsSupport"] = async (user) => ("Support", "TicketQueue", null),
                ["SystemObserver"] = async (user) => ("Reports", "ViewOnly", null),

                // Default roles
                ["Employee"] = async (user) => ("Dashboard", "Index", null),
                ["Guest"] = async (user) => ("Home", "Limited", null)
            };
        }

        public async Task<(string Controller, string Action, object? RouteValues)> GetRouteForUserAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {Email} has roles: {Roles}", user.Email, string.Join(", ", roles));

            // Priority-based role checking (highest privilege first)
            var priorityRoles = new[]
            {
                "PlatformAdmin",
                "SystemAdministrator",
                "TenantOwner",
                "TenantAdmin",
                "ChiefRiskOfficer",
                "ChiefComplianceOfficer",
                "ExecutiveDirector",
                "RiskManager",
                "ComplianceManager",
                "AuditManager",
                "SecurityManager",
                "LegalManager",
                "ComplianceOfficer",
                "RiskAnalyst",
                "PrivacyOfficer",
                "QualityAssuranceManager",
                "ProcessOwner",
                "OperationsSupport",
                "SystemObserver",
                "Employee",
                "Guest"
            };

            // Check roles in priority order
            foreach (var role in priorityRoles)
            {
                if (roles.Contains(role) && _roleRoutes.ContainsKey(role))
                {
                    _logger.LogInformation("Routing user {Email} based on role {Role}", user.Email, role);
                    return await _roleRoutes[role](user);
                }
            }

            // Check if user needs onboarding
            if (await NeedsOnboarding(user))
            {
                _logger.LogInformation("User {Email} needs onboarding", user.Email);
                return ("Onboarding", "Start", null);
            }

            // Default route for authenticated users without specific roles
            _logger.LogInformation("User {Email} using default route", user.Email);
            return ("Dashboard", "Index", null);
        }

        private async Task<(string, string, object?)> GetTenantAdminRoute(ApplicationUser user)
        {
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            if (tenantUser?.Tenant != null)
            {
                _logger.LogInformation("TenantAdmin {Email} routing to tenant {TenantSlug}",
                    user.Email, tenantUser.Tenant.TenantSlug);
                return ("TenantAdmin", "Dashboard", new { tenantSlug = tenantUser.Tenant.TenantSlug });
            }

            _logger.LogWarning("TenantAdmin {Email} has no tenant, routing to onboarding", user.Email);
            return ("Onboarding", "CreateTenant", null);
        }

        private async Task<(string, string, object?)> GetTenantOwnerRoute(ApplicationUser user)
        {
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && tu.IsOwnerGenerated && !tu.IsDeleted);

            if (tenantUser?.Tenant != null)
            {
                return ("TenantOwner", "Dashboard", new { tenantSlug = tenantUser.Tenant.TenantSlug });
            }

            return ("Onboarding", "CreateTenant", null);
        }

        private async Task<bool> NeedsOnboarding(ApplicationUser user)
        {
            // Check if user has completed onboarding
            var hasWorkspace = await _context.UserWorkspaces
                .AnyAsync(uw => uw.UserId == user.Id && uw.IsConfigured && !uw.IsDeleted);

            var hasTenant = await _context.TenantUsers
                .AnyAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            // User needs onboarding if they don't have workspace or tenant
            return !hasWorkspace && !hasTenant;
        }
    }
}
