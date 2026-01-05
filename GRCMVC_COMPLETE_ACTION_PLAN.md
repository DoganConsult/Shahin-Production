# GrcMvc Complete Action Plan - Full Feature Implementation

**Generated**: 2026-01-04
**Application**: http://localhost:5137 (GrcMvc)
**Status**: Production Hardening & Feature Completion
**Timeline**: 2-4 weeks for complete implementation

---

## Table of Contents

1. [Critical Security Fixes (Week 1)](#phase-1-critical-security-fixes-week-1)
2. [Production Hardening (Week 2)](#phase-2-production-hardening-week-2)
3. [Advanced Features (Week 3)](#phase-3-advanced-features-week-3)
4. [Monitoring & Operations (Week 4)](#phase-4-monitoring--operations-week-4)
5. [Execution Checklist](#execution-checklist)

---

## Phase 1: Critical Security Fixes (Week 1)

### Task 1.1: Enable HTTPS/TLS with Certificates ⚠️ CRITICAL
**Priority**: P0
**Effort**: 4 hours
**Files to modify**:
- `src/GrcMvc/Program.cs`
- `docker-compose.grcmvc.yml`
- Create: `src/GrcMvc/certificates/`

**Action Steps**:

```bash
# 1. Generate self-signed certificate for development/staging
cd src/GrcMvc
mkdir -p certificates

# Generate certificate
dotnet dev-certs https -ep certificates/aspnetapp.pfx -p "YourSecurePassword123!"
dotnet dev-certs https --trust

# For production, use Let's Encrypt or purchased certificate
```

**Code Changes**:

**File**: `src/GrcMvc/Program.cs` (after line 21)
```csharp
// Configure Kestrel for HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        var certPath = builder.Configuration["Certificates:Path"];
        var certPassword = builder.Configuration["Certificates:Password"];

        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
        }
    });

    // Request size limits
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});
```

**File**: `src/GrcMvc/Program.cs` (line 173 - uncomment and modify)
```csharp
// Enable HTTPS redirection
app.UseHttpsRedirection();
```

**File**: `docker-compose.grcmvc.yml`
```yaml
services:
  grcmvc:
    build:
      context: .
      dockerfile: src/GrcMvc/Dockerfile
    container_name: grcmvc-app
    ports:
      - "5137:80"
      - "5138:443"  # Add HTTPS port
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
    volumes:
      - ./src/GrcMvc/certificates:/https:ro  # Mount certificates
      - dataprotection_keys:/home/appuser/.aspnet/DataProtection-Keys
      - app_logs:/app/logs
    env_file:
      - .env.grcmvc.production
    depends_on:
      - db
    networks:
      - grc-network
    restart: unless-stopped

volumes:
  grcmvc_db_data:
  dataprotection_keys:  # Add this
  app_logs:  # Add this
```

**File**: `.env.grcmvc.production` (add)
```bash
# Certificate Configuration
CERT_PASSWORD=YourSecurePassword123!
```

**Verification**:
```bash
curl -k https://localhost:5138
```

---

### Task 1.2: Implement Secrets Management ⚠️ CRITICAL
**Priority**: P0
**Effort**: 6 hours

**Action Steps**:

```bash
# Install Docker secrets support (if using Docker Swarm)
# OR use Azure Key Vault for production

# For Docker Compose with secrets (Docker Swarm mode):
docker swarm init

# Create secrets
echo "YourStrongDbPassword123!" | docker secret create db_password -
echo "YourJWTSecretKey_MustBe32CharsOrMore!" | docker secret create jwt_secret -
echo "YourSecurePassword123!" | docker secret create cert_password -
```

**Alternative: Use .env with stricter permissions**:
```bash
# Create secure .env file
touch .env.grcmvc.secrets
chmod 600 .env.grcmvc.secrets

cat > .env.grcmvc.secrets << 'EOF'
# Secrets - DO NOT COMMIT
DB_PASSWORD=GenerateStrong32CharacterPassword!
JWT_SECRET=GenerateSecure64CharacterJWTSecret!
ADMIN_INITIAL_PASSWORD=SecureAdmin@Pass123!
CERT_PASSWORD=SecureCert@Pass123!
EOF

# Add to .gitignore
echo ".env.grcmvc.secrets" >> .gitignore
```

**Code Changes**:

**File**: `src/GrcMvc/Program.cs` (replace lines 192-241)
```csharp
// Seed initial data with secure password from environment
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GrcDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        // Apply migrations
        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = { "Admin", "ComplianceOfficer", "RiskManager", "Auditor", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user with password from environment
        var adminEmail = configuration["AdminUser:Email"] ?? "Info@doganconsult.com";
        var adminPassword = configuration["AdminUser:Password"];

        if (string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException(
                "Admin password not configured. Set AdminUser__Password environment variable.");
        }

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                Department = "Management",
                JobTitle = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        throw; // Re-throw to prevent app startup with invalid state
    }
}
```

**File**: `.env.grcmvc.production`
```bash
# Database Configuration
CONNECTION_STRING=Host=db;Database=GrcMvcDb;Username=grc_user;Password=${DB_PASSWORD};Port=5432;SSL Mode=Prefer
DB_PASSWORD=GenerateStrong32CharacterPassword!

# JWT Configuration
JWT_SECRET=GenerateSecure64CharacterJWTSecret_MustBe32CharsMinimum!
JWT_ISSUER=https://portal.shahin-ai.com
JWT_AUDIENCE=https://portal.shahin-ai.com

# Admin User
AdminUser__Email=Info@doganconsult.com
AdminUser__Password=SecureAdmin@Pass123!

# Certificate
CERT_PASSWORD=SecureCert@Pass123!

# Host Configuration
ALLOWED_HOSTS=portal.shahin-ai.com;157.180.105.48

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

---

### Task 1.3: Persist Data Protection Keys ⚠️ CRITICAL
**Priority**: P0
**Effort**: 2 hours

**Already included in Task 1.1 docker-compose changes**:
```yaml
volumes:
  - dataprotection_keys:/home/appuser/.aspnet/DataProtection-Keys
```

**Additional Code**:

**File**: `src/GrcMvc/Program.cs` (after line 128)
```csharp
// Configure Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/home/appuser/.aspnet/DataProtection-Keys"))
    .SetApplicationName("GrcMvc")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
```

---

### Task 1.4: Change Default Database Credentials ⚠️ CRITICAL
**Priority**: P0
**Effort**: 1 hour

**File**: `docker-compose.grcmvc.yml`
```yaml
  db:
    image: postgres:15-alpine
    container_name: grcmvc-db
    environment:
      - POSTGRES_USER=grc_user  # Changed from postgres
      - POSTGRES_PASSWORD=${DB_PASSWORD}  # From secrets
      - POSTGRES_DB=GrcMvcDb
    ports:
      - "5434:5432"  # REMOVE THIS LINE for security
    volumes:
      - grcmvc_db_data:/var/lib/postgresql/data
    networks:
      - grc-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U grc_user -d GrcMvcDb"]
      interval: 10s
      timeout: 5s
      retries: 5
```

---

### Task 1.5: Add Security Headers Middleware ⚠️ CRITICAL
**Priority**: P0
**Effort**: 3 hours

**Create New File**: `src/GrcMvc/Middleware/SecurityHeadersMiddleware.cs`
```csharp
namespace GrcMvc.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server header
        context.Response.Headers.Remove("Server");

        // Add security headers
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        // Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
            "font-src 'self' https://cdn.jsdelivr.net; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");

        // Strict Transport Security (HSTS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
```

**File**: `src/GrcMvc/Program.cs` (after line 175)
```csharp
app.UseSecurityHeaders(); // Add before UseRouting
app.UseRouting();
```

**File**: `src/GrcMvc/Program.cs` (line 170 - uncomment)
```csharp
app.UseHsts(); // Uncomment this
```

---

## Phase 2: Production Hardening (Week 2)

### Task 2.1: Add Health Check Endpoints
**Priority**: P1
**Effort**: 4 hours

**File**: `src/GrcMvc/Program.cs` (after line 32)
```csharp
// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString!,
        name: "database",
        tags: new[] { "db", "postgresql" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
        tags: new[] { "api" });

// Add health checks UI (optional)
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();
```

**File**: `src/GrcMvc/Program.cs` (before app.Run())
```csharp
// Map health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

**File**: `src/GrcMvc/GrcMvc.csproj` (add package)
```xml
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.2" />
<PackageReference Include="AspNetCore.HealthChecks.UI" Version="8.0.2" />
<PackageReference Include="AspNetCore.HealthChecks.UI.InMemory.Storage" Version="8.0.2" />
```

**Verification**:
```bash
curl http://localhost:5137/health
curl http://localhost:5137/health/ready
curl http://localhost:5137/health/live
```

---

### Task 2.2: Enable Forwarded Headers for Proxy Support
**Priority**: P1
**Effort**: 2 hours

**File**: `src/GrcMvc/Program.cs` (lines 161-164 - uncomment and enhance)
```csharp
// Configure Forwarded Headers for Proxy Support (Nginx, Cloudflare, etc.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                      ForwardedHeaders.XForwardedProto |
                      ForwardedHeaders.XForwardedHost,
    KnownNetworks = { }, // Clear to allow all networks
    KnownProxies = { }   // Clear to allow all proxies
});
```

---

### Task 2.3: Implement Rate Limiting
**Priority**: P1
**Effort**: 5 hours

**File**: `src/GrcMvc/Program.cs` (after line 32)
```csharp
// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limit
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // API endpoints - stricter limits
    options.AddFixedWindowLimiter("api", options =>
    {
        options.PermitLimit = 30;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });

    // Login endpoint - prevent brute force
    options.AddFixedWindowLimiter("auth", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(5);
        options.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken: token);
    };
});
```

**File**: `src/GrcMvc/Program.cs` (after line 176)
```csharp
app.UseRateLimiter(); // Add after UseRouting, before UseAuthentication
```

**File**: `src/GrcMvc/Controllers/AccountController.cs` (add attribute to login methods)
```csharp
[EnableRateLimiting("auth")]
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
{
    // existing code
}
```

**File**: `src/GrcMvc/GrcMvc.csproj` (add package)
```xml
<PackageReference Include="System.Threading.RateLimiting" Version="8.0.0" />
```

---

### Task 2.4: Implement Comprehensive Logging
**Priority**: P1
**Effort**: 6 hours

**File**: `src/GrcMvc/Program.cs` (after line 21)
```csharp
// Configure Serilog for structured logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "GrcMvc")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "/app/logs/grcmvc-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        shared: true)
    .WriteTo.File(
        path: "/app/logs/grcmvc-errors-.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        retainedFileCountLimit: 60)
);
```

**File**: `src/GrcMvc/appsettings.json`
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/grcmvc-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

**File**: `src/GrcMvc/GrcMvc.csproj` (add packages)
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

**Create Logging Middleware**: `src/GrcMvc/Middleware/RequestLoggingMiddleware.cs`
```csharp
namespace GrcMvc.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "HTTP {Method} {Path} failed after {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
```

---

### Task 2.5: Add Request Validation & Anti-Forgery
**Priority**: P1
**Effort**: 3 hours

**File**: `src/GrcMvc/Program.cs` (after line 24)
```csharp
// Add anti-forgery token validation
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Configure MVC with stricter validation
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelValidatorProviders.Clear();
});
```

---

## Phase 3: Advanced Features (Week 3)

### Task 3.1: Implement Audit Logging System
**Priority**: P2
**Effort**: 8 hours

**Create**: `src/GrcMvc/Services/Interfaces/IAuditLogger.cs`
```csharp
namespace GrcMvc.Services.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string action, string entityType, string entityId,
                  string userId, string userName, object? oldValues = null,
                  object? newValues = null);
    Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType = null,
                                             string? userId = null,
                                             DateTime? from = null,
                                             DateTime? to = null);
}

public class AuditLog
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```

**Create**: `src/GrcMvc/Services/Implementations/DatabaseAuditLogger.cs`
```csharp
namespace GrcMvc.Services.Implementations;

public class DatabaseAuditLogger : IAuditLogger
{
    private readonly GrcDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DatabaseAuditLogger> _logger;

    public DatabaseAuditLogger(GrcDbContext context,
                              IHttpContextAccessor httpContextAccessor,
                              ILogger<DatabaseAuditLogger> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, string entityId,
                              string userId, string userName,
                              object? oldValues = null, object? newValues = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                UserName = userName,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry for {Action} on {EntityType}:{EntityId}",
                action, entityType, entityId);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? entityType = null,
                                                          string? userId = null,
                                                          DateTime? from = null,
                                                          DateTime? to = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }
}
```

**Add to DbContext**: `src/GrcMvc/Data/GrcDbContext.cs`
```csharp
public DbSet<AuditLog> AuditLogs { get; set; } = null!;
```

**Register Service**: `src/GrcMvc/Program.cs`
```csharp
builder.Services.AddScoped<IAuditLogger, DatabaseAuditLogger>();
```

---

### Task 3.2: Add Distributed Caching with Redis
**Priority**: P2
**Effort**: 4 hours

**File**: `src/GrcMvc/Program.cs` (after line 128)
```csharp
// Add distributed caching
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "GrcMvc_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache(); // Fallback to in-memory
}
```

**File**: `docker-compose.grcmvc.yml`
```yaml
  redis:
    image: redis:7-alpine
    container_name: grcmvc-redis
    command: redis-server --requirepass ${REDIS_PASSWORD}
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - grc-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "redis-cli", "--raw", "incr", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  grcmvc_db_data:
  dataprotection_keys:
  app_logs:
  redis_data:  # Add this
```

**File**: `.env.grcmvc.production`
```bash
# Redis Configuration
REDIS_PASSWORD=SecureRedisPassword123!
ConnectionStrings__Redis=grcmvc-redis:6379,password=${REDIS_PASSWORD},ssl=false,abortConnect=false
```

**File**: `src/GrcMvc/GrcMvc.csproj`
```xml
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
```

---

### Task 3.3: Implement Email Notifications Service
**Priority**: P2
**Effort**: 6 hours

**Already partially implemented**. Enhance:

**File**: `src/GrcMvc/Services/Implementations/SmtpEmailSender.cs`
```csharp
// Add retry logic with Polly
public class SmtpEmailSender : IAppEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IAsyncPolicy<bool> _retryPolicy;

    public SmtpEmailSender(IOptions<EmailSettings> emailSettings,
                          ILogger<SmtpEmailSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;

        // Configure retry policy
        _retryPolicy = Policy<bool>
            .Handle<SmtpException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Email send attempt {RetryCount} failed. Retrying in {Delay}ms",
                        retryCount, timespan.TotalMilliseconds);
                });
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = isHtml ? body : null,
                    TextBody = !isHtml ? body : null
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                throw;
            }
        });
    }
}
```

**File**: `src/GrcMvc/GrcMvc.csproj`
```xml
<PackageReference Include="Polly" Version="8.2.0" />
```

---

### Task 3.4: Add Background Job Processing (Hangfire)
**Priority**: P2
**Effort**: 8 hours

**File**: `src/GrcMvc/Program.cs` (after line 56)
```csharp
// Add Hangfire for background jobs
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseNpgsqlStorage(c =>
        c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.ServerName = "GrcMvc-BackgroundWorker";
});
```

**File**: `src/GrcMvc/Program.cs` (before app.Run())
```csharp
// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Schedule recurring jobs
RecurringJob.AddOrUpdate<INotificationService>(
    "send-compliance-reminders",
    service => service.SendComplianceRemindersAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<IRiskService>(
    "calculate-risk-scores",
    service => service.RecalculateAllRiskScoresAsync(),
    Cron.Weekly);
```

**Create**: `src/GrcMvc/Infrastructure/HangfireAuthorizationFilter.cs`
```csharp
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.IsInRole("Admin");
    }
}
```

**File**: `src/GrcMvc/GrcMvc.csproj`
```xml
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.6" />
<PackageReference Include="Hangfire.PostgreSql" Version="1.20.8" />
```

---

### Task 3.5: Implement Two-Factor Authentication (2FA)
**Priority**: P2
**Effort**: 10 hours

**File**: `src/GrcMvc/Program.cs` (modify Identity configuration around line 59)
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12; // Increased from 8
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Increased
    options.Lockout.MaxFailedAccessAttempts = 3; // Reduced from 5
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedAccount = true;

    // Two-factor authentication
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
})
.AddEntityFrameworkStores<GrcDbContext>()
.AddDefaultTokenProviders()
.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("Default");
```

**Create Controller**: `src/GrcMvc/Controllers/TwoFactorController.cs`
```csharp
[Authorize]
public class TwoFactorController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TwoFactorController> _logger;

    public TwoFactorController(UserManager<ApplicationUser> userManager,
                              ILogger<TwoFactorController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Enable()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return RedirectToAction(nameof(Disable));
        }

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var model = new EnableTwoFactorViewModel
        {
            SharedKey = FormatKey(key!),
            AuthenticatorUri = GenerateQrCodeUri(user.Email!, key!)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(EnableTwoFactorViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("Code", "Verification code is invalid.");
            return View(model);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        _logger.LogInformation("User {Email} has enabled 2FA", user.Email);

        TempData["StatusMessage"] = "Two-factor authentication has been enabled.";
        return RedirectToAction("Index", "Account");
    }

    private string FormatKey(string key)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < key.Length)
        {
            result.Append(key.Substring(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < key.Length)
        {
            result.Append(key.Substring(currentPosition));
        }
        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string key)
    {
        return string.Format(
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            Uri.EscapeDataString("GRC System"),
            Uri.EscapeDataString(email),
            key);
    }
}
```

**File**: `src/GrcMvc/GrcMvc.csproj`
```xml
<PackageReference Include="QRCoder" Version="1.4.3" />
```

---

## Phase 4: Monitoring & Operations (Week 4)

### Task 4.1: Add Application Performance Monitoring (APM)
**Priority**: P2
**Effort**: 6 hours

**File**: `src/GrcMvc/Program.cs`
```csharp
// Add Application Insights
var appInsightsKey = builder.Configuration["ApplicationInsights:InstrumentationKey"];
if (!string.IsNullOrEmpty(appInsightsKey))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.InstrumentationKey = appInsightsKey;
        options.EnableAdaptiveSampling = true;
        options.EnableDependencyTrackingTelemetryModule = true;
        options.EnablePerformanceCounterCollectionModule = true;
    });
}
```

**File**: `src/GrcMvc/GrcMvc.csproj`
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
```

