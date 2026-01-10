using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    /// <summary>
    /// Valid risk statuses for state transitions
    /// </summary>
    public static class RiskStatusTransitions
    {
        public static readonly string[] ValidStatuses = { "Identified", "Active", "Under Review", "Accepted", "Mitigated", "Closed" };
        
        public static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            { "Identified", new[] { "Active", "Under Review", "Closed" } },
            { "Active", new[] { "Under Review", "Accepted", "Mitigated", "Closed" } },
            { "Under Review", new[] { "Active", "Accepted", "Mitigated", "Closed" } },
            { "Accepted", new[] { "Active", "Mitigated", "Closed" } },
            { "Mitigated", new[] { "Active", "Closed" } },
            { "Closed", new[] { "Active" } } // Reopening
        };

        public static bool IsValidTransition(string fromStatus, string toStatus)
        {
            if (string.IsNullOrEmpty(fromStatus)) return ValidStatuses.Contains(toStatus);
            if (!AllowedTransitions.TryGetValue(fromStatus, out var allowed)) return false;
            return allowed.Contains(toStatus);
        }
    }

    /// <summary>
    /// Validator for CreateRiskDto
    /// </summary>
    public class CreateRiskDtoValidator : AbstractValidator<CreateRiskDto>
    {
        public CreateRiskDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المخاطرة مطلوب | Risk name is required")
                .MaximumLength(200).WithMessage("اسم المخاطرة لا يمكن أن يتجاوز 200 حرف | Risk name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف المخاطرة مطلوب | Risk description is required")
                .MaximumLength(2000).WithMessage("وصف المخاطرة لا يمكن أن يتجاوز 2000 حرف | Risk description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("فئة المخاطرة مطلوبة | Risk category is required")
                .MaximumLength(100).WithMessage("فئة المخاطرة لا يمكن أن تتجاوز 100 حرف | Risk category cannot exceed 100 characters");

            RuleFor(x => x.Probability)
                .InclusiveBetween(1, 5).WithMessage("الاحتمالية يجب أن تكون بين 1 و 5 | Probability must be between 1 and 5");

            RuleFor(x => x.Impact)
                .InclusiveBetween(1, 5).WithMessage("التأثير يجب أن يكون بين 1 و 5 | Impact must be between 1 and 5");

            // Auto-calculation validation: RiskScore = Probability * Impact
            RuleFor(x => x)
                .Must(x => x.RiskScore == 0 || x.RiskScore == x.Probability * x.Impact)
                .WithMessage("درجة المخاطرة يجب أن تساوي الاحتمالية × التأثير | Risk score must equal Probability × Impact");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("مالك المخاطرة مطلوب | Risk owner is required")
                .MaximumLength(100).WithMessage("مالك المخاطرة لا يمكن أن يتجاوز 100 حرف | Risk owner cannot exceed 100 characters")
                .EmailAddress().When(x => x.Owner?.Contains("@") == true)
                .WithMessage("البريد الإلكتروني للمالك غير صالح | Owner email is invalid");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("حالة المخاطرة مطلوبة | Risk status is required")
                .Must(BeValidStatus).WithMessage("الحالة يجب أن تكون: محدد، نشط، قيد المراجعة، مقبول، مخفف، أو مغلق | Status must be: Identified, Active, Under Review, Accepted, Mitigated, or Closed");

            RuleFor(x => x.MitigationStrategy)
                .NotEmpty().When(x => x.Status == "Mitigated")
                .WithMessage("استراتيجية التخفيف مطلوبة للمخاطر المخففة | Mitigation strategy required for mitigated risks")
                .MaximumLength(2000).WithMessage("استراتيجية التخفيف لا يمكن أن تتجاوز 2000 حرف | Mitigation strategy cannot exceed 2000 characters");

            RuleFor(x => x.DueDate)
                .GreaterThan(System.DateTime.UtcNow.AddDays(-1))
                .When(x => x.DueDate.HasValue)
                .WithMessage("تاريخ الاستحقاق يجب أن يكون في المستقبل | Due date must be in the future");

            // Control linkage validation: High/Critical risks should have controls
            RuleFor(x => x)
                .Must(x => x.Probability * x.Impact < 15 || !string.IsNullOrEmpty(x.MitigationStrategy))
                .WithMessage("المخاطر العالية/الحرجة يجب أن تحتوي على استراتيجية تخفيف | High/Critical risks must have a mitigation strategy");
        }

        private bool BeValidStatus(string status)
        {
            return RiskStatusTransitions.ValidStatuses.Contains(status);
        }
    }

    /// <summary>
    /// Validator for UpdateRiskDto with state transition validation
    /// </summary>
    public class UpdateRiskDtoValidator : AbstractValidator<UpdateRiskDto>
    {
        public UpdateRiskDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم المخاطرة مطلوب | Risk name is required")
                .MaximumLength(200).WithMessage("اسم المخاطرة لا يمكن أن يتجاوز 200 حرف | Risk name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("وصف المخاطرة مطلوب | Risk description is required")
                .MaximumLength(2000).WithMessage("وصف المخاطرة لا يمكن أن يتجاوز 2000 حرف | Risk description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("فئة المخاطرة مطلوبة | Risk category is required")
                .MaximumLength(100).WithMessage("فئة المخاطرة لا يمكن أن تتجاوز 100 حرف | Risk category cannot exceed 100 characters");

            RuleFor(x => x.Probability)
                .InclusiveBetween(1, 5).WithMessage("الاحتمالية يجب أن تكون بين 1 و 5 | Probability must be between 1 and 5");

            RuleFor(x => x.Impact)
                .InclusiveBetween(1, 5).WithMessage("التأثير يجب أن يكون بين 1 و 5 | Impact must be between 1 and 5");

            // Auto-calculation validation
            RuleFor(x => x)
                .Must(x => x.RiskScore == 0 || x.RiskScore == x.Probability * x.Impact)
                .WithMessage("درجة المخاطرة يجب أن تساوي الاحتمالية × التأثير | Risk score must equal Probability × Impact");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("مالك المخاطرة مطلوب | Risk owner is required")
                .MaximumLength(100).WithMessage("مالك المخاطرة لا يمكن أن يتجاوز 100 حرف | Risk owner cannot exceed 100 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("حالة المخاطرة مطلوبة | Risk status is required")
                .Must(BeValidStatus).WithMessage("الحالة يجب أن تكون صالحة | Status must be valid");

            RuleFor(x => x.MitigationStrategy)
                .NotEmpty().When(x => x.Status == "Mitigated")
                .WithMessage("استراتيجية التخفيف مطلوبة للمخاطر المخففة | Mitigation strategy required for mitigated risks")
                .MaximumLength(2000).WithMessage("استراتيجية التخفيف لا يمكن أن تتجاوز 2000 حرف | Mitigation strategy cannot exceed 2000 characters");

            RuleFor(x => x.DueDate)
                .GreaterThan(System.DateTime.UtcNow.AddDays(-1))
                .When(x => x.DueDate.HasValue)
                .WithMessage("تاريخ الاستحقاق يجب أن يكون في المستقبل | Due date must be in the future");

            // Control linkage validation for high risks
            RuleFor(x => x)
                .Must(x => x.Probability * x.Impact < 15 || !string.IsNullOrEmpty(x.MitigationStrategy))
                .WithMessage("المخاطر العالية/الحرجة يجب أن تحتوي على استراتيجية تخفيف | High/Critical risks must have a mitigation strategy");
        }

        private bool BeValidStatus(string status)
        {
            return RiskStatusTransitions.ValidStatuses.Contains(status);
        }
    }

    /// <summary>
    /// Validator for risk state transitions
    /// </summary>
    public class RiskStateTransitionValidator : AbstractValidator<(string FromStatus, string ToStatus)>
    {
        public RiskStateTransitionValidator()
        {
            RuleFor(x => x)
                .Must(x => RiskStatusTransitions.IsValidTransition(x.FromStatus, x.ToStatus))
                .WithMessage(x => $"انتقال الحالة غير مسموح من {x.FromStatus} إلى {x.ToStatus} | Invalid state transition from {x.FromStatus} to {x.ToStatus}");
        }
    }

    /// <summary>
    /// Validator for risk acceptance
    /// </summary>
    public class RiskAcceptanceValidator : AbstractValidator<RiskDto>
    {
        public RiskAcceptanceValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => s == "Active" || s == "Under Review")
                .WithMessage("يمكن قبول المخاطر النشطة أو قيد المراجعة فقط | Only Active or Under Review risks can be accepted");

            RuleFor(x => x.Owner)
                .NotEmpty()
                .WithMessage("يجب تحديد مالك للمخاطرة قبل القبول | Risk must have an owner before acceptance");

            // Critical risks require escalation before acceptance
            RuleFor(x => x)
                .Must(x => x.Probability * x.Impact < 20)
                .WithMessage("المخاطر الحرجة تتطلب موافقة الإدارة العليا | Critical risks require executive approval");
        }
    }

    /// <summary>
    /// Validator for risk escalation
    /// </summary>
    public class RiskEscalationValidator : AbstractValidator<RiskDto>
    {
        public RiskEscalationValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => s != "Closed" && s != "Mitigated")
                .WithMessage("لا يمكن تصعيد المخاطر المغلقة أو المخففة | Cannot escalate closed or mitigated risks");

            // Only high/critical risks should be escalated
            RuleFor(x => x)
                .Must(x => x.Probability * x.Impact >= 12)
                .WithMessage("يجب تصعيد المخاطر العالية أو الحرجة فقط | Only High or Critical risks should be escalated");
        }
    }

    /// <summary>
    /// Validator for CreateRiskAppetiteSettingDto
    /// </summary>
    public class CreateRiskAppetiteSettingDtoValidator : AbstractValidator<CreateRiskAppetiteSettingDto>
    {
        private static readonly string[] ValidCategories = 
        {
            "Strategic", "Operational", "Financial", "Compliance",
            "Reputational", "Technology", "Legal", "Market", "Credit", "Liquidity"
        };

        public CreateRiskAppetiteSettingDtoValidator()
        {
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("فئة المخاطر مطلوبة | Risk category is required")
                .MaximumLength(100).WithMessage("فئة المخاطر لا يمكن أن تتجاوز 100 حرف | Category cannot exceed 100 characters")
                .Must(c => ValidCategories.Contains(c))
                .WithMessage("فئة المخاطر غير صالحة | Invalid risk category");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم الإعداد مطلوب | Setting name is required")
                .MaximumLength(200).WithMessage("اسم الإعداد لا يمكن أن يتجاوز 200 حرف | Name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("الوصف لا يمكن أن يتجاوز 2000 حرف | Description cannot exceed 2000 characters");

            RuleFor(x => x.MinimumRiskScore)
                .InclusiveBetween(0, 100)
                .WithMessage("الحد الأدنى يجب أن يكون بين 0 و 100 | Minimum score must be between 0 and 100");

            RuleFor(x => x.MaximumRiskScore)
                .InclusiveBetween(0, 100)
                .WithMessage("الحد الأقصى يجب أن يكون بين 0 و 100 | Maximum score must be between 0 and 100");

            RuleFor(x => x.TargetRiskScore)
                .InclusiveBetween(0, 100)
                .WithMessage("الهدف يجب أن يكون بين 0 و 100 | Target score must be between 0 and 100");

            RuleFor(x => x.TolerancePercentage)
                .InclusiveBetween(0, 100)
                .WithMessage("نسبة التسامح يجب أن تكون بين 0 و 100 | Tolerance must be between 0 and 100");

            RuleFor(x => x.ReviewReminderDays)
                .InclusiveBetween(1, 365)
                .WithMessage("أيام التذكير يجب أن تكون بين 1 و 365 | Reminder days must be between 1 and 365");

            // Cross-field validation: min <= target <= max
            RuleFor(x => x)
                .Must(x => x.MinimumRiskScore <= x.MaximumRiskScore)
                .WithMessage("الحد الأدنى لا يمكن أن يتجاوز الحد الأقصى | Minimum cannot exceed maximum");

            RuleFor(x => x)
                .Must(x => x.TargetRiskScore >= x.MinimumRiskScore && x.TargetRiskScore <= x.MaximumRiskScore)
                .WithMessage("الهدف يجب أن يكون بين الحد الأدنى والأقصى | Target must be between minimum and maximum");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(System.DateTime.UtcNow)
                .When(x => x.ExpiryDate.HasValue)
                .WithMessage("تاريخ الانتهاء يجب أن يكون في المستقبل | Expiry date must be in the future");
        }
    }

    /// <summary>
    /// Validator for UpdateRiskAppetiteSettingDto
    /// </summary>
    public class UpdateRiskAppetiteSettingDtoValidator : AbstractValidator<UpdateRiskAppetiteSettingDto>
    {
        private static readonly string[] ValidCategories = 
        {
            "Strategic", "Operational", "Financial", "Compliance",
            "Reputational", "Technology", "Legal", "Market", "Credit", "Liquidity"
        };

        public UpdateRiskAppetiteSettingDtoValidator()
        {
            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("فئة المخاطر مطلوبة | Risk category is required")
                .MaximumLength(100)
                .Must(c => ValidCategories.Contains(c))
                .WithMessage("فئة المخاطر غير صالحة | Invalid risk category");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم الإعداد مطلوب | Setting name is required")
                .MaximumLength(200);

            RuleFor(x => x.MinimumRiskScore)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.MaximumRiskScore)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.TargetRiskScore)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.TolerancePercentage)
                .InclusiveBetween(0, 100);

            RuleFor(x => x.ReviewReminderDays)
                .InclusiveBetween(1, 365);

            RuleFor(x => x)
                .Must(x => x.MinimumRiskScore <= x.MaximumRiskScore)
                .WithMessage("الحد الأدنى لا يمكن أن يتجاوز الحد الأقصى | Minimum cannot exceed maximum");

            RuleFor(x => x)
                .Must(x => x.TargetRiskScore >= x.MinimumRiskScore && x.TargetRiskScore <= x.MaximumRiskScore)
                .WithMessage("الهدف يجب أن يكون بين الحد الأدنى والأقصى | Target must be between minimum and maximum");
        }
    }
}