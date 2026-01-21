using Hangfire.Dashboard;

namespace GrcMvc.Security
{
    /// <summary>
    /// Authorization filter for Hangfire Dashboard
    /// Restricts access to Admin role only
    /// </summary>
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Require authenticated user
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Require Admin role
            if (!httpContext.User.IsInRole("Admin"))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Extended authorization filter with IP restrictions
    /// </summary>
    public class HangfireSecureAuthFilter : IDashboardAuthorizationFilter
    {
        private readonly string[] _allowedIpAddresses;
        private readonly bool _requireAdmin;

        public HangfireSecureAuthFilter(string[]? allowedIpAddresses = null, bool requireAdmin = true)
        {
            _allowedIpAddresses = allowedIpAddresses ?? Array.Empty<string>();
            _requireAdmin = requireAdmin;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Always require authentication
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Check admin role if required
            if (_requireAdmin && !httpContext.User.IsInRole("Admin"))
            {
                return false;
            }

            // Check IP whitelist if configured
            if (_allowedIpAddresses.Length > 0)
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrEmpty(remoteIp))
                {
                    return false;
                }

                // Allow localhost
                if (remoteIp == "127.0.0.1" || remoteIp == "::1")
                {
                    return true;
                }

                // Check against whitelist
                if (!_allowedIpAddresses.Contains(remoteIp))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
