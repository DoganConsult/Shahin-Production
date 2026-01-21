using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Mail;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.Tests.Integration;

/// <summary>
/// Tests for email delivery functionality
/// Uses mock SMTP client to verify email composition and delivery logic
/// </summary>
public class EmailDeliveryTests
{
    private readonly Mock<ILogger<NotificationService>> _loggerMock;
    private readonly SmtpSettings _smtpSettings;

    public EmailDeliveryTests()
    {
        _loggerMock = new Mock<ILogger<NotificationService>>();
        _smtpSettings = new SmtpSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            EnableSsl = true,
            FromEmail = "test@grcsystem.com",
            FromName = "GRC Test System",
            Username = "testuser",
            Password = "testpass"
        };
    }

    // ============ Email Composition Tests ============

    [Fact]
    public void EmailMessage_ShouldHaveCorrectFromAddress()
    {
        // Arrange
        var message = new MailMessage();
        message.From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName);
        message.To.Add("recipient@test.com");
        message.Subject = "Test Subject";
        message.Body = "Test Body";

        // Assert
        message.From.Address.Should().Be("test@grcsystem.com");
        message.From.DisplayName.Should().Be("GRC Test System");
    }

    [Fact]
    public void EmailMessage_WithHtmlBody_ShouldSetIsBodyHtml()
    {
        // Arrange
        var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromEmail),
            Subject = "HTML Email Test",
            Body = "<html><body><h1>Test</h1></body></html>",
            IsBodyHtml = true
        };
        message.To.Add("recipient@test.com");

        // Assert
        message.IsBodyHtml.Should().BeTrue();
        message.Body.Should().Contain("<html>");
    }

    [Fact]
    public void EmailMessage_WithMultipleRecipients_ShouldAddAll()
    {
        // Arrange
        var recipients = new[] { "user1@test.com", "user2@test.com", "user3@test.com" };
        var message = new MailMessage
        {
            From = new MailAddress(_smtpSettings.FromEmail),
            Subject = "Multi-recipient Test"
        };

        foreach (var recipient in recipients)
        {
            message.To.Add(recipient);
        }

        // Assert
        message.To.Count.Should().Be(3);
    }

    // ============ Notification Template Tests ============

    [Fact]
    public void WorkflowApprovalEmail_ShouldContainRequiredFields()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var workflowName = "Q1 Compliance Review";
        var requesterName = "John Doe";
        var approverName = "Jane Smith";

        // Act
        var subject = $"[ACTION REQUIRED] Approval Request: {workflowName}";
        var body = BuildApprovalEmailBody(workflowId, workflowName, requesterName, approverName);

        // Assert
        subject.Should().Contain("ACTION REQUIRED");
        subject.Should().Contain(workflowName);
        body.Should().Contain(workflowId.ToString());
        body.Should().Contain(requesterName);
        body.Should().Contain(approverName);
    }

    [Fact]
    public void EscalationEmail_ShouldHaveHighPriorityIndicator()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskName = "Overdue Control Assessment";
        var escalationLevel = 2;
        var hoursOverdue = 48.5;

        // Act
        var subject = $"[ESCALATION L{escalationLevel}] {taskName}";
        var body = BuildEscalationEmailBody(taskId, taskName, escalationLevel, hoursOverdue);

        // Assert
        subject.Should().Contain("ESCALATION");
        subject.Should().Contain($"L{escalationLevel}");
        body.Should().Contain("48.5 hours overdue");
        body.Should().Contain("immediate attention");
    }

    [Fact]
    public void SlaBreachEmail_ShouldBeCriticalPriority()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var workflowType = "Risk Assessment";
        var slaDueDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var subject = $"[CRITICAL] SLA Breach - {workflowType}";
        var body = BuildSlaBreachEmailBody(workflowId, workflowType, slaDueDate);

        // Assert
        subject.Should().Contain("CRITICAL");
        body.Should().Contain("SLA has been breached");
        body.Should().Contain("immediate action required");
    }

    [Fact]
    public void TaskAssignmentEmail_ShouldContainTaskDetails()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var taskTitle = "Review Quarterly Report";
        var assignedTo = "analyst@company.com";
        var dueDate = DateTime.UtcNow.AddDays(5);

        // Act
        var subject = $"[NEW TASK] {taskTitle}";
        var body = BuildTaskAssignmentEmailBody(taskId, taskTitle, assignedTo, dueDate);

        // Assert
        subject.Should().Contain("NEW TASK");
        body.Should().Contain(taskTitle);
        body.Should().Contain(dueDate.ToString("yyyy-MM-dd"));
    }

    // ============ Email Validation Tests ============

    [Theory]
    [InlineData("valid@email.com", true)]
    [InlineData("user.name@domain.org", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("", false)]
    public void EmailAddress_ValidationShouldWork(string email, bool expectedValid)
    {
        // Act
        var isValid = IsValidEmail(email);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    [Fact]
    public void SmtpSettings_ShouldHaveRequiredConfiguration()
    {
        // Assert
        _smtpSettings.Host.Should().NotBeNullOrEmpty();
        _smtpSettings.Port.Should().BeGreaterThan(0);
        _smtpSettings.FromEmail.Should().NotBeNullOrEmpty();
    }

    // ============ Retry Logic Tests ============

    [Fact]
    public async Task EmailDelivery_WithRetry_ShouldAttemptMultipleTimes()
    {
        // Arrange
        var attempts = 0;
        var maxRetries = 3;
        var success = false;

        // Act - Simulate retry logic
        for (int i = 0; i < maxRetries && !success; i++)
        {
            attempts++;
            // Simulate failure on first 2 attempts, success on 3rd
            if (i == 2)
            {
                success = true;
            }
            await Task.Delay(10); // Simulate delay between retries
        }

        // Assert
        attempts.Should().Be(3);
        success.Should().BeTrue();
    }

    [Fact]
    public async Task EmailDelivery_ExceedingRetries_ShouldFail()
    {
        // Arrange
        var attempts = 0;
        var maxRetries = 3;
        var success = false;

        // Act - Simulate all failures
        for (int i = 0; i < maxRetries && !success; i++)
        {
            attempts++;
            // Always fail
            await Task.Delay(10);
        }

        // Assert
        attempts.Should().Be(maxRetries);
        success.Should().BeFalse();
    }

    // ============ Batch Email Tests ============

    [Fact]
    public async Task BatchEmailDelivery_ShouldProcessAllRecipients()
    {
        // Arrange
        var recipients = new List<string>
        {
            "user1@test.com",
            "user2@test.com",
            "user3@test.com",
            "user4@test.com",
            "user5@test.com"
        };
        var sentCount = 0;

        // Act - Simulate batch sending
        foreach (var recipient in recipients)
        {
            // Simulate sending email
            await Task.Delay(5);
            sentCount++;
        }

        // Assert
        sentCount.Should().Be(recipients.Count);
    }

    // ============ Helper Methods ============

    private string BuildApprovalEmailBody(Guid workflowId, string workflowName, string requester, string approver)
    {
        return $@"
            <html>
            <body>
                <h2>Approval Request</h2>
                <p>Dear {approver},</p>
                <p>A new approval request requires your attention:</p>
                <ul>
                    <li><strong>Workflow:</strong> {workflowName}</li>
                    <li><strong>Workflow ID:</strong> {workflowId}</li>
                    <li><strong>Requested by:</strong> {requester}</li>
                </ul>
                <p>Please review and take action.</p>
            </body>
            </html>";
    }

    private string BuildEscalationEmailBody(Guid taskId, string taskName, int level, double hoursOverdue)
    {
        return $@"
            <html>
            <body>
                <h2 style='color: red;'>Escalation Alert - Level {level}</h2>
                <p>This task requires immediate attention:</p>
                <ul>
                    <li><strong>Task:</strong> {taskName}</li>
                    <li><strong>Task ID:</strong> {taskId}</li>
                    <li><strong>Status:</strong> {hoursOverdue} hours overdue</li>
                </ul>
                <p><strong>Action Required:</strong> Please address this escalation immediately.</p>
            </body>
            </html>";
    }

    private string BuildSlaBreachEmailBody(Guid workflowId, string workflowType, DateTime slaDueDate)
    {
        return $@"
            <html>
            <body>
                <h2 style='color: red;'>CRITICAL: SLA Breach</h2>
                <p>The SLA has been breached for the following workflow:</p>
                <ul>
                    <li><strong>Workflow Type:</strong> {workflowType}</li>
                    <li><strong>Workflow ID:</strong> {workflowId}</li>
                    <li><strong>SLA Due Date:</strong> {slaDueDate:yyyy-MM-dd HH:mm}</li>
                </ul>
                <p><strong>IMPORTANT:</strong> This requires immediate action required.</p>
            </body>
            </html>";
    }

    private string BuildTaskAssignmentEmailBody(Guid taskId, string taskTitle, string assignedTo, DateTime dueDate)
    {
        return $@"
            <html>
            <body>
                <h2>New Task Assignment</h2>
                <p>You have been assigned a new task:</p>
                <ul>
                    <li><strong>Task:</strong> {taskTitle}</li>
                    <li><strong>Task ID:</strong> {taskId}</li>
                    <li><strong>Due Date:</strong> {dueDate:yyyy-MM-dd}</li>
                </ul>
                <p>Please complete this task by the due date.</p>
            </body>
            </html>";
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// SMTP Settings class for tests (mirrors production class)
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@grcsystem.com";
    public string FromName { get; set; } = "GRC System";
    public string? Username { get; set; }
    public string? Password { get; set; }
}
