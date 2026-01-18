using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using GrcMvc.Models.Entities;

namespace GrcMvc.Data
{
    /// <summary>
    /// Dedicated DbContext for Identity/Authentication data.
    /// Now uses ABP Identity framework with ApplicationUser extending ABP IdentityUser.
    /// 
    /// Migration completed: ApplicationUser now inherits from ABP Identity.
    /// This enables full ABP Identity service integration (IIdentityUserAppService, etc.)
    /// while maintaining separate database for security isolation.
    ///
    /// Contains:
    /// - ABP Identity tables (AbpUsers, AbpRoles, etc.) 
    /// - Authentication tokens and sessions
    /// - User profile data and custom properties
    ///
    /// Does NOT contain:
    /// - Tenant membership (in GrcDbContext)
    /// - Workspace membership (in GrcDbContext)
    /// - App-specific role assignments (in GrcDbContext)
    /// </summary>
    public class GrcAuthDbContext : AbpDbContext<GrcAuthDbContext>
    {
        public GrcAuthDbContext(DbContextOptions<GrcAuthDbContext> options)
            : base(options)
        {
        }

        // ABP Identity tables are automatically configured by ConfigureIdentity()
        // No need to manually define DbSets for ABP entities
        
        // Custom security audit tables
        public DbSet<PasswordHistory> PasswordHistory { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
        public DbSet<AuthenticationAuditLog> AuthenticationAuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ABP Identity tables (for ApplicationUser extending ABP Identity)
        builder.ConfigureIdentity();

        // Customize Identity table names if needed (optional)
        // builder.Entity<ApplicationUser>().ToTable("Users");
        // builder.Entity<IdentityRole>().ToTable("Roles");

            // Index on email for faster lookups
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique(false);

            // Index on normalized email (used by Identity)
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.NormalizedEmail);

            // Index on IsActive for filtering
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.IsActive);

            // PasswordHistory indexes
            builder.Entity<PasswordHistory>()
                .HasIndex(ph => ph.UserId);

            builder.Entity<PasswordHistory>()
                .HasIndex(ph => ph.ChangedAt);

            // RefreshToken indexes
            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => new { rt.UserId, rt.RevokedAt, rt.ExpiresAt });

            // LoginAttempt indexes
            builder.Entity<LoginAttempt>()
                .HasIndex(la => la.UserId);

            builder.Entity<LoginAttempt>()
                .HasIndex(la => la.IpAddress);

            builder.Entity<LoginAttempt>()
                .HasIndex(la => la.Timestamp);

            builder.Entity<LoginAttempt>()
                .HasIndex(la => new { la.AttemptedEmail, la.Timestamp });

            // AuthenticationAuditLog indexes
            builder.Entity<AuthenticationAuditLog>()
                .HasIndex(aal => aal.UserId);

            builder.Entity<AuthenticationAuditLog>()
                .HasIndex(aal => aal.EventType);

            builder.Entity<AuthenticationAuditLog>()
                .HasIndex(aal => aal.Timestamp);

            builder.Entity<AuthenticationAuditLog>()
                .HasIndex(aal => aal.CorrelationId);

            // Foreign key relationships - Temporarily disabled due to ID type mismatch
            // ApplicationUser now uses Guid ID (ABP Identity) but audit entities use string UserId
            // These will be re-enabled after audit entities are migrated to Guid UserId in future deployment
            
            // TODO: Re-enable foreign key relationships after migrating audit entities to Guid UserId:
            // builder.Entity<PasswordHistory>()
            //     .HasOne(ph => ph.User)
            //     .WithMany()
            //     .HasForeignKey(ph => ph.UserId)
            //     .OnDelete(DeleteBehavior.Cascade);
            // (same for RefreshToken, LoginAttempt, AuthenticationAuditLog)

            // Configure Details as JSONB column (PostgreSQL)
            builder.Entity<AuthenticationAuditLog>()
                .Property(aal => aal.Details)
                .HasColumnType("jsonb");
        }
    }
}
