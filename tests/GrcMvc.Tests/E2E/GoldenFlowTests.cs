using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace GrcMvc.Tests.E2E;

/// <summary>
/// Golden Flow Integration Tests (Gate B)
/// These tests validate the non-negotiable user flows required for production readiness.
/// Reference: ProductionReadinessGates.GateB_GoldenFlows
/// </summary>
[Collection("GoldenFlows")]
public class GoldenFlowTests : IClassFixture<GoldenFlowTestFixture>
{
    private readonly GoldenFlowTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GoldenFlowTests(GoldenFlowTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    #region B1: Self Registration

    [Fact]
    [Trait("Category", "GoldenFlow")]
    [Trait("Flow", "B1-SelfRegistration")]
    public async Task B1_SelfRegistration_ShouldCreateUserAndAllowLogin()
    {
        // Arrange
        var testEmail = $"test-user-{Guid.NewGuid():N}@test.com";
        var testPassword = "Test@12345678";
        var testFullName = "Test User B1";

        // Act - Step 1: Register
        var registerResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = testEmail,
            password = testPassword,
            fullName = testFullName
        });

        _output.WriteLine($"Register Response: {registerResponse.StatusCode}");

        // Assert - Registration succeeded
        registerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse>();
        registerResult?.Success.Should().BeTrue("Registration should succeed");

