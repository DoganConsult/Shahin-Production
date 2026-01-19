namespace GrcMvc.Configuration;

/// <summary>
/// Production Readiness Gates - NON-NEGOTIABLE
/// These gates must ALL pass before any production deployment.
/// Reference: Go/No-Go Process for GRC Platform
/// </summary>
public static class ProductionReadinessGates
{
    /// <summary>
    /// Gate A: Build & Release Quality (NON-NEGOTIABLE)
    /// </summary>
    public static class GateA_BuildReleaseQuality
    {
        public const string GateId = "GATE-A";
        public const string Name = "Build & Release Quality";
        public const bool IsNonNegotiable = true;

        public static readonly GateCriteria[] Criteria = new[]
        {
            new GateCriteria
            {
                Id = "A1",
                Name = "Clean CI Build",
                Description = "dotnet restore, dotnet build -c Release succeeds with no errors",
                Evidence = "CI logs + release artifact hash",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "A2",
                Name = "No Compiler Warnings as Errors",
                Description = "No compiler errors/warnings treated as errors for Release configuration",
                Evidence = "Build logs showing zero errors/warnings",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "A3",
                Name = "DB Migrations Clean",
                Description = "Migrations apply cleanly to empty database AND upgraded database",
                Evidence = "Migration execution log + schema version",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "A4",
                Name = "Versioning and Release Notes",
                Description = "Version number assigned and release notes documented",
                Evidence = "Release notes document + version in assembly",
                IsBlocking = true
            }
        };
    }

    /// <summary>
    /// Gate B: End-to-End Golden Flows (NON-NEGOTIABLE)
    /// These flows must work with a test tenant in staging
    /// </summary>
    public static class GateB_GoldenFlows
    {
        public const string GateId = "GATE-B";
        public const string Name = "End-to-End Golden Flows";
        public const bool IsNonNegotiable = true;

        public static readonly GoldenFlow[] Flows = new[]
        {
            new GoldenFlow
            {
                Id = "B1",
                Name = "Self Registration",
                Endpoint = "/api/auth/register",
                Steps = new[]
                {
                    "POST /api/auth/register with email, password, fullName",
                    "Verify user created in database",
                    "Complete email verification (if enabled)",
                    "POST /api/auth/login succeeds",
                    "Verify JWT token returned"
                },
                ExpectedAuditEvents = new[] { "AM01_USER_CREATED", "AM01_USER_REGISTERED" },
                Evidence = "API request/response logs, DB user record, token validation"
            },
            new GoldenFlow
            {
                Id = "B2",
                Name = "Trial Signup",
                Endpoint = "/api/trial/signup",
                Steps = new[]
                {
                    "POST /api/trial/signup with company info",
                    "Verify trial record created",
                    "Verify trial status is Pending"
                },
                ExpectedAuditEvents = new[] { "AM01_TRIAL_SIGNUP_INITIATED" },
                Evidence = "API response, Trial record in DB"
            },
            new GoldenFlow
            {
                Id = "B3",
                Name = "Trial Provision",
                Endpoint = "/api/trial/provision",
                Steps = new[]
                {
                    "POST /api/trial/provision with trial ID",
                    "Verify Tenant created",
                    "Verify User created with TenantAdmin role",
                    "Verify TenantUser junction record",
                    "POST /api/auth/login with new user succeeds immediately"
                },
                ExpectedAuditEvents = new[] { "AM01_TENANT_CREATED", "AM01_USER_CREATED", "AM03_ROLE_ASSIGNED" },
                Evidence = "API response, Tenant/User/TenantUser records, successful login"
            },
            new GoldenFlow
            {
                Id = "B4",
                Name = "User Invite",
                Endpoint = "/api/tenants/{tenantId}/users/invite",
                Steps = new[]
                {
                    "POST invite with email and role",
                    "Verify UserInvitation record created",
                    "Verify email sent (check email delivery logs)"
                },
                ExpectedAuditEvents = new[] { "AM01_USER_INVITED" },
                Evidence = "API response, UserInvitation record, email delivery message ID"
            },
            new GoldenFlow
            {
                Id = "B5",
                Name = "Accept Invite",
                Endpoint = "/api/invitation/accept",
                Steps = new[]
                {
                    "POST accept with invitation token and password",
                    "Verify User created",
                    "Verify TenantUser created with assigned role",
                    "Verify UserInvitation status = Accepted",
                    "POST /api/auth/login succeeds"
                },
                ExpectedAuditEvents = new[] { "AM01_USER_CREATED", "AM03_ROLE_ASSIGNED" },
                Evidence = "API response, User/TenantUser records, successful login"
            },
            new GoldenFlow
            {
                Id = "B6",
                Name = "Role Change",
                Endpoint = "/api/tenants/{tenantId}/users/{userId}/roles",
                Steps = new[]
                {
                    "PUT/POST role assignment",
                    "Verify TenantUser.RoleCode updated",
                    "Verify permissions enforce correctly (test protected endpoint)"
                },
                ExpectedAuditEvents = new[] { "AM03_ROLE_ASSIGNED", "AM03_ROLE_CHANGED" },
                Evidence = "API response, TenantUser record, permission test results"
            }
        };
    }

