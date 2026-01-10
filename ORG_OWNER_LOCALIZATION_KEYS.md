# Organization & Owner Management Localization Keys
**Batch 4: 20 View Files**

Generated: 2026-01-10
Status: Keys extracted, .resx update pending

---

## OrgSetup Views (7 files)

### CreateTeam.cshtml
```
OrgSetup.CreateTeam.Title.Edit = Edit Team
OrgSetup.CreateTeam.Title.Create = Create Team
OrgSetup.CreateTeam.Header.Edit = Edit Team
OrgSetup.CreateTeam.Header.Create = Create New Team
OrgSetup.CreateTeam.Label.TeamCode = Team Code
OrgSetup.CreateTeam.Placeholder.TeamCode = e.g., SEC-OPS
OrgSetup.CreateTeam.Label.TeamType = Team Type
OrgSetup.CreateTeam.Option.Operational = Operational
OrgSetup.CreateTeam.Option.Governance = Governance
OrgSetup.CreateTeam.Option.Project = Project
OrgSetup.CreateTeam.Option.External = External
OrgSetup.CreateTeam.Label.NameEnglish = Team Name (English)
OrgSetup.CreateTeam.Placeholder.NameEnglish = e.g., Security Operations
OrgSetup.CreateTeam.Label.NameArabic = Team Name (Arabic)
OrgSetup.CreateTeam.Placeholder.NameArabic = e.g., عمليات الأمن
OrgSetup.CreateTeam.Label.Purpose = Purpose
OrgSetup.CreateTeam.Placeholder.Purpose = Brief description of team's purpose
OrgSetup.CreateTeam.Label.Description = Description
OrgSetup.CreateTeam.Placeholder.Description = Detailed description...
OrgSetup.CreateTeam.Label.BusinessUnit = Business Unit
OrgSetup.CreateTeam.Placeholder.BusinessUnit = e.g., IT, Finance, Compliance
OrgSetup.CreateTeam.Label.IsDefaultFallback = Default Fallback Team
OrgSetup.CreateTeam.Help.IsDefaultFallback = This team will receive unassigned tasks
OrgSetup.CreateTeam.Button.Cancel = Cancel
OrgSetup.CreateTeam.Button.Update = Update Team
OrgSetup.CreateTeam.Button.Create = Create Team
```

### Index.cshtml (OrgSetup Dashboard)
```
OrgSetup.Index.Title = Organization Setup
OrgSetup.Index.Subtitle = Configure your GRC platform
OrgSetup.Index.Button.AutoProvision = Auto-Provision Defaults
OrgSetup.Index.Section.SetupProgress = Setup Progress
OrgSetup.Index.Progress.OrganizationProfile = Organization Profile
OrgSetup.Index.Progress.TeamsConfigured = Teams Configured
OrgSetup.Index.Progress.RACIConfigured = RACI Assignments
OrgSetup.Index.Progress.UsersActivated = Users Activated
OrgSetup.Index.Stats.Teams = Teams
OrgSetup.Index.Stats.TotalMembers = total members
OrgSetup.Index.Stats.ActiveUsers = Active Users
OrgSetup.Index.Stats.TotalUsers = total users
OrgSetup.Index.Stats.Sector = Sector
OrgSetup.Index.Stats.Onboarding = Onboarding
OrgSetup.Index.Card.Teams = Teams
OrgSetup.Index.Card.Teams.Description = Manage your GRC teams and assign members to handle workflows.
OrgSetup.Index.Card.Teams.Button = Manage Teams
OrgSetup.Index.Card.Users = Users
OrgSetup.Index.Card.Users.Description = View and manage users in your organization.
OrgSetup.Index.Card.Users.Button = Manage Users
OrgSetup.Index.Card.RACI = RACI Matrix
OrgSetup.Index.Card.RACI.Description = Configure responsibility assignments for control families.
OrgSetup.Index.Card.RACI.Button = Manage RACI
```

### RACI.cshtml
```
OrgSetup.RACI.Title = RACI Matrix
OrgSetup.RACI.Subtitle = Responsibility assignments for control families and workflows
OrgSetup.RACI.Button.Back = Back
OrgSetup.RACI.Legend = RACI Legend:
OrgSetup.RACI.Badge.Responsible = R = Responsible
OrgSetup.RACI.Badge.Accountable = A = Accountable
OrgSetup.RACI.Badge.Consulted = C = Consulted
OrgSetup.RACI.Badge.Informed = I = Informed
OrgSetup.RACI.NoAssignments = No RACI assignments configured yet.
OrgSetup.RACI.AutoProvisionLink = Auto-provision default RACI assignments
OrgSetup.RACI.Header.Assignments = Assignments
OrgSetup.RACI.Table.ScopeId = Scope ID
OrgSetup.RACI.Table.Team = Team
OrgSetup.RACI.Table.RACI = RACI
OrgSetup.RACI.Table.Role = Role
OrgSetup.RACI.Table.Priority = Priority
OrgSetup.RACI.Table.Status = Status
OrgSetup.RACI.Status.Active = Active
OrgSetup.RACI.Status.Inactive = Inactive
```

