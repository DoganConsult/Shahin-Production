using GrcMvc.Models.Entities;
using GrcMvc.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrcMvc.Data
{
    /// <summary>
    /// Entity Framework configuration for Access Management entities.
    /// Apply these configurations in ApplicationDbContext.OnModelCreating.
    /// </summary>
    public static class AccessManagementDbConfiguration
    {
        /// <summary>
        /// Configure all Access Management entities.
        /// </summary>
        public static void ConfigureAccessManagementEntities(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccessManagementAuditEventConfiguration());
            modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
            modelBuilder.ApplyConfiguration(new ApiKeyConfiguration());
            modelBuilder.ApplyConfiguration(new AccessReviewConfiguration());
            modelBuilder.ApplyConfiguration(new AccessReviewItemConfiguration());
            modelBuilder.ApplyConfiguration(new SoDRuleConfiguration());
            modelBuilder.ApplyConfiguration(new SoDActionRecordConfiguration());
        }
    }

    /// <summary>
    /// AM-10: Audit Event entity configuration.
    /// </summary>
    public class AccessManagementAuditEventConfiguration : IEntityTypeConfiguration<AccessManagementAuditEvent>
    {
        public void Configure(EntityTypeBuilder<AccessManagementAuditEvent> builder)
        {
            builder.ToTable("AccessManagementAuditEvents");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.EventId).IsUnique();
            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.ActorUserId);
            builder.HasIndex(e => e.TargetUserId);
            builder.HasIndex(e => e.EventType);
            builder.HasIndex(e => e.ControlNumber);
            builder.HasIndex(e => e.Timestamp);
            builder.HasIndex(e => new { e.TenantId, e.Timestamp });

            // Configure JSON column
            builder.Property(e => e.DetailsJson)
                .HasColumnType("jsonb");
        }
    }

    /// <summary>
    /// AM-08: Password Reset Token entity configuration.
    /// </summary>
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.TokenHash);
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Email);
            builder.HasIndex(e => e.ExpiresAt);
            builder.HasIndex(e => new { e.UserId, e.IsUsed, e.IsRevoked });
        }
    }

    /// <summary>
    /// AM-02: API Key entity configuration.
    /// </summary>
    public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
    {
        public void Configure(EntityTypeBuilder<ApiKey> builder)
        {
            builder.ToTable("ApiKeys");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.KeyHash).IsUnique();
            builder.HasIndex(e => e.KeyPrefix);
            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.CreatedBy);
            builder.HasIndex(e => new { e.TenantId, e.RevokedAt });

            // Configure JSON columns
            builder.Property(e => e.AllowedDomainsJson)
                .HasColumnType("jsonb");
            builder.Property(e => e.AllowedIpsJson)
                .HasColumnType("jsonb");
            builder.Property(e => e.ScopesJson)
                .HasColumnType("jsonb");
        }
    }

    /// <summary>
    /// AM-11: Access Review entity configuration.
    /// </summary>
    public class AccessReviewConfiguration : IEntityTypeConfiguration<AccessReview>
    {
        public void Configure(EntityTypeBuilder<AccessReview> builder)
        {
            builder.ToTable("AccessReviews");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.DueDate);
            builder.HasIndex(e => new { e.TenantId, e.Status });

            builder.HasMany(e => e.Items)
                .WithOne(i => i.Review)
                .HasForeignKey(i => i.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// AM-11: Access Review Item entity configuration.
    /// </summary>
    public class AccessReviewItemConfiguration : IEntityTypeConfiguration<AccessReviewItem>
    {
        public void Configure(EntityTypeBuilder<AccessReviewItem> builder)
        {
            builder.ToTable("AccessReviewItems");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.ReviewId);
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Decision);
            builder.HasIndex(e => new { e.ReviewId, e.Decision });
        }
    }

    /// <summary>
    /// AM-12: SoD Rule entity configuration.
    /// </summary>
    public class SoDRuleConfiguration : IEntityTypeConfiguration<SoDRule>
    {
        public void Configure(EntityTypeBuilder<SoDRule> builder)
        {
            builder.ToTable("SoDRules");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.Action1);
            builder.HasIndex(e => e.Action2);
            builder.HasIndex(e => e.IsEnabled);
        }
    }

    /// <summary>
    /// AM-12: SoD Action Record entity configuration.
    /// </summary>
    public class SoDActionRecordConfiguration : IEntityTypeConfiguration<SoDActionRecord>
    {
        public void Configure(EntityTypeBuilder<SoDActionRecord> builder)
        {
            builder.ToTable("SoDActionRecords");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.TenantId);
            builder.HasIndex(e => e.Action);
            builder.HasIndex(e => e.EntityId);
            builder.HasIndex(e => e.PerformedAt);
            builder.HasIndex(e => new { e.UserId, e.TenantId, e.Action, e.EntityId });
        }
    }
}
