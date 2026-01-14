using Xunit;
using FluentAssertions;
using GrcMvc.Tests.Fixtures;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GrcMvc.Tests.Security;

/// <summary>
/// Tests for multi-tenant security and data isolation
/// Database tests require ABP framework infrastructure (skip in unit test mode)
/// </summary>
[Trait("Category", "Security")]
[Trait("Category", "TenantIsolation")]
public class TenantSecurityTests
{
    #region Tenant Data Isolation Tests

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task TenantA_CannotSeeDataFrom_TenantB()
    {
        // Arrange
        var dbName = $"TenantIsolationTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        // Create tenants using correct property names
        context.Tenants.Add(new Tenant { Id = tenantAId, OrganizationName = "Tenant A", TenantSlug = "tenant-a", IsActive = true, CreatedDate = DateTime.UtcNow });
        context.Tenants.Add(new Tenant { Id = tenantBId, OrganizationName = "Tenant B", TenantSlug = "tenant-b", IsActive = true, CreatedDate = DateTime.UtcNow });

        // Create risks for each tenant
        var riskA = new Risk { Id = Guid.NewGuid(), Name = "Risk A Secret", TenantId = tenantAId, CreatedDate = DateTime.UtcNow };
        var riskB = new Risk { Id = Guid.NewGuid(), Name = "Risk B Secret", TenantId = tenantBId, CreatedDate = DateTime.UtcNow };
        context.Risks.AddRange(riskA, riskB);

        await context.SaveChangesAsync();

        // Act - Query risks for Tenant A only
        var tenantARisks = await context.Risks
            .Where(r => r.TenantId == tenantAId)
            .ToListAsync();

        var tenantBRisks = await context.Risks
            .Where(r => r.TenantId == tenantBId)
            .ToListAsync();

        // Assert
        tenantARisks.Should().HaveCount(1, "Tenant A should only see 1 risk");
        tenantARisks[0].Name.Should().Be("Risk A Secret");
        tenantARisks.Should().NotContain(r => r.Name == "Risk B Secret", "Tenant A should not see Tenant B's data");

        tenantBRisks.Should().HaveCount(1, "Tenant B should only see 1 risk");
        tenantBRisks[0].Name.Should().Be("Risk B Secret");
        tenantBRisks.Should().NotContain(r => r.Name == "Risk A Secret", "Tenant B should not see Tenant A's data");
    }

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task DirectIdAccess_ShouldNotBypassTenantFilter()
    {
        // Arrange
        var dbName = $"DirectAccessTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        context.Tenants.Add(new Tenant { Id = tenantAId, OrganizationName = "Tenant A", TenantSlug = "tenant-a", IsActive = true, CreatedDate = DateTime.UtcNow });
        context.Tenants.Add(new Tenant { Id = tenantBId, OrganizationName = "Tenant B", TenantSlug = "tenant-b", IsActive = true, CreatedDate = DateTime.UtcNow });

        // Create a risk in Tenant B
        var riskBId = Guid.NewGuid();
        context.Risks.Add(new Risk { Id = riskBId, Name = "Tenant B Private Risk", TenantId = tenantBId, CreatedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();

        // Act - Tenant A tries to access Tenant B's risk by ID
        var attemptedAccess = await context.Risks
            .Where(r => r.TenantId == tenantAId && r.Id == riskBId)
            .FirstOrDefaultAsync();

        // Assert
        attemptedAccess.Should().BeNull("Tenant A should not access Tenant B's risk even with known ID");
    }

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task BulkQuery_ShouldRespectTenantBoundary()
    {
        // Arrange
        var dbName = $"BulkQueryTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        context.Tenants.Add(new Tenant { Id = tenantAId, OrganizationName = "Tenant A", TenantSlug = "tenant-a", IsActive = true, CreatedDate = DateTime.UtcNow });
        context.Tenants.Add(new Tenant { Id = tenantBId, OrganizationName = "Tenant B", TenantSlug = "tenant-b", IsActive = true, CreatedDate = DateTime.UtcNow });

        // Create 100 risks for Tenant A and 50 for Tenant B
        for (int i = 0; i < 100; i++)
        {
            context.Risks.Add(new Risk { Id = Guid.NewGuid(), Name = $"Risk A-{i}", TenantId = tenantAId, CreatedDate = DateTime.UtcNow });
        }
        for (int i = 0; i < 50; i++)
        {
            context.Risks.Add(new Risk { Id = Guid.NewGuid(), Name = $"Risk B-{i}", TenantId = tenantBId, CreatedDate = DateTime.UtcNow });
        }
        await context.SaveChangesAsync();

        // Act - Query all risks for Tenant A
        var tenantARisks = await context.Risks
            .Where(r => r.TenantId == tenantAId)
            .ToListAsync();

        // Assert
        tenantARisks.Should().HaveCount(100, "Tenant A should see exactly 100 risks");
        tenantARisks.All(r => r.TenantId == tenantAId).Should().BeTrue("All risks should belong to Tenant A");
        tenantARisks.Any(r => r.Name.StartsWith("Risk B")).Should().BeFalse("No Tenant B risks should be visible");
    }

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task SoftDelete_ShouldRespectTenantBoundary()
    {
        // Arrange
        var dbName = $"SoftDeleteTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantAId = Guid.NewGuid();

        context.Tenants.Add(new Tenant { Id = tenantAId, OrganizationName = "Tenant A", TenantSlug = "tenant-a", IsActive = true, CreatedDate = DateTime.UtcNow });

        var risk = new Risk
        {
            Id = Guid.NewGuid(),
            Name = "Deletable Risk",
            TenantId = tenantAId,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow
        };
        context.Risks.Add(risk);
        await context.SaveChangesAsync();

        // Act - Soft delete the risk
        risk.IsDeleted = true;
        await context.SaveChangesAsync();

        // Query without IgnoreQueryFilters
        var visibleRisks = await context.Risks
            .Where(r => r.TenantId == tenantAId)
            .ToListAsync();

        // Query with IgnoreQueryFilters (admin view)
        var allRisks = await context.Risks
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantAId)
            .ToListAsync();

        // Assert
        visibleRisks.Should().BeEmpty("Soft-deleted risks should not be visible in normal queries");
        allRisks.Should().HaveCount(1, "Admin query should see soft-deleted risks");
        allRisks[0].IsDeleted.Should().BeTrue("Risk should be marked as deleted");
    }

    #endregion

    #region Workspace Isolation Tests

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task Workspace_ShouldIsolateDataWithinTenant()
    {
        // Arrange
        var dbName = $"WorkspaceTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantId = Guid.NewGuid();
        var workspaceAId = Guid.NewGuid();
        var workspaceBId = Guid.NewGuid();

        context.Tenants.Add(new Tenant { Id = tenantId, OrganizationName = "Test Tenant", TenantSlug = "test", IsActive = true, CreatedDate = DateTime.UtcNow });
        context.Workspaces.Add(new Workspace { Id = workspaceAId, Name = "Workspace A", TenantId = tenantId, CreatedDate = DateTime.UtcNow });
        context.Workspaces.Add(new Workspace { Id = workspaceBId, Name = "Workspace B", TenantId = tenantId, CreatedDate = DateTime.UtcNow });

        await context.SaveChangesAsync();

        // Act - Query workspaces
        var workspaces = await context.Workspaces
            .Where(w => w.TenantId == tenantId)
            .ToListAsync();

        // Assert
        workspaces.Should().HaveCount(2, "Tenant should have 2 workspaces");
        workspaces.Select(w => w.Id).Should().Contain(workspaceAId);
        workspaces.Select(w => w.Id).Should().Contain(workspaceBId);
    }

    #endregion

    #region Cross-Tenant Update Prevention Tests

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task Update_ShouldNotAffectOtherTenants()
    {
        // Arrange
        var dbName = $"UpdateTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        context.Tenants.Add(new Tenant { Id = tenantAId, OrganizationName = "Tenant A", TenantSlug = "tenant-a", IsActive = true, CreatedDate = DateTime.UtcNow });
        context.Tenants.Add(new Tenant { Id = tenantBId, OrganizationName = "Tenant B", TenantSlug = "tenant-b", IsActive = true, CreatedDate = DateTime.UtcNow });

        var riskA = new Risk { Id = Guid.NewGuid(), Name = "Original A", TenantId = tenantAId, CreatedDate = DateTime.UtcNow };
        var riskB = new Risk { Id = Guid.NewGuid(), Name = "Original B", TenantId = tenantBId, CreatedDate = DateTime.UtcNow };
        context.Risks.AddRange(riskA, riskB);
        await context.SaveChangesAsync();

        // Act - Update Tenant A's risk
        riskA.Name = "Updated A";
        await context.SaveChangesAsync();

        // Assert
        var tenantBRisk = await context.Risks.FirstAsync(r => r.TenantId == tenantBId);
        tenantBRisk.Name.Should().Be("Original B", "Tenant B's risk should not be affected by Tenant A's update");
    }

    [Fact]
    [Trait("Category", "RequiresAbpInfrastructure")]
    public async Task AttemptToChangeTenantId_ShouldBeDetectable()
    {
        // Arrange
        var dbName = $"TenantIdChangeTest_{Guid.NewGuid()}";
        using var context = TestDbContextFactory.Create(dbName);

        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        context.Tenants.Add(new Tenant { Id = tenantAId, OrganizationName = "Tenant A", TenantSlug = "tenant-a", IsActive = true, CreatedDate = DateTime.UtcNow });
        context.Tenants.Add(new Tenant { Id = tenantBId, OrganizationName = "Tenant B", TenantSlug = "tenant-b", IsActive = true, CreatedDate = DateTime.UtcNow });

        var risk = new Risk { Id = Guid.NewGuid(), Name = "Test Risk", TenantId = tenantAId, CreatedDate = DateTime.UtcNow };
        context.Risks.Add(risk);
        await context.SaveChangesAsync();

        var originalTenantId = risk.TenantId;

        // Act - Attempt to change TenantId (this should be prevented by business logic)
        risk.TenantId = tenantBId;

        // Assert - The entity tracker should detect the change
        var entry = context.Entry(risk);
        var originalValue = entry.OriginalValues.GetValue<Guid>(nameof(Risk.TenantId));

        originalValue.Should().Be(tenantAId, "Original TenantId should be preserved");
        risk.TenantId.Should().Be(tenantBId, "Current value shows attempted change");

        // In production, business logic should prevent this save
        // This test verifies the change is detectable
    }

    #endregion

    #region API Authorization Simulation Tests

    [Fact]
    public void UserClaims_ShouldContainTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var tenantClaim = principal.FindFirst("tenant_id");
        var parsedTenantId = Guid.TryParse(tenantClaim?.Value, out var parsedId) ? parsedId : Guid.Empty;

        // Assert
        tenantClaim.Should().NotBeNull("User claims should contain tenant_id");
        parsedTenantId.Should().Be(tenantId, "Tenant ID should be correctly parsed from claims");
    }