### TeamMembers.cshtml
```
OrgSetup.TeamMembers.Title = Members
OrgSetup.TeamMembers.Subtitle = Manage team members and their roles
OrgSetup.TeamMembers.Button.BackToTeams = Back to Teams
OrgSetup.TeamMembers.Section.AddMember = Add Member
OrgSetup.TeamMembers.Label.SelectUser = Select User
OrgSetup.TeamMembers.Option.SelectUser = -- Select User --
OrgSetup.TeamMembers.Label.Role = Role
OrgSetup.TeamMembers.Option.ControlOwner = Control Owner
OrgSetup.TeamMembers.Option.EvidenceCustodian = Evidence Custodian
OrgSetup.TeamMembers.Option.Approver = Approver
OrgSetup.TeamMembers.Option.Assessor = Assessor/Tester
OrgSetup.TeamMembers.Option.RemediationOwner = Remediation Owner
OrgSetup.TeamMembers.Option.Viewer = Viewer/Auditor
OrgSetup.TeamMembers.Label.IsPrimaryForRole = Primary for this role
OrgSetup.TeamMembers.Label.CanApprove = Can approve on behalf of team
OrgSetup.TeamMembers.Label.CanDelegate = Can delegate tasks
OrgSetup.TeamMembers.Button.AddMember = Add Member
OrgSetup.TeamMembers.Section.CurrentMembers = Current Members
OrgSetup.TeamMembers.NoMembers = No members in this team yet.
OrgSetup.TeamMembers.Table.User = User
OrgSetup.TeamMembers.Table.Role = Role
OrgSetup.TeamMembers.Table.Permissions = Permissions
OrgSetup.TeamMembers.Table.Joined = Joined
OrgSetup.TeamMembers.Badge.Primary = Primary
OrgSetup.TeamMembers.Badge.Approve = Approve
OrgSetup.TeamMembers.Badge.Delegate = Delegate
OrgSetup.TeamMembers.Confirm.Remove = Remove this member?
```

### Teams.cshtml
```
OrgSetup.Teams.Title = Teams Management
OrgSetup.Teams.Subtitle = Configure teams for workflow routing and task assignment
OrgSetup.Teams.Button.Back = Back
OrgSetup.Teams.Button.CreateTeam = Create Team
OrgSetup.Teams.NoTeams = No teams configured yet.
OrgSetup.Teams.NoTeams.Link.Create = Create your first team
OrgSetup.Teams.NoTeams.Link.AutoProvision = auto-provision default teams
OrgSetup.Teams.Badge.Default = Default
OrgSetup.Teams.Label.Members = members
OrgSetup.Teams.Button.Members = Members
OrgSetup.Teams.Button.Edit = Edit
```

### Users.cshtml
```
OrgSetup.Users.Title = Users Management
OrgSetup.Users.Subtitle = View and manage organization users
OrgSetup.Users.Button.Back = Back
OrgSetup.Users.NoUsers = No users found. Users are created during onboarding or invited by admins.
OrgSetup.Users.Table.User = User
OrgSetup.Users.Table.Role = Role
OrgSetup.Users.Table.Title = Title
OrgSetup.Users.Table.Department = Department
OrgSetup.Users.Table.Status = Status
OrgSetup.Users.Status.Active = Active
OrgSetup.Users.Status.Pending = Pending
```

---

## Owner Views (7 files)

### Create.cshtml
```
Owner.Create.Title = Create Tenant
Owner.Create.Header = Create New Tenant with Full Features
Owner.Create.Subtitle = Create a tenant that bypasses payment and gets Enterprise tier
Owner.Create.Label.OrganizationName = Organization Name
Owner.Create.Placeholder.OrganizationName = Acme Corporation
Owner.Create.Label.AdminEmail = Admin Email
Owner.Create.Placeholder.AdminEmail = admin@acme.com
Owner.Create.Help.AdminEmail = Email address for the tenant admin
Owner.Create.Label.TenantSlug = Tenant Slug
Owner.Create.Placeholder.TenantSlug = acme-corp
Owner.Create.Help.TenantSlug = Unique identifier (lowercase, numbers, hyphens only)
Owner.Create.Label.ExpirationDays = Expiration Days
Owner.Create.Help.ExpirationDays = Number of days until admin credentials expire (7-90 days)
Owner.Create.Info.Note = Note:
Owner.Create.Info.Description = This tenant will be created with:
Owner.Create.Info.EnterpriseTier = Enterprise subscription tier
Owner.Create.Info.PaymentBypass = Payment bypass enabled
Owner.Create.Info.AutoActivated = Auto-activated status
Owner.Create.Info.FullAccess = Full feature access
Owner.Create.Button.Cancel = Cancel
Owner.Create.Button.CreateTenant = Create Tenant
```

