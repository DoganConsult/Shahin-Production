-- Production Database Schema Verification Script
-- Run this against GrcAuthDb database to verify all ApplicationUser columns exist

-- 1. Check if AspNetUsers table exists
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_name = 'AspNetUsers'
) AS "Table Exists";

-- 2. List all ApplicationUser custom columns
SELECT 
    column_name AS "Column Name", 
    data_type AS "Data Type", 
    is_nullable AS "Nullable", 
    column_default AS "Default Value"
FROM information_schema.columns
WHERE table_name = 'AspNetUsers'
AND column_name IN (
    'FirstName', 'LastName', 'Department', 'JobTitle',
    'RoleProfileId', 'KsaCompetencyLevel',
    'KnowledgeAreas', 'Skills', 'Abilities', 'AssignedScope',
    'IsActive', 'CreatedDate', 'LastLoginDate',
    'RefreshToken', 'RefreshTokenExpiry',
    'MustChangePassword', 'LastPasswordChangedAt'
)
ORDER BY column_name;

-- 3. Count total custom columns (should be 17)
SELECT COUNT(*) AS "Total Custom Columns"
FROM information_schema.columns
WHERE table_name = 'AspNetUsers'
AND column_name IN (
    'FirstName', 'LastName', 'Department', 'JobTitle',
    'RoleProfileId', 'KsaCompetencyLevel',
    'KnowledgeAreas', 'Skills', 'Abilities', 'AssignedScope',
    'IsActive', 'CreatedDate', 'LastLoginDate',
    'RefreshToken', 'RefreshTokenExpiry',
    'MustChangePassword', 'LastPasswordChangedAt'
);

-- 4. Verify indexes
SELECT indexname AS "Index Name", indexdef AS "Index Definition"
FROM pg_indexes
WHERE tablename = 'AspNetUsers'
AND indexname IN (
    'IX_AspNetUsers_Email',
    'IX_AspNetUsers_IsActive',
    'IX_AspNetUsers_RoleProfileId'
)
ORDER BY indexname;

-- 5. Check migration history
SELECT "MigrationId", "ProductVersion"
FROM "__EFMigrationsHistory"
WHERE "ContextKey" LIKE '%GrcAuthDbContext%'
ORDER BY "MigrationId" DESC
LIMIT 5;

-- 6. Verify foreign key constraint (if RoleProfile table exists)
SELECT 
    tc.constraint_name AS "Constraint Name",
    kcu.column_name AS "Column Name",
    ccu.table_name AS "Referenced Table"
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu 
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage ccu 
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name = 'AspNetUsers'
    AND tc.constraint_name = 'FK_AspNetUsers_RoleProfile_RoleProfileId';
