using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GrcMvc.Models.Entities;

namespace GrcMvc.Data
{
    /// <summary>
    /// Dedicated DbContext for Identity/Authentication data.
    /// Separate from main app database for security isolation.
    ///
    /// Contains:
    /// - ASP.NET Identity tables (AspNetUsers, AspNetRoles, etc.)
    /// - Authentication tokens and sessions
    /// - User profile data (name, email, etc.)
    ///
    /// Does NOT contain:
    /// - Tenant membership (in GrcDbContext)
    /// - Workspace membership (in GrcDbContext)
    /// - App-specific role assignments (in GrcDbContext)
    /// </summary>
    public class GrcAuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public GrcAuthDbContext(DbContextOptions<GrcAuthDbContext> options)
            : base(options)
        {
        }

        // Security audit tables
        public DbSet<PasswordHistory> PasswordHistory { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;
        public DbSet<AuthenticationAuditLog> AuthenticationAuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            // Foreign key relationships
            builder.Entity<PasswordHistory>()
                .HasOne(ph => ph.User)
                .WithMany()
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LoginAttempt>()
                .HasOne(la => la.User)
                .WithMany()
                .HasForeignKey(la => la.UserId)
                .OnDelete(DeleteBehavior.SetNull); // Keep attempts even if user deleted

            builder.Entity<AuthenticationAuditLog>()
                .HasOne(aal => aal.User)
                .WithMany()
                .HasForeignKey(aal => aal.UserId)
                .OnDelete(DeleteBehavior.SetNull); // Keep audit logs even if user deleted

            // Configure Details as JSONB column (PostgreSQL)
            builder.Entity<AuthenticationAuditLog>()
                .Property(aal => aal.Details)
                .HasColumnType("jsonb");
        }
    }
}