### Credentials.cshtml
```
Owner.Credentials.Title = Admin Credentials
Owner.Credentials.Header = Admin Credentials - Show Once
Owner.Credentials.Warning.Header = Warning:
Owner.Credentials.Warning.NotShownAgain = These credentials will not be shown again. Save them securely.
Owner.Credentials.Security.Warning = SECURITY WARNING:
Owner.Credentials.Security.DisplayOnce = This page will only be displayed once. Copy these credentials immediately.
Owner.Credentials.Section.LoginInfo = Login Information
Owner.Credentials.Label.Organization = Organization:
Owner.Credentials.Label.TenantId = Tenant ID:
Owner.Credentials.Label.Username = Username:
Owner.Credentials.Label.Password = Password:
Owner.Credentials.Button.Copy = Copy
Owner.Credentials.Label.LoginUrl = Login URL:
Owner.Credentials.Button.Open = Open
Owner.Credentials.Label.CredentialsExpire = Credentials Expire:
Owner.Credentials.Section.DeliveryOptions = Delivery Options:
Owner.Credentials.Button.CopyAll = Copy All to Clipboard
Owner.Credentials.Button.BackToDetails = Back to Tenant Details
Owner.Credentials.Section.SecurityInstructions = Security Instructions:
Owner.Credentials.Security.TemporaryCredentials = These credentials are temporary and will expire on {0}
Owner.Credentials.Security.MustChangePassword = You must change your password on first login
Owner.Credentials.Security.NoSharing = Do not share these credentials with anyone
Owner.Credentials.Security.SecureConnection = Use a secure connection (HTTPS) when logging in
Owner.Credentials.Security.DeliverSecurely = Deliver credentials securely to the tenant admin
Owner.Credentials.Alert.CopiedToClipboard = Copied to clipboard!
Owner.Credentials.Clipboard.Header = GRC Platform Admin Credentials
```

### Details.cshtml
```
Owner.Details.Title = Tenant Details
Owner.Details.Breadcrumb.OwnerDashboard = Owner Dashboard
Owner.Details.Breadcrumb.AllTenants = All Tenants
Owner.Details.Breadcrumb.Details = Details
Owner.Details.Warning.CredentialsExpired = Warning:
Owner.Details.Warning.CredentialsExpiredMessage = Admin credentials have expired. Please generate new credentials.
Owner.Details.Section.TenantInfo = Tenant Information
Owner.Details.Label.TenantId = Tenant ID:
Owner.Details.Label.TenantSlug = Tenant Slug:
Owner.Details.Label.OrganizationName = Organization Name:
Owner.Details.Label.AdminEmail = Admin Email:
Owner.Details.Label.Status = Status:
Owner.Details.Label.SubscriptionTier = Subscription Tier:
Owner.Details.Label.OwnerCreated = Owner Created:
Owner.Details.Label.BypassPayment = Bypass Payment:
Owner.Details.Label.AdminAccountGenerated = Admin Account Generated:
Owner.Details.Label.Generated = Generated:
Owner.Details.Label.CredentialExpires = Credential Expires:
Owner.Details.Label.DaysRemaining = days remaining
Owner.Details.Status.ExpiredOn = Expired on {0}
Owner.Details.Status.NotSet = Not set
Owner.Details.Section.AdminAccountStatus = Admin Account Status
Owner.Details.Alert.AdminGenerated = Admin account has been generated.
Owner.Details.Section.OwnerGeneratedUsers = Owner-Generated Admin Users:
Owner.Details.Status.Expired = Expired
Owner.Details.Label.Expires = Expires: {0}
Owner.Details.Button.ExtendExpiration = Extend Expiration
Owner.Details.Placeholder.Days = Days
Owner.Details.Alert.AdminNotGenerated = Admin account has not been generated yet.
Owner.Details.Button.GenerateAdmin = Generate Admin Account
Owner.Details.Section.Actions = Actions
Owner.Details.Button.ViewStatus = View Status
Owner.Details.Button.BackToList = Back to List
```

### GenerateAdmin.cshtml
```
Owner.GenerateAdmin.Title = Generate Admin Account
Owner.GenerateAdmin.Header = Generate Admin Account
Owner.GenerateAdmin.Subtitle = Generate admin credentials for: {0}
Owner.GenerateAdmin.Info.WhatWillBeGenerated = What will be generated:
Owner.GenerateAdmin.Info.AdminUsername = Admin username (format: admin-{tenant-slug})
Owner.GenerateAdmin.Info.SecurePassword = Secure password (16 characters, mixed case, numbers, symbols)
Owner.GenerateAdmin.Info.CredentialsExpire = Credentials will expire after the specified number of days
Owner.GenerateAdmin.Info.MustChangePassword = Admin must change password on first login
Owner.GenerateAdmin.Label.ExpirationDays = Expiration Days
Owner.GenerateAdmin.Help.ExpirationDays = Number of days until credentials expire (7-90 days, default: 14)
Owner.GenerateAdmin.Warning.Important = Important:
Owner.GenerateAdmin.Warning.ShownOnce = Credentials will be shown only once. Make sure to save them securely or deliver them to the tenant admin immediately.
Owner.GenerateAdmin.Button.Cancel = Cancel
Owner.GenerateAdmin.Button.Generate = Generate Admin Account
```

