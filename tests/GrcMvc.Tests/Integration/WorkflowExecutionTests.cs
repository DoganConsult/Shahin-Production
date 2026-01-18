using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using GrcMvc.Services.Interfaces.Workflows;

namespace GrcMvc.Tests.Integration;

/// <summary>
/// Integration tests for workflow execution
/// Tests real service implementations with in-memory database
/// Requires ABP infrastructure for DbContext operations
/// </summary>
[Trait("Category", "RequiresAbpInfrastructure")]
public class WorkflowExecutionTests : IDisposable
{
    private readonly GrcDbContext _context;
    private readonly WorkflowEngineService _workflowService;
    private readonly Guid _testTenantId = Guid.NewGuid();
    private readonly Guid _testDefinitionId = Guid.NewGuid();

    public WorkflowExecutionTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<GrcDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new GrcDbContext(options);

        // Setup dependencies
        var logger = new Mock<ILogger<WorkflowEngineService>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var bpmnParser = new Mock<IBpmnParser>();
        var assigneeResolver = new Mock<IWorkflowAssigneeResolver>();
        
        // Setup mock for BPMN parser to return empty workflow
        bpmnParser.Setup(x => x.Parse(It.IsAny<string>()))
            .Returns(new BpmnWorkflow { Steps = new List<BpmnStep>() });

