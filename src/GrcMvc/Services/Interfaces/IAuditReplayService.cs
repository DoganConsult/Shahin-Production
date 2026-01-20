using System;
using System.Threading;
using System.Threading.Tasks;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for logging agent code execution to AuditReplayEvent.
    /// Enables deterministic replay and auditability of agent operations.
    /// </summary>
    public interface IAuditReplayService
    {
        /// <summary>
        /// Logs an agent execution event for replay and audit purposes.
        /// </summary>
        /// <param name="replayKey">Deterministic key for replay (hash of normalized input + tenant + version)</param>
        /// <param name="tenantId">Current tenant ID</param>
        /// <param name="userId">Current user ID (if available)</param>
        /// <param name="input">Normalized input data</param>
        /// <param name="output">Agent response</param>
        /// <param name="correlationId">Correlation/Trace ID from request context</param>
        /// <param name="ct">Cancellation token</param>
        Task LogAgentExecutionAsync(
            string replayKey,
            Guid tenantId,
            string? userId,
            object input,
            AgentResponseDto output,
            string? correlationId = null,
            CancellationToken ct = default);
    }
}
