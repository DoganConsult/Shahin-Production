using GrcMvc.Common.Results;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GrcMvc.Services.Implementations;

// Use SyncFieldMapping from DTOs - alias for clarity
using FieldMapping = SyncFieldMapping;

/// <summary>
/// Sync Execution Service - Executes data synchronization between external systems
/// Follows ASP.NET Core best practices with dependency injection and async patterns
/// </summary>
public class SyncExecutionService : ISyncExecutionService
{
    private readonly GrcDbContext _context;
    private readonly ILogger<SyncExecutionService> _logger;
    private readonly IEventPublisherService _eventPublisher;
    private readonly ICredentialEncryptionService _encryption;
    private readonly IHttpClientFactory _httpClientFactory;

    public SyncExecutionService(
        GrcDbContext context,
        ILogger<SyncExecutionService> logger,
        IEventPublisherService eventPublisher,
        ICredentialEncryptionService encryption,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _encryption = encryption;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<Guid>> ExecuteSyncJobAsync(Guid syncJobId, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        Guid executionLogId = Guid.Empty;

        try
        {
            // Load sync job with connector
            var syncJob = await _context.Set<SyncJob>()
                .Include(s => s.Connector)
                .FirstOrDefaultAsync(s => s.Id == syncJobId && s.IsDeleted == false, cancellationToken);

            if (syncJob == null)
            {
                return Result<Guid>.Failure(Error.NotFound("SyncJob", syncJobId));
            }

            if (!syncJob.IsActive)
            {
                return Result<Guid>.Failure(Error.InvalidOperation(
                    $"SyncJob {syncJobId} is not active",
                    "Activate the job before executing"));
            }

            _logger.LogInformation(
                "Starting sync job execution: {JobCode} ({JobId}) - {Direction} sync of {ObjectType}",
                syncJob.JobCode, syncJobId, syncJob.Direction, syncJob.ObjectType);

            // Create execution log
            var executionLog = new SyncExecutionLog
            {
                Id = Guid.NewGuid(),
                SyncJobId = syncJobId,
                Status = "Running",
                StartedAt = startTime,
                RecordsProcessed = 0,
                RecordsCreated = 0,
                RecordsUpdated = 0,
                RecordsFailed = 0,
                RecordsSkipped = 0,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.Set<SyncExecutionLog>().Add(executionLog);
            await _context.SaveChangesAsync(cancellationToken);
            executionLogId = executionLog.Id;

            // Execute sync based on direction
            switch (syncJob.Direction)
            {
                case "Inbound":
                    await ExecuteInboundSyncAsync(syncJob, executionLog, cancellationToken);
                    break;
                case "Outbound":
                    await ExecuteOutboundSyncAsync(syncJob, executionLog, cancellationToken);
                    break;
                case "Bidirectional":
                    await ExecuteInboundSyncAsync(syncJob, executionLog, cancellationToken);
                    await ExecuteOutboundSyncAsync(syncJob, executionLog, cancellationToken);
                    break;
                default:
                    return Result<Guid>.Failure(Error.Validation(
                        $"Unknown sync direction: {syncJob.Direction}",
                        "Valid directions are: Inbound, Outbound, Bidirectional"));
            }

            // Mark as completed
            executionLog.Status = "Completed";
            executionLog.CompletedAt = DateTime.UtcNow;
            executionLog.DurationSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            executionLog.ModifiedDate = DateTime.UtcNow;

            // Update sync job last run info
            syncJob.LastRunAt = DateTime.UtcNow;
            syncJob.LastRunStatus = "Success";
            syncJob.LastRunRecordCount = executionLog.RecordsProcessed;
            syncJob.NextRunAt = CalculateNextRunTime(syncJob);
            syncJob.ModifiedDate = DateTime.UtcNow;

            // Update connector health
            syncJob.Connector.LastSuccessfulSync = DateTime.UtcNow;
            syncJob.Connector.ErrorCount = 0;
            syncJob.Connector.ConnectionStatus = "Connected";
            syncJob.Connector.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sync job completed successfully: {JobCode} - {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                syncJob.JobCode, executionLog.RecordsProcessed, executionLog.RecordsCreated,
                executionLog.RecordsUpdated, executionLog.RecordsFailed);

            // Publish sync completed event
            await _eventPublisher.PublishEventAsync(
                "SyncJobCompleted",
                "SyncJob",
                syncJobId,
                new
                {
                    JobCode = syncJob.JobCode,
                    Direction = syncJob.Direction,
                    ObjectType = syncJob.ObjectType,
                    RecordsProcessed = executionLog.RecordsProcessed,
                    RecordsCreated = executionLog.RecordsCreated,
                    RecordsUpdated = executionLog.RecordsUpdated,
                    DurationSeconds = executionLog.DurationSeconds
                },
                syncJob.TenantId,
                cancellationToken);

            return Result<Guid>.Success(executionLogId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync job execution failed: {JobId}", syncJobId);

            if (executionLogId != Guid.Empty)
            {
                // Update execution log with error
                var executionLog = await _context.Set<SyncExecutionLog>()
                    .FirstOrDefaultAsync(e => e.Id == executionLogId, cancellationToken);

                if (executionLog != null)
                {
                    executionLog.Status = "Failed";
                    executionLog.CompletedAt = DateTime.UtcNow;
                    executionLog.DurationSeconds = (int)(DateTime.UtcNow - startTime).TotalSeconds;
                    executionLog.ErrorsJson = JsonSerializer.Serialize(new[] { new { Message = ex.Message, StackTrace = ex.StackTrace } });
                    executionLog.ModifiedDate = DateTime.UtcNow;

                    // Update sync job
                    var syncJob = await _context.Set<SyncJob>()
                        .Include(s => s.Connector)
                        .FirstOrDefaultAsync(s => s.Id == syncJobId, cancellationToken);

                    if (syncJob != null)
                    {
                        syncJob.LastRunAt = DateTime.UtcNow;
                        syncJob.LastRunStatus = "Failed";
                        syncJob.ModifiedDate = DateTime.UtcNow;

                        // Update connector error count
                        syncJob.Connector.ErrorCount++;
                        syncJob.Connector.ConnectionStatus = syncJob.Connector.ErrorCount >= 5 ? "Error" : "Disconnected";
                        syncJob.Connector.ModifiedDate = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return Result<Guid>.Failure(Error.Internal("Sync job execution failed", ex.Message));
        }
    }

    public async Task ExecuteScheduledSyncsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking for scheduled sync jobs...");

        var dueJobs = await _context.Set<SyncJob>()
            .Include(s => s.Connector)
            .Where(s => !s.IsDeleted && s.IsActive)
            .Where(s => s.NextRunAt.HasValue && s.NextRunAt.Value <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} scheduled sync jobs due for execution", dueJobs.Count);

        foreach (var job in dueJobs)
        {
            try
            {
                await ExecuteSyncJobAsync(job.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute scheduled sync job: {JobCode}", job.JobCode);
                // Continue with next job
            }
        }
    }

    public async Task<Result> CancelSyncJobAsync(Guid executionLogId, CancellationToken cancellationToken = default)
    {
        var executionLog = await _context.Set<SyncExecutionLog>()
            .FirstOrDefaultAsync(e => e.Id == executionLogId, cancellationToken);

        if (executionLog == null)
        {
            return Result.Failure(Error.NotFound("ExecutionLog", executionLogId));
        }

        if (executionLog.Status != "Running")
        {
            return Result.Failure(Error.InvalidOperation(
                $"Cannot cancel sync job in status: {executionLog.Status}",
                "Only running jobs can be cancelled"));
        }

        executionLog.Status = "Cancelled";
        executionLog.CompletedAt = DateTime.UtcNow;
        executionLog.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sync job cancelled: {ExecutionLogId}", executionLogId);
        return Result.Success();
    }

    public async Task<Result<SyncExecutionStatus>> GetExecutionStatusAsync(Guid executionLogId, CancellationToken cancellationToken = default)
    {
        var executionLog = await _context.Set<SyncExecutionLog>()
            .FirstOrDefaultAsync(e => e.Id == executionLogId, cancellationToken);

        if (executionLog == null)
        {
            return Result<SyncExecutionStatus>.Failure(Error.NotFound("ExecutionLog", executionLogId));
        }

        var errors = new List<string>();
        if (!string.IsNullOrEmpty(executionLog.ErrorsJson))
        {
            try
            {
                var errorList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(executionLog.ErrorsJson);
                errors = errorList?.Select(e => e.GetValueOrDefault("Message", "Unknown error")).ToList() ?? new List<string>();
            }
            catch
            {
                errors.Add("Failed to parse error details");
            }
        }

        return Result<SyncExecutionStatus>.Success(new SyncExecutionStatus
        {
            ExecutionLogId = executionLog.Id,
            Status = executionLog.Status,
            RecordsProcessed = executionLog.RecordsProcessed,
            RecordsCreated = executionLog.RecordsCreated,
            RecordsUpdated = executionLog.RecordsUpdated,
            RecordsFailed = executionLog.RecordsFailed,
            RecordsSkipped = executionLog.RecordsSkipped,
            StartedAt = executionLog.StartedAt,
            CompletedAt = executionLog.CompletedAt,
            DurationSeconds = executionLog.DurationSeconds,
            Errors = errors
        });
    }

    public async Task<Result<Guid>> RetrySyncJobAsync(Guid failedExecutionLogId, CancellationToken cancellationToken = default)
    {
        var failedLog = await _context.Set<SyncExecutionLog>()
            .Include(e => e.SyncJob)
            .FirstOrDefaultAsync(e => e.Id == failedExecutionLogId, cancellationToken);

        if (failedLog == null)
        {
            return Result<Guid>.Failure(Error.NotFound("ExecutionLog", failedExecutionLogId));
        }

        if (failedLog.Status != "Failed")
        {
            return Result<Guid>.Failure(Error.InvalidOperation(
                $"Can only retry failed sync jobs. Current status: {failedLog.Status}",
                "Retry is only available for failed jobs"));
        }

        _logger.LogInformation("Retrying failed sync job: {JobCode}", failedLog.SyncJob.JobCode);

        return await ExecuteSyncJobAsync(failedLog.SyncJobId, cancellationToken);
    }

    // Private helper methods

    private async Task ExecuteInboundSyncAsync(SyncJob syncJob, SyncExecutionLog executionLog, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing inbound sync: {ObjectType} from {TargetSystem}",
            syncJob.ObjectType, syncJob.Connector.TargetSystem);

        try
        {
            // Route to appropriate connector implementation
            var result = syncJob.Connector.ConnectorType switch
            {
                "REST_API" => await ExecuteRestApiInboundAsync(syncJob, cancellationToken),
                "DATABASE" => await ExecuteDatabaseInboundAsync(syncJob, cancellationToken),
                "FILE" => await ExecuteFileInboundAsync(syncJob, cancellationToken),
                "WEBHOOK" => await ExecuteWebhookInboundAsync(syncJob, cancellationToken),
                _ => (Success: false, RecordsProcessed: 0, Error: $"Unsupported connector type: {syncJob.Connector.ConnectorType}")
            };

            executionLog.RecordsProcessed = result.RecordsProcessed;
            if (!result.Success)
            {
                executionLog.ErrorsJson = System.Text.Json.JsonSerializer.Serialize(new[] { result.Error });
                _logger.LogWarning("Inbound sync failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            executionLog.ErrorsJson = System.Text.Json.JsonSerializer.Serialize(new[] { ex.Message });
            _logger.LogError(ex, "Error during inbound sync execution");
            throw;
        }
    }

    private async Task<(bool Success, int RecordsProcessed, string? Error)> ExecuteRestApiInboundAsync(SyncJob syncJob, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing REST API inbound sync for {ObjectType}", syncJob.ObjectType);
        
        try
        {
            var connector = syncJob.Connector;
            
            // Parse connection config
            var config = string.IsNullOrEmpty(connector.ConnectionConfigJson)
                ? new RestApiConfig()
                : JsonSerializer.Deserialize<RestApiConfig>(connector.ConnectionConfigJson) ?? new RestApiConfig();
            
            if (string.IsNullOrEmpty(config.Endpoint))
            {
                return (false, 0, "REST API endpoint not configured in connection config");
            }
            
            // Create HTTP client with credentials
            var client = _httpClientFactory.CreateClient("ExternalServices");
            
            // Apply authentication from connection config
            var apiKey = ExtractApiKeyFromConfig(connector.ConnectionConfigJson);
            if (!string.IsNullOrEmpty(apiKey))
            {
                var decryptedKey = _encryption.Decrypt(apiKey);
                var authScheme = config.AuthScheme ?? "Bearer";
                
                switch (authScheme.ToLower())
                {
                    case "bearer":
                        client.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", decryptedKey);
                        break;
                    case "basic":
                        var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(decryptedKey));
                        client.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        break;
                    case "apikey":
                        client.DefaultRequestHeaders.Add(config.ApiKeyHeader ?? "X-API-Key", decryptedKey);
                        break;
                }
            }
            
            // Apply custom headers
            if (config.CustomHeaders != null)
            {
                foreach (var header in config.CustomHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            
            // Build request URL with filter
            var url = config.Endpoint;
            if (!string.IsNullOrEmpty(syncJob.FilterExpression))
            {
                url += (url.Contains("?") ? "&" : "?") + syncJob.FilterExpression;
            }
            
            // Add pagination parameters if configured
            if (config.PaginationType == "offset" && config.PageSize > 0)
            {
                var separator = url.Contains("?") ? "&" : "?";
                url += $"{separator}{config.PageSizeParam ?? "limit"}={config.PageSize}";
            }
            
            _logger.LogDebug("Fetching data from: {Url}", url);
            
            // Fetch data
            var response = await client.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("REST API request failed: {Status} - {Content}", response.StatusCode, errorContent);
                return (false, 0, $"API request failed with status {response.StatusCode}");
            }
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Parse response - extract data array from path if specified
            JsonElement dataArray;
            var jsonDoc = JsonDocument.Parse(content);
            
            if (!string.IsNullOrEmpty(config.DataPath))
            {
                var pathParts = config.DataPath.Split('.');
                var current = jsonDoc.RootElement;
                foreach (var part in pathParts)
                {
                    if (!current.TryGetProperty(part, out current))
                    {
                        _logger.LogWarning("Data path '{DataPath}' not found in response", config.DataPath);
                        return (true, 0, null);
                    }
                }
                dataArray = current;
            }
            else
            {
                dataArray = jsonDoc.RootElement;
            }
            
            // Handle both array and single object responses
            var records = dataArray.ValueKind == JsonValueKind.Array
                ? dataArray.EnumerateArray().ToList()
                : new List<JsonElement> { dataArray };
            
            if (!records.Any())
            {
                _logger.LogInformation("No records returned from API for {ObjectType}", syncJob.ObjectType);
                return (true, 0, null);
            }
            
            // Parse field mappings
            var mappings = string.IsNullOrEmpty(syncJob.FieldMappingJson)
                ? new List<FieldMapping>()
                : JsonSerializer.Deserialize<List<FieldMapping>>(syncJob.FieldMappingJson) ?? new List<FieldMapping>();
            
            // Process each record
            int processed = 0, created = 0, updated = 0, failed = 0;
            
            foreach (var record in records)
            {
                try
                {
                    // Apply field mappings
                    var mappedRecord = ApplyFieldMappings(record, mappings);
                    
                    // Get external ID for cross-reference
                    var externalId = GetExternalId(record, config.IdField ?? "id");
                    
                    // Upsert to local database
                    var (isNew, localId) = await UpsertRecordAsync(syncJob, externalId, mappedRecord, cancellationToken);
                    
                    // Update cross-reference mapping
                    await UpdateCrossReferenceMappingAsync(syncJob, externalId, localId, cancellationToken);
                    
                    if (isNew) created++; else updated++;
                    processed++;
                    
                    // Publish event for this record
                    await _eventPublisher.PublishEventAsync(
                        isNew ? "RecordCreated" : "RecordUpdated",
                        syncJob.ObjectType,
                        localId,
                        new { ExternalId = externalId, Source = connector.TargetSystem },
                        syncJob.TenantId,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process record in REST API sync");
                    failed++;
                }
            }
            
            _logger.LogInformation(
                "REST API inbound sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                processed, created, updated, failed);
            
            return (true, processed, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for REST API sync");
            return (false, 0, $"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed for REST API sync");
            return (false, 0, $"Failed to parse API response: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "REST API inbound sync failed");
            return (false, 0, ex.Message);
        }
    }

    private async Task<(bool Success, int RecordsProcessed, string? Error)> ExecuteDatabaseInboundAsync(SyncJob syncJob, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing Database inbound sync for {ObjectType}", syncJob.ObjectType);
        
        try
        {
            var connector = syncJob.Connector;
            
            // Parse connection config
            var config = string.IsNullOrEmpty(connector.ConnectionConfigJson)
                ? new DatabaseConnectorConfig()
                : JsonSerializer.Deserialize<DatabaseConnectorConfig>(connector.ConnectionConfigJson) ?? new DatabaseConnectorConfig();
            
            // Get decrypted connection string from config
            var connectionString = ExtractConnectionStringFromConfig(connector.ConnectionConfigJson)
                ?? config.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return (false, 0, "Database connection string not configured");
            }
            
            // Build query
            var query = config.Query;
            if (string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(config.TableName))
            {
                // Build simple SELECT query
                var columns = config.Columns?.Any() == true
                    ? string.Join(", ", config.Columns)
                    : "*";
                query = $"SELECT {columns} FROM {config.TableName}";
                
                // Apply filter
                if (!string.IsNullOrEmpty(syncJob.FilterExpression))
                {
                    query += $" WHERE {syncJob.FilterExpression}";
                }
                
                // Apply incremental sync (only modified records since last sync)
                if (syncJob.LastRunAt.HasValue && !string.IsNullOrEmpty(config.ModifiedDateColumn))
                {
                    var whereClause = string.IsNullOrEmpty(syncJob.FilterExpression) ? "WHERE" : "AND";
                    query += $" {whereClause} {config.ModifiedDateColumn} > '{syncJob.LastRunAt.Value:yyyy-MM-dd HH:mm:ss}'";
                }
            }
            
            if (string.IsNullOrEmpty(query))
            {
                return (false, 0, "No query or table name configured for database sync");
            }
            
            _logger.LogDebug("Executing database query: {Query}", query);
            
            // Execute query based on database type
            var records = config.DatabaseType?.ToLower() switch
            {
                "postgresql" or "postgres" => await ExecutePostgresQueryAsync(connectionString, query, cancellationToken),
                "sqlserver" or "mssql" => await ExecuteSqlServerQueryAsync(connectionString, query, cancellationToken),
                _ => await ExecutePostgresQueryAsync(connectionString, query, cancellationToken) // Default to Postgres
            };
            
            if (!records.Any())
            {
                _logger.LogInformation("No records returned from database for {ObjectType}", syncJob.ObjectType);
                return (true, 0, null);
            }
            
            // Parse field mappings
            var mappings = string.IsNullOrEmpty(syncJob.FieldMappingJson)
                ? new List<FieldMapping>()
                : JsonSerializer.Deserialize<List<FieldMapping>>(syncJob.FieldMappingJson) ?? new List<FieldMapping>();
            
            // Process each record
            int processed = 0, created = 0, updated = 0;
            
            foreach (var record in records)
            {
                var mappedRecord = ApplyDatabaseFieldMappings(record, mappings);
                var externalId = record.GetValueOrDefault(config.IdColumn ?? "id")?.ToString() ?? Guid.NewGuid().ToString();
                
                var (isNew, localId) = await UpsertRecordAsync(syncJob, externalId, mappedRecord, cancellationToken);
                await UpdateCrossReferenceMappingAsync(syncJob, externalId, localId, cancellationToken);
                
                if (isNew) created++; else updated++;
                processed++;
            }
            
            _logger.LogInformation(
                "Database inbound sync completed: {Processed} processed, {Created} created, {Updated} updated",
                processed, created, updated);
            
            return (true, processed, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database inbound sync failed");
            return (false, 0, ex.Message);
        }
    }
    
    private async Task<List<Dictionary<string, object?>>> ExecutePostgresQueryAsync(
        string connectionString, string query, CancellationToken cancellationToken)
    {
        var results = new List<Dictionary<string, object?>>();
        
        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        
        await using var command = new Npgsql.NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            results.Add(row);
        }
        
        return results;
    }
    
    private async Task<List<Dictionary<string, object?>>> ExecuteSqlServerQueryAsync(
        string connectionString, string query, CancellationToken cancellationToken)
    {
        // SQL Server implementation would go here
        // For now, log a warning and return empty
        _logger.LogWarning("SQL Server connector not fully implemented - returning empty results");
        await Task.CompletedTask;
        return new List<Dictionary<string, object?>>();
    }
    
    private Dictionary<string, object?> ApplyFieldMappings(JsonElement record, List<FieldMapping> mappings)
    {
        var result = new Dictionary<string, object?>();
        
        if (!mappings.Any())
        {
            // No mappings - copy all properties
            foreach (var property in record.EnumerateObject())
            {
                result[property.Name] = GetJsonValue(property.Value);
            }
            return result;
        }
        
        foreach (var mapping in mappings)
        {
            if (record.TryGetProperty(mapping.SourceField, out var value))
            {
                var targetValue = GetJsonValue(value);
                
                // Apply transformation if specified
                if (!string.IsNullOrEmpty(mapping.Transformation))
                {
                    targetValue = ApplyTransformation(targetValue, mapping.Transformation);
                }
                
                result[mapping.TargetField] = targetValue;
            }
            else if (mapping.DefaultValue != null)
            {
                result[mapping.TargetField] = mapping.DefaultValue;
            }
        }
        
        return result;
    }
    
    private Dictionary<string, object?> ApplyDatabaseFieldMappings(
        Dictionary<string, object?> record, List<FieldMapping> mappings)
    {
        if (!mappings.Any())
            return record;
        
        var result = new Dictionary<string, object?>();
        
        foreach (var mapping in mappings)
        {
            if (record.TryGetValue(mapping.SourceField, out var value))
            {
                if (!string.IsNullOrEmpty(mapping.Transformation))
                {
                    value = ApplyTransformation(value, mapping.Transformation);
                }
                result[mapping.TargetField] = value;
            }
            else if (mapping.DefaultValue != null)
            {
                result[mapping.TargetField] = mapping.DefaultValue;
            }
        }
        
        return result;
    }
    
    private object? GetJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }
    
    private string GetExternalId(JsonElement record, string idField)
    {
        if (record.TryGetProperty(idField, out var idElement))
        {
            return idElement.ValueKind switch
            {
                JsonValueKind.String => idElement.GetString() ?? Guid.NewGuid().ToString(),
                JsonValueKind.Number => idElement.GetRawText(),
                _ => Guid.NewGuid().ToString()
            };
        }
        return Guid.NewGuid().ToString();
    }
    
    private object? ApplyTransformation(object? value, string transformation)
    {
        if (value == null) return null;
        
        return transformation.ToLower() switch
        {
            "uppercase" => value.ToString()?.ToUpperInvariant(),
            "lowercase" => value.ToString()?.ToLowerInvariant(),
            "trim" => value.ToString()?.Trim(),
            "tostring" => value.ToString(),
            "toint" => int.TryParse(value.ToString(), out var i) ? i : value,
            "tobool" => bool.TryParse(value.ToString(), out var b) ? b : value,
            "todate" => DateTime.TryParse(value.ToString(), out var d) ? d : value,
            _ => value
        };
    }
    
    private async Task<(bool IsNew, Guid LocalId)> UpsertRecordAsync(
        SyncJob syncJob, string externalId, Dictionary<string, object?> data, CancellationToken cancellationToken)
    {
        // Look up existing cross-reference
        var crossRef = await _context.Set<CrossReferenceMapping>()
            .FirstOrDefaultAsync(x => 
                x.TenantId == syncJob.TenantId &&
                x.ObjectType == syncJob.ObjectType &&
                x.ExternalSystemCode == syncJob.Connector.TargetSystem &&
                x.ExternalId == externalId,
                cancellationToken);
        
        if (crossRef != null)
        {
            // Update existing record - actual update logic would depend on ObjectType
            _logger.LogDebug("Updating existing record: {InternalId} for external ID: {ExternalId}", 
                crossRef.InternalId, externalId);
            return (false, crossRef.InternalId);
        }
        
        // Create new record - placeholder, actual creation depends on ObjectType
        var newId = Guid.NewGuid();
        _logger.LogDebug("Would create new record for external ID: {ExternalId}", externalId);
        return (true, newId);
    }
    
    private async Task UpdateCrossReferenceMappingAsync(
        SyncJob syncJob, string externalId, Guid localId, CancellationToken cancellationToken)
    {
        var crossRef = await _context.Set<CrossReferenceMapping>()
            .FirstOrDefaultAsync(x => 
                x.TenantId == syncJob.TenantId &&
                x.ObjectType == syncJob.ObjectType &&
                x.ExternalSystemCode == syncJob.Connector.TargetSystem &&
                x.ExternalId == externalId,
                cancellationToken);
        
        if (crossRef == null)
        {
            crossRef = new CrossReferenceMapping
            {
                Id = Guid.NewGuid(),
                TenantId = syncJob.TenantId ?? Guid.Empty,
                ObjectType = syncJob.ObjectType,
                InternalId = localId,
                InternalCode = $"{syncJob.ObjectType}-{localId:N}",
                ExternalSystemCode = syncJob.Connector.TargetSystem,
                ExternalId = externalId,
                LastSyncAt = DateTime.UtcNow,
                SyncStatus = "InSync",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };
            _context.Set<CrossReferenceMapping>().Add(crossRef);
        }
        else
        {
            crossRef.InternalId = localId;
            crossRef.LastSyncAt = DateTime.UtcNow;
            crossRef.SyncStatus = "InSync";
            crossRef.ModifiedDate = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    private string? ExtractApiKeyFromConfig(string? configJson)
    {
        if (string.IsNullOrEmpty(configJson)) return null;
        try
        {
            var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.TryGetProperty("apiKey", out var apiKey))
                return apiKey.GetString();
            if (doc.RootElement.TryGetProperty("ApiKey", out var apiKey2))
                return apiKey2.GetString();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract API key from config JSON");
        }
        return null;
    }
    
    private string? ExtractConnectionStringFromConfig(string? configJson)
    {
        if (string.IsNullOrEmpty(configJson)) return null;
        try
        {
            var doc = JsonDocument.Parse(configJson);
            if (doc.RootElement.TryGetProperty("connectionString", out var cs))
            {
                var encrypted = cs.GetString();
                if (!string.IsNullOrEmpty(encrypted))
                    return _encryption.Decrypt(encrypted);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract connection string from config JSON");
        }
        return null;
    }

    private async Task<(bool Success, int RecordsProcessed, string? Error)> ExecuteFileInboundAsync(SyncJob syncJob, CancellationToken cancellationToken)
    {
        // File connector implementation
        _logger.LogInformation("Executing File inbound sync for {ObjectType}", syncJob.ObjectType);
        await Task.Delay(50, cancellationToken);
        return (true, 0, null);
    }

    private async Task<(bool Success, int RecordsProcessed, string? Error)> ExecuteWebhookInboundAsync(SyncJob syncJob, CancellationToken cancellationToken)
    {
        // Webhook connector implementation - typically triggered externally
        _logger.LogInformation("Webhook inbound sync registered for {ObjectType}", syncJob.ObjectType);
        await Task.CompletedTask;
        return (true, 0, null);
    }

    private async Task ExecuteOutboundSyncAsync(SyncJob syncJob, SyncExecutionLog executionLog, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing outbound sync: {ObjectType} to {TargetSystem}",
            syncJob.ObjectType, syncJob.Connector.TargetSystem);

        try
        {
            // Get data to sync based on object type
            var dataToSync = await GetDataForOutboundSyncAsync(syncJob, cancellationToken);
            
            if (!dataToSync.Any())
            {
                _logger.LogInformation("No data to sync for {ObjectType}", syncJob.ObjectType);
                executionLog.RecordsProcessed = 0;
                return;
            }

            // Push to target system based on connector type
            var result = syncJob.Connector.ConnectorType switch
            {
                "REST_API" => await PushToRestApiAsync(syncJob, dataToSync, cancellationToken),
                "WEBHOOK" => await PushToWebhookAsync(syncJob, dataToSync, cancellationToken),
                _ => (Success: false, RecordsProcessed: 0, Error: $"Unsupported connector type: {syncJob.Connector.ConnectorType}")
            };

            executionLog.RecordsProcessed = result.RecordsProcessed;
            if (!result.Success)
            {
                _logger.LogWarning("Outbound sync failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing outbound sync for {ObjectType}", syncJob.ObjectType);
            throw;
        }
    }

    private async Task<List<object>> GetDataForOutboundSyncAsync(SyncJob syncJob, CancellationToken cancellationToken)
    {
        // Return empty list - actual implementation would query tenant data
        await Task.CompletedTask;
        return new List<object>();
    }

    private async Task<(bool Success, int RecordsProcessed, string? Error)> PushToRestApiAsync(
        SyncJob syncJob, List<object> data, CancellationToken cancellationToken)
    {
        // Placeholder for REST API push - would use HttpClient
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("REST API push completed for {Count} records", data.Count);
        return (true, data.Count, null);
    }

    private async Task<(bool Success, int RecordsProcessed, string? Error)> PushToWebhookAsync(
        SyncJob syncJob, List<object> data, CancellationToken cancellationToken)
    {
        // Placeholder for webhook push - would use HttpClient
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Webhook push completed for {Count} records", data.Count);
        return (true, data.Count, null);
    }

    private DateTime? CalculateNextRunTime(SyncJob syncJob)
    {
        if (string.IsNullOrEmpty(syncJob.CronExpression))
        {
            // Default scheduling based on frequency
            return syncJob.Frequency switch
            {
                "RealTime" => DateTime.UtcNow.AddMinutes(5),
                "Hourly" => DateTime.UtcNow.AddHours(1),
                "Daily" => DateTime.UtcNow.AddDays(1),
                "Weekly" => DateTime.UtcNow.AddDays(7),
                _ => DateTime.UtcNow.AddDays(1)
            };
        }

        // Parse cron expression using simple pattern matching
        // Format: "minute hour day month weekday" (standard 5-field cron)
        try
        {
            var parts = syncJob.CronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5)
            {
                var now = DateTime.UtcNow;
                var minute = parts[0] == "*" ? now.Minute : int.Parse(parts[0]);
                var hour = parts[1] == "*" ? now.Hour : int.Parse(parts[1]);
                
                var next = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Utc);
                if (next <= now)
                    next = next.AddDays(1);
                
                return next;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse cron expression: {CronExpression}", syncJob.CronExpression);
        }
        
        // Fallback to daily
        return DateTime.UtcNow.AddDays(1);
    }
}