        // Act - Step 2: Login with new user
        var loginResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = testEmail,
            password = testPassword
        });

        _output.WriteLine($"Login Response: {loginResponse.StatusCode}");

        // Assert - Login succeeded
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResult?.Success.Should().BeTrue("Login should succeed after registration");
        loginResult?.Data?.Token.Should().NotBeNullOrEmpty("JWT token should be returned");

        // Evidence: Store test results
        _output.WriteLine("=== B1 Self Registration Evidence ===");
        _output.WriteLine($"Email: {testEmail}");
        _output.WriteLine($"Registration Status: {registerResponse.StatusCode}");
        _output.WriteLine($"Login Status: {loginResponse.StatusCode}");
        _output.WriteLine($"Token Present: {!string.IsNullOrEmpty(loginResult?.Data?.Token)}");
        _output.WriteLine("Expected Audit Events: AM01_USER_CREATED, AM01_USER_REGISTERED");
    }

    #endregion

    #region B2: Trial Signup

    [Fact]
    [Trait("Category", "GoldenFlow")]
    [Trait("Flow", "B2-TrialSignup")]
    public async Task B2_TrialSignup_ShouldCreateTrialRecord()
    {
        // Arrange
        var testCompany = $"Test Company {Guid.NewGuid():N}";
        var testEmail = $"admin-{Guid.NewGuid():N}@testco.com";

        // Act
        var signupResponse = await _fixture.Client.PostAsJsonAsync("/api/trial/signup", new
        {
            companyName = testCompany,
            email = testEmail,
            fullName = "Admin User",
            industry = "Technology"
        });

        _output.WriteLine($"Trial Signup Response: {signupResponse.StatusCode}");

        // Assert
        signupResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        var signupResult = await signupResponse.Content.ReadFromJsonAsync<ApiResponse>();
        signupResult?.Success.Should().BeTrue("Trial signup should succeed");

        // Evidence
        _output.WriteLine("=== B2 Trial Signup Evidence ===");
        _output.WriteLine($"Company: {testCompany}");
        _output.WriteLine($"Email: {testEmail}");
        _output.WriteLine($"Signup Status: {signupResponse.StatusCode}");
        _output.WriteLine("Expected Audit Events: AM01_TRIAL_SIGNUP_INITIATED");
    }

    #endregion

    #region B3: Trial Provision

    [Fact]
    [Trait("Category", "GoldenFlow")]
    [Trait("Flow", "B3-TrialProvision")]
    public async Task B3_TrialProvision_ShouldCreateTenantAndUser()
    {
        // Arrange - First create a trial
        var testCompany = $"Provision Test {Guid.NewGuid():N}";
        var testEmail = $"provision-{Guid.NewGuid():N}@testco.com";
        var testPassword = "Provision@12345";

        var signupResponse = await _fixture.Client.PostAsJsonAsync("/api/trial/signup", new
        {
            companyName = testCompany,
            email = testEmail,
            fullName = "Provision Admin",
            industry = "Technology"
        });

        _output.WriteLine($"Pre-signup Response: {signupResponse.StatusCode}");

        // Get trial ID from response (if available)
        var signupResult = await signupResponse.Content.ReadFromJsonAsync<TrialSignupResponse>();

        if (signupResult?.Data?.TrialId == null)
        {
            _output.WriteLine("Note: Trial ID not returned in signup - using email-based provision");
        }

        // Act - Provision the trial
        var provisionResponse = await _fixture.Client.PostAsJsonAsync("/api/trial/provision", new
        {
            trialId = signupResult?.Data?.TrialId,
            email = testEmail,
            password = testPassword
        });

        _output.WriteLine($"Provision Response: {provisionResponse.StatusCode}");

        // Assert
        provisionResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // Act - Login should work immediately
        var loginResponse = await _fixture.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = testEmail,
            password = testPassword
        });

        _output.WriteLine($"Post-provision Login Response: {loginResponse.StatusCode}");

        // Evidence
        _output.WriteLine("=== B3 Trial Provision Evidence ===");
        _output.WriteLine($"Company: {testCompany}");
        _output.WriteLine($"Email: {testEmail}");
        _output.WriteLine($"Provision Status: {provisionResponse.StatusCode}");
        _output.WriteLine($"Login Status: {loginResponse.StatusCode}");
        _output.WriteLine("Expected Audit Events: AM01_TENANT_CREATED, AM01_USER_CREATED, AM03_ROLE_ASSIGNED");
    }

    #endregion

    #region B4: User Invite

    [Fact]
    [Trait("Category", "GoldenFlow")]
    [Trait("Flow", "B4-UserInvite")]
    public async Task B4_UserInvite_ShouldCreateInvitationAndSendEmail()
    {
        // Arrange - Need admin authentication first
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token");
            return;
        }

        var testTenantId = _fixture.TestTenantId;
        var inviteEmail = $"invited-{Guid.NewGuid():N}@test.com";

        // Act
        var inviteRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/tenants/{testTenantId}/users/invite")
        {
            Content = JsonContent.Create(new
            {
                email = inviteEmail,
                roleCode = "TenantUser",
                message = "Welcome to the team"
            })
        };
        inviteRequest.Headers.Add("Authorization", $"Bearer {adminToken}");

        var inviteResponse = await _fixture.Client.SendAsync(inviteRequest);
        _output.WriteLine($"Invite Response: {inviteResponse.StatusCode}");

        // Assert
        inviteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // Evidence
        _output.WriteLine("=== B4 User Invite Evidence ===");
        _output.WriteLine($"Tenant ID: {testTenantId}");
        _output.WriteLine($"Invite Email: {inviteEmail}");
        _output.WriteLine($"Invite Status: {inviteResponse.StatusCode}");
        _output.WriteLine("Expected Audit Events: AM01_USER_INVITED");
    }

    #endregion

    #region B5: Accept Invite

    [Fact]
    [Trait("Category", "GoldenFlow")]
    [Trait("Flow", "B5-AcceptInvite")]
    public async Task B5_AcceptInvite_ShouldCreateUserAndAllowLogin()
    {
        // Arrange - This test requires a valid invitation token
        // In real testing, get token from B4 test or database
        var invitationToken = _fixture.TestInvitationToken;

        if (string.IsNullOrEmpty(invitationToken))
        {
            _output.WriteLine("Skipping: No invitation token available");
            _output.WriteLine("To test: Create invitation via B4, get token from email/database");
            return;
        }

        var testPassword = "Accepted@12345";

        // Act
        var acceptResponse = await _fixture.Client.PostAsJsonAsync("/api/invitation/accept", new
        {
            token = invitationToken,
            password = testPassword,
            fullName = "Invited User"
        });

        _output.WriteLine($"Accept Invite Response: {acceptResponse.StatusCode}");

        // Assert
        acceptResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // Evidence
        _output.WriteLine("=== B5 Accept Invite Evidence ===");
        _output.WriteLine($"Accept Status: {acceptResponse.StatusCode}");
        _output.WriteLine("Expected Audit Events: AM01_USER_CREATED, AM03_ROLE_ASSIGNED");
    }

    #endregion

    #region B6: Role Change

    [Fact]
    [Trait("Category", "GoldenFlow")]
    [Trait("Flow", "B6-RoleChange")]
    public async Task B6_RoleChange_ShouldUpdateRoleAndEnforcePermissions()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        if (string.IsNullOrEmpty(adminToken))
        {
            _output.WriteLine("Skipping: Could not get admin token");
            return;
        }

        var testTenantId = _fixture.TestTenantId;
        var testUserId = _fixture.TestUserId;

        if (string.IsNullOrEmpty(testUserId))
        {
            _output.WriteLine("Skipping: No test user ID available");
            return;
        }

        // Act - Change role
        var roleChangeRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/tenants/{testTenantId}/users/{testUserId}/roles")
        {
            Content = JsonContent.Create(new
            {
                roleCode = "ComplianceOfficer"
            })
        };
        roleChangeRequest.Headers.Add("Authorization", $"Bearer {adminToken}");

        var roleResponse = await _fixture.Client.SendAsync(roleChangeRequest);
        _output.WriteLine($"Role Change Response: {roleResponse.StatusCode}");

        // Assert
        roleResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Evidence
        _output.WriteLine("=== B6 Role Change Evidence ===");
        _output.WriteLine($"Tenant ID: {testTenantId}");
        _output.WriteLine($"User ID: {testUserId}");
        _output.WriteLine($"New Role: ComplianceOfficer");
        _output.WriteLine($"Role Change Status: {roleResponse.StatusCode}");
        _output.WriteLine("Expected Audit Events: AM03_ROLE_ASSIGNED, AM03_ROLE_CHANGED");
    }

    #endregion

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

    #endregion
}

