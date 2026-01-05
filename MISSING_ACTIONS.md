# GRC System - Missing Actions Checklist

**Date**: January 4, 2026  
**Status**: In Progress  
**Total Actions**: 47 items

---

## 🎯 Phase 1: API Route Mapping (HIGH PRIORITY)

### API Routes Configuration
- [ ] **Map Workflow Routes** (Program.cs)
  - [ ] GET /api/workflow → list all workflows
  - [ ] POST /api/workflow → create workflow
  - [ ] GET /api/workflow/{id} → get workflow details
  - [ ] PUT /api/workflow/{id} → update workflow
  - [ ] DELETE /api/workflow/{id} → delete workflow
  - [ ] GET /api/workflow/{id}/history → get audit log
  - [ ] POST /api/workflow/{id}/start → create instance
  - [ ] GET /api/workflow/instances → list user instances
  - Estimated Time: 30 minutes

- [ ] **Map Approval Routes**
  - [ ] GET /api/approval-chain → list approval chains
  - [ ] POST /api/approval-chain → create chain
  - [ ] GET /api/approval-chain/{id} → get chain details
  - [ ] POST /api/approval/{id}/approve → approve request
  - [ ] POST /api/approval/{id}/reject → reject request
  - Estimated Time: 20 minutes

- [ ] **Map Inbox Routes**
  - [ ] GET /api/inbox → get user inbox
  - [ ] POST /api/inbox/task/{id}/approve → approve task
  - [ ] POST /api/inbox/task/{id}/reject → reject task
  - [ ] POST /api/inbox/task/{id}/escalate → escalate task
  - [ ] POST /api/inbox/task/{id}/comment → add comment
  - [ ] GET /api/inbox/task/{id}/comments → get comments
  - Estimated Time: 20 minutes

- [ ] **Enable CORS** (if needed for SPA)
  - [ ] Configure CORS policy in Program.cs
  - Estimated Time: 10 minutes

- [ ] **Test API Routes**
  - [ ] Verify all routes return 200 (not 404)
  - [ ] Verify auth redirects to 302 when needed
  - Estimated Time: 15 minutes

**Subtotal Phase 1**: ~95 minutes

---

## 🧪 Phase 2: Execute Existing Tests (HIGH PRIORITY)

### Run Test Suite
- [ ] **Unit Tests Execution**
  ```bash
  dotnet test tests/GrcMvc.Tests/ --filter "Services"
  ```
  - Expected: 31 tests pass
  - Time: 10 minutes

- [ ] **Integration Tests Execution**
  ```bash
  dotnet test tests/GrcMvc.Tests/ --filter "Integration"
  ```
  - Expected: 13 tests pass
  - Time: 30 minutes

- [ ] **Component Tests Execution**
  ```bash
  dotnet test tests/GrcMvc.Tests/ --filter "Components"
  ```
  - Expected: 15 tests pass
  - Time: 20 minutes

- [ ] **API Tests Execution**
  ```bash
  dotnet test tests/GrcMvc.Tests/ --filter "Controllers"
  ```
  - Expected: 19 tests pass
  - Time: 15 minutes

- [ ] **Generate Coverage Report**
  ```bash
  dotnet test tests/GrcMvc.Tests/ /p:CollectCoverage=true
  ```
  - Expected: ≥80% coverage
  - Time: 5 minutes

- [ ] **Review Test Results**
  - [ ] Check pass/fail counts
  - [ ] Review coverage gaps
  - [ ] Document results
  - Time: 15 minutes

**Subtotal Phase 2**: ~95 minutes

---

## 📝 Phase 3: Complete Missing Tests (MEDIUM PRIORITY)

### E2E Test Implementation
- [ ] **OnboardingFlowTests.cs** (5 tests)
  - [ ] Test: Register → Verify → Login → Onboarding complete
  - Estimated Time: 30 minutes

- [ ] **CompleteWorkflowTests.cs** (5 tests)
  - [ ] Test: Create → L1 Approve → L2 Approve → L3 Approve → Complete
  - Estimated Time: 30 minutes

