using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    public class CreateAssessmentDtoValidator : AbstractValidator<CreateAssessmentDto>
    {
        public CreateAssessmentDtoValidator()
        {
            RuleFor(x => x.AssessmentNumber)
                .NotEmpty().WithMessage("Assessment number is required")
                .MaximumLength(50).WithMessage("Assessment number cannot exceed 50 characters");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Assessment type is required")
                .MaximumLength(100).WithMessage("Assessment type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Assessment type must be one of: Risk, Control, Compliance");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Assessment name is required")
                .MaximumLength(200).WithMessage("Assessment name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past");

            RuleFor(x => x.AssignedTo)
                .NotEmpty().WithMessage("Assigned to field is required")
                .MaximumLength(100).WithMessage("Assigned to cannot exceed 100 characters");

            RuleFor(x => x.RiskId)
                .NotEmpty().When(x => x.ControlId == null).WithMessage("Either Risk ID or Control ID must be specified");

            RuleFor(x => x.ControlId)
                .NotEmpty().When(x => x.RiskId == null).WithMessage("Either Risk ID or Control ID must be specified");
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Risk", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Control", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Compliance", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class UpdateAssessmentDtoValidator : AbstractValidator<UpdateAssessmentDto>
    {
        public UpdateAssessmentDtoValidator()
        {
            RuleFor(x => x.AssessmentNumber)
                .NotEmpty().WithMessage("Assessment number is required")
                .MaximumLength(50).WithMessage("Assessment number cannot exceed 50 characters");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Assessment type is required")
                .MaximumLength(100).WithMessage("Assessment type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Assessment type must be one of: Risk, Control, Compliance");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Assessment name is required")
                .MaximumLength(200).WithMessage("Assessment name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.EndDate.HasValue)
                .WithMessage("End date must be after start date");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Planned, InProgress, Completed, Cancelled");

            RuleFor(x => x.AssignedTo)
                .MaximumLength(100).WithMessage("Assigned to cannot exceed 100 characters");

            RuleFor(x => x.ReviewedBy)
                .MaximumLength(100).WithMessage("Reviewed by cannot exceed 100 characters");

            RuleFor(x => x.ComplianceScore)
                .InclusiveBetween(0, 100).When(x => x.ComplianceScore.HasValue)
                .WithMessage("Compliance score must be between 0 and 100");

            RuleFor(x => x.Results)
                .MaximumLength(2000).WithMessage("Results cannot exceed 2000 characters");

            RuleFor(x => x.Findings)
                .MaximumLength(2000).WithMessage("Findings cannot exceed 2000 characters");

            RuleFor(x => x.Recommendations)
                .MaximumLength(2000).WithMessage("Recommendations cannot exceed 2000 characters");
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Risk", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Control", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Compliance", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Planned", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
        }
    }
}
