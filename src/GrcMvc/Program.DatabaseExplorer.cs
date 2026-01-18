using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;

namespace GrcMvc
{
    /// <summary>
    /// Database Explorer - Command line tool to explore database tables
    /// Usage: dotnet run -- explore-db [options]
    /// </summary>
    public static class DatabaseExplorer
    {
        public static async Task<int> ExploreDatabase(string[] args)
        {
            var showSampleData = args.Contains("--sample-data");
            var sampleRows = 5;
            var exportToFile = args.Contains("--export");
            var showSchema = args.Contains("--schema");
            var tableName = args.FirstOrDefault(a => a.StartsWith("--table="))?.Split('=')[1];

            Console.WriteLine("========================================");
            Console.WriteLine("DATABASE EXPLORER");
            Console.WriteLine("Using Application's DbContext");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Get connection string
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("❌ No connection string found!");
                Console.WriteLine();
                Console.WriteLine("Set one of these environment variables:");
                Console.WriteLine("  DATABASE_URL (Railway format)");
                Console.WriteLine("  ConnectionStrings__DefaultConnection");
                return 1;
            }

            // Convert Railway format if needed
            if (connectionString.StartsWith("postgresql://"))
            {
                try
                {
                    var uri = new Uri(connectionString);
                    var userInfo = uri.UserInfo.Split(':');
                    var user = Uri.UnescapeDataString(userInfo[0]);
                    var password = Uri.UnescapeDataString(userInfo[1]);
                    var database = uri.AbsolutePath.TrimStart('/');

                    connectionString = $"Host={uri.Host};Database={database};Username={user};Password={password};Port={uri.Port}";
                    Console.WriteLine("✅ Converted Railway DATABASE_URL format");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to parse DATABASE_URL: {ex.Message}");
                    return 1;
                }
            }

            // Mask password for display
            var maskedConnection = System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"(Password=)[^;]+", 
                "$1***");
            Console.WriteLine($"Connection: {maskedConnection}");
            Console.WriteLine();

            try
            {
                // Create DbContext
                var optionsBuilder = new DbContextOptionsBuilder<GrcDbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.EnableSensitiveDataLogging(false);

                using var context = new GrcDbContext(optionsBuilder.Options);

                // Test connection
                Console.WriteLine("🔌 Testing connection...");
                var canConnect = await context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    Console.WriteLine("❌ Cannot connect to database!");
                    return 1;
                }

                Console.WriteLine("✅ Connected successfully!");
                Console.WriteLine();

                // Get database info
                var dbConnection = context.Database.GetDbConnection();
                await dbConnection.OpenAsync();

                Console.WriteLine("========================================");
                Console.WriteLine("DATABASE INFORMATION");
                Console.WriteLine("========================================");
                Console.WriteLine($"Database: {dbConnection.Database}");
                Console.WriteLine($"Server: {dbConnection.DataSource}");
                Console.WriteLine($"State: {dbConnection.State}");
                Console.WriteLine();

