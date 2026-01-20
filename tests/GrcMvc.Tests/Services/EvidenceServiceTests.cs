using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Application.Policy;
using GrcMvc.Data;
using GrcMvc.Models.DTOs;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Unit tests for EvidenceService - CRUD operations, policy enforcement, and statistics
/// </summary>
public class EvidenceServiceTests : IDisposable
{
    private readonly Mock<IDbContextFactory<GrcDbContext>> _mockContextFactory;
    private readonly Mock<ILogger<EvidenceService>> _mockLogger;
    private readonly Mock<PolicyEnforcementHelper> _mockPolicyHelper;
    private readonly Mock<IWorkspaceContextService> _mockWorkspaceContext;
    private readonly GrcDbContext _context;
    private readonly EvidenceService _service;

    public EvidenceServiceTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<GrcDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new GrcDbContext(options);

        _mockContextFactory = new Mock<IDbContextFactory<GrcDbContext>>();
        _mockContextFactory.Setup(f => f.CreateDbContext()).Returns(_context);

        _mockLogger = new Mock<ILogger<EvidenceService>>();

        // Create minimal mock for PolicyEnforcementHelper
        _mockPolicyHelper = new Mock<PolicyEnforcementHelper>(
            Mock.Of<IPolicyEnforcer>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IWebHostEnvironment>(),
            Mock.Of<ILogger<PolicyEnforcementHelper>>());

        _mockWorkspaceContext = new Mock<IWorkspaceContextService>();
        _mockWorkspaceContext.Setup(w => w.HasWorkspaceContext()).Returns(false);

