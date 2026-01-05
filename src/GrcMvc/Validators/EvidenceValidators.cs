using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    public class CreateEvidenceDtoValidator : AbstractValidator<CreateEvidenceDto>
    {
        public CreateEvidenceDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Evidence name is required")
                .MaximumLength(200).WithMessage("Evidence name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.EvidenceType)
                .NotEmpty().WithMessage("Evidence type is required")
                .MaximumLength(100).WithMessage("Evidence type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Evidence type must be one of: Documentation, Interview, Observation, Screenshot, Log, Report");

            RuleFor(x => x.DataClassification)
                .NotEmpty().WithMessage("Data classification is required")
                .Must(BeValidClassification).WithMessage("Data classification must be one of: Public, Internal, Confidential, Restricted");

            RuleFor(x => x.Source)
                .MaximumLength(200).WithMessage("Source cannot exceed 200 characters");

            RuleFor(x => x.CollectionDate)
                .NotEmpty().WithMessage("Collection date is required")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Collection date cannot be in the future");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(x => x.CollectionDate).When(x => x.ExpirationDate.HasValue)
                .WithMessage("Expiration date must be after collection date");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Active, Expired, Archived, Deleted");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("Owner is required")
                .MaximumLength(100).WithMessage("Owner cannot exceed 100 characters");

            RuleFor(x => x.Location)
                .MaximumLength(500).WithMessage("Location cannot exceed 500 characters");

            RuleFor(x => x.Tags)
                .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Documentation", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Interview", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Observation", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Screenshot", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Log", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Report", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidClassification(string classification)
        {
            return classification.Equals("Public", StringComparison.OrdinalIgnoreCase) ||
                   classification.Equals("Internal", StringComparison.OrdinalIgnoreCase) ||
                   classification.Equals("Confidential", StringComparison.OrdinalIgnoreCase) ||
                   classification.Equals("Restricted", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Expired", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Archived", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Deleted", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class UpdateEvidenceDtoValidator : AbstractValidator<UpdateEvidenceDto>
    {
        public UpdateEvidenceDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Evidence name is required")
                .MaximumLength(200).WithMessage("Evidence name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.EvidenceType)
                .MaximumLength(100).WithMessage("Evidence type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Evidence type must be one of: Documentation, Interview, Observation, Screenshot, Log, Report");

            RuleFor(x => x.DataClassification)
                .Must(BeValidClassification).WithMessage("Data classification must be one of: Public, Internal, Confidential, Restricted");

            RuleFor(x => x.Source)
                .MaximumLength(200).WithMessage("Source cannot exceed 200 characters");

            RuleFor(x => x.CollectionDate)
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Collection date cannot be in the future");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(x => x.CollectionDate).When(x => x.ExpirationDate.HasValue)
                .WithMessage("Expiration date must be after collection date");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Active, Expired, Archived, Deleted");

            RuleFor(x => x.Owner)
                .MaximumLength(100).WithMessage("Owner cannot exceed 100 characters");

            RuleFor(x => x.Location)
                .MaximumLength(500).WithMessage("Location cannot exceed 500 characters");

            RuleFor(x => x.Tags)
                .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Documentation", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Interview", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Observation", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Screenshot", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Log", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Report", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidClassification(string classification)
        {
            return classification.Equals("Public", StringComparison.OrdinalIgnoreCase) ||
                   classification.Equals("Internal", StringComparison.OrdinalIgnoreCase) ||
                   classification.Equals("Confidential", StringComparison.OrdinalIgnoreCase) ||
                   classification.Equals("Restricted", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Expired", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Archived", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Deleted", StringComparison.OrdinalIgnoreCase);
        }
    }
}
