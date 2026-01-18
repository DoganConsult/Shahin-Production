using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GrcMvc
{
    /// <summary>
    /// Simple database connection test
    /// Run: dotnet run -- TestDb
    /// </summary>
    public class TestDbConnection
    {
        public static async Task<int> Run(IConfiguration configuration)
        {
            var logPath = @"c:\Shahin-ai\.cursor\debug.log";
            
            Console.WriteLine("========================================");
            Console.WriteLine("Database Connection Test");
            Console.WriteLine("========================================\n");

            // Test DefaultConnection
            var defaultConn = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(defaultConn))
            {
                Console.WriteLine("❌ DefaultConnection: Not configured");
                Console.WriteLine("\nPlease set connection string:");
                Console.WriteLine("  Environment variable: ConnectionStrings__DefaultConnection");
                Console.WriteLine("  Or in appsettings.json: ConnectionStrings.DefaultConnection");
                return 1;
            }

            Console.WriteLine($"Testing DefaultConnection...");
            Console.WriteLine($"  Connection String: {MaskConnectionString(defaultConn)}\n");

            // #region agent log - DB_TEST: Connection test start
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "db-test-run",
                    hypothesisId = "DB_TEST",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "TestDbConnection.cs:Run",
                    message = "Starting database connection test",
                    data = new
                    {
                        hasConnectionString = !string.IsNullOrEmpty(defaultConn),
                        connectionStringLength = defaultConn?.Length ?? 0
                    }
                });
                File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            try
            {
                using var connection = new NpgsqlConnection(defaultConn);
                
                // #region agent log - DB_TEST: Opening connection
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDbConnection.cs:Run",
                        message = "Opening database connection",
                        data = new { }
                    });
                    File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                Console.Write("Connecting... ");
                await connection.OpenAsync();
                Console.WriteLine("✅ Connected");

                    // Test query - Get database info
                using var infoCommand = new NpgsqlCommand("SELECT version(), current_database(), current_user", connection);
                using var infoReader = await infoCommand.ExecuteReaderAsync();
                
                string? version = null;
                string? database = null;
                string? user = null;
                
                if (await infoReader.ReadAsync())
                {
                    version = infoReader.GetString(0);
                    database = infoReader.GetString(1);
                    user = infoReader.GetString(2);
                }
                await infoReader.CloseAsync();
                
                // Count tables
                Console.WriteLine("\n📊 Counting tables in database...");
                using var tableCommand = new NpgsqlCommand(@"
                    SELECT COUNT(*) 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_type = 'BASE TABLE'", connection);
                var tableCount = (long)await tableCommand.ExecuteScalarAsync();
                
                // Get table names
                using var tablesCommand = new NpgsqlCommand(@"
                    SELECT table_name 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_type = 'BASE TABLE'
                    ORDER BY table_name", connection);
                using var tablesReader = await tablesCommand.ExecuteReaderAsync();
                
                var tableNames = new List<string>();
                while (await tablesReader.ReadAsync())
                {
                    tableNames.Add(tablesReader.GetString(0));
                }
                await tablesReader.CloseAsync();
                
                await connection.CloseAsync();
                
                // #region agent log - DB_TEST: Connection success with table count
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDbConnection.cs:Run",
                        message = "Database connection test successful with table count",
                        data = new
                        {
                            database = database,
                            user = user,
                            version = version?.Substring(0, Math.Min(50, version?.Length ?? 0)),
                            tableCount = tableCount,
                            tables = tableNames
                        }
                    });
                    File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                Console.WriteLine("\n✅ Connection Test Successful!");
                Console.WriteLine($"   Database: {database}");
                Console.WriteLine($"   User: {user}");
                Console.WriteLine($"   PostgreSQL: {version?.Substring(0, Math.Min(50, version?.Length ?? 0))}...");
                Console.WriteLine("");
                Console.WriteLine("📊 Database Tables:");
                Console.WriteLine($"   Total Tables: {tableCount}");
                Console.WriteLine("");
                
                if (tableNames.Count > 0)
                {
                    Console.WriteLine("   Table Names:");
                    foreach (var tableName in tableNames)
                    {
                        Console.WriteLine($"     - {tableName}");
                    }
                }
                else
                {
                    Console.WriteLine("   ⚠️  No tables found in 'public' schema");
                }
                Console.WriteLine("");
                
                return 0;
            }
            catch (NpgsqlException ex)
            {
                // #region agent log - DB_TEST: Connection failed
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDbConnection.cs:Run",
                        message = "Database connection test failed",
                        data = new
                        {
                            error = ex.Message,
                            sqlState = ex.SqlState
                        }
                    });
                    File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                Console.WriteLine($"❌ Connection Failed!");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.WriteLine($"   SQL State: {ex.SqlState}");
                Console.WriteLine("");
                Console.WriteLine("Troubleshooting:");
                Console.WriteLine("  1. Verify PostgreSQL server is running");
                Console.WriteLine("  2. Check connection string format");
                Console.WriteLine("  3. Verify database exists");
                Console.WriteLine("  4. Check user credentials and permissions");
                Console.WriteLine("  5. Verify firewall/network settings");
                
                return 1;
            }
            catch (Exception ex)
            {
                // #region agent log - DB_TEST: Connection error
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDbConnection.cs:Run",
                        message = "Database connection test error",
                        data = new
                        {
                            error = ex.Message,
                            errorType = ex.GetType().Name
                        }
                    });
                    File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                
                return 1;
            }
        }

        private static string MaskConnectionString(string connString)
        {
            if (string.IsNullOrWhiteSpace(connString))
                return "(empty)";

            try
            {
                var parts = connString.Split(';');
                var masked = new List<string>();
                foreach (var part in parts)
                {
                    if (part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                        part.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                    {
                        masked.Add(part.Split('=')[0] + "=***");
                    }
                    else
                    {
                        masked.Add(part);
                    }
                }
                return string.Join(";", masked);
            }
            catch
            {
                return "(error parsing)";
            }
        }
    }
}