### Index.cshtml (Owner Dashboard)
```
Owner.Index.Title = Owner Dashboard
Owner.Index.Subtitle = Platform administration - Manage tenants, sectors, frameworks, and regulatory content
Owner.Index.Section.TenantManagement = Tenant Management
Owner.Index.Stats.TotalTenants = Total Tenants
Owner.Index.Stats.ActiveTenants = Active Tenants
Owner.Index.Stats.OwnerCreated = Owner Created
Owner.Index.Stats.WithAdmin = With Admin
Owner.Index.Section.SectorsFrameworks = Sectors & Frameworks (KSA GOSI: 70+ → 18 Main)
Owner.Index.Stats.MainSectors = Main Sectors
Owner.Index.Stats.MainSectors.Description = GRC Categories
Owner.Index.Stats.GosiSubSectors = GOSI Sub-Sectors
Owner.Index.Stats.GosiSubSectors.Description = ISIC Rev 4
Owner.Index.Stats.FrameworkMappings = Framework Mappings
Owner.Index.Stats.FrameworkMappings.Description = Sector → Framework
Owner.Index.Stats.EvidenceTypes = Evidence Types
Owner.Index.Stats.EvidenceTypes.Description = Scoring Criteria
Owner.Index.Section.RegulatoryContent = Regulatory Content
Owner.Index.Stats.Regulators = Regulators
Owner.Index.Stats.Regulators.Description = KSA + International
Owner.Index.Stats.Frameworks = Frameworks
Owner.Index.Stats.Frameworks.Description = Compliance Standards
Owner.Index.Stats.Controls = Controls
Owner.Index.Stats.Controls.Description = Compliance Requirements
Owner.Index.Stats.Workflows = Workflows
Owner.Index.Stats.Workflows.Description = Process Templates
Owner.Index.Section.QuickActions = Quick Actions
Owner.Index.Button.CreateTenant = Create Tenant
Owner.Index.Button.ViewTenants = View Tenants
Owner.Index.Button.AddRegulator = Add Regulator
Owner.Index.Button.AddFramework = Add Framework
Owner.Index.Button.AddControl = Add Control
Owner.Index.Button.SectorMapGuide = Sector Map Guide
Owner.Index.Tab.Regulators = Regulators
Owner.Index.Tab.Frameworks = Frameworks
Owner.Index.Tab.Controls = Controls
Owner.Index.Tab.GosiMapping = GOSI Mapping
Owner.Index.Filter.AllTypes = All Types
Owner.Index.Filter.Government = Government
Owner.Index.Filter.Industry = Industry
Owner.Index.Filter.International = International
Owner.Index.Filter.Search = Search regulators...
Owner.Index.Button.AddNew = Add New
Owner.Index.Button.ExportCSV = Export CSV
Owner.Index.Table.Code = Code
Owner.Index.Table.Name = Name
Owner.Index.Table.NameArabic = الاسم
Owner.Index.Table.Jurisdiction = Jurisdiction
Owner.Index.Table.Type = Type
Owner.Index.Table.Website = Website
Owner.Index.Table.Actions = Actions
Owner.Index.Loading = Loading...
Owner.Index.NoRegulatorsFound = No regulators found
Owner.Index.Filter.AllFrameworks = All Frameworks
Owner.Index.Table.Version = Version
Owner.Index.Table.Controls = Controls
Owner.Index.Table.Status = Status
Owner.Index.Status.Active = Active
Owner.Index.Status.Inactive = Inactive
Owner.Index.NoFrameworksFound = No frameworks found
Owner.Index.Filter.AllDomains = All Domains
Owner.Index.Table.ControlNumber = Control #
Owner.Index.Table.Framework = Framework
Owner.Index.Table.Domain = Domain
Owner.Index.Table.Title = Title
Owner.Index.NoControlsFound = No controls found
Owner.Index.Filter.AllMainSectors = All Main Sectors (18)
Owner.Index.Table.ISIC = ISIC
Owner.Index.Table.GosiCode = GOSI Code
Owner.Index.Table.SubSectorEN = Sub-Sector (EN)
Owner.Index.Table.SubSectorAR = القطاع الفرعي
Owner.Index.Table.MainSector = Main Sector
Owner.Index.Table.Regulator = Regulator
Owner.Index.NoGosiMappings = Error loading GOSI mappings
Owner.Index.Modal.AddRegulator = Add Regulator
Owner.Index.Label.Code = Code
Owner.Index.Placeholder.Code = e.g., NCA, SAMA
Owner.Index.Label.NameEnglish = Name (English)
Owner.Index.Label.NameArabic = Name (Arabic)
Owner.Index.Label.Jurisdiction = Jurisdiction
Owner.Index.Label.Type = Type
Owner.Index.Label.Website = Website
Owner.Index.Label.ContactEmail = Contact Email
Owner.Index.Label.IsPrimary = Primary Regulator
Owner.Index.Label.Description = Description
Owner.Index.Button.Cancel = Cancel
Owner.Index.Button.SaveRegulator = Save Regulator
Owner.Index.Modal.AddFramework = Add Framework
Owner.Index.Label.Version = Version
Owner.Index.Label.Category = Category
Owner.Index.Option.Regulatory = Regulatory
Owner.Index.Option.IndustryStandard = Industry Standard
Owner.Index.Option.BestPractice = Best Practice
Owner.Index.Label.RegulatorCode = Regulator Code
Owner.Index.Label.DescriptionEnglish = Description (English)
Owner.Index.Label.Active = Active
Owner.Index.Button.SaveFramework = Save Framework
Owner.Index.Modal.AddControl = Add Control
Owner.Index.Label.FrameworkCode = Framework Code
Owner.Index.Label.ControlNumber = Control Number
Owner.Index.Label.Domain = Domain
Owner.Index.Placeholder.Domain = e.g., Governance
Owner.Index.Label.ControlType = Control Type
Owner.Index.Option.Preventive = Preventive
Owner.Index.Option.Detective = Detective
Owner.Index.Option.Corrective = Corrective
Owner.Index.Label.TitleEnglish = Title (English)
Owner.Index.Label.TitleArabic = Title (Arabic)
Owner.Index.Label.RequirementEnglish = Requirement (English)
Owner.Index.Label.MaturityLevel = Maturity Level
Owner.Index.Option.Level1 = Level 1 - Initial
Owner.Index.Option.Level2 = Level 2 - Managed
Owner.Index.Option.Level3 = Level 3 - Defined
Owner.Index.Option.Level4 = Level 4 - Measured
Owner.Index.Option.Level5 = Level 5 - Optimized
Owner.Index.Label.ISO27001Mapping = ISO 27001 Mapping
Owner.Index.Label.NISTMapping = NIST Mapping
Owner.Index.Label.ImplementationGuidance = Implementation Guidance
Owner.Index.Label.EvidenceRequirements = Evidence Requirements
Owner.Index.Placeholder.EvidenceRequirements = What evidence is needed to demonstrate compliance?
Owner.Index.Button.SaveControl = Save Control
Owner.Index.Modal.ControlDetails = Control Details
Owner.Index.Section.ControlInformation = Control Information
Owner.Index.Section.Mappings = Mappings
Owner.Index.Section.Requirement = Requirement
Owner.Index.Modal.ConfirmDelete = Confirm Delete
Owner.Index.Delete.Message = Are you sure you want to delete "{0}"? This action cannot be undone.
Owner.Index.Button.Delete = Delete
Owner.Index.Pagination.Previous = Previous
Owner.Index.Pagination.Next = Next
Owner.Index.Error.LoadingRegulators = Error loading regulators
Owner.Index.Error.LoadingFrameworks = Error loading frameworks
Owner.Index.Error.LoadingControls = Error loading controls
```

