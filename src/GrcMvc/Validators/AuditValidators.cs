using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    public class CreateAuditDtoValidator : AbstractValidator<CreateAuditDto>
    {
        public CreateAuditDtoValidator()
        {
            RuleFor(x => x.AuditNumber)
                .NotEmpty().WithMessage("Audit number is required")
                .MaximumLength(50).WithMessage("Audit number cannot exceed 50 characters");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Audit type is required")
                .MaximumLength(100).WithMessage("Audit type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Audit type must be one of: Internal, External, SOX, ISO, NIST");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Audit name is required")
                .MaximumLength(200).WithMessage("Audit name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.AssignedTo)
                .NotEmpty().WithMessage("Assigned to field is required")
                .MaximumLength(100).WithMessage("Assigned to cannot exceed 100 characters");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Planned, InProgress, Completed, Cancelled");

            RuleFor(x => x.Scope)
                .MaximumLength(1000).WithMessage("Scope cannot exceed 1000 characters");

            RuleFor(x => x.Objectives)
                .MaximumLength(2000).WithMessage("Objectives cannot exceed 2000 characters");

            RuleFor(x => x.Criteria)
                .MaximumLength(2000).WithMessage("Criteria cannot exceed 2000 characters");

            RuleFor(x => x.Methodology)
                .MaximumLength(1000).WithMessage("Methodology cannot exceed 1000 characters");

            RuleFor(x => x.ReportSummary)
                .MaximumLength(5000).WithMessage("Report summary cannot exceed 5000 characters");
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Internal", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("External", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("SOX", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("ISO", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("NIST", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Planned", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class UpdateAuditDtoValidator : AbstractValidator<UpdateAuditDto>
    {
        public UpdateAuditDtoValidator()
        {
            RuleFor(x => x.AuditNumber)
                .NotEmpty().WithMessage("Audit number is required")
                .MaximumLength(50).WithMessage("Audit number cannot exceed 50 characters");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Audit type is required")
                .MaximumLength(100).WithMessage("Audit type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Audit type must be one of: Internal, External, SOX, ISO, NIST");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Audit name is required")
                .MaximumLength(200).WithMessage("Audit name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Planned, InProgress, Completed, Cancelled");

            RuleFor(x => x.AssignedTo)
                .MaximumLength(100).WithMessage("Assigned to cannot exceed 100 characters");

            RuleFor(x => x.Scope)
                .MaximumLength(1000).WithMessage("Scope cannot exceed 1000 characters");

            RuleFor(x => x.Objectives)
                .MaximumLength(2000).WithMessage("Objectives cannot exceed 2000 characters");

            RuleFor(x => x.Criteria)
                .MaximumLength(2000).WithMessage("Criteria cannot exceed 2000 characters");

            RuleFor(x => x.Methodology)
                .MaximumLength(1000).WithMessage("Methodology cannot exceed 1000 characters");

            RuleFor(x => x.ReportSummary)
                .MaximumLength(5000).WithMessage("Report summary cannot exceed 5000 characters");
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Internal", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("External", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("SOX", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("ISO", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("NIST", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Planned", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CreateAuditFindingDtoValidator : AbstractValidator<CreateAuditFindingDto>
    {
        public CreateAuditFindingDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Finding title is required")
                .MaximumLength(200).WithMessage("Finding title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Finding description is required")
                .MaximumLength(2000).WithMessage("Finding description cannot exceed 2000 characters");

            RuleFor(x => x.Severity)
                .Must(BeValidSeverity).WithMessage("Severity must be one of: Critical, High, Medium, Low");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Open, InProgress, Closed, Resolved");

            RuleFor(x => x.Recommendation)
                .MaximumLength(2000).WithMessage("Recommendation cannot exceed 2000 characters");

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
                   status.Equals("Closed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Resolved", StringComparison.OrdinalIgnoreCase);
        }
    }
}
