using GrcMvc.Abp;
using GrcMvc.Configuration;
using GrcMvc.Data;
using GrcMvc.Data.Seeds;
using GrcMvc.Extensions;
using Serilog;
using Volo.Abp;
using GrcMvc;

// =============================================================================
// Shahin GRC Platform - Startup Configuration
// Clean, maintainable, production-ready ASP.NET Core 8.0 application
// =============================================================================

// Check for test commands
if (args.Length > 0 && args[0] == "TestDb")
{
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
        .AddEnvironmentVariables();
    
    var testConfig = configBuilder.Build();
    var exitCode = await TestDbConnection.Run(testConfig);
    Environment.Exit(exitCode);
    return;
}

// Check for database explorer command
if (args.Length > 0 && args[0] == "explore-db")
{
    var exitCode = await DatabaseExplorer.ExploreDatabase(args.Skip(1).ToArray());
    Environment.Exit(exitCode);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// #region agent log - Startup Entry Point
var startupLogPath = @"c:\Shahin-ai\.cursor\debug.log";
try
{
    var logEntry = System.Text.Json.JsonSerializer.Serialize(new
    {
        sessionId = "debug-session",
        runId = "startup-run-1",
        hypothesisId = "ALL",
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        location = "Program.cs:16",
        message = "Application startup entry point",
        data = new
        {
            startTime = DateTime.UtcNow,
            args = args?.Length ?? 0
        }
    });
    System.IO.File.AppendAllText(startupLogPath, logEntry + "\n");
}
catch { }
// #endregion

try
{
    // =============================================================================
    // PHASE 1: LOGGING CONFIGURATION
    // =============================================================================
    builder.ConfigureLogging();
    Log.Information("Starting Shahin GRC Platform...");

    // =============================================================================
    // PHASE 2: CONFIGURATION & ENVIRONMENT SETUP
    // =============================================================================
    builder.LoadEnvironmentConfiguration();
    await builder.ConfigureAbpFramework();
    builder.BindConfiguration();

    // =============================================================================
    // PHASE 3: INFRASTRUCTURE CONFIGURATION
    // =============================================================================
    builder.ConfigureKestrelServer();
    
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddCompressionConfiguration();
    builder.Services.AddApplicationInsightsConfiguration(builder.Configuration, builder.Environment);
    builder.Services.AddForwardedHeadersConfiguration(); // ✅ CLOUDFLARE FIX: Trust proxy headers
    builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);
    builder.Services.AddLocalizationConfiguration();

    // =============================================================================
    // PHASE 4: DATABASE CONFIGURATION
    // =============================================================================
    builder.Services.AddDatabaseContexts(builder.Configuration);

    // =============================================================================
    // PHASE 5: AUTHENTICATION & SECURITY
    // =============================================================================
    builder.Services.AddIdentityConfiguration();
    builder.Services.AddOpenIddictAuthentication(builder.Configuration);
    builder.Services.AddAuthorizationPolicies();
    builder.Services.AddRateLimiting(builder.Configuration);
    builder.Services.AddDataProtectionConfiguration(builder.Configuration, builder.Environment);
    builder.Services.AddSecureSession(builder.Environment);
    builder.Services.AddAntiForgeryConfiguration(builder.Environment);
    builder.Services.AddSecureCookiePolicy(builder.Environment);

    // =============================================================================
    // PHASE 6: MVC & API CONFIGURATION
    // =============================================================================
    builder.Services.AddMvcConfiguration();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddValidators();
    builder.Services.AddSwaggerConfiguration();

    // =============================================================================
    // PHASE 7: INFRASTRUCTURE SERVICES
    // =============================================================================
    builder.Services.AddHangfireConfiguration(builder.Configuration, builder.Environment);
    builder.Services.AddMassTransitConfiguration(builder.Configuration);
    builder.Services.AddHealthChecksConfiguration(builder.Configuration);
    builder.Services.AddHttpResiliencePolicies();
    builder.Services.AddCachingConfiguration(builder.Configuration);
    builder.Services.AddSignalRConfiguration(builder.Configuration);

    // =============================================================================
    // PHASE 8: APPLICATION SERVICES
    // =============================================================================
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddAbpSettings();

    // =============================================================================
    // PHASE 9: HOSTED SERVICES & VALIDATORS
    // =============================================================================
    builder.Services.AddHostedService<GrcMvc.Data.UserSeedingHostedService>();
    builder.Services.AddHostedService<GrcMvc.Services.StartupValidators.OnboardingServicesStartupValidator>();
    builder.Services.AddHostedService<ConfigurationValidator>();

    // Seed services
    builder.Services.AddScoped<ApplicationInitializer>();
    // Temporarily commented out - missing implementations
    // builder.Services.AddScoped<CatalogSeederService>();
    // builder.Services.AddScoped<WorkflowDefinitionSeederService>();
    // builder.Services.AddScoped<FrameworkControlImportService>();
    // builder.Services.AddScoped<Services.Implementations.IPocSeederService, Services.Implementations.PocSeederService>();

    // =============================================================================
    // BUILD APPLICATION
    // =============================================================================
    var app = builder.Build();

    Log.Information("Application built successfully. Environment: {Environment}", app.Environment.EnvironmentName);

    // =============================================================================
    // ABP FRAMEWORK INITIALIZATION
    // =============================================================================
    await app.InitializeApplicationAsync();
    Log.Information("ABP Framework initialized");

    // =============================================================================
    // DATABASE MIGRATIONS
    // =============================================================================
    // #region agent log - H5: Before migration application
    try
    {
        var logEntry = System.Text.Json.JsonSerializer.Serialize(new
        {
            sessionId = "debug-session",
            runId = "startup-run-1",
            hypothesisId = "H5",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            location = "Program.cs:115",
            message = "About to apply database migrations",
            data = new { phase = "before_migration" }
        });
        System.IO.File.AppendAllText(startupLogPath, logEntry + "\n");
    }
    catch { }
    // #endregion
    
    await app.ApplyDatabaseMigrationsAsync();

    // #region agent log - H5: After migration application
    try
    {
        var logEntry = System.Text.Json.JsonSerializer.Serialize(new
        {
            sessionId = "debug-session",
            runId = "startup-run-1",
            hypothesisId = "H5",
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            location = "Program.cs:120",
            message = "Database migrations applied",
            data = new { phase = "after_migration", success = true }
        });
        System.IO.File.AppendAllText(startupLogPath, logEntry + "\n");
    }
    catch { }
    // #endregion

    // =============================================================================
    // VERIFY MIGRATION HEALTH
    // =============================================================================
    await app.VerifyMigrationHealthAsync();

    // =============================================================================
    // MIDDLEWARE PIPELINE
    // =============================================================================
    app.ConfigureMiddlewarePipeline();

    // =============================================================================
    // HANGFIRE CONFIGURATION
    // =============================================================================
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    app.ConfigureHangfire(app.Configuration, logger);

    // =============================================================================
    // ENDPOINT CONFIGURATION
    // =============================================================================
    app.ConfigureEndpoints();

    // =============================================================================
    // SEED DATA INITIALIZATION
    // =============================================================================
    await app.InitializeSeedDataAsync();

    // =============================================================================
    // START APPLICATION
    // =============================================================================
    Log.Information("Shahin GRC Platform starting on: {Urls}", 
        Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:8080");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// =============================================================================
// LEGACY SMTP SETTINGS CLASS (for backward compatibility)
// TODO: Move to Configuration folder
// =============================================================================
public class SmtpSettings
{
    public string Host { get; set; } = "smtp.office365.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@grcsystem.com";
    public string FromName { get; set; } = "GRC System";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseOAuth2 { get; set; } = false;
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
