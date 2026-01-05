# Missing Actions for Full Feature User Experience

## Overview
This document lists all missing actions, UI pages, API endpoints, and features needed to complete the GRC system user experience based on existing code analysis.

---

## 1. MENU & NAVIGATION (Arabic Menu Requirements)

### Missing Menu Items (Expected vs Current)

| Arabic Menu Item | Expected Route | Current Status | Missing Actions |
|-----------------|----------------|----------------|----------------|
| الصفحة الرئيسية | `/` | ✅ Exists | - |
| لوحة التحكم | `/dashboard` | ✅ Exists | - |
| الاشتراكات | `/subscriptions` | ⚠️ Partial | **Full subscription management UI** |
| الإدارة | `/admin` | ✅ Exists | **Tenants management page** |
| مكتبة الأطر التنظيمية | `/frameworks` | ❌ Missing | **Complete framework library UI** |
| الجهات التنظيمية | `/regulators` | ❌ Missing | **Regulator management UI** |
| التقييمات | `/assessments` | ✅ Exists | - |
| تقييمات الضوابط | `/control-assessments` | ❌ Missing | **Control assessment UI** |
| الأدلة | `/evidence` | ✅ Exists | **File deletion, bulk operations** |
| إدارة المخاطر | `/risks` | ✅ Exists | **Category filtering UI** |
| إدارة المراجعة | `/audits` | ✅ Exists | **Scope validation feedback** |
| خطط العمل | `/action-plans` | ❌ Missing | **Action plan management UI** |
| إدارة السياسات | `/policies` | ✅ Exists | **Compliance validation feedback** |
| تقويم الامتثال | `/compliance-calendar` | ❌ Missing | **Compliance calendar UI** |
| محرك سير العمل | `/workflow` | ✅ Exists | **Workflow filtering by entity type** |
| الإشعارات | `/notifications` | ❌ Missing | **Notification center UI** |
| إدارة الموردين | `/vendors` | ❌ Missing | **Vendor management UI** |
| التقارير والتحليلات | `/reports` | ✅ Exists | - |
| مركز التكامل | `/integrations` | ❌ Missing | **Integration center UI** |

---

## 2. SUBSCRIPTION MANAGEMENT (27 Missing Actions)

### Subscription Lifecycle Management
- [ ] **Create Subscription UI** (`/subscriptions/create`)
  - Trial creation form
  - Plan selection interface
  - Payment method setup
  
- [ ] **Subscription Status Management UI** (`/subscriptions/{id}/manage`)
  - Activate trial button
  - Activate subscription button
  - Suspend subscription button
  - Cancel subscription button
  - Renew subscription button
  - Status change history view

- [ ] **Subscription Dashboard** (`/subscriptions/dashboard`)
  - Current subscription status
  - Usage metrics (users, features)
  - Billing cycle information
  - Next renewal date
  - Subscription health indicators

### Payment Management
- [ ] **Payment History UI** (`/subscriptions/payments`)
  - List all payments
  - Payment details view
  - Receipt download
  - Payment status indicators

- [ ] **Manual Payment Recording** (`/subscriptions/payments/record`)
  - Record payment form
  - Payment method selection
  - Receipt upload

- [ ] **Refund Processing UI** (`/subscriptions/payments/{id}/refund`)
  - Refund request form
  - Refund status tracking
  - Refund history

### Invoice Management
- [ ] **Invoice List View** (`/subscriptions/invoices`)
  - All invoices for subscription
  - Invoice status (paid, pending, overdue)
  - Download PDF
  - Filter by date range

- [ ] **Invoice Detail View** (`/subscriptions/invoices/{id}`)
  - Full invoice details
  - Line items
  - Payment status
  - Download/print

- [ ] **Manual Invoice Generation** (`/subscriptions/invoices/create`)
  - Create invoice form
  - Add line items
  - Set due date
  - Send invoice button

- [ ] **Invoice Actions**
  - Mark as paid button
  - Send invoice email button
  - Resend invoice

### Subscription Notifications
- [ ] **Email Notification Settings** (`/subscriptions/notifications`)
  - Configure welcome emails
  - Payment confirmation emails
  - Renewal reminders
  - Expiration warnings

