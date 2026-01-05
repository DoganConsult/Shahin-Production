# GRC System - Detailed Test Plan

**Document Version**: 1.0  
**Date Created**: January 4, 2026  
**Project**: GRC Management System (STAGE 2)  
**Test Lead**: QA Team  
**Status**: In Development

---

## 📋 Test Plan Overview

This document defines the comprehensive testing strategy for the GRC Governance System, including:
- Unit testing approach for services
- Integration testing for workflows
- Component testing for UI pages
- End-to-end user flow testing
- Performance and load testing
- Security testing

---

## 🎯 Testing Objectives

1. **Functional Testing**: Verify all features work as designed
2. **Integration Testing**: Verify components work together correctly
3. **Performance Testing**: Verify system meets performance SLAs
4. **Security Testing**: Verify data protection and access controls
5. **Regression Testing**: Prevent bugs from recurring
6. **User Acceptance Testing**: Verify business requirements

---

## 📊 Test Scope

### In Scope
- ✅ Workflow creation and execution
- ✅ Approval chain processing
- ✅ User authentication and authorization
- ✅ Multi-tenant isolation
- ✅ Role-based access control
- ✅ SLA tracking and escalation
- ✅ Inbox and task management
- ✅ API endpoints
- ✅ Blazor UI components
- ✅ Database operations

### Out of Scope
- ❌ Third-party library testing
- ❌ Browser compatibility testing (browser-specific)
- ❌ Load testing >1000 concurrent users
- ❌ Penetration testing (separate engagement)

---

## 🧪 Test Levels

### 1. Unit Tests (30+ tests)

#### WorkflowEngineService (8 tests)
```
Test 1.1: CreateWorkflowInstance_WithValidDefinition
  - Input: Valid workflow definition
  - Expected: Instance created with correct status
  - Assertion: Instance.Status = "InProgress"

Test 1.2: CreateWorkflowInstance_WithInvalidDefinition
  - Input: Non-existent definition ID
  - Expected: Exception thrown
  - Assertion: InvalidOperationException

Test 1.3: AdvanceTask_UpdatesStatus
  - Input: Task ID, new status
  - Expected: Task status updated
  - Assertion: Database reflects change

Test 1.4: CompleteTask_UpdatesInstance
  - Input: Task completion
  - Expected: Instance may auto-complete if all done
  - Assertion: Instance.CompletedAt set

Test 1.5: GetWorkflowHistory_ReturnsAuditLog
  - Input: Workflow instance ID
  - Expected: List of audit entries
  - Assertion: Entries ordered by date

Test 1.6: GetWorkflowHistory_FiltersByAction
  - Input: Filter by "Approved"
  - Expected: Only approval entries returned
  - Assertion: Count matches approvals only

Test 1.7: CreateWorkflowInstance_SetsTenantContext
  - Input: Multi-tenant scenario
  - Expected: Correct tenant isolation
  - Assertion: TenantId matches request

Test 1.8: WorkflowInstance_EnforcesTotusValidation
  - Input: Invalid status value
  - Expected: Validation error
  - Assertion: Exception with message
```

#### InboxService (7 tests)
```
Test 2.1: GetInbox_ReturnsUserTasks
  - Input: User ID
  - Expected: Tasks assigned to user
  - Assertion: All tasks have UserId = input

Test 2.2: ApproveTask_UpdatesStatus
  - Input: Task ID, approval details
  - Expected: Task marked as approved
  - Assertion: Task.Status = "Approved"

Test 2.3: RejectTask_UpdatesStatus
  - Input: Task ID, rejection reason
  - Expected: Task marked as rejected
  - Assertion: Task.Status = "Rejected", reason stored

Test 2.4: EscalateTask_UpdatesPriority
  - Input: Task ID, escalation reason
  - Expected: Priority increased, escalated flag set
  - Assertion: Task.Priority increased, EscalatedAt set

Test 2.5: CommentOnTask_AddsComment
  - Input: Task ID, comment text, user
  - Expected: Comment added to task
  - Assertion: Comment in database

Test 2.6: GetTaskComments_ReturnsAll
  - Input: Task ID
  - Expected: All comments for task
  - Assertion: Count matches added comments

Test 2.7: InboxFilter_ByStatus
  - Input: Filter by "Pending"
  - Expected: Only pending tasks
  - Assertion: All returned tasks status = "Pending"
```

