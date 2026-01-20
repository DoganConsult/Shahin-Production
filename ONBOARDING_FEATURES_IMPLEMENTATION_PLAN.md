# Onboarding Features Implementation Plan

**Date:** 2026-01-20  
**Status:** Implementation Plan Created

---

## 📋 CURRENT STATUS

### ✅ Already Implemented
1. ✅ **Auto-Save Functionality** - Basic implementation exists
   - File: `wwwroot/js/wizard-autosave.js`
   - Endpoint: `POST /OnboardingWizard/AutoSave/{tenantId}/{stepName}`
   - Features: 30-second intervals, field change detection, manual save (Ctrl+S)

2. ✅ **Progress Tracking** - Basic implementation exists
   - Endpoint: `GET /OnboardingWizard/GetProgress/{tenantId}`
   - Fields: `CurrentStep`, `ProgressPercent`, `CompletedSectionsJson`, `LastStepSavedAt`

3. ✅ **Abandonment Detection** - Implemented
   - File: `BackgroundJobs/OnboardingAbandonmentJob.cs`
   - Features: Detects 7+ days incomplete, sends recovery emails

---

## 🚀 ENHANCEMENTS NEEDED

### 1. Enhanced Auto-Save (Priority: HIGH)
**Current**: Basic auto-save every 30 seconds  
**Enhancement**: 
- ✅ Debounced field-level saves (3 seconds after typing stops)
- ✅ Visual save indicators (saving/saved/error)
- ✅ Last saved timestamp display
- ❌ **MISSING**: Browser storage fallback for offline scenarios
- ❌ **MISSING**: Conflict resolution (if data changed server-side)

**Files to Modify**:
- `wwwroot/js/wizard-autosave.js` - Add localStorage backup
- `Controllers/OnboardingWizardController.cs` - Enhance AutoSave endpoint

### 2. Resume Mechanism (Priority: HIGH)
**Current**: `CurrentStep` tracks position, but no explicit resume flow  
**Enhancement**:
- ✅ Wizard redirects to `CurrentStep` on `Index` action
- ❌ **MISSING**: Resume link generation with secure token
- ❌ **MISSING**: Resume from email link
- ❌ **MISSING**: Resume confirmation page

**Files to Create/Modify**:
- `Controllers/OnboardingWizardController.cs` - Add `Resume` action
- `Services/Interfaces/IOnboardingWizardService.cs` - Add `GenerateResumeLinkAsync`
- `Models/Entities/OnboardingResumeToken.cs` - New entity for secure resume links

### 3. Browser Storage Fallback (Priority: MEDIUM)
**Current**: No localStorage backup  
**Enhancement**:
- ❌ Save form data to localStorage on every change
- ❌ Restore from localStorage on page load
- ❌ Sync localStorage → server when online
- ❌ Clear localStorage after successful save

**Files to Create/Modify**:
- `wwwroot/js/wizard-storage.js` - New file for localStorage management
- `wwwroot/js/wizard-autosave.js` - Integrate storage fallback

### 4. Progress Persistence (Priority: HIGH)
**Current**: Basic progress tracking exists  
**Enhancement**:
- ✅ `CurrentStep` is saved
- ✅ `LastStepSavedAt` is updated
- ❌ **MISSING**: Field-level progress tracking (which fields completed)
- ❌ **MISSING**: Progress snapshots for history
- ❌ **MISSING**: Progress analytics

**Files to Create/Modify**:
- `Models/Entities/OnboardingProgressSnapshot.cs` - New entity
- `Services/Implementations/OnboardingWizardService.cs` - Add snapshot methods
- `Controllers/OnboardingWizardController.cs` - Add progress endpoints

### 5. Team Member Provisioning (Priority: CRITICAL)
**Current**: Section H collects team data, but no provisioning  
**Enhancement**:
- ❌ Parse `TeamMembersJson` from Section H
- ❌ Create `ApplicationUser` accounts for each team member
- ❌ Assign roles based on `SelectedRoleCatalogJson`
- ❌ Create workspaces and add members
- ❌ Send invitation emails with setup links
- ❌ Track invitation status

