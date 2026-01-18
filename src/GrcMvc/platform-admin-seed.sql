-- Create platform admin user in PostgreSQL
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
    "FirstName", "LastName", "IsActive", "CreatedAt", "MustChangePassword"
) VALUES (
    'platform-admin-001', 'admin@platform.local', 'ADMIN@PLATFORM.LOCAL',
    'admin@platform.local', 'ADMIN@PLATFORM.LOCAL', true,
    'AQAAAAEAACcQAAAAEGqK8wqYx5JhV6+L9yH3xQzVKQH7zYlHkPQj2B+hGU4nDp3gV7rKfQ2VQHZxJKTkXg==',
    'SECURITYSTAMP', '00000000-0000-0000-0000-000000000000',
    false, false, false, 0,
    'Platform', 'Administrator', true, NOW(), false
) ON CONFLICT ("Id") DO NOTHING;

-- Create roles
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES 
('platform-admin-role', 'PlatformAdmin', 'PLATFORMADMIN', '00000000-0000-0000-0000-000000000000'),
('super-admin-role', 'SuperAdmin', 'SUPERADMIN', '00000000-0000-0000-0000-000000000001')
ON CONFLICT ("Id") DO NOTHING;

-- Assign roles
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES 
('platform-admin-001', 'platform-admin-role'),
('platform-admin-001', 'super-admin-role')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;
