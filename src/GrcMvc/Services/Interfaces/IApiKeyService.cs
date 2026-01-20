using GrcMvc.Models.Entities;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// AM-02: API Key management service interface.
    /// Handles key creation, validation, and lifecycle management.
    /// </summary>
    public interface IApiKeyService
    {
        /// <summary>
        /// Create a new API key.
        /// </summary>
        /// <param name="request">Key creation request</param>
        /// <param name="createdBy">User ID creating the key</param>
        /// <returns>Created key info with plaintext key (shown only once)</returns>
        Task<ApiKeyCreationResult> CreateKeyAsync(ApiKeyCreateRequest request, Guid createdBy);

        /// <summary>
        /// Validate an API key and return the associated key entity if valid.
        /// </summary>
        /// <param name="plaintextKey">The API key to validate</param>
        /// <param name="ipAddress">Client IP address for IP restriction check</param>
        /// <returns>Validation result with key details if valid</returns>
        Task<ApiKeyValidationResult> ValidateKeyAsync(string plaintextKey, string? ipAddress = null);

        /// <summary>
        /// Revoke an API key.
        /// </summary>
        /// <param name="keyId">Key ID to revoke</param>
        /// <param name="revokedBy">User ID revoking the key</param>
        /// <param name="reason">Reason for revocation</param>
        Task<bool> RevokeKeyAsync(Guid keyId, Guid revokedBy, string reason);

        /// <summary>
        /// Get all API keys for a tenant.
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="includeRevoked">Include revoked keys</param>
        /// <returns>List of API keys (without hashes)</returns>
        Task<IEnumerable<ApiKeyInfo>> GetKeysByTenantAsync(Guid tenantId, bool includeRevoked = false);

        /// <summary>
        /// Get API key details by ID.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        /// <returns>Key info or null</returns>
        Task<ApiKeyInfo?> GetKeyByIdAsync(Guid keyId);

        /// <summary>
        /// Check rate limit for an API key.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        /// <returns>Rate limit status</returns>
        Task<ApiKeyRateLimitStatus> CheckRateLimitAsync(Guid keyId);

        /// <summary>
        /// Record API key usage.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        /// <param name="ipAddress">Client IP</param>
        /// <param name="endpoint">API endpoint accessed</param>
        Task RecordUsageAsync(Guid keyId, string? ipAddress, string? endpoint);

        /// <summary>
        /// Get usage statistics for an API key.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        /// <param name="days">Number of days to look back</param>
        /// <returns>Usage statistics</returns>
        Task<ApiKeyUsageStats> GetUsageStatsAsync(Guid keyId, int days = 30);

        /// <summary>
        /// Update API key settings.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        /// <param name="request">Update request</param>
        /// <param name="updatedBy">User ID making the update</param>
        Task<bool> UpdateKeyAsync(Guid keyId, ApiKeyUpdateRequest request, Guid updatedBy);

        /// <summary>
        /// Check if a domain is allowed for an API key.
        /// </summary>
        /// <param name="keyId">Key ID</param>
        /// <param name="domain">Domain to check</param>
        /// <returns>True if domain is allowed or no restrictions</returns>
        Task<bool> IsDomainAllowedAsync(Guid keyId, string domain);
    }

    /// <summary>
    /// Request to create a new API key.
    /// </summary>
    public class ApiKeyCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
        public string[]? AllowedDomains { get; set; }
        public string[]? AllowedIps { get; set; }
        public string[]? Scopes { get; set; }
        public int? MaxTenantsPerDay { get; set; }
        public int? MaxRequestsPerHour { get; set; }
        public int? ExpiresInDays { get; set; }
    }

    /// <summary>
    /// Request to update an API key.
    /// </summary>
    public class ApiKeyUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string[]? AllowedDomains { get; set; }
        public string[]? AllowedIps { get; set; }
        public string[]? Scopes { get; set; }
        public int? MaxTenantsPerDay { get; set; }
        public int? MaxRequestsPerHour { get; set; }
    }

    /// <summary>
    /// Result of API key creation.
    /// </summary>
    public class ApiKeyCreationResult
    {
        public bool Success { get; set; }
        public Guid? KeyId { get; set; }
        public string? PlaintextKey { get; set; } // Only returned once!
        public string? KeyPrefix { get; set; }
        public string? ErrorMessage { get; set; }

        public static ApiKeyCreationResult Succeeded(Guid keyId, string plaintextKey, string keyPrefix) => new()
        {
            Success = true,
            KeyId = keyId,
            PlaintextKey = plaintextKey,
            KeyPrefix = keyPrefix
        };

        public static ApiKeyCreationResult Failed(string error) => new()
        {
            Success = false,
            ErrorMessage = error
        };
    }

    /// <summary>
    /// Result of API key validation.
    /// </summary>
    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public Guid? KeyId { get; set; }
        public Guid? TenantId { get; set; }
        public string? KeyName { get; set; }
        public string[]? Scopes { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }

        public static ApiKeyValidationResult Valid(ApiKey key) => new()
        {
            IsValid = true,
            KeyId = key.Id,
            TenantId = key.TenantId,
            KeyName = key.Name,
            Scopes = key.GetScopes()
        };

        public static ApiKeyValidationResult Invalid(string errorCode, string errorMessage) => new()
        {
            IsValid = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };

        public static ApiKeyValidationResult NotFound() =>
            Invalid("invalid_key", "Invalid API key.");

        public static ApiKeyValidationResult Revoked() =>
            Invalid("key_revoked", "This API key has been revoked.");

        public static ApiKeyValidationResult Expired() =>
            Invalid("key_expired", "This API key has expired.");

        public static ApiKeyValidationResult IpNotAllowed() =>
            Invalid("ip_not_allowed", "Request from this IP address is not allowed.");

        public static ApiKeyValidationResult RateLimited() =>
            Invalid("rate_limited", "API key rate limit exceeded.");
    }

    /// <summary>
    /// API key information (safe to return to clients).
    /// </summary>
    public class ApiKeyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string KeyPrefix { get; set; } = string.Empty;
        public Guid? TenantId { get; set; }
        public string[]? AllowedDomains { get; set; }
        public string[]? Scopes { get; set; }
        public int? MaxTenantsPerDay { get; set; }
        public int? MaxRequestsPerHour { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public long UsageCount { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// API key rate limit status.
    /// </summary>
    public class ApiKeyRateLimitStatus
    {
        public bool IsLimited { get; set; }
        public int RequestsRemaining { get; set; }
        public int RequestsUsed { get; set; }
        public int RequestsLimit { get; set; }
        public DateTime WindowResetAt { get; set; }

        public static ApiKeyRateLimitStatus Allowed(int used, int limit, DateTime resetAt) => new()
        {
            IsLimited = false,
            RequestsUsed = used,
            RequestsRemaining = limit - used,
            RequestsLimit = limit,
            WindowResetAt = resetAt
        };

        public static ApiKeyRateLimitStatus Limited(int limit, DateTime resetAt) => new()
        {
            IsLimited = true,
            RequestsUsed = limit,
            RequestsRemaining = 0,
            RequestsLimit = limit,
            WindowResetAt = resetAt
        };
    }

    /// <summary>
    /// API key usage statistics.
    /// </summary>
    public class ApiKeyUsageStats
    {
        public Guid KeyId { get; set; }
        public long TotalRequests { get; set; }
        public int UniqueIps { get; set; }
        public int DaysActive { get; set; }
        public Dictionary<string, int> RequestsByEndpoint { get; set; } = new();
        public Dictionary<string, int> RequestsByDay { get; set; } = new();
    }
}