---

## 3. FRAMEWORK LIBRARY (مكتبة الأطر التنظيمية)

### Missing UI Pages
- [ ] **Framework Library Home** (`/frameworks`)
  - Browse all 163+ frameworks
  - Search and filter frameworks
  - Framework categories
  - Framework statistics

- [ ] **Framework Detail View** (`/frameworks/{code}`)
  - Framework information
  - Version history
  - Related controls
  - Applicable sectors
  - Compliance requirements

- [ ] **Framework Management** (`/frameworks/manage`) [Admin]
  - Create framework
  - Edit framework
  - Publish version
  - Retire framework
  - Import frameworks from CSV

- [ ] **Framework Import** (`/frameworks/import`)
  - CSV upload interface
  - Import preview
  - Validation results
  - Import history

### Missing Actions
- [ ] Framework search and filtering
- [ ] Framework comparison view
- [ ] Framework-to-control mapping visualization
- [ ] Framework compliance tracking
- [ ] Framework version comparison

---

## 4. REGULATOR MANAGEMENT (الجهات التنظيمية)

### Missing UI Pages
- [ ] **Regulator List** (`/regulators`)
  - Browse all 92 regulators
  - Filter by country/region
  - Filter by sector
  - Regulator statistics

- [ ] **Regulator Detail View** (`/regulators/{code}`)
  - Regulator information
  - Issued frameworks list
  - Compliance requirements
  - Contact information

- [ ] **Regulator Management** (`/regulators/manage`) [Admin]
  - Create regulator
  - Edit regulator
  - Activate/deactivate regulator
  - Import regulators from CSV

- [ ] **Regulator Import** (`/regulators/import`)
  - CSV upload interface
  - Import preview
  - Validation results

### Missing Actions
- [ ] Regulator search and filtering
- [ ] Regulator-to-framework relationship view
- [ ] Regulator compliance calendar
- [ ] Regulator notification subscriptions

---

## 5. CONTROL ASSESSMENTS (تقييمات الضوابط)

### Missing UI Pages
- [ ] **Control Assessment List** (`/control-assessments`)
  - List all control assessments
  - Filter by control, framework, status
  - Assessment statistics

- [ ] **Create Control Assessment** (`/control-assessments/create`)
  - Assessment form
  - Control selection
  - Assessment criteria
  - Evidence requirements

- [ ] **Control Assessment Detail** (`/control-assessments/{id}`)
  - Assessment details
  - Test results
  - Evidence links
  - Remediation actions
  - Assessment history

- [ ] **Control Assessment Edit** (`/control-assessments/{id}/edit`)
  - Update assessment
  - Record test results
  - Link evidence
  - Update status

### Missing Actions
- [ ] Assessment workflow (draft → in-progress → completed)
- [ ] Assessment approval process
- [ ] Bulk assessment creation
- [ ] Assessment reporting
- [ ] Assessment scheduling

---

## 6. ACTION PLANS (خطط العمل)

### Missing UI Pages
- [ ] **Action Plan List** (`/action-plans`)
  - List all action plans
  - Filter by status, priority, owner
  - Action plan statistics

- [ ] **Create Action Plan** (`/action-plans/create`)
  - Action plan form
  - Link to risk/audit finding
  - Assign owner
  - Set due date
  - Define milestones

- [ ] **Action Plan Detail** (`/action-plans/{id}`)
  - Plan details
  - Milestones tracking
  - Progress indicators
  - Related risks/audits
  - Completion history

- [ ] **Action Plan Edit** (`/action-plans/{id}/edit`)
  - Update plan
  - Update milestones
  - Change status
  - Reassign owner

### Missing Actions
- [ ] Action plan assignment workflow
- [ ] Action plan closure process
- [ ] Action plan escalation
- [ ] Action plan reporting
- [ ] Action plan dashboard widgets

---

## 7. COMPLIANCE CALENDAR (تقويم الامتثال)

### Missing UI Pages
- [ ] **Compliance Calendar View** (`/compliance-calendar`)
  - Calendar interface (monthly/weekly/daily)
  - Compliance events
  - Due dates
  - Deadlines
  - Recurring events

