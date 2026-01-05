using System;
using System.Collections.Generic;
using GrcMvc.Models.Entities;

namespace GrcMvc.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string TitleName { get; set; } = string.Empty;
        public int PendingTasksCount { get; set; }

        public List<UserProfileInfo> AssignedProfiles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<string> WorkflowRoles { get; set; } = new();
        public NotificationPreferencesInfo? NotificationPreferences { get; set; }
    }

    public class UserProfileInfo
    {
        public Guid ProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class NotificationPreferencesInfo
    {
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = false;
        public bool InAppEnabled { get; set; } = true;
        public string DigestFrequency { get; set; } = "Immediate";
    }
}
