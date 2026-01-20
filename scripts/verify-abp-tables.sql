-- Verification Script: Check if ABP Framework tables exist in PostgreSQL
-- Run this query to verify all ABP tables are created

-- Check ABP Identity tables
SELECT 'Identity Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN ('AspNetUsers', 'AspNetRoles', 'AspNetUserRoles', 'AspNetUserClaims', 'AspNetRoleClaims');

-- Check ABP Permission Management tables
SELECT 'Permission Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'AbpPermissionGrants%';

-- Check ABP Audit Logging tables
SELECT 'Audit Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'AbpAuditLogs%';

-- Check ABP Feature Management tables
SELECT 'Feature Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'AbpFeatureValues%' OR table_name LIKE 'AbpFeatureGroups%';

-- Check ABP Tenant Management tables
SELECT 'Tenant Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND (table_name LIKE 'AbpTenants%' OR table_name = 'Tenants');

-- Check ABP Settings tables
SELECT 'Settings Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'AbpSettings%';

-- Check OpenIddict tables
SELECT 'OpenIddict Tables' AS Category, COUNT(*) AS TableCount
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'OpenIddict%';

-- Summary: List all ABP-related tables
SELECT table_name, 
       (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = t.table_name) AS column_count
FROM information_schema.tables t
WHERE table_schema = 'public'
  AND (
    table_name LIKE 'Abp%' 
    OR table_name LIKE 'AspNet%'
    OR table_name LIKE 'OpenIddict%'
    OR table_name = 'Tenants'
  )
ORDER BY table_name;
