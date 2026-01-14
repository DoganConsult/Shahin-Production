-- Migration: Add Trial and Collaboration Entities
-- Date: 2026-01-14

CREATE TABLE IF NOT EXISTS "TrialSignups" (
    "Id" uuid PRIMARY KEY,
    "Email" varchar(255) NOT NULL,
    "FirstName" varchar(100),
    "LastName" varchar(100),
    "CompanyName" varchar(255),
    "Phone" varchar(50),
    "Sector" varchar(100),
    "Source" varchar(50) DEFAULT 'website',
    "ReferralCode" varchar(50),
    "ActivationToken" varchar(255),
    "Status" varchar(50) DEFAULT 'pending',
    "TenantId" uuid,
    "ActivatedAt" timestamp,
    "ProvisionedAt" timestamp,
    "ExpiredAt" timestamp,
    "Notes" varchar(500),
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "TrialExtensions" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "DaysAdded" int NOT NULL,
    "Reason" varchar(500),
    "PreviousEndDate" timestamp NOT NULL,
    "NewEndDate" timestamp NOT NULL,
    "ApprovedBy" varchar(100),
    "ApprovalMethod" varchar(100),
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "TrialEmailLogs" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "EmailType" varchar(50) NOT NULL,
    "SentTo" varchar(255) NOT NULL,
    "SentAt" timestamp NOT NULL,
    "Status" varchar(50),
    "OpenedAt" timestamp,
    "ClickedAt" timestamp,
    "ErrorMessage" varchar(500),
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "EcosystemPartners" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(255) NOT NULL,
    "Type" varchar(50) NOT NULL,
    "Sector" varchar(100),
    "Description" varchar(1000),
    "ServicesJson" text,
    "CertificationsJson" text,
    "ContactEmail" varchar(255),
    "Website" varchar(255),
    "LogoUrl" varchar(255),
    "Rating" double precision DEFAULT 0,
    "ReviewCount" int DEFAULT 0,
    "ConnectionsCount" int DEFAULT 0,
    "IsVerified" boolean DEFAULT false,
    "IsActive" boolean DEFAULT true,
    "Country" varchar(50),
    "City" varchar(100),
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "EcosystemConnections" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "PartnerType" varchar(50),
    "PartnerId" uuid,
    "PartnerEmail" varchar(255),
    "PartnerName" varchar(255),
    "Purpose" varchar(500),
    "SharedDataTypesJson" text,
    "ExpiresAt" timestamp,
    "Status" varchar(50),
    "RequestedAt" timestamp,
    "ApprovedAt" timestamp,
    "RejectedAt" timestamp,
    "RejectionReason" varchar(500),
    "InteractionsCount" int DEFAULT 0,
    "LastInteractionAt" timestamp,
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "TeamActivities" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TargetUserId" uuid,
    "ActivityType" varchar(100) NOT NULL,
    "Description" varchar(500),
    "MetadataJson" text,
    "Module" varchar(50),
    "Action" varchar(50),
    "ResourceId" uuid,
    "ResourceType" varchar(100),
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "CollaborationWorkspaces" (
    "Id" uuid PRIMARY KEY,
    "TenantId" uuid NOT NULL,
    "Name" varchar(255) NOT NULL,
    "Description" varchar(1000),
    "Type" varchar(50) DEFAULT 'general',
    "FrameworkId" uuid,
    "ProjectId" uuid,
    "MembersJson" text,
    "SettingsJson" text,
    "IsActive" boolean DEFAULT true,
    "ArchivedAt" timestamp,
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "CollaborationItems" (
    "Id" uuid PRIMARY KEY,
    "WorkspaceId" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "ItemType" varchar(50) NOT NULL,
    "Title" varchar(500) NOT NULL,
    "Content" text,
    "Status" varchar(50),
    "Priority" varchar(50),
    "AssignedTo" uuid,
    "DueDate" timestamp,
    "TagsJson" text,
    "AttachmentsJson" text,
    "CommentsCount" int DEFAULT 0,
    "ReactionsCount" int DEFAULT 0,
    "ParentItemId" uuid,
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

CREATE TABLE IF NOT EXISTS "CollaborationComments" (
    "Id" uuid PRIMARY KEY,
    "ItemId" uuid NOT NULL,
    "TenantId" uuid NOT NULL,
    "AuthorId" uuid NOT NULL,
    "Content" text NOT NULL,
    "MentionsJson" text,
    "AttachmentsJson" text,
    "ParentCommentId" uuid,
    "IsEdited" boolean DEFAULT false,
    "EditedAt" timestamp,
    "CreatedDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ModifiedDate" timestamp,
    "CreatedBy" varchar(255),
    "ModifiedBy" varchar(255),
    "DeletedAt" timestamp
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_TrialSignups_Email" ON "TrialSignups" ("Email");
CREATE INDEX IF NOT EXISTS "IX_TrialSignups_Status" ON "TrialSignups" ("Status");
CREATE INDEX IF NOT EXISTS "IX_TrialExtensions_TenantId" ON "TrialExtensions" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_TrialEmailLogs_TenantId" ON "TrialEmailLogs" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_EcosystemPartners_Type" ON "EcosystemPartners" ("Type");
CREATE INDEX IF NOT EXISTS "IX_EcosystemConnections_TenantId" ON "EcosystemConnections" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_TeamActivities_TenantId" ON "TeamActivities" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CollaborationWorkspaces_TenantId" ON "CollaborationWorkspaces" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CollaborationItems_WorkspaceId" ON "CollaborationItems" ("WorkspaceId");
CREATE INDEX IF NOT EXISTS "IX_CollaborationComments_ItemId" ON "CollaborationComments" ("ItemId");