---

### Task 4.2: Implement Prometheus Metrics Export
**Priority**: P2
**Effort**: 4 hours

**File**: `src/GrcMvc/Program.cs`
```csharp
// Add Prometheus metrics
builder.Services.AddSingleton<IMetricServer>(new KestrelMetricServer(port: 9090));
```

**File**: `src/GrcMvc/Program.cs` (before app.Run())
```csharp
// Expose Prometheus metrics
app.UseHttpMetrics();
app.MapMetrics("/metrics");
```

**File**: `src/GrcMvc/GrcMvc.csproj`
```xml
<PackageReference Include="prometheus-net.AspNetCore" Version="8.2.1" />
```

---

### Task 4.3: Add Grafana Dashboard Configuration
**Priority**: P2
**Effort**: 8 hours

**Create**: `monitoring/docker-compose.monitoring.yml`
```yaml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    container_name: grcmvc-prometheus
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
    ports:
      - "9091:9090"
    networks:
      - grc-network
    restart: unless-stopped

  grafana:
    image: grafana/grafana:latest
    container_name: grcmvc-grafana
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
      - GF_INSTALL_PLUGINS=redis-datasource
    volumes:
      - grafana_data:/var/lib/grafana
      - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./grafana/datasources:/etc/grafana/provisioning/datasources
    ports:
      - "3000:3000"
    networks:
      - grc-network
    restart: unless-stopped
    depends_on:
      - prometheus

volumes:
  prometheus_data:
  grafana_data:

networks:
  grc-network:
    external: true
```

