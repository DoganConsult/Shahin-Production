using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrcMvc.Migrations.Auth
{
    /// <inheritdoc />
    public partial class AddApplicationUserCustomColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create AspNetUsers table if it doesn't exist, then add missing ApplicationUser custom columns
            // Using raw SQL to check and create/add columns conditionally (PostgreSQL IF NOT EXISTS)
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Create AspNetRoles table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetRoles') THEN
                        CREATE TABLE ""AspNetRoles"" (
                            ""Id"" TEXT NOT NULL,
                            ""Name"" VARCHAR(256),
                            ""NormalizedName"" VARCHAR(256),
                            ""ConcurrencyStamp"" TEXT,
                            CONSTRAINT ""PK_AspNetRoles"" PRIMARY KEY (""Id"")
                        );
                        CREATE UNIQUE INDEX IF NOT EXISTS ""RoleNameIndex"" ON ""AspNetRoles"" (""NormalizedName"");
                    END IF;
                    
                    -- Create AspNetRoleClaims table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetRoleClaims') THEN
                        CREATE TABLE ""AspNetRoleClaims"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""RoleId"" TEXT NOT NULL,
                            ""ClaimType"" TEXT,
                            ""ClaimValue"" TEXT,
                            CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_AspNetRoleClaims_RoleId"" ON ""AspNetRoleClaims"" (""RoleId"");
                    END IF;
                    
                    -- Create AspNetUsers table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUsers') THEN
                        CREATE TABLE ""AspNetUsers"" (
                            ""Id"" TEXT NOT NULL,
                            ""UserName"" VARCHAR(256),
                            ""NormalizedUserName"" VARCHAR(256),
                            ""Email"" VARCHAR(256),
                            ""NormalizedEmail"" VARCHAR(256),
                            ""EmailConfirmed"" BOOLEAN NOT NULL DEFAULT false,
                            ""PasswordHash"" TEXT,
                            ""SecurityStamp"" TEXT,
                            ""ConcurrencyStamp"" TEXT,
                            ""PhoneNumber"" TEXT,
                            ""PhoneNumberConfirmed"" BOOLEAN NOT NULL DEFAULT false,
                            ""TwoFactorEnabled"" BOOLEAN NOT NULL DEFAULT false,
                            ""LockoutEnd"" TIMESTAMP WITH TIME ZONE,
                            ""LockoutEnabled"" BOOLEAN NOT NULL DEFAULT false,
                            ""AccessFailedCount"" INTEGER NOT NULL DEFAULT 0,
                            CONSTRAINT ""PK_AspNetUsers"" PRIMARY KEY (""Id"")
                        );
                        
                        -- Create standard Identity indexes
                        CREATE INDEX IF NOT EXISTS ""EmailIndex"" ON ""AspNetUsers"" (""NormalizedEmail"");
                        CREATE UNIQUE INDEX IF NOT EXISTS ""UserNameIndex"" ON ""AspNetUsers"" (""NormalizedUserName"") WHERE ""NormalizedUserName"" IS NOT NULL;
                    END IF;
                    
                    -- Create AspNetUserClaims table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUserClaims') THEN
                        CREATE TABLE ""AspNetUserClaims"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""UserId"" TEXT NOT NULL,
                            ""ClaimType"" TEXT,
                            ""ClaimValue"" TEXT,
                            CONSTRAINT ""FK_AspNetUserClaims_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_AspNetUserClaims_UserId"" ON ""AspNetUserClaims"" (""UserId"");
                    END IF;
                    
                    -- Create AspNetUserLogins table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUserLogins') THEN
                        CREATE TABLE ""AspNetUserLogins"" (
                            ""LoginProvider"" TEXT NOT NULL,
                            ""ProviderKey"" TEXT NOT NULL,
                            ""ProviderDisplayName"" TEXT,
                            ""UserId"" TEXT NOT NULL,
                            CONSTRAINT ""PK_AspNetUserLogins"" PRIMARY KEY (""LoginProvider"", ""ProviderKey""),
                            CONSTRAINT ""FK_AspNetUserLogins_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_AspNetUserLogins_UserId"" ON ""AspNetUserLogins"" (""UserId"");
                    END IF;
                    
                    -- Create AspNetUserRoles table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUserRoles') THEN
                        CREATE TABLE ""AspNetUserRoles"" (
                            ""UserId"" TEXT NOT NULL,
                            ""RoleId"" TEXT NOT NULL,
                            CONSTRAINT ""PK_AspNetUserRoles"" PRIMARY KEY (""UserId"", ""RoleId""),
                            CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_AspNetUserRoles_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_AspNetUserRoles_RoleId"" ON ""AspNetUserRoles"" (""RoleId"");
                    END IF;
                    
                    -- Create AspNetUserTokens table if it doesn't exist
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUserTokens') THEN
                        CREATE TABLE ""AspNetUserTokens"" (
                            ""UserId"" TEXT NOT NULL,
                            ""LoginProvider"" TEXT NOT NULL,
                            ""Name"" TEXT NOT NULL,
                            ""Value"" TEXT,
                            CONSTRAINT ""PK_AspNetUserTokens"" PRIMARY KEY (""UserId"", ""LoginProvider"", ""Name""),
                            CONSTRAINT ""FK_AspNetUserTokens_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
                        );
                    END IF;
                    
                    -- Add FirstName if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'FirstName') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""FirstName"" TEXT NOT NULL DEFAULT '';
                    END IF;
                    
                    -- Add LastName if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'LastName') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""LastName"" TEXT NOT NULL DEFAULT '';
                    END IF;
                    
                    -- Add Department if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'Department') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""Department"" TEXT NOT NULL DEFAULT '';
                    END IF;
                    
                    -- Add JobTitle if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'JobTitle') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""JobTitle"" TEXT NOT NULL DEFAULT '';
                    END IF;
                    
                    -- Add RoleProfileId if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'RoleProfileId') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""RoleProfileId"" UUID NULL;
                    END IF;
                    
                    -- Add KsaCompetencyLevel if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'KsaCompetencyLevel') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""KsaCompetencyLevel"" INTEGER NOT NULL DEFAULT 3;
                    END IF;
                    
                    -- Add KnowledgeAreas if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'KnowledgeAreas') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""KnowledgeAreas"" TEXT NULL;
                    END IF;
                    
                    -- Add Skills if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'Skills') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""Skills"" TEXT NULL;
                    END IF;
                    
                    -- Add Abilities if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'Abilities') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""Abilities"" TEXT NULL;
                    END IF;
                    
                    -- Add AssignedScope if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'AssignedScope') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""AssignedScope"" TEXT NULL;
                    END IF;
                    
                    -- Add IsActive if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'IsActive') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""IsActive"" BOOLEAN NOT NULL DEFAULT true;
                    END IF;
                    
                    -- Add CreatedDate if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'CreatedDate') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""CreatedDate"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW();
                    END IF;
                    
                    -- Add LastLoginDate if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'LastLoginDate') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""LastLoginDate"" TIMESTAMP WITH TIME ZONE NULL;
                    END IF;
                    
                    -- Add RefreshToken if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'RefreshToken') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""RefreshToken"" TEXT NULL;
                    END IF;
                    
                    -- Add RefreshTokenExpiry if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'RefreshTokenExpiry') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""RefreshTokenExpiry"" TIMESTAMP WITH TIME ZONE NULL;
                    END IF;
                    
                    -- Add MustChangePassword if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'MustChangePassword') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""MustChangePassword"" BOOLEAN NOT NULL DEFAULT true;
                    END IF;
                    
                    -- Add LastPasswordChangedAt if not exists
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name = 'AspNetUsers' AND column_name = 'LastPasswordChangedAt') THEN
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""LastPasswordChangedAt"" TIMESTAMP WITH TIME ZONE NULL;
                    END IF;
                END $$;
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AspNetUsers_Email"" ON ""AspNetUsers"" (""Email"");
                CREATE INDEX IF NOT EXISTS ""IX_AspNetUsers_IsActive"" ON ""AspNetUsers"" (""IsActive"");
            ");

            // Add foreign key for RoleProfileId if RoleProfile table exists and FK doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'RoleProfile')
                       AND NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                                       WHERE constraint_name = 'FK_AspNetUsers_RoleProfile_RoleProfileId') THEN
                        ALTER TABLE ""AspNetUsers""
                        ADD CONSTRAINT ""FK_AspNetUsers_RoleProfile_RoleProfileId""
                        FOREIGN KEY (""RoleProfileId"") REFERENCES ""RoleProfile"" (""Id"");
                    END IF;
                END $$;
            ");

            // Add index for RoleProfileId if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_AspNetUsers_RoleProfileId"" ON ""AspNetUsers"" (""RoleProfileId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key constraint if it exists
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
                               WHERE constraint_name = 'FK_AspNetUsers_RoleProfile_RoleProfileId') THEN
                        ALTER TABLE ""AspNetUsers""
                        DROP CONSTRAINT ""FK_AspNetUsers_RoleProfile_RoleProfileId"";
                    END IF;
                END $$;
            ");

            // Drop indexes if they exist
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_AspNetUsers_Email"";
                DROP INDEX IF EXISTS ""IX_AspNetUsers_IsActive"";
                DROP INDEX IF EXISTS ""IX_AspNetUsers_RoleProfileId"";
            ");

            // Remove ApplicationUser custom columns
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Remove columns if they exist
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""FirstName"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""LastName"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""Department"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""JobTitle"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""RoleProfileId"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""KsaCompetencyLevel"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""KnowledgeAreas"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""Skills"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""Abilities"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""AssignedScope"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""IsActive"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""CreatedDate"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""LastLoginDate"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""RefreshToken"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""RefreshTokenExpiry"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""MustChangePassword"";
                    ALTER TABLE ""AspNetUsers"" DROP COLUMN IF EXISTS ""LastPasswordChangedAt"";
                END $$;
            ");
        }
    }
}
