# Risk Module - Code Snippets & Common Tasks

**Document Date:** January 10, 2026
**Purpose:** Ready-to-use code examples for developers
**Language:** C# (.NET 8)

---

## 📋 Table of Contents

1. [Service Layer Integration](#service-layer-integration)
2. [Controller Examples](#controller-examples)
3. [Validation Examples](#validation-examples)
4. [Workflow Implementation](#workflow-implementation)
5. [Repository Patterns](#repository-patterns)
6. [Testing Examples](#testing-examples)

---

## 1. Service Layer Integration

### 1.1 Inject Risk Service

```csharp
using GrcMvc.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class MyController : ControllerBase
{
    private readonly IRiskService _riskService;
    private readonly ILogger<MyController> _logger;

    public MyController(
        IRiskService riskService,
        ILogger<MyController> logger)
    {
        _riskService = riskService;
        _logger = logger;
    }
}
```

---

### 1.2 Create a New Risk

```csharp
public async Task<IActionResult> CreateVendorRisk(string vendorId)
{
    var createDto = new CreateRiskDto
    {
        Name = $"Vendor Risk - {vendorId}",
        Description = "Third-party vendor security assessment required",
        Category = "Vendor Risk",
        Probability = 3,
        Impact = 4,
        Status = "Identified",
        Owner = User.Identity?.Name ?? "system",
        MitigationStrategy = "Conduct security questionnaire and review SOC 2 report",
        DueDate = DateTime.UtcNow.AddDays(90)
    };

    var result = await _riskService.CreateAsync(createDto);

    if (result.IsSuccess)
    {
        _logger.LogInformation("Risk created: {RiskId}", result.Value.Id);
        return Ok(result.Value);
    }

    return BadRequest(result.Error);
}
```

---

### 1.3 Update Risk with Validation

```csharp
public async Task<IActionResult> UpdateRiskScore(Guid riskId, int newProbability, int newImpact)
{
    // Get existing risk
    var result = await _riskService.GetByIdAsync(riskId);
    if (result.IsFailure)
        return NotFound();

    var risk = result.Value;

    // Update DTO
    var updateDto = new UpdateRiskDto
    {
        Name = risk.Name,
        Description = risk.Description,
        Category = risk.Category,
        Probability = newProbability,  // Updated
        Impact = newImpact,             // Updated
        RiskScore = newProbability * newImpact,  // Auto-calculated
        Status = risk.Status,
        Owner = risk.Owner,
        MitigationStrategy = risk.MitigationStrategy
    };

    var updateResult = await _riskService.UpdateAsync(riskId, updateDto);

    if (updateResult.IsSuccess)
        return Ok(updateResult.Value);

    return BadRequest(updateResult.Error);
}
```

---

### 1.4 Get Filtered Risks

```csharp
public async Task<IActionResult> GetCriticalRisks()
{
    var allRisksResult = await _riskService.GetAllAsync();

    if (allRisksResult.IsFailure)
        return BadRequest(allRisksResult.Error);

    var criticalRisks = allRisksResult.Value
        .Where(r => r.RiskScore >= 20)
        .OrderByDescending(r => r.RiskScore)
        .ToList();

    return Ok(new
    {
        Count = criticalRisks.Count,
        Risks = criticalRisks
    });
}
```

---

## 2. Controller Examples

### 2.1 Complete MVC Controller Action

```csharp
using Microsoft.AspNetCore.Authorization;
using GrcMvc.Application.Permissions;

[Authorize(GrcPermissions.Risks.View)]
public async Task<IActionResult> Dashboard()
{
    // Get statistics
    var statsResult = await _riskService.GetStatisticsAsync();
    if (statsResult.IsFailure)
    {
        _logger.LogError("Failed to load risk statistics: {Error}", statsResult.Error);
        return View("Error");
    }

    // Get recent risks
    var recentRisksResult = await _riskService.GetAllAsync();
    var recentRisks = recentRisksResult.IsSuccess
        ? recentRisksResult.Value.OrderByDescending(r => r.CreatedDate).Take(10).ToList()
        : new List<RiskDto>();

    // Build view model
    var viewModel = new RiskDashboardViewModel
    {
        Statistics = statsResult.Value,
        RecentRisks = recentRisks,
        UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
    };

    return View(viewModel);
}
```

---

### 2.2 API Controller with Error Handling

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RisksController : ControllerBase
{
    private readonly IRiskService _riskService;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RiskDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetRisk(Guid id)
    {
        try
        {
            var result = await _riskService.GetByIdAsync(id);

            if (result.IsFailure)
                return NotFound(ApiResponse<object>.ErrorResponse(result.Error));

            return Ok(ApiResponse<RiskDto>.SuccessResponse(
                result.Value,
                "Risk retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk {RiskId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while retrieving the risk"));
        }
    }
}
```

---

## 3. Validation Examples

### 3.1 Custom Validator with Dependency Injection

```csharp
using FluentValidation;
using GrcMvc.Services.Interfaces;

public class CustomRiskValidator : AbstractValidator<CreateRiskDto>
{
    private readonly IUserDirectoryService _userDirectory;

    public CustomRiskValidator(IUserDirectoryService userDirectory)
    {
        _userDirectory = userDirectory;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Owner)
            .MustAsync(OwnerExistsAsync)
            .WithMessage("Owner must be a valid user");

        // Business rule: Critical risks require mitigation strategy
        RuleFor(x => x)
            .Must(x => x.Probability * x.Impact < 20 ||
                      !string.IsNullOrEmpty(x.MitigationStrategy))
            .WithMessage("Critical risks must have a mitigation strategy");
    }

    private async Task<bool> OwnerExistsAsync(
        string owner,
        CancellationToken cancellation)
    {
        if (string.IsNullOrEmpty(owner))
            return false;

        var user = await _userDirectory.GetUserByEmailAsync(owner);
        return user != null && user.IsActive;
    }
}
```

---

### 3.2 Manual Validation in Service

```csharp
public async Task<Result<RiskDto>> ValidateAndCreateRiskAsync(CreateRiskDto dto)
{
    // Manual validation
    if (dto.Probability < 1 || dto.Probability > 5)
        return Result.ValidationError<RiskDto>("Probability must be between 1 and 5");

    if (dto.Impact < 1 || dto.Impact > 5)
        return Result.ValidationError<RiskDto>("Impact must be between 1 and 5");

    // Calculate risk score
    dto.RiskScore = dto.Probability * dto.Impact;

    // Determine risk level
    var riskLevel = dto.RiskScore switch
    {
        >= 20 => "Critical",
        >= 12 => "High",
        >= 6 => "Medium",
        _ => "Low"
    };

    // Create risk
    return await _riskService.CreateAsync(dto);
}
```

---

## 4. Workflow Implementation

### 4.1 Accept Risk with Workflow

```csharp
using GrcMvc.Services.Interfaces;

public class RiskApprovalService
{
    private readonly IRiskWorkflowService _workflowService;
    private readonly IRiskService _riskService;

    public async Task<Result> AcceptRiskAsync(Guid riskId, string acceptedBy, string justification)
    {
        try
        {
            // Get current risk
            var riskResult = await _riskService.GetByIdAsync(riskId);
            if (riskResult.IsFailure)
                return Result.Failure("Risk not found");

            var risk = riskResult.Value;

            // Validate risk level for acceptance
            if (risk.RiskScore >= 20)
                return Result.Failure("Critical risks require board approval");

            // Execute workflow
            var acceptedRisk = await _workflowService.AcceptAsync(
                riskId,
                acceptedBy,
                justification);

            return Result.Success();
        }
        catch (InvalidStateTransitionException ex)
        {
            return Result.Failure($"Cannot accept risk: {ex.Message}");
        }
    }
}
```

---

### 4.2 State Machine Transition Check

```csharp
public bool CanTransitionToStatus(string currentStatus, string targetStatus)
{
    var transitions = new Dictionary<string, string[]>
    {
        ["Identified"] = new[] { "Active", "Under Review", "Closed" },
        ["Active"] = new[] { "Under Review", "Accepted", "Mitigated", "Closed" },
        ["Under Review"] = new[] { "Active", "Accepted", "Mitigated", "Rejected" },
        ["Accepted"] = new[] { "Active", "Mitigated", "Closed" },
        ["Mitigated"] = new[] { "Closed" },
        ["Closed"] = new[] { "Active" }  // Can reopen
    };

    return transitions.TryGetValue(currentStatus, out var allowed) &&
           allowed.Contains(targetStatus);
}
```

---

## 5. Repository Patterns

### 5.1 Using IUnitOfWork

```csharp
public class CustomRiskService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<List<Risk>> GetRisksWithControlsAsync()
    {
        // Unit of Work automatically applies tenant filtering
        var risks = await _unitOfWork.Risks.Query()
            .Include(r => r.RiskControlMappings)
            .ThenInclude(rcm => rcm.Control)
            .Where(r => r.Status == "Active")
            .ToListAsync();

        return risks;
    }

    public async Task<bool> UpdateRiskWithTransaction async(Guid riskId, UpdateRiskDto dto)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var risk = await _unitOfWork.Risks.GetByIdAsync(riskId);
            if (risk == null)
                return false;

            // Update risk
            risk.Name = dto.Name;
            risk.Probability = dto.Probability;
            risk.Impact = dto.Impact;
            risk.RiskScore = dto.Probability * dto.Impact;

            await _unitOfWork.Risks.UpdateAsync(risk);

            // Update related entities
            // ...

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

---

### 5.2 Query with Filters

```csharp
public async Task<List<RiskDto>> GetHighRisksByOwnerAsync(string owner)
{
    // Tenant filtering applied automatically by global query filter
    var risks = await _unitOfWork.Risks.Query()
        .Where(r => r.Owner == owner && r.RiskScore >= 12)
        .OrderByDescending(r => r.RiskScore)
        .Select(r => new RiskDto
        {
            Id = r.Id,
            Name = r.Name,
            RiskScore = r.RiskScore,
            Status = r.Status
        })
        .ToListAsync();

    return risks;
}
```

---

## 6. Testing Examples

### 6.1 Unit Test with Mock

```csharp
using Xunit;
using Moq;
using FluentAssertions;

public class RiskServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<RiskService>> _mockLogger;
    private readonly RiskService _sut;  // System Under Test

    public RiskServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RiskService>>();
        _sut = new RiskService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateRisk_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateRiskDto
        {
            Name = "Test Risk",
            Probability = 3,
            Impact = 4,
            Category = "Security"
        };

        _mockUnitOfWork.Setup(x => x.Risks.AddAsync(It.IsAny<Risk>()))
            .ReturnsAsync((Risk r) => r);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Risk");
        result.Value.RiskScore.Should().Be(12);

        _mockUnitOfWork.Verify(x => x.Risks.AddAsync(It.IsAny<Risk>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 3)]  // Probability too low
    [InlineData(6, 3)]  // Probability too high
    [InlineData(3, 0)]  // Impact too low
    [InlineData(3, 6)]  // Impact too high
    public async Task CreateRisk_InvalidScores_ReturnsValidationError(
        int probability,
        int impact)
    {
        // Arrange
        var createDto = new CreateRiskDto
        {
            Name = "Test",
            Probability = probability,
            Impact = impact
        };

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("must be between 1 and 5");
    }
}
```

---

### 6.2 Integration Test

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

public class RiskApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RiskApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRisks_WithValidToken_ReturnsOk()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GetTestToken());

        // Act
        var response = await _client.GetAsync("/api/risks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apiResponse = await response.Content
            .ReadFromJsonAsync<ApiResponse<List<RiskDto>>>();

        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeTrue();
    }
}
```

---

## 7. Advanced Scenarios

### 7.1 Generate Risks from Assessment

```csharp
public async Task<List<RiskDto>> GenerateRisksFromGapsAsync(Guid assessmentId)
{
    var generatedRisks = new List<RiskDto>();

    // Get assessment
    var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
    if (assessment == null)
        return generatedRisks;

    // Find requirements with low scores (< 50%)
    var gaps = assessment.Requirements
        .Where(r => (r.Score ?? 0) < 50)
        .OrderBy(r => r.Score)
        .Take(10);

    foreach (var gap in gaps)
    {
        var createDto = new CreateRiskDto
        {
            Name = $"Gap: {gap.ControlTitle}",
            Description = $"Control {gap.ControlNumber} scored {gap.Score}%: {gap.Findings}",
            Category = "Compliance",
            Probability = 3,
            Impact = CalculateImpactFromScore(gap.Score ?? 0),
            Status = "Identified",
            Owner = assessment.AssignedTo
        };

        var result = await _riskService.CreateAsync(createDto);
        if (result.IsSuccess)
        {
            // Link to assessment
            await _riskService.LinkToAssessmentAsync(
                result.Value.Id,
                assessmentId,
                gap.ControlNumber);

            generatedRisks.Add(result.Value);
        }
    }

    return generatedRisks;
}

private int CalculateImpactFromScore(int score)
{
    return score switch
    {
        < 20 => 5,  // Critical gap
        < 40 => 4,  // Major gap
        < 60 => 3,  // Moderate gap
        _ => 2      // Minor gap
    };
}
```

---

### 7.2 Calculate Residual Risk

```csharp
public async Task<decimal> CalculateResidualRiskAsync(Guid riskId)
{
    // Get risk
    var riskResult = await _riskService.GetByIdAsync(riskId);
    if (riskResult.IsFailure)
        return 0;

    var risk = riskResult.Value;

    // Calculate inherent risk
    decimal inherentRisk = risk.Probability * risk.Impact;

    // Get control effectiveness
    var effectivenessResult = await _riskService
        .CalculateControlEffectivenessAsync(riskId);

    var controlEffectiveness = effectivenessResult.IsSuccess
        ? effectivenessResult.Value
        : 0;

    // Residual Risk = Inherent Risk × (1 - Control Effectiveness / 100)
    decimal residualRisk = inherentRisk * (1 - controlEffectiveness / 100);

    return Math.Round(residualRisk, 2);
}
```

---

## 📚 Related Documentation

- **API Examples:** [RISK_MODULE_API_EXAMPLES.md](./RISK_MODULE_API_EXAMPLES.md)
- **Glossary:** [RISK_MODULE_GLOSSARY.md](./RISK_MODULE_GLOSSARY.md)
- **Troubleshooting:** [RISK_MODULE_TROUBLESHOOTING.md](./RISK_MODULE_TROUBLESHOOTING.md)

---

**Last Updated:** January 10, 2026
**Framework:** .NET 8
**Language:** C# 12