#### ApprovalWorkflowService (7 tests)
```
Test 3.1: CreateApprovalChain_WithLevels
  - Input: Workflow ID, approval levels
  - Expected: Chain created with all levels
  - Assertion: Chain.LevelCount = input count

Test 3.2: RecordApproval_UpdatesChain
  - Input: Approval decision
  - Expected: Current level marked, next triggered
  - Assertion: CurrentLevel incremented

Test 3.3: EvaluateApprovalChain_AllApproved
  - Input: All approvals done
  - Expected: Status = "Completed"
  - Assertion: Evaluation returns completed status

Test 3.4: EvaluateApprovalChain_OneRejected
  - Input: One approval rejected
  - Expected: Status = "Rejected"
  - Assertion: Evaluation returns rejected status

Test 3.5: GetApprovalRoleAsync_ReturnsCorrectRole
  - Input: Level number
  - Expected: Role profile for that level
  - Assertion: Role authority level matches

Test 3.6: ValidateApproverAuthority_Succeeds
  - Input: Approver with sufficient authority
  - Expected: Validation passes
  - Assertion: No exception

Test 3.7: ValidateApproverAuthority_Fails
  - Input: Approver with insufficient authority
  - Expected: Validation fails
  - Assertion: UnauthorizedAccessException
```

#### EscalationService (5 tests)
```
Test 4.1: ProcessEscalations_FindsOverdueItems
  - Input: Items past SLA time
  - Expected: Identified and escalated
  - Assertion: Count > 0, escalated flag set

Test 4.2: ProcessEscalations_NoOverdueItems
  - Input: All items within SLA
  - Expected: No escalations
  - Assertion: Count = 0

Test 4.3: TrackEscalationMetrics_ReturnsStats
  - Input: Date range
  - Expected: Metrics calculated
  - Assertion: Metrics object has correct structure

Test 4.4: TriggerEscalation_NotifiesManager
  - Input: Task to escalate
  - Expected: Manager notified
  - Assertion: Notification created

Test 4.5: EscalationRule_AppliedCorrectly
  - Input: Rule configuration
  - Expected: Rule evaluated correctly
  - Assertion: Rule result matches expected
```

### 2. Integration Tests (15+ tests)

#### Workflow Execution Flow
```
Test 5.1: Single-Level Approval
  - Create workflow → Submit → Approve → Complete
  - Assert: Final status = "Completed"

Test 5.2: Multi-Level Approvals (3 levels)
  - Create → Level 1 approve → Level 2 approve → Level 3 approve → Complete
  - Assert: All transitions successful

Test 5.3: Approval with Rejection
  - Create → Approve at L1 → Reject at L2 → Route back → Resubmit → Complete
  - Assert: Proper state transitions

Test 5.4: Escalation on SLA Breach
  - Create → Wait past SLA → Trigger escalation → Manager approves → Complete
  - Assert: Escalation flag set, manager notified

Test 5.5: Parallel Approvals
  - Create → 3 parallel approvers → All approve → Auto-complete
  - Assert: Completed when all done

Test 5.6: Conditional Approvals
  - Amount < $10K → Skip level 2 → Complete faster
  - Amount > $100K → Add executive level → Extended chain
  - Assert: Logic applied correctly

Test 5.7: Workflow with Comments
  - Create → Level 1 adds comments → Level 2 reads comments → Approves
  - Assert: Comments persisted and visible

Test 5.8: Multiple Concurrent Workflows
  - Create 10 workflows simultaneously
  - Each progresses independently
  - Assert: No cross-contamination, all track separately
```

#### User Onboarding
```
Test 6.1: New User Registration
  - Register → Verify → Login → Assign Role → Access Workflow
  - Assert: User successfully onboarded

Test 6.2: Role Assignment
  - Create user → Assign "Approver" role → Verify permissions
  - Assert: User can access approval tasks

Test 6.3: Multi-Tenant Isolation
  - Create 2 tenants → Create users in each → Verify isolation
  - Assert: Users only see their tenant's data

Test 6.4: Permission Escalation Prevention
  - Try to access higher-level approval as non-approver
  - Assert: Access denied, audit logged

Test 6.5: Workspace Setup
  - New tenant → Initialize workflows → Create users → Assign roles
  - Assert: Tenant fully operational
```