        // Setup audit service to actually persist audit entries to the database
        var auditService = new Mock<IWorkflowAuditService>();
        auditService.Setup(x => x.RecordInstanceEventAsync(
                It.IsAny<WorkflowInstance>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(async (WorkflowInstance instance, string eventType, string? oldStatus, string description) =>
            {
                // Extract acting user name from description if available (e.g., "Workflow approved by approver. Reason: ...")
                string actingUserName = instance.InitiatedByUserName ?? "System";
                if (description.Contains("approved by"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(description, @"approved by (\w+)");
                    if (match.Success)
                    {
                        actingUserName = match.Groups[1].Value;
                    }
                }
                else if (description.Contains("rejected by"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(description, @"rejected by (\w+)");
                    if (match.Success)
                    {
                        actingUserName = match.Groups[1].Value;
                    }
                }

                var auditEntry = new WorkflowAuditEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = instance.TenantId,
                    WorkflowInstanceId = instance.Id,
                    EventType = eventType,
                    SourceEntity = "WorkflowInstance",
                    SourceEntityId = instance.Id,
                    OldStatus = oldStatus,
                    NewStatus = instance.Status,
                    ActingUserId = instance.InitiatedByUserId ?? Guid.Empty,
                    ActingUserName = actingUserName,
                    Description = description,
                    EventTime = DateTime.UtcNow
                };
                _context.WorkflowAuditEntries.Add(auditEntry);
                await _context.SaveChangesAsync();
            });

        _workflowService = new WorkflowEngineService(
            _context,
            logger.Object,
            cache,
            bpmnParser.Object,
            assigneeResolver.Object,
            auditService.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var definition = new WorkflowDefinition
        {
            Id = _testDefinitionId,
            TenantId = _testTenantId,
            Name = "Test Approval Workflow",
            Type = "Approval",
            WorkflowNumber = "WF-001",
            Description = "Test workflow for integration tests",
            IsDeleted = false
        };

        _context.WorkflowDefinitions.Add(definition);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============ Workflow Creation Tests ============

    [Fact]
    public async Task CreateWorkflow_WithValidDefinition_ShouldCreateInstance()
    {
        // Act
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId,
            _testDefinitionId,
            "High",
            "test-user");

        // Assert
        workflow.Should().NotBeNull();
        workflow.Id.Should().NotBeEmpty();
        workflow.Status.Should().Be("Pending");
        workflow.TenantId.Should().Be(_testTenantId);
        workflow.WorkflowDefinitionId.Should().Be(_testDefinitionId);
    }

    [Fact]
    public async Task CreateWorkflow_WithInvalidDefinition_ShouldThrowException()
    {
        // Arrange
        var invalidDefinitionId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<GrcMvc.Exceptions.WorkflowNotFoundException>(async () =>
            await _workflowService.CreateWorkflowAsync(
                _testTenantId,
                invalidDefinitionId,
                "Medium",
                "test-user"));
    }

    // ============ Workflow State Transition Tests ============

    [Fact]
    public async Task ApproveWorkflow_ShouldTransitionToInApproval()
    {
        // Arrange
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Medium", "test-user");

        // Act
        var result = await _workflowService.ApproveWorkflowAsync(
            _testTenantId, workflow.Id, "Approved for testing", "approver-user");

        // Assert
        result.Should().BeTrue();

        var updatedWorkflow = await _workflowService.GetWorkflowAsync(_testTenantId, workflow.Id);
        updatedWorkflow.Status.Should().Be("InApproval");
    }

    [Fact]
    public async Task RejectWorkflow_ShouldTransitionToRejected()
    {
        // Arrange
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Medium", "test-user");

        // Act
        var result = await _workflowService.RejectWorkflowAsync(
            _testTenantId, workflow.Id, "Does not meet requirements", "reviewer-user");

        // Assert
        result.Should().BeTrue();

        var updatedWorkflow = await _workflowService.GetWorkflowAsync(_testTenantId, workflow.Id);
        updatedWorkflow.Status.Should().Be("Rejected");
    }

    [Fact]
    public async Task CompleteWorkflow_ShouldSetCompletedStatus()
    {
        // Arrange
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Medium", "test-user");

        // Act
        var result = await _workflowService.CompleteWorkflowAsync(_testTenantId, workflow.Id);

        // Assert
        result.Should().BeTrue();

        var updatedWorkflow = await _workflowService.GetWorkflowAsync(_testTenantId, workflow.Id);
        updatedWorkflow.Status.Should().Be("Completed");
        updatedWorkflow.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteWorkflow_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var invalidWorkflowId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<GrcMvc.Exceptions.WorkflowNotFoundException>(async () =>
            await _workflowService.CompleteWorkflowAsync(_testTenantId, invalidWorkflowId));
    }

    // ============ Workflow Retrieval Tests ============

    [Fact]
    public async Task GetWorkflow_ShouldReturnWorkflowWithDefinition()
    {
        // Arrange
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Medium", "test-user");

        // Act
        var retrieved = await _workflowService.GetWorkflowAsync(_testTenantId, workflow.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Id.Should().Be(workflow.Id);
        retrieved.WorkflowDefinition.Should().NotBeNull();
        retrieved.WorkflowDefinition.Name.Should().Be("Test Approval Workflow");
    }

    [Fact]
    public async Task GetUserWorkflows_ShouldReturnPaginatedResults()
    {
        // Arrange - Create multiple workflows
        for (int i = 0; i < 5; i++)
        {
            await _workflowService.CreateWorkflowAsync(
                _testTenantId, _testDefinitionId, "Medium", $"user-{i}");
        }

        // Act
        var workflows = await _workflowService.GetUserWorkflowsAsync(_testTenantId, page: 1, pageSize: 3);

        // Assert
        workflows.Should().HaveCount(3);
    }

    // ============ Statistics Tests ============

    [Fact]
    public async Task GetStatistics_ShouldReturnCorrectCounts()
    {
        // Arrange - Create workflows with different statuses
        var workflow1 = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "High", "user1");
        var workflow2 = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Medium", "user2");
        var workflow3 = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Low", "user3");

        await _workflowService.CompleteWorkflowAsync(_testTenantId, workflow1.Id);
        await _workflowService.RejectWorkflowAsync(_testTenantId, workflow2.Id, "Rejected", "admin");

        // Act
        var stats = await _workflowService.GetStatisticsAsync(_testTenantId);

        // Assert
        stats.TotalWorkflows.Should().Be(3);
        stats.CompletedWorkflows.Should().Be(1);
        stats.RejectedWorkflows.Should().Be(1);
        stats.PendingWorkflows.Should().Be(1);
    }

    // ============ Audit Trail Tests ============

    [Fact]
    public async Task ApproveWorkflow_ShouldCreateAuditEntry()
    {
        // Arrange
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "Medium", "test-user");

        // Act
        await _workflowService.ApproveWorkflowAsync(
            _testTenantId, workflow.Id, "Test approval", "approver");

        // Assert
        var auditEntries = await _context.WorkflowAuditEntries
            .Where(e => e.WorkflowInstanceId == workflow.Id)
            .ToListAsync();

        auditEntries.Should().HaveCount(1);
        auditEntries[0].EventType.Should().Be("ApprovalApproved");
        auditEntries[0].ActingUserName.Should().Be("approver");
    }

    // ============ Full Workflow Lifecycle Test ============

    [Fact]
    public async Task FullWorkflowLifecycle_ShouldProcessThroughAllStages()
    {
        // 1. Create workflow
        var workflow = await _workflowService.CreateWorkflowAsync(
            _testTenantId, _testDefinitionId, "High", "initiator");
        workflow.Status.Should().Be("Pending");

        // 2. Approve workflow
        await _workflowService.ApproveWorkflowAsync(
            _testTenantId, workflow.Id, "Manager approved", "manager");
        var afterApproval = await _workflowService.GetWorkflowAsync(_testTenantId, workflow.Id);
        afterApproval.Status.Should().Be("InApproval");

        // 3. Complete workflow
        await _workflowService.CompleteWorkflowAsync(_testTenantId, workflow.Id);
        var afterCompletion = await _workflowService.GetWorkflowAsync(_testTenantId, workflow.Id);
        afterCompletion.Status.Should().Be("Completed");
        afterCompletion.CompletedAt.Should().NotBeNull();

        // 4. Verify audit trail
        var auditEntries = await _context.WorkflowAuditEntries
            .Where(e => e.WorkflowInstanceId == workflow.Id)
            .OrderBy(e => e.EventTime)
            .ToListAsync();

        auditEntries.Should().HaveCount(2); // Approval and completion create audit entries
        auditEntries[0].EventType.Should().Be("ApprovalApproved");
        auditEntries[1].EventType.Should().Be("InstanceCompleted");
    }

    // ============ Concurrent Access Tests ============

    [Fact]
    public async Task ConcurrentWorkflowCreation_ShouldHandleMultipleRequests()
    {
        // Arrange
        var semaphore = new SemaphoreSlim(1, 1); // Serialize access to DbContext
        var tasks = new List<Task<WorkflowInstance>>();

        // Act - Create 10 workflows with serialized access to avoid DbContext thread-safety issues
        // Note: In-memory database is not thread-safe with concurrent DbContext instances
        // In production, each request would have its own DbContext instance
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await _workflowService.CreateWorkflowAsync(
                        _testTenantId, _testDefinitionId, "Medium", $"user-{index}");
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        var workflows = await Task.WhenAll(tasks);

        // Assert
        workflows.Should().HaveCount(10);
        workflows.Select(w => w.Id).Distinct().Should().HaveCount(10);
    }
}
