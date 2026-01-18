using System.Collections.Generic;

namespace GrcMvc.Models.DTOs;

/// <summary>
/// Configuration for REST API connectors
/// </summary>
public class RestApiConfig
{
    /// <summary>API endpoint URL</summary>
    public string? Endpoint { get; set; }
    
    /// <summary>Authentication scheme: Bearer, Basic, ApiKey</summary>
    public string? AuthScheme { get; set; } = "Bearer";
    
    /// <summary>Header name for API key authentication</summary>
    public string? ApiKeyHeader { get; set; } = "X-API-Key";
    
    /// <summary>Custom HTTP headers to include in requests</summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }
    
    /// <summary>JSON path to data array in response (e.g., "data.items")</summary>
    public string? DataPath { get; set; }
    
    /// <summary>Field name for record ID in API response</summary>
    public string? IdField { get; set; } = "id";
    
    /// <summary>Pagination type: offset, cursor, page</summary>
    public string? PaginationType { get; set; }
    
    /// <summary>Page size for pagination</summary>
    public int PageSize { get; set; } = 100;
    
    /// <summary>Query parameter name for page size</summary>
    public string? PageSizeParam { get; set; } = "limit";
    
    /// <summary>Query parameter name for offset/page</summary>
    public string? OffsetParam { get; set; } = "offset";
    
    /// <summary>HTTP method for read operations</summary>
    public string? HttpMethod { get; set; } = "GET";
    
    /// <summary>Request timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>Number of retry attempts</summary>
    public int RetryCount { get; set; } = 3;
}

/// <summary>
/// Configuration for database connectors
/// </summary>
public class DatabaseConnectorConfig
{
    /// <summary>Database type: postgresql, sqlserver, mysql, oracle</summary>
    public string? DatabaseType { get; set; } = "postgresql";
    
    /// <summary>Connection string (usually encrypted in connector)</summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>Table name for simple queries</summary>
    public string? TableName { get; set; }
    
    /// <summary>Custom SQL query</summary>
    public string? Query { get; set; }
    
    /// <summary>Columns to select (if not using custom query)</summary>
    public List<string>? Columns { get; set; }
    
    /// <summary>Column name for primary key/ID</summary>
    public string? IdColumn { get; set; } = "id";
    
    /// <summary>Column name for modified date (for incremental sync)</summary>
    public string? ModifiedDateColumn { get; set; }
    
    /// <summary>Schema name (optional)</summary>
    public string? Schema { get; set; }
    
    /// <summary>Batch size for reading records</summary>
    public int BatchSize { get; set; } = 1000;
}

/// <summary>
/// Field mapping for sync jobs (compatible with SyncJob.FieldMappingJson)
/// </summary>
public class SyncFieldMapping
{
    /// <summary>Source field name from external system</summary>
    public string SourceField { get; set; } = "";
    
    /// <summary>Target field name in local system</summary>
    public string TargetField { get; set; } = "";
    
    /// <summary>Transformation to apply: uppercase, lowercase, trim, tostring, toint, tobool, todate</summary>
    public string? Transformation { get; set; }
    
    /// <summary>Default value if source field is null/missing</summary>
    public object? DefaultValue { get; set; }
    
    /// <summary>Whether this field is required</summary>
    public bool IsRequired { get; set; }
    
    /// <summary>Data type: string, int, bool, date, decimal</summary>
    public string? DataType { get; set; }
}

/// <summary>
/// Configuration for webhook connectors
/// </summary>
public class WebhookConnectorConfig
{
    /// <summary>Secret for validating webhook signatures</summary>
    public string? WebhookSecret { get; set; }
    
    /// <summary>Algorithm for signature validation: hmac-sha256, hmac-sha1</summary>
    public string? SignatureAlgorithm { get; set; } = "hmac-sha256";
    
    /// <summary>Header name containing signature</summary>
    public string? SignatureHeader { get; set; } = "X-Webhook-Signature";
    
    /// <summary>Expected source IPs (for additional validation)</summary>
    public List<string>? AllowedIPs { get; set; }
    
    /// <summary>Event types to process</summary>
    public List<string>? EventTypes { get; set; }
}

/// <summary>
/// Configuration for file-based connectors (SFTP, S3, etc.)
/// </summary>
public class FileConnectorConfig
{
    /// <summary>File source type: sftp, s3, azure, local</summary>
    public string? SourceType { get; set; }
    
    /// <summary>Host for SFTP connections</summary>
    public string? Host { get; set; }
    
    /// <summary>Port for SFTP connections</summary>
    public int? Port { get; set; }
    
    /// <summary>Username for SFTP authentication</summary>
    public string? Username { get; set; }
    
    /// <summary>Path to files</summary>
    public string? Path { get; set; }
    
    /// <summary>File pattern to match (e.g., "*.csv")</summary>
    public string? FilePattern { get; set; }
    
    /// <summary>File format: csv, json, xml, xlsx</summary>
    public string? FileFormat { get; set; } = "csv";
    
    /// <summary>CSV delimiter character</summary>
    public string? Delimiter { get; set; } = ",";
    
    /// <summary>Whether first row is header</summary>
    public bool HasHeader { get; set; } = true;
    
    /// <summary>S3 bucket name</summary>
    public string? BucketName { get; set; }
    
    /// <summary>Azure container name</summary>
    public string? ContainerName { get; set; }
}
