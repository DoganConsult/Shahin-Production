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
        }
    }
}
