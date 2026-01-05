using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    /// <summary>
    /// Validator for CreateRiskDto
    /// </summary>
    public class CreateRiskDtoValidator : AbstractValidator<CreateRiskDto>
    {
        public CreateRiskDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Risk name is required")
                .MaximumLength(200).WithMessage("Risk name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Risk description is required")
                .MaximumLength(2000).WithMessage("Risk description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Risk category is required")
                .MaximumLength(100).WithMessage("Risk category cannot exceed 100 characters");

            RuleFor(x => x.Probability)
                .InclusiveBetween(1, 5).WithMessage("Probability must be between 1 and 5");

            RuleFor(x => x.Impact)
                .InclusiveBetween(1, 5).WithMessage("Impact must be between 1 and 5");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("Risk owner is required")
                .MaximumLength(100).WithMessage("Risk owner cannot exceed 100 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Risk status is required")
                .Must(BeValidStatus).WithMessage("Status must be Active, Inactive, or Mitigated");

            RuleFor(x => x.MitigationStrategy)
                .MaximumLength(2000).WithMessage("Mitigation strategy cannot exceed 2000 characters");

            RuleFor(x => x.DueDate)
                .GreaterThan(System.DateTime.UtcNow.AddDays(-1))
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date must be in the future");
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Active", "Inactive", "Mitigated", "Under Review", "Closed" };
            return validStatuses.Contains(status);
        }
    }

    /// <summary>
    /// Validator for UpdateRiskDto
    /// </summary>
    public class UpdateRiskDtoValidator : AbstractValidator<UpdateRiskDto>
    {
        public UpdateRiskDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Risk name is required")
                .MaximumLength(200).WithMessage("Risk name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Risk description is required")
                .MaximumLength(2000).WithMessage("Risk description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Risk category is required")
                .MaximumLength(100).WithMessage("Risk category cannot exceed 100 characters");

            RuleFor(x => x.Probability)
                .InclusiveBetween(1, 5).WithMessage("Probability must be between 1 and 5");

            RuleFor(x => x.Impact)
                .InclusiveBetween(1, 5).WithMessage("Impact must be between 1 and 5");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("Risk owner is required")
                .MaximumLength(100).WithMessage("Risk owner cannot exceed 100 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Risk status is required")
                .Must(BeValidStatus).WithMessage("Status must be Active, Inactive, Mitigated, Under Review, or Closed");

            RuleFor(x => x.MitigationStrategy)
                .MaximumLength(2000).WithMessage("Mitigation strategy cannot exceed 2000 characters");

            RuleFor(x => x.DueDate)
                .GreaterThan(System.DateTime.UtcNow.AddDays(-1))
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date must be in the future");
        }

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Active", "Inactive", "Mitigated", "Under Review", "Closed" };
            return validStatuses.Contains(status);
        }
    }
}