using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrcMvc.Migrations.Auth
{
    /// <inheritdoc />
    public partial class CreateMissingIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create all missing Identity tables if they don't exist
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
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
