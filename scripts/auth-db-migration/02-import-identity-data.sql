-- Auth DB Split Migration Script - Step 2: Import Identity Data to GrcAuthDb
-- Run this against the GrcAuthDb (new auth database)
-- Assumes EF Core migrations have already created the schema

-- Disable foreign key checks during import
SET session_replication_role = 'replica';

-- Clear existing data (if any) - BE CAREFUL IN PRODUCTION
TRUNCATE TABLE "AspNetUserTokens" CASCADE;
TRUNCATE TABLE "AspNetUserRoles" CASCADE;
TRUNCATE TABLE "AspNetUserLogins" CASCADE;
TRUNCATE TABLE "AspNetUserClaims" CASCADE;
TRUNCATE TABLE "AspNetRoleClaims" CASCADE;
TRUNCATE TABLE "AspNetUsers" CASCADE;
TRUNCATE TABLE "AspNetRoles" CASCADE;
TRUNCATE TABLE "RoleProfile" CASCADE;

-- Import in correct order (using dblink or pg_dump/pg_restore in practice)
-- This is a template - actual import would use:
-- pg_dump -t 'auth_export."AspNetRoles_backup"' GrcMvcDb | psql GrcAuthDb

-- 1. Import RoleProfiles first (Users reference this)
INSERT INTO "RoleProfile" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."RoleProfiles_backup"')
AS t(/* column definitions from RoleProfile */);

-- 2. Import AspNetRoles
INSERT INTO "AspNetRoles" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetRoles_backup"')
AS t("Id" text, "Name" varchar(256), "NormalizedName" varchar(256), "ConcurrencyStamp" text);

-- 3. Import AspNetUsers
INSERT INTO "AspNetUsers" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetUsers_backup"')
AS t(/* column definitions from ApplicationUser - customize for your schema */);

-- 4. Import AspNetRoleClaims
INSERT INTO "AspNetRoleClaims" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetRoleClaims_backup"')
AS t("Id" int, "RoleId" text, "ClaimType" text, "ClaimValue" text);

-- 5. Import AspNetUserClaims
INSERT INTO "AspNetUserClaims" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetUserClaims_backup"')
AS t("Id" int, "UserId" text, "ClaimType" text, "ClaimValue" text);

-- 6. Import AspNetUserLogins
INSERT INTO "AspNetUserLogins" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetUserLogins_backup"')
AS t("LoginProvider" text, "ProviderKey" text, "ProviderDisplayName" text, "UserId" text);

-- 7. Import AspNetUserRoles
INSERT INTO "AspNetUserRoles" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetUserRoles_backup"')
AS t("UserId" text, "RoleId" text);

-- 8. Import AspNetUserTokens
INSERT INTO "AspNetUserTokens" SELECT * FROM dblink('dbname=GrcMvcDb', 'SELECT * FROM auth_export."AspNetUserTokens_backup"')
AS t("UserId" text, "LoginProvider" text, "Name" text, "Value" text);

-- Re-enable foreign key checks
SET session_replication_role = 'origin';

-- Verification counts
SELECT 'AspNetRoles' as table_name, COUNT(*) as row_count FROM "AspNetRoles"
UNION ALL
SELECT 'AspNetUsers', COUNT(*) FROM "AspNetUsers"
UNION ALL
SELECT 'AspNetRoleClaims', COUNT(*) FROM "AspNetRoleClaims"
UNION ALL
SELECT 'AspNetUserClaims', COUNT(*) FROM "AspNetUserClaims"
UNION ALL
SELECT 'AspNetUserLogins', COUNT(*) FROM "AspNetUserLogins"
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM "AspNetUserRoles"
UNION ALL
SELECT 'AspNetUserTokens', COUNT(*) FROM "AspNetUserTokens"
UNION ALL
SELECT 'RoleProfile', COUNT(*) FROM "RoleProfile";
