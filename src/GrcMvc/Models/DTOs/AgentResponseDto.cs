using System;
using System.Text.Json.Serialization;

namespace GrcMvc.Models.DTOs
{
    /// <summary>
    /// Standard response DTO for agent code endpoints.
    /// Ensures deterministic JSON output with Rationale and Version fields.
    /// </summary>
    public class AgentResponseDto
    {
        [JsonPropertyName("result")]
        public object Result { get; set; } = null!;

        [JsonPropertyName("rationale")]
        public string Rationale { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = "v1";

        [JsonPropertyName("replayKey")]
        public string? ReplayKey { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public AgentResponseDto()
        {
        }

        public AgentResponseDto(object result, string rationale, string version = "v1", string? replayKey = null)
        {
            Result = result;
            Rationale = rationale;
            Version = version;
            ReplayKey = replayKey;
        }
    }
}
