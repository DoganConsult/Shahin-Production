# Implementation Plan: World-Class Team Engagements & Ecosystem Facility

## Overview
Implement comprehensive team collaboration and ecosystem partnership features to bring the TrialLifecycleService to world-class SaaS standards.

---

## Part 1: Team Collaboration (World-Class)

### 1.1 Team Member Invitation System
**File:** `TrialLifecycleService.cs` - `InviteTeamMemberAsync`

**Implementation:**
- Validate team size limits (5 for trial)
- Create/lookup user by email
- Generate secure invite token
- Create TenantUser with "invited" status
- Send templated invite email with personalized welcome message
- Return invite link for sharing

### 1.2 Team Management
**File:** `TrialLifecycleService.cs` - `GetTrialTeamAsync`

**Implementation:**
- Query TenantUsers with User navigation
- Calculate engagement metrics per member
- Include last activity, actions completed, contribution score
- Return sorted by role (admin first, then by activity)

### 1.3 Activity Tracking
**File:** `TrialLifecycleService.cs` - `TrackTeamActivityAsync`

**Implementation:**
- Create UserActivity/TeamActivity record
- Update TenantUser.LastActiveAt and ActionsCompleted
- Calculate contribution score based on activity weight
- Support activity types: login, document_upload, control_review, report_generated, comment, mention

### 1.4 New Methods to Add

#### Team Onboarding Checklist
```csharp
Task<TeamOnboardingProgress> GetTeamOnboardingProgressAsync(Guid tenantId)
```
- Track completion of key onboarding steps
- Steps: profile_complete, first_framework_added, first_control_reviewed, team_invited, first_report_generated

#### Team Engagement Dashboard
```csharp
Task<TeamEngagementDashboard> GetTeamEngagementAsync(Guid tenantId)
```
- Aggregate team metrics
- Individual contribution leaderboard
- Activity heatmap data
- Collaboration score

#### Bulk Team Invite
```csharp
Task<BulkInviteResult> InviteTeamMembersBulkAsync(Guid tenantId, List<TeamInviteRequest> requests)
```
- Process multiple invites in batch
- Return success/failure per invite

---

## Part 2: Ecosystem Collaboration (World-Class)

### 2.1 Partner Connection Requests
**File:** `TrialLifecycleService.cs` - `RequestPartnerConnectionAsync`

**Implementation:**
- Create EcosystemConnection record
- Set status to "pending"
- Store shared data types as JSON
- Send notification email to partner
- Log audit event

### 2.2 Available Partners
**File:** `TrialLifecycleService.cs` - `GetAvailablePartnersAsync`

**Implementation:**
- Query real Partner registry (new entity if needed)
- Filter by sector compatibility
- Include ratings, certifications, services
- Sort by rating and connections count

### 2.3 Ecosystem Connections
**File:** `TrialLifecycleService.cs` - `GetEcosystemConnectionsAsync`

**Implementation:**
- Query EcosystemConnections for tenant
- Include partner details
- Return connection status and interaction metrics

### 2.4 New Methods to Add

#### Approve/Reject Partner Connection
```csharp
Task<bool> ApprovePartnerConnectionAsync(Guid connectionId, string approvedBy)
Task<bool> RejectPartnerConnectionAsync(Guid connectionId, string reason, string rejectedBy)
```

#### Partner Interaction Tracking
```csharp
Task TrackPartnerInteractionAsync(Guid connectionId, string interactionType, string details)
```

#### Integration Marketplace
```csharp
Task<List<IntegrationDto>> GetAvailableIntegrationsAsync(string category)
Task<IntegrationResult> ConnectIntegrationAsync(Guid tenantId, string integrationCode, Dictionary<string, string> config)
```

---

## Part 3: New DTOs Required

