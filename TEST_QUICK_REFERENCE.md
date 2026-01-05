# GRC System - Test Quick Reference Guide

**Last Updated**: January 4, 2026

---

## 🎯 Quick Commands

### Run Everything (All Tests)
```bash
cd /home/dogan/grc-system
dotnet test tests/GrcMvc.Tests/
```

### Run Unit Tests Only
```bash
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~Services" \
  --logger "console;verbosity=normal"
```

### Run Integration Tests Only
```bash
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~Integration" \
  --logger "console;verbosity=normal"
```

### Run Component Tests Only
```bash
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~Components" \
  --logger "console;verbosity=normal"
```

### Run API Tests Only
```bash
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~Controllers" \
  --logger "console;verbosity=normal"
```

### Run E2E Tests Only
```bash
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~E2E" \
  --logger "console;verbosity=normal"
```

---

## 📊 Generate Reports

### Generate Coverage Report
```bash
dotnet test tests/GrcMvc.Tests/ \
  /p:CollectCoverage=true \
  /p:CoverageFormat=opencover \
  /p:CoverageDirectory=./coverage \
  /p:Exclude="[*.Tests]*"
```

### Generate Test Results XML (for CI/CD)
```bash
dotnet test tests/GrcMvc.Tests/ \
  --logger "trx;LogFileName=test-results.trx" \
  --results-directory ./test-results
```

### Generate Detailed HTML Report
```bash
dotnet test tests/GrcMvc.Tests/ \
  -v detailed \
  --logger "console;verbosity=detailed" \
  > test-results.txt
```

---

## 🔍 Run Specific Test

### Run Single Test Class
```bash
# Example: Run only WorkflowEngineService tests
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~WorkflowEngineServiceTests"
```

### Run Single Test Method
```bash
# Example: Run only one test
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~WorkflowEngineServiceTests.CreateWorkflowInstance_WithValidDefinition_ReturnsInstance"
```

### Run Tests by Category
```bash
# Replace "ServiceName" with actual service/component name
dotnet test tests/GrcMvc.Tests/ \
  --filter "ClassName~ServiceName"
```

---

## 📈 Test Monitoring

### Watch Mode (Auto-run on File Changes)
```bash
dotnet watch test tests/GrcMvc.Tests/ --no-exit-on-failure
```

### Verbose Output
```bash
dotnet test tests/GrcMvc.Tests/ \
  -v detailed \
  --logger "console;verbosity=detailed"
```

### Quiet Output (Summary Only)
```bash
dotnet test tests/GrcMvc.Tests/ -v quiet
```

---

## 🐛 Debugging Tests

### Run Single Test with Debug Info
```bash
dotnet test tests/GrcMvc.Tests/ \
  --filter "FullyQualifiedName~YourTestName" \
  -v detailed \
  --no-build
```

### Run with Longer Timeout
```bash
# Edit xunit.runner.json:
# "longRunningTestSeconds": 30
```

### List All Available Tests
```bash
dotnet test tests/GrcMvc.Tests/ --list-tests
```

### Count Total Tests
```bash
dotnet test tests/GrcMvc.Tests/ --list-tests | wc -l
```

---

## ✅ Verification Checklist

### Before Running Tests
- [ ] .NET 8.0 SDK installed: `dotnet --version`
- [ ] PostgreSQL running (if needed): `sudo service postgresql status`
- [ ] NuGet packages restored: `dotnet restore`
- [ ] Solution builds: `dotnet build`
- [ ] Application runs: `dotnet run --project src/GrcMvc`

### During Test Execution
- [ ] No build errors
- [ ] All tests found
- [ ] No timeout errors
- [ ] No database connection errors
- [ ] No mock setup errors

### After Test Execution
- [ ] Pass rate = 100%
- [ ] No skipped tests
- [ ] Coverage ≥ 80%
- [ ] No warnings
- [ ] Results saved

---

## 📊 Expected Results

### Unit Tests (31 tests)
```
Execution Time:     < 10 seconds
Expected Results:   31 PASSED, 0 FAILED
Coverage Target:    90%
```

### Integration Tests (13 tests)
```
Execution Time:     < 30 seconds
Expected Results:   13 PASSED, 0 FAILED
Coverage Target:    85%
```

### Component Tests (15 tests)
```
Execution Time:     < 20 seconds
Expected Results:   15 PASSED, 0 FAILED
Coverage Target:    80%
```

### Controller Tests (19 tests)
```
Execution Time:     < 15 seconds
Expected Results:   19 PASSED, 0 FAILED
Coverage Target:    85%
```

### E2E Tests (5 tests)
```
Execution Time:     < 60 seconds
Expected Results:   5 PASSED, 0 FAILED
Coverage Target:    75%
```

### ALL TESTS (83 tests)
```
Execution Time:     < 2 minutes
Expected Results:   83 PASSED, 0 FAILED
Overall Coverage:   80%
```

---

## 🚀 CI/CD Integration

### GitHub Actions Example
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
      - run: dotnet restore
      - run: dotnet build
      - run: dotnet test tests/GrcMvc.Tests/
```

### Azure Pipelines Example
```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.0.x'
  - script: dotnet restore
  - script: dotnet build
  - script: dotnet test tests/GrcMvc.Tests/
