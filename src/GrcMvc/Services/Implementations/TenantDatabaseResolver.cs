using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// Resolves database connection string for each tenant
    /// Implements database-per-tenant architecture for maximum isolation
    /// </summary>
    public class TenantDatabaseResolver : ITenantDatabaseResolver
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantDatabaseResolver> _logger;
        private readonly string _baseConnectionString;
        private readonly string _serverHost;
        private readonly string _serverPort;
        private readonly string _serverUser;
        private readonly string _serverPassword;

        public TenantDatabaseResolver(
            IConfiguration configuration,
            ILogger<TenantDatabaseResolver> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Get base connection string from configuration
            _baseConnectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new GrcException("DefaultConnection string not configured", GrcErrorCodes.GeneralError);

            // Parse connection string to extract components
            var builder = new NpgsqlConnectionStringBuilder(_baseConnectionString);
            _serverHost = builder.Host ?? "localhost";
            _serverPort = builder.Port.ToString();
            _serverUser = builder.Username ?? "postgres";
            _serverPassword = builder.Password ?? "";
        }

        /// <summary>
        /// Gets connection string for tenant database
        /// Format: Database name = "GrcMvc_Tenant_{TenantId}"
        /// </summary>
        public string GetConnectionString(Guid tenantId)
        {
            var databaseName = GetDatabaseName(tenantId);
            
            var connectionBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = _serverHost,
                Port = int.Parse(_serverPort),
                Username = _serverUser,
                Password = _serverPassword,
                Database = databaseName,
                Pooling = true,
                MinPoolSize = 2,
                MaxPoolSize = 20,
                CommandTimeout = 30,
                Timeout = 30
            };

            return connectionBuilder.ConnectionString;
        }

        /// <summary>
        /// Gets database name for tenant
        /// Uses sanitized format: GrcMvc_Tenant_{TenantId}
        /// </summary>
        public string GetDatabaseName(Guid tenantId)
        {
            // PostgreSQL database names must be lowercase and can contain underscores
            // Remove hyphens from GUID for valid database name
            var sanitizedId = tenantId.ToString("N"); // Remove hyphens
            return $"grcmvc_tenant_{sanitizedId}";
        }

        /// <summary>
        /// Creates a new database for tenant
        /// </summary>
        public async Task<bool> CreateTenantDatabaseAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var databaseName = GetDatabaseName(tenantId);

                // Check if database already exists
                if (await DatabaseExistsAsync(tenantId, cancellationToken))
                {
                    _logger.LogInformation("Database {DatabaseName} already exists for tenant {TenantId}", 
                        databaseName, tenantId);
                    return true;
                }

                // Connect to postgres database to create new database
                var masterConnectionString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
                {
                    Database = "postgres" // Connect to default postgres database
                }.ConnectionString;

                await using var connection = new NpgsqlConnection(masterConnectionString);
                await connection.OpenAsync(cancellationToken);

                // Create database (PostgreSQL doesn't support parameters in CREATE DATABASE)
                // We sanitize tenantId to prevent SQL injection
                var createDbCommand = $"CREATE DATABASE \"{databaseName}\" WITH ENCODING 'UTF8' LC_COLLATE='en_US.UTF-8' LC_CTYPE='en_US.UTF-8'";
                
                await using var command = new NpgsqlCommand(createDbCommand, connection);
                await command.ExecuteNonQueryAsync(cancellationToken);

                _logger.LogInformation("Created database {DatabaseName} for tenant {TenantId}", 
                    databaseName, tenantId);

                // Run migrations on new database
                await MigrateTenantDatabaseAsync(tenantId, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database for tenant {TenantId}", tenantId);
                return false;
            }
        }

        /// <summary>
        /// Checks if tenant database exists
        /// </summary>
        public async Task<bool> DatabaseExistsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var databaseName = GetDatabaseName(tenantId);

                // Connect to postgres database to check
                var masterConnectionString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
                {
                    Database = "postgres"
                }.ConnectionString;

                await using var connection = new NpgsqlConnection(masterConnectionString);
                await connection.OpenAsync(cancellationToken);

                var checkCommand = @"
                    SELECT 1 FROM pg_database 
                    WHERE datname = @databaseName";

                await using var command = new NpgsqlCommand(checkCommand, connection);
                command.Parameters.AddWithValue("databaseName", databaseName);

                var result = await command.ExecuteScalarAsync(cancellationToken);
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if database exists for tenant {TenantId}", tenantId);
                return false;
            }
        }

        /// <summary>
        /// Runs migrations on tenant database
        /// </summary>
        public async Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = GetConnectionString(tenantId);
                
                var optionsBuilder = new DbContextOptionsBuilder<GrcDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                await using var tenantContext = new GrcDbContext(optionsBuilder.Options);
                await tenantContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("Migrated database for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate database for tenant {TenantId}", tenantId);
                throw;
            }
        }

        /// <summary>
        /// Gets all tenant IDs that have databases
        /// </summary>
        public async Task<List<Guid>> GetTenantIdsWithDatabasesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tenantIds = new List<Guid>();

                // Connect to postgres database to list all tenant databases
                var masterConnectionString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
                {
                    Database = "postgres"
                }.ConnectionString;

                await using var connection = new NpgsqlConnection(masterConnectionString);
                await connection.OpenAsync(cancellationToken);

                var listCommand = @"
                    SELECT datname FROM pg_database 
                    WHERE datname LIKE 'grcmvc_tenant_%'";

                await using var command = new NpgsqlCommand(listCommand, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    var databaseName = reader.GetString(0);
                    // Extract tenant ID from database name: grcmvc_tenant_{guid}
                    var guidString = databaseName.Replace("grcmvc_tenant_", "");
                    if (Guid.TryParse(guidString, out var tenantId))
                    {
                        tenantIds.Add(tenantId);
                    }
                }

                return tenantIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tenant IDs with databases");
                return new List<Guid>();
            }
        }

    }
}
