using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrcMvc.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Assigned role profile (for scope-based filtering)
        /// </summary>
        public Guid? RoleProfileId { get; set; }
        public RoleProfile? RoleProfile { get; set; }

        /// <summary>
        /// KSA Competency Level (1-5, where 1=Novice, 5=Expert)
        /// </summary>
        public int KsaCompetencyLevel { get; set; } = 3;

        /// <summary>
        /// Knowledge areas (JSON array)
        /// </summary>
        public string? KnowledgeAreas { get; set; }

        /// <summary>
        /// Skills (JSON array)
        /// </summary>
        public string? Skills { get; set; }

        /// <summary>
        /// Abilities (JSON array)
        /// </summary>
        public string? Abilities { get; set; }

        /// <summary>
        /// User's assigned scope (inherited from RoleProfile)
        /// </summary>
        public string? AssignedScope { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        /// <summary>
        /// Force user to change password on next login (first login or admin reset)
        /// </summary>
        public bool MustChangePassword { get; set; } = true;

        /// <summary>
        /// Last password change date
        /// </summary>
        public DateTime? LastPasswordChangedAt { get; set; }

        /// <summary>
        /// MEDIUM PRIORITY FIX: Check if password has expired (90 days for GRC compliance)
        /// </summary>
        public bool IsPasswordExpired(int maxAgeDays = 90)
        {
            if (!LastPasswordChangedAt.HasValue) return true; // Never changed, consider expired
            return (DateTime.UtcNow - LastPasswordChangedAt.Value).TotalDays > maxAgeDays;
        }

        /// <summary>
        /// Days until password expires
        /// </summary>
        public int? DaysUntilPasswordExpires(int maxAgeDays = 90)
        {
            if (!LastPasswordChangedAt.HasValue) return null;
            var daysSinceChange = (DateTime.UtcNow - LastPasswordChangedAt.Value).TotalDays;
            return Math.Max(0, (int)(maxAgeDays - daysSinceChange));
        }

        // Alias properties for backward compatibility
        [NotMapped]
        public DateTime CreatedAt
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }

        [NotMapped]
        public DateTime? LastLoginAt
        {
            get => LastLoginDate;
            set => LastLoginDate = value;
        }
    }
}