### Status.cshtml
```
Owner.Status.Title = Tenant Status
Owner.Status.Header = Tenant Status: {0}
Owner.Status.Breadcrumb.OwnerDashboard = Owner Dashboard
Owner.Status.Breadcrumb.AllTenants = All Tenants
Owner.Status.Breadcrumb.Details = Details
Owner.Status.Breadcrumb.Status = Status
Owner.Status.Alert.CredentialsExpired = Credentials Expired:
Owner.Status.Alert.CredentialsExpiredMessage = Admin credentials have expired. Please generate new credentials.
Owner.Status.Alert.Warning = Warning:
Owner.Status.Alert.WillExpireMessage = Credentials will expire in {0} day(s). Consider extending expiration.
Owner.Status.Section.TenantStatus = Tenant Status
Owner.Status.Label.Status = Status:
Owner.Status.Label.SubscriptionTier = Subscription Tier:
Owner.Status.Label.OwnerCreated = Owner Created:
Owner.Status.Label.BypassPayment = Bypass Payment:
Owner.Status.Label.AdminAccountGenerated = Admin Account Generated:
Owner.Status.Section.CredentialExpiration = Credential Expiration
Owner.Status.Label.ExpiresAt = Expires At:
Owner.Status.Label.DaysRemaining = Days Remaining:
Owner.Status.Status.Expired = Expired
Owner.Status.Label.Days = {0} days
Owner.Status.Placeholder.AdditionalDays = Additional days
Owner.Status.Button.Extend = Extend
Owner.Status.NoExpiration = No expiration set
Owner.Status.Section.OwnerGeneratedUsers = Owner-Generated Admin Users
Owner.Status.Table.RoleCode = Role Code
Owner.Status.Table.Status = Status
Owner.Status.Table.ExpiresAt = Expires At
Owner.Status.Table.MustChangePassword = Must Change Password
Owner.Status.Status.Active = Active
Owner.Status.Section.CredentialDeliveryHistory = Credential Delivery History
Owner.Status.Label.DeliveryMethod = Delivery Method:
Owner.Status.Label.CredentialsDelivered = Credentials Delivered:
Owner.Status.Label.Delivered = Delivered: {0}
Owner.Status.Label.DeliveryNotes = Delivery Notes:
Owner.Status.NoDeliveryHistory = No delivery history available.
Owner.Status.Button.BackToDetails = Back to Details
```

