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

                // Default roles - using Home controller (guaranteed to exist)
                ["Employee"] = async (user) => ("Home", "Index", null),
                ["Guest"] = async (user) => ("Home", "Index", null)
            };
        }

        public async Task<(string Controller, string Action, object? RouteValues)> GetRouteForUserAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {Email} has roles: {Roles}", user.Email, string.Join(", ", roles));

            // First check if user needs onboarding (before role-based routing)
            var tenantUser = await _context.TenantUsers
                .Include(tu => tu.Tenant)
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id && !tu.IsDeleted);

            if (tenantUser == null)
            {
                _logger.LogInformation("User {Email} has no tenant - routing to onboarding", user.Email);
                return ("Onboarding", "Index", null);
            }

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

            // Default route for authenticated users without specific roles
            // Redirect to Home/Index which is a known, working endpoint
            _logger.LogInformation("User {Email} using default route to Home", user.Email);
            return ("Home", "Index", null);
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

    }
}
