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

            // Get connection string from environment variables or configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string not found. Please set ConnectionStrings__DefaultConnection environment variable " +
                    "or configure it in appsettings.json. Example: " +
                    "ConnectionStrings__DefaultConnection=\"Host=localhost;Database=GrcMvc;Username=postgres;Password=your_password;Port=5432\"");
            }

            // Configure PostgreSQL (adjust if using SQL Server)
            optionsBuilder.UseNpgsql(connectionString);

            return new GrcDbContext(optionsBuilder.Options);
        }
    }
}
