using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using System.Diagnostics;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Database schema and table structure test controller.
    /// Provides detailed information about all tables, columns, and relationships.
    /// </summary>
    [Route("api/test/schema")]
    [ApiController]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    public class SchemaTestController : ControllerBase
    {
        private readonly GrcDbContext _dbContext;
        private readonly ILogger<SchemaTestController> _logger;

        public SchemaTestController(
            GrcDbContext dbContext,
            ILogger<SchemaTestController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Get complete database schema report
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchemaReport()
        {
            var report = new SchemaReport
            {
                Timestamp = DateTime.UtcNow
            };

            var sw = Stopwatch.StartNew();

            try
            {
                report.Tables = await GetTablesAsync();
                report.ForeignKeys = await GetForeignKeysAsync();
                report.Indexes = await GetIndexesAsync();
                report.TableCounts = await GetTableCountsAsync();
                report.Status = "success";
            }
            catch (Exception ex)
            {
                report.Status = "error";
                report.Error = ex.Message;
                _logger.LogError(ex, "Schema report failed");
            }

            sw.Stop();
            report.DurationMs = sw.ElapsedMilliseconds;

            return Ok(report);
        }

        /// <summary>
        /// Get all tables
        /// </summary>
        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            var tables = await GetTablesAsync();
            return Ok(tables);
        }

        /// <summary>
        /// Get table details with columns
        /// </summary>
        [HttpGet("tables/{tableName}")]
        public async Task<IActionResult> GetTableDetails(string tableName)
        {
            var columns = await GetTableColumnsAsync(tableName);
            var constraints = await GetTableConstraintsAsync(tableName);
            var indexes = await GetTableIndexesAsync(tableName);
            var rowCount = await GetTableRowCountAsync(tableName);

            return Ok(new
            {
                tableName,
                rowCount,
                columns,
                constraints,
                indexes
            });
        }

        /// <summary>
        /// Get all foreign key relationships
        /// </summary>
        [HttpGet("relations")]
        public async Task<IActionResult> GetRelations()
        {
            var fks = await GetForeignKeysAsync();
            return Ok(fks);
        }

        /// <summary>
        /// Get row counts for all tables
        /// </summary>
        [HttpGet("counts")]
        public async Task<IActionResult> GetCounts()
        {
            var counts = await GetTableCountsAsync();
            return Ok(counts);
        }

        #region Private Methods

        private async Task<List<TableInfo>> GetTablesAsync()
        {
            var sql = @"
                SELECT 
                    table_name,
                    (SELECT COUNT(*) FROM information_schema.columns c 
                     WHERE c.table_name = t.table_name AND c.table_schema = 'public') as column_count
                FROM information_schema.tables t
                WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
                ORDER BY table_name";

            var tables = new List<TableInfo>();
            var connection = _dbContext.Database.GetDbConnection();
            
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(new TableInfo
                {
                    Name = reader.GetString(0),
                    ColumnCount = reader.GetInt32(1)
                });
            }
            await connection.CloseAsync();

            return tables;
        }

        private async Task<List<ColumnInfo>> GetTableColumnsAsync(string tableName)
        {
            var sql = @"
                SELECT 
                    column_name,
                    data_type,
                    is_nullable,
                    column_default,
                    character_maximum_length
                FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @tableName
                ORDER BY ordinal_position";

            var columns = new List<ColumnInfo>();
            var connection = _dbContext.Database.GetDbConnection();
            
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql.Replace("@tableName", $"'{tableName}'");
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(new ColumnInfo
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3),
                    MaxLength = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                });
            }
            await connection.CloseAsync();

            return columns;
        }

        private async Task<List<ForeignKeyInfo>> GetForeignKeysAsync()
        {
            var sql = @"
                SELECT
                    tc.table_name AS source_table,
                    kcu.column_name AS source_column,
                    ccu.table_name AS target_table,
                    ccu.column_name AS target_column,
                    tc.constraint_name
                FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu 
                    ON tc.constraint_name = kcu.constraint_name
                JOIN information_schema.constraint_column_usage ccu 
                    ON ccu.constraint_name = tc.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                    AND tc.table_schema = 'public'
                ORDER BY tc.table_name, kcu.column_name";

            var fks = new List<ForeignKeyInfo>();
            var connection = _dbContext.Database.GetDbConnection();
            
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fks.Add(new ForeignKeyInfo
                {
                    SourceTable = reader.GetString(0),
                    SourceColumn = reader.GetString(1),
                    TargetTable = reader.GetString(2),
                    TargetColumn = reader.GetString(3),
                    ConstraintName = reader.GetString(4)
                });
            }
            await connection.CloseAsync();

            return fks;
        }

        private async Task<List<IndexInfo>> GetIndexesAsync()
        {
            var sql = @"
                SELECT
                    tablename AS table_name,
                    indexname AS index_name,
                    indexdef AS definition
                FROM pg_indexes
                WHERE schemaname = 'public'
                ORDER BY tablename, indexname";

            var indexes = new List<IndexInfo>();
            var connection = _dbContext.Database.GetDbConnection();
            
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                indexes.Add(new IndexInfo
                {
                    TableName = reader.GetString(0),
                    IndexName = reader.GetString(1),
                    Definition = reader.GetString(2)
                });
            }
            await connection.CloseAsync();

            return indexes;
        }

        private async Task<List<IndexInfo>> GetTableIndexesAsync(string tableName)
        {
            var allIndexes = await GetIndexesAsync();
            return allIndexes.Where(i => i.TableName == tableName).ToList();
        }

        private async Task<List<ConstraintInfo>> GetTableConstraintsAsync(string tableName)
        {
            var sql = @"
                SELECT 
                    constraint_name,
                    constraint_type
                FROM information_schema.table_constraints
                WHERE table_schema = 'public' AND table_name = @tableName";

            var constraints = new List<ConstraintInfo>();
            var connection = _dbContext.Database.GetDbConnection();
            
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql.Replace("@tableName", $"'{tableName}'");
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                constraints.Add(new ConstraintInfo
                {
                    Name = reader.GetString(0),
                    Type = reader.GetString(1)
                });
            }
            await connection.CloseAsync();

            return constraints;
        }

        private async Task<Dictionary<string, long>> GetTableCountsAsync()
        {
            var tables = await GetTablesAsync();
            var counts = new Dictionary<string, long>();

            foreach (var table in tables)
            {
                try
                {
                    var count = await GetTableRowCountAsync(table.Name);
                    counts[table.Name] = count;
                }
                catch
                {
                    counts[table.Name] = -1;
                }
            }

            return counts;
        }

        private async Task<long> GetTableRowCountAsync(string tableName)
        {
            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
            
            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            
            return Convert.ToInt64(result);
        }

        #endregion
    }

    #region Models

    public class SchemaReport
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
        public long DurationMs { get; set; }
        public List<TableInfo> Tables { get; set; } = new();
        public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
        public List<IndexInfo> Indexes { get; set; } = new();
        public Dictionary<string, long> TableCounts { get; set; } = new();
        public int TotalTables => Tables.Count;
        public int TotalForeignKeys => ForeignKeys.Count;
        public int TotalIndexes => Indexes.Count;
    }

    public class TableInfo
    {
        public string Name { get; set; } = string.Empty;
        public int ColumnCount { get; set; }
    }

    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public string? DefaultValue { get; set; }
        public int? MaxLength { get; set; }
    }

    public class ForeignKeyInfo
    {
        public string SourceTable { get; set; } = string.Empty;
        public string SourceColumn { get; set; } = string.Empty;
        public string TargetTable { get; set; } = string.Empty;
        public string TargetColumn { get; set; } = string.Empty;
        public string ConstraintName { get; set; } = string.Empty;
    }

    public class IndexInfo
    {
        public string TableName { get; set; } = string.Empty;
        public string IndexName { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
    }

    public class ConstraintInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    #endregion
}
