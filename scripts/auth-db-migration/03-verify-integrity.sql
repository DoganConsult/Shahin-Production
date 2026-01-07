-- Auth DB Split Migration Script - Step 3: Verify Data Integrity
-- Run this to check data consistency between GrcMvcDb and GrcAuthDb

-- ====================
-- GrcAuthDb Verification
-- ====================

-- Check for orphaned user roles (users without matching role)
SELECT ur."UserId", ur."RoleId"
FROM "AspNetUserRoles" ur
LEFT JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE r."Id" IS NULL;

-- Check for orphaned user roles (roles without matching user)
SELECT ur."UserId", ur."RoleId"
FROM "AspNetUserRoles" ur
LEFT JOIN "AspNetUsers" u ON ur."UserId" = u."Id"
WHERE u."Id" IS NULL;

-- Check for orphaned user claims
SELECT uc."UserId"
FROM "AspNetUserClaims" uc
LEFT JOIN "AspNetUsers" u ON uc."UserId" = u."Id"
WHERE u."Id" IS NULL;

-- Check for orphaned role claims
SELECT rc."RoleId"
FROM "AspNetRoleClaims" rc
LEFT JOIN "AspNetRoles" r ON rc."RoleId" = r."Id"
WHERE r."Id" IS NULL;

-- ====================
-- Cross-DB Verification (run from app or use dblink)
-- ====================

-- These queries check that all UserIds referenced in GrcMvcDb exist in GrcAuthDb
-- Run against GrcMvcDb:

-- Check TenantUsers references
-- SELECT tu."UserId" FROM "TenantUsers" tu WHERE tu."UserId" NOT IN (SELECT "Id" FROM GrcAuthDb."AspNetUsers");

-- Check UserRoleAssignments references
-- SELECT ura."UserId", ura."RoleId" FROM "UserRoleAssignments" ura
-- WHERE ura."UserId" NOT IN (SELECT "Id" FROM GrcAuthDb."AspNetUsers")
--    OR ura."RoleId" NOT IN (SELECT "Id" FROM GrcAuthDb."AspNetRoles");

-- Check WorkspaceMemberships references
-- SELECT wm."UserId" FROM "WorkspaceMemberships" wm WHERE wm."UserId" NOT IN (SELECT "Id" FROM GrcAuthDb."AspNetUsers");

-- ====================
-- Summary Statistics
-- ====================

SELECT 'GrcAuthDb Statistics' as report;
SELECT 'Total Users' as metric, COUNT(*) as count FROM "AspNetUsers"
UNION ALL
SELECT 'Active Users', COUNT(*) FROM "AspNetUsers" WHERE "IsActive" = true
UNION ALL
SELECT 'Total Roles', COUNT(*) FROM "AspNetRoles"
UNION ALL
SELECT 'User-Role Assignments', COUNT(*) FROM "AspNetUserRoles"
UNION ALL
SELECT 'User Claims', COUNT(*) FROM "AspNetUserClaims"
UNION ALL
SELECT 'Role Claims', COUNT(*) FROM "AspNetRoleClaims";
