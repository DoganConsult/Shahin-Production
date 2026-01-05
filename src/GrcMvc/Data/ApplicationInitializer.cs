using GrcMvc.Data.Seeds;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Data;

/// <summary>
/// Application Initializer - Runs database seed operations on startup
/// Initializes all reference data including workflows, entities, and configurations
/// </summary>
public class ApplicationInitializer
{
    private readonly GrcDbContext _context;
    private readonly ILogger<ApplicationInitializer> _logger;
    private readonly IHostEnvironment _environment;

    public ApplicationInitializer(
        GrcDbContext context,
        ILogger<ApplicationInitializer> logger,
        IHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>Initialize all seed data</summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🚀 Starting application initialization...");

            // Layer 1: Global Catalogs from CSV files (92 regulators, 163 frameworks, 13,528 controls)
            await SeedCatalogsFromCsvAsync();

            // Seed Role Profiles (STAGE 2 - KSA & Multi-level Approval)
            await RoleProfileSeeds.SeedRoleProfilesAsync(_context, _logger);

            // Seed Workflow Definitions (STAGE 2)
            await WorkflowDefinitionSeeds.SeedWorkflowDefinitionsAsync(_context, _logger);

            _logger.LogInformation("✅ Application initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during application initialization");
            throw;
        }
    }

    private async Task SeedCatalogsFromCsvAsync()
    {
        // Determine the path to CSV seed files
        var seedDataPath = Path.Combine(_environment.ContentRootPath, "Models", "Entities", "Catalogs");

        if (!Directory.Exists(seedDataPath))
        {
            _logger.LogWarning($"⚠️ Seed data path not found: {seedDataPath}. Falling back to hardcoded seeds.");
            await RegulatorSeeds.SeedRegulatorsAsync(_context, _logger);
            return;
        }

        var csvSeeder = new CatalogCsvSeeder(_context, _logger, seedDataPath);

        await csvSeeder.SeedAllCatalogsAsync();
    }
}
