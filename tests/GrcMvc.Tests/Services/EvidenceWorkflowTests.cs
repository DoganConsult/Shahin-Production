using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Data.Repositories;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Enums;
using GrcMvc.Services.Implementations;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrcMvc.Tests.Services;

/// <summary>
/// Unit tests for EvidenceWorkflowService - State machine transitions and workflow operations
/// Tests the Submit → Review → Approve/Reject → Archive workflow
/// </summary>
public class EvidenceWorkflowTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<EvidenceWorkflowService>> _mockLogger;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IUserDirectoryService> _mockUserDirectoryService;
    private readonly Mock<IGenericRepository<Evidence>> _mockEvidenceRepo;
    private readonly EvidenceWorkflowService _service;

    public EvidenceWorkflowTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<EvidenceWorkflowService>>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockUserDirectoryService = new Mock<IUserDirectoryService>();
        _mockEvidenceRepo = new Mock<IRepository<Evidence>>();

        _mockUnitOfWork.Setup(u => u.Evidences).Returns(_mockEvidenceRepo.Object);
        _mockUserDirectoryService.Setup(u => u.GetUsersInRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ApplicationUser>());

        _service = new EvidenceWorkflowService(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockNotificationService.Object,
            _mockUserDirectoryService.Object);
    }

    #region State Machine Tests

    [Theory]
    [InlineData(EvidenceVerificationStatus.Draft, EvidenceVerificationStatus.Pending, true)]
    [InlineData(EvidenceVerificationStatus.Pending, EvidenceVerificationStatus.UnderReview, true)]
    [InlineData(EvidenceVerificationStatus.UnderReview, EvidenceVerificationStatus.Verified, true)]
    [InlineData(EvidenceVerificationStatus.UnderReview, EvidenceVerificationStatus.Rejected, true)]
    [InlineData(EvidenceVerificationStatus.Rejected, EvidenceVerificationStatus.Pending, true)]
    [InlineData(EvidenceVerificationStatus.Verified, EvidenceVerificationStatus.Archived, true)]
    public void EvidenceStateMachine_CanTransition_ValidTransitions(
        EvidenceVerificationStatus from,
        EvidenceVerificationStatus to,
        bool expected)
    {
        // Act
        var result = EvidenceStateMachine.CanTransition(from, to);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(EvidenceVerificationStatus.Draft, EvidenceVerificationStatus.Verified)]
    [InlineData(EvidenceVerificationStatus.Pending, EvidenceVerificationStatus.Verified)]
    [InlineData(EvidenceVerificationStatus.Verified, EvidenceVerificationStatus.UnderReview)]
    [InlineData(EvidenceVerificationStatus.Archived, EvidenceVerificationStatus.Pending)]
    [InlineData(EvidenceVerificationStatus.Rejected, EvidenceVerificationStatus.Verified)]
    public void EvidenceStateMachine_CanTransition_InvalidTransitions_ReturnsFalse(
        EvidenceVerificationStatus from,
        EvidenceVerificationStatus to)
    {
        // Act
        var result = EvidenceStateMachine.CanTransition(from, to);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvidenceStateMachine_CanTransition_SameState_ReturnsTrue()
    {
        // All same-state transitions should be valid (no-op)
        foreach (EvidenceVerificationStatus status in Enum.GetValues<EvidenceVerificationStatus>())
        {
            Assert.True(EvidenceStateMachine.CanTransition(status, status));
        }
    }

    [Fact]
    public void EvidenceStateMachine_Archived_IsTerminal()
    {
        // Archived should have no valid transitions
        var validTransitions = EvidenceStateMachine.GetValidTransitions(EvidenceVerificationStatus.Archived);
        Assert.Empty(validTransitions);
    }

    #endregion

    #region SubmitForReviewAsync Tests

    [Fact]
    public async Task SubmitForReviewAsync_WithDraftEvidence_TransitionsToUnderReview()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Draft");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act
        var result = await _service.SubmitForReviewAsync(evidenceId, "test-user");

        // Assert
        Assert.Equal("Under Review", result.VerificationStatus);
        _mockEvidenceRepo.Verify(r => r.UpdateAsync(It.IsAny<Evidence>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithPendingEvidence_TransitionsToUnderReview()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Pending");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act
        var result = await _service.SubmitForReviewAsync(evidenceId, "test-user");

        // Assert
        Assert.Equal("Under Review", result.VerificationStatus);
    }

    [Fact]
    public async Task SubmitForReviewAsync_NotifiesReviewers()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Draft");
        var reviewers = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", Email = "reviewer1@test.com" },
            new ApplicationUser { Id = "user2", Email = "reviewer2@test.com" }
        };

        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);
        _mockUserDirectoryService.Setup(u => u.GetUsersInRoleAsync("ComplianceManager"))
            .ReturnsAsync(reviewers);

        // Act
        await _service.SubmitForReviewAsync(evidenceId, "test-user");

        // Assert - notification sent to reviewers
        _mockNotificationService.Verify(
            n => n.SendNotificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                "EvidenceReview",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithNonExistentEvidence_ThrowsWorkflowNotFoundException()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync((Evidence?)null);

        // Act & Assert
        await Assert.ThrowsAsync<WorkflowNotFoundException>(
            () => _service.SubmitForReviewAsync(evidenceId, "test-user"));
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithVerifiedEvidence_ThrowsInvalidStateTransition()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Verified");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStateTransitionException>(
            () => _service.SubmitForReviewAsync(evidenceId, "test-user"));
    }

    #endregion

    #region ApproveAsync Tests

    [Fact]
    public async Task ApproveAsync_WithUnderReviewEvidence_TransitionsToVerified()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Under Review");
        evidence.CollectedBy = "submitter@test.com";
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act
        var result = await _service.ApproveAsync(evidenceId, "approver-user", "Looks good");

        // Assert
        Assert.Equal("Verified", result.VerificationStatus);
        Assert.Equal("approver-user", result.VerifiedBy);
        Assert.NotNull(result.VerificationDate);
        Assert.Equal("Looks good", result.Comments);
    }

    [Fact]
    public async Task ApproveAsync_SetsVerificationDate()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Under Review");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        var beforeApproval = DateTime.UtcNow;

        // Act
        var result = await _service.ApproveAsync(evidenceId, "approver-user");

        // Assert
        Assert.NotNull(result.VerificationDate);
        Assert.True(result.VerificationDate >= beforeApproval);
    }

    [Fact]
    public async Task ApproveAsync_WithPendingEvidence_ThrowsInvalidStateTransition()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Pending");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStateTransitionException>(
            () => _service.ApproveAsync(evidenceId, "approver-user"));
    }

    #endregion

    #region RejectAsync Tests

    [Fact]
    public async Task RejectAsync_WithUnderReviewEvidence_TransitionsToRejected()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Under Review");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act
        var result = await _service.RejectAsync(evidenceId, "reviewer-user", "Missing documentation");

        // Assert
        Assert.Equal("Rejected", result.VerificationStatus);
        Assert.Equal("reviewer-user", result.VerifiedBy);
        Assert.Equal("Missing documentation", result.Comments);
    }

    [Fact]
    public async Task RejectAsync_WithDraftEvidence_ThrowsInvalidStateTransition()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Draft");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStateTransitionException>(
            () => _service.RejectAsync(evidenceId, "reviewer-user", "Reason"));
    }

    #endregion

    #region ArchiveAsync Tests

    [Fact]
    public async Task ArchiveAsync_WithVerifiedEvidence_TransitionsToArchived()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Verified");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act
        var result = await _service.ArchiveAsync(evidenceId, "archive-user");

        // Assert
        Assert.Equal("Archived", result.VerificationStatus);
    }

    [Fact]
    public async Task ArchiveAsync_WithUnderReviewEvidence_ThrowsInvalidStateTransition()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Under Review");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStateTransitionException>(
            () => _service.ArchiveAsync(evidenceId, "archive-user"));
    }

    #endregion

    #region ResubmitAsync Tests

    [Fact]
    public async Task ResubmitAsync_WithRejectedEvidence_TransitionsToPending()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Rejected");
        evidence.Comments = "Previous rejection reason";
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act
        var result = await _service.ResubmitAsync(evidenceId, "submitter-user");

        // Assert
        Assert.Equal("Pending", result.VerificationStatus);
        Assert.Equal(string.Empty, result.Comments); // Comments cleared
    }

    [Fact]
    public async Task ResubmitAsync_WithVerifiedEvidence_ThrowsInvalidStateTransition()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Verified");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStateTransitionException>(
            () => _service.ResubmitAsync(evidenceId, "submitter-user"));
    }

    #endregion

    #region Complete Workflow Tests

    [Fact]
    public async Task FullWorkflow_DraftToArchived_SuccessPath()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Draft");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert - Complete workflow

        // Step 1: Submit for review
        await _service.SubmitForReviewAsync(evidenceId, "submitter");
        Assert.Equal("Under Review", evidence.VerificationStatus);

        // Step 2: Approve
        await _service.ApproveAsync(evidenceId, "approver", "Approved");
        Assert.Equal("Verified", evidence.VerificationStatus);

        // Step 3: Archive
        await _service.ArchiveAsync(evidenceId, "archivist");
        Assert.Equal("Archived", evidence.VerificationStatus);
    }

    [Fact]
    public async Task FullWorkflow_WithRejectionAndResubmission_SuccessPath()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Draft");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        // Act & Assert

        // Step 1: Submit for review
        await _service.SubmitForReviewAsync(evidenceId, "submitter");
        Assert.Equal("Under Review", evidence.VerificationStatus);

        // Step 2: Reject
        await _service.RejectAsync(evidenceId, "reviewer", "Needs more detail");
        Assert.Equal("Rejected", evidence.VerificationStatus);

        // Step 3: Resubmit
        await _service.ResubmitAsync(evidenceId, "submitter");
        Assert.Equal("Pending", evidence.VerificationStatus);

        // Step 4: Submit for review again
        await _service.SubmitForReviewAsync(evidenceId, "submitter");
        Assert.Equal("Under Review", evidence.VerificationStatus);

        // Step 5: Approve
        await _service.ApproveAsync(evidenceId, "reviewer", "Now looks good");
        Assert.Equal("Verified", evidence.VerificationStatus);
    }

    #endregion

    #region Modification Tracking Tests

    [Fact]
    public async Task SubmitForReviewAsync_UpdatesModifiedDateAndBy()
    {
        // Arrange
        var evidenceId = Guid.NewGuid();
        var evidence = CreateEvidence(evidenceId, "Draft");
        _mockEvidenceRepo.Setup(r => r.GetByIdAsync(evidenceId)).ReturnsAsync(evidence);

        var beforeSubmit = DateTime.UtcNow;

        // Act
        await _service.SubmitForReviewAsync(evidenceId, "test-user");

        // Assert
        Assert.True(evidence.ModifiedDate >= beforeSubmit);
        Assert.Equal("test-user", evidence.ModifiedBy);
    }

    #endregion

    #region Helper Methods

    private Evidence CreateEvidence(Guid id, string status)
    {
        return new Evidence
        {
            Id = id,
            Title = "Test Evidence",
            VerificationStatus = status,
            EvidenceNumber = $"EV-{DateTime.UtcNow:yyyyMMdd}-TEST",
            TenantId = Guid.NewGuid()
        };
    }

    #endregion
}