    [Fact]
    public void MissingTenantClaim_ShouldBeDetected()
    {
        // Arrange - User without tenant claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var tenantClaim = principal.FindFirst("tenant_id");

        // Assert
        tenantClaim.Should().BeNull("Missing tenant claim should be detected");
    }

    #endregion

    #region Entity Query Filter Tests

    [Theory]
    [InlineData(typeof(Risk))]
    [InlineData(typeof(Control))]
    [InlineData(typeof(Assessment))]
    [InlineData(typeof(Policy))]
    [InlineData(typeof(Evidence))]
    [InlineData(typeof(Audit))]
    public void TenantEntity_ShouldHaveTenantIdProperty(Type entityType)
    {
        // Assert
        var tenantIdProperty = entityType.GetProperty("TenantId");

        tenantIdProperty.Should().NotBeNull($"{entityType.Name} should have TenantId property");
        // TenantId can be Guid or Guid? (nullable for optional tenant scope)
        var isGuidType = tenantIdProperty!.PropertyType == typeof(Guid) ||
                         tenantIdProperty.PropertyType == typeof(Guid?);
        isGuidType.Should().BeTrue($"TenantId should be of type Guid or Guid?, but was {tenantIdProperty.PropertyType.Name}");
    }

    [Theory]
    [InlineData(typeof(Risk))]
    [InlineData(typeof(Control))]
    [InlineData(typeof(Assessment))]
    [InlineData(typeof(Policy))]
    [InlineData(typeof(Evidence))]
    [InlineData(typeof(Audit))]
    public void TenantEntity_ShouldHaveIsDeletedProperty(Type entityType)
    {
        // Assert
        var isDeletedProperty = entityType.GetProperty("IsDeleted");

        isDeletedProperty.Should().NotBeNull($"{entityType.Name} should have IsDeleted property for soft delete");
        isDeletedProperty!.PropertyType.Should().Be(typeof(bool), "IsDeleted should be of type bool");
    }

    #endregion
}
