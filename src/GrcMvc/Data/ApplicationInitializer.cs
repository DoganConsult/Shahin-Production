using GrcMvc.Data.Seeds;
using GrcMvc.Data.Seed;
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
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ApplicationInitializer.cs:39", message = "InitializeAsync entry", data = new { threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        try
        {
            _logger.LogInformation("🚀 Starting application initialization...");

            // Layer 1: Global Catalogs from CSV files (92 regulators, 163 frameworks, 13,528 controls)
            await SeedCatalogsFromCsvAsync();

            // Seed Subscription Plans (MVP, Professional, Enterprise)
            _logger.LogInformation("📋 Seeding subscription plans...");
            await SubscriptionPlanSeeds.SeedAsync(_context);

            // Seed Role Profiles (STAGE 2 - KSA & Multi-level Approval)
            await RoleProfileSeeds.SeedRoleProfilesAsync(_context, _logger);

            // Seed Workflow Definitions (STAGE 2)
            await WorkflowDefinitionSeeds.SeedWorkflowDefinitionsAsync(_context, _logger);

            // Seed Comprehensive Derivation Rules (50+ rules for KSA GRC)
            await DerivationRulesSeeds.SeedAsync(_context, _logger);

            // Seed RBAC System (Permissions, Features, Roles, Mappings) - MUST be before user seeding
            var defaultTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "ApplicationInitializer.cs:63", message = "Default tenant lookup result", data = new { defaultTenantId = defaultTenant?.Id.ToString(), defaultTenantName = defaultTenant?.OrganizationName, isNull = defaultTenant == null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            if (defaultTenant != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ApplicationInitializer.cs:67", message = "Before RBAC seeding", data = new { tenantId = defaultTenant.Id.ToString(), roleManagerExists = roleManager != null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                await RbacSeeds.SeedRbacSystemAsync(_context, roleManager, defaultTenant.Id, _logger);

                // Seed GRC Policy Enforcement Roles (8 baseline roles)
                // Uses RoleManager API to add permission claims to AspNetRoleClaims
                using var grcScope = _serviceProvider.CreateScope();
                var grcLogger = grcScope.ServiceProvider.GetRequiredService<ILogger<GrcRoleDataSeedContributor>>();
                var grcRoleSeeder = new GrcRoleDataSeedContributor(roleManager, grcLogger);
                await grcRoleSeeder.SeedAsync();
            }
            else
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "ApplicationInitializer.cs:75", message = "Default tenant is null - skipping RBAC", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
            }

            // Seed Predefined Users (Admin, Manager) - MUST be after RBAC system
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ApplicationInitializer.cs:78", message = "Before UserSeeds.SeedUsersAsync", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            await UserSeeds.SeedUsersAsync(_context, _userManager, _logger);

            // Seed Platform Admin (Dooganlap@gmail.com as Owner)
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ApplicationInitializer.cs:81", message = "Before PlatformAdminSeeds", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            await PlatformAdminSeeds.SeedPlatformAdminAsync(_context, _userManager, _logger);

            // Create Ahmet Dogan user (Platform Admin)
            await CreateAhmetDoganUser.CreateUserAsync(_userManager, _context, _logger);

            // Seed AI Agent Team (Dr. Dogan's AI Team) - 12 registered agents with roles and permissions
            _logger.LogInformation("🤖 Seeding AI Agent Team (Dr. Dogan's Team)...");
            await AiAgentTeamSeeds.SeedAsync(_context);

            _logger.LogInformation("✅ Application initialization completed successfully");
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ApplicationInitializer.cs:90", message = "InitializeAsync exit success", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
        }
        catch (Exception ex)
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "ApplicationInitializer.cs:94", message = "InitializeAsync exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, (ex.StackTrace?.Length).GetValueOrDefault(0))), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
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
