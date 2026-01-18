using FluentAssertions;
using GrcMvc.Common.Results;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using GrcMvc.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Unit tests for SerialCodeService validating the Result pattern refactor.
/// Tests all 7 refactored methods: GenerateAsync, Parse, CreateNewVersionAsync,
/// ConfirmReservationAsync, CancelReservationAsync, VoidAsync, GetTraceabilityReportAsync.
/// </summary>
public class SerialCodeServiceTests : IDisposable
{
    private readonly GrcDbContext _context;
    private readonly SerialCodeService _service;
    private readonly Mock<ILogger<SerialCodeService>> _loggerMock;

    public SerialCodeServiceTests()
    {
        _context = TestDbContextFactory.Create();
        _loggerMock = new Mock<ILogger<SerialCodeService>>();
        _service = new SerialCodeService(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GenerateAsync Tests

    [Theory]
    [InlineData("abc")]      // lowercase not allowed
    [InlineData("ab")]       // too short (< 3 chars)
    [InlineData("ABCDEFGH")] // too long (> 6 chars)
    [InlineData("TEST")]     // reserved code
    [InlineData("SYS")]      // reserved code
    [InlineData("ADM")]      // reserved code
    public async Task GenerateAsync_WithInvalidTenantCode_ReturnsValidationError(string invalidTenantCode)
    {
        // Arrange
        var request = new SerialCodeRequest
        {
            EntityType = "assessment",
            TenantCode = invalidTenantCode
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
    }

    [Fact]
    public async Task GenerateAsync_WithValidRequest_ReturnsSuccessAndCreatesRecords()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var request = new SerialCodeRequest
        {
            EntityType = "assessment",
            TenantCode = "ACME",
            EntityId = entityId,
            Stage = 1,
            Year = 2026,
            CreatedBy = "TestUser"
        };

        // Act
        var result = await _service.GenerateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().MatchRegex(@"^ASM-ACME-01-2026-\d{6}-01$");
        result.Value.Prefix.Should().Be("ASM");
        result.Value.TenantCode.Should().Be("ACME");
        result.Value.Stage.Should().Be(1);
        result.Value.Year.Should().Be(2026);
        result.Value.Sequence.Should().Be(1);
        result.Value.Version.Should().Be(1);
        result.Value.CreatedBy.Should().Be("TestUser");
        result.Value.EntityId.Should().Be(entityId);

        // Verify DB side-effects
        var registry = await _context.Set<SerialCodeRegistry>()
            .FirstOrDefaultAsync(r => r.Code == result.Value.Code);
        registry.Should().NotBeNull();
        registry!.Status.Should().Be("active");
        registry.EntityId.Should().Be(entityId);

        var counter = await _context.Set<SerialSequenceCounter>()
            .FirstOrDefaultAsync(c => c.Prefix == "ASM" && c.TenantCode == "ACME" && c.Stage == 1 && c.Year == 2026);
        counter.Should().NotBeNull();
        counter!.CurrentSequence.Should().Be(1);
    }

    [Fact]
    public async Task GenerateAsync_CalledTwice_IncrementsSequence()
    {
        // Arrange
        var request = new SerialCodeRequest
        {
            EntityType = "risk",
            TenantCode = "ACME",
            Stage = 2,
            Year = 2026
        };

        // Act
        var result1 = await _service.GenerateAsync(request);
        var result2 = await _service.GenerateAsync(request);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value!.Sequence.Should().Be(1);
        result2.Value!.Sequence.Should().Be(2);
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("")]
    [InlineData("BAD")]
    [InlineData("INVALID-FORMAT")]
    [InlineData("ASM-ACME-01-2026")]           // missing sequence and version
    [InlineData("ASM-ACME-01-2026-000001")]    // missing version
    [InlineData("ASM-acme-01-2026-000001-01")] // lowercase tenant
    public void Parse_WithInvalidCodeFormat_ReturnsValidationError(string invalidCode)
    {
        // Act
        var result = _service.Parse(invalidCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
    }

    [Theory]
    [InlineData("ASM-ACME-01-2026-000142-01", "ASM", "ACME", 1, 2026, 142, 1)]
    [InlineData("RSK-TEST1-02-2025-000001-05", "RSK", "TEST1", 2, 2025, 1, 5)]
    [InlineData("CTL-ABC123-00-2024-999999-99", "CTL", "ABC123", 0, 2024, 999999, 99)]
    public void Parse_WithValidCode_ReturnsSuccessWithParsedComponents(
        string code, string expectedPrefix, string expectedTenant,
        int expectedStage, int expectedYear, int expectedSequence, int expectedVersion)
    {
        // Act
        var result = _service.Parse(code);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Prefix.Should().Be(expectedPrefix);
        result.Value.TenantCode.Should().Be(expectedTenant);
        result.Value.Stage.Should().Be(expectedStage);
        result.Value.Year.Should().Be(expectedYear);
        result.Value.Sequence.Should().Be(expectedSequence);
        result.Value.Version.Should().Be(expectedVersion);
        result.Value.IsValid.Should().BeTrue();
    }

    #endregion

    #region CreateNewVersionAsync Tests

    [Fact]
    public async Task CreateNewVersionAsync_WithNonExistentCode_ReturnsNotFound()
    {
        // Arrange
        var nonExistentCode = "ASM-ACME-01-2026-000999-01";

        // Act
        var result = await _service.CreateNewVersionAsync(nonExistentCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task CreateNewVersionAsync_WithVersionAtLimit_ReturnsInvalidOperation()
    {
        // Arrange - seed a record with version 99
        var code = "ASM-ACME-01-2026-000001-99";
        var registry = new SerialCodeRegistry
        {
            Code = code,
            Prefix = "ASM",
            TenantCode = "ACME",
            Stage = 1,
            Year = 2026,
            Sequence = 1,
            Version = 99,
            EntityType = "assessment",
            EntityId = Guid.NewGuid(),
            Status = "active",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Set<SerialCodeRegistry>().Add(registry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CreateNewVersionAsync(code);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Contain("Maximum version");
    }

    [Fact]
    public async Task CreateNewVersionAsync_WithValidCode_ReturnsSuccessAndUpdatesRecords()
    {
        // Arrange - seed an active v1 record
        var originalCode = "RSK-ACME-02-2026-000001-01";
        var entityId = Guid.NewGuid();
        var registry = new SerialCodeRegistry
        {
            Code = originalCode,
            Prefix = "RSK",
            TenantCode = "ACME",
            Stage = 2,
            Year = 2026,
            Sequence = 1,
            Version = 1,
            EntityType = "risk",
            EntityId = entityId,
            Status = "active",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Set<SerialCodeRegistry>().Add(registry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CreateNewVersionAsync(originalCode, "Updated for testing");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be(2);
        result.Value.Code.Should().Be("RSK-ACME-02-2026-000001-02");

        // Verify old record is superseded
        var oldRecord = await _context.Set<SerialCodeRegistry>()
            .FirstOrDefaultAsync(r => r.Code == originalCode);
        oldRecord.Should().NotBeNull();
        oldRecord!.Status.Should().Be("superseded");
        oldRecord.StatusReason.Should().Contain("Updated for testing");

        // Verify new record
        var newRecord = await _context.Set<SerialCodeRegistry>()
            .FirstOrDefaultAsync(r => r.Code == result.Value.Code);
        newRecord.Should().NotBeNull();
        newRecord!.Status.Should().Be("active");
        newRecord.Version.Should().Be(2);
        newRecord.PreviousVersionCode.Should().Be(originalCode);
        newRecord.EntityId.Should().Be(entityId);
    }

    #endregion

    #region ConfirmReservationAsync Tests

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("")]
    public async Task ConfirmReservationAsync_WithInvalidGuid_ReturnsValidationError(string invalidGuid)
    {
        // Act
        var result = await _service.ConfirmReservationAsync(invalidGuid, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithNonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var randomGuid = Guid.NewGuid().ToString();

        // Act
        var result = await _service.ConfirmReservationAsync(randomGuid, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    [Theory]
    [InlineData("confirmed")]
    [InlineData("expired")]
    [InlineData("cancelled")]
    public async Task ConfirmReservationAsync_WithWrongStatus_ReturnsInvalidOperation(string wrongStatus)
    {
        // Arrange
        var reservation = new SerialCodeReservation
        {
            Id = Guid.NewGuid(),
            ReservedCode = "CTL-ACME-00-2026-000001-01",
            Prefix = "CTL",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Status = wrongStatus,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedBy = "System"
        };
        _context.Set<SerialCodeReservation>().Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConfirmReservationAsync(reservation.Id.ToString(), Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithExpiredReservation_ReturnsInvalidOperationAndUpdatesStatus()
    {
        // Arrange - reservation expired 1 hour ago
        var reservation = new SerialCodeReservation
        {
            Id = Guid.NewGuid(),
            ReservedCode = "EVD-ACME-00-2026-000001-01",
            Prefix = "EVD",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Status = "reserved",
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // expired
            CreatedBy = "System"
        };
        _context.Set<SerialCodeReservation>().Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConfirmReservationAsync(reservation.Id.ToString(), Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Contain("expired");

        // Verify reservation status updated to expired
        var updatedReservation = await _context.Set<SerialCodeReservation>()
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);
        updatedReservation.Should().NotBeNull();
        updatedReservation!.Status.Should().Be("expired");
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithValidReservation_ReturnsSuccessAndCreatesRegistry()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var reservation = new SerialCodeReservation
        {
            Id = Guid.NewGuid(),
            ReservedCode = "POL-ACME-00-2026-000001-01",
            Prefix = "POL",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Status = "reserved",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30), // valid
            CreatedBy = "TestUser"
        };
        _context.Set<SerialCodeReservation>().Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConfirmReservationAsync(reservation.Id.ToString(), entityId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be("POL-ACME-00-2026-000001-01");
        result.Value.EntityId.Should().Be(entityId);

        // Verify reservation is confirmed
        var updatedReservation = await _context.Set<SerialCodeReservation>()
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);
        updatedReservation.Should().NotBeNull();
        updatedReservation!.Status.Should().Be("confirmed");
        updatedReservation.ConfirmedAt.Should().NotBeNull();

        // Verify registry created
        var registry = await _context.Set<SerialCodeRegistry>()
            .FirstOrDefaultAsync(r => r.Code == reservation.ReservedCode);
        registry.Should().NotBeNull();
        registry!.Status.Should().Be("active");
        registry.EntityId.Should().Be(entityId);
    }

    #endregion

    #region CancelReservationAsync Tests

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("invalid")]
    [InlineData("")]
    public async Task CancelReservationAsync_WithInvalidGuid_ReturnsValidationError(string invalidGuid)
    {
        // Act
        var result = await _service.CancelReservationAsync(invalidGuid);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.ValidationError);
    }

    [Fact]
    public async Task CancelReservationAsync_WithNonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var randomGuid = Guid.NewGuid().ToString();

        // Act
        var result = await _service.CancelReservationAsync(randomGuid);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    [Theory]
    [InlineData("confirmed")]
    [InlineData("expired")]
    [InlineData("cancelled")]
    public async Task CancelReservationAsync_WithWrongStatus_ReturnsInvalidOperation(string wrongStatus)
    {
        // Arrange
        var reservation = new SerialCodeReservation
        {
            Id = Guid.NewGuid(),
            ReservedCode = "AUD-ACME-00-2026-000001-01",
            Prefix = "AUD",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Status = wrongStatus,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedBy = "System"
        };
        _context.Set<SerialCodeReservation>().Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CancelReservationAsync(reservation.Id.ToString());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
    }

    [Fact]
    public async Task CancelReservationAsync_WithValidReservation_ReturnsSuccessAndUpdatesCancelledAt()
    {
        // Arrange
        var reservation = new SerialCodeReservation
        {
            Id = Guid.NewGuid(),
            ReservedCode = "WFL-ACME-00-2026-000001-01",
            Prefix = "WFL",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Status = "reserved",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedBy = "TestUser"
        };
        _context.Set<SerialCodeReservation>().Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CancelReservationAsync(reservation.Id.ToString());

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify reservation is cancelled
        var updatedReservation = await _context.Set<SerialCodeReservation>()
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);
        updatedReservation.Should().NotBeNull();
        updatedReservation!.Status.Should().Be("cancelled");
        updatedReservation.CancelledAt.Should().NotBeNull();
    }

    #endregion

    #region VoidAsync Tests

    [Fact]
    public async Task VoidAsync_WithNonExistentCode_ReturnsNotFound()
    {
        // Arrange
        var nonExistentCode = "ASM-ACME-01-2026-000999-01";

        // Act
        var result = await _service.VoidAsync(nonExistentCode, "Test reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task VoidAsync_WithAlreadyVoidCode_ReturnsInvalidOperation()
    {
        // Arrange
        var code = "CMP-ACME-03-2026-000001-01";
        var registry = new SerialCodeRegistry
        {
            Code = code,
            Prefix = "CMP",
            TenantCode = "ACME",
            Stage = 3,
            Year = 2026,
            Sequence = 1,
            Version = 1,
            EntityType = "compliance",
            EntityId = Guid.NewGuid(),
            Status = "void", // already void
            StatusReason = "Previous void reason",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Set<SerialCodeRegistry>().Add(registry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VoidAsync(code, "Another reason");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.InvalidOperation);
        result.Error.Message.Should().Contain("already void");
    }

    [Fact]
    public async Task VoidAsync_WithValidCode_ReturnsSuccessAndUpdatesStatus()
    {
        // Arrange
        var code = "RES-ACME-04-2026-000001-01";
        var registry = new SerialCodeRegistry
        {
            Code = code,
            Prefix = "RES",
            TenantCode = "ACME",
            Stage = 4,
            Year = 2026,
            Sequence = 1,
            Version = 1,
            EntityType = "resilience",
            EntityId = Guid.NewGuid(),
            Status = "active",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Set<SerialCodeRegistry>().Add(registry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VoidAsync(code, "Voided for testing");

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify record is voided
        var updatedRegistry = await _context.Set<SerialCodeRegistry>()
            .FirstOrDefaultAsync(r => r.Code == code);
        updatedRegistry.Should().NotBeNull();
        updatedRegistry!.Status.Should().Be("void");
        updatedRegistry.StatusReason.Should().Be("Voided for testing");
    }

    #endregion

    #region GetTraceabilityReportAsync Tests

    [Fact]
    public async Task GetTraceabilityReportAsync_WithNonExistentCode_ReturnsNotFound()
    {
        // Arrange
        var nonExistentCode = "EXC-ACME-05-2026-000999-01";

        // Act
        var result = await _service.GetTraceabilityReportAsync(nonExistentCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task GetTraceabilityReportAsync_WithValidCode_ReturnsSuccessWithReport()
    {
        // Arrange - seed version 1 and version 2 of the same base code
        var entityId = Guid.NewGuid();
        var codeV1 = "SUS-ACME-06-2026-000001-01";
        var codeV2 = "SUS-ACME-06-2026-000001-02";

        var registryV1 = new SerialCodeRegistry
        {
            Code = codeV1,
            Prefix = "SUS",
            TenantCode = "ACME",
            Stage = 6,
            Year = 2026,
            Sequence = 1,
            Version = 1,
            EntityType = "sustainability",
            EntityId = entityId,
            Status = "superseded",
            StatusReason = "Superseded by v2",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        var registryV2 = new SerialCodeRegistry
        {
            Code = codeV2,
            Prefix = "SUS",
            TenantCode = "ACME",
            Stage = 6,
            Year = 2026,
            Sequence = 1,
            Version = 2,
            EntityType = "sustainability",
            EntityId = entityId,
            Status = "active",
            PreviousVersionCode = codeV1,
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Set<SerialCodeRegistry>().AddRange(registryV1, registryV2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTraceabilityReportAsync(codeV2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CurrentCode.Should().Be(codeV2);
        result.Value.EntityType.Should().Be("sustainability");
        result.Value.EntityId.Should().Be(entityId);
        result.Value.VersionHistory.Should().HaveCount(2);
        result.Value.VersionHistory.Should().BeInAscendingOrder(v => v.Version);
        result.Value.VersionHistory[0].Code.Should().Be(codeV1);
        result.Value.VersionHistory[0].Version.Should().Be(1);
        result.Value.VersionHistory[1].Code.Should().Be(codeV2);
        result.Value.VersionHistory[1].Version.Should().Be(2);
    }

    [Fact]
    public async Task GetTraceabilityReportAsync_WithRelatedCodes_IncludesRelations()
    {
        // Arrange - seed one code and a related code with same entityId
        var entityId = Guid.NewGuid();
        var mainCode = "CTL-ACME-00-2026-000001-01";
        var relatedCode = "EVD-ACME-00-2026-000001-01";

        var mainRegistry = new SerialCodeRegistry
        {
            Code = mainCode,
            Prefix = "CTL",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Version = 1,
            EntityType = "control",
            EntityId = entityId,
            Status = "active",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var relatedRegistry = new SerialCodeRegistry
        {
            Code = relatedCode,
            Prefix = "EVD",
            TenantCode = "ACME",
            Stage = 0,
            Year = 2026,
            Sequence = 1,
            Version = 1,
            EntityType = "evidence",
            EntityId = entityId, // same entity
            Status = "active",
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Set<SerialCodeRegistry>().AddRange(mainRegistry, relatedRegistry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTraceabilityReportAsync(mainCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.RelatedCodes.Should().HaveCount(1);
        result.Value.RelatedCodes[0].Code.Should().Be(relatedCode);
        result.Value.RelatedCodes[0].EntityType.Should().Be("evidence");
    }

    #endregion
}
