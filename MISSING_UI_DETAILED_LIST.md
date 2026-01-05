# Detailed List of Backend Methods Without UI

## Service-by-Service Breakdown

### 1. Subscription Service (27 methods without UI out of 32)

**Plans**
- GetPlanByCodeAsync() - Lookup by code

**Subscriptions - Status Management**
- CreateSubscriptionAsync() - Trial creation (implicit, API-only)
- UpdateSubscriptionStatusAsync() - Status changes
- ActivateTrialAsync() - Trial activation
- ActivateSubscriptionAsync() - Convert to active
- SuspendSubscriptionAsync() - Suspension
- CancelSubscriptionAsync() - Cancellation
- RenewSubscriptionAsync() - Renewal
- CheckSubscriptionStatusAsync() - Scheduled check

**Payments**
- GetPaymentByIdAsync() - Payment details lookup
- RecordPaymentAsync() - Manual payment recording
- RefundPaymentAsync() - Refund processing

**Invoices**
- GetInvoiceByIdAsync() - Invoice details
- GetInvoiceByNumberAsync() - Invoice lookup
- GetInvoicesBySubscriptionAsync() - Invoice list (API)
- CreateInvoiceAsync() - Manual invoice generation
- SendInvoiceAsync() - Manual send
- MarkInvoiceAsPaidAsync() - Mark paid

**Notifications**
- SendWelcomeEmailAsync() - Auto on activation
- SendPaymentConfirmationEmailAsync() - Auto on payment
- SendInvoiceEmailAsync() - Auto with invoice
- SendSubscriptionRenewalReminderAsync() - Scheduled
- SendSubscriptionExpiringReminderAsync() - Scheduled

**Access Control**
- IsUserLimitReachedAsync() - Validation check
- IsFeatureAvailableAsync() - Feature check

**Impact**: No way to manage subscriptions after creation
**Affects**: Admins, Tenant Managers

---

### 2. Rules Engine Service (5 methods without UI out of 5)

- EvaluateRulesAsync() - Triggered during onboarding only
- CreateRulesetAsync() - No rule creation UI
- AddRuleAsync() - No rule addition UI
- GetRulesForRulesetAsync() - Lookup only
- GetExecutionLogsAsync() - Audit only

**Impact**: Can't create or manage compliance rules
**Affects**: Compliance Officers, Admins
**Used for**: Determining what baselines/packages apply to tenant

---

### 3. Escalation Service (5 methods without UI out of 6)

- ProcessEscalationsAsync() - Scheduled background job
- EscalateApprovalAsync() - Manual escalation not available
- GetEscalationConfigAsync() - Configuration lookup only
- UpdateEscalationRulesAsync() - Can't update rules
- GetEscalationStatsAsync() - Stats not displayed

**Impact**: No manual escalation, can't configure SLA rules
**Affects**: Managers, Compliance Officers

---

### 4. Approval Workflow Service (2 methods without UI out of 8)

- SubmitForApprovalAsync() - No dedicated submit UI
- DelegateAsync() - No delegation UI

**Impact**: Limited approval workflow flexibility
**Affects**: Workflow users, Managers

---

### 5. Workflow Service (2 methods without UI out of 16)

- GetByEntityTypeAsync() - Filtering by entity type not exposed
- ValidateWorkflowAsync() - Validation is silent

**Impact**: Can't filter workflows by entity type via UI
**Affects**: Users looking for specific workflow types

---

### 6. Audit Service (1 method without UI out of 12)

- ValidateAuditScopeAsync() - Backend validation only

**Impact**: No UI feedback on audit scope validation
**Affects**: Minimal (validation is silent)

---

### 7. Policy Service (1 method without UI out of 13)

- ValidateComplianceAsync() - Backend validation only

**Impact**: No UI feedback on compliance status
**Affects**: Minimal (validation is silent)

---

### 8. Risk Service (1 method without UI out of 8)

- GetByCategoryAsync() - Filtering not exposed in UI

**Impact**: Can't filter risks by category via UI
**Affects**: Users needing category-based views

---

### 9. File Upload Service (3 methods without UI out of 4)

- ValidateFileAsync() - Transparent validation
- DeleteFileAsync() - No file deletion UI
- GetContentType() - Utility function

**Impact**: Can't delete files via UI, no file browser
**Affects**: Evidence managers

---

### 10. Audit Event Service (3 methods without UI out of 3) - ALL METHODS

- LogEventAsync() - Background logging
- GetEventsByTenantAsync() - No audit log viewer
- GetEventsByCorrelationIdAsync() - No correlation viewer

**Impact**: Audit trail exists but can't be reviewed
**Affects**: Compliance teams, Auditors

---

### 11. Email Service (3 methods without UI out of 3) - ALL METHODS

