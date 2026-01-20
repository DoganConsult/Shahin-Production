using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Service for logging agent code execution to AuditReplayEvent.
    /// Enables deterministic replay and auditability of agent operations.
    /// </summary>
    public class AuditReplayService : IAuditReplayService
    {
        private readonly GrcDbContext _context;
        private readonly ILogger<AuditReplayService> _logger;

        public AuditReplayService(
            GrcDbContext context,
            ILogger<AuditReplayService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LogAgentExecutionAsync(
            string replayKey,
            Guid tenantId,
            string? userId,
            object input,
            AgentResponseDto output,
            string? correlationId = null,
            CancellationToken ct = default)
        {
            try
            {
                // Serialize input and output deterministically
                var inputJson = JsonSerializer.Serialize(input, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var outputJson = JsonSerializer.Serialize(output, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Create audit replay event using AuditEvent entity
                var auditEvent = new AuditEvent
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    EventId = $"replay-{replayKey.Substring(0, Math.Min(8, replayKey.Length))}",
                    EventType = "AgentExecution",
                    CorrelationId = correlationId ?? string.Empty,
                    AffectedEntityType = "AgentResponse",
                    AffectedEntityId = replayKey,
                    Actor = userId ?? "SYSTEM",
                    Action = "Execute",
                    PayloadJson = JsonSerializer.Serialize(new
                    {
                        Input = inputJson,
                        Output = outputJson,
                        ReplayKey = replayKey,
                        Version = output.Version
                    }),
                    Status = "Success",
                    EventTimestamp = DateTime.UtcNow,
                    Severity = "Info",
                    UserId = userId
                };

                _context.AuditEvents.Add(auditEvent);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "AuditReplayEvent logged: ReplayKey={ReplayKey}, TenantId={TenantId}, Version={Version}",
                    replayKey, tenantId, output.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to log AuditReplayEvent: ReplayKey={ReplayKey}, TenantId={TenantId}",
                    replayKey, tenantId);
                // Don't throw - audit logging failure should not break agent execution
            }
        }
    }

    /// <summary>
    /// Helper class for computing deterministic replay keys.
    /// </summary>
    public static class ReplayKeyHelper
    {
        /// <summary>
        /// Computes a deterministic replay key from normalized input, tenant ID, and version.
        /// </summary>
        public static string ComputeReplayKey(object normalizedInput, Guid tenantId, string version)
        {
            var inputJson = JsonSerializer.Serialize(normalizedInput, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var combined = $"{inputJson}|{tenantId}|{version}";
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
