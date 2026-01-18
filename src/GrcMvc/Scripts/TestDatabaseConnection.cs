using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GrcMvc.Scripts
{
    /// <summary>
    /// Simple database connection test script
    /// Usage: dotnet run --project src/GrcMvc -- TestDbConnection
    /// </summary>
    public class TestDatabaseConnection
    {
        public static async Task<int> TestConnections(IConfiguration configuration)
        {
            var logPath = @"c:\Shahin-ai\.cursor\debug.log";
            
            Console.WriteLine("========================================");
            Console.WriteLine("Database Connection Test");
            Console.WriteLine("========================================\n");

            var results = new List<(string Name, bool Success, string Message, TimeSpan Duration)>();

            // Test DefaultConnection (Main Database)
            var defaultConn = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(defaultConn))
            {
                var result = await TestConnection("DefaultConnection (Main DB)", defaultConn, logPath);
                results.Add(result);
            }
            else
            {
                Console.WriteLine("❌ DefaultConnection: Not configured");
                results.Add(("DefaultConnection", false, "Not configured", TimeSpan.Zero));
            }

            // Test GrcAuthDb (Auth Database)
            var authConn = configuration.GetConnectionString("GrcAuthDb");
            if (!string.IsNullOrWhiteSpace(authConn))
            {
                var result = await TestConnection("GrcAuthDb (Auth DB)", authConn, logPath);
                results.Add(result);
            }
            else
            {
                Console.WriteLine("⚠️ GrcAuthDb: Not configured (will use DefaultConnection)");
            }

            // Test HangfireConnection (if configured)
            var hangfireConn = configuration.GetConnectionString("HangfireConnection");
            if (!string.IsNullOrWhiteSpace(hangfireConn))
            {
                var result = await TestConnection("HangfireConnection", hangfireConn, logPath);
                results.Add(result);
            }
            else
            {
                Console.WriteLine("ℹ️ HangfireConnection: Not configured (optional)");
            }

            // Summary
            Console.WriteLine("\n========================================");
            Console.WriteLine("Test Summary");
            Console.WriteLine("========================================");
            
            var allSuccess = true;
            foreach (var (name, success, message, duration) in results)
            {
                var status = success ? "✅" : "❌";
                Console.WriteLine($"{status} {name}: {message} ({duration.TotalMilliseconds:F0}ms)");
                if (!success) allSuccess = false;
            }

            return allSuccess ? 0 : 1;
        }

        private static async Task<(string Name, bool Success, string Message, TimeSpan Duration)> TestConnection(
            string name, 
            string connectionString, 
            string logPath)
        {
            var startTime = DateTime.UtcNow;
            
            // #region agent log - DB_TEST: Connection test start
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "db-test-run",
                    hypothesisId = "DB_TEST",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "TestDatabaseConnection.cs:TestConnection",
                    message = $"Testing {name} connection",
                    data = new
                    {
                        connectionName = name,
                        hasConnectionString = !string.IsNullOrEmpty(connectionString),
                        connectionStringLength = connectionString?.Length ?? 0
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            try
            {
                Console.Write($"Testing {name}... ");
                
                using var connection = new NpgsqlConnection(connectionString);
                
                // #region agent log - DB_TEST: Connection attempt
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDatabaseConnection.cs:TestConnection",
                        message = $"Opening connection to {name}",
                        data = new { connectionName = name }
                    });
                    System.IO.File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                await connection.OpenAsync();
                
                // #region agent log - DB_TEST: Connection opened
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDatabaseConnection.cs:TestConnection",
                        message = $"Connection to {name} opened successfully",
                        data = new { connectionName = name }
                    });
                    System.IO.File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                // Test query
                using var command = new NpgsqlCommand("SELECT version(), current_database(), current_user", connection);
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var version = reader.GetString(0);
                    var database = reader.GetString(1);
                    var user = reader.GetString(2);
                    
                    await reader.CloseAsync();
                    await connection.CloseAsync();
                    
                    var duration = DateTime.UtcNow - startTime;
                    
                    // #region agent log - DB_TEST: Connection test success
                    try
                    {
                        var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            sessionId = "debug-session",
                            runId = "db-test-run",
                            hypothesisId = "DB_TEST",
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            location = "TestDatabaseConnection.cs:TestConnection",
                            message = $"Connection to {name} test successful",
                            data = new
                            {
                                connectionName = name,
                                database = database,
                                user = user,
                                durationMs = duration.TotalMilliseconds
                            }
                        });
                        System.IO.File.AppendAllText(logPath, logEntry + "\n");
                    }
                    catch { }
                    // #endregion

                    Console.WriteLine($"✅ SUCCESS");
                    Console.WriteLine($"   Database: {database}");
                    Console.WriteLine($"   User: {user}");
                    Console.WriteLine($"   PostgreSQL: {version.Substring(0, Math.Min(50, version.Length))}...");
                    
                    return (name, true, "Connected successfully", duration);
                }
                else
                {
                    var duration = DateTime.UtcNow - startTime;
                    return (name, false, "Query returned no results", duration);
                }
            }
            catch (NpgsqlException ex)
            {
                var duration = DateTime.UtcNow - startTime;
                
                // #region agent log - DB_TEST: Connection test failed
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDatabaseConnection.cs:TestConnection",
                        message = $"Connection to {name} test failed",
                        data = new
                        {
                            connectionName = name,
                            error = ex.Message,
                            sqlState = ex.SqlState,
                            durationMs = duration.TotalMilliseconds
                        }
                    });
                    System.IO.File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                Console.WriteLine($"❌ FAILED");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.WriteLine($"   SQL State: {ex.SqlState}");
                
                return (name, false, ex.Message, duration);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                
                // #region agent log - DB_TEST: Connection test error
                try
                {
                    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        sessionId = "debug-session",
                        runId = "db-test-run",
                        hypothesisId = "DB_TEST",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "TestDatabaseConnection.cs:TestConnection",
                        message = $"Connection to {name} test error",
                        data = new
                        {
                            connectionName = name,
                            error = ex.Message,
                            errorType = ex.GetType().Name,
                            durationMs = duration.TotalMilliseconds
                        }
                    });
                    System.IO.File.AppendAllText(logPath, logEntry + "\n");
                }
                catch { }
                // #endregion

                Console.WriteLine($"❌ ERROR");
                Console.WriteLine($"   Error: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                
                return (name, false, ex.Message, duration);
            }
        }
    }
}
