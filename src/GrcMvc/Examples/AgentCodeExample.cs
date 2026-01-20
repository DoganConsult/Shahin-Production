using GrcMvc.Services.Base;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GrcMvc.Examples
{
    /// <summary>
    /// EXAMPLE: Agent code standard pattern
    /// 
    /// Requirements:
    /// - Always log AuditReplayEvent
    /// - Always return deterministic JSON with Rationale field
    /// - Include Version field for evolution
    /// - Normalize inputs (stable ordering, case-folding)
    /// - No timestamps/random GUIDs in responses
    /// </summary>
    public interface IAgentService
    {
        Task<AgentResponseDto> RunAgentAsync(AgentRequestDto dto, CancellationToken ct);
    }

    public sealed class AgentService : TenantAwareAppService, IAgentService
    {
        private readonly IAuditReplayService? _auditReplayService;

        public AgentService(
            ITenantContextService tenantContext,
            ILogger<AgentService> logger,
            IAuditReplayService? auditReplayService = null)
            : base(tenantContext, logger)
        {
            _auditReplayService = auditReplayService;
        }

        public async Task<AgentResponseDto> RunAgentAsync(AgentRequestDto dto, CancellationToken ct)
        {
            // 1. Normalize inputs (stable ordering, case-folding)
            var normalized = NormalizeInput(dto);

            // 2. Compute deterministic replay key
            var replayKey = ComputeReplayKey(normalized, TenantId, "v1");

            // 3. Run deterministic logic (no randomness/time)
            var result = RunDeterministicLogic(normalized);

            // 4. Build response with Rationale and Version
            var response = new AgentResponseDto(
                Result: result,
                Rationale: BuildRationale(normalized, result),
                Version: "v1"
            );

            // 5. Log AuditReplayEvent
            if (_auditReplayService != null)
            {
                await _auditReplayService.LogReplayEventAsync(new AuditReplayEvent
                {
                    ReplayKey = replayKey,
                    TenantId = TenantId,
                    Input = normalized,
                    Output = response,
                    CorrelationId = dto.CorrelationId ?? Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }, ct);
            }

            return response;
        }

        private AgentRequestDto NormalizeInput(AgentRequestDto dto)
        {
            // Normalize: trim, case-fold, stable ordering
            return new AgentRequestDto(
                Query: dto.Query?.Trim().ToLowerInvariant() ?? string.Empty,
                Filters: dto.Filters?.OrderBy(f => f.Key).ToDictionary(f => f.Key.ToLowerInvariant(), f => f.Value?.ToLowerInvariant() ?? string.Empty) ?? new Dictionary<string, string>(),
                CorrelationId: dto.CorrelationId
            );
        }

        private string ComputeReplayKey(AgentRequestDto normalized, Guid tenantId, string version)
        {
            // Deterministic hash of normalized input + tenant + version
            var json = JsonSerializer.Serialize(new { normalized, tenantId, version });
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(hash);
        }

        private object RunDeterministicLogic(AgentRequestDto normalized)
        {
            // Deterministic logic - no timestamps, no random GUIDs
            // Example: simple search result
            return new
            {
                Items = new[]
                {
                    new { Id = "item-1", Name = "Result 1" },
                    new { Id = "item-2", Name = "Result 2" }
                },
                Count = 2
            };
        }

        private string BuildRationale(AgentRequestDto normalized, object result)
        {
            return $"Processed query '{normalized.Query}' with {normalized.Filters.Count} filters. Found {((dynamic)result).Count} results.";
        }
    }

    // Request DTO
    public sealed record AgentRequestDto(
        string? Query,
        Dictionary<string, string>? Filters,
        string? CorrelationId
    );

    // Response DTO (deterministic JSON)
    public sealed record AgentResponseDto(
        object Result,
        string Rationale,
        string Version = "v1"
    );

    // Audit Replay Event
    public class AuditReplayEvent
    {
        public string ReplayKey { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public object Input { get; set; } = new();
        public object Output { get; set; } = new();
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // Audit Replay Service Interface
    public interface IAuditReplayService
    {
        Task LogReplayEventAsync(AuditReplayEvent evt, CancellationToken ct);
    }
}