- [ ] **RejectionFlowTests.cs** (5 tests)
  - [ ] Test: Submit → Reject → Revise → Resubmit → Approve → Complete
  - Estimated Time: 30 minutes

### Performance Tests
- [ ] **PerformanceTests.cs** (10 tests)
  - [ ] Test: Response time < 500ms for API endpoints
  - [ ] Test: Database query performance
  - [ ] Test: Memory usage under load
  - [ ] Test: Concurrent request handling
  - Estimated Time: 45 minutes

### Security Tests
- [ ] **SecurityTests.cs** (15 tests)
  - [ ] Test: SQL injection prevention
  - [ ] Test: Cross-site scripting (XSS) prevention
  - [ ] Test: Cross-site request forgery (CSRF) protection
  - [ ] Test: Authentication enforcement
  - [ ] Test: Authorization validation
  - [ ] Test: Password hashing
  - [ ] Test: Session timeout
  - Estimated Time: 60 minutes

**Subtotal Phase 3**: ~195 minutes

---

## 🎨 Phase 4: Blazor UI Pages Implementation (MEDIUM PRIORITY)

### Workflow Pages
- [ ] **Pages/Workflow/Index.razor**
  - [ ] Display list of workflows with search
  - [ ] Create button
  - [ ] Edit button
  - [ ] Delete button
  - Estimated Time: 45 minutes

- [ ] **Pages/Workflow/Create.razor**
  - [ ] Form for new workflow
  - [ ] Input validation
  - [ ] Submit button
  - Estimated Time: 45 minutes

- [ ] **Pages/Workflow/Edit.razor**
  - [ ] Edit existing workflow
  - [ ] Form validation
  - Estimated Time: 30 minutes

- [ ] **Pages/Workflow/Details.razor**
  - [ ] View workflow details
  - [ ] View approval steps
  - [ ] View history/audit log
  - Estimated Time: 45 minutes

### Approval Pages
- [ ] **Pages/Approval/MyApprovals.razor**
  - [ ] List pending approvals
  - [ ] Filter by status
  - [ ] Search
  - Estimated Time: 40 minutes

- [ ] **Pages/Approval/Details.razor**
  - [ ] View approval chain details
  - [ ] Approve/Reject buttons
  - [ ] Comments section
  - Estimated Time: 45 minutes

### Inbox Pages
- [ ] **Pages/Inbox/Dashboard.razor**
  - [ ] Display pending tasks
  - [ ] SLA indicator
  - [ ] Filter/search
  - Estimated Time: 45 minutes

- [ ] **Pages/Inbox/TaskDetail.razor**
  - [ ] View task details
  - [ ] Approve/Reject/Escalate
  - [ ] Comments
  - Estimated Time: 40 minutes

### Admin Pages
- [ ] **Pages/Admin/RoleManagement.razor**
  - [ ] List roles
  - [ ] Create role
  - [ ] Edit role
  - [ ] Delete role
  - Estimated Time: 60 minutes

- [ ] **Pages/Admin/UserManagement.razor**
  - [ ] List users
  - [ ] Create user
  - [ ] Assign roles
  - [ ] Reset password
  - Estimated Time: 60 minutes

**Subtotal Phase 4**: ~495 minutes

---

## 🔧 Phase 5: Reusable Components Implementation (MEDIUM PRIORITY)

### Core Components
- [ ] **Components/ProcessCard.razor**
  - [ ] Display approval process steps
  - [ ] Show current step
  - [ ] Show completed steps
  - Estimated Time: 30 minutes

- [ ] **Components/SLAIndicator.razor**
  - [ ] Show SLA status
  - [ ] Color coding (green/yellow/red)
  - [ ] Time remaining display
  - Estimated Time: 20 minutes

- [ ] **Components/ApprovalModal.razor**
  - [ ] Modal for approval decision
  - [ ] Comments field
  - [ ] Approve/Reject buttons
  - Estimated Time: 25 minutes