### Tenants.cshtml
```
Owner.Tenants.Title = All Tenants
Owner.Tenants.Subtitle = Manage all tenants in the system
Owner.Tenants.Button.CreateNewTenant = Create New Tenant
Owner.Tenants.Table.OrganizationName = Organization Name
Owner.Tenants.Table.TenantSlug = Tenant Slug
Owner.Tenants.Table.Status = Status
Owner.Tenants.Table.SubscriptionTier = Subscription Tier
Owner.Tenants.Table.OwnerCreated = Owner Created
Owner.Tenants.Table.AdminGenerated = Admin Generated
Owner.Tenants.Table.ExpiresAt = Expires At
Owner.Tenants.Table.CreatedDate = Created Date
Owner.Tenants.Table.Actions = Actions
Owner.Tenants.Status.Active = Active
Owner.Tenants.Status.Yes = Yes
Owner.Tenants.Status.No = No
Owner.Tenants.Status.Expired = Expired
Owner.Tenants.Button.Details = Details
Owner.Tenants.Button.GenerateAdmin = Generate Admin
```

---

## OwnerSetup Views (1 file)

### Index.cshtml
```
OwnerSetup.Index.Title = Owner Setup - First Time Configuration
OwnerSetup.Index.Header = Owner Account Setup
OwnerSetup.Index.Warning.OwnerExists = {0}
OwnerSetup.Index.Alert.FirstTimeSetup = First Time Setup
OwnerSetup.Index.Alert.SetupDescription = This is a one-time setup to create the first owner account. After this setup is complete, this page will no longer be accessible.
OwnerSetup.Index.Label.Email = Email
OwnerSetup.Index.Label.FirstName = First Name
OwnerSetup.Index.Label.LastName = Last Name
OwnerSetup.Index.Label.Password = Password
OwnerSetup.Index.Help.Password = Password must be at least 8 characters long.
OwnerSetup.Index.Label.ConfirmPassword = Confirm Password
OwnerSetup.Index.Label.OrganizationName = Organization Name
OwnerSetup.Index.Help.OrganizationName = Optional: Your organization name.
OwnerSetup.Index.Button.CreateOwnerAccount = Create Owner Account
```

---

## Specialized Views (6 files)

### ActionPlans/Index.cshtml
**NOTE: This file is entirely in Arabic - already localized**
```
ActionPlans.Index.Title = خطط العمل
ActionPlans.Index.Subtitle = إدارة خطط العمل والإجراءات التصحيحية
ActionPlans.Index.Button.NewActionPlan = خطة عمل جديدة
ActionPlans.Index.Stats.TotalPlans = إجمالي الخطط
ActionPlans.Index.Stats.InProgress = قيد التنفيذ
ActionPlans.Index.Stats.Completed = مكتملة
ActionPlans.Index.Stats.Overdue = متأخرة
ActionPlans.Index.Filter.Status = الحالة
ActionPlans.Index.Filter.AllStatuses = جميع الحالات
ActionPlans.Index.Filter.Pending = معلقة
ActionPlans.Index.Filter.Priority = الأولوية
ActionPlans.Index.Filter.AllPriorities = جميع الأولويات
ActionPlans.Index.Filter.Critical = حرجة
ActionPlans.Index.Filter.High = عالية
ActionPlans.Index.Filter.Medium = متوسطة
ActionPlans.Index.Filter.Low = منخفضة
ActionPlans.Index.Filter.Owner = المسؤول
ActionPlans.Index.Filter.AllOwners = جميع المسؤولين
ActionPlans.Index.Filter.Search = البحث
ActionPlans.Index.Placeholder.Search = ابحث عن خطة عمل...
ActionPlans.Index.Table.PlanNumber = رقم الخطة
ActionPlans.Index.Table.Title = العنوان
ActionPlans.Index.Table.RelatedFramework = الإطار المرتبط
ActionPlans.Index.Table.Owner = المسؤول
ActionPlans.Index.Table.Priority = الأولوية
ActionPlans.Index.Table.Progress = التقدم
ActionPlans.Index.Table.DueDate = تاريخ الاستحقاق
ActionPlans.Index.Table.Status = الحالة
ActionPlans.Index.Table.Actions = الإجراءات
ActionPlans.Index.Pagination.Previous = السابق
ActionPlans.Index.Pagination.Next = التالي
ActionPlans.Index.Modal.NewActionPlan = خطة عمل جديدة
ActionPlans.Index.Modal.Label.Title = عنوان الخطة
ActionPlans.Index.Modal.Placeholder.Title = أدخل عنوان خطة العمل
ActionPlans.Index.Modal.Label.Framework = الإطار المرتبط
ActionPlans.Index.Modal.Option.SelectFramework = اختر الإطار
ActionPlans.Index.Modal.Label.Owner = المسؤول
ActionPlans.Index.Modal.Option.SelectOwner = اختر المسؤول
ActionPlans.Index.Modal.Label.Priority = الأولوية
ActionPlans.Index.Modal.Label.DueDate = تاريخ الاستحقاق
ActionPlans.Index.Modal.Label.Description = الوصف
ActionPlans.Index.Modal.Placeholder.Description = وصف تفصيلي لخطة العمل
ActionPlans.Index.Modal.Button.Cancel = إلغاء
ActionPlans.Index.Modal.Button.Save = حفظ الخطة
```

