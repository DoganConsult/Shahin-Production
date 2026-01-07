-- Auth DB Split Migration Script - Step 1: Export Identity Data from GrcMvcDb
-- Run this against the GrcMvcDb (main database)
-- This exports Identity tables in the correct order to avoid FK violations

-- Export order matters due to foreign key dependencies:
-- 1. AspNetRoles (no dependencies)
-- 2. AspNetUsers (no dependencies)
-- 3. AspNetRoleClaims (depends on Roles)
-- 4. AspNetUserClaims (depends on Users)
-- 5. AspNetUserLogins (depends on Users)
-- 6. AspNetUserRoles (depends on Users and Roles)
-- 7. AspNetUserTokens (depends on Users)
-- 8. RoleProfiles (no dependencies, but referenced by Users)

-- Create export schema if needed
CREATE SCHEMA IF NOT EXISTS auth_export;

-- 1. Export AspNetRoles
SELECT * INTO auth_export."AspNetRoles_backup" FROM "AspNetRoles";

-- 2. Export AspNetUsers
SELECT * INTO auth_export."AspNetUsers_backup" FROM "AspNetUsers";

-- 3. Export AspNetRoleClaims
SELECT * INTO auth_export."AspNetRoleClaims_backup" FROM "AspNetRoleClaims";

-- 4. Export AspNetUserClaims
SELECT * INTO auth_export."AspNetUserClaims_backup" FROM "AspNetUserClaims";

-- 5. Export AspNetUserLogins
SELECT * INTO auth_export."AspNetUserLogins_backup" FROM "AspNetUserLogins";

-- 6. Export AspNetUserRoles
SELECT * INTO auth_export."AspNetUserRoles_backup" FROM "AspNetUserRoles";

-- 7. Export AspNetUserTokens
SELECT * INTO auth_export."AspNetUserTokens_backup" FROM "AspNetUserTokens";

-- 8. Export RoleProfiles (app table but referenced by Users)
SELECT * INTO auth_export."RoleProfiles_backup" FROM "RoleProfiles";

-- Verification counts
SELECT 'AspNetRoles' as table_name, COUNT(*) as row_count FROM auth_export."AspNetRoles_backup"
UNION ALL
SELECT 'AspNetUsers', COUNT(*) FROM auth_export."AspNetUsers_backup"
UNION ALL
SELECT 'AspNetRoleClaims', COUNT(*) FROM auth_export."AspNetRoleClaims_backup"
UNION ALL
SELECT 'AspNetUserClaims', COUNT(*) FROM auth_export."AspNetUserClaims_backup"
UNION ALL
SELECT 'AspNetUserLogins', COUNT(*) FROM auth_export."AspNetUserLogins_backup"
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM auth_export."AspNetUserRoles_backup"
UNION ALL
SELECT 'AspNetUserTokens', COUNT(*) FROM auth_export."AspNetUserTokens_backup"
UNION ALL
SELECT 'RoleProfiles', COUNT(*) FROM auth_export."RoleProfiles_backup";
