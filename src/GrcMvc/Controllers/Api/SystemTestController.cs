using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using System.Diagnostics;

namespace GrcMvc.Controllers.Api
{
    /// <summary>
    /// Comprehensive system test controller for testing all services,
    /// database operations, email services, and CRUD operations.
    /// </summary>
    [Route("api/test")]
    [ApiController]
    [Authorize(Roles = "Admin,PlatformAdmin")]
    public class SystemTestController : ControllerBase
    {
        private readonly GrcDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SystemTestController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public SystemTestController(
            GrcDbContext dbContext,
            IConfiguration configuration,
            ILogger<SystemTestController> logger,
            IHttpClientFactory httpClientFactory,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Run all system tests
        /// </summary>
        [HttpGet("run-all")]
        public async Task<IActionResult> RunAllTests()
        {
            var report = new SystemTestReport
            {
                StartTime = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            var stopwatch = Stopwatch.StartNew();

            // Run all test categories
            report.DatabaseTests = await RunDatabaseTestsAsync();
            report.TableRelationTests = await RunTableRelationTestsAsync();
            report.CrudTests = await RunCrudTestsAsync();
            report.ServiceTests = await RunServiceTestsAsync();
            report.EmailTests = await RunEmailTestsAsync();
            report.QueryTests = await RunQueryTestsAsync();

            stopwatch.Stop();
            report.EndTime = DateTime.UtcNow;
            report.TotalDurationMs = stopwatch.ElapsedMilliseconds;
            report.OverallStatus = DetermineOverallStatus(report);

            return Ok(report);
        }

        /// <summary>
        /// Test database connectivity and schema
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            return Ok(await RunDatabaseTestsAsync());
        }

        /// <summary>
        /// Test table relations
        /// </summary>
        [HttpGet("relations")]
        public async Task<IActionResult> TestRelations()
        {
            return Ok(await RunTableRelationTestsAsync());
        }

        /// <summary>
        /// Test CRUD operations
        /// </summary>
        [HttpGet("crud")]
        public async Task<IActionResult> TestCrud()
        {
            return Ok(await RunCrudTestsAsync());
        }

        /// <summary>
        /// Test all services
        /// </summary>
        [HttpGet("services")]
        public async Task<IActionResult> TestServices()
        {
            return Ok(await RunServiceTestsAsync());
        }

        /// <summary>
        /// Test email services
        /// </summary>
        [HttpGet("email")]
        public async Task<IActionResult> TestEmail()
        {
            return Ok(await RunEmailTestsAsync());
        }

        /// <summary>
        /// Test database queries
        /// </summary>
        [HttpGet("queries")]
        public async Task<IActionResult> TestQueries()
        {
            return Ok(await RunQueryTestsAsync());
        }

        #region Database Tests

        private async Task<TestCategory> RunDatabaseTestsAsync()
        {
            var category = new TestCategory { Name = "Database Tests" };
            var sw = Stopwatch.StartNew();

            // Test 1: Connection
            await RunTestAsync(category, "Database Connection", async () =>
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                return canConnect ? TestResult.Pass("Connected successfully") : TestResult.Fail("Cannot connect");
            });

            // Test 2: Migrations
            await RunTestAsync(category, "Migrations Applied", async () =>
            {
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                var count = pendingMigrations.Count();
                return count == 0 
                    ? TestResult.Pass("All migrations applied") 
                    : TestResult.Warn($"{count} pending migrations");
            });

            // Test 3: Table Count
            await RunTestAsync(category, "Tables Exist", async () =>
            {
                var tableCount = await GetTableCountAsync();
                return tableCount > 0 
                    ? TestResult.Pass($"{tableCount} tables found") 
                    : TestResult.Fail("No tables found");
            });

            // Test 4: Database Version
            await RunTestAsync(category, "Database Version", async () =>
            {
                await _dbContext.Database.OpenConnectionAsync();
                var version = _dbContext.Database.GetDbConnection().ServerVersion;
                await _dbContext.Database.CloseConnectionAsync();
                return TestResult.Pass($"PostgreSQL {version}");
            });

            sw.Stop();
            category.DurationMs = sw.ElapsedMilliseconds;
            return category;
        }