        _service = new EvidenceService(
            _mockContextFactory.Object,
            _mockLogger.Object,
            _mockPolicyHelper.Object,
            _mockWorkspaceContext.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithNoEvidences_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithEvidences_ReturnsAllEvidences()
    {
        // Arrange
        SeedTestEvidences(3);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_MapsPropertiesToDto()
    {
        // Arrange
        var evidence = new Evidence
        {
            Id = Guid.NewGuid(),
            Title = "Test Evidence",
            Description = "Test Description",
            Type = "Document",
            CollectionDate = DateTime.UtcNow,
            VerificationStatus = "Pending",
            CollectedBy = "test-user",
            FilePath = "/path/to/file",
            Comments = "Test notes"
        };
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllAsync()).First();

        // Assert
        Assert.Equal(evidence.Id, result.Id);
        Assert.Equal("Test Evidence", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("Document", result.EvidenceType);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("test-user", result.Owner);
        Assert.Equal("/path/to/file", result.Location);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEvidence()
    {
        // Arrange
        var evidence = CreateTestEvidence();
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(evidence.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(evidence.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _service.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesEvidence()
    {
        // Arrange
        var dto = new CreateEvidenceDto
        {
            Name = "New Evidence",
            Description = "Test description",
            EvidenceType = "Document",
            CollectionDate = DateTime.UtcNow,
            Owner = "test-user",
            DataClassification = "internal"
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Evidence", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_GeneratesEvidenceNumber()
    {
        // Arrange
        var dto = new CreateEvidenceDto
        {
            Name = "Test Evidence",
            DataClassification = "internal"
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.NotNull(evidence?.EvidenceNumber);
        Assert.StartsWith("EV-", evidence.EvidenceNumber);
    }

    [Fact]
    public async Task CreateAsync_SetsDefaultStatusToPending()
    {
        // Arrange
        var dto = new CreateEvidenceDto
        {
            Name = "Test Evidence",
            DataClassification = "internal"
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateEvidenceDto
        {
            Name = "",
            DataClassification = "internal"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithWorkspaceContext_SetsWorkspaceId()
    {
        // Arrange
        var workspaceId = Guid.NewGuid();
        _mockWorkspaceContext.Setup(w => w.HasWorkspaceContext()).Returns(true);
        _mockWorkspaceContext.Setup(w => w.GetCurrentWorkspaceId()).Returns(workspaceId);

        var dto = new CreateEvidenceDto
        {
            Name = "Test Evidence",
            DataClassification = "internal"
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        var evidence = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == result.Id);
        Assert.Equal(workspaceId, evidence?.WorkspaceId);
    }

    [Fact]
    public async Task CreateAsync_EnforcesPolicyBeforeSaving()
    {
        // Arrange
        var dto = new CreateEvidenceDto
        {
            Name = "Test Evidence",
            DataClassification = "confidential",
            Owner = "test-owner"
        };

        // Act
        await _service.CreateAsync(dto);

        // Assert
        _mockPolicyHelper.Verify(
            p => p.EnforceCreateAsync(
                "Evidence",
                It.IsAny<Evidence>(),
                "confidential",
                "test-owner"),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidId_UpdatesEvidence()
    {
        // Arrange
        var evidence = CreateTestEvidence();
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEvidenceDto
        {
            Name = "Updated Title",
            Description = "Updated Description"
        };

        // Act
        var result = await _service.UpdateAsync(evidence.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Name);
        Assert.Equal("Updated Description", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_PreservesUnchangedFields()
    {
        // Arrange
        var evidence = CreateTestEvidence();
        evidence.Type = "Original Type";
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateEvidenceDto
        {
            Name = "Updated Title"
            // Type is not set, should be preserved
        };

        // Act
        var result = await _service.UpdateAsync(evidence.Id, updateDto);

        // Assert
        Assert.Equal("Original Type", result?.EvidenceType);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new UpdateEvidenceDto { Name = "Updated" };

        // Act
        var result = await _service.UpdateAsync(nonExistentId, updateDto);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_RemovesEvidence()
    {
        // Arrange
        var evidence = CreateTestEvidence();
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(evidence.Id);

        // Assert
        var deleted = await _context.Evidences.FirstOrDefaultAsync(e => e.Id == evidence.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_DoesNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - Should complete without throwing
        await _service.DeleteAsync(nonExistentId);
    }

    #endregion

    #region GetByTypeAsync Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsEvidencesOfType()
    {
        // Arrange
        _context.Evidences.AddRange(
            CreateTestEvidence(type: "Document"),
            CreateTestEvidence(type: "Document"),
            CreateTestEvidence(type: "Screenshot"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByTypeAsync("Document");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.Equal("Document", e.EvidenceType));
    }

    [Fact]
    public async Task GetByTypeAsync_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        _context.Evidences.Add(CreateTestEvidence(type: "Document"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByTypeAsync("Video");

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetExpiringEvidencesAsync Tests

    [Fact]
    public async Task GetExpiringEvidencesAsync_ReturnsExpiringEvidences()
    {
        // Arrange
        var expiringEvidence = CreateTestEvidence();
        expiringEvidence.VerificationDate = DateTime.UtcNow.AddDays(5);

        var notExpiringEvidence = CreateTestEvidence();
        notExpiringEvidence.VerificationDate = DateTime.UtcNow.AddDays(60);

        _context.Evidences.AddRange(expiringEvidence, notExpiringEvidence);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetExpiringEvidencesAsync(30);

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetByAuditIdAsync Tests

    [Fact]
    public async Task GetByAuditIdAsync_ReturnsEvidencesForAudit()
    {
        // Arrange
        var auditId = Guid.NewGuid();
        var evidence1 = CreateTestEvidence();
        evidence1.AuditId = auditId;
        var evidence2 = CreateTestEvidence();
        evidence2.AuditId = auditId;
        var evidence3 = CreateTestEvidence();
        evidence3.AuditId = Guid.NewGuid();

        _context.Evidences.AddRange(evidence1, evidence2, evidence3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByAuditIdAsync(auditId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Fact]
    public async Task GetStatisticsAsync_CalculatesTotalEvidences()
    {
        // Arrange
        SeedTestEvidences(5);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.Equal(5, result.TotalEvidences);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesStatusDistribution()
    {
        // Arrange
        _context.Evidences.AddRange(
            CreateTestEvidence(status: "Verified"),
            CreateTestEvidence(status: "Verified"),
            CreateTestEvidence(status: "Pending"),
            CreateTestEvidence(status: "Rejected"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.Equal(2, result.ActiveEvidences); // Verified count
        Assert.Equal(1, result.ExpiredEvidences); // Rejected count
        Assert.Contains("Verified", result.StatusDistribution.Keys);
        Assert.Equal(2, result.StatusDistribution["Verified"]);
    }

    [Fact]
    public async Task GetStatisticsAsync_CalculatesEvidencesByType()
    {
        // Arrange
        _context.Evidences.AddRange(
            CreateTestEvidence(type: "Document"),
            CreateTestEvidence(type: "Document"),
            CreateTestEvidence(type: "Screenshot"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.Equal(2, result.EvidencesByType["Document"]);
        Assert.Equal(1, result.EvidencesByType["Screenshot"]);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithNoEvidences_ReturnsZeroCounts()
    {
        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.Equal(0, result.TotalEvidences);
        Assert.Equal(0, result.ActiveEvidences);
        Assert.Empty(result.StatusDistribution);
    }

    #endregion

    #region Helper Methods

    private Evidence CreateTestEvidence(string? type = null, string? status = null)
    {
        return new Evidence
        {
            Id = Guid.NewGuid(),
            Title = "Test Evidence",
            Description = "Test Description",
            Type = type ?? "Document",
            CollectionDate = DateTime.UtcNow,
            VerificationStatus = status ?? "Pending",
            CollectedBy = "test-user",
            FilePath = "/test/path",
            Comments = "Test notes",
            EvidenceNumber = $"EV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}"
        };
    }

    private void SeedTestEvidences(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _context.Evidences.Add(CreateTestEvidence());
        }
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #endregion
}

// Stub interfaces for compilation (these would normally come from the project)
public interface IHttpContextAccessor { }
public interface IWebHostEnvironment { }