    /// <summary>
    /// Gate C: Audit & Security Controls (NON-NEGOTIABLE)
    /// </summary>
    public static class GateC_AuditSecurity
    {
        public const string GateId = "GATE-C";
        public const string Name = "Audit & Security Controls";
        public const bool IsNonNegotiable = true;

        public static readonly GateCriteria[] Criteria = new[]
        {
            new GateCriteria
            {
                Id = "C1",
                Name = "Audit Events Emitted",
                Description = "Every access action emits correct audit events per traceability matrix",
                Evidence = "AuditEvent extracts with correlationId and actor/tenant IDs",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "C2",
                Name = "Authentication Logging",
                Description = "All auth events logged in AuthenticationAuditLog: Login, FailedLogin, RoleChanged, 2FAEnabled",
                Evidence = "AuthenticationAuditLog extracts",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "C3",
                Name = "Privileged Access MFA",
                Description = "PlatformAdmin/TenantAdmin require MFA (or documented compensating control)",
                Evidence = "MFA policy config + test proof OR compensating control documentation",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "C4",
                Name = "Rate Limiting Active",
                Description = "Rate limiting enabled on auth endpoints [EnableRateLimiting(\"auth\")]",
                Evidence = "Rate limit config and test proof (429 response on threshold)",
                IsBlocking = true
            }
        };

        /// <summary>
        /// Required audit events per flow (must all be present)
        /// </summary>
        public static readonly Dictionary<string, string[]> RequiredAuditEvents = new()
        {
            ["Register"] = new[] { "AM01_USER_CREATED", "AM01_USER_REGISTERED" },
            ["TrialSignup"] = new[] { "AM01_TRIAL_SIGNUP_INITIATED" },
            ["TrialProvision"] = new[] { "AM01_TENANT_CREATED", "AM01_USER_CREATED", "AM03_ROLE_ASSIGNED" },
            ["Invite"] = new[] { "AM01_USER_INVITED" },
            ["AcceptInvite"] = new[] { "AM01_USER_CREATED", "AM03_ROLE_ASSIGNED" },
            ["Login"] = new[] { "AM02_LOGIN_SUCCESS" },
            ["FailedLogin"] = new[] { "AM02_LOGIN_FAILED" },
            ["RoleChange"] = new[] { "AM03_ROLE_ASSIGNED" }
        };
    }

    /// <summary>
    /// Gate D: Operational Readiness (NON-NEGOTIABLE)
    /// </summary>
    public static class GateD_OperationalReadiness
    {
        public const string GateId = "GATE-D";
        public const string Name = "Operational Readiness";
        public const bool IsNonNegotiable = true;

        public static readonly GateCriteria[] Criteria = new[]
        {
            new GateCriteria
            {
                Id = "D1",
                Name = "TLS/SSL Enabled",
                Description = "TLS/SSL enabled in production environment",
                Evidence = "SSL certificate + HTTPS-only config",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "D2",
                Name = "Secrets Secured",
                Description = "No secrets in appsettings committed to git; use Azure Key Vault or env vars",
                Evidence = "Secret management config + git history audit",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "D3",
                Name = "Backups Configured",
                Description = "PostgreSQL backups configured with tested restore procedure",
                Evidence = "Backup schedule config + restore test report",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "D4",
                Name = "Monitoring Dashboards",
                Description = "Dashboards exist for: error rate, latency, job failures, email failures",
                Evidence = "Dashboard links + screenshots",
                IsBlocking = true
            },
            new GateCriteria
            {
                Id = "D5",
                Name = "Alerting Configured",
                Description = "Alerts for: login failure spikes, job failures, DB down, high error rate",
                Evidence = "Alert rules configuration + test notification",
                IsBlocking = true
            }
        };
    }

    /// <summary>
    /// Minimum Production Scope - What MUST be implemented
    /// </summary>
    public static class MinimumProductionScope
    {
        public static readonly string[] MustBeImplemented = new[]
        {
            "Build is green and deployable (Gate A)",
            "Golden flows pass (Gate B)",
            "Audit events emitted consistently per traceability matrix",
            "Role enforcement works (RoleConstants are canonical)",
            "Hangfire jobs stable (compensating control for ABP workers)"
        };

        public static readonly string[] CanBePostponed = new[]
        {
            "Kafka integration (disabled with governance)",
            "Camunda integration (disabled with governance)",
            "ClickHouse analytics (disabled with governance)",
            "Redis caching (disabled with governance)",
            "AI modules (disabled until policy + provider + opt-in approved)"
        };
    }