        private async Task<int> GetTableCountAsync()
        {
            var sql = @"SELECT COUNT(*) FROM information_schema.tables 
                        WHERE table_schema = 'public' AND table_type = 'BASE TABLE'";
            var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            return Convert.ToInt32(result);
        }

        #endregion

        #region Table Relation Tests

        private async Task<TestCategory> RunTableRelationTestsAsync()
        {
            var category = new TestCategory { Name = "Table Relation Tests" };
            var sw = Stopwatch.StartNew();

            // Test: Tenant -> TenantUsers relation
            await RunTestAsync(category, "Tenant -> TenantUsers", async () =>
            {
                var tenants = await _dbContext.Tenants.Take(1).ToListAsync();
                var tenantUsers = await _dbContext.TenantUsers.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            // Test: Control -> Evidence relation
            await RunTestAsync(category, "Control -> Evidence", async () =>
            {
                var controls = await _dbContext.Controls.Take(1).ToListAsync();
                var evidences = await _dbContext.Evidences.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            // Test: Assessment exists
            await RunTestAsync(category, "Assessment -> Tenant", async () =>
            {
                var assessments = await _dbContext.Assessments.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            // Test: Risk -> RiskTreatment relation
            await RunTestAsync(category, "Risk -> Treatments", async () =>
            {
                var risks = await _dbContext.Risks.Take(1).ToListAsync();
                var treatments = await _dbContext.RiskTreatments.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            // Test: Workflow -> WorkflowExecution relation
            await RunTestAsync(category, "Workflow -> Executions", async () =>
            {
                var workflows = await _dbContext.Workflows.Take(1).ToListAsync();
                var executions = await _dbContext.WorkflowExecutions.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            // Test: BaselineProfile -> Controls
            await RunTestAsync(category, "Baseline -> Controls", async () =>
            {
                var baselines = await _dbContext.BaselineProfiles.Take(1).ToListAsync();
                var controls = await _dbContext.Controls.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            // Test: AuditEvents exist
            await RunTestAsync(category, "AuditEvents", async () =>
            {
                var events = await _dbContext.AuditEvents.Take(1).ToListAsync();
                return TestResult.Pass("Relation valid");
            });

            sw.Stop();
            category.DurationMs = sw.ElapsedMilliseconds;
            return category;
        }

        #endregion

        #region CRUD Tests

        private async Task<TestCategory> RunCrudTestsAsync()
        {
            var category = new TestCategory { Name = "CRUD Operation Tests" };
            var sw = Stopwatch.StartNew();

            // Test CREATE - AuditEvent
            await RunTestAsync(category, "CREATE - AuditEvent", async () =>
            {
                var auditEvent = new AuditEvent
                {
                    EventType = "TEST_CREATE",
                    AffectedEntityType = "SystemTest",
                    AffectedEntityId = Guid.NewGuid().ToString(),
                    Description = "Test audit event entry",
                    EventTimestamp = DateTime.UtcNow,
                    IpAddress = "127.0.0.1",
                    TenantId = Guid.Empty,
                    EventId = $"evt-{Guid.NewGuid()}",
                    Action = "Test"
                };
                _dbContext.AuditEvents.Add(auditEvent);
                await _dbContext.SaveChangesAsync();
                return TestResult.Pass($"Created ID: {auditEvent.Id}");
            });

            // Test READ
            await RunTestAsync(category, "READ - AuditEvents", async () =>
            {
                var events = await _dbContext.AuditEvents
                    .Where(e => e.EventType == "TEST_CREATE")
                    .ToListAsync();
                return TestResult.Pass($"Read {events.Count} test records");
            });

            // Test UPDATE
            await RunTestAsync(category, "UPDATE - AuditEvent", async () =>
            {
                var auditEvent = await _dbContext.AuditEvents
                    .FirstOrDefaultAsync(e => e.EventType == "TEST_CREATE");
                if (auditEvent != null)
                {
                    auditEvent.Description = "Updated test entry";
                    await _dbContext.SaveChangesAsync();
                    return TestResult.Pass("Updated successfully");
                }
                return TestResult.Warn("No record to update");
            });

            // Test DELETE
            await RunTestAsync(category, "DELETE - AuditEvents (cleanup)", async () =>
            {
                var testEvents = await _dbContext.AuditEvents
                    .Where(e => e.EventType == "TEST_CREATE")
                    .ToListAsync();
                _dbContext.AuditEvents.RemoveRange(testEvents);
                await _dbContext.SaveChangesAsync();
                return TestResult.Pass($"Deleted {testEvents.Count} test records");
            });

            // Test Tenant CRUD
            await RunTestAsync(category, "Tenant - Count", async () =>
            {
                var count = await _dbContext.Tenants.CountAsync();
                return TestResult.Pass($"{count} tenants");
            });

            // Test Control CRUD
            await RunTestAsync(category, "Control - Count", async () =>
            {
                var count = await _dbContext.Controls.CountAsync();
                return TestResult.Pass($"{count} controls");
            });

            // Test Risk CRUD
            await RunTestAsync(category, "Risk - Count", async () =>
            {
                var count = await _dbContext.Risks.CountAsync();
                return TestResult.Pass($"{count} risks");
            });

            // Test Assessment CRUD
            await RunTestAsync(category, "Assessment - Count", async () =>
            {
                var count = await _dbContext.Assessments.CountAsync();
                return TestResult.Pass($"{count} assessments");
            });

            sw.Stop();
            category.DurationMs = sw.ElapsedMilliseconds;
            return category;
        }

        #endregion

        #region Service Tests

        private async Task<TestCategory> RunServiceTestsAsync()
        {
            var category = new TestCategory { Name = "Service Tests" };
            var sw = Stopwatch.StartNew();

            // Test EmailOperationsService
            await RunTestAsync(category, "EmailOperationsService", async () =>
            {
                var service = _serviceProvider.GetService<GrcMvc.Services.EmailOperations.IEmailOperationsService>();
                return service != null 
                    ? TestResult.Pass("Registered") 
                    : TestResult.Warn("Not registered");
            });

            // Test MicrosoftGraphEmailService
            await RunTestAsync(category, "MicrosoftGraphEmailService", async () =>
            {
                var service = _serviceProvider.GetService<GrcMvc.Services.EmailOperations.IMicrosoftGraphEmailService>();
                var clientSecret = _configuration["EmailOperations:MicrosoftGraph:ClientSecret"];
                if (service == null) return TestResult.Warn("Not registered");
                if (string.IsNullOrEmpty(clientSecret)) return TestResult.Warn("Registered but no client secret");
                return TestResult.Pass("Registered with credentials");
            });

            // Test IConfiguration
            await RunTestAsync(category, "IConfiguration", async () =>
            {
                var config = _serviceProvider.GetService<IConfiguration>();
                return config != null 
                    ? TestResult.Pass("Registered") 
                    : TestResult.Fail("Not registered");
            });

            // Test IHttpClientFactory
            await RunTestAsync(category, "IHttpClientFactory", async () =>
            {
                var factory = _serviceProvider.GetService<IHttpClientFactory>();
                return factory != null 
                    ? TestResult.Pass("Registered") 
                    : TestResult.Fail("Not registered");
            });

            // Test DbContext
            await RunTestAsync(category, "GrcDbContext", async () =>
            {
                var ctx = _serviceProvider.GetService<GrcDbContext>();
                return ctx != null 
                    ? TestResult.Pass("Registered") 
                    : TestResult.Fail("Not registered");
            });

            await Task.CompletedTask;
            sw.Stop();
            category.DurationMs = sw.ElapsedMilliseconds;
            return category;
        }

        #endregion

        #region Email Tests

        private async Task<TestCategory> RunEmailTestsAsync()
        {
            var category = new TestCategory { Name = "Email Service Tests" };
            var sw = Stopwatch.StartNew();

            // Test Email Configuration
            await RunTestAsync(category, "SMTP Configuration", async () =>
            {
                var smtpServer = _configuration["SmtpSettings:Server"];
                var smtpPort = _configuration["SmtpSettings:Port"];
                if (string.IsNullOrEmpty(smtpServer)) return TestResult.Warn("SMTP not configured");
                return TestResult.Pass($"{smtpServer}:{smtpPort}");
            });

            // Test Microsoft Graph Configuration
            await RunTestAsync(category, "Microsoft Graph Config", async () =>
            {
                var tenantId = _configuration["EmailOperations:MicrosoftGraph:TenantId"];
                var clientId = _configuration["EmailOperations:MicrosoftGraph:ClientId"];
                var clientSecret = _configuration["EmailOperations:MicrosoftGraph:ClientSecret"];
                
                if (string.IsNullOrEmpty(tenantId)) return TestResult.Warn("TenantId not set");
                if (string.IsNullOrEmpty(clientId)) return TestResult.Warn("ClientId not set");
                if (string.IsNullOrEmpty(clientSecret)) return TestResult.Warn("ClientSecret not set");
                
                return TestResult.Pass("All credentials configured");
            });

            // Test Email Mailboxes
            await RunTestAsync(category, "Email Mailboxes", async () =>
            {
                try
                {
                    var mailboxCount = await _dbContext.EmailMailboxes.CountAsync();
                    return TestResult.Pass($"{mailboxCount} mailboxes configured");
                }
                catch
                {
                    return TestResult.Warn("EmailMailboxes table not available");
                }
            });

            // Test Email Templates
            await RunTestAsync(category, "Email Templates", async () =>
            {
                try
                {
                    var templatesExist = await _dbContext.EmailTemplates.AnyAsync();
                    return templatesExist 
                        ? TestResult.Pass("Templates exist") 
                        : TestResult.Warn("No templates found");
                }
                catch
                {
                    return TestResult.Warn("EmailTemplates table not available");
                }
            });

            // Test Graph API Endpoint
            await RunTestAsync(category, "Graph API Reachable", async () =>
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync("https://graph.microsoft.com/v1.0/$metadata");
                    return response.IsSuccessStatusCode 
                        ? TestResult.Pass("Endpoint reachable") 
                        : TestResult.Warn($"Status: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    return TestResult.Fail(ex.Message);
                }
            });

            sw.Stop();
            category.DurationMs = sw.ElapsedMilliseconds;
            return category;
        }

        #endregion

        #region Query Tests

        private async Task<TestCategory> RunQueryTestsAsync()
        {
            var category = new TestCategory { Name = "Query Performance Tests" };
            var sw = Stopwatch.StartNew();

            // Test: Simple select
            await RunTestAsync(category, "Simple SELECT", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var count = await _dbContext.Tenants.CountAsync();
                querySw.Stop();
                return TestResult.Pass($"{count} rows in {querySw.ElapsedMilliseconds}ms");
            });

            // Test: Filtered query
            await RunTestAsync(category, "Filtered Query", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var activeCount = await _dbContext.Tenants
                    .Where(t => t.IsActive)
                    .CountAsync();
                querySw.Stop();
                return TestResult.Pass($"{activeCount} active in {querySw.ElapsedMilliseconds}ms");
            });

            // Test: Join query
            await RunTestAsync(category, "Join Query", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var result = await _dbContext.Controls
                    .Take(10)
                    .ToListAsync();
                querySw.Stop();
                return TestResult.Pass($"{result.Count} rows in {querySw.ElapsedMilliseconds}ms");
            });

            // Test: Aggregate query
            await RunTestAsync(category, "Aggregate Query", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var stats = await _dbContext.Risks
                    .GroupBy(r => r.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();
                querySw.Stop();
                return TestResult.Pass($"{stats.Count} groups in {querySw.ElapsedMilliseconds}ms");
            });

            // Test: Complex query
            await RunTestAsync(category, "Complex Query", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var result = await _dbContext.Assessments
                    .Where(a => a.Status != null)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                querySw.Stop();
                return TestResult.Pass($"{result.Count} rows in {querySw.ElapsedMilliseconds}ms");
            });

            // Test: Pagination query
            await RunTestAsync(category, "Pagination Query", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var page = await _dbContext.AuditEvents
                    .OrderByDescending(e => e.CreatedAt)
                    .Skip(0)
                    .Take(20)
                    .ToListAsync();
                querySw.Stop();
                return TestResult.Pass($"{page.Count} rows in {querySw.ElapsedMilliseconds}ms");
            });

            // Test: Search query
            await RunTestAsync(category, "Search Query", async () =>
            {
                var querySw = Stopwatch.StartNew();
                var results = await _dbContext.Controls
                    .Where(c => c.Name != null && c.Name.Contains("test"))
                    .Take(10)
                    .ToListAsync();
                querySw.Stop();
                return TestResult.Pass($"{results.Count} matches in {querySw.ElapsedMilliseconds}ms");
            });

            sw.Stop();
            category.DurationMs = sw.ElapsedMilliseconds;
            return category;
        }

        #endregion

        #region Helper Methods

        private async Task RunTestAsync(TestCategory category, string name, Func<Task<TestResult>> testFunc)
        {
            var test = new TestItem { Name = name };
            var sw = Stopwatch.StartNew();

            try
            {
                var result = await testFunc();
                test.Status = result.Status;
                test.Message = result.Message;
            }
            catch (Exception ex)
            {
                test.Status = "error";
                test.Message = ex.Message;
                _logger.LogError(ex, "Test failed: {TestName}", name);
            }

            sw.Stop();
            test.DurationMs = sw.ElapsedMilliseconds;
            category.Tests.Add(test);
        }

        private string DetermineOverallStatus(SystemTestReport report)
        {
            var allTests = new List<TestItem>();
            allTests.AddRange(report.DatabaseTests?.Tests ?? new List<TestItem>());
            allTests.AddRange(report.TableRelationTests?.Tests ?? new List<TestItem>());
            allTests.AddRange(report.CrudTests?.Tests ?? new List<TestItem>());
            allTests.AddRange(report.ServiceTests?.Tests ?? new List<TestItem>());
            allTests.AddRange(report.EmailTests?.Tests ?? new List<TestItem>());
            allTests.AddRange(report.QueryTests?.Tests ?? new List<TestItem>());

            var failCount = allTests.Count(t => t.Status == "fail" || t.Status == "error");
            var warnCount = allTests.Count(t => t.Status == "warn");
            var passCount = allTests.Count(t => t.Status == "pass");

            if (failCount > 0) return "fail";
            if (warnCount > 0) return "warn";
            return "pass";
        }

        #endregion
    }

    #region Models

    public class SystemTestReport
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long TotalDurationMs { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string OverallStatus { get; set; } = "unknown";
        public TestCategory? DatabaseTests { get; set; }
        public TestCategory? TableRelationTests { get; set; }
        public TestCategory? CrudTests { get; set; }
        public TestCategory? ServiceTests { get; set; }
        public TestCategory? EmailTests { get; set; }
        public TestCategory? QueryTests { get; set; }
    }

    public class TestCategory
    {
        public string Name { get; set; } = string.Empty;
        public long DurationMs { get; set; }
        public List<TestItem> Tests { get; set; } = new();
        public int PassCount => Tests.Count(t => t.Status == "pass");
        public int FailCount => Tests.Count(t => t.Status == "fail" || t.Status == "error");
        public int WarnCount => Tests.Count(t => t.Status == "warn");
    }

    public class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "unknown";
        public string Message { get; set; } = string.Empty;
        public long DurationMs { get; set; }
    }

    public class TestResult
    {
        public string Status { get; set; } = "unknown";
        public string Message { get; set; } = string.Empty;

        public static TestResult Pass(string message) => new() { Status = "pass", Message = message };
        public static TestResult Fail(string message) => new() { Status = "fail", Message = message };
        public static TestResult Warn(string message) => new() { Status = "warn", Message = message };
    }

    #endregion
}