### AssessmentExecution/Execute.cshtml
```
AssessmentExecution.Execute.Title = Execute Assessment
AssessmentExecution.Execute.Breadcrumb.Assessments = Assessments
AssessmentExecution.Execute.Loading.Framework = Loading framework information...
AssessmentExecution.Execute.Button.BackToDetails = Back to Details
AssessmentExecution.Execute.Button.Export = Export
AssessmentExecution.Execute.Section.OverallProgress = Overall Progress
AssessmentExecution.Execute.Stats.Completed = Completed
AssessmentExecution.Execute.Stats.InProgress = In Progress
AssessmentExecution.Execute.Stats.Pending = Pending
AssessmentExecution.Execute.Stats.AvgScore = Avg Score
AssessmentExecution.Execute.Loading.Message = Loading assessment data...
AssessmentExecution.Execute.Modal.UploadEvidence = Upload Evidence
AssessmentExecution.Execute.Label.Title = Title
AssessmentExecution.Execute.Label.Description = Description
AssessmentExecution.Execute.Label.File = File
AssessmentExecution.Execute.Help.FileSize = Max file size: 10MB
AssessmentExecution.Execute.Button.Cancel = Cancel
AssessmentExecution.Execute.Button.Upload = Upload
AssessmentExecution.Execute.Modal.Notes = Notes
AssessmentExecution.Execute.Placeholder.AddNote = Add a new note...
AssessmentExecution.Execute.Button.SaveNote = Save Note
AssessmentExecution.Execute.Section.NotesHistory = Notes History
AssessmentExecution.Execute.NoNotes = No notes yet
AssessmentExecution.Execute.Modal.ScoringGuide = Scoring Guide
AssessmentExecution.Execute.Status.NotStarted = Not Started
AssessmentExecution.Execute.Status.InProgress = In Progress
AssessmentExecution.Execute.Status.Compliant = Compliant
AssessmentExecution.Execute.Status.PartiallyCompliant = Partially Compliant
AssessmentExecution.Execute.Status.NonCompliant = Non-Compliant
AssessmentExecution.Execute.Status.NotApplicable = Not Applicable
AssessmentExecution.Execute.Label.Requirements = requirements
AssessmentExecution.Execute.Section.Requirement = Requirement
AssessmentExecution.Execute.NoRequirementText = No requirement text
AssessmentExecution.Execute.Section.ImplementationGuidance = Implementation Guidance
AssessmentExecution.Execute.Section.BestPractices = Best Practices:
AssessmentExecution.Execute.Section.CommonGaps = Common Gaps:
AssessmentExecution.Execute.Label.Score = Score (0-{0})
AssessmentExecution.Execute.Button.ViewScoringGuide = View Scoring Guide
AssessmentExecution.Execute.Label.ScoreRationale = Score Rationale
AssessmentExecution.Execute.Button.Evidence = Evidence
AssessmentExecution.Execute.Button.Notes = Notes
AssessmentExecution.Execute.Alert.StatusUpdated = Status updated successfully
AssessmentExecution.Execute.Alert.ScoreUpdated = Score updated successfully
AssessmentExecution.Execute.Alert.ProvideTitleAndFile = Please provide a title and select a file
AssessmentExecution.Execute.Alert.EvidenceUploaded = Evidence uploaded successfully
AssessmentExecution.Execute.Alert.EnterNote = Please enter a note
AssessmentExecution.Execute.Alert.NoteSaved = Note saved successfully
AssessmentExecution.Execute.ScoringGuide.NoGuide = No scoring guide available for this requirement.
AssessmentExecution.Execute.ScoringGuide.DefaultCriteria = Default scoring criteria:
AssessmentExecution.Execute.ScoringGuide.Compliant = 80-100: Compliant - Fully implemented
AssessmentExecution.Execute.ScoringGuide.PartiallyCompliant = 50-79: Partially Compliant - Partially implemented
AssessmentExecution.Execute.ScoringGuide.NonCompliant = 0-49: Non-Compliant - Not implemented or significant gaps
AssessmentExecution.Execute.Table.Score = Score
AssessmentExecution.Execute.Table.Label = Label
AssessmentExecution.Execute.Table.Criteria = Criteria
```