```

---

## 📋 Test File Organization

```
tests/GrcMvc.Tests/
├── GrcMvc.Tests.csproj              # Project configuration
├── TestDbContextFixture.cs          # In-memory DB setup
│
├── Services/                        # Unit tests for services
│   ├── WorkflowEngineServiceTests.cs
│   ├── InboxServiceTests.cs
│   ├── ApprovalWorkflowServiceTests.cs
│   ├── EscalationServiceTests.cs
│   └── UserWorkspaceServiceTests.cs
│
├── Integration/                     # Integration tests
│   ├── WorkflowExecutionTests.cs
│   └── ApprovalFlowTests.cs
│
├── Components/                      # Blazor component tests
│   ├── WorkflowListPageTests.cs
│   ├── InboxDashboardPageTests.cs
│   ├── ProcessCardComponentTests.cs
│   └── ApprovalListPageTests.cs
│
├── Controllers/                     # API endpoint tests
│   ├── WorkflowControllerTests.cs
│   ├── ApprovalControllerTests.cs
│   └── InboxControllerTests.cs
│
└── E2E/                            # End-to-end tests
    └── WorkflowEndToEndTests.cs
```

---

## 🎓 Test Examples

### Example 1: Simple Unit Test
```csharp
[Fact]
public async Task CreateWorkflowInstance_WithValidDefinition_ReturnsInstance()
{
    // Arrange - Setup
    var workflowId = Guid.NewGuid();
    var mockService = new Mock<WorkflowEngineService>();
    
    // Act - Execute
    var result = await mockService.Object.CreateWorkflowInstanceAsync(workflowId);
    
    // Assert - Verify
    result.Should().NotBeNull();
    result.Status.Should().Be("InProgress");
}
```

### Example 2: Integration Test with Database
```csharp
[Fact]
public async Task Workflow_MultiLevelApproval_Completes()
{
    // Arrange
    using var dbContext = new TestDbContext();
    var service = new ApprovalWorkflowService(dbContext);
    var workflow = dbContext.CreateTestWorkflow();
    
    // Act
    await service.RecordApprovalAsync(workflow.Id, userId1, "Approved", "");
    await service.RecordApprovalAsync(workflow.Id, userId2, "Approved", "");
    await service.RecordApprovalAsync(workflow.Id, userId3, "Approved", "");
    
    // Assert
    var result = await service.GetApprovalChainAsync(workflow.Id);
    result.Status.Should().Be("Completed");
}
```

### Example 3: Component Test
```csharp
[Fact]
public void InboxDashboard_ApproveButton_CallsService()
{
    // Arrange
    var mockInboxService = new Mock<InboxService>();
    var taskId = Guid.NewGuid();
    
    // Act
    var component = RenderComponent<InboxDashboardPage>(
        parameters => parameters.Add(p => p.InboxService, mockInboxService.Object)
    );
    component.Find(".approve-btn").Click();
    
    // Assert
    mockInboxService.Verify(
        x => x.ApproveTaskAsync(taskId, It.IsAny<Guid>()),
        Times.Once
    );
}
```

### Example 4: API Controller Test
```csharp
[Fact]
public async Task WorkflowController_GetWorkflows_Returns200AndList()
{
    // Arrange
    var mockService = new Mock<WorkflowEngineService>();
    var controller = new WorkflowController(mockService.Object);
    mockService.Setup(x => x.GetAllWorkflowDefinitionsAsync())
        .ReturnsAsync(new List<WorkflowDefinitionDto>
        {
            new WorkflowDefinitionDto { Id = Guid.NewGuid(), Name = "Test" }
        });
    
    // Act
    var result = await controller.GetWorkflows();
    
    // Assert
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    okResult.StatusCode.Should().Be(200);
}
```

---

## 🆘 Troubleshooting

### "Test Discovery Failed"
- **Solution**: Ensure test classes end with `Tests` suffix
- Verify: `dotnet test --list-tests`

### "xUnit not found"
- **Solution**: Restore NuGet packages
- Command: `dotnet restore tests/GrcMvc.Tests/`

### "Database connection error"
- **Solution**: Tests use in-memory DB - no external DB needed
- Check: TestDbContextFixture initialization

### "Moq setup error"
- **Solution**: Verify mock matches service interface
- Pattern: `mockService.Setup(x => x.MethodName(...))`

### "Assertion failed unexpectedly"
- **Solution**: Add debugging
- Command: `dotnet test --filter "TestName" -v detailed`

### "Tests timeout"
- **Solution**: Increase timeout in xunit.runner.json
- Or: Add `[Fact(Timeout = 5000)]` to individual tests

---

## 📞 Additional Resources

- **Comprehensive Testing Guide**: `COMPREHENSIVE_TESTING_GUIDE.md`
- **Test Plan**: `DETAILED_TEST_PLAN.md`
- **Execution Checklist**: `TEST_EXECUTION_CHECKLIST.md`
- **Test Results**: `TEST_RESULTS_SUMMARY.md`

---

## ⚡ Pro Tips

1. **Use watch mode during development**
   ```bash
   dotnet watch test
   ```

2. **Filter by test name pattern**
   ```bash
   dotnet test --filter "CreateWorkflow"
   ```

3. **Generate coverage visualization**
   ```bash
   dotnet test /p:CollectCoverage=true
   # View: coverage/index.html
   ```

4. **Run tests in parallel** (faster)
   ```bash
   dotnet test -p:ParallelizeAssembly=true
   ```

5. **Save test results for CI/CD**
   ```bash
   dotnet test --logger "trx;LogFileName=results.trx"
   ```

---

**Last Updated**: January 4, 2026  
**Total Test Methods**: 83  
**Framework**: xUnit 2.6.3  
**Status**: Ready for Execution
