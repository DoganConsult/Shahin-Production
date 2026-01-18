-- Alternative SQL Script to Create Production Tenants
-- Run this directly in your PostgreSQL database if the C# script doesn't work

BEGIN;

-- Create 3 production tenants
INSERT INTO "Tenants" (
    "Id", "TenantSlug", "OrganizationName", "TenantCode", "BusinessCode",
    "Industry", "AdminEmail", "Email", "Status", "IsActive",
    "ActivatedAt", "ActivatedBy", "SubscriptionTier", "SubscriptionStartDate",
    "SubscriptionEndDate", "BillingStatus", "IsTrial", "OnboardingStatus",
    "CreatedDate", "CreatedBy", "CorrelationId", "FirstAdminUserId"
) VALUES 
-- Tenant 1: Al Rajhi Banking Corporation
(
    '11111111-1111-1111-1111-111111111111', 
    'alrajhi', 
    'Al Rajhi Banking Corporation', 
    'ARBC', 
    'ARBC-TEN-2026-000001',
    'Financial Services', 
    'admin@alrajhi.com.sa', 
    'admin@alrajhi.com.sa', 
    'Active', 
    true,
    NOW(), 
    'system', 
    'Enterprise', 
    NOW(),
    NOW() + INTERVAL '1 year', 
    'Active', 
    false, 
    'NOT_STARTED',
    NOW(), 
    'system', 
    gen_random_uuid()::text,
    NULL -- Will be updated after user creation
),
-- Tenant 2: National Cybersecurity Authority
(
    '22222222-2222-2222-2222-222222222222', 
    'nca', 
    'National Cybersecurity Authority', 
    'NCA', 
    'NCA-TEN-2026-000002',
    'Government', 
    'admin@nca.gov.sa', 
    'admin@nca.gov.sa', 
    'Active', 
    true,
    NOW(), 
    'system', 
    'Enterprise', 
    NOW(),
    NOW() + INTERVAL '1 year', 
    'Active', 
    false, 
    'NOT_STARTED',
    NOW(), 
    'system', 
    gen_random_uuid()::text,
    NULL
),
-- Tenant 3: King Faisal Specialist Hospital
(
    '33333333-3333-3333-3333-333333333333', 
    'kfsh', 
    'King Faisal Specialist Hospital', 
    'KFSH', 
    'KFSH-TEN-2026-000003',
    'Healthcare', 
    'admin@kfsh.med.sa', 
    'admin@kfsh.med.sa', 
    'Active', 
    true,
    NOW(), 
    'system', 
    'Professional', 
    NOW(),
    NOW() + INTERVAL '1 year', 
    'Active', 
    false, 
    'NOT_STARTED',
    NOW(), 
    'system', 
    gen_random_uuid()::text,
    NULL
);

-- Create workspaces for each tenant
INSERT INTO "Workspaces" (
    "Id", "TenantId", "Name", "Description", "IsDefault", "IsActive",
    "MaxUsers", "Settings", "CreatedDate", "CreatedBy"
) VALUES
(
    'a1111111-1111-1111-1111-111111111111',
    '11111111-1111-1111-1111-111111111111',
    'Al Rajhi Banking Corporation Primary',
    'Primary workspace for Al Rajhi Banking Corporation',
    true,
    true,
    500,
    '{"industry": "Financial Services", "country": "Saudi Arabia", "billingCycle": "Annual"}',
    NOW(),
    'system'
),
(
    'a2222222-2222-2222-2222-222222222222',
    '22222222-2222-2222-2222-222222222222',
    'National Cybersecurity Authority Primary',
    'Primary workspace for National Cybersecurity Authority',
    true,
    true,
    200,
    '{"industry": "Government", "country": "Saudi Arabia", "billingCycle": "Annual"}',
    NOW(),
    'system'
),
(
    'a3333333-3333-3333-3333-333333333333',
    '33333333-3333-3333-3333-333333333333',
    'King Faisal Specialist Hospital Primary',
    'Primary workspace for King Faisal Specialist Hospital',
    true,
    true,
    150,
    '{"industry": "Healthcare", "country": "Saudi Arabia", "billingCycle": "Monthly"}',
    NOW(),
    'system'
);

