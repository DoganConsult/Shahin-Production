using GrcMvc.BackgroundJobs;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using WorkflowNotification = GrcMvc.Models.Workflows.WorkflowNotification;
using WorkflowEscalation = GrcMvc.Models.Workflows.WorkflowEscalation;

namespace GrcMvc.Tests.Integration
{
    /// <summary>
    /// Integration tests for background jobs
    /// Requires ABP infrastructure for DbContext operations
    /// </summary>
    [Trait("Category", "RequiresAbpInfrastructure")]
    public class BackgroundJobTests : IDisposable
    {
        private readonly GrcDbContext _context;
        private readonly Mock<IEscalationService> _mockEscalationService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<EscalationJob>> _mockEscalationLogger;
        private readonly Mock<ILogger<SlaMonitorJob>> _mockSlaLogger;
        private readonly Guid _testTenantId = Guid.NewGuid();
        private readonly Guid _testWorkflowId1 = Guid.NewGuid();
        private readonly Guid _testWorkflowId2 = Guid.NewGuid();
        private readonly Guid _testWorkflowId3 = Guid.NewGuid();
        private readonly Guid _testTaskId1 = Guid.NewGuid();
        private readonly Guid _testTaskId10 = Guid.NewGuid();
        private readonly Guid _testTaskId11 = Guid.NewGuid();
        private readonly Guid _testTaskId12 = Guid.NewGuid();
        private readonly Guid _testTaskId13 = Guid.NewGuid();
        private readonly Guid _testTaskId20 = Guid.NewGuid();

        public BackgroundJobTests()
        {
            var options = new DbContextOptionsBuilder<GrcDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GrcDbContext(options);
            _mockEscalationService = new Mock<IEscalationService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockEscalationLogger = new Mock<ILogger<EscalationJob>>();
            _mockSlaLogger = new Mock<ILogger<SlaMonitorJob>>();

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test tenant
            _context.Tenants.Add(new Tenant
            {
                Id = _testTenantId,
                OrganizationName = "Test Tenant",
                TenantSlug = "test-tenant",
                AdminEmail = "admin@test.com",
                IsActive = true
            });

            _context.SaveChanges();
        }

        #region Escalation Job Tests

        [Fact]
        public async Task EscalationJob_ProcessesOverdueTasks()
        {
            // Arrange
            var workflow = new WorkflowInstance
            {
                Id = _testWorkflowId1,
                TenantId = _testTenantId,
                WorkflowType = "ControlImplementation",
                Status = "InProgress",
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            };
            _context.WorkflowInstances.Add(workflow);

            var overdueTask = new WorkflowTask
            {
                Id = _testTaskId1,
                WorkflowInstanceId = _testWorkflowId1,
                TaskName = "Overdue Task",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-25), // 25 hours overdue
                TenantId = _testTenantId,
                IsEscalated = false
            };
            _context.WorkflowTasks.Add(overdueTask);
            await _context.SaveChangesAsync();

            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockNotificationService.Object, _mockEscalationLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var escalation = await _context.WorkflowEscalations.FirstOrDefaultAsync();
            Assert.NotNull(escalation);
            Assert.Equal(2, escalation.EscalationLevel); // Level 2 for 24-48 hours overdue

            var updatedTask = await _context.WorkflowTasks.FindAsync(_testTaskId1);
            Assert.True(updatedTask!.IsEscalated);
        }