**Files to Create/Modify**:
- `Services/Interfaces/ITeamMemberProvisioningService.cs` - New interface
- `Services/Implementations/TeamMemberProvisioningService.cs` - New service
- `Controllers/OnboardingWizardController.cs` - Call provisioning after Section H save
- `Models/Entities/TeamMemberInvitation.cs` - New entity for tracking

### 6. Data Cleanup Policy (Priority: MEDIUM)
**Current**: Abandonment detection exists, but no cleanup  
**Enhancement**:
- ❌ Background job to cleanup incomplete onboarding after X days (default: 90)
- ❌ Archive incomplete data before deletion
- ❌ Send final warning email before cleanup
- ❌ Configurable retention period per tenant

**Files to Create/Modify**:
- `BackgroundJobs/OnboardingCleanupJob.cs` - New background job
- `BackgroundWorkers/OnboardingCleanupWorker.cs` - New worker
- `Services/Interfaces/IOnboardingCleanupService.cs` - New interface
- `Configuration/OnboardingOptions.cs` - Add cleanup settings

### 7. Resume Link Generation (Priority: MEDIUM)
**Current**: No secure resume links  
**Enhancement**:
- ❌ Generate secure token for resume link
- ❌ Store token with expiry (default: 30 days)
- ❌ Email resume link to user
- ❌ Validate token on resume attempt
- ❌ Track resume link usage

**Files to Create/Modify**:
- `Models/Entities/OnboardingResumeToken.cs` - New entity
- `Services/Interfaces/IOnboardingResumeService.cs` - New interface
- `Services/Implementations/OnboardingResumeService.cs` - New service
- `Controllers/OnboardingWizardController.cs` - Add `Resume` action

---

## 📊 IMPLEMENTATION PRIORITY

| Feature | Priority | Estimated Time | Dependencies |
|---------|----------|---------------|--------------|
| **Team Member Provisioning** | 🔴 CRITICAL | 4-6 hours | Section H data, User creation, Email service |
| **Enhanced Auto-Save** | 🟠 HIGH | 2-3 hours | Existing auto-save, localStorage API |
| **Resume Mechanism** | 🟠 HIGH | 3-4 hours | Token generation, Email service |
| **Progress Persistence** | 🟠 HIGH | 2-3 hours | Database schema, Snapshot entity |
| **Browser Storage Fallback** | 🟡 MEDIUM | 2 hours | localStorage API, Sync logic |
| **Resume Link Generation** | 🟡 MEDIUM | 2-3 hours | Token service, Email service |
| **Data Cleanup Policy** | 🟡 MEDIUM | 2-3 hours | Background job, Archive service |

**Total Estimated Time**: 17-24 hours

---

## 🎯 PHASE 1: Critical Features (Week 1)

### 1. Team Member Provisioning
**Why Critical**: Required for Section H completion, enables multi-user access

**Implementation Steps**:
1. Create `ITeamMemberProvisioningService` interface
2. Implement `TeamMemberProvisioningService`
3. Parse `TeamMembersJson` from `OnboardingWizard`
4. Create `ApplicationUser` accounts using `IIdentityUserAppService`
5. Assign roles using `IUserRoleAssignmentService`
6. Create workspaces using `IWorkspaceManagementService`
7. Send invitation emails using `IGrcEmailService`
8. Track invitations in `TeamMemberInvitation` entity
9. Integrate into `OnboardingWizardController.StepH` POST action

**Files to Create**:
- `Services/Interfaces/ITeamMemberProvisioningService.cs`
- `Services/Implementations/TeamMemberProvisioningService.cs`
- `Models/Entities/TeamMemberInvitation.cs`
- `Migrations/AddTeamMemberInvitationTable.cs`

### 2. Enhanced Auto-Save with Browser Storage
**Why Critical**: Prevents data loss, improves UX

**Implementation Steps**:
1. Enhance `wizard-autosave.js` to save to localStorage
2. Add restore from localStorage on page load
3. Add sync logic (localStorage → server when online)
4. Add visual indicators (saving/saved/error)
5. Add conflict resolution UI

**Files to Modify**:
- `wwwroot/js/wizard-autosave.js`
- `Controllers/OnboardingWizardController.cs` - Enhance AutoSave endpoint

### 3. Resume Mechanism
**Why Critical**: Allows users to continue after interruption

