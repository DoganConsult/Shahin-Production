# Auth DB Split Audit Document

## Phase 1: Preparation (Discovery + Safety) ✅ COMPLETE

---

## 1. Identity Tables Inventory

### Standard ASP.NET Identity Tables (in GrcMvcDb)

| Table | Purpose | ID Type |
|-------|---------|---------|
| `AspNetUsers` | User accounts | `string` (GUID format) |
| `AspNetRoles` | Role definitions | `string` (GUID format) |
| `AspNetUserRoles` | User-Role junction | Composite FK |
| `AspNetUserClaims` | User claims | int PK, string UserId FK |
| `AspNetRoleClaims` | Role claims | int PK, string RoleId FK |
| `AspNetUserLogins` | External logins | Composite PK |
| `AspNetUserTokens` | Auth tokens | Composite PK |

### Custom Identity-Related Fields (in ApplicationUser)

| Field | Purpose | Notes |
|-------|---------|-------|
| `FirstName` | Display name | Move to Auth DB |
| `LastName` | Display name | Move to Auth DB |
| `Department` | Org info | Move to Auth DB |
| `JobTitle` | Org info | Move to Auth DB |
| `RefreshToken` | Session | Move to Auth DB |
| `RefreshTokenExpiry` | Session | Move to Auth DB |
| `IsActive` | Account status | Move to Auth DB |
| `RoleProfileId` | App reference | **SOFT LINK** - keep as Guid only |

---

## 2. App Tables with Hard FKs to Identity (MUST REMOVE)

| Table | Field | FK Target | Action |
|-------|-------|-----------|--------|
| `TenantUser` | `UserId` | `AspNetUsers` | Remove FK → soft link |
| `WorkspaceMembership` | `UserId` | `AspNetUsers` | Remove FK → soft link |
| `UserWorkspace` | `UserId` | `AspNetUsers` | Remove FK → soft link |
| `UserConsent` | `UserId` | `AspNetUsers` | Remove FK → soft link |
| `UserRoleAssignment` | `UserId` | `AspNetUsers` | Remove FK → soft link |
| `UserRoleAssignment` | `RoleId` | `AspNetRoles` | Remove FK → soft link |
| `RolePermission` | `RoleId` | `AspNetRoles` | Remove FK → soft link |
| `RoleFeature` | `RoleId` | `AspNetRoles` | Remove FK → soft link |
| `TenantRoleConfiguration` | `RoleId` | `AspNetRoles` | Remove FK → soft link |

---

## 3. Soft References (Already Safe - No Changes Needed)

- `PlatformAdmin.UserId` (string)
- `AuditEvent.UserId` (string)
- `TaskComment.CommentedByUserId` (string)
- `OnboardingWizard.CompletedByUserId` (string)
- `WorkspaceControl.OwnerUserId` (string)
- `WorkflowTask.AssignedToUserId` (Guid?)
- `WorkflowTask.CompletedByUserId` (Guid?)
- `WorkflowInstance.InitiatedByUserId` (Guid?)
- `Team.ManagerUserId` (Guid?)
- `Asset.OwnerUserId` (Guid?)
- `DelegationRule.*UserId` fields (Guid?)
- `Resilience.*UserId` fields (Guid?)
- ~20+ more tables with soft UserId references

---

## 4. Architecture Decision: Option A

**Auth DB (GrcAuthDb)** - Identity only:
- AspNetUsers, AspNetRoles, AspNetUserRoles
- AspNetUserClaims, AspNetRoleClaims
- AspNetUserLogins, AspNetUserTokens

**Main DB (GrcMvcDb)** - Membership + App data:
- TenantUser (TenantId, UserId, RoleCode, Status)
- WorkspaceMembership (WorkspaceId, UserId, roles)
- UserRoleAssignment (tenant-scoped role assignment)
- All other app tables

---

## 5. ID Type: `string` for UserId

- Auth DB: `AspNetUsers.Id` = string (Identity default)
- Main DB: Store UserId as `string` (no conversion)
- Lookup: `IUserDirectoryService.GetUsersByIdsAsync(IEnumerable<string>)`

---

## Status

| Phase | Status |
|-------|--------|
| Phase 1: Audit | ✅ COMPLETE |
| Phase 2: Infrastructure | 🔄 IN PROGRESS |
| Phase 3: Code Changes | ⏳ PENDING |
| Phase 4: Data Migration | ⏳ PENDING |
| Phase 5: Cutover | ⏳ PENDING |
