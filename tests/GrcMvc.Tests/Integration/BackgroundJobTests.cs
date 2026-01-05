using GrcMvc.BackgroundJobs;
using GrcMvc.Data;
using GrcMvc.Models.Workflows;
using GrcMvc.Services.Interfaces.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Integration
{
    /// <summary>
    /// Integration tests for background jobs
    /// </summary>
    public class BackgroundJobTests : IDisposable
    {
        private readonly GrcDbContext _context;
        private readonly Mock<IEscalationService> _mockEscalationService;
        private readonly Mock<ILogger<EscalationJob>> _mockEscalationLogger;
        private readonly Mock<ILogger<SlaMonitorJob>> _mockSlaLogger;

        public BackgroundJobTests()
        {
            var options = new DbContextOptionsBuilder<GrcDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GrcDbContext(options);
            _mockEscalationService = new Mock<IEscalationService>();
            _mockEscalationLogger = new Mock<ILogger<EscalationJob>>();
            _mockSlaLogger = new Mock<ILogger<SlaMonitorJob>>();

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test tenant
            _context.Tenants.Add(new Tenant { Id = 1, Name = "Test Tenant", IsActive = true });

            // Add test users
            _context.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "user-1",
                Email = "user1@example.com",
                UserName = "user1"
            });

            _context.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "manager-1",
                Email = "manager@example.com",
                UserName = "manager"
            });

            // Add user role assignment for escalation target
            _context.UserRoleAssignments.Add(new UserRoleAssignment
            {
                UserId = "manager-1",
                TenantId = 1,
                RoleId = "ComplianceOfficer",
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
                Id = 1,
                WorkflowType = "ControlImplementation",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };
            _context.WorkflowInstances.Add(workflow);

            var overdueTask = new WorkflowTask
            {
                Id = 1,
                WorkflowInstanceId = 1,
                Title = "Overdue Task",
                AssignedToUserId = "user-1",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-25), // 25 hours overdue
                TenantId = 1,
                IsEscalated = false
            };
            _context.WorkflowTasks.Add(overdueTask);
            await _context.SaveChangesAsync();

            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockEscalationLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var escalation = await _context.WorkflowEscalations.FirstOrDefaultAsync();
            Assert.NotNull(escalation);
            Assert.Equal(2, escalation.EscalationLevel); // Level 2 for 24-48 hours overdue

            var updatedTask = await _context.WorkflowTasks.FindAsync(1);
            Assert.True(updatedTask!.IsEscalated);
        }

        [Fact]
        public async Task EscalationJob_SetsCorrectLevelBasedOnOverdueHours()
        {
            // Arrange - Create tasks with different overdue periods
            var workflow = new WorkflowInstance
            {
                Id = 2,
                WorkflowType = "Test",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1"
            };
            _context.WorkflowInstances.Add(workflow);

            // 12 hours overdue -> Level 1
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = 10,
                WorkflowInstanceId = 2,
                Title = "Level 1 Task",
                AssignedToUserId = "user-1",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-12),
                TenantId = 1,
                IsEscalated = false
            });

            // 36 hours overdue -> Level 2
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = 11,
                WorkflowInstanceId = 2,
                Title = "Level 2 Task",
                AssignedToUserId = "user-1",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-36),
                TenantId = 1,
                IsEscalated = false
            });

            // 60 hours overdue -> Level 3
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = 12,
                WorkflowInstanceId = 2,
                Title = "Level 3 Task",
                AssignedToUserId = "user-1",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-60),
                TenantId = 1,
                IsEscalated = false
            });

            // 100 hours overdue -> Level 4
            _context.WorkflowTasks.Add(new WorkflowTask
            {
                Id = 13,
                WorkflowInstanceId = 2,
                Title = "Level 4 Task",
                AssignedToUserId = "user-1",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-100),
                TenantId = 1,
                IsEscalated = false
            });

            await _context.SaveChangesAsync();

            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockEscalationLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var escalations = await _context.WorkflowEscalations.ToListAsync();
            Assert.Equal(4, escalations.Count);

            Assert.Contains(escalations, e => e.TaskId == 10 && e.EscalationLevel == 1);
            Assert.Contains(escalations, e => e.TaskId == 11 && e.EscalationLevel == 2);
            Assert.Contains(escalations, e => e.TaskId == 12 && e.EscalationLevel == 3);
            Assert.Contains(escalations, e => e.TaskId == 13 && e.EscalationLevel == 4);
        }

        [Fact]
        public async Task EscalationJob_DoesNotReescalateAlreadyEscalatedTasks()
        {
            // Arrange
            var workflow = new WorkflowInstance
            {
                Id = 3,
                WorkflowType = "Test",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1"
            };
            _context.WorkflowInstances.Add(workflow);

            var alreadyEscalated = new WorkflowTask
            {
                Id = 20,
                WorkflowInstanceId = 3,
                Title = "Already Escalated",
                AssignedToUserId = "user-1",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddHours(-30),
                TenantId = 1,
                IsEscalated = true // Already escalated
            };
            _context.WorkflowTasks.Add(alreadyEscalated);
            await _context.SaveChangesAsync();

            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockEscalationLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var escalationCount = await _context.WorkflowEscalations
                .Where(e => e.TaskId == 20)
                .CountAsync();
            Assert.Equal(0, escalationCount);
        }

        #endregion

        #region SLA Monitor Job Tests

        [Fact]
        public async Task SlaMonitorJob_SendsWarningForUpcomingSla()
        {
            // Arrange
            var workflow = new WorkflowInstance
            {
                Id = 100,
                WorkflowType = "Approval",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1",
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
            var workflow = new WorkflowInstance
            {
                Id = 101,
                WorkflowType = "Approval",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1",
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
            var workflow = new WorkflowInstance
            {
                Id = 102,
                WorkflowType = "Approval",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1",
                SlaDueDate = DateTime.UtcNow.AddHours(-2), // 2 hours past due
                SlaBreached = false
            };
            _context.WorkflowInstances.Add(workflow);
            await _context.SaveChangesAsync();

            var job = new SlaMonitorJob(_context, _mockSlaLogger.Object);

            // Act
            await job.ExecuteAsync();

            // Assert
            var updatedWorkflow = await _context.WorkflowInstances.FindAsync(102);
            Assert.True(updatedWorkflow!.SlaBreached);
            Assert.NotNull(updatedWorkflow.SlaBreachedAt);

            var escalation = await _context.WorkflowEscalations
                .Where(e => e.WorkflowInstanceId == 102 && e.IsSlaEscalation)
                .FirstOrDefaultAsync();
            Assert.NotNull(escalation);
            Assert.Equal(4, escalation.EscalationLevel); // Highest level for SLA breach

            var auditLog = await _context.AuditLogs
                .Where(a => a.EntityId == "102" && a.Action == "SLA_BREACH")
                .FirstOrDefaultAsync();
            Assert.NotNull(auditLog);
        }

        [Fact]
        public async Task SlaMonitorJob_DoesNotReprocessAlreadyBreachedSla()
        {
            // Arrange
            var workflow = new WorkflowInstance
            {
                Id = 103,
                WorkflowType = "Approval",
                Status = "InProgress",
                TenantId = 1,
                CreatedByUserId = "user-1",
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
                .Where(e => e.WorkflowInstanceId == 103)
                .CountAsync();
            Assert.Equal(0, escalationCount);
        }

        #endregion

        #region Job Scheduling Tests

        [Fact]
        public void EscalationJob_CanBeInstantiated()
        {
            // Arrange & Act
            var job = new EscalationJob(_context, _mockEscalationService.Object, _mockEscalationLogger.Object);

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

    // Mock classes for testing
    public class UserRoleAssignment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int TenantId { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