**Create**: `monitoring/prometheus.yml`
```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'grcmvc'
    static_configs:
      - targets: ['grcmvc-app:9090']
    metrics_path: '/metrics'

  - job_name: 'postgresql'
    static_configs:
      - targets: ['grcmvc-db:9187']

  - job_name: 'redis'
    static_configs:
      - targets: ['grcmvc-redis:9121']
```

---

### Task 4.4: Setup Automated Backups
**Priority**: P1
**Effort**: 4 hours

**Create**: `scripts/backup-database.sh`
```bash
#!/bin/bash
set -e

BACKUP_DIR="/backups/grcmvc"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/grcmvc_backup_$TIMESTAMP.sql.gz"

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Backup database
docker exec grcmvc-db pg_dump -U grc_user GrcMvcDb | gzip > "$BACKUP_FILE"

# Keep only last 30 days of backups
find "$BACKUP_DIR" -name "grcmvc_backup_*.sql.gz" -mtime +30 -delete

echo "Backup completed: $BACKUP_FILE"

# Upload to S3 (optional)
# aws s3 cp "$BACKUP_FILE" s3://your-bucket/backups/grcmvc/
```

**Setup cron job**:
```bash
# Run daily at 2 AM
0 2 * * * /path/to/scripts/backup-database.sh >> /var/log/grcmvc-backup.log 2>&1
```

