using GrcMvc.BackgroundJobs;
using GrcMvc.Data;
using GrcMvc.Models.Workflows;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Integration
{
    /// <summary>
    /// Integration tests for notification delivery
    /// </summary>
    public class NotificationTests : IDisposable
    {
        private readonly GrcDbContext _context;
        private readonly Mock<ISmtpEmailService> _mockEmailService;
        private readonly Mock<ILogger<NotificationDeliveryJob>> _mockLogger;
        private readonly NotificationDeliveryJob _job;

        public NotificationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<GrcDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GrcDbContext(options);

            // Setup mocks
            _mockEmailService = new Mock<ISmtpEmailService>();
            _mockLogger = new Mock<ILogger<NotificationDeliveryJob>>();

            var settings = Options.Create(new WorkflowSettings
            {
                MaxRetryAttempts = 3,
                NotificationDeliveryIntervalMinutes = 5
            });

            _job = new NotificationDeliveryJob(
                _context,
                _mockEmailService.Object,
                _mockLogger.Object,
                settings);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test tenant
            _context.Tenants.Add(new Tenant { Id = 1, Name = "Test Tenant", IsActive = true });

            // Add test user
            _context.Users.Add(new Microsoft.AspNetCore.Identity.IdentityUser
            {
                Id = "test-user-1",
                Email = "test@example.com",
                UserName = "testuser"
            });

            // Add test workflow
            _context.WorkflowInstances.Add(new WorkflowInstance
            {
                Id = 1,
                WorkflowType = "TestWorkflow",
                Status = "Active",
                TenantId = 1,
                CreatedByUserId = "test-user-1",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesPendingNotifications()
        {
            // Arrange
            var notification = new WorkflowNotification
            {
                Id = 1,
                WorkflowInstanceId = 1,
                RecipientUserId = "test-user-1",
                NotificationType = "TaskAssigned",
                Subject = "Test Subject",
                Body = "Test Body",
                Priority = "Normal",
                TenantId = 1,
                CreatedAt = DateTime.UtcNow,
                RequiresEmail = true,
                IsDelivered = false,
                DeliveryAttempts = 0
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            _mockEmailService
                .Setup(x => x.SendTemplatedEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedNotification = await _context.WorkflowNotifications.FindAsync(1);
            Assert.NotNull(updatedNotification);
            Assert.True(updatedNotification.IsDelivered);
            Assert.NotNull(updatedNotification.DeliveredAt);
        }

        [Fact]
        public async Task ExecuteAsync_RetriesFailedDelivery()
        {
            // Arrange
            var notification = new WorkflowNotification
            {
                Id = 2,
                WorkflowInstanceId = 1,
                RecipientUserId = "test-user-1",
                NotificationType = "TaskAssigned",
                Subject = "Test Subject",
                Body = "Test Body",
                Priority = "Normal",
                TenantId = 1,
                CreatedAt = DateTime.UtcNow,
                RequiresEmail = true,
                IsDelivered = false,
                DeliveryAttempts = 0
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            _mockEmailService
                .Setup(x => x.SendTemplatedEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(new Exception("SMTP connection failed"));

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedNotification = await _context.WorkflowNotifications.FindAsync(2);
            Assert.NotNull(updatedNotification);
            Assert.False(updatedNotification.IsDelivered);
            Assert.Equal(1, updatedNotification.DeliveryAttempts);
            Assert.NotNull(updatedNotification.NextRetryAt);
        }

        [Fact]
        public async Task ExecuteAsync_RespectsUserPreferences()
        {
            // Arrange
            var preference = new UserNotificationPreference
            {
                UserId = "test-user-1",
                TenantId = 1,
                EmailEnabled = false,
                SmsEnabled = false
            };
            _context.UserNotificationPreferences.Add(preference);

            var notification = new WorkflowNotification
            {
                Id = 3,
                WorkflowInstanceId = 1,
                RecipientUserId = "test-user-1",
                NotificationType = "TaskAssigned",
                Subject = "Test Subject",
                Body = "Test Body",
                Priority = "Normal",
                TenantId = 1,
                CreatedAt = DateTime.UtcNow,
                RequiresEmail = true,
                IsDelivered = false,
                DeliveryAttempts = 0
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedNotification = await _context.WorkflowNotifications.FindAsync(3);
            Assert.NotNull(updatedNotification);
            Assert.True(updatedNotification.IsDelivered); // Marked as delivered but not sent
            Assert.Contains("disabled", updatedNotification.DeliveryNote ?? "");

            // Verify email was NOT sent
            _mockEmailService.Verify(
                x => x.SendTemplatedEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_PrioritizesCriticalNotifications()
        {
            // Arrange
            var normalNotification = new WorkflowNotification
            {
                Id = 4,
                WorkflowInstanceId = 1,
                RecipientUserId = "test-user-1",
                NotificationType = "TaskAssigned",
                Subject = "Normal",
                Body = "Normal priority",
                Priority = "Normal",
                TenantId = 1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                RequiresEmail = true,
                IsDelivered = false
            };

            var criticalNotification = new WorkflowNotification
            {
                Id = 5,
                WorkflowInstanceId = 1,
                RecipientUserId = "test-user-1",
                NotificationType = "SLA_Breach",
                Subject = "Critical",
                Body = "Critical priority",
                Priority = "Critical",
                TenantId = 1,
                CreatedAt = DateTime.UtcNow,
                RequiresEmail = true,
                IsDelivered = false
            };

            _context.WorkflowNotifications.AddRange(normalNotification, criticalNotification);
            await _context.SaveChangesAsync();

            var callOrder = new List<string>();
            _mockEmailService
                .Setup(x => x.SendTemplatedEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()))
                .Callback<string, string, string, Dictionary<string, object>>((email, subject, template, data) =>
                {
                    callOrder.Add(subject);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _job.ExecuteAsync();

            // Assert - Critical should be processed first
            Assert.Equal(2, callOrder.Count);
            Assert.Equal("Critical", callOrder[0]);
        }

        [Fact]
        public async Task ExecuteAsync_StopsAfterMaxRetries()
        {
            // Arrange
            var notification = new WorkflowNotification
            {
                Id = 6,
                WorkflowInstanceId = 1,
                RecipientUserId = "test-user-1",
                NotificationType = "TaskAssigned",
                Subject = "Test",
                Body = "Test",
                Priority = "Normal",
                TenantId = 1,
                CreatedAt = DateTime.UtcNow,
                RequiresEmail = true,
                IsDelivered = false,
                DeliveryAttempts = 3 // Already at max
            };

            _context.WorkflowNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            await _job.ExecuteAsync();

            // Assert - Should not attempt delivery
            _mockEmailService.Verify(
                x => x.SendTemplatedEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>()),
                Times.Never);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
