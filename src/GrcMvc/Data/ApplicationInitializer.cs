using GrcMvc.Data.Seeds;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceProvider _serviceProvider;

    public ApplicationInitializer(
        GrcDbContext context,
        ILogger<ApplicationInitializer> logger,
        IHostEnvironment environment,
        UserManager<ApplicationUser> userManager,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
        _userManager = userManager;
        _serviceProvider = serviceProvider;
    }

    /// <summary>Initialize all seed data</summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🚀 Starting application initialization...");

            // Layer 1: Global Catalogs from CSV files (92 regulators, 163 frameworks, 13,528 controls)
            await SeedCatalogsFromCsvAsync();

            // Seed Subscription Plans (MVP, Professional, Enterprise)
            _logger.LogInformation("📋 Seeding subscription plans...");
            await SubscriptionPlanSeeds.SeedAsync(_context);

            // Seed GRC Lookup Tables (Countries, Sectors, Frameworks, etc.)
            _logger.LogInformation("🌍 Seeding GRC lookup tables (Countries, Sectors, Frameworks, Regulators)...");
            await GrcLookupSeeds.SeedAllAsync(_context, _logger);

            // Seed Role Profiles (STAGE 2 - KSA & Multi-level Approval)
            await RoleProfileSeeds.SeedRoleProfilesAsync(_context, _logger);

            // Seed Workflow Definitions (STAGE 2)
            await WorkflowDefinitionSeeds.SeedWorkflowDefinitionsAsync(_context, _logger);

            // Seed Comprehensive Derivation Rules (50+ rules for KSA GRC)
            await DerivationRulesSeeds.SeedAsync(_context, _logger);

            // Seed RBAC System (Permissions, Features, Roles, Mappings) - MUST be before user seeding
            var defaultTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);
            if (defaultTenant != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await RbacSeeds.SeedRbacSystemAsync(_context, roleManager, defaultTenant.Id, _logger);

                // Seed GRC Policy Enforcement Roles (8 baseline roles)
                // Uses RoleManager API to add permission claims to AspNetRoleClaims
                using var grcScope = _serviceProvider.CreateScope();
                var grcLogger = grcScope.ServiceProvider.GetRequiredService<ILogger<GrcRoleDataSeedContributor>>();
                var grcRoleSeeder = new GrcRoleDataSeedContributor(grcScope.ServiceProvider, grcLogger);
                await grcRoleSeeder.SeedAsync();
            }
            else
            {
            }

            // Seed Predefined Users (Admin, Manager) - MUST be after RBAC system
            await UserSeeds.SeedUsersAsync(_context, _userManager, _logger);

            // Seed Platform Admin (Dooganlap@gmail.com as Owner)
            await PlatformAdminSeeds.SeedPlatformAdminAsync(_context, _userManager, _logger);

            // Create Ahmet Dogan user (Platform Admin)
            await CreateAhmetDoganUser.CreateUserAsync(_userManager, _context, _logger);

            // Seed AI Agent Team (Dr. Dogan's AI Team) - 12 registered agents with roles and permissions
            _logger.LogInformation("🤖 Seeding AI Agent Team (Dr. Dogan's Team)...");
            await AiAgentTeamSeeds.SeedAsync(_context);

            // Seed OpenIddict applications (OAuth2 clients)
            _logger.LogInformation("🔐 Seeding OpenIddict applications...");
            await SeedOpenIddictDataAsync();

            _logger.LogInformation("✅ Application initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during application initialization");
            throw;
        }
    }

    private async Task SeedOpenIddictDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var openIddictSeeder = scope.ServiceProvider.GetService<OpenIddictDataSeeder>();
            if (openIddictSeeder != null)
            {
                await openIddictSeeder.SeedAsync();
            }
            else
            {
                _logger.LogWarning("OpenIddictDataSeeder not registered. Skipping OpenIddict seeding.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to seed OpenIddict data. This may be expected if OpenIddict tables are not created yet.");
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
