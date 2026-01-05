using Microsoft.AspNetCore.Identity;
using System;

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
    }
}