-- Create organization profiles
INSERT INTO "OrganizationProfiles" (
    "Id", "TenantId", "WorkspaceId", "CompanyName", "CompanyWebsite",
    "CompanySize", "Industry", "SubIndustry", "Country", "State", "City",
    "Address", "PrimaryContactName", "PrimaryContactEmail", "PrimaryContactPhone",
    "BusinessRegistrationNumber", "TaxId", "OnboardingStatus", "CreatedDate", "CreatedBy"
) VALUES
(
    'b1111111-1111-1111-1111-111111111111',
    '11111111-1111-1111-1111-111111111111',
    'a1111111-1111-1111-1111-111111111111',
    'Al Rajhi Banking Corporation',
    'https://www.alrajhibank.com.sa',
    '10000+',
    'Financial Services',
    'Banking',
    'Saudi Arabia',
    'Riyadh Region',
    'Riyadh',
    'King Fahd Road, Riyadh 11411',
    'Mohammed Al-Rashid',
    'admin@alrajhi.com.sa',
    '+966501234567',
    '300012345600003',
    '300012345600003',
    'NOT_STARTED',
    NOW(),
    'system'
),
(
    'b2222222-2222-2222-2222-222222222222',
    '22222222-2222-2222-2222-222222222222',
    'a2222222-2222-2222-2222-222222222222',
    'National Cybersecurity Authority',
    'https://nca.gov.sa',
    '1000-5000',
    'Government',
    'Federal Agency',
    'Saudi Arabia',
    'Riyadh Region',
    'Riyadh',
    'Digital City, Riyadh',
    'Abdullah Al-Swaha',
    'admin@nca.gov.sa',
    '+966512345678',
    '300098765400001',
    '300098765400001',
    'NOT_STARTED',
    NOW(),
    'system'
),
(
    'b3333333-3333-3333-3333-333333333333',
    '33333333-3333-3333-3333-333333333333',
    'a3333333-3333-3333-3333-333333333333',
    'King Faisal Specialist Hospital',
    'https://www.kfsh.med.sa',
    '5000-10000',
    'Healthcare',
    'Hospital & Medical Center',
    'Saudi Arabia',
    'Riyadh Region',
    'Riyadh',
    'Zahrawi Street, Riyadh 11211',
    'Dr. Fatima Al-Zahrani',
    'admin@kfsh.med.sa',
    '+966503456789',
    '300054321600002',
    '300054321600002',
    'NOT_STARTED',
    NOW(),
    'system'
);

COMMIT;

-- Display created tenants
SELECT 
    "TenantSlug" as "Slug",
    "OrganizationName" as "Organization",
    "TenantCode" as "Code",
    "Industry",
    "SubscriptionTier" as "Tier",
    "AdminEmail" as "Admin Email"
FROM "Tenants"
WHERE "TenantSlug" IN ('alrajhi', 'nca', 'kfsh');

-- IMPORTANT: Admin users must be created through the application
-- Use the following credentials for each tenant:
-- 
-- TENANT 1: Al Rajhi Banking Corporation
-- =====================================
-- URL: https://yourdomain.com/alrajhi
-- Email: admin@alrajhi.com.sa
-- Temporary Password: Admin@2026!ARBC
-- 
-- TENANT 2: National Cybersecurity Authority
-- =====================================
-- URL: https://yourdomain.com/nca
-- Email: admin@nca.gov.sa
-- Temporary Password: Admin@2026!NCA
-- 
-- TENANT 3: King Faisal Specialist Hospital
-- =====================================
-- URL: https://yourdomain.com/kfsh
-- Email: admin@kfsh.med.sa
-- Temporary Password: Admin@2026!KFSH
