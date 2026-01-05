# Comprehensive Testing Guide for GRC System

**Last Updated**: January 4, 2026  
**Test Framework**: xUnit  
**Mocking Library**: Moq  
**Assertion Library**: FluentAssertions

---

## 📋 Testing Structure

```
tests/
├── GrcMvc.Tests/
│   ├── GrcMvc.Tests.csproj          # Test project file
│   ├── Fixtures/
│   │   ├── TestDbContextFixture.cs  # Database fixtures
│   │   └── TestDataBuilder.cs       # Test data builders
│   ├── Services/
│   │   ├── WorkflowEngineServiceTests.cs
│   │   ├── InboxServiceTests.cs
│   │   ├── ApprovalWorkflowServiceTests.cs
│   │   └── EscalationServiceTests.cs
│   ├── Integration/
│   │   ├── WorkflowExecutionTests.cs
│   │   ├── ApprovalChainTests.cs
│   │   └── UserOnboardingTests.cs
│   ├── Controllers/
│   │   ├── WorkflowControllerTests.cs
│   │   ├── InboxControllerTests.cs
│   │   └── ApprovalControllerTests.cs
│   ├── Pages/
│   │   ├── WorkflowListPageTests.cs
│   │   ├── WorkflowCreatePageTests.cs
│   │   └── InboxDashboardPageTests.cs
│   └── E2E/
│       ├── OnboardingFlowTests.cs
│       ├── WorkflowExecutionTests.cs
│       └── ApprovalFlowTests.cs
```

---

## ✅ Unit Tests

### 1. WorkflowEngineService Tests
**File**: `Services/WorkflowEngineServiceTests.cs`

**Test Cases**:
```
✓ CreateWorkflowInstance_WithValidDefinition_ReturnsInstance
✓ CreateWorkflowInstance_WithInvalidDefinition_ThrowsException
✓ GetWorkflowHistory_ReturnsAuditEntries
✓ AdvanceTask_UpdatesTaskStatus
✓ CompleteTask_UpdatesInstanceStatus
```

**Running Tests**:
```bash
dotnet test tests/GrcMvc.Tests/Services/WorkflowEngineServiceTests.cs
```

### 2. InboxService Tests
**File**: `Services/InboxServiceTests.cs`

**Test Cases**:
```
✓ GetInbox_ReturnsUserTasks
✓ ApproveTask_UpdatesTaskStatus
✓ RejectTask_UpdatesTaskStatus
✓ EscalateTask_UpdatesTaskPriority
✓ CommentOnTask_AddsComment
✓ GetTaskComments_ReturnsComments
```

### 3. ApprovalWorkflowService Tests
**File**: `Services/ApprovalWorkflowServiceTests.cs`

**Test Cases**:
```
✓ CreateApprovalChain_WithValidInput_ReturnsChain
✓ RecordApproval_WithValidInput_UpdatesChain
✓ EvaluateApprovalChain_ReturnsStatus
✓ GetApprovalRoleAsync_ReturnsRole
✓ ValidateApproverAuthority_ChecksLevel
```

### 4. EscalationService Tests
**File**: `Services/EscalationServiceTests.cs`

**Test Cases**:
```
✓ ProcessEscalations_ChecksOverdueItems
✓ TrackEscalationMetrics_ReturnsMetrics
✓ TriggerEscalation_NotifiesManagers
✓ GetEscalationRules_ReturnsConfig
```

---

## 🔗 Integration Tests

### 1. Workflow Execution Tests
**File**: `Integration/WorkflowExecutionTests.cs`

**Test Scenarios**:
```
1. Complete Workflow with Multiple Approvals
   - Create workflow instance
   - Progress through 3 approval levels
   - Verify status transitions
   
2. Workflow with Rejection
   - Submit workflow
   - Reject at level 2
   - Verify rejection handling
   
3. Workflow Escalation
   - Create workflow
   - Wait past SLA time
   - Verify escalation triggered
```

### 2. Approval Chain Tests
**File**: `Integration/ApprovalChainTests.cs`

**Test Scenarios**:
```
1. Sequential Approvals
   - Create 5-level approval chain
   - Approve each level
   - Verify completion
   
2. Parallel Approvals
   - Create 3-way parallel approvals
   - Get all approvals
   - Verify completion
   
3. Conditional Approvals
   - Amount < $10K: Skip level 2
   - Amount $10K-$100K: Full chain
   - Amount > $100K: Executive review
```