- [ ] **Components/TaskCard.razor**
  - [ ] Compact task display
  - [ ] Priority indicator
  - [ ] Due date
  - [ ] Quick actions
  - Estimated Time: 25 minutes

- [ ] **Components/StatusBadge.razor**
  - [ ] Color-coded status display
  - [ ] Different statuses (Pending, Approved, Rejected, etc.)
  - Estimated Time: 15 minutes

- [ ] **Components/PaginationControl.razor**
  - [ ] Page navigation
  - [ ] Items per page selector
  - [ ] Go to page input
  - Estimated Time: 20 minutes

- [ ] **Components/SearchFilter.razor**
  - [ ] Search input
  - [ ] Filter dropdowns
  - [ ] Filter button
  - Estimated Time: 20 minutes

- [ ] **Components/ConfirmDialog.razor**
  - [ ] Confirmation dialog
  - [ ] Yes/No buttons
  - [ ] Custom message
  - Estimated Time: 15 minutes

**Subtotal Phase 5**: ~170 minutes

---

## 🛠️ Phase 6: DTOs & API Response Models (LOW PRIORITY)

### Request DTOs
- [ ] **CreateWorkflowDto**
- [ ] **UpdateWorkflowDto**
- [ ] **CreateApprovalChainDto**
- [ ] **ApproveRequestDto**
- [ ] **RejectRequestDto**
- [ ] **ApproveTaskDto**
- [ ] **RejectTaskDto**
- [ ] **EscalateTaskDto**
- [ ] **CommentOnTaskDto**
- [ ] **CreateUserDto**
- [ ] **UpdateUserDto**
- Estimated Time: 30 minutes

### Response DTOs
- [ ] **WorkflowDefinitionDto**
- [ ] **WorkflowInstanceDto**
- [ ] **ApprovalChainDto**
- [ ] **TaskDto**
- [ ] **UserDto**
- [ ] **RoleDto**
- Estimated Time: 20 minutes

### Validation
- [ ] **DataAnnotations** (Required, MaxLength, etc.)
- [ ] **Custom Validators** (Email, Phone, etc.)
- Estimated Time: 15 minutes

**Subtotal Phase 6**: ~65 minutes

---

## 📊 Phase 7: Database & Migrations (LOW PRIORITY)

### Seed Data Enhancement
- [ ] **Add Sample Users** (5-10 test users with different roles)
- [ ] **Add Sample Workflows** (workflow instances for testing)
- [ ] **Add Sample Approvals** (approval chains in different states)
- Estimated Time: 30 minutes

### Database Optimization
- [ ] **Add Indexes** (improve query performance)
  - [ ] Tenant filtering
  - [ ] User lookups
  - [ ] Status filtering
- [ ] **Add Stored Procedures** (if needed for complex queries)
- Estimated Time: 30 minutes

**Subtotal Phase 7**: ~60 minutes

---

## 📱 Phase 8: Frontend Polish (LOW PRIORITY)

### Styling
- [ ] **Bootstrap/Tailwind Setup**
- [ ] **Custom CSS** for GRC-specific styling
- [ ] **Responsive Design** for mobile/tablet
- Estimated Time: 120 minutes

### User Experience
- [ ] **Loading States** (spinners, skeletons)
- [ ] **Error Messages** (clear, actionable)
- [ ] **Success Notifications** (toast messages)
- [ ] **Confirmation Dialogs** (for destructive actions)
- Estimated Time: 60 minutes

### Accessibility
- [ ] **ARIA Labels**
- [ ] **Keyboard Navigation**
- [ ] **Color Contrast** checks
- [ ] **Screen Reader** testing
- Estimated Time: 60 minutes

**Subtotal Phase 8**: ~240 minutes

---

## 🚀 Phase 9: Deployment Preparation (LOW PRIORITY)

### Docker
- [ ] **Update Dockerfile** (if using containers)
- [ ] **Docker Compose** for development/production
- Estimated Time: 30 minutes

