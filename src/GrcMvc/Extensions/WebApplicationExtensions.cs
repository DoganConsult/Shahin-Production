using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Middleware;
using GrcMvc.Security;
using Hangfire;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace GrcMvc.Extensions;

/// <summary>
/// Extension methods for middleware pipeline configuration
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure the middleware pipeline
    /// </summary>
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        // ✅ CLOUDFLARE FIX: Forwarded headers MUST be first (before HTTPS redirect)
        app.UseForwardedHeaders();

        // Response Compression (must be early)
        app.UseResponseCompression();

        // Exception handling
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // Swagger UI (all environments)
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Shahin GRC API v1");
            options.RoutePrefix = "api-docs";
            options.DocumentTitle = "Shahin GRC API Documentation";
            options.DefaultModelsExpandDepth(-1);
        });

        // Global exception handling
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Request localization
        var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        app.UseRequestLocalization(localizationOptions);

        // Status code pages
        app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

        // Owner setup (early)
        app.UseMiddleware<OwnerSetupMiddleware>();

        // Policy violation exception handling
        app.UseMiddleware<PolicyViolationExceptionMiddleware>();

        // Tenant resolution (early for performance)
        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Security headers
        app.UseSecurityHeaders();

        // Host-based routing
        // REMOVED: app.UseHostRouting(); // Method doesn't exist

        app.UseRouting();

        // Rate limiting
        app.UseRateLimiter();

        // Session
        app.UseSession();

        // CORS
        app.UseCors("AllowApiClients");

        // Response caching
        app.UseResponseCaching();

        // Authentication & Authorization
        app.UseAuthentication();
        
        // ✅ CRITICAL SECURITY FIX: Trial Enforcement (Issue #3)
        // Blocks access for tenants with expired trials
        app.UseTrialEnforcement();

        app.UseAuthorization();

        // Onboarding redirect
        app.UseMiddleware<OnboardingRedirectMiddleware>();

        return app;
    }

    /// <summary>
    /// Configure Hangfire dashboard and recurring jobs
    /// </summary>
    public static WebApplication ConfigureHangfire(this WebApplication app, IConfiguration configuration, ILogger logger)
    {
        var enableHangfire = configuration.GetValue<bool>("Hangfire:Enabled", true);

        // #region agent log - H4: Hangfire configuration entry
        var logPath = @"c:\Shahin-ai\.cursor\debug.log";
        try
        {
            var logEntry = System.Text.Json.JsonSerializer.Serialize(new
            {
                sessionId = "debug-session",
                runId = "startup-run-1",
                hypothesisId = "H4",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                location = "WebApplicationExtensions.cs:110",
                message = "ConfigureHangfire entry",
                data = new { enabled = enableHangfire }
            });
            System.IO.File.AppendAllText(logPath, logEntry + "\n");
        }
        catch { }
        // #endregion

        if (!enableHangfire)
        {
            logger.LogWarning("⚠️ Hangfire disabled");
            return app;
        }

        // Check if Hangfire services were actually registered
        try
        {
            var hangfireService = app.Services.GetService<IGlobalConfiguration>();
            
            // #region agent log - H4: Hangfire service registration check
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H4",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "WebApplicationExtensions.cs:123",
                    message = "Hangfire service registration check",
                    data = new { serviceRegistered = hangfireService != null }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion
            
            if (hangfireService == null)
            {
                logger.LogWarning("⚠️ Hangfire services not registered, skipping dashboard configuration");
                return app;
            }

            var dashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath", "/hangfire");
            app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthFilter() },
                DashboardTitle = "Shahin GRC - Background Jobs",
                DisplayStorageConnectionString = false
            });
            logger.LogInformation("✅ Hangfire dashboard enabled at {Path}", dashboardPath);

            // Configure recurring jobs
            ConfigureRecurringJobs(logger, configuration);
        }
        catch (Exception ex)
        {
            logger.LogWarning("⚠️ Hangfire configuration skipped: {Message}", ex.Message);
        }

        return app;
    }

    /// <summary>
    /// Configure endpoint routing
    /// </summary>
    public static WebApplication ConfigureEndpoints(this WebApplication app)
    {
        // SignalR Hub
        var signalREnabled = app.Configuration.GetValue<bool>("SignalR:Enabled", true);
        if (signalREnabled)
        {
            app.MapHub<Hubs.DashboardHub>("/hubs/dashboard");
            app.Logger.LogInformation("✅ SignalR Dashboard Hub mapped");
        }

        // Platform Admin routes (must be first)
        app.MapControllerRoute(
            name: "platform-admin",
            pattern: "platform-admin/{action=Dashboard}/{id?}",
            defaults: new { controller = "PlatformAdminDashboard" });

        app.MapControllerRoute(
            name: "admin-portal",
            pattern: "admin/{action=Login}/{id?}",
            defaults: new { controller = "AdminPortal" });

        // Owner routes
        app.MapControllerRoute(
            name: "owner",
            pattern: "owner/{controller=Owner}/{action=Index}/{id?}");

        // Tenant routes
        app.MapControllerRoute(
            name: "tenant",
            pattern: "tenant/{slug}/{controller=Home}/{action=Index}/{id?}",
            constraints: new { slug = @"[a-z0-9-]+" });

        app.MapControllerRoute(
            name: "tenant-admin",
            pattern: "tenant/{slug}/admin/{controller=Dashboard}/{action=Index}/{id?}",
            constraints: new { slug = @"[a-z0-9-]+" });

        // Onboarding routes
        app.MapControllerRoute(
            name: "onboarding-wizard",
            pattern: "OnboardingWizard/{action=Index}/{tenantId?}",
            defaults: new { controller = "OnboardingWizard" });

        app.MapControllerRoute(
            name: "org-setup",
            pattern: "OrgSetup/{action=Index}/{id?}",
            defaults: new { controller = "OrgSetup" });

        app.MapControllerRoute(
            name: "onboarding",
            pattern: "Onboarding/{action=Index}/{id?}",
            defaults: new { controller = "Onboarding" });

        // Login redirect
        app.MapControllerRoute(
            name: "login-redirect",
            pattern: "login-redirect",
            defaults: new { controller = "Account", action = "LoginRedirect" });

        // Enable attribute routing
        app.MapControllers();

        // Landing page
        app.MapControllerRoute(
            name: "landing",
            pattern: "",
            defaults: new { controller = "Landing", action = "Index" });

        // Plural redirects
        app.MapGet("/Risks", () => Results.Redirect("/Risk"));
        app.MapGet("/Policies", () => Results.Redirect("/Policy"));
        app.MapGet("/Audits", () => Results.Redirect("/Audit"));
        app.MapGet("/Assessments", () => Results.Redirect("/Assessment"));

        // Default route
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        // Health checks
        ConfigureHealthChecks(app);

        return app;
    }

    /// <summary>
    /// Apply database migrations and seed data
    /// CRITICAL: Always use Migrate() not EnsureCreated() to track migration history
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        // #region agent log - H5: Migration History Tracking
        var logPath = @"c:\Shahin-ai\.cursor\debug.log";
        try
        {
            var logEntry = System.Text.Json.JsonSerializer.Serialize(new
            {
                sessionId = "debug-session",
                runId = "startup-run-1",
                hypothesisId = "H5",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                location = "WebApplicationExtensions.cs:239",
                message = "ApplyDatabaseMigrationsAsync entry",
                data = new
                {
                    environment = app.Environment.EnvironmentName,
                    isProduction = app.Environment.IsProduction()
                }
            });
            System.IO.File.AppendAllText(logPath, logEntry + "\n");
        }
        catch { }
        // #endregion

        try
        {
            var dbContext = services.GetRequiredService<GrcDbContext>();
            var authContext = services.GetRequiredService<GrcAuthDbContext>();

            // #region agent log - H5: Test database connection
            try
            {
                var canConnectMain = await dbContext.Database.CanConnectAsync();
                var canConnectAuth = await authContext.Database.CanConnectAsync();
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H5",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "WebApplicationExtensions.cs:248",
                    message = "Database connection test",
                    data = new
                    {
                        mainDbConnected = canConnectMain,
                        authDbConnected = canConnectAuth,
                        mainDbProvider = dbContext.Database.ProviderName,
                        authDbProvider = authContext.Database.ProviderName
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            // #region agent log - H5: Get Applied Migrations
            try
            {
                var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList();
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H5",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "WebApplicationExtensions.cs:250",
                    message = "Applied migrations from database",
                    data = new
                    {
                        count = appliedMigrations.Count,
                        migrations = appliedMigrations.Take(10).ToArray(),
                        hasHistory = appliedMigrations.Any()
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            // ===================================================================
            // MAIN DATABASE (GrcDbContext)
            // ===================================================================
            if (app.Environment.IsProduction())
            {
                logger.LogInformation("🔄 Applying database migrations (Production)...");
                
                // Check for pending migrations
                var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("📦 Found {Count} pending migrations for main database", pendingMigrations.Count);
                    logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
                    
                    // Apply migrations with timeout handling
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("✅ Main database migrations applied successfully");
                }
                else
                {
                    // No pending migrations - verify database exists
                    var canConnect = await dbContext.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        logger.LogInformation("✅ Main database schema is up-to-date (no pending migrations)");
                    }
                    else
                    {
                        logger.LogWarning("⚠️ Database exists but cannot connect - may need manual intervention");
                    }
                }
            }
            else
            {
                // DEVELOPMENT: Use Migrate() for consistency (not EnsureCreated)
                logger.LogInformation("🔄 Applying database migrations (Development)...");
                
                var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("📦 Found {Count} pending migrations", pendingMigrations.Count);
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("✅ Development database migrations applied");
                }
                else
                {
                    logger.LogInformation("✅ Development database is up-to-date");
                }
            }

            // ===================================================================
            // AUTH DATABASE (GrcAuthDbContext)
            // CRITICAL: ALWAYS use Migrate() - never EnsureCreated()
            // See: docs/IDENTITY_SCHEMA_SAFEGUARDS.md
            // ===================================================================
            logger.LogInformation("🔄 Applying Auth database migrations...");
            
            var authPendingMigrations = authContext.Database.GetPendingMigrations().ToList();
            if (authPendingMigrations.Any())
            {
                logger.LogInformation("📦 Found {Count} pending migrations for Auth database", authPendingMigrations.Count);
                logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", authPendingMigrations));
                
                await authContext.Database.MigrateAsync();
                logger.LogInformation("✅ Auth database migrations applied successfully");
            }
            else
            {
                // Verify ApplicationUser schema is complete
                var canConnect = await authContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    // Check if critical columns exist (quick validation)
                    try
                    {
                        var hasCustomColumns = await authContext.Database.ExecuteSqlRawAsync(
                            "SELECT 1 FROM information_schema.columns " +
                            "WHERE table_name = 'AspNetUsers' AND column_name = 'FirstName'");
                        
                        if (hasCustomColumns > 0)
                        {
                            logger.LogInformation("✅ Auth database schema verified (custom columns present)");
                        }
                        else
                        {
                            logger.LogWarning("⚠️ Auth database exists but missing custom columns - may need manual migration");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "⚠️ Could not verify Auth database schema - proceeding anyway");
                    }
                }
                else
                {
                    logger.LogWarning("⚠️ Auth database cannot connect - may need manual intervention");
                }
            }
        }
        catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "57P01" || ex.SqlState == "57P03")
        {
            // Database connection errors
            logger.LogError(ex, "❌ Database connection failed during migration. Check connection string and database availability.");
            throw;
        }
        catch (TimeoutException ex)
        {
            // Command timeout
            logger.LogError(ex, "❌ Migration timed out. Database may be locked or schema too large. Consider increasing CommandTimeout.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database migration failed: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Verify migration health after application startup
    /// </summary>
    public static async Task VerifyMigrationHealthAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        // #region agent log - H5: Verify migration health entry
        var logPath = @"c:\Shahin-ai\.cursor\debug.log";
        try
        {
            var logEntry = System.Text.Json.JsonSerializer.Serialize(new
            {
                sessionId = "debug-session",
                runId = "startup-run-1",
                hypothesisId = "H5",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                location = "WebApplicationExtensions.cs:371",
                message = "VerifyMigrationHealthAsync entry",
                data = new { verifying = true }
            });
            System.IO.File.AppendAllText(logPath, logEntry + "\n");
        }
        catch { }
        // #endregion

        try
        {
            var dbContext = services.GetRequiredService<GrcDbContext>();
            var authContext = services.GetRequiredService<GrcAuthDbContext>();

            // Check main DB
            var mainMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            logger.LogInformation("✅ Main database: {Count} migrations applied", mainMigrations.Count());

            // #region agent log - H5: Main DB migration count
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H5",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "WebApplicationExtensions.cs:383",
                    message = "Main database migrations applied",
                    data = new
                    {
                        count = mainMigrations.Count(),
                        lastFive = mainMigrations.TakeLast(5).ToArray()
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            // Check auth DB
            var authMigrations = await authContext.Database.GetAppliedMigrationsAsync();
            logger.LogInformation("✅ Auth database: {Count} migrations applied", authMigrations.Count());

            // Verify critical tables exist
            var mainTableCount = 0;
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'";
                await dbContext.Database.OpenConnectionAsync();
                var result = await command.ExecuteScalarAsync();
                mainTableCount = Convert.ToInt32(result);
                await dbContext.Database.CloseConnectionAsync();
            }
            logger.LogInformation("✅ Main database: {Count} tables exist", mainTableCount);

            // #region agent log - H6: Verify FirstAdminUserId column
            bool hasFirstAdminUserId = false;
            try
            {
                using (var command = dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'Tenants' AND column_name = 'FirstAdminUserId'";
                    await dbContext.Database.OpenConnectionAsync();
                    var result = await command.ExecuteScalarAsync();
                    hasFirstAdminUserId = Convert.ToInt32(result) > 0;
                    await dbContext.Database.CloseConnectionAsync();
                }
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H6",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "WebApplicationExtensions.cs:425",
                    message = "Verified FirstAdminUserId column existence",
                    data = new
                    {
                        hasFirstAdminUserId,
                        tableName = "Tenants",
                        columnName = "FirstAdminUserId"
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            // ABP requires all database changes through EF Core migrations
            // Do not manually alter database schema
            if (!hasFirstAdminUserId)
            {
                logger.LogWarning("⚠️ FirstAdminUserId column is missing. Please run migrations: dotnet ef database update --context GrcDbContext");
            }

            // Verify ApplicationUser custom columns
            var customColumnExists = false;
            using (var command = authContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name IN ('FirstName', 'LastName', 'Department', 'JobTitle', 'Abilities')";
                await authContext.Database.OpenConnectionAsync();
                var result = await command.ExecuteScalarAsync();
                customColumnExists = Convert.ToInt32(result) > 0;
                await authContext.Database.CloseConnectionAsync();
            }
            
            if (customColumnExists)
            {
                logger.LogInformation("✅ Auth database: ApplicationUser custom columns verified");
            }
            else
            {
                logger.LogWarning("⚠️ Auth database: ApplicationUser custom columns missing - migration may be incomplete");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Migration health check failed");
            // Don't throw - this is just verification
        }
    }

    /// <summary>
    /// Initialize seed data asynchronously
    /// </summary>
    public static async Task InitializeSeedDataAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<ApplicationInitializer>();
            logger.LogInformation("🚀 Starting application initialization (seed data)...");
            await initializer.InitializeAsync();
            logger.LogInformation("✅ Application initialization completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to initialize seed data");
            // Don't throw - allow app to continue running even if seed data fails
        }
    }

    #region Private Helper Methods

    private static void ConfigureHealthChecks(WebApplication app)
    {
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    version = "2.0.0",
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalMilliseconds
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db")
        });

        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("api")
        });
    }

    private static void ConfigureRecurringJobs(ILogger logger, IConfiguration configuration)
    {
        // Core jobs
        RecurringJob.AddOrUpdate<BackgroundJobs.NotificationDeliveryJob>(
            "notification-delivery",
            job => job.ExecuteAsync(),
            "*/5 * * * *");

        RecurringJob.AddOrUpdate<BackgroundJobs.EscalationJob>(
            "escalation-check",
            job => job.ExecuteAsync(),
            "0 * * * *");

        RecurringJob.AddOrUpdate<BackgroundJobs.SlaMonitorJob>(
            "sla-monitor",
            job => job.ExecuteAsync(),
            "*/15 * * * *");

        RecurringJob.AddOrUpdate<BackgroundJobs.WebhookRetryJob>(
            "webhook-retry",
            job => job.ExecuteAsync(),
            "*/2 * * * *");

        // Email operations
        RecurringJob.AddOrUpdate<Services.EmailOperations.EmailProcessingJob>(
            "email-sla-check",
            job => job.CheckSlaBreachesAsync(),
            "0 * * * *");

        RecurringJob.AddOrUpdate<Services.EmailOperations.EmailProcessingJob>(
            "email-polling-sync",
            job => job.SyncAllMailboxesAsync(),
            "*/5 * * * *");

        // Database backup
        var backupEnabled = configuration.GetValue("Backup:Enabled", true);
        if (backupEnabled)
        {
            var backupSchedule = configuration["Backup:CronSchedule"] ?? "0 2 * * *";
            RecurringJob.AddOrUpdate<BackgroundJobs.DatabaseBackupJob>(
                "database-backup-daily",
                job => job.BackupAllDatabasesAsync(),
                backupSchedule);
            
            logger.LogInformation("✅ Database backup job scheduled: {Schedule}", backupSchedule);
        }

        // Trial lifecycle
        RecurringJob.AddOrUpdate<BackgroundJobs.TrialNurtureJob>(
            "trial-nurture-hourly",
            job => job.ProcessNurtureEmailsAsync(),
            "0 * * * *");

        RecurringJob.AddOrUpdate<BackgroundJobs.TrialNurtureJob>(
            "trial-expiring-daily",
            job => job.CheckExpiringTrialsAsync(),
            "0 9 * * *");

        // AM-11: Access Review Reminders (BP-01 Critical Job)
        // Daily at 9 AM - check for overdue access reviews
        RecurringJob.AddOrUpdate<BackgroundJobs.AccessReviewReminderJob>(
            "access-review-reminder-daily",
            job => job.ExecuteAsync(),
            "0 9 * * *");

        // Proactive reminders for approaching due dates (run at 8 AM)
        RecurringJob.AddOrUpdate<BackgroundJobs.AccessReviewReminderJob>(
            "access-review-approaching-deadline",
            job => job.SendApproachingDueDateRemindersAsync(),
            "0 8 * * *");

        logger.LogInformation("✅ Recurring jobs configured");
    }

    #endregion
}