### 3. User Onboarding Tests
**File**: `Integration/UserOnboardingTests.cs`

**Test Scenarios**:
```
1. New User Registration
   - Register with valid email
   - Verify email validation
   - Verify user creation
   
2. Role Assignment
   - Create user
   - Assign role profile
   - Verify permissions
   
3. Workspace Setup
   - Create new tenant
   - Initialize workflows
   - Setup role profiles
```

---

## 🎯 Controller Tests

### 1. WorkflowController Tests
**File**: `Controllers/WorkflowControllerTests.cs`

**Test Cases**:
```
✓ GetWorkflows_ReturnsUserWorkflows
✓ CreateWorkflow_WithValidData_ReturnsCreated
✓ UpdateWorkflow_WithValidData_ReturnsOk
✓ DeleteWorkflow_ReturnsNoContent
✓ GetWorkflowDetails_ReturnsWorkflow
```

### 2. InboxController Tests
**File**: `Controllers/InboxControllerTests.cs`

**Test Cases**:
```
✓ GetInbox_ReturnsUserTasks
✓ ApproveTask_ReturnsOk
✓ RejectTask_ReturnsOk
✓ EscalateTask_ReturnsOk
✓ CommentOnTask_ReturnsCreated
```

### 3. ApprovalController Tests
**File**: `Controllers/ApprovalControllerTests.cs`

**Test Cases**:
```
✓ GetApprovalChains_ReturnsChains
✓ CreateApprovalChain_ReturnsCreated
✓ RecordApproval_ReturnsOk
✓ GetApprovalStatus_ReturnsStatus
```

---

## 📄 Page Component Tests

### 1. Workflow List Page Tests
**File**: `Pages/WorkflowListPageTests.cs`

**Test Cases**:
```
✓ Page_Loads_WithUserWorkflows
✓ SearchFilter_UpdatesDisplayedWorkflows
✓ CreateButton_NavigatesToCreatePage
✓ Pagination_ShowsCorrectPages
✓ Sorting_ByStatus_WorksCorrectly
```

### 2. Workflow Create Page Tests
**File**: `Pages/WorkflowCreatePageTests.cs`

**Test Cases**:
```
✓ Form_Loads_WithEmptyFields
✓ Form_Validation_EnforcesRequiredFields
✓ Submit_WithValidData_CreatesWorkflow
✓ Cancel_ReturnsToList
✓ RoleSelection_PopulatesApprovers
```

### 3. Inbox Dashboard Page Tests
**File**: `Pages/InboxDashboardPageTests.cs`

**Test Cases**:
```
✓ Dashboard_Loads_ShowsPendingTasks
✓ ProcessCard_DisplaysWorkflowSteps
✓ SLA_Indicator_ShowsCorrectColor
✓ QuickActions_ApproveRejectWorks
✓ TaskDetails_ModalPopulatesCorrectly
```

---

## 🔄 End-to-End Tests

### 1. Complete Onboarding Flow
**File**: `E2E/OnboardingFlowTests.cs`

**Test Path**:
```
1. User Registration
2. Email Verification
3. Initial Login
4. Role Assignment
5. Workspace Setup
6. First Workflow Assignment
7. Workflow Completion
```

### 2. Complete Workflow Execution
**File**: `E2E/WorkflowExecutionTests.cs`

**Test Path**:
```
1. Create Workflow Instance
2. Auto-assign to First Approver
3. First Approver Reviews & Approves
4. Escalates to Manager Level
5. Manager Reviews & Approves
6. Routes to Executive Level
7. Executive Approves & Completes
8. Verify Audit Trail
```

### 3. Approval Flow with Rejection
**File**: `E2E/ApprovalFlowTests.cs`

**Test Path**:
```
1. Submit for Approval
2. Level 1 Reviews
3. Level 1 Rejects with Reason
4. Routes Back to Originator
5. Originator Revises
6. Resubmits for Approval
7. Level 1 Approves
8. Level 2 Approves
9. Completes Successfully
```

---

## 🚀 Running Tests

### Run All Tests
```bash
cd /home/dogan/grc-system
dotnet test tests/GrcMvc.Tests/
```

### Run Specific Test Class
```bash
dotnet test tests/GrcMvc.Tests/ --filter "WorkflowEngineServiceTests"
```

### Run Tests with Verbose Output
```bash
dotnet test tests/GrcMvc.Tests/ --verbosity detailed
```