### AssessmentTemplate/Index.cshtml
```
AssessmentTemplate.Index.Title = Assessment Templates
AssessmentTemplate.Index.Subtitle = Pre-configured templates for compliance assessments
AssessmentTemplate.Index.Stats.Templates = {0} Templates
AssessmentTemplate.Index.Card.NoDescription = No description
AssessmentTemplate.Index.Card.Version = v{0}
AssessmentTemplate.Index.Button.View = View
AssessmentTemplate.Index.Button.Start = Start
AssessmentTemplate.Index.Alert.NoTemplates = No assessment templates available. Complete onboarding to generate templates.
```

### AuditPackage/Index.cshtml
```
AuditPackage.Index.Title = Audit Package Generation
AuditPackage.Index.Subtitle = Export evidence packages for auditors
AuditPackage.Index.Stats.CompletedAssessments = Completed Assessments
AuditPackage.Index.Stats.ApprovedEvidence = Approved Evidence Items
AuditPackage.Index.Section.AvailableForExport = Available for Export
AuditPackage.Index.Table.Assessment = Assessment
AuditPackage.Index.Table.Status = Status
AuditPackage.Index.Table.Created = Created
AuditPackage.Index.Table.Actions = Actions
AuditPackage.Index.Button.Preview = Preview
AuditPackage.Index.Button.Export = Export
AuditPackage.Index.Alert.NoAssessments = No completed assessments available for export
```

### CCM/Index.cshtml
```
CCM.Index.Title = CCM - Continuous Control Monitoring
CCM.Index.Subtitle = Automated control testing and validation
CCM.Index.Stats.TotalTests = Total Tests
CCM.Index.Stats.Passed = Passed
CCM.Index.Stats.Failed = Failed
CCM.Index.Section.AvailableTests = Available Tests
CCM.Index.Table.TestName = Test Name
CCM.Index.Table.Type = Type
CCM.Index.Table.Action = Action
CCM.Index.Button.Run = Run
CCM.Index.Alert.NoTests = No tests configured
CCM.Index.Section.RecentResults = Recent Results
CCM.Index.Table.Executed = Executed
CCM.Index.Table.Result = Result
CCM.Index.Table.By = By
CCM.Index.Status.Pass = Pass
CCM.Index.Status.Fail = Fail
CCM.Index.Alert.NoResults = No results yet
```

### CCM/Package.cshtml
```
CCM.Package.Title = CCM Package Details
CCM.Package.Breadcrumb.CCM = CCM
CCM.Package.Breadcrumb.PackageDetails = Package Details
CCM.Package.Header = Control Package
CCM.Package.Subtitle = Continuous Control Monitoring Package
CCM.Package.Button.BackToCCM = Back to CCM
CCM.Package.Section.Requirements = Requirements
CCM.Package.Table.Requirement = Requirement
CCM.Package.Table.Status = Status
CCM.Package.Table.LastCheck = Last Check
CCM.Package.Status.Compliant = Compliant
CCM.Package.Alert.NoRequirements = No requirements found for this package.
CCM.Package.Section.Evidence = Evidence
CCM.Package.Status.Valid = Valid
CCM.Package.Alert.NoEvidence = No evidence collected yet.
CCM.Package.Section.ComplianceScore = Compliance Score
CCM.Package.Score.Overall = Overall Compliance
CCM.Package.Section.MonitoringStatus = Monitoring Status
CCM.Package.Label.LastScan = Last Scan:
CCM.Package.Label.NextScan = Next Scan:
CCM.Package.Label.Frequency = Frequency:
CCM.Package.Frequency.Daily = Daily
```

---

## Summary

**Total Localization Keys**: ~450 keys across 20 files

### Key Categories:
- **OrgSetup (7 files)**: ~120 keys
- **Owner (7 files)**: ~200 keys
- **OwnerSetup (1 file)**: ~15 keys
- **ActionPlans (1 file)**: ~50 keys (Arabic, already localized)
- **AssessmentExecution (1 file)**: ~60 keys
- **AssessmentTemplate (1 file)**: ~8 keys
- **AuditPackage (1 file)**: ~12 keys
- **CCM (2 files)**: ~30 keys

### Next Steps:
1. Add these keys to `en.json` and `ar.json` resource files
2. Convert all view files to use `@Localizer["Key"]` pattern
3. Test localization switching for all views
4. Verify Arabic RTL rendering for all components

**Status**: ✅ Keys extracted and documented
**Pending**: .resx file updates and view file conversion
