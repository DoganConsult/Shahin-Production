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
        _logger.LogInformation("User seeding service starting...");

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
            _logger.LogDebug("Database connection check: {CanConnect}", canConnect);
            if (!canConnect)
            {
                _logger.LogWarning("⚠️ Database not accessible. Skipping user seeding.");
                return;
            }

            // Seed RBAC system first (if not already seeded)
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.TenantSlug == "default" && !t.IsDeleted);
            _logger.LogDebug("Default tenant lookup: {TenantId}, {TenantName}", defaultTenant?.Id, defaultTenant?.OrganizationName);
            if (defaultTenant != null)
            {
                _logger.LogDebug("Seeding RBAC system for tenant {TenantId}", defaultTenant.Id);
                await RbacSeeds.SeedRbacSystemAsync(context, roleManager, defaultTenant.Id, logger);
            }

            // Seed users (after RBAC system is ready)
            _logger.LogDebug("Seeding users...");
            await UserSeeds.SeedUsersAsync(context, userManager, logger);

            _logger.LogInformation("User seeding service completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in user seeding service: {Message}", ex.Message);
            // Don't throw - allow application to start even if seeding fails
            // But log at Error level so it's visible in monitoring
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