---

### Task 4.5: Create Deployment Pipeline
**Priority**: P1
**Effort**: 10 hours

**Create**: `.github/workflows/grcmvc-deploy.yml`
```yaml
name: GrcMvc CI/CD Pipeline

on:
  push:
    branches: [ main ]
    paths:
      - 'src/GrcMvc/**'
      - 'docker-compose.grcmvc.yml'
  pull_request:
    branches: [ main ]
    paths:
      - 'src/GrcMvc/**'

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}/grcmvc

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore src/GrcMvc/GrcMvc.csproj

    - name: Build
      run: dotnet build src/GrcMvc/GrcMvc.csproj --configuration Release --no-restore

    - name: Run tests
      run: dotnet test src/GrcMvc/GrcMvc.csproj --configuration Release --no-build --verbosity normal

    - name: Publish
      run: dotnet publish src/GrcMvc/GrcMvc.csproj --configuration Release --output ./publish

  security-scan:
    runs-on: ubuntu-latest
    needs: build-and-test

    steps:
    - uses: actions/checkout@v4

    - name: Run Snyk Security Scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high

    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: 'src/GrcMvc'
        format: 'sarif'
        output: 'trivy-results.sarif'

  docker-build:
    runs-on: ubuntu-latest
    needs: security-scan

    steps:
    - uses: actions/checkout@v4

    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}

    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/GrcMvc/Dockerfile
        push: ${{ github.event_name != 'pull_request' }}
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}

  deploy-staging:
    runs-on: ubuntu-latest
    needs: docker-build
    if: github.ref == 'refs/heads/main'
    environment: staging

    steps:
    - name: Deploy to Staging
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.STAGING_HOST }}
        username: ${{ secrets.STAGING_USER }}
        key: ${{ secrets.STAGING_SSH_KEY }}
        script: |
          cd /opt/grcmvc
          docker-compose pull
          docker-compose up -d
          docker system prune -f

  deploy-production:
    runs-on: ubuntu-latest
    needs: deploy-staging
    if: github.ref == 'refs/heads/main'
    environment: production

    steps:
    - name: Deploy to Production
      uses: appleboy/ssh-action@master
      with:
        host: ${{ secrets.PROD_HOST }}
        username: ${{ secrets.PROD_USER }}
        key: ${{ secrets.PROD_SSH_KEY }}
        script: |
          cd /opt/grcmvc
          docker-compose pull
          docker-compose up -d --no-deps grcmvc
          docker system prune -f
```

