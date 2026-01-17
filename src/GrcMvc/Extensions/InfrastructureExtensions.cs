using FluentValidation;
using FluentValidation.AspNetCore;
using GrcMvc.Configuration;
using GrcMvc.Filters;
using GrcMvc.Resources;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using Polly;
using Polly.Extensions.Http;

namespace GrcMvc.Extensions;

/// <summary>
/// Extension methods for infrastructure service configuration (Hangfire, MassTransit, Health Checks, etc.)
/// </summary>
public static class InfrastructureExtensions
{
    /// <summary>
    /// Configure Hangfire background job processing
    /// </summary>
    public static IServiceCollection AddHangfireConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var enableHangfire = configuration.GetValue<bool>("Hangfire:Enabled", true);
        
        if (!enableHangfire)
        {
            Console.WriteLine("⚠️ Hangfire disabled via configuration");
            return services;
        }

        // Prefer HangfireConnection, fallback to DefaultConnection
        var connectionString = configuration.GetConnectionString("HangfireConnection")
                            ?? configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("⚠️ Hangfire disabled: No connection string");
            return services;
        }

        // #region agent log - H4: Hangfire configuration attempt
        var logPath = @"c:\Shahin-ai\.cursor\debug.log";
        try
        {
            var logEntry = System.Text.Json.JsonSerializer.Serialize(new
            {
                sessionId = "debug-session",
                runId = "startup-run-1",
                hypothesisId = "H4",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                location = "InfrastructureExtensions.cs:48",
                message = "Hangfire configuration starting",
                data = new
                {
                    hasConnectionString = !string.IsNullOrEmpty(connectionString),
                    connectionStringLength = connectionString?.Length ?? 0
                }
            });
            System.IO.File.AppendAllText(logPath, logEntry + "\n");
        }
        catch { }
        // #endregion

        try
        {
            // Test database connection with timeout
            using var testConnection = new NpgsqlConnection(connectionString);
            testConnection.Open();
            testConnection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Hangfire DB test failed (will retry on demand): {ex.Message}");
            // #region agent log - H4: Database connection test failed
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H4",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "InfrastructureExtensions.cs:52",
                    message = "Hangfire database connection test failed",
                    data = new
                    {
                        error = ex.Message,
                        continueAnyway = true
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion
            // Continue anyway - Hangfire can retry connection
        }

        try
        {
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UsePostgreSqlStorage(options =>
                      {
                          options.UseNpgsqlConnection(connectionString);
                      });
            });

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = configuration.GetValue<int>("Hangfire:WorkerCount", 
                    Environment.ProcessorCount * 2);
                options.Queues = new[] { "critical", "default", "low" };
            });

            // #region agent log - H4: Hangfire server registered successfully
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H4-FIX",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "InfrastructureExtensions.cs:66",
                    message = "Hangfire server registered successfully",
                    data = new { registered = true }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion

            Console.WriteLine("✅ Hangfire configured successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Hangfire disabled: Configuration failed - {ex.Message}");
            // #region agent log - H4: Hangfire configuration failed
            try
            {
                var logEntry = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "debug-session",
                    runId = "startup-run-1",
                    hypothesisId = "H4",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "InfrastructureExtensions.cs:75",
                    message = "Hangfire configuration failed completely",
                    data = new
                    {
                        error = ex.Message,
                        innerError = ex.InnerException?.Message
                    }
                });
                System.IO.File.AppendAllText(logPath, logEntry + "\n");
            }
            catch { }
            // #endregion
        }

        return services;
    }

    /// <summary>
    /// Configure MassTransit message queue
    /// </summary>
    public static IServiceCollection AddMassTransitConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() 
            ?? new RabbitMqSettings();

        if (rabbitMqSettings.Enabled)
        {
            services.AddMassTransit(x =>
            {
                // Register consumers
                x.AddConsumer<Messaging.Consumers.NotificationConsumer>();
                x.AddConsumer<Messaging.Consumers.WebhookConsumer>();
                x.AddConsumer<Messaging.Consumers.GrcEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
                    {
                        h.Username(rabbitMqSettings.Username);
                        h.Password(rabbitMqSettings.Password);
                    });

                    cfg.PrefetchCount = rabbitMqSettings.PrefetchCount;

                    cfg.UseMessageRetry(r =>
                    {
                        r.Intervals(rabbitMqSettings.RetryIntervals
                            .Select(i => TimeSpan.FromSeconds(i)).ToArray());
                    });

                    // Configure endpoints
                    cfg.ReceiveEndpoint("grc-notifications", e =>
                    {
                        e.ConfigureConsumer<Messaging.Consumers.NotificationConsumer>(context);
                        e.ConcurrentMessageLimit = rabbitMqSettings.ConcurrencyLimit;
                    });

                    cfg.ReceiveEndpoint("grc-webhooks", e =>
                    {
                        e.ConfigureConsumer<Messaging.Consumers.WebhookConsumer>(context);
                        e.ConcurrentMessageLimit = rabbitMqSettings.ConcurrencyLimit;
                    });

                    cfg.ReceiveEndpoint("grc-events", e =>
                    {
                        e.ConfigureConsumer<Messaging.Consumers.GrcEventConsumer>(context);
                        e.ConcurrentMessageLimit = rabbitMqSettings.ConcurrencyLimit;
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            Console.WriteLine("✅ MassTransit configured with RabbitMQ");
        }
        else
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<Messaging.Consumers.NotificationConsumer>();
                x.AddConsumer<Messaging.Consumers.WebhookConsumer>();
                x.AddConsumer<Messaging.Consumers.GrcEventConsumer>();

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            Console.WriteLine("⚠️ MassTransit using in-memory transport");
        }

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));

        return services;
    }

    /// <summary>
    /// Configure health checks
    /// </summary>
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "master-database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "postgresql", "master", "critical" },
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck<HealthChecks.TenantDatabaseHealthCheck>(
                name: "tenant-database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "db", "postgresql", "tenant", "critical" },
                timeout: TimeSpan.FromSeconds(5))
            .AddHangfire(
                options => { options.MinimumAvailableServers = 1; },
                name: "hangfire",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "jobs", "hangfire" },
                timeout: TimeSpan.FromSeconds(3))
            .AddCheck<HealthChecks.OnboardingCoverageHealthCheck>(
                name: "onboarding-coverage",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "onboarding", "coverage", "manifest" },
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck<HealthChecks.FieldRegistryHealthCheck>(
                name: "field-registry",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "onboarding", "field-registry", "validation" },
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"),
                tags: new[] { "api", "self" });

        // Redis health check (if enabled)
        var redisEnabled = configuration.GetValue<bool>("Redis:Enabled", false);
        if (redisEnabled)
        {
            var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString") ?? "grc-redis:6379";
            services.AddHealthChecks()
                .AddRedis(
                    redisConnectionString,
                    name: "redis-cache",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "cache", "redis" },
                    timeout: TimeSpan.FromSeconds(3));
        }

        Console.WriteLine("[HEALTH] Health checks configured");

        return services;
    }

    /// <summary>
    /// Configure HTTP resilience policies
    /// </summary>
    public static IServiceCollection AddHttpResiliencePolicies(this IServiceCollection services)
    {
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient("ExternalServices")
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

        services.AddHttpClient("EmailService")
            .AddPolicyHandler(retryPolicy);

        return services;
    }

    /// <summary>
    /// Configure caching (Memory and optionally Redis)
    /// </summary>
    public static IServiceCollection AddCachingConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddResponseCaching();

        var redisEnabled = configuration.GetValue<bool>("Redis:Enabled", false);
        if (redisEnabled)
        {
            var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString") ?? "grc-redis:6379";
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "GrcCache_";
            });

            Console.WriteLine($"✅ Redis caching enabled: {redisConnectionString}");
        }
        else
        {
            Console.WriteLine("ℹ️ Redis disabled - using IMemoryCache fallback");
        }

        return services;
    }

    /// <summary>
    /// Configure forwarded headers for reverse proxy support (Cloudflare, nginx, etc.)
    /// </summary>
    public static IServiceCollection AddForwardedHeadersConfiguration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = 
                ForwardedHeaders.XForwardedFor | 
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;
            
            // Trust all proxies (Cloudflare, load balancers, etc.)
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            
            // Required for Cloudflare and other CDNs
            options.ForwardLimit = null;
        });

        Console.WriteLine("✅ Forwarded headers configured for reverse proxy support");
        return services;
    }

    /// <summary>
    /// Configure CORS with proper security settings
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowApiClients", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")?.Get<string[]>();

                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-CSRF-Token")
                        .AllowCredentials();
                }
                else if (environment.IsDevelopment())
                {
                    // Development only - allow localhost
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5137")
                        .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                        .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-CSRF-Token")
                        .AllowCredentials();
                }
                else
                {
                    // Production - CORS must be explicitly configured
                    throw new InvalidOperationException(
                        "CORS AllowedOrigins must be configured in Production. Set Cors:AllowedOrigins in appsettings.json");
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configure SignalR with optional Redis backplane
    /// </summary>
    public static IServiceCollection AddSignalRConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var signalREnabled = configuration.GetValue<bool>("SignalR:Enabled", true);
        
        if (!signalREnabled)
        {
            return services;
        }

        var signalRBuilder = services.AddSignalR(options =>
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            options.EnableDetailedErrors = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
            options.KeepAliveInterval = TimeSpan.FromSeconds(
                configuration.GetValue<int>("SignalR:KeepAliveIntervalSeconds", 15));
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(
                configuration.GetValue<int>("SignalR:ClientTimeoutSeconds", 30));
            options.MaximumReceiveMessageSize =
                configuration.GetValue<int>("SignalR:MaximumReceiveMessageSize", 32768);
        });

        // Redis backplane (if enabled)
        var useRedisBackplane = configuration.GetValue<bool>("SignalR:UseRedisBackplane", false);
        var redisEnabled = configuration.GetValue<bool>("Redis:Enabled", false);
        
        if (useRedisBackplane && redisEnabled)
        {
            // Note: Requires Microsoft.AspNetCore.SignalR.StackExchangeRedis package
            // Currently disabled - uncomment when package is added
            Console.WriteLine("⚠️ SignalR Redis backplane disabled - package not installed");
        }

        return services;
    }

    /// <summary>
    /// Configure Swagger/OpenAPI documentation
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Shahin GRC API",
                Version = "v1",
                Description = "Governance, Risk, and Compliance Platform API",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Shahin AI Support",
                    Email = "support@shahin-ai.com",
                    Url = new Uri("https://shahin-ai.com")
                }
            });

            // JWT Bearer authentication
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configure MVC with validation and localization
    /// </summary>
    public static IServiceCollection AddMvcConfiguration(this IServiceCollection services)
    {
        services.AddControllersWithViews(options =>
        {
            // Global anti-forgery token validation
            options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());

            // Duplicate property binding detection
            options.Filters.Add<DuplicatePropertyBindingFilter>();

            // CSP nonce filter for XSS protection
            options.Filters.Add<CspNonceFilter>();
            
            // Request size limits (DoS prevention)
            options.MaxModelBindingCollectionSize = 1000;
        })
        .AddRazorRuntimeCompilation()
        .AddViewLocalization()
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
                factory.Create(typeof(SharedResource));
        });

        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // Register API Exception Filter
        services.AddScoped<ApiExceptionFilterAttribute>();

        services.AddRazorPages();

        return services;
    }

    /// <summary>
    /// Configure response compression
    /// </summary>
    public static IServiceCollection AddCompressionConfiguration(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/json", "text/html", "text/css", "application/javascript", "image/svg+xml" });
        });

        services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
        });

        return services;
    }

    /// <summary>
    /// Configure Application Insights telemetry
    /// </summary>
    public static IServiceCollection AddApplicationInsightsConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var appInsightsConnectionString = configuration["ApplicationInsights:ConnectionString"];
        
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsConnectionString;
            });
            Console.WriteLine("[APM] Application Insights telemetry enabled");
        }
        else
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.EnableAdaptiveSampling = false;
                options.DeveloperMode = environment.IsDevelopment();
            });
            Console.WriteLine("[APM] Application Insights in development mode");
        }

        return services;
    }

    /// <summary>
    /// Configure Kestrel web server
    /// </summary>
    public static WebApplicationBuilder ConfigureKestrelServer(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.AddServerHeader = false;

            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
            {
                var certPath = builder.Configuration["Certificates:Path"];
                var certPassword = builder.Configuration["Certificates:Password"];

                if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
                {
                    httpsOptions.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        certPath, certPassword);
                }
            });

            // Request size limits (DoS prevention)
            serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
            serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            serverOptions.Limits.MaxConcurrentConnections = 100;
            serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
        });

        return builder;
    }

    /// <summary>
    /// Register validators
    /// </summary>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Validators.CreateRiskDtoValidator>();
        
        return services;
    }
}