**Implementation Steps**:
1. Create `OnboardingResumeToken` entity
2. Create `IOnboardingResumeService` interface
3. Implement `OnboardingResumeService`
4. Add `Resume` action to `OnboardingWizardController`
5. Generate secure tokens with expiry
6. Email resume links in abandonment recovery emails
7. Validate tokens on resume

**Files to Create**:
- `Models/Entities/OnboardingResumeToken.cs`
- `Services/Interfaces/IOnboardingResumeService.cs`
- `Services/Implementations/OnboardingResumeService.cs`
- `Migrations/AddOnboardingResumeTokenTable.cs`

---

## 🎯 PHASE 2: Important Features (Week 2)

### 4. Progress Persistence & Snapshots
**Why Important**: Enables progress analytics and recovery

**Implementation Steps**:
1. Create `OnboardingProgressSnapshot` entity
2. Add snapshot creation on each step save
3. Add progress history endpoint
4. Add progress analytics dashboard

**Files to Create**:
- `Models/Entities/OnboardingProgressSnapshot.cs`
- `Migrations/AddOnboardingProgressSnapshotTable.cs`

### 5. Data Cleanup Policy
**Why Important**: Prevents database bloat, enforces data retention

**Implementation Steps**:
1. Create `OnboardingCleanupJob`
2. Create `OnboardingCleanupWorker`
3. Add cleanup configuration options
4. Add archive before delete
5. Add final warning email

**Files to Create**:
- `BackgroundJobs/OnboardingCleanupJob.cs`
- `BackgroundWorkers/OnboardingCleanupWorker.cs`
- `Services/Interfaces/IOnboardingCleanupService.cs`
- `Services/Implementations/OnboardingCleanupService.cs`

---

## 📝 DETAILED IMPLEMENTATION SPECS

### Team Member Provisioning Service

```csharp
public interface ITeamMemberProvisioningService
{
    Task<TeamProvisioningResult> ProvisionTeamMembersAsync(
        Guid tenantId, 
        OnboardingWizard wizard, 
        string initiatedByUserId);
    
    Task<bool> SendTeamInvitationsAsync(
        Guid tenantId, 
        List<TeamMemberInvitationDto> invitations);
    
    Task<TeamMemberInvitationStatus> GetInvitationStatusAsync(
        Guid invitationId);
}
```

**Key Methods**:
- `ProvisionTeamMembersAsync` - Main provisioning logic
- `SendTeamInvitationsAsync` - Send invitation emails
- `GetInvitationStatusAsync` - Check invitation status

**Data Flow**:
1. Parse `TeamMembersJson` from `OnboardingWizard`
2. For each team member:
   - Create `ApplicationUser` if not exists
   - Assign role from `SelectedRoleCatalogJson`
   - Add to workspace from `TeamListJson`
   - Create `TeamMemberInvitation` record
   - Send invitation email
3. Return provisioning result with success/failure counts

### Resume Link Generation

```csharp
public interface IOnboardingResumeService
{
    Task<string> GenerateResumeLinkAsync(Guid tenantId, string userEmail);
    Task<bool> ValidateResumeTokenAsync(string token, out Guid tenantId);
    Task<bool> ResumeWizardAsync(string token);
}
```

**Token Format**: `{tenantId}-{timestamp}-{hash}` (Base64 encoded)  
**Expiry**: 30 days (configurable)  
**Storage**: `OnboardingResumeToken` table

---

## ✅ ACCEPTANCE CRITERIA

### Team Member Provisioning
- ✅ All team members from Section H are created as users
- ✅ Roles are assigned correctly
- ✅ Workspaces are created and members added
- ✅ Invitation emails are sent
- ✅ Invitation status is trackable

### Enhanced Auto-Save
- ✅ Form data saved to localStorage
- ✅ Data restored on page load
- ✅ Sync to server when online
- ✅ Visual indicators work
- ✅ No data loss on browser close

### Resume Mechanism
- ✅ Secure resume links generated
- ✅ Links work after 30 days
- ✅ Resume redirects to correct step
- ✅ Links invalidated after use (optional)

---

**Last Updated:** 2026-01-20  
**Next Action**: Start implementing Team Member Provisioning (highest priority)