---

## Execution Checklist

### Pre-Implementation
- [ ] Backup current database: `scripts/backup-database.sh`
- [ ] Document current configuration
- [ ] Create feature branch: `git checkout -b feature/production-hardening`
- [ ] Review all changes with team

### Phase 1 Implementation (Week 1)
- [ ] Task 1.1: HTTPS/TLS Configuration
- [ ] Task 1.2: Secrets Management
- [ ] Task 1.3: Data Protection Keys
- [ ] Task 1.4: Database Credentials
- [ ] Task 1.5: Security Headers
- [ ] Test all critical paths
- [ ] Deploy to staging environment

### Phase 2 Implementation (Week 2)
- [ ] Task 2.1: Health Checks
- [ ] Task 2.2: Forwarded Headers
- [ ] Task 2.3: Rate Limiting
- [ ] Task 2.4: Comprehensive Logging
- [ ] Task 2.5: Request Validation
- [ ] Load testing
- [ ] Security audit

### Phase 3 Implementation (Week 3)
- [ ] Task 3.1: Audit Logging
- [ ] Task 3.2: Redis Caching
- [ ] Task 3.3: Email Notifications
- [ ] Task 3.4: Background Jobs
- [ ] Task 3.5: Two-Factor Authentication
- [ ] Integration testing
- [ ] User acceptance testing

