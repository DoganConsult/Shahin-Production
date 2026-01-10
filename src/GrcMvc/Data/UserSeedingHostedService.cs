using GrcMvc.Data.Seeds;
using GrcMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GrcMvc.Data;

/// <summary>
/// Hosted service that seeds users on application startup
/// Runs after database is ready and other seed data is initialized
/// </summary>
public class UserSeedingHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserSeedingHostedService> _logger;

    public UserSeedingHostedService(
        IServiceProvider serviceProvider,
        ILogger<UserSeedingHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // #region agent log
        try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "UserSeedingHostedService.cs:28", message = "UserSeedingHostedService StartAsync entry", data = new { threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        _logger.LogInformation("🌱 User seeding service starting...");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GrcDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserSeedingHostedService>>();

        try
        {
            // Wait a bit for database to be ready
            await Task.Delay(2000, cancellationToken);

            // Check if database is accessible
            var canConnect = await context.Database.CanConnectAsync(cancellationToken);
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "UserSeedingHostedService.cs:44", message = "Database connection check", data = new { canConnect, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            if (!canConnect)
            {
                _logger.LogWarning("⚠️ Database not accessible. Skipping user seeding.");
                return;
            }

            // Seed RBAC system first (if not already seeded)
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "UserSeedingHostedService.cs:53", message = "UserSeedingHostedService default tenant lookup", data = new { defaultTenantId = defaultTenant?.Id.ToString(), defaultTenantName = defaultTenant?.OrganizationName, isNull = defaultTenant == null, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            if (defaultTenant != null)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "UserSeedingHostedService.cs:56", message = "Before UserSeedingHostedService RBAC seeding", data = new { tenantId = defaultTenant.Id.ToString(), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                await RbacSeeds.SeedRbacSystemAsync(context, roleManager, defaultTenant.Id, logger);
            }

            // Seed users (after RBAC system is ready)
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "UserSeedingHostedService.cs:59", message = "Before UserSeedingHostedService UserSeeds", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            await UserSeeds.SeedUsersAsync(context, userManager, logger);

            _logger.LogInformation("✅ User seeding service completed.");
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "UserSeedingHostedService.cs:62", message = "UserSeedingHostedService exit success", data = new { timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
        }
        catch (Exception ex)
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync("/home/Shahin-ai/.cursor/debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "UserSeedingHostedService.cs:65", message = "UserSeedingHostedService exception", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, (ex.StackTrace?.Length).GetValueOrDefault(0))), timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            _logger.LogError(ex, "❌ Error in user seeding service");
            // Don't throw - allow application to start even if seeding fails
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