### CI/CD
- [ ] **GitHub Actions** workflow for tests
- [ ] **Automated Deployment** (optional)
- Estimated Time: 45 minutes

### Documentation
- [ ] **User Guide** (how to use the system)
- [ ] **Admin Guide** (maintenance, configuration)
- [ ] **Developer Guide** (architecture, extending)
- Estimated Time: 120 minutes

### Environment
- [ ] **.env** configuration file
- [ ] **Secrets Management** (API keys, etc.)
- [ ] **Logging Setup** (structured logging)
- Estimated Time: 30 minutes

**Subtotal Phase 9**: ~225 minutes

---

## 🧹 Phase 10: Cleanup & Verification (LOW PRIORITY)

### Code Quality
- [ ] **Remove TODO comments**
- [ ] **Remove unused code**
- [ ] **Consistent naming** across codebase
- [ ] **Code formatting** (editorconfig)
- Estimated Time: 45 minutes

### Testing Verification
- [ ] **All tests passing** (100% pass rate)
- [ ] **Code coverage** ≥ 80%
- [ ] **No warnings** in build
- Estimated Time: 30 minutes

### Documentation Verification
- [ ] **API docs** (Swagger/OpenAPI)
- [ ] **Code comments** updated
- [ ] **README** accurate
- Estimated Time: 30 minutes

**Subtotal Phase 10**: ~105 minutes

---

## 📋 Summary by Priority

### HIGH PRIORITY (Complete ASAP)
1. **Phase 1: API Route Mapping** - 95 minutes
2. **Phase 2: Execute Tests** - 95 minutes
   - **Subtotal**: ~190 minutes (3.2 hours)

### MEDIUM PRIORITY (Complete This Week)
3. **Phase 3: Missing Tests** - 195 minutes
4. **Phase 4: Blazor UI Pages** - 495 minutes
5. **Phase 5: Components** - 170 minutes
   - **Subtotal**: ~860 minutes (14.3 hours)

### LOW PRIORITY (Complete Next Week)
6. **Phase 6: DTOs** - 65 minutes
7. **Phase 7: Database** - 60 minutes
8. **Phase 8: Frontend Polish** - 240 minutes
9. **Phase 9: Deployment** - 225 minutes
10. **Phase 10: Cleanup** - 105 minutes
    - **Subtotal**: ~695 minutes (11.6 hours)

---

## ⏱️ Total Time Estimate

```
HIGH PRIORITY:     3.2 hours    ████████
MEDIUM PRIORITY:  14.3 hours    ████████████████████████████████████
LOW PRIORITY:     11.6 hours    ████████████████████████████
─────────────────────────────────
TOTAL:            29.1 hours
```

---

## 🎯 Recommended Execution Order

**Week 1 (Days 1-2):**
- [ ] Complete Phase 1: API Routes (3.2 hours)
- [ ] Complete Phase 2: Execute Tests (included in Phase 1 time)

**Week 1 (Days 3-4):**
- [ ] Complete Phase 3: Missing Tests (3.2 hours)
- [ ] Complete Phase 4: Blazor UI Pages (8.2 hours)

**Week 1 (Day 5) + Week 2 (Days 1-2):**
- [ ] Complete Phase 5: Components (2.8 hours)

**Week 2 (Days 3-5):**
- [ ] Complete Phase 6-10: Polish & Deployment (18.8 hours)

---

## ✅ Definition of Done

Each action is "done" when:
- [ ] Code is written and compiles without errors
- [ ] Tests are written and pass
- [ ] Code is reviewed and approved
- [ ] Documentation is updated
- [ ] No warnings in build output

---

## 📞 Notes

- All estimated times are optimistic (actual time may be longer)
- Tests should be written as features are implemented
- API routes should be mapped before E2E tests
- UI pages should be created after API routes are confirmed working
- Keep test coverage ≥ 80% throughout development

---

**Created**: January 4, 2026  
**Status**: Planning Complete  
**Next Action**: Implement Phase 1 (API Routes)