### Phase 4 Implementation (Week 4)
- [ ] Task 4.1: APM Setup
- [ ] Task 4.2: Prometheus Metrics
- [ ] Task 4.3: Grafana Dashboards
- [ ] Task 4.4: Automated Backups
- [ ] Task 4.5: CI/CD Pipeline
- [ ] Final security scan
- [ ] Performance testing
- [ ] Production deployment

### Post-Deployment
- [ ] Monitor logs for 48 hours
- [ ] Review metrics and dashboards
- [ ] Conduct security audit
- [ ] Update documentation
- [ ] Train team on new features
- [ ] Schedule regular security reviews

---

## Quick Start Commands

```bash
# 1. Update environment variables
cp .env.grcmvc.production .env.grcmvc.production.backup
nano .env.grcmvc.production

# 2. Generate certificates
cd src/GrcMvc
mkdir -p certificates
dotnet dev-certs https -ep certificates/aspnetapp.pfx -p "YourSecurePassword123!"

# 3. Update docker-compose
nano docker-compose.grcmvc.yml

# 4. Rebuild and restart
docker-compose -f docker-compose.grcmvc.yml down
docker-compose -f docker-compose.grcmvc.yml build --no-cache
docker-compose -f docker-compose.grcmvc.yml up -d

# 5. Verify health
curl https://localhost:5138/health
curl https://localhost:5138/health/ready

# 6. Check logs
docker logs grcmvc-app -f

# 7. Monitor metrics
curl http://localhost:5137/metrics
```