### 3. Component Tests (10+ tests)

#### Blazor Pages
```
Test 7.1: WorkflowListPage_Loads
  - Load page → Assert: Workflows displayed

Test 7.2: WorkflowListPage_Search
  - Type in search → Assert: List filtered

Test 7.3: WorkflowCreatePage_Validation
  - Submit empty form → Assert: Validation errors shown

Test 7.4: WorkflowCreatePage_Submit
  - Fill form → Submit → Assert: Workflow created

Test 7.5: InboxDashboard_ShowsPendingTasks
  - Load dashboard → Assert: User's tasks displayed

Test 7.6: InboxDashboard_SLA_Indicator
  - Task approaching SLA → Assert: Color changes to yellow
  - Task past SLA → Assert: Color changes to red

Test 7.7: ProcessCard_ShowsSteps
  - Load process card → Assert: All approval steps visible

Test 7.8: ProcessCard_InteractiveApproval
  - Click Approve button → Assert: Modal opens, approve submitted

Test 7.9: ApprovalList_FilterByStatus
  - Filter "Pending" → Assert: Only pending approvals shown

Test 7.10: ApprovalList_BulkActions
  - Select multiple → Click "Approve All" → Assert: All approved
```

### 4. API Endpoint Tests (12+ tests)

#### Workflow API
```
Test 8.1: GET /api/workflow
  - Call endpoint → Assert: 200 OK, returns list

Test 8.2: POST /api/workflow
  - Create with JSON → Assert: 201 Created, returns ID

Test 8.3: GET /api/workflow/{id}
  - Get specific → Assert: 200 OK, returns details

Test 8.4: PUT /api/workflow/{id}
  - Update definition → Assert: 200 OK, updates applied

Test 8.5: DELETE /api/workflow/{id}
  - Delete workflow → Assert: 204 No Content, deleted

Test 8.6: GET /api/workflow/{id}/history
  - Get audit log → Assert: 200 OK, audit entries returned

Test 8.7: POST /api/workflow/{id}/start
  - Start instance → Assert: 201 Created, instance created

Test 8.8: GET /api/workflow/instances
  - List instances → Assert: 200 OK, user's instances only
```

#### Approval API
```
Test 9.1: GET /api/approval-chain
  - List chains → Assert: 200 OK

Test 9.2: POST /api/approval-chain
  - Create chain → Assert: 201 Created

Test 9.3: POST /api/approval-chain/{id}/approve
  - Approve step → Assert: 200 OK, status updated

Test 9.4: POST /api/approval-chain/{id}/reject
  - Reject step → Assert: 200 OK, rejection recorded
```

#### Inbox API
```
Test 10.1: GET /api/inbox
  - Get tasks → Assert: 200 OK, user's tasks

Test 10.2: POST /api/inbox/task/{id}/approve
  - Approve task → Assert: 200 OK

Test 10.3: POST /api/inbox/task/{id}/comment
  - Add comment → Assert: 201 Created
```

### 5. End-to-End Tests (5+ tests)

```
Test 11.1: Complete Onboarding
  1. Register new user → 2. Verify email → 3. Login
  4. Accept role assignment → 5. Complete onboarding

Test 11.2: Complete Workflow
  1. Submit document → 2. L1 approver reviews and approves
  3. L2 approver reviews and approves → 4. L3 executive approves
  5. Workflow completes → 6. Requester notified

Test 11.3: Rejection and Resubmission
  1. Submit → 2. Reject at L2 → 3. Return to requester
  4. Requester revises → 5. Resubmit → 6. New approval chain starts
  7. Approvers approve → 8. Complete

Test 11.4: SLA Breach Escalation
  1. Submit workflow → 2. L1 approver inactive
  3. SLA time expires → 4. Escalation triggers
  5. Manager overrides → 6. Complete

Test 11.5: Multi-Tenant Isolation
  1. Create 2 organizations → 2. Create users in each
  3. Each user creates workflow → 4. Verify workflows don't cross-mingle
  5. Only see own org workflows
```

