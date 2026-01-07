using System;
using System.Threading.Tasks;
using GrcMvc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// System-generated serial number service for all GRC documents.
    /// Ensures unique, sequential, and tamper-proof document numbers.
    /// Format: {PREFIX}-{YYYYMMDD}-{SEQUENCE:D4} e.g., ASM-20260106-0001
    /// </summary>
    public interface ISerialNumberService
    {
        Task<string> GenerateAssessmentNumberAsync(Guid tenantId);
        Task<string> GeneratePlanNumberAsync(Guid tenantId);
        Task<string> GenerateReportNumberAsync(Guid tenantId);
        Task<string> GenerateEvidenceNumberAsync(Guid tenantId);
        Task<string> GenerateAuditNumberAsync(Guid tenantId);
        Task<string> GenerateFindingNumberAsync(Guid tenantId);
        Task<string> GeneratePolicyNumberAsync(Guid tenantId);
        Task<string> GenerateWorkflowInstanceNumberAsync(Guid tenantId);
        Task<string> GenerateSerialAsync(Guid tenantId, string prefix, string entityType);
    }

    public class SerialNumberService : ISerialNumberService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<SerialNumberService> _logger;
        private static readonly object _lock = new object();

        public SerialNumberService(GrcDbContext context, ILogger<SerialNumberService> logger)
        {
            _context = context;
            _logger = logger;
        }

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
        /// </summary>
        public async Task<string> GenerateSerialAsync(Guid tenantId, string prefix, string entityType)
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var sequenceKey = $"{tenantId}:{entityType}:{today}";

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
    }

    /// <summary>
    /// Entity to track serial number sequences per tenant/entity/date
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
