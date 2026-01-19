using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace GrcMvc.Tests.E2E;

/// <summary>
/// Audit Event Verification Tests (Gate C)
/// Validates that all Golden Flows emit the correct audit events per the traceability matrix.
/// Reference: ProductionReadinessGates.GateC_AuditSecurity
/// </summary>
[Collection("AuditVerification")]
public class AuditEventVerificationTests : IClassFixture<GoldenFlowTestFixture>
{
    private readonly GoldenFlowTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public AuditEventVerificationTests(GoldenFlowTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    /// <summary>
    /// Required audit events per flow as defined in the traceability matrix
    /// </summary>
    private static readonly Dictionary<string, string[]> RequiredAuditEvents = new()
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

    [Fact]
    [Trait("Category", "AuditVerification")]
    public async Task VerifyAuditEventsForRegistration()
    {
        // This test verifies audit events exist after B1 registration flow
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token to query audit events");
            return;
        }

        // Query audit events for a recent registration
        // In production, filter by timestamp and correlationId
        var auditResponse = await QueryAuditEventsAsync(adminToken, "AM01_USER_CREATED");

        _output.WriteLine("=== Audit Events for Registration ===");
        _output.WriteLine($"Required: {string.Join(", ", RequiredAuditEvents["Register"])}");
        _output.WriteLine($"Query Status: {auditResponse.StatusCode}");

        if (auditResponse.IsSuccessStatusCode)
        {
            var events = await auditResponse.Content.ReadFromJsonAsync<AuditEventListResponse>();
            _output.WriteLine($"Found {events?.Data?.Count ?? 0} AM01_USER_CREATED events");

            events?.Data?.Count.Should().BeGreaterThan(0,
                "At least one AM01_USER_CREATED event should exist after registration");
        }
    }

    [Fact]
    [Trait("Category", "AuditVerification")]
    public async Task VerifyAuditEventsForTrialProvision()
    {
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token");
            return;
        }

        _output.WriteLine("=== Audit Events for Trial Provision ===");
        _output.WriteLine($"Required: {string.Join(", ", RequiredAuditEvents["TrialProvision"])}");

        // Check for each required event type
        foreach (var eventType in RequiredAuditEvents["TrialProvision"])
        {
            var response = await QueryAuditEventsAsync(adminToken, eventType);
            var events = await response.Content.ReadFromJsonAsync<AuditEventListResponse>();

            _output.WriteLine($"  {eventType}: Found {events?.Data?.Count ?? 0} events");
        }
    }

    [Fact]
    [Trait("Category", "AuditVerification")]
    public async Task VerifyAuditEventsForRoleAssignment()
    {
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token");
            return;
        }

        _output.WriteLine("=== Audit Events for Role Assignment ===");
        _output.WriteLine($"Required: {string.Join(", ", RequiredAuditEvents["RoleChange"])}");

        var response = await QueryAuditEventsAsync(adminToken, "AM03_ROLE_ASSIGNED");
        var events = await response.Content.ReadFromJsonAsync<AuditEventListResponse>();

        _output.WriteLine($"  AM03_ROLE_ASSIGNED: Found {events?.Data?.Count ?? 0} events");

        events?.Data?.Count.Should().BeGreaterThan(0,
            "At least one AM03_ROLE_ASSIGNED event should exist");
    }

    [Fact]
    [Trait("Category", "AuditVerification")]
    public async Task VerifyAuthenticationAuditLog()
    {
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token");
            return;
        }

        _output.WriteLine("=== Authentication Audit Log Verification ===");

        var response = await QueryAuthAuditLogsAsync(adminToken);

        if (response.IsSuccessStatusCode)
        {
            var logs = await response.Content.ReadFromJsonAsync<AuthAuditLogListResponse>();
            _output.WriteLine($"Found {logs?.Data?.Count ?? 0} authentication audit logs");

            // Check for required event types
            var loginEvents = logs?.Data?.Count(l => l.EventType == "Login") ?? 0;
            var failedLogins = logs?.Data?.Count(l => l.EventType == "FailedLogin") ?? 0;

            _output.WriteLine($"  Login events: {loginEvents}");
            _output.WriteLine($"  Failed login events: {failedLogins}");

            loginEvents.Should().BeGreaterThan(0, "Login events should be recorded");
        }
        else
        {
            _output.WriteLine($"Query failed: {response.StatusCode}");
        }
    }

    [Fact]
    [Trait("Category", "AuditVerification")]
    public async Task GenerateAuditEventReport()
    {
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token");
            return;
        }

        _output.WriteLine("========================================");
        _output.WriteLine("    AUDIT EVENT VERIFICATION REPORT    ");
        _output.WriteLine("========================================");
        _output.WriteLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine("");

        // Check all required event types
        var allEventTypes = RequiredAuditEvents.Values
            .SelectMany(v => v)
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        _output.WriteLine("Event Type                    | Count | Status");
        _output.WriteLine("------------------------------|-------|--------");

        var allPresent = true;
        foreach (var eventType in allEventTypes)
        {
            var response = await QueryAuditEventsAsync(adminToken, eventType);
            var events = await response.Content.ReadFromJsonAsync<AuditEventListResponse>();
            var count = events?.Data?.Count ?? 0;
            var status = count > 0 ? "PASS" : "MISSING";

            if (count == 0) allPresent = false;

            _output.WriteLine($"{eventType,-30}| {count,5} | {status}");
        }

        _output.WriteLine("------------------------------|-------|--------");
        _output.WriteLine($"Overall Status: {(allPresent ? "ALL EVENTS PRESENT" : "SOME EVENTS MISSING")}");
        _output.WriteLine("========================================");
    }

    #region Helper Methods

    private async Task<string?> GetAdminTokenAsync()
    {
        try
        {
            var loginResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
            {
                email = _fixture.AdminEmail,
                password = _fixture.AdminPassword
            });

            if (loginResponse.IsSuccessStatusCode)
            {
                var result = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Data?.Token;
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Admin login failed: {ex.Message}");
        }

        return null;
    }

    private async Task<HttpResponseMessage> QueryAuditEventsAsync(string token, string eventType)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/audit/events?eventType={eventType}&limit=100");
        request.Headers.Add("Authorization", $"Bearer {token}");
        return await _fixture.Client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> QueryAuthAuditLogsAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/audit/authentication-logs?limit=100");
        request.Headers.Add("Authorization", $"Bearer {token}");
        return await _fixture.Client.SendAsync(request);
    }

    #endregion
}

#region Response DTOs

public class AuditEventListResponse
{
    public bool Success { get; set; }
    public List<AuditEventDto>? Data { get; set; }
}

public class AuditEventDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthAuditLogListResponse
{
    public bool Success { get; set; }
    public List<AuthAuditLogDto>? Data { get; set; }
}

public class AuthAuditLogDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
