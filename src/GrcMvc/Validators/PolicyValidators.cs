using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    public class CreatePolicyDtoValidator : AbstractValidator<CreatePolicyDto>
    {
        public CreatePolicyDtoValidator()
        {
            RuleFor(x => x.PolicyNumber)
                .NotEmpty().WithMessage("Policy number is required")
                .MaximumLength(50).WithMessage("Policy number cannot exceed 50 characters");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Policy title is required")
                .MaximumLength(200).WithMessage("Policy title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
                .Must(BeValidCategory).WithMessage("Category must be one of: Security, Privacy, Compliance, Risk, Operational");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Policy type is required")
                .MaximumLength(100).WithMessage("Policy type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Type must be one of: Mandatory, Advisory, Informational");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Draft, Review, Approved, Published, Archived");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty().WithMessage("Effective date is required")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Effective date cannot be in the past");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(x => x.EffectiveDate).When(x => x.ExpirationDate.HasValue)
                .WithMessage("Expiration date must be after effective date");

            RuleFor(x => x.ReviewDate)
                .GreaterThan(x => x.EffectiveDate).When(x => x.ReviewDate.HasValue)
                .WithMessage("Review date must be after effective date");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("Owner is required")
                .MaximumLength(100).WithMessage("Owner cannot exceed 100 characters");

            RuleFor(x => x.Approver)
                .MaximumLength(100).WithMessage("Approver cannot exceed 100 characters");

            RuleFor(x => x.Scope)
                .MaximumLength(1000).WithMessage("Scope cannot exceed 1000 characters");

            RuleFor(x => x.Requirements)
                .MaximumLength(5000).WithMessage("Requirements cannot exceed 5000 characters");

            RuleFor(x => x.Procedures)
                .MaximumLength(5000).WithMessage("Procedures cannot exceed 5000 characters");

            RuleFor(x => x.References)
                .MaximumLength(2000).WithMessage("References cannot exceed 2000 characters");
        }

        private bool BeValidCategory(string category)
        {
            return category.Equals("Security", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Privacy", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Compliance", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Risk", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Operational", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Mandatory", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Advisory", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Informational", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Review", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Published", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Archived", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class UpdatePolicyDtoValidator : AbstractValidator<UpdatePolicyDto>
    {
        public UpdatePolicyDtoValidator()
        {
            RuleFor(x => x.PolicyNumber)
                .NotEmpty().WithMessage("Policy number is required")
                .MaximumLength(50).WithMessage("Policy number cannot exceed 50 characters");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Policy title is required")
                .MaximumLength(200).WithMessage("Policy title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
                .Must(BeValidCategory).WithMessage("Category must be one of: Security, Privacy, Compliance, Risk, Operational");

            RuleFor(x => x.Type)
                .MaximumLength(100).WithMessage("Policy type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Type must be one of: Mandatory, Advisory, Informational");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Draft, Review, Approved, Published, Archived");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty().WithMessage("Effective date is required");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(x => x.EffectiveDate).When(x => x.ExpirationDate.HasValue)
                .WithMessage("Expiration date must be after effective date");

            RuleFor(x => x.ReviewDate)
                .GreaterThan(x => x.EffectiveDate).When(x => x.ReviewDate.HasValue)
                .WithMessage("Review date must be after effective date");

            RuleFor(x => x.Owner)
                .MaximumLength(100).WithMessage("Owner cannot exceed 100 characters");

            RuleFor(x => x.Approver)
                .MaximumLength(100).WithMessage("Approver cannot exceed 100 characters");

            RuleFor(x => x.Scope)
                .MaximumLength(1000).WithMessage("Scope cannot exceed 1000 characters");

            RuleFor(x => x.Requirements)
                .MaximumLength(5000).WithMessage("Requirements cannot exceed 5000 characters");

            RuleFor(x => x.Procedures)
                .MaximumLength(5000).WithMessage("Procedures cannot exceed 5000 characters");

            RuleFor(x => x.References)
                .MaximumLength(2000).WithMessage("References cannot exceed 2000 characters");
        }

        private bool BeValidCategory(string category)
        {
            return category.Equals("Security", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Privacy", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Compliance", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Risk", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Operational", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Mandatory", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Advisory", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Informational", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Review", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Published", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Archived", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CreatePolicyViolationDtoValidator : AbstractValidator<CreatePolicyViolationDto>
    {
        public CreatePolicyViolationDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Violation title is required")
                .MaximumLength(200).WithMessage("Violation title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Severity)
                .Must(BeValidSeverity).WithMessage("Severity must be one of: Critical, High, Medium, Low");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Open, InProgress, Resolved, Closed");

            RuleFor(x => x.DetectedDate)
                .NotEmpty().WithMessage("Detected date is required")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Detected date cannot be in the future");

            RuleFor(x => x.ResolutionPlan)
                .MaximumLength(2000).WithMessage("Resolution plan cannot exceed 2000 characters");

            RuleFor(x => x.Owner)
                .MaximumLength(100).WithMessage("Owner cannot exceed 100 characters");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue)
                .WithMessage("Due date cannot be in the past");
        }

        private bool BeValidSeverity(string severity)
        {
            return severity.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
                   severity.Equals("High", StringComparison.OrdinalIgnoreCase) ||
                   severity.Equals("Medium", StringComparison.OrdinalIgnoreCase) ||
                   severity.Equals("Low", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Open", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Closed", StringComparison.OrdinalIgnoreCase);
        }
    }
}
