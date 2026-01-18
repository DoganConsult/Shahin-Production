using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using StackExchange.Redis;
using System.Diagnostics;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Health check endpoint for all integrations and services.
    /// Provides detailed status of database, cache, and external API connections.
    /// NOTE: Route changed from api/health to api/integration-health to avoid conflict with ApiHealthController
    /// </summary>
    [Route("api/integration-health")]
    [ApiController]
    public class IntegrationHealthController : ControllerBase
    {
        private readonly GrcDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IntegrationHealthController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public IntegrationHealthController(
            GrcDbContext dbContext,
            IConfiguration configuration,
            ILogger<IntegrationHealthController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Basic health check - returns 200 if application is running
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = _configuration["AppInfo:Version"] ?? "1.0.0"
            });
        }

        /// <summary>
        /// Detailed health check for all integrations
        /// </summary>
        [HttpGet("integrations")]
        [Authorize(Roles = "Admin,PlatformAdmin")]
        public async Task<IActionResult> GetIntegrationHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new IntegrationHealthReport
            {
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            // Check all integrations in parallel
            var tasks = new List<Task>
            {
                CheckDatabaseAsync(results),
                CheckRedisAsync(results),
                CheckClaudeApiAsync(results),
                CheckMicrosoftGraphAsync(results),
                CheckCamundaAsync(results),
                CheckKafkaAsync(results)
            };

            await Task.WhenAll(tasks);

            // Check configuration status
            CheckConfigurationStatus(results);

            stopwatch.Stop();
            results.TotalCheckDurationMs = stopwatch.ElapsedMilliseconds;
            results.OverallStatus = DetermineOverallStatus(results);

            return Ok(results);
        }

        /// <summary>
        /// Quick liveness probe for Kubernetes/Docker
        /// </summary>
        [HttpGet("live")]
        [AllowAnonymous]
        public IActionResult GetLiveness()
        {
            return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Readiness probe - checks if app can serve requests
        /// </summary>
        [HttpGet("ready")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                // Quick database check
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(503, new { status = "not_ready", reason = "database_unavailable" });
                }

                return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new { status = "not_ready", reason = ex.Message });
            }
        }

        /// <summary>
        /// Get port allocation map
        /// </summary>
        [HttpGet("ports")]
        [Authorize(Roles = "Admin,PlatformAdmin")]
        public IActionResult GetPortAllocation()
        {
            var ports = new
            {
                Application = new
                {
                    GrcPortalHttp = 5000,
                    GrcPortalHttps = 5001,
                    GrcPortalDocker = 8080,
                    LandingPage = 3000,
                    Staging = 5001,
                    DevTestDemo = 5002
                },
                Database = new
                {
                    PostgresPrimary = 5432,
                    PostgresReplica = 5433,
                    PgBouncer = 6432,
                    HAProxyStats = 8404
                },
                Cache = new
                {
                    RedisMaster = 6379,
                    RedisSentinel = 26379
                },
                Messaging = new
                {
                    Kafka = 9092,
                    KafkaConnect = 8083,
                    RabbitMq = 5672
                },
                Workflow = new
                {
                    Camunda = 8085,
                    N8n = 5678
                },
                Monitoring = new
                {
                    Prometheus = 9090,
                    Grafana = 3030,
                    KafkaUi = 9080,
                    Superset = 8088,
                    TraefikDashboard = 8080
                }
            };

            return Ok(ports);
        }

        /// <summary>
        /// Get configuration status
        /// </summary>
        [HttpGet("config")]
        [Authorize(Roles = "Admin,PlatformAdmin")]
        public IActionResult GetConfigurationStatus()
        {
            var configStatus = new Dictionary<string, ConfigItemStatus>();

            // Check critical configurations
            CheckConfigItem(configStatus, "ConnectionStrings:DefaultConnection", true);
            CheckConfigItem(configStatus, "JwtSettings:Secret", true);
            CheckConfigItem(configStatus, "ClaudeAgents:ApiKey", false);
            CheckConfigItem(configStatus, "EmailOperations:MicrosoftGraph:ClientSecret", false);
            CheckConfigItem(configStatus, "Security:Captcha:SecretKey", false);
            CheckConfigItem(configStatus, "SmtpSettings:Password", false);
            CheckConfigItem(configStatus, "Demo:Email", false);
            CheckConfigItem(configStatus, "Demo:Password", false);

            // Check feature flags
            var featureFlags = new Dictionary<string, bool>
            {
                ["Kafka"] = _configuration.GetValue<bool>("Kafka:Enabled"),
                ["Camunda"] = _configuration.GetValue<bool>("Camunda:Enabled"),
                ["RabbitMq"] = _configuration.GetValue<bool>("RabbitMq:Enabled"),
                ["Captcha"] = _configuration.GetValue<bool>("Security:Captcha:Enabled"),
                ["RateLimiting"] = _configuration.GetValue<bool>("RateLimiting:Enabled"),
                ["FraudDetection"] = _configuration.GetValue<bool>("FraudDetection:Enabled"),
                ["DemoLogin"] = !_configuration.GetValue<bool>("GrcFeatureFlags:DisableDemoLogin"),
                ["EmailOperations"] = _configuration.GetValue<bool>("EmailOperations:Enabled"),
                ["Alerts"] = _configuration.GetValue<bool>("Alerts:EnableEmail") || 
                            _configuration.GetValue<bool>("Alerts:EnableSlack") ||
                            _configuration.GetValue<bool>("Alerts:EnableTeams")
            };

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                configurations = configStatus,
                featureFlags = featureFlags
            });
        }

        #region Private Methods

        private async Task CheckDatabaseAsync(IntegrationHealthReport results)
        {
            var check = new IntegrationCheck { Name = "PostgreSQL", Type = "Database" };
            var sw = Stopwatch.StartNew();

            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                sw.Stop();

                if (canConnect)
                {
                    check.Status = "healthy";
                    check.Message = "Connected successfully";
                    
                    // Get additional info
                    var serverVersion = _dbContext.Database.GetDbConnection().ServerVersion;
                    check.Details = new { serverVersion };
                }
                else
                {
                    check.Status = "unhealthy";
                    check.Message = "Cannot connect to database";
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                check.Status = "unhealthy";
                check.Message = ex.Message;
                _logger.LogError(ex, "Database health check failed");
            }

            check.ResponseTimeMs = sw.ElapsedMilliseconds;
            results.Integrations.Add(check);
        }

        private async Task CheckRedisAsync(IntegrationHealthReport results)
        {
            var check = new IntegrationCheck { Name = "Redis", Type = "Cache" };
            var sw = Stopwatch.StartNew();

            try
            {
                var redisConnection = _configuration["Redis:ConnectionString"] ?? "localhost:6379";
                var redis = await ConnectionMultiplexer.ConnectAsync(redisConnection);
                var db = redis.GetDatabase();
                
                await db.PingAsync();
                sw.Stop();

                check.Status = "healthy";
                check.Message = "Connected successfully";
                check.Details = new { endpoint = redisConnection };
                
                await redis.CloseAsync();
            }
            catch (Exception ex)
            {
                sw.Stop();
                check.Status = "unhealthy";
                check.Message = ex.Message;
                _logger.LogWarning("Redis health check failed: {Message}", ex.Message);
            }

            check.ResponseTimeMs = sw.ElapsedMilliseconds;
            results.Integrations.Add(check);
        }

        private async Task CheckClaudeApiAsync(IntegrationHealthReport results)
        {
            var check = new IntegrationCheck { Name = "Claude AI", Type = "ExternalAPI" };
            var sw = Stopwatch.StartNew();

            var apiKey = _configuration["ClaudeAgents:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                check.Status = "not_configured";
                check.Message = "API key not configured";
                results.Integrations.Add(check);
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync("https://api.anthropic.com/v1/models");
                sw.Stop();

                check.Status = response.IsSuccessStatusCode ? "healthy" : "degraded";
                check.Message = response.IsSuccessStatusCode ? "API accessible" : $"Status: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                sw.Stop();
                check.Status = "unhealthy";
                check.Message = ex.Message;
            }

            check.ResponseTimeMs = sw.ElapsedMilliseconds;
            results.Integrations.Add(check);
        }

        private async Task CheckMicrosoftGraphAsync(IntegrationHealthReport results)
        {
            var check = new IntegrationCheck { Name = "Microsoft Graph", Type = "ExternalAPI" };
            var sw = Stopwatch.StartNew();

            var clientSecret = _configuration["EmailOperations:MicrosoftGraph:ClientSecret"];
            if (string.IsNullOrEmpty(clientSecret))
            {
                check.Status = "not_configured";
                check.Message = "Client secret not configured";
                results.Integrations.Add(check);
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                // Just check if the endpoint is reachable
                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/$metadata");
                sw.Stop();

                check.Status = response.IsSuccessStatusCode ? "healthy" : "degraded";
                check.Message = "Endpoint reachable";
            }
            catch (Exception ex)
            {
                sw.Stop();
                check.Status = "unhealthy";
                check.Message = ex.Message;
            }

            check.ResponseTimeMs = sw.ElapsedMilliseconds;
            results.Integrations.Add(check);
        }

        private async Task CheckCamundaAsync(IntegrationHealthReport results)
        {
            var check = new IntegrationCheck { Name = "Camunda BPM", Type = "Workflow" };
            
            var enabled = _configuration.GetValue<bool>("Camunda:Enabled");
            if (!enabled)
            {
                check.Status = "disabled";
                check.Message = "Camunda is disabled in configuration";
                results.Integrations.Add(check);
                return;
            }

            var sw = Stopwatch.StartNew();
            try
            {
                var baseUrl = _configuration["Camunda:BaseUrl"] ?? "http://localhost:8085/camunda";
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.GetAsync($"{baseUrl}/engine-rest/engine");
                sw.Stop();

                check.Status = response.IsSuccessStatusCode ? "healthy" : "unhealthy";
                check.Message = response.IsSuccessStatusCode ? "Engine running" : $"Status: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                sw.Stop();
                check.Status = "unhealthy";
                check.Message = ex.Message;
            }

            check.ResponseTimeMs = sw.ElapsedMilliseconds;
            results.Integrations.Add(check);
        }

        private async Task CheckKafkaAsync(IntegrationHealthReport results)
        {
            var check = new IntegrationCheck { Name = "Kafka", Type = "Messaging" };
            
            var enabled = _configuration.GetValue<bool>("Kafka:Enabled");
            if (!enabled)
            {
                check.Status = "disabled";
                check.Message = "Kafka is disabled in configuration";
                results.Integrations.Add(check);
                return;
            }

            // Kafka check would require Confluent.Kafka package
            check.Status = "not_checked";
            check.Message = "Kafka health check requires additional setup";
            results.Integrations.Add(check);
            
            await Task.CompletedTask;
        }

        private void CheckConfigurationStatus(IntegrationHealthReport results)
        {
            var missingConfigs = new List<string>();

            // Check critical configs
            if (IsPlaceholder(_configuration["JwtSettings:Secret"]))
                missingConfigs.Add("JwtSettings:Secret");

            if (IsPlaceholder(_configuration.GetConnectionString("DefaultConnection")))
                missingConfigs.Add("ConnectionStrings:DefaultConnection");

            results.MissingConfigurations = missingConfigs;
        }

        private bool IsPlaceholder(string? value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            var indicators = new[] { "CHANGE_THIS", "CHANGE_ME", "${", "YOUR_", "TODO", "PLACEHOLDER" };
            return indicators.Any(i => value.Contains(i, StringComparison.OrdinalIgnoreCase));
        }

        private void CheckConfigItem(Dictionary<string, ConfigItemStatus> items, string key, bool isRequired)
        {
            var value = _configuration[key];
            var status = new ConfigItemStatus
            {
                Key = key,
                IsRequired = isRequired,
                IsConfigured = !string.IsNullOrEmpty(value) && !IsPlaceholder(value),
                IsPlaceholder = IsPlaceholder(value)
            };

            if (status.IsRequired && !status.IsConfigured)
                status.Status = "missing";
            else if (status.IsPlaceholder)
                status.Status = "placeholder";
            else if (status.IsConfigured)
                status.Status = "configured";
            else
                status.Status = "empty";

            items[key] = status;
        }

        private string DetermineOverallStatus(IntegrationHealthReport results)
        {
            var statuses = results.Integrations.Select(i => i.Status).ToList();

            if (statuses.Any(s => s == "unhealthy"))
                return "unhealthy";
            if (statuses.Any(s => s == "degraded"))
                return "degraded";
            if (results.MissingConfigurations.Any())
                return "warning";

            return "healthy";
        }

        #endregion
    }

    #region Models

    public class IntegrationHealthReport
    {
        public DateTime Timestamp { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string OverallStatus { get; set; } = "unknown";
        public long TotalCheckDurationMs { get; set; }
        public List<IntegrationCheck> Integrations { get; set; } = new();
        public List<string> MissingConfigurations { get; set; } = new();
    }

    public class IntegrationCheck
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = "unknown";
        public string Message { get; set; } = string.Empty;
        public long ResponseTimeMs { get; set; }
        public object? Details { get; set; }
    }

    public class ConfigItemStatus
    {
        public string Key { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsConfigured { get; set; }
        public bool IsPlaceholder { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    #endregion
}
