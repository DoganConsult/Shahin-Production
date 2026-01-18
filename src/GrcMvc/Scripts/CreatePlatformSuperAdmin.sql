-- ========================================
-- PLATFORM SUPER ADMIN SETUP
-- ========================================
-- This script creates the platform super admin account
-- for managing all tenants and system configuration

-- Step 1: Create the platform owner tenant (if not exists)
INSERT INTO "Tenants" (
    "Id", 
    "TenantSlug", 
    "OrganizationName", 
    "TenantCode", 
    "BusinessCode",
    "Industry", 
    "AdminEmail", 
    "Email", 
    "Status", 
    "IsActive",
    "ActivatedAt", 
    "ActivatedBy", 
    "SubscriptionTier", 
    "SubscriptionStartDate",
    "SubscriptionEndDate", 
    "BillingStatus", 
    "IsTrial", 
    "OnboardingStatus",
    "CreatedDate", 
    "CreatedBy", 
    "CorrelationId"
) VALUES (
    '00000000-0000-0000-0000-000000000000', -- Special platform tenant ID
    'admin', 
    'Platform Administration', 
    'ADMIN', 
    'ADMIN-TEN-2026-000000',
    'Technology', 
    'admin@platform.local', 
    'admin@platform.local', 
    'Active', 
    true,
    NOW(), 
    'system', 
    'Enterprise', 
    NOW(),
    NOW() + INTERVAL '100 years', -- Never expires
    'Active', 
    false, 
    'COMPLETED',
    NOW(), 
    'system', 
    gen_random_uuid()::text
) ON CONFLICT ("Id") DO NOTHING;

-- Step 2: Create workspace for platform admin
INSERT INTO "Workspaces" (
    "Id", 
    "TenantId", 
    "Name", 
    "Description", 
    "IsDefault", 
    "IsActive",
    "MaxUsers", 
    "Settings", 
    "CreatedDate", 
    "CreatedBy"
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    '00000000-0000-0000-0000-000000000000',
    'Platform Administration',
    'System administration workspace',
    true,
    true,
    9999, -- Unlimited users
    '{"type": "platform", "access": "full"}',
    NOW(),
    'system'
) ON CONFLICT ("Id") DO NOTHING;

-- Step 3: Create the AspNetUsers entry for super admin
-- NOTE: Password hash is for 'Admin@Platform2026!'
-- You should change this immediately after first login
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount",
    "TenantId",
    "FirstName",
    "LastName",
    "FullName",
    "IsActive",
    "CreatedAt"
) VALUES (
    'superadmin-0000-0000-0000-000000000000',
    'admin@platform.local',
    'ADMIN@PLATFORM.LOCAL',
    'admin@platform.local',
    'ADMIN@PLATFORM.LOCAL',
    true,
    'AQAAAAEAACcQAAAAEGqK8wqYx5JhV6+L9yH3xQzVKQH7zYlHkPQj2B+hGU4nDp3gV7rKfQ2VQHZxJKTkXg==', -- Admin@Platform2026!
    'QWERTYUIOP',
    gen_random_uuid()::text,
    false,
    false,
    false,
    0,
    '00000000-0000-0000-0000-000000000000',
    'Platform',
    'Administrator',
    'Platform Administrator',
    true,
    NOW()
) ON CONFLICT ("Id") DO UPDATE SET
    "PasswordHash" = EXCLUDED."PasswordHash",
    "SecurityStamp" = EXCLUDED."SecurityStamp";

-- Step 4: Create SuperAdmin role if not exists
INSERT INTO "AspNetRoles" (
    "Id",
    "Name",
    "NormalizedName",
    "ConcurrencyStamp"
) VALUES (
    'superadmin-role-0000-0000-000000000000',
    'SuperAdmin',
    'SUPERADMIN',
    gen_random_uuid()::text
) ON CONFLICT ("Id") DO NOTHING;

-- Step 5: Assign SuperAdmin role to the platform admin user
INSERT INTO "AspNetUserRoles" (
    "UserId",
    "RoleId"
) VALUES (
    'superadmin-0000-0000-0000-000000000000',
    'superadmin-role-0000-0000-000000000000'
) ON CONFLICT ("UserId", "RoleId") DO NOTHING;

-- Step 6: Update the tenant with FirstAdminUserId
UPDATE "Tenants" 
SET "FirstAdminUserId" = 'superadmin-0000-0000-0000-000000000000'
WHERE "Id" = '00000000-0000-0000-0000-000000000000';

-- Step 7: Display access information
SELECT '========================================' AS info
UNION ALL
SELECT 'PLATFORM SUPER ADMIN CREATED' AS info
UNION ALL
SELECT '========================================' AS info
UNION ALL
SELECT '' AS info
UNION ALL
SELECT 'Access URL: https://yourdomain.com/admin' AS info
UNION ALL
SELECT 'Username: admin@platform.local' AS info
UNION ALL
SELECT 'Password: Admin@Platform2026!' AS info
UNION ALL
SELECT '' AS info
UNION ALL
SELECT 'IMPORTANT: Change password after first login!' AS info
UNION ALL
SELECT '========================================' AS info;

-- Verify the setup
SELECT 
    t."TenantSlug" AS "Tenant",
    t."OrganizationName" AS "Organization",
    u."Email" AS "Admin Email",
    u."FullName" AS "Admin Name",
    r."Name" AS "Role"
FROM "Tenants" t
LEFT JOIN "AspNetUsers" u ON u."Id" = t."FirstAdminUserId"
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
WHERE t."TenantSlug" = 'admin';
