using System.Text.Json;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Agent Communication Service - Manages inter-agent communication contracts and messages.
/// Implements the fullplan specification for agent orchestration and coordination.
/// </summary>
public class AgentCommunicationService : IAgentCommunicationService
{
    private readonly GrcDbContext _dbContext;
    private readonly ILogger<AgentCommunicationService> _logger;

    public AgentCommunicationService(
        GrcDbContext dbContext,
        ILogger<AgentCommunicationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AgentCommunicationContract> RegisterContractAsync(
        AgentCommunicationContract contract,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate contract
            if (string.IsNullOrEmpty(contract.SourceAgentCode))
                throw new ArgumentException("Source agent code is required");

            if (string.IsNullOrEmpty(contract.TargetAgentCode))
                throw new ArgumentException("Target agent code is required");

            if (string.IsNullOrEmpty(contract.MessageType))
                throw new ArgumentException("Message type is required");

            // Check for existing contract
            var existingContract = await _dbContext.Set<AgentCommunicationContract>()
                .FirstOrDefaultAsync(c =>
                    c.SourceAgentCode == contract.SourceAgentCode &&
                    c.TargetAgentCode == contract.TargetAgentCode &&
                    c.MessageType == contract.MessageType &&
                    c.IsActive,
                    cancellationToken);

            if (existingContract != null)
            {
                // Update existing contract
                existingContract.RequestSchemaJson = contract.RequestSchemaJson;
                existingContract.ResponseSchemaJson = contract.ResponseSchemaJson;
                existingContract.TimeoutSeconds = contract.TimeoutSeconds;
                existingContract.RetryPolicy = contract.RetryPolicy;
                existingContract.MaxRetries = contract.MaxRetries;
                existingContract.RequiresAcknowledgment = contract.RequiresAcknowledgment;
                existingContract.PriorityLevel = contract.PriorityLevel;
                existingContract.ModifiedDate = DateTime.UtcNow;
                existingContract.Version++;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Updated communication contract {ContractCode} v{Version}",
                    existingContract.ContractCode, existingContract.Version);

                return existingContract;
            }

            // Create new contract
            if (contract.Id == Guid.Empty)
                contract.Id = Guid.NewGuid();

            contract.CreatedDate = DateTime.UtcNow;
            contract.Version = 1;
            contract.IsActive = true;

            _dbContext.Set<AgentCommunicationContract>().Add(contract);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Registered new communication contract {ContractCode} between {Source} and {Target}",
                contract.ContractCode, contract.SourceAgentCode, contract.TargetAgentCode);