    /// <summary>
    /// Hardening Checklist - Common production blockers
    /// </summary>
    public static class HardeningChecklist
    {
        public static readonly HardeningItem[] Items = new[]
        {
            new HardeningItem
            {
                Id = "H1",
                Name = "Consistent Status Codes",
                Description = "Use canonical Pending/Active/Suspended everywhere",
                RiskIfMissing = "Data inconsistency, broken queries"
            },
            new HardeningItem
            {
                Id = "H2",
                Name = "Consistent Role Codes",
                Description = "APIs accept only RoleConstants values or map legacy values",
                RiskIfMissing = "Authorization bypass, role confusion"
            },
            new HardeningItem
            {
                Id = "H3",
                Name = "Idempotency",
                Description = "Provision endpoints and reminder jobs safe to retry",
                RiskIfMissing = "Duplicate records, data corruption"
            },
            new HardeningItem
            {
                Id = "H4",
                Name = "Error Handling",
                Description = "No raw exceptions returned; consistent error envelope + correlationId",
                RiskIfMissing = "Information disclosure, poor debugging"
            },
            new HardeningItem
            {
                Id = "H5",
                Name = "Data Retention",
                Description = "Defined retention for audit logs and authentication logs",
                RiskIfMissing = "Compliance violation, storage bloat"
            }
        };
    }

    /// <summary>
    /// Evidence Pack - What to show auditors/customers
    /// </summary>
    public static class EvidencePack
    {
        public const string FolderName = "Production_Readiness_Evidence";

        public static readonly string[] RequiredDocuments = new[]
        {
            "Release build logs + version",
            "DB migration proof (execution log + schema version)",
            "Golden flow test evidence (requests/responses + DB snapshots)",
            "AuditEvent extracts for each flow step",
            "AuthenticationAuditLog extracts (login/failed/role/mfa)",
            "Backup/restore test evidence",
            "Monitoring/alerting screenshots"
        };
    }

    /// <summary>
    /// Get all gates summary
    /// </summary>
    public static GateSummary[] GetAllGates() => new[]
    {
        new GateSummary { GateId = "GATE-A", Name = GateA_BuildReleaseQuality.Name, CriteriaCount = GateA_BuildReleaseQuality.Criteria.Length, IsNonNegotiable = true },
        new GateSummary { GateId = "GATE-B", Name = GateB_GoldenFlows.Name, CriteriaCount = GateB_GoldenFlows.Flows.Length, IsNonNegotiable = true },
        new GateSummary { GateId = "GATE-C", Name = GateC_AuditSecurity.Name, CriteriaCount = GateC_AuditSecurity.Criteria.Length, IsNonNegotiable = true },
        new GateSummary { GateId = "GATE-D", Name = GateD_OperationalReadiness.Name, CriteriaCount = GateD_OperationalReadiness.Criteria.Length, IsNonNegotiable = true }
    };

    /// <summary>
    /// Validate all gates (returns blocking issues)
    /// </summary>
    public static string[] ValidateGates(GateValidationContext context)
    {
        var issues = new List<string>();

        // Gate A validation
        if (!context.BuildSucceeded) issues.Add("[GATE-A] Build failed - BLOCKING");
        if (!context.MigrationsApplied) issues.Add("[GATE-A] Migrations not applied - BLOCKING");

        // Gate B validation
        foreach (var flow in GateB_GoldenFlows.Flows)
        {
            if (!context.PassedFlows.Contains(flow.Id))
                issues.Add($"[GATE-B] Golden flow {flow.Name} not validated - BLOCKING");
        }

        // Gate C validation
        if (!context.AuditEventsVerified) issues.Add("[GATE-C] Audit events not verified - BLOCKING");
        if (!context.RateLimitingEnabled) issues.Add("[GATE-C] Rate limiting not enabled - BLOCKING");

        // Gate D validation
        if (!context.TlsEnabled) issues.Add("[GATE-D] TLS not enabled - BLOCKING");
        if (!context.SecretsSecured) issues.Add("[GATE-D] Secrets not secured - BLOCKING");
        if (!context.BackupsConfigured) issues.Add("[GATE-D] Backups not configured - BLOCKING");
        if (!context.MonitoringConfigured) issues.Add("[GATE-D] Monitoring not configured - BLOCKING");

        return issues.ToArray();
    }
}

#region Supporting Types

public class GateCriteria
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public bool IsBlocking { get; set; } = true;
}

public class GoldenFlow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string[] Steps { get; set; } = Array.Empty<string>();
    public string[] ExpectedAuditEvents { get; set; } = Array.Empty<string>();
    public string Evidence { get; set; } = string.Empty;
}

public class HardeningItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RiskIfMissing { get; set; } = string.Empty;
}

public class GateSummary
{
    public string GateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CriteriaCount { get; set; }
    public bool IsNonNegotiable { get; set; }
}

public class GateValidationContext
{
    public bool BuildSucceeded { get; set; }
    public bool MigrationsApplied { get; set; }
    public HashSet<string> PassedFlows { get; set; } = new();
    public bool AuditEventsVerified { get; set; }
    public bool RateLimitingEnabled { get; set; }
    public bool TlsEnabled { get; set; }
    public bool SecretsSecured { get; set; }
    public bool BackupsConfigured { get; set; }
    public bool MonitoringConfigured { get; set; }
}

#endregion