- [ ] **Create Compliance Event** (`/compliance-calendar/events/create`)
  - Event form
  - Event type selection
  - Framework/regulator link
  - Due date
  - Recurrence settings

- [ ] **Compliance Event Detail** (`/compliance-calendar/events/{id}`)
  - Event details
  - Related frameworks
  - Completion status
  - Evidence requirements
  - Reminders

- [ ] **Compliance Calendar Management** (`/compliance-calendar/manage`)
  - Edit events
  - Delete events
  - Bulk operations
  - Import events

### Missing Actions
- [ ] Calendar view (monthly, weekly, daily)
- [ ] Event reminders and notifications
- [ ] Compliance event completion tracking
- [ ] Calendar export (iCal, CSV)
- [ ] Compliance dashboard integration

---

## 8. VENDOR MANAGEMENT (إدارة الموردين)

### Missing UI Pages
- [ ] **Vendor List** (`/vendors`)
  - List all vendors
  - Vendor search and filter
  - Vendor statistics
  - Vendor categories

- [ ] **Create Vendor** (`/vendors/create`)
  - Vendor registration form
  - Contact information
  - Vendor classification
  - Risk assessment

- [ ] **Vendor Detail View** (`/vendors/{id}`)
  - Vendor information
  - Vendor assessments
  - Contract information
  - Compliance status
  - Risk profile

- [ ] **Vendor Assessment** (`/vendors/{id}/assess`)
  - Assessment form
  - Risk evaluation
  - Compliance check
  - Evidence collection
  - Assessment history

- [ ] **Vendor Edit** (`/vendors/{id}/edit`)
  - Update vendor information
  - Update risk profile
  - Update compliance status

### Missing Actions
- [ ] Vendor risk assessment workflow
- [ ] Vendor compliance tracking
- [ ] Vendor contract management
- [ ] Vendor performance monitoring
- [ ] Vendor reporting

---

## 9. INTEGRATION CENTER (مركز التكامل)

### Missing UI Pages
- [ ] **Integration List** (`/integrations`)
  - List all integrations
  - Integration status
  - Integration types
  - Connection health

- [ ] **Create Integration** (`/integrations/create`)
  - Integration type selection
  - Configuration form
  - Authentication setup
  - Connection testing

- [ ] **Integration Detail** (`/integrations/{id}`)
  - Integration details
  - Connection status
  - Sync history
  - Error logs
  - Configuration

- [ ] **Integration Management** (`/integrations/{id}/manage`)
  - Edit configuration
  - Test connection
  - Enable/disable integration
  - View logs
  - Sync now button

### Missing Actions
- [ ] Integration connection testing
- [ ] Integration sync scheduling
- [ ] Integration error monitoring
- [ ] Integration log viewer
- [ ] Integration health dashboard

---

## 10. NOTIFICATION CENTER (الإشعارات)

### Missing UI Pages
- [ ] **Notification Inbox** (`/notifications`)
  - List all notifications
  - Unread notifications
  - Notification categories
  - Mark as read/unread

- [ ] **Notification Settings** (`/notifications/settings`)
  - Email preferences
  - In-app preferences
  - Notification types
  - Quiet hours

- [ ] **Notification Detail** (`/notifications/{id}`)
  - Notification details
  - Related entity link
  - Action buttons
  - Mark as read

### Missing Actions
- [ ] Real-time notification updates
- [ ] Notification filtering
- [ ] Bulk mark as read
- [ ] Notification preferences management
- [ ] Notification history

---

## 11. ADMINISTRATION (الإدارة)

### Missing Pages
- [ ] **Tenant Management** (`/admin/tenants`)
  - List all tenants
  - Create tenant
  - Edit tenant
  - Tenant subscription status
  - Tenant statistics

- [ ] **Permission Management** (`/admin/permissions`)
  - List all permissions
  - Permission groups
  - Permission assignment
  - Permission testing

### Missing Actions
- [ ] Tenant CRUD operations
- [ ] Tenant subscription linking
- [ ] Tenant user management
- [ ] Permission matrix view
- [ ] Role-permission assignment UI