            return contract;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering communication contract");
            throw;
        }
    }

    public async Task<AgentCommunicationContract?> GetContractAsync(
        string sourceAgentCode,
        string targetAgentCode,
        string messageType,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<AgentCommunicationContract>()
            .FirstOrDefaultAsync(c =>
                c.SourceAgentCode == sourceAgentCode &&
                c.TargetAgentCode == targetAgentCode &&
                c.MessageType == messageType &&
                c.IsActive,
                cancellationToken);
    }

    public async Task<List<AgentCommunicationContract>> GetContractsForAgentAsync(
        string agentCode,
        bool asSource = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AgentCommunicationContract>()
            .Where(c => c.IsActive);

        if (asSource)
        {
            query = query.Where(c => c.SourceAgentCode == agentCode);
        }
        else
        {
            query = query.Where(c => c.TargetAgentCode == agentCode);
        }

        return await query
            .OrderBy(c => c.TargetAgentCode)
            .ThenBy(c => c.MessageType)
            .ToListAsync(cancellationToken);
    }

    public async Task<AgentMessage> SendMessageAsync(
        string sourceAgentCode,
        string targetAgentCode,
        string messageType,
        Dictionary<string, object> payload,
        Guid? tenantId = null,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the contract
            var contract = await GetContractAsync(sourceAgentCode, targetAgentCode, messageType, cancellationToken);

            // Create the message
            var message = new AgentMessage
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SourceAgentCode = sourceAgentCode,
                TargetAgentCode = targetAgentCode,
                MessageType = messageType,
                ContractId = contract?.Id,
                CorrelationId = correlationId ?? Guid.NewGuid(),
                PayloadJson = JsonSerializer.Serialize(payload),
                Status = "Pending",
                Priority = contract?.PriorityLevel ?? 5,
                RequiresResponse = contract?.RequiresAcknowledgment ?? false,
                TimeoutSeconds = contract?.TimeoutSeconds ?? 30,
                CreatedDate = DateTime.UtcNow
            };

            // Validate payload against contract schema if available
            if (contract != null && !string.IsNullOrEmpty(contract.RequestSchemaJson))
            {
                var validationResult = ValidateAgainstSchema(payload, contract.RequestSchemaJson);
                if (!validationResult.IsValid)
                {
                    message.Status = "ValidationFailed";
                    message.ErrorMessage = string.Join("; ", validationResult.Errors);
                    _logger.LogWarning(
                        "Message validation failed for {MessageType}: {Errors}",
                        messageType, message.ErrorMessage);
                }
            }

            _dbContext.Set<AgentMessage>().Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // If validation passed, queue for delivery
            if (message.Status == "Pending")
            {
                message.Status = "Queued";
                message.QueuedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Message {MessageId} queued from {Source} to {Target} ({Type})",
                    message.Id, sourceAgentCode, targetAgentCode, messageType);
            }

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending message from {Source} to {Target}",
                sourceAgentCode, targetAgentCode);
            throw;
        }
    }

    public async Task<AgentMessage?> GetMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<AgentMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
    }

    public async Task<List<AgentMessage>> GetPendingMessagesAsync(
        string targetAgentCode,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<AgentMessage>()
            .Where(m => m.TargetAgentCode == targetAgentCode &&
                       (m.Status == "Queued" || m.Status == "Pending"))
            .OrderBy(m => m.Priority)
            .ThenBy(m => m.CreatedDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AcknowledgeMessageAsync(
        Guid messageId,
        string status,
        Dictionary<string, object>? responsePayload = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Set<AgentMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message == null)
            return false;

        message.Status = status;
        message.ProcessedAt = DateTime.UtcNow;
        message.ResponsePayloadJson = responsePayload != null
            ? JsonSerializer.Serialize(responsePayload)
            : null;
        message.ErrorMessage = errorMessage;

        // Calculate processing time
        if (message.QueuedAt.HasValue)
        {
            message.ProcessingTimeMs = (int)(DateTime.UtcNow - message.QueuedAt.Value).TotalMilliseconds;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Message {MessageId} acknowledged with status {Status}",
            messageId, status);

        return true;
    }

    public async Task<CommunicationMetrics> GetCommunicationMetricsAsync(
        string? agentCode = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AgentMessage>().AsQueryable();

        if (!string.IsNullOrEmpty(agentCode))
        {
            query = query.Where(m =>
                m.SourceAgentCode == agentCode ||
                m.TargetAgentCode == agentCode);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(m => m.CreatedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(m => m.CreatedDate <= toDate.Value);
        }

        var messages = await query.ToListAsync(cancellationToken);

        var metrics = new CommunicationMetrics
        {
            TotalMessages = messages.Count,
            MessagesSent = messages.Count(m => m.SourceAgentCode == agentCode),
            MessagesReceived = messages.Count(m => m.TargetAgentCode == agentCode),
            SuccessfulDeliveries = messages.Count(m => m.Status == "Delivered" || m.Status == "Processed"),
            FailedDeliveries = messages.Count(m => m.Status == "Failed" || m.Status == "ValidationFailed"),
            PendingMessages = messages.Count(m => m.Status == "Pending" || m.Status == "Queued"),
            AverageProcessingTimeMs = messages
                .Where(m => m.ProcessingTimeMs.HasValue)
                .Select(m => m.ProcessingTimeMs!.Value)
                .DefaultIfEmpty(0)
                .Average(),
            MessagesByType = messages
                .GroupBy(m => m.MessageType)
                .ToDictionary(g => g.Key, g => g.Count()),
            MessagesByStatus = messages
                .GroupBy(m => m.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return metrics;
    }

    public async Task<bool> RetryMessageAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Set<AgentMessage>()
            .Include(m => m.Contract)
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message == null)
            return false;

        // Check if retries are allowed
        var maxRetries = message.Contract?.MaxRetries ?? 3;
        if (message.RetryCount >= maxRetries)
        {
            _logger.LogWarning(
                "Message {MessageId} has exceeded max retries ({MaxRetries})",
                messageId, maxRetries);
            return false;
        }

        message.RetryCount++;
        message.Status = "Queued";
        message.LastRetryAt = DateTime.UtcNow;
        message.ErrorMessage = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Message {MessageId} queued for retry ({RetryCount}/{MaxRetries})",
            messageId, message.RetryCount, maxRetries);

        return true;
    }

    public async Task<List<AgentMessage>> GetConversationAsync(
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<AgentMessage>()
            .Where(m => m.CorrelationId == correlationId)
            .OrderBy(m => m.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeactivateContractAsync(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _dbContext.Set<AgentCommunicationContract>()
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null)
            return false;

        contract.IsActive = false;
        contract.ModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Communication contract {ContractCode} deactivated",
            contract.ContractCode);

        return true;
    }

    #region Private Methods

    private SchemaValidationResult ValidateAgainstSchema(
        Dictionary<string, object> payload,
        string schemaJson)
    {
        var result = new SchemaValidationResult { IsValid = true };

        try
        {
            var schema = JsonSerializer.Deserialize<Dictionary<string, object>>(schemaJson);
            if (schema == null)
                return result;

            // Get required fields from schema
            if (schema.TryGetValue("required", out var requiredObj) &&
                requiredObj is JsonElement requiredElement &&
                requiredElement.ValueKind == JsonValueKind.Array)
            {
                var requiredFields = requiredElement.EnumerateArray()
                    .Select(e => e.GetString())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                foreach (var field in requiredFields)
                {
                    if (!payload.ContainsKey(field!))
                    {
                        result.IsValid = false;
                        result.Errors.Add($"Required field '{field}' is missing");
                    }
                }
            }

            // Get properties from schema
            if (schema.TryGetValue("properties", out var propertiesObj) &&
                propertiesObj is JsonElement propertiesElement &&
                propertiesElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in propertiesElement.EnumerateObject())
                {
                    if (payload.TryGetValue(property.Name, out var value))
                    {
                        // Type validation
                        if (property.Value.TryGetProperty("type", out var typeElement))
                        {
                            var expectedType = typeElement.GetString();
                            var actualType = GetJsonType(value);

                            if (expectedType != actualType && !IsTypeCompatible(expectedType!, actualType))
                            {
                                result.IsValid = false;
                                result.Errors.Add(
                                    $"Field '{property.Name}' expected type '{expectedType}' but got '{actualType}'");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating schema");
            // Don't fail on schema validation errors
        }

        return result;
    }

    private string GetJsonType(object? value)
    {
        return value switch
        {
            null => "null",
            string => "string",
            bool => "boolean",
            int or long or float or double or decimal => "number",
            JsonElement je => je.ValueKind switch
            {
                JsonValueKind.String => "string",
                JsonValueKind.Number => "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Array => "array",
                JsonValueKind.Object => "object",
                JsonValueKind.Null => "null",
                _ => "unknown"
            },
            System.Collections.IEnumerable => "array",
            _ => "object"
        };
    }

    private bool IsTypeCompatible(string expected, string actual)
    {
        // Allow integer/number compatibility
        if (expected == "integer" && actual == "number")
            return true;
        if (expected == "number" && actual == "integer")
            return true;

        return false;
    }

    #endregion

    private class SchemaValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}

/// <summary>
/// Agent message entity for inter-agent communication
/// </summary>
public class AgentMessage : BaseEntity
{
    public string SourceAgentCode { get; set; } = string.Empty;
    public string TargetAgentCode { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public Guid? ContractId { get; set; }
    public Guid CorrelationId { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public string? ResponsePayloadJson { get; set; }
    public string Status { get; set; } = "Pending";
    public int Priority { get; set; } = 5;
    public bool RequiresResponse { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; }
    public DateTime? QueuedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public int? ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation property
    public virtual AgentCommunicationContract? Contract { get; set; }
}

/// <summary>
/// Communication metrics DTO
/// </summary>
public class CommunicationMetrics
{
    public int TotalMessages { get; set; }
    public int MessagesSent { get; set; }
    public int MessagesReceived { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public int PendingMessages { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public Dictionary<string, int> MessagesByType { get; set; } = new();
    public Dictionary<string, int> MessagesByStatus { get; set; } = new();
}
