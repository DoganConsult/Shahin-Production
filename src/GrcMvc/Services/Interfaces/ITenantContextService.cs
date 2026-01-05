using System;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service to get current tenant context from authenticated user
    /// </summary>
    public interface ITenantContextService
    {
        Guid GetCurrentTenantId();
        string GetCurrentUserId();
        string GetCurrentUserName();
        bool IsAuthenticated();
    }
}