---

## 12. EVIDENCE MANAGEMENT (الأدلة)

### Missing Actions
- [ ] **File Deletion UI**
  - Delete button on evidence detail
  - Bulk delete functionality
  - Delete confirmation dialog
  - Soft delete with restore option

- [ ] **Bulk Evidence Operations**
  - Bulk upload
  - Bulk status update
  - Bulk assignment
  - Bulk export

- [ ] **Evidence File Browser**
  - File tree view
  - File search
  - File preview
  - File metadata

---

## 13. WORKFLOW MANAGEMENT

### Missing Actions
- [ ] **Workflow Filtering by Entity Type**
  - Filter dropdown in workflow list
  - Entity type badges
  - Entity type statistics

- [ ] **Workflow Submission UI**
  - Submit for approval button
  - Submission form
  - Approval chain preview

- [ ] **Workflow Delegation UI**
  - Delegate button
  - Delegate to user selector
  - Delegation history

- [ ] **Workflow Escalation UI**
  - Manual escalation button
  - Escalation reason
  - Escalation history

---

## 14. RISK MANAGEMENT

### Missing Actions
- [ ] **Risk Category Filtering UI**
  - Category filter dropdown
  - Category badges
  - Category statistics
  - Category-based views

---

## 15. AUDIT MANAGEMENT

### Missing Actions
- [ ] **Audit Scope Validation Feedback**
  - Validation results display
  - Validation warnings
  - Scope recommendations
  - Scope completeness indicator

---

## 16. POLICY MANAGEMENT

### Missing Actions
- [ ] **Compliance Validation Feedback**
  - Policy compliance status
  - Compliance violations list
  - Remediation suggestions
  - Compliance dashboard widget

---

## 17. RULES ENGINE ADMINISTRATION

### Missing UI Pages
- [ ] **Rules Engine Dashboard** (`/admin/rules`)
  - List all rulesets
  - Rule execution logs
  - Rule statistics
  - Rule performance

- [ ] **Create Ruleset** (`/admin/rules/rulesets/create`)
  - Ruleset creation form
  - Rule builder interface
  - Condition builder
  - Action builder

- [ ] **Rule Management** (`/admin/rules/rulesets/{id}`)
  - Add rule
  - Edit rule
  - Delete rule
  - Test rule
  - Rule execution history

- [ ] **Rule Testing** (`/admin/rules/test`)
  - Test rule interface
  - Test data input
  - Test results
  - Rule validation

### Missing Actions
- [ ] Visual rule builder
- [ ] Rule testing interface
- [ ] Rule execution log viewer
- [ ] Rule performance analytics

---

## 18. ESCALATION MANAGEMENT

### Missing UI Pages
- [ ] **Escalation Configuration** (`/admin/escalations`)
  - Escalation rules management
  - SLA settings
  - Escalation thresholds
  - Escalation actions

- [ ] **Escalation Dashboard** (`/escalations`)
  - Active escalations
  - Escalation statistics
  - Escalation trends
  - SLA compliance

### Missing Actions
- [ ] Manual escalation button
- [ ] Escalation rule editor
- [ ] Escalation statistics display
- [ ] Escalation history viewer

---

## 19. AUDIT EVENT LOGGING

### Missing UI Pages
- [ ] **Audit Log Viewer** (`/admin/audit-logs`)
  - Event timeline
  - Event filtering
  - Event search
  - Event export
  - Correlation ID tracking

- [ ] **Audit Log Detail** (`/admin/audit-logs/{id}`)
  - Event details
  - Related events
  - Event metadata
  - Event context

### Missing Actions
- [ ] Audit log search and filtering
- [ ] Audit log export (CSV, JSON)
- [ ] Audit log correlation viewer
- [ ] Audit log analytics

---

## 20. EMAIL TEMPLATE MANAGEMENT

### Missing UI Pages
- [ ] **Email Template List** (`/admin/email-templates`)
  - List all templates
  - Template categories
  - Template usage statistics

- [ ] **Email Template Editor** (`/admin/email-templates/{id}/edit`)
  - Template editor
  - Variable insertion
  - Template preview
  - Template testing