### Team DTOs
```csharp
public class TeamOnboardingProgress
{
    public Guid TenantId { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public double ProgressPercent { get; set; }
    public List<OnboardingStep> Steps { get; set; }
}

public class OnboardingStep
{
    public string StepId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
}

public class TeamEngagementDashboard
{
    public Guid TenantId { get; set; }
    public double OverallScore { get; set; }
    public int ActiveMembers { get; set; }
    public int TotalActions { get; set; }
    public List<MemberContribution> Leaderboard { get; set; }
    public Dictionary<string, int> ActivityByDay { get; set; }
    public Dictionary<string, int> ActivityByType { get; set; }
}

public class MemberContribution
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public int ActionsCompleted { get; set; }
    public double ContributionScore { get; set; }
    public int Rank { get; set; }
}

public class BulkInviteResult
{
    public int TotalRequested { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<InviteStatus> Results { get; set; }
}
```

### Ecosystem DTOs
```csharp
public class IntegrationDto
{
    public string IntegrationCode { get; set; }
    public string Name { get; set; }
    public string Category { get; set; } // siem, ticketing, hr, identity, cloud
    public string Description { get; set; }
    public string LogoUrl { get; set; }
    public bool IsAvailableInTrial { get; set; }
    public List<string> RequiredScopes { get; set; }
}

public class IntegrationResult
{
    public bool Success { get; set; }
    public string IntegrationCode { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string> ConnectionDetails { get; set; }
}
```

---

## Part 4: Interface Updates

**File:** `ITrialLifecycleService.cs`

Add new method signatures:
```csharp
// Team Collaboration
Task<TeamOnboardingProgress> GetTeamOnboardingProgressAsync(Guid tenantId);
Task<TeamEngagementDashboard> GetTeamEngagementAsync(Guid tenantId);
Task<BulkInviteResult> InviteTeamMembersBulkAsync(Guid tenantId, List<TeamInviteRequest> requests);

// Ecosystem
Task<bool> ApprovePartnerConnectionAsync(Guid connectionId, string approvedBy);
Task<bool> RejectPartnerConnectionAsync(Guid connectionId, string reason, string rejectedBy);
Task TrackPartnerInteractionAsync(Guid connectionId, string interactionType, string details);
Task<List<IntegrationDto>> GetAvailableIntegrationsAsync(string category);
Task<IntegrationResult> ConnectIntegrationAsync(Guid tenantId, string integrationCode, Dictionary<string, string> config);
```

---

## Implementation Order

1. **Phase 1 - Core Team Features** (Implement stubbed methods)
   - `InviteTeamMemberAsync` - Full implementation
   - `GetTrialTeamAsync` - Full implementation
   - `TrackTeamActivityAsync` - Full implementation

2. **Phase 2 - Ecosystem Features** (Implement stubbed methods)
   - `RequestPartnerConnectionAsync` - Full implementation
   - `GetAvailablePartnersAsync` - Real data (keep mock as fallback)
   - `GetEcosystemConnectionsAsync` - Full implementation

3. **Phase 3 - New DTOs**
   - Create TrialTeamDtos.cs with team-related DTOs
   - Create EcosystemDtos.cs with ecosystem-related DTOs

4. **Phase 4 - Interface Updates**
   - Add new methods to ITrialLifecycleService

5. **Phase 5 - Advanced Features**
   - Team onboarding progress tracking
   - Team engagement dashboard
   - Bulk invite
   - Partner approval/rejection
   - Integration marketplace

---

## Files to Modify

| File | Changes |
|------|---------|
| `Services/Implementations/TrialLifecycleService.cs` | Implement stubbed methods + add new methods |
| `Services/Interfaces/ITrialLifecycleService.cs` | Add new method signatures |
| `Models/DTOs/TrialTeamDtos.cs` | New file - Team DTOs |
| `Models/DTOs/EcosystemDtos.cs` | New file - Ecosystem DTOs |

---

## Success Criteria

- All stubbed methods fully implemented
- Team invites work with email notifications
- Activity tracking persists to database
- Partner connections can be requested and managed
- Integration marketplace returns available integrations
- All new DTOs properly defined with validation