        [Fact]
        public async Task EscalationJob_SetsCorrectLevelBasedOnOverdueHours()
        {
            // Arrange - Create tasks with different overdue periods
            var workflow = new WorkflowInstance
            {
                Id = _testWorkflowId2,
                TenantId = _testTenantId,
                WorkflowType = "Test",
                Status = "InProgress"
            };
            _context.WorkflowInstances.Add(workflow);

            // 12 hours overdue -> Level 1
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = _testTaskId10,
                WorkflowInstanceId = _testWorkflowId2,
                TaskName = "Level 1 Task",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-12),
                TenantId = _testTenantId,
                IsEscalated = false
            });

            // 36 hours overdue -> Level 2
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = _testTaskId11,
                WorkflowInstanceId = _testWorkflowId2,
                TaskName = "Level 2 Task",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-36),
                TenantId = _testTenantId,
                IsEscalated = false
            });

            // 60 hours overdue -> Level 3
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = _testTaskId12,
                WorkflowInstanceId = _testWorkflowId2,
                TaskName = "Level 3 Task",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-60),
                TenantId = _testTenantId,
                IsEscalated = false
            });

            // 100 hours overdue -> Level 4
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = _testTaskId13,
                WorkflowInstanceId = _testWorkflowId2,
                TaskName = "Level 4 Task",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-100),
                TenantId = _testTenantId,
                IsEscalated = false
            });

            await _context.SaveChangesAsync();

            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockNotificationService.Object, _mockEscalationLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var escalations = await _context.WorkflowEscalations.ToListAsync();
            Assert.Equal(4, escalations.Count);

            Assert.Contains(escalations, e => e.TaskId == _testTaskId10 && e.EscalationLevel == 1);
            Assert.Contains(escalations, e => e.TaskId == _testTaskId11 && e.EscalationLevel == 2);
            Assert.Contains(escalations, e => e.TaskId == _testTaskId12 && e.EscalationLevel == 3);
            Assert.Contains(escalations, e => e.TaskId == _testTaskId13 && e.EscalationLevel == 4);
        }

        [Fact]
        public async Task EscalationJob_DoesNotReescalateAlreadyEscalatedTasks()
        {
            // Arrange
            var workflow = new WorkflowInstance
            {
                Id = _testWorkflowId3,
                TenantId = _testTenantId,
                WorkflowType = "Test",
                Status = "InProgress"
            };
            _context.WorkflowInstances.Add(workflow);

            var alreadyEscalated = new WorkflowTask
            {
                Id = _testTaskId20,
                WorkflowInstanceId = _testWorkflowId3,
                TaskName = "Already Escalated",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-30),
                TenantId = _testTenantId,
                IsEscalated = true // Already escalated
            };
            _context.WorkflowTasks.Add(alreadyEscalated);
            await _context.SaveChangesAsync();

            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockNotificationService.Object, _mockEscalationLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var escalationCount = await _context.WorkflowEscalations
                .Where(e => e.TaskId == _testTaskId20)
                .CountAsync();
            Assert.Equal(0, escalationCount);
        }

        #endregion

        #region SLA Monitor Job Tests

        [Fact]
        public async Task SlaMonitorJob_SendsWarningForUpcomingSla()
        {
            // Arrange
            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowInstance
            {
                Id = workflowId,
                TenantId = _testTenantId,
                WorkflowType = "Approval",
                Status = "InProgress",
                SlaDueDate = DateTime.UtcNow.AddHours(12), // Due in 12 hours
                SlaBreached = false
            };
            _context.WorkflowInstances.Add(workflow);
            await _context.SaveChangesAsync();

            var job = new SlaMonitorJob(_context, _mockSlaLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var notification = await _context.WorkflowNotifications
                .Where(n => n.NotificationType == "SLA_Warning")
                .FirstOrDefaultAsync();
            Assert.NotNull(notification);
            Assert.Contains("WARNING", notification.Subject);
        }

        [Fact]
        public async Task SlaMonitorJob_SendsCriticalForImminentBreach()
        {
            // Arrange
            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowInstance
            {
                Id = workflowId,
                TenantId = _testTenantId,
                WorkflowType = "Approval",
                Status = "InProgress",
                SlaDueDate = DateTime.UtcNow.AddHours(2), // Due in 2 hours - critical
                SlaBreached = false
            };
            _context.WorkflowInstances.Add(workflow);
            await _context.SaveChangesAsync();

            var job = new SlaMonitorJob(_context, _mockSlaLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var notification = await _context.WorkflowNotifications
                .Where(n => n.NotificationType == "SLA_Critical")
                .FirstOrDefaultAsync();
            Assert.NotNull(notification);
            Assert.Equal("Critical", notification.Priority);
        }

        [Fact]
        public async Task SlaMonitorJob_ProcessesSlaBreachCorrectly()
        {
            // Arrange
            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowInstance
            {
                Id = workflowId,
                TenantId = _testTenantId,
                WorkflowType = "Approval",
                Status = "InProgress",
                SlaDueDate = DateTime.UtcNow.AddHours(-2), // 2 hours past due
                SlaBreached = false
            };
            _context.WorkflowInstances.Add(workflow);
            await _context.SaveChangesAsync();

            var job = new SlaMonitorJob(_context, _mockSlaLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var updatedWorkflow = await _context.WorkflowInstances.FindAsync(workflowId);
            Assert.True(updatedWorkflow!.SlaBreached);
            Assert.NotNull(updatedWorkflow.SlaBreachedAt);

            var escalation = await _context.WorkflowEscalations
                .Where(e => e.WorkflowInstanceId == workflowId)
                .FirstOrDefaultAsync();
            Assert.NotNull(escalation);
            Assert.Equal(4, escalation.EscalationLevel); // Highest level for SLA breach
        }

        [Fact]
        public async Task SlaMonitorJob_DoesNotReprocessAlreadyBreachedSla()
        {
            // Arrange
            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowInstance
            {
                Id = workflowId,
                TenantId = _testTenantId,
                WorkflowType = "Approval",
                Status = "InProgress",
                SlaDueDate = DateTime.UtcNow.AddHours(-5),
                SlaBreached = true, // Already marked as breached
                SlaBreachedAt = DateTime.UtcNow.AddHours(-3)
            };
            _context.WorkflowInstances.Add(workflow);
            await _context.SaveChangesAsync();

            var job = new SlaMonitorJob(_context, _mockSlaLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert - Should not create new escalation
            var escalationCount = await _context.WorkflowEscalations
                .Where(e => e.WorkflowInstanceId == workflowId)
                .CountAsync();
            Assert.Equal(0, escalationCount);
        }

        #endregion

        #region Job Scheduling Tests

        [Fact]
        public void EscalationJob_CanBeInstantiated()
        {
            // Arrange & Act
            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockNotificationService.Object, _mockEscalationLogger.Object);

            // Assert
            Assert.NotNull(job);
        }

        [Fact]
        public void SlaMonitorJob_CanBeInstantiated()
        {
            // Arrange & Act
            var job = new SlaMonitorJob(_context, _mockSlaLogger.Object);

            // Assert
            Assert.NotNull(job);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