- [ ] **Email Send Logs** (`/admin/email-templates/logs`)
  - Sent email history
  - Email status
  - Email delivery tracking
  - Email failure logs

### Missing Actions
- [ ] Template editor with variables
- [ ] Template preview
- [ ] Template testing
- [ ] Email send logs viewer
- [ ] Email delivery tracking

---

## 21. DASHBOARD ENHANCEMENTS

### Missing Dashboard Widgets
- [ ] **Escalation Statistics Widget**
  - Active escalations count
  - SLA compliance percentage
  - Escalation trends chart

- [ ] **Subscription Health Widget**
  - Subscription status
  - Usage metrics
  - Renewal countdown

- [ ] **Framework Compliance Widget**
  - Framework coverage
  - Compliance percentage
  - Framework status

- [ ] **Vendor Risk Widget**
  - High-risk vendors
  - Vendor assessment status
  - Vendor compliance score

---

## 22. API ENDPOINTS MISSING

### Subscription APIs
- [ ] `GET /api/subscriptions/{id}/status` - Detailed status
- [ ] `POST /api/subscriptions/{id}/activate-trial` - Activate trial
- [ ] `POST /api/subscriptions/{id}/suspend` - Suspend subscription
- [ ] `POST /api/subscriptions/{id}/cancel` - Cancel subscription
- [ ] `POST /api/subscriptions/{id}/renew` - Renew subscription
- [ ] `GET /api/subscriptions/{id}/usage` - Usage metrics

### Framework APIs
- [ ] `GET /api/frameworks` - List frameworks (with UI)
- [ ] `GET /api/frameworks/{code}` - Framework details
- [ ] `POST /api/frameworks` - Create framework
- [ ] `PUT /api/frameworks/{id}` - Update framework
- [ ] `POST /api/frameworks/import` - Import frameworks

### Regulator APIs
- [ ] `GET /api/regulators` - List regulators (with UI)
- [ ] `GET /api/regulators/{code}` - Regulator details
- [ ] `POST /api/regulators` - Create regulator
- [ ] `PUT /api/regulators/{id}` - Update regulator
- [ ] `POST /api/regulators/import` - Import regulators

### Control Assessment APIs
- [ ] `GET /api/control-assessments` - List assessments
- [ ] `GET /api/control-assessments/{id}` - Assessment details
- [ ] `POST /api/control-assessments` - Create assessment
- [ ] `PUT /api/control-assessments/{id}` - Update assessment
- [ ] `POST /api/control-assessments/{id}/approve` - Approve assessment

### Action Plan APIs
- [ ] `GET /api/action-plans` - List action plans
- [ ] `GET /api/action-plans/{id}` - Action plan details
- [ ] `POST /api/action-plans` - Create action plan
- [ ] `PUT /api/action-plans/{id}` - Update action plan
- [ ] `POST /api/action-plans/{id}/close` - Close action plan

### Compliance Calendar APIs
- [ ] `GET /api/compliance-calendar` - Get calendar events
- [ ] `GET /api/compliance-calendar/events/{id}` - Event details
- [ ] `POST /api/compliance-calendar/events` - Create event
- [ ] `PUT /api/compliance-calendar/events/{id}` - Update event
- [ ] `DELETE /api/compliance-calendar/events/{id}` - Delete event

### Vendor APIs
- [ ] `GET /api/vendors` - List vendors
- [ ] `GET /api/vendors/{id}` - Vendor details
- [ ] `POST /api/vendors` - Create vendor
- [ ] `PUT /api/vendors/{id}` - Update vendor
- [ ] `POST /api/vendors/{id}/assess` - Assess vendor

### Integration APIs
- [ ] `GET /api/integrations` - List integrations
- [ ] `GET /api/integrations/{id}` - Integration details
- [ ] `POST /api/integrations` - Create integration
- [ ] `PUT /api/integrations/{id}` - Update integration
- [ ] `POST /api/integrations/{id}/test` - Test connection
- [ ] `POST /api/integrations/{id}/sync` - Trigger sync

