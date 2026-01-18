using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// System-generated business reference code service for all GRC artifacts.
    /// Ensures unique, sequential, and tamper-proof document numbers.
    /// 
    /// NEW FORMAT: {TENANTCODE}-{OBJECTTYPE}-{YYYY}-{SEQUENCE:D6}
    /// Example: ACME-CTRL-2026-000143
    /// 
    /// LEGACY FORMAT (still supported): {PREFIX}-{YYYYMMDD}-{SEQUENCE:D4}
    /// Example: ASM-20260106-0001
    /// </summary>
    public interface ISerialNumberService
    {
        // Legacy methods (keep for backward compatibility)
        Task<string> GenerateAssessmentNumberAsync(Guid tenantId);
        Task<string> GeneratePlanNumberAsync(Guid tenantId);
        Task<string> GenerateReportNumberAsync(Guid tenantId);
        Task<string> GenerateEvidenceNumberAsync(Guid tenantId);
        Task<string> GenerateAuditNumberAsync(Guid tenantId);
        Task<string> GenerateFindingNumberAsync(Guid tenantId);
        Task<string> GeneratePolicyNumberAsync(Guid tenantId);
        Task<string> GenerateWorkflowInstanceNumberAsync(Guid tenantId);
        Task<string> GenerateSerialAsync(Guid tenantId, string prefix, string entityType);

        // New business code methods
        Task<string> GenerateBusinessCodeAsync(Guid tenantId, string objectType);
        Task<string> GenerateBusinessCodeAsync(string tenantCode, string objectType);
        Task<string> GenerateBusinessCodeForAsync<T>(Guid tenantId) where T : class;
        Task<string> GetTenantCodeAsync(Guid tenantId);
        Task<string> EnsureTenantCodeAsync(Guid tenantId, string? proposedCode = null);
        
        // Risk management
        Task<string> GenerateRiskNumberAsync(Guid tenantId);
        Task<string> GenerateControlNumberAsync(Guid tenantId);
        Task<string> GenerateExceptionNumberAsync(Guid tenantId);
        Task<string> GenerateVendorNumberAsync(Guid tenantId);
        Task<string> GenerateTaskNumberAsync(Guid tenantId);
    }

    public class SerialNumberService : ISerialNumberService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<SerialNumberService> _logger;

        public SerialNumberService(GrcDbContext context, ILogger<SerialNumberService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Legacy Methods (Backward Compatibility)

        public async Task<string> GenerateAssessmentNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "ASM", "Assessment");

        public async Task<string> GeneratePlanNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "PLN", "Plan");

        public async Task<string> GenerateReportNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "RPT", "Report");

        public async Task<string> GenerateEvidenceNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "EVD", "Evidence");

        public async Task<string> GenerateAuditNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "AUD", "Audit");

        public async Task<string> GenerateFindingNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "FND", "AuditFinding");

        public async Task<string> GeneratePolicyNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "POL", "Policy");

        public async Task<string> GenerateWorkflowInstanceNumberAsync(Guid tenantId)
            => await GenerateSerialAsync(tenantId, "WFI", "WorkflowInstance");

        /// <summary>
        /// Generate a unique serial number for any entity type.
        /// Thread-safe and tenant-isolated.
        /// LEGACY FORMAT: {PREFIX}-{YYYYMMDD}-{SEQUENCE:D4}
        /// </summary>
        public async Task<string> GenerateSerialAsync(Guid tenantId, string prefix, string entityType)
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");

            // Get or create sequence counter
            var counter = await _context.Set<SerialNumberCounter>()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId &&
                                          c.EntityType == entityType &&
                                          c.DateKey == today);

            if (counter == null)
            {
                counter = new SerialNumberCounter
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    EntityType = entityType,
                    DateKey = today,
                    LastSequence = 0,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Set<SerialNumberCounter>().Add(counter);
            }

            // Increment sequence
            counter.LastSequence++;
            counter.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var serialNumber = $"{prefix}-{today}-{counter.LastSequence:D4}";
            _logger.LogInformation("Generated serial number {Serial} for {EntityType} in tenant {TenantId}",
                serialNumber, entityType, tenantId);

            return serialNumber;
        }

        #endregion

        #region New Business Code Methods

        public async Task<string> GenerateRiskNumberAsync(Guid tenantId)
            => await GenerateBusinessCodeAsync(tenantId, ObjectTypeCodes.Risk);

        public async Task<string> GenerateControlNumberAsync(Guid tenantId)
            => await GenerateBusinessCodeAsync(tenantId, ObjectTypeCodes.Control);

        public async Task<string> GenerateExceptionNumberAsync(Guid tenantId)
            => await GenerateBusinessCodeAsync(tenantId, ObjectTypeCodes.Exception);

        public async Task<string> GenerateVendorNumberAsync(Guid tenantId)
            => await GenerateBusinessCodeAsync(tenantId, ObjectTypeCodes.Vendor);

        public async Task<string> GenerateTaskNumberAsync(Guid tenantId)
            => await GenerateBusinessCodeAsync(tenantId, ObjectTypeCodes.Task);

        /// <summary>
        /// Generate a business reference code using tenant ID.
        /// NEW FORMAT: {TENANTCODE}-{OBJECTTYPE}-{YYYY}-{SEQUENCE:D6}
        /// </summary>
        public async Task<string> GenerateBusinessCodeAsync(Guid tenantId, string objectType)
        {
            var tenantCode = await GetTenantCodeAsync(tenantId);
            if (string.IsNullOrEmpty(tenantCode))
            {
                tenantCode = await EnsureTenantCodeAsync(tenantId);
            }
            return await GenerateBusinessCodeAsync(tenantCode, objectType);
        }

        /// <summary>
        /// Generate a business reference code using tenant code directly.
        /// NEW FORMAT: {TENANTCODE}-{OBJECTTYPE}-{YYYY}-{SEQUENCE:D6}
        /// </summary>
        public async Task<string> GenerateBusinessCodeAsync(string tenantCode, string objectType)
        {
            if (string.IsNullOrWhiteSpace(tenantCode))
                throw new ArgumentException("Tenant code is required", nameof(tenantCode));
            if (string.IsNullOrWhiteSpace(objectType))
                throw new ArgumentException("Object type is required", nameof(objectType));

            var normalizedTenantCode = NormalizeTenantCode(tenantCode);
            var normalizedObjectType = objectType.ToUpperInvariant();
            var year = DateTime.UtcNow.Year;

            // Get next sequence with transaction safety
            var sequence = await GetNextSequenceAsync(normalizedTenantCode, normalizedObjectType, year);
            
            var code = $"{normalizedTenantCode}-{normalizedObjectType}-{year}-{sequence:D6}";
            _logger.LogDebug("Generated business code: {Code} for {TenantCode}/{ObjectType}", 
                code, normalizedTenantCode, normalizedObjectType);
            
            return code;
        }

        /// <summary>
        /// Generate code for a specific entity type (auto-detects object type code)
        /// </summary>
        public async Task<string> GenerateBusinessCodeForAsync<T>(Guid tenantId) where T : class
        {
            var entityName = typeof(T).Name;
            var objectType = ObjectTypeCodes.GetCode(entityName);
            return await GenerateBusinessCodeAsync(tenantId, objectType);
        }

        /// <summary>
        /// Get the tenant code for a tenant ID
        /// </summary>
        public async Task<string> GetTenantCodeAsync(Guid tenantId)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                return string.Empty;
            }

            // Check TenantCode property first, then Labels
            if (!string.IsNullOrEmpty(tenant.TenantCode))
            {
                return tenant.TenantCode;
            }

            // Fallback to Labels for backward compatibility
            if (!string.IsNullOrEmpty(tenant.Labels) && tenant.Labels.Contains("TenantCode"))
            {
                // Parse TenantCode from Labels JSON string if needed
                // For now, derive from organization name
            }

            // Derive from organization name
            return DeriveCodeFromName(tenant.OrganizationName);
        }

        /// <summary>
        /// Ensure a tenant has a tenant code assigned
        /// </summary>
        public async Task<string> EnsureTenantCodeAsync(Guid tenantId, string? proposedCode = null)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
            {
                throw new InvalidOperationException($"Tenant {tenantId} not found");
            }

            // Check if already has a code
            if (!string.IsNullOrEmpty(tenant.TenantCode))
            {
                return tenant.TenantCode;
            }

            // Generate or normalize code
            var newCode = !string.IsNullOrWhiteSpace(proposedCode)
                ? NormalizeTenantCode(proposedCode)
                : DeriveCodeFromName(tenant.OrganizationName);

            // Ensure uniqueness
            newCode = await EnsureUniqueTenantCodeAsync(newCode, tenantId);

            // Store on entity
            tenant.TenantCode = newCode;
            
            // Also store in Labels for backward compatibility
            // Labels is a string property, so we'll store as JSON
            tenant.Labels = System.Text.Json.JsonSerializer.Serialize(new { TenantCode = newCode });
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Assigned tenant code {TenantCode} to tenant {TenantId}", newCode, tenantId);
            return newCode;
        }

        #endregion

        #region Private Methods

        private async Task<long> GetNextSequenceAsync(string tenantCode, string objectType, int year)
        {
            // Find tenant by code
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantCode == tenantCode);
            
            var tenantId = tenant?.Id ?? Guid.Empty;

            // Use a transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Try to get existing counter
                var counter = await _context.Set<SerialCounter>()
                    .FirstOrDefaultAsync(c => 
                        c.TenantId == tenantId && 
                        c.ObjectType == objectType && 
                        c.Year == year);

                if (counter == null)
                {
                    // Create new counter
                    counter = new SerialCounter
                    {
                        TenantId = tenantId,
                        ObjectType = objectType,
                        Year = year,
                        NextValue = 1
                    };
                    _context.Set<SerialCounter>().Add(counter);
                }

                var sequence = counter.NextValue;
                counter.NextValue++;
                counter.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return sequence;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string NormalizeTenantCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "TEN";

            // Remove special characters, uppercase, limit length
            var normalized = Regex.Replace(code.ToUpperInvariant(), @"[^A-Z0-9]", "");
            
            if (normalized.Length < 2)
                normalized = "TEN" + normalized;
            
            if (normalized.Length > 10)
                normalized = normalized[..10];

            return normalized;
        }

        private static string DeriveCodeFromName(string organizationName)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
                return "TEN";

            // Extract first letters of words, or first 4 chars
            var words = organizationName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length >= 2)
            {
                // Use initials (e.g., "Saudi Telecom Company" -> "STC")
                var initials = string.Concat(words.Take(4).Select(w => w[0])).ToUpperInvariant();
                if (initials.Length >= 2)
                    return NormalizeTenantCode(initials);
            }

            // Use first word or first 4 chars
            var firstWord = Regex.Replace(words[0].ToUpperInvariant(), @"[^A-Z0-9]", "");
            return NormalizeTenantCode(firstWord.Length >= 4 ? firstWord[..4] : firstWord);
        }

        private async Task<string> EnsureUniqueTenantCodeAsync(string proposedCode, Guid excludeTenantId)
        {
            var baseCode = proposedCode;
            var suffix = 1;

            while (true)
            {
                var codeToCheck = suffix == 1 ? baseCode : $"{baseCode}{suffix}";
                
                var exists = await _context.Tenants
                    .AnyAsync(t => t.Id != excludeTenantId && t.TenantCode == codeToCheck);
                
                if (!exists)
                    return codeToCheck;
                
                suffix++;
                if (suffix > 99)
                    throw new InvalidOperationException($"Cannot generate unique tenant code from {baseCode}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Entity to track serial number sequences per tenant/entity/date (LEGACY)
    /// </summary>
    public class SerialNumberCounter
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string DateKey { get; set; } = string.Empty; // YYYYMMDD
        public int LastSequence { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