---

## ⏱️ Test Timeline

| Phase | Duration | Tests | Target |
|-------|----------|-------|--------|
| Unit Testing | 2 weeks | 30 | ✓ |
| Integration Testing | 2 weeks | 15 | ✓ |
| Component Testing | 1.5 weeks | 10 | ✓ |
| API Testing | 1 week | 12 | ✓ |
| E2E Testing | 1 week | 5 | ✓ |
| Performance Testing | 1 week | 5 | ✓ |
| **Total** | **8.5 weeks** | **77** | **✓** |

---

## 📈 Success Criteria

### Code Coverage
- **Services**: ≥90%
- **Controllers**: ≥85%
- **Overall**: ≥80%

### Test Results
- **Pass Rate**: 100%
- **Flaky Tests**: 0%
- **Skipped Tests**: 0%

### Performance
- **Unit Test Execution**: <10 seconds total
- **Integration Tests**: <30 seconds total
- **E2E Tests**: <2 minutes per scenario

### Security
- **Authentication Tests**: 100% pass
- **Authorization Tests**: 100% pass
- **Injection Tests**: All blocked

---

## 🔄 Test Data Management

### Test Data Strategy
1. **Fresh DB per test** - In-memory database
2. **Fixture-based setup** - Reusable test data
3. **Teardown cleanup** - Automatic via IDisposable

### Test Data Sets
```
Tenant Data:
- TenantId: 00000000-0000-0000-0000-000000000001
- Name: Test Tenant
- Tier: Enterprise

User Data:
- Approvers: 3 users (L1, L2, Executive)
- Requester: 1 user
- Roles: 5 predefined

Workflow Data:
- Definition: 7 templates
- Instances: 10 per test scenario
- Tasks: 30+ tasks
```

---

## 🛠️ Test Tools & Environment

### Testing Framework
- **Framework**: xUnit
- **Assertion**: FluentAssertions
- **Mocking**: Moq
- **Coverage**: OpenCover/Coverlet

### Test Environment
- **Database**: In-Memory (EF Core)
- **OS**: Linux/Windows/macOS
- **Runtime**: .NET 8.0
- **CI/CD**: GitHub Actions

---

## 📝 Test Execution Records

### Sample Test Run Output
```
[xUnit.net 00:00:00.00] GrcMvc.Tests.Services.WorkflowEngineServiceTests

[Fact] CreateWorkflowInstance_WithValidDefinition_ReturnsInstance PASSED (25ms)
[Fact] CreateWorkflowInstance_WithInvalidDefinition_ThrowsException PASSED (10ms)
[Fact] AdvanceTask_UpdatesTaskStatus PASSED (15ms)

Tests run: 3, Passed: 3, Failed: 0, Skipped: 0

Total time: 50ms
```

---

## ✅ Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Test Lead | QA Team | 2026-01-04 | ✓ |
| Development Lead | Dev Team | 2026-01-04 | ✓ |
| Project Manager | PM | TBD | |

---

## Appendix A: Test Naming Convention

```
[MethodName]_[InputCondition]_[ExpectedResult]

Examples:
- CreateWorkflowInstance_WithValidDefinition_ReturnsInstance
- ApproveTask_WithoutPermission_ThrowsUnauthorizedAccessException
- GetInbox_WithPendingTasks_ReturnsFilteredList
```

## Appendix B: Assertion Examples

```csharp
// Equality
result.Should().Be(expected);

// Null checks
result.Should().NotBeNull();

// Collections
list.Should().HaveCount(5);
list.Should().Contain(item);

// Exceptions
action.Should().Throw<InvalidOperationException>();

// Boolean
flag.Should().BeTrue();
```

## Appendix C: Mock Examples

```csharp
// Setup return value
mockService
    .Setup(x => x.GetItemAsync(It.IsAny<int>()))
    .ReturnsAsync(expectedItem);

// Verify call
mockService.Verify(
    x => x.LogAsync(It.IsAny<string>()),
    Times.Once());
```

---

**Document End**
