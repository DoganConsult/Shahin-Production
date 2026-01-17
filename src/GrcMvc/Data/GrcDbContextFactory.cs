using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GrcMvc.Data
{
    /// <summary>
    /// Design-time factory for GrcDbContext.
    /// Used by EF Core tools (migrations, etc.) when running outside of the application.
    /// </summary>
    public class GrcDbContextFactory : IDesignTimeDbContextFactory<GrcDbContext>
    {
        public GrcDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<GrcDbContext>();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Fallback for local development migrations if not set in environment
                connectionString = "Host=localhost;Database=GrcMvc;Username=postgres;Password=CHANGE_ME";
            }

            // Configure PostgreSQL (adjust if using SQL Server)
            optionsBuilder.UseNpgsql(connectionString);

            return new GrcDbContext(optionsBuilder.Options);
        }
    }
}