---

## Success Criteria

### Security
- ✅ All traffic encrypted with TLS 1.2+
- ✅ No secrets in source code or environment
- ✅ All security headers present
- ✅ Rate limiting active
- ✅ 2FA available for admin users
- ✅ Audit logging complete

### Reliability
- ✅ Health checks passing
- ✅ Zero downtime deployments
- ✅ Automated backups running
- ✅ Data protection keys persisted
- ✅ 99.9% uptime

### Observability
- ✅ Structured logging to files
- ✅ Metrics exported to Prometheus
- ✅ Dashboards in Grafana
- ✅ APM tracking requests
- ✅ Alerts configured

### Performance
- ✅ Response time < 200ms (95th percentile)
- ✅ Redis caching active
- ✅ Database queries optimized
- ✅ Static assets cached
- ✅ Background jobs processing

---

## Rollback Plan

If issues occur:

```bash
# 1. Stop new version
docker-compose -f docker-compose.grcmvc.yml down

# 2. Restore previous version
git checkout main
docker-compose -f docker-compose.grcmvc.yml up -d

# 3. Restore database if needed
docker exec -i grcmvc-db psql -U grc_user GrcMvcDb < /backups/grcmvc/latest_backup.sql

# 4. Verify
curl http://localhost:5137/health
```

---

## Support & Resources

- **Documentation**: `/docs/GRCMVC_IMPLEMENTATION.md`
- **Issues**: Create ticket with `[GrcMvc]` prefix
- **Emergency Contact**: Info@doganconsult.com
- **Monitoring**: https://grafana.portal.shahin-ai.com
- **Logs**: `docker logs grcmvc-app`

---

**End of Action Plan**
