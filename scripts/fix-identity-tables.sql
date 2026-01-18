-- Fix Identity Tables: Drop and Recreate with Complete ApplicationUser Schema
-- This ensures all ApplicationUser properties are present in AspNetUsers table
-- Run this script against GrcAuthDb database

-- Drop existing Identity tables (in correct order to handle foreign keys)
DROP TABLE IF EXISTS "AspNetUserTokens" CASCADE;
DROP TABLE IF EXISTS "AspNetUserLogins" CASCADE;
DROP TABLE IF EXISTS "AspNetUserClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetUserRoles" CASCADE;
DROP TABLE IF EXISTS "AspNetRoleClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetUsers" CASCADE;
DROP TABLE IF EXISTS "AspNetRoles" CASCADE;

-- Create AspNetRoles table
CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

-- Create RoleProfile table (if it doesn't exist)
CREATE TABLE IF NOT EXISTS "RoleProfile" (
    "Id" UUID NOT NULL,
    "RoleCode" TEXT NOT NULL,
    "RoleName" TEXT NOT NULL,
    "Layer" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Scope" TEXT NOT NULL,
    "Responsibilities" TEXT NOT NULL,
    "ApprovalLevel" INTEGER NOT NULL,
    "ApprovalAuthority" NUMERIC,
    "CanEscalate" BOOLEAN NOT NULL,
    "CanApprove" BOOLEAN NOT NULL,
    "CanReject" BOOLEAN NOT NULL,
    "CanReassign" BOOLEAN NOT NULL,
    "ParticipatingWorkflows" TEXT,
    "IsActive" BOOLEAN NOT NULL,
    "TenantId" UUID,
    "DisplayOrder" INTEGER NOT NULL,
    "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ModifiedDate" TIMESTAMP WITH TIME ZONE,
    "CreatedBy" TEXT,
    "ModifiedBy" TEXT,
    "IsDeleted" BOOLEAN NOT NULL,
    "DeletedAt" TIMESTAMP WITH TIME ZONE,
    "RowVersion" BYTEA,
    CONSTRAINT "PK_RoleProfile" PRIMARY KEY ("Id")
);

-- Create AspNetUsers table with ALL ApplicationUser properties
CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL,
    -- Standard Identity properties
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL,
    "TwoFactorEnabled" BOOLEAN NOT NULL,
    "LockoutEnd" TIMESTAMP WITH TIME ZONE,
    "LockoutEnabled" BOOLEAN NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    -- ApplicationUser custom properties
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NOT NULL,
    "Department" TEXT NOT NULL,
    "JobTitle" TEXT NOT NULL,
    "RoleProfileId" UUID,
    "KsaCompetencyLevel" INTEGER NOT NULL DEFAULT 3,
    "KnowledgeAreas" TEXT,
    "Skills" TEXT,
    "Abilities" TEXT,
    "AssignedScope" TEXT,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "LastLoginDate" TIMESTAMP WITH TIME ZONE,
    "RefreshToken" TEXT,
    "RefreshTokenExpiry" TIMESTAMP WITH TIME ZONE,
    "MustChangePassword" BOOLEAN NOT NULL DEFAULT true,
    "LastPasswordChangedAt" TIMESTAMP WITH TIME ZONE,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUsers_RoleProfile_RoleProfileId" FOREIGN KEY ("RoleProfileId") REFERENCES "RoleProfile" ("Id")
);

-- Create remaining Identity tables
CREATE TABLE "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_Email" ON "AspNetUsers" ("Email");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedEmail" ON "AspNetUsers" ("NormalizedEmail");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_RoleProfileId" ON "AspNetUsers" ("RoleProfileId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_IsActive" ON "AspNetUsers" ("IsActive");

-- Verify table structure
SELECT 
    column_name, 
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers' 
ORDER BY ordinal_position;

SELECT 'Identity tables recreated successfully with complete ApplicationUser schema!' AS status;