### Notification APIs
- [ ] `GET /api/notifications` - List notifications
- [ ] `GET /api/notifications/{id}` - Notification details
- [ ] `PUT /api/notifications/{id}/read` - Mark as read
- [ ] `PUT /api/notifications/read-all` - Mark all as read
- [ ] `GET /api/notifications/settings` - Get settings
- [ ] `PUT /api/notifications/settings` - Update settings

---

## 23. PERMISSIONS & AUTHORIZATION

### Missing Permission Definitions
- [ ] `Grc.Frameworks.*` - Framework permissions
- [ ] `Grc.Regulators.*` - Regulator permissions
- [ ] `Grc.ControlAssessments.*` - Control assessment permissions
- [ ] `Grc.ActionPlans.*` - Action plan permissions
- [ ] `Grc.ComplianceCalendar.*` - Compliance calendar permissions
- [ ] `Grc.Vendors.*` - Vendor permissions
- [ ] `Grc.Integrations.*` - Integration permissions
- [ ] `Grc.Notifications.*` - Notification permissions

### Missing Permission Enforcement
- [ ] Permission checks on all new UI pages
- [ ] Permission-based menu visibility
- [ ] Permission-based button visibility
- [ ] Permission-based API authorization

---

## 24. POLICY ENFORCEMENT

### Missing Policy Engine Implementation
- [ ] Policy engine infrastructure
- [ ] Policy context creation
- [ ] Policy enforcer service
- [ ] Policy store (YAML loader)
- [ ] Policy violation exceptions
- [ ] Policy audit logging

### Missing Policy Enforcement Points
- [ ] Evidence create/update policy checks
- [ ] Assessment submit/approve policy checks
- [ ] Policy document publish policy checks
- [ ] Risk accept policy checks
- [ ] Audit close policy checks

---

## 25. USER EXPERIENCE ENHANCEMENTS

### Missing UX Features
- [ ] **Search Functionality**
  - Global search bar
  - Search across all entities
  - Search filters
  - Search history

- [ ] **Bulk Operations**
  - Bulk select
  - Bulk actions menu
  - Bulk status update
  - Bulk export

- [ ] **Export Functionality**
  - Export to Excel
  - Export to PDF
  - Export to CSV
  - Custom export templates

- [ ] **Advanced Filtering**
  - Multi-criteria filters
  - Saved filters
  - Filter presets
  - Filter sharing

- [ ] **Data Visualization**
  - Charts and graphs
  - Trend analysis
  - Comparative views
  - Interactive dashboards

- [ ] **Mobile Responsiveness**
  - Mobile-optimized layouts
  - Touch-friendly controls
  - Mobile navigation
  - Mobile-specific features

---

## SUMMARY STATISTICS

### Missing UI Pages: **~45 pages**
### Missing API Endpoints: **~60 endpoints**
### Missing Actions/Features: **~150+ actions**
### Missing Components: **~30 components**

### Priority Breakdown

**HIGH PRIORITY (Core Features)**
- Subscription management UI
- Framework library UI
- Regulator management UI
- Control assessments UI
- Action plans UI
- Compliance calendar UI
- Vendor management UI
- Integration center UI
- Notification center UI

**MEDIUM PRIORITY (Enhancements)**
- Rules engine admin UI
- Escalation management UI
- Audit log viewer
- Email template management
- Advanced filtering and search
- Bulk operations

**LOW PRIORITY (Nice to Have)**
- Mobile optimizations
- Advanced visualizations
- Custom export templates
- Saved filters and presets

---

## ESTIMATED EFFORT

| Category | Pages | APIs | Components | Estimated Hours |
|----------|-------|------|------------|----------------|
| Core Features | 25 | 40 | 15 | 120-160 hours |
| Enhancements | 15 | 15 | 10 | 60-80 hours |
| Polish & UX | 5 | 5 | 5 | 20-30 hours |
| **TOTAL** | **45** | **60** | **30** | **200-270 hours** |

**Estimated Timeline**: 5-7 weeks (single developer, full-time)

---

*Generated: 2025-01-22*
*Based on: Existing codebase analysis and Arabic menu requirements*