- SendEmailAsync() - No manual send UI
- SendEmailBatchAsync() - No bulk send UI
- SendTemplatedEmailAsync() - Not implemented

**Impact**: Email is automatic only, no templates
**Affects**: Communication managers

---

### 12. Workflow Engine Service (1 method without UI out of 10)

- CreateWorkflowAsync() - Internal, called by WorkflowService

**Impact**: Minimal (internal implementation)

---

### 13. Onboarding Service (partial analysis)

Most methods have UI, only potential gaps in:
- Rule evaluation (called internally)

---

### 14. Authentication Service (complete)

All methods have UI

---

### 15. Authorization Service

Not fully analyzed in this review

---

## Methods by Missing Feature Category

### Admin/Configuration Functions (No UI)
1. CreateRulesetAsync() - Rules
2. AddRuleAsync() - Rules
3. UpdateEscalationRulesAsync() - Escalations
4. GetEscalationConfigAsync() - Escalations
5. SendTemplatedEmailAsync() - Email templates

### Payment/Invoice Operations (No UI)
6. CreateInvoiceAsync()
7. RecordPaymentAsync()
8. RefundPaymentAsync()
9. MarkInvoiceAsPaidAsync()
10. SendInvoiceAsync()

### Subscription Lifecycle (No UI)
11. CreateSubscriptionAsync()
12. UpdateSubscriptionStatusAsync()
13. ActivateTrialAsync()
14. ActivateSubscriptionAsync()
15. SuspendSubscriptionAsync()
16. CancelSubscriptionAsync()
17. RenewSubscriptionAsync()
18. CheckSubscriptionStatusAsync()

### Workflow Control (No UI)
19. SubmitForApprovalAsync()
20. DelegateAsync()
21. EscalateApprovalAsync()
22. ProcessEscalationsAsync()

### Filtering/Lookup (No UI)
23. GetByCategoryAsync() - Risk
24. GetByEntityTypeAsync() - Workflow
25. GetPlanByCodeAsync() - Subscription
26. GetEscalationsAsync() - No stats display
27. GetInvoiceByIdAsync()
28. GetInvoiceByNumberAsync()
29. GetInvoicesBySubscriptionAsync()
30. GetPaymentByIdAsync()

### Notifications (No UI)
31. SendWelcomeEmailAsync()
32. SendPaymentConfirmationEmailAsync()
33. SendInvoiceEmailAsync()
34. SendSubscriptionRenewalReminderAsync()
35. SendSubscriptionExpiringReminderAsync()
36. SendEmailAsync()
37. SendEmailBatchAsync()

### Audit/Logging (No UI)
38. LogEventAsync()
39. GetEventsByTenantAsync()
40. GetEventsByCorrelationIdAsync()
41. GetExecutionLogsAsync() - Rules

### File Management (No UI)
42. DeleteFileAsync()
43. ValidateFileAsync()
44. GetContentType()

### Validation (Backend Only)
45. ValidateAuditScopeAsync()
46. ValidateComplianceAsync()
47. ValidateWorkflowAsync()
48. IsUserLimitReachedAsync()
49. IsFeatureAvailableAsync()

### Access Control (No UI)
50. GetEscalationStatsAsync()

---

## Implementation Complexity Assessment

### Quick Wins (1-2 hours each)
- [ ] Add Risk category filtering to Risk/Index view
- [ ] Display GetEscalationStatsAsync() on Dashboard
- [ ] Add PaymentByIdAsync() detail view in receipt page
- [ ] Add File deletion to Evidence management
- [ ] Add ValidateAuditScopeAsync() feedback to audit creation

### Medium Effort (4-8 hours each)
- [ ] Create Rules Engine Basic Admin Panel
  - Rule CRUD
  - Test rules
  - View logs
- [ ] Create Escalation Configuration UI
  - Rule management
  - SLA settings
  - Manual escalation buttons
- [ ] Create Audit Log Viewer
  - Timeline display
  - Filtering
  - Export

### Major Effort (2-3 days each)
- [ ] Create Subscription Admin Dashboard
  - Status overview
  - Invoice management
  - Payment history
  - Renewal workflow
- [ ] Create Rules Engine Visual Builder
  - Condition builder
  - Action builder
  - Rule testing
- [ ] Create Email Template Manager
  - Template editor
  - Send logs
  - Preferences

### Not Recommended (Architectural changes needed)
- Audit Event Logging viewer (would require redesign)
- Email service UI (would require workflow changes)

---

## Estimated Total Development Effort

- Quick Wins: 5 items × 2 hours = 10 hours
- Medium Effort: 3 items × 6 hours = 18 hours
- Major Effort: 3 items × 3 days = 36 hours (72 hours / 8-hour days)

**Total: ~100 hours = 2.5 weeks with single developer**

---

Generated: 2024-01-04