                // Get PostgreSQL version
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = "SELECT version()";
                    var version = await cmd.ExecuteScalarAsync();
                    Console.WriteLine($"Version: {version?.ToString()?.Split('\n')[0]}");
                }
                Console.WriteLine();

                // Get all entity types from the model
                var entityTypes = context.Model.GetEntityTypes()
                    .Where(t => !t.IsOwned())
                    .OrderBy(t => t.GetTableName())
                    .ToList();

                Console.WriteLine("========================================");
                Console.WriteLine($"TABLES ({entityTypes.Count} total)");
                Console.WriteLine("========================================");
                Console.WriteLine();

                var tableStats = new System.Collections.Generic.List<(string TableName, string EntityName, int Count, bool HasData)>();

                foreach (var entityType in entityTypes)
                {
                    var currentTableName = entityType.GetTableName();
                    var schema = entityType.GetSchema() ?? "public";
                    var entityName = entityType.ClrType.Name;

                    try
                    {
                        // Get row count
                        using var cmd = dbConnection.CreateCommand();
                        cmd.CommandText = $"SELECT COUNT(*) FROM \"{schema}\".\"{currentTableName}\"";
                        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                        tableStats.Add((currentTableName, entityName, count, count > 0));

                        var status = count > 0 ? "✅" : "⚪";
                        Console.WriteLine($"{status} {currentTableName,-50} ({count,8:N0} rows) - {entityName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  {currentTableName,-50} (ERROR) - {ex.Message}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("SUMMARY");
                Console.WriteLine("========================================");
                Console.WriteLine($"Total Tables: {tableStats.Count}");
                Console.WriteLine($"Tables with Data: {tableStats.Count(t => t.HasData)}");
                Console.WriteLine($"Empty Tables: {tableStats.Count(t => !t.HasData)}");
                Console.WriteLine($"Total Rows: {tableStats.Sum(t => t.Count):N0}");
                Console.WriteLine();

                // Show top tables
                Console.WriteLine("Top 15 Tables by Row Count:");
                Console.WriteLine("----------------------------");
                foreach (var table in tableStats.OrderByDescending(t => t.Count).Take(15))
                {
                    Console.WriteLine($"  {table.TableName,-45} {table.Count,10:N0} rows");
                }
                Console.WriteLine();

                // Show schema if requested
                if (showSchema)
                {
                    Console.WriteLine("========================================");
                    Console.WriteLine("TABLE SCHEMAS");
                    Console.WriteLine("========================================");
                    Console.WriteLine();

                    foreach (var entityType in entityTypes.Take(10))
                    {
                        var schemaTableName = entityType.GetTableName();
                        Console.WriteLine($"--- {schemaTableName} ---");

                        var properties = entityType.GetProperties();
                        foreach (var prop in properties)
                        {
                            var columnName = prop.GetColumnName();
                            var columnType = prop.GetColumnType();
                            var isNullable = prop.IsNullable ? "NULL" : "NOT NULL";
                            var isPK = prop.IsPrimaryKey() ? " [PK]" : "";
                            var isFK = prop.IsForeignKey() ? " [FK]" : "";

                            Console.WriteLine($"  {columnName,-40} {columnType,-20} {isNullable}{isPK}{isFK}");
                        }
                        Console.WriteLine();
                    }
                }

                // Show sample data if requested
                if (showSampleData && !string.IsNullOrEmpty(tableName))
                {
                    Console.WriteLine("========================================");
                    Console.WriteLine($"SAMPLE DATA: {tableName}");
                    Console.WriteLine("========================================");
                    Console.WriteLine();

                    var entityType = entityTypes.FirstOrDefault(e => 
                        e.GetTableName().Equals(tableName, StringComparison.OrdinalIgnoreCase));

                    if (entityType != null)
                    {
                        var schema = entityType.GetSchema() ?? "public";
                        using var cmd = dbConnection.CreateCommand();
                        cmd.CommandText = $"SELECT * FROM \"{schema}\".\"{tableName}\" LIMIT {sampleRows}";

                        using var reader = await cmd.ExecuteReaderAsync();
                        var rowNum = 1;

                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine($"Row {rowNum}:");
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var fieldName = reader.GetName(i);
                                var value = reader.IsDBNull(i) ? "(null)" : reader.GetValue(i)?.ToString();
                                
                                if (value != null && value.Length > 100)
                                    value = value.Substring(0, 97) + "...";

                                Console.WriteLine($"  {fieldName}: {value}");
                            }
                            Console.WriteLine();
                            rowNum++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ Table '{tableName}' not found!");
                    }
                }

                await dbConnection.CloseAsync();

                Console.WriteLine("========================================");
                Console.WriteLine("Exploration Complete!");
                Console.WriteLine("========================================");
                Console.WriteLine();
                Console.WriteLine("Usage examples:");
                Console.WriteLine("  dotnet run -- explore-db");
                Console.WriteLine("  dotnet run -- explore-db --schema");
                Console.WriteLine("  dotnet run -- explore-db --sample-data --table=Tenants");
                Console.WriteLine();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return 1;
            }
        }
    }
}