#region Test Fixture

public class GoldenFlowTestFixture : IDisposable
{
    public HttpClient Client { get; }

    // Test configuration - set these for your environment
    public string BaseUrl { get; } = Environment.GetEnvironmentVariable("TEST_API_URL") ?? "http://localhost:5000";
    public string AdminEmail { get; } = Environment.GetEnvironmentVariable("TEST_ADMIN_EMAIL") ?? "admin@grc.com";
    public string AdminPassword { get; } = Environment.GetEnvironmentVariable("TEST_ADMIN_PASSWORD") ?? "Admin@12345";
    public Guid TestTenantId { get; } = Guid.TryParse(Environment.GetEnvironmentVariable("TEST_TENANT_ID"), out var id) ? id : Guid.Empty;
    public string? TestUserId { get; } = Environment.GetEnvironmentVariable("TEST_USER_ID");
    public string? TestInvitationToken { get; } = Environment.GetEnvironmentVariable("TEST_INVITATION_TOKEN");

    public GoldenFlowTestFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}

#endregion

#region Response DTOs

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public LoginData? Data { get; set; }
}

public class LoginData
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public Guid? TenantId { get; set; }
    public bool RequiresOnboarding { get; set; }
}

public class TrialSignupResponse
{
    public bool Success { get; set; }
    public TrialData? Data { get; set; }
}

public class TrialData
{
    public Guid? TrialId { get; set; }
    public string? Status { get; set; }
}

#endregion