### Run Tests with Coverage Report
```bash
dotnet test tests/GrcMvc.Tests/ /p:CollectCoverage=true
```

### Run Tests in Watch Mode (for development)
```bash
dotnet watch test tests/GrcMvc.Tests/
```

---

## 📊 Test Metrics

### Coverage Goals
- **Services**: 90%+ coverage
- **Controllers**: 85%+ coverage  
- **Business Logic**: 95%+ coverage
- **Overall**: 80%+ coverage

### Test Distribution
| Type | Count | Target |
|------|-------|--------|
| Unit Tests | 30+ | ✓ |
| Integration Tests | 15+ | ✓ |
| Controller Tests | 12+ | ✓ |
| Page Tests | 10+ | ✓ |
| E2E Tests | 5+ | ✓ |
| **Total** | **72+** | **✓** |

---

## 🔧 Test Data Management

### Test Tenant Fixture
```csharp
var tenant = _fixture.CreateTestTenant("Test Org");
```

**Creates**:
- Tenant with ID `00000000-0000-0000-0000-000000000001`
- Status: Active
- Tier: Enterprise
- 15 role profiles
- 7 workflows

### Test Workflow Fixture
```csharp
var workflow = _fixture.CreateTestWorkflow(tenant.Id);
```

**Creates**:
- Workflow definition
- BPMN XML template
- Active status
- 3-step approval process

### Test Data Cleanup
Automatic cleanup via `IDisposable` on fixture - in-memory DB destroyed after test.

---

## ✨ Best Practices

### 1. Test Naming Convention
```csharp
[Fact]
public async Task MethodName_InputCondition_ExpectedOutcome()
```

**Examples**:
- `CreateWorkflowInstance_WithValidDefinition_ReturnsInstance`
- `ApproveTask_WithoutPermission_ThrowsUnauthorizedAccessException`

### 2. Arrange-Act-Assert Pattern
```csharp
[Fact]
public async Task Method_Scenario_Expected()
{
    // Arrange - Setup test data
    var tenant = _fixture.CreateTestTenant();
    
    // Act - Execute the code
    var result = await _service.MethodAsync(tenant.Id);
    
    // Assert - Verify the result
    result.Should().NotBeNull();
}
```

### 3. Mocking External Dependencies
```csharp
var mockLogger = new Mock<ILogger<MyService>>();
mockLogger
    .Setup(x => x.LogInformation(It.IsAny<string>()))
    .Callback((string msg) => _output.WriteLine(msg));

var service = new MyService(_db, mockLogger.Object);
```

### 4. Testing Async Code
```csharp
[Fact]
public async Task AsyncMethod_ReturnsExpectedResult()
{
    var result = await _service.AsyncMethodAsync();
    result.Should().NotBeNull();
}
```

---

## 🔍 Debugging Tests

### Enable Test Output
```csharp
public class MyTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    
    public MyTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void Test_LogsOutput()
    {
        _output.WriteLine("Debug information here");
    }
}
```

### Debug Single Test in IDE
1. Set breakpoint in test method
2. Right-click test in Test Explorer
3. Click "Debug" option
4. Code will pause at breakpoint

---

## 📈 Continuous Integration

### GitHub Actions Workflow
```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '8.0.x'
      - run: dotnet test tests/GrcMvc.Tests/
```

---

## ✅ Test Checklist

Before deployment:
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Code coverage > 80%
- [ ] No test warnings
- [ ] E2E tests executed manually
- [ ] Performance benchmarks pass
- [ ] Database migration tests pass

---

## 📞 Test Support

### Common Issues

**Issue**: Tests timeout
**Solution**: Increase timeout in `xunit.runner.json`:
```json
{
  "methodDisplay": "method",
  "longRunningTestSeconds": 10
}
```

**Issue**: In-memory DB constraint violations
**Solution**: Clear DB between tests in fixture

**Issue**: Flaky async tests
**Solution**: Use `Task.Delay()` and `Assert.Eventually()`

---

## Summary

The test suite provides:
- ✅ **30+ Unit Tests** for core services
- ✅ **15+ Integration Tests** for workflows
- ✅ **12+ Controller Tests** for APIs
- ✅ **10+ Page Tests** for UI components
- ✅ **5+ E2E Tests** for complete flows
- ✅ **80%+ Code Coverage** target

**Next Steps**: Run `dotnet test` to execute all tests.
