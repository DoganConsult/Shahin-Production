using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace GrcMvc.Models.Entities
{
    /// <summary>
    /// AM-02: API Key entity for secure trial provisioning and API access.
    /// Keys are hashed for storage; only prefix is stored for identification.
    /// </summary>
    [Table("ApiKeys")]
    public class ApiKey
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Human-readable name for the API key (e.g., "Partner Integration").
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this key is used for.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// SHA-256 hash of the API key. Never store plaintext keys.
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string KeyHash { get; set; } = string.Empty;

        /// <summary>
        /// Key prefix for identification (e.g., "sk_live_abc123...").
        /// First 12 characters of the key for display purposes.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string KeyPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Tenant ID this key belongs to. Null for platform-level keys.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Allowed domain patterns for provisioning (e.g., "*.partner.com").
        /// Stored as JSON array.
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? AllowedDomainsJson { get; set; }

        /// <summary>
        /// Allowed IP addresses or CIDR ranges.
        /// Stored as JSON array.
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? AllowedIpsJson { get; set; }

        /// <summary>
        /// Scopes/permissions granted to this key.
        /// Stored as JSON array (e.g., ["trial:provision", "user:read"]).
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? ScopesJson { get; set; }

        /// <summary>
        /// Maximum number of tenants this key can provision per day.
        /// Null means unlimited.
        /// </summary>
        public int? MaxTenantsPerDay { get; set; }

        /// <summary>
        /// Maximum API requests per hour for rate limiting.
        /// Null means use default limits.
        /// </summary>
        public int? MaxRequestsPerHour { get; set; }

        /// <summary>
        /// When the API key was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID who created the key.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// When the API key expires. Null means no expiry.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// When the API key was revoked.
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Reason for revocation.
        /// </summary>
        [MaxLength(500)]
        public string? RevocationReason { get; set; }

        /// <summary>
        /// User ID who revoked the key.
        /// </summary>
        public Guid? RevokedBy { get; set; }

        /// <summary>
        /// Last time this key was used.
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// Last IP address that used this key.
        /// </summary>
        [MaxLength(45)]
        public string? LastUsedIp { get; set; }

        /// <summary>
        /// Total number of times this key has been used.
        /// </summary>
        public long UsageCount { get; set; } = 0;

        /// <summary>
        /// Whether the key is currently active.
        /// </summary>
        public bool IsActive => RevokedAt == null && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);

        /// <summary>
        /// Navigation property to Tenant.
        /// </summary>
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Create a new API key with secure random value.
        /// </summary>
        /// <param name="name">Key name</param>
        /// <param name="createdBy">Creator user ID</param>
        /// <param name="tenantId">Optional tenant ID</param>
        /// <param name="expiresInDays">Optional expiry in days</param>
        /// <returns>Tuple of (API key entity, plaintext key for one-time display)</returns>
        public static (ApiKey Key, string PlaintextKey) Create(
            string name,
            Guid createdBy,
            Guid? tenantId = null,
            int? expiresInDays = null)
        {
            // Generate a secure random key
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            // Create key in format: sk_live_{base64url}
            var keyBase64 = Convert.ToBase64String(keyBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            var prefix = tenantId.HasValue ? "sk_tenant_" : "sk_platform_";
            var plaintextKey = $"{prefix}{keyBase64}";

            // Hash for storage
            var keyHash = HashKey(plaintextKey);

            var apiKey = new ApiKey
            {
                Name = name,
                KeyHash = keyHash,
                KeyPrefix = plaintextKey.Substring(0, Math.Min(20, plaintextKey.Length)),
                TenantId = tenantId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresInDays.HasValue ? DateTime.UtcNow.AddDays(expiresInDays.Value) : null
            };

            return (apiKey, plaintextKey);
        }

        /// <summary>
        /// Hash an API key using SHA-256.
        /// </summary>
        public static string HashKey(string plaintextKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(plaintextKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Verify a plaintext key against this key's hash.
        /// </summary>
        public bool VerifyKey(string plaintextKey)
        {
            var hash = HashKey(plaintextKey);
            return string.Equals(KeyHash, hash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Revoke this API key.
        /// </summary>
        public void Revoke(Guid revokedBy, string reason)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
            RevocationReason = reason;
        }

        /// <summary>
        /// Record usage of this key.
        /// </summary>
        public void RecordUsage(string? ipAddress)
        {
            LastUsedAt = DateTime.UtcNow;
            LastUsedIp = ipAddress;
            UsageCount++;
        }

        /// <summary>
        /// Get allowed domains as array.
        /// </summary>
        public string[] GetAllowedDomains()
        {
            if (string.IsNullOrEmpty(AllowedDomainsJson))
                return Array.Empty<string>();

            return System.Text.Json.JsonSerializer.Deserialize<string[]>(AllowedDomainsJson)
                   ?? Array.Empty<string>();
        }

        /// <summary>
        /// Set allowed domains from array.
        /// </summary>
        public void SetAllowedDomains(string[] domains)
        {
            AllowedDomainsJson = System.Text.Json.JsonSerializer.Serialize(domains);
        }

        /// <summary>
        /// Get scopes as array.
        /// </summary>
        public string[] GetScopes()
        {
            if (string.IsNullOrEmpty(ScopesJson))
                return Array.Empty<string>();

            return System.Text.Json.JsonSerializer.Deserialize<string[]>(ScopesJson)
                   ?? Array.Empty<string>();
        }

        /// <summary>
        /// Set scopes from array.
        /// </summary>
        public void SetScopes(string[] scopes)
        {
            ScopesJson = System.Text.Json.JsonSerializer.Serialize(scopes);
        }

        /// <summary>
        /// Check if this key has a specific scope.
        /// </summary>
        public bool HasScope(string scope)
        {
            var scopes = GetScopes();
            return scopes.Contains(scope, StringComparer.OrdinalIgnoreCase) ||
                   scopes.Contains("*", StringComparer.OrdinalIgnoreCase);
        }
    }
}
