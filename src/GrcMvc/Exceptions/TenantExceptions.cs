using System;

namespace GrcMvc.Exceptions
{
    /// <summary>
    /// Exception thrown when tenant context is required but not available.
    /// Maps to HTTP 400 Bad Request.
    /// </summary>
    public class TenantRequiredException : InvalidOperationException
    {
        public int SuggestedStatusCode => 400;
        public string ErrorCode => "TENANT_REQUIRED";

        public TenantRequiredException() : base("Tenant context is required but not available.") { }

        public TenantRequiredException(string message) : base(message) { }

        public TenantRequiredException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when tenant context is invalid or user is not authorized for the tenant.
    /// Maps to HTTP 403 Forbidden.
    /// </summary>
    public class TenantForbiddenException : UnauthorizedAccessException
    {
        public int SuggestedStatusCode => 403;
        public string ErrorCode => "TENANT_FORBIDDEN";
        
        public TenantForbiddenException(string message) : base(message) { }
        
        public TenantForbiddenException(string message, Exception innerException) : base(message, innerException) { }
    }
